using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ShipmentPrePaidCharges : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ShipmentPrePaidCharges({jobshipmentpk})>",
				ResString.GetMultilingualString("57af1ede-901f-418b-9e0d-89d760c1e7bc", @"Returns a list of Prepaid Charges for the Shipment ({0}) indicated by the PK.",
				"JobShipment"),
				new List<(string example, object expectedResult)> { ("<ShipmentPrePaidCharges(<JS_PK>)>", "CRT, ") });
		}

		#region GetReplacement

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (++fNumberOfRunsWithCurrentFactory > MaxNumberOfRunsWithSameFactory)
			{
				ResetFactoryAndCounter();
			}
			object result = "";
			try
			{
				var parameter = CachedRegex.Value.Match(macro).Groups[1].Value;
				ZGuid pK = new ZGuid(parameter);
				if (!pK.IsEmpty)
				{
					BusinessObject ship = (BusinessObject)Factory.Load<Enterprise.Integration.Freight.ICommonShipment>(pK);
					ZString incoTermFromShip = (ZString)ship[JobShipmentSchema.JS_INCO.Name];
					if (IncoTermRegistry.TryGetValue(incoTermFromShip, out var inco) && inco != null)
					{
						var accChargeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
						var jobChargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AC);
						var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
						jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentID, pK);
						jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
						jobChargeQuery.AddSubQuery(JobChargeSchema.JR_JH, jobHeaderQuery, JoinCondition.And);
						accChargeQuery.AddSubQuery(jobChargeQuery, JoinCondition.And);
						accChargeQuery.OrderBy = AccChargeCodeSchema.AC_Code.Name;

						var accCharges = Factory.Load<AccChargeCode>(accChargeQuery);
						foreach (var accCharge in accCharges)
						{
							var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(accCharge.AC_ChargeGroup, inco.IncoTermCode);
							if (prepaidCollect == Core.Constants.PaymentType.Prepaid)
							{
								result = result + accCharge.AC_Code + ", ";
							}
						}
					}
				}
			}
			catch (NullReferenceException)
			{
				result = 0;
			}
			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		void ResetFactoryAndCounter()
		{
			factory = new BusinessObjectFactory();
			fNumberOfRunsWithCurrentFactory = 0;
		}

		int fNumberOfRunsWithCurrentFactory;
		const int MaxNumberOfRunsWithSameFactory = 1000;

		#endregion

		public override Regex Regex => CachedRegex.Value;

		static readonly Lazy<Regex> CachedRegex = new(() =>
		{
			var s = (NoResString)@"(?:[\s]*)";
			var macros = @"(?:<.*>)";
			var guid = (NoResString)@"(?:(?:\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(?:\}){0,1})";
			var oneOf = new Func<string, string, string>((r1, r2) => string.Format("({0}|{1})", r1, r2));

			var regexString = string.Concat("^<", s, "ShipmentPrePaidCharges", s, @"\(", s, oneOf(macros, guid), s, @"\)", s, ">$");

			return new Regex(regexString, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		});
	}
}
