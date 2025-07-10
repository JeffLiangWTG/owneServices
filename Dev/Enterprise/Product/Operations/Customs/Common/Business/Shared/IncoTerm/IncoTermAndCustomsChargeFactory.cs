using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.ITOTIncoTerm;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public abstract partial class IncoTermAndCustomsChargeFactory
	{
		public static IncoTermAndCustomsChargeFactory GetByCountryCode(string countryCode)
		{
			IncoTermAndCustomsChargeFactory result = null;
			countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			if (!string.IsNullOrEmpty(countryCode))
			{
				if (!Dictionary.TryGetValue(countryCode, out result))
				{
					var types = ObjectFactory.Get<Hashtable>("IncoTermAndCustomsChargeFactories");
					var objectHandle = (ObjectHandle)types[countryCode];
					result = (IncoTermAndCustomsChargeFactory)objectHandle?.GetObject();

					if (result == null && ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode))
					{
						objectHandle = (ObjectHandle)types[EuropeanCustomsUnion];
						result = (IncoTermAndCustomsChargeFactory)objectHandle?.GetObject();
					}
					if (result == null)
					{
						if (!Dictionary.TryGetValue(Common, out result))
						{
							result = new CommonIncoTermAndCustomsChargeFactory();
							Dictionary.Add(Common, result);
						}
					}
					Dictionary.Add(countryCode, result);
				}
			}
			return result;
		}
		const string EuropeanCustomsUnion = "EUN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		const string Common = "Common";

		static Dictionary<string, IncoTermAndCustomsChargeFactory> Dictionary => dictionary ?? (dictionary = new Dictionary<string, IncoTermAndCustomsChargeFactory>());

		[ThreadStatic]
		static Dictionary<string, IncoTermAndCustomsChargeFactory> dictionary;

		protected IncoTermAndCustomsChargeFactory()
		{
		}

		public ZString[] GetAllIncoTerms() => allIncoTerms ?? (allIncoTerms = IncotermChargeRelationshipConfigurations.Keys.ToArray());
		ZString[] allIncoTerms;

		public IEnumerable<ICustomsChargeCode> GetChargeList(ChargeParentTypes parentType)
		{
			var allCharges = GetAllCharges();
			foreach (var charge in allCharges)
			{
				if ((charge.ParentTypes & parentType) == parentType || (charge.ParentTypes & parentType) == charge.ParentTypes)
				{
					yield return charge;
				}
			}
		}

		public virtual bool MakeFlagsReadOnlyWhenDeemed => false;

		public bool IsIncludedInInvoiceAmountFixed(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && chargeConfiguration.IsIncludedInInvoiceAmountFixed;
		}

		public virtual bool GetDefaultIsIncludedInInvoice(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && chargeConfiguration.IsIncludedInInvoice;
		}

		public bool CanThisIncoTermHaveThisCharge(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && chargeConfiguration.IsIncludedInInvoice;
		}

		/// <summary>
		/// Used to add a validation for an incoterm that has missing mandatory charges
		/// </summary>
		public ICustomsChargeCode[] MissingMandatoryCharges(ZString incoTerm, ICommonInvoice invoice)
		{
			var result = new List<ICustomsChargeCode>();
			if (invoice != null)
			{
				var allCharges = GetAllCharges();
				foreach (ICustomsChargeCode charge in allCharges)
				{
					if (IsThisChargeMandatory(incoTerm, charge.Code) && !invoice.HasChargeWithThisKey(charge.ChargeCodeChargeKey))
					{
						result.Add(charge);
					}
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// In terms of Customs messaging system
		/// </summary>
		public bool IsThisChargeMandatory(ZString incoTerm, ZString chargeCode)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, chargeCode);
			return chargeConfiguration != null && chargeConfiguration.IsMandatory;
		}

		/// <summary>
		/// Used to populate rows of charges depending on an incoterm
		/// </summary>
		/// <param name="invoice"></param>
		/// <returns></returns>
		public ICustomsChargeCode[] MissingRecommendedCharges(ZString incoTerm, ICommonInvoice invoice)
		{
			var result = new List<ICustomsChargeCode>();
			if (invoice != null)
			{
				var allCharges = GetAllCharges();
				foreach (var charge in allCharges)
				{
					if (IsThisChargeRecommendedForThisIncoTerm(incoTerm, charge.Code) && IsThisChargeRecommendedForThisInvoice(invoice, incoTerm, charge.Code) && !invoice.HasChargeWithThisKey(charge.ChargeCodeChargeKey))
					{
						result.Add(charge);
					}
				}
			}
			return result.ToArray();
		}

		public bool IsIncludedInITOTReadOnlyForGroupCharge(ZString chargeCode)
		{
			return !chargeCode.IsEmpty && IsIncludedInITOTReadOnlyForGroupChargeCore(chargeCode);
		}

		protected virtual bool IsIncludedInITOTReadOnlyForGroupChargeCore(ZString chargeCode)
		{
			var charge = GetCharge(chargeCode);
			return charge != null && !charge.IsIncoTermNeutral;
		}

		public bool IsThisChargeRecommendedForThisIncoTerm(ZString incoTerm, ZString chargeCode)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, chargeCode);
			return chargeConfiguration != null && chargeConfiguration.IsRecommended;
		}

		public virtual bool IsThisChargeRecommendedForThisInvoice(ICommonInvoice invoice, ZString incoterm, ZString chargeCode)
		{
			return true;
		}

		public virtual bool IsThisChargeDiscount(ZString chargeCode)
		{
			return chargeCode == CustomsChargeTypeList.Codes.Discount;
		}

		public virtual bool CanThisChargeBeIncludedOnLineButNotOnInvoice(ZString chargeCode)
		{
			return false;
		}

		public const string ErrorIncoTermCode = "XXX";

		protected void AddChargeConfiguration(ZString incoTerm, ICustomsChargeCode charge, ChargeConfiguration chargeConfiguration, bool ignoreExisting = false)
		{
			AddChargeConfiguration(incoTerm, charge.Code, chargeConfiguration, ignoreExisting);
		}
		protected void AddChargeConfiguration(ZString incoTerm, string chargeCode, ChargeConfiguration chargeConfiguration, bool ignoreExisting = false)
		{
			var chargeConfigurationDictionary = GetChargeConfigurationDictionary(incoTerm);
			if (chargeConfigurationDictionary.ContainsKey(chargeCode))
			{
				if (!ignoreExisting)
				{
					ErrorReporter.ReportOnce(string.Format(Culture.Invariant, "IncoTerm '{0}' already has ChargeCode '{1}' configured", incoTerm, chargeCode));
				}
				chargeConfigurationDictionary[chargeCode] = chargeConfiguration;
			}
			else
			{
				chargeConfigurationDictionary.Add(chargeCode, chargeConfiguration);
			}
		}

		SortedDictionary<ZString, ChargeConfiguration> GetChargeConfigurationDictionary(ZString incoTerm)
		{
			SortedDictionary<ZString, ChargeConfiguration> chargeConfigurationDictionary;
			if (!IncotermChargeRelationshipConfigurations.TryGetValue(incoTerm, out chargeConfigurationDictionary))
			{
				chargeConfigurationDictionary = new SortedDictionary<ZString, ChargeConfiguration>();
				IncotermChargeRelationshipConfigurations.Add(incoTerm, chargeConfigurationDictionary);
			}

			return chargeConfigurationDictionary;
		}

		public ChargeConfiguration GetConfiguration(ZString incoTerm, ZString chargeCode)
		{
			ChargeConfiguration result = null;
			if (!chargeCode.IsEmpty)
			{
				SortedDictionary<ZString, ChargeConfiguration> value;
				if (IncotermChargeRelationshipConfigurations.TryGetValue(incoTerm, out value))
				{
					value.TryGetValue(chargeCode, out result);
				}

				if (result == null)
				{
					result = GetConfigurationWhenChargeConfiguration_Is_Not_Found(chargeCode);
				}
			}
			return result;
		}

		protected virtual ChargeConfiguration GetConfigurationWhenChargeConfiguration_Is_Not_Found(ZString chargeCode)
		{
			ChargeConfiguration result = null;
			if (chargeCode.Equals(CustomsChargeTypeList.Codes.AdditionCharge))
			{
				result = new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false };
			}
			if (chargeCode.Equals(CustomsChargeTypeList.Codes.DeductionCharge))
			{
				result = new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false };
			}
			return result;
		}

		SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>> IncotermChargeRelationshipConfigurations
		{
			get
			{
				if (configurations == null)
				{
					configurations = new SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>>();
					SetupIncotermChargeConfigurations();
					SetupErrorConfiguration();
				}
				return configurations;
			}
		}
		SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>> configurations;

#if DEBUG
		public object GetIncotermChargeRelationshipConfigurations()
		{
			return IncotermChargeRelationshipConfigurations;
		}
#endif

		protected abstract void SetupIncotermChargeConfigurations();
		protected abstract void SetupErrorConfiguration();

		public ZString ITOTIncoTerm(ZString incoTerm, ICommonInvoice invoice)
		{
			var result = incoTerm;
			if (invoice != null)
			{
				IITOTIncoTermCalculator calculator;
				if (ITOTIncotermConfigurations.TryGetValue(incoTerm, out calculator))
				{
					result = calculator.Calculate(invoice);
				}
			}
			return result;
		}

		SortedDictionary<ZString, IITOTIncoTermCalculator> ITOTIncotermConfigurations
		{
			get
			{
				if (iTOTconfigurations == null)
				{
					iTOTconfigurations = new SortedDictionary<ZString, IITOTIncoTermCalculator>(GetIITOTIncoTermCalculators().ToDictionary(x => x.IncoTerm));
				}
				return iTOTconfigurations;
			}
		}
		SortedDictionary<ZString, IITOTIncoTermCalculator> iTOTconfigurations;

		protected abstract IEnumerable<IITOTIncoTermCalculator> GetIITOTIncoTermCalculators();
	}
}

