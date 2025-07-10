using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
		, IAddInfo
		, IAddInfoManager
		, Integration.Customs.AU.IJobComInvoiceGroupHeader
		, IUniversalRateCalcData
	{
		#region Constants
		public new class Schema : BaseJobComInvoiceHeader.Schema
		{
			public const string JZ_CommissionType = "JZ_CommissionType";
		}
		#endregion

		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		void IAggregatedAddInfo.MarkAsNeedingValidation()
		{
			MarkAsNeedingValidationForMajorDataChange();
		}

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new InvoiceHeaderActiveCollection AllJobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.AllJobComInvoiceHeaders;

		protected override Customs.Business.InvoiceHeaderActiveCollection GetNewAllInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, false);

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public void CopyPersistentValuesFrom(JobComInvoiceGroupHeader sourceObject)
		{
			base.CopyPersistentValuesFrom(sourceObject);
		}

		public CodeDescriptionPairList JZ_CommissionType_List => new CodeDescriptionPairListCustomsCommissionType();

		[ChildEditable(true)]
		public new JobComInvChargeCollection<GroupInvoiceCharge> Charges
		{
			get { return base.Charges as JobComInvChargeCollection<GroupInvoiceCharge>; }
		}

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new JobComInvChargeCollection<GroupInvoiceCharge>(this);

		#region New Properties

		public ZString JZ_CommissionType
		{
			get { return AddInfo.ZA_CommissionType_Hidden; }
			set { AddInfo.ZA_CommissionType_Hidden = value; }
		}
		public ZPropertyInfo JZ_CommissionTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_CommissionType, x => AddInfo.ZA_CommissionType_HiddenInfo); }
		}

		public bool HasOtherCharge
		{
			get { return Charges.GetCharge(AUChargeCodeList.Codes.OtherCharges, JobDeclaration.GetLocalCurrency()) != 0m; }
		}

		#endregion

		#region IAddInfo Members

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get
			{
				if (AddInfo.ZA_ORG != "" || AddInfo.ZA_PRF != "")
				{
					return AddInfo.ZA_ORG;
				}
				else
				{
					if (Master != null)
					{
						return ((IAddInfo)Master).AggregatedZA_ORG;
					}
				}
				return "";
			}
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get
			{
				if (AddInfo.ZA_ORG != "" || AddInfo.ZA_PRF != "")
				{
					return AddInfo.ZA_PRF;
				}
				else
				{
					if (Master != null)
					{
						return ((IAddInfo)Master).AggregatedZA_PRF;
					}
				}
				return "";
			}
		}

		public IZType AggregatedValue(string propertyName)
		{
			IZType result = (IZType)AddInfo[propertyName];
			if (result.IsEmpty)
			{
				if (Master != null)
				{
					result = ((IAddInfo)Master).AggregatedValue(propertyName);
				}
			}
			return result;
		}

		public ZDateTime DateOfValuation
		{
			get
			{
				return JobComInvoiceHeaders.ValuationDate;
			}
		}

		public ZDateTime EffectiveDutyDate
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AUAddInfo(this, JZ_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get
			{
				return IsCopying;
			}
		}

		#endregion

		#region overridden properties

		public override ZString JZ_AddInfo
		{
			get { return base.JZ_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (JZ_AddInfo != value)
				{
					base.JZ_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(value);
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected AUAddInfo fAddInfo;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			AddInfo.OnSaved(saveSucceeded);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return JobValidation;
		}

		protected internal JobComInvoiceGroupHeaderValidation JobValidation
		{
			get
			{
				if (JobDeclaration != null)
				{
					if (JobDeclaration.IsExport)
					{
						return new EXDJobComInvoiceGroupHeaderValidation(this);
					}
					else if (JobDeclaration.IsImport)
					{
						return new ImportJobComInvoiceGroupHeaderValidation(this);
					}
				}
				return new JobComInvoiceGroupHeaderValidation(this);
			}
		}

		#endregion
		#region Calculated Fields For Document Wrappers

		public ZDecimal CalcBuyingCommission
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(AUChargeCodeList.Codes.BuyingCommission)); }
		}

		public ZGuid CalcBuyingCommissionCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(AUChargeCodeList.Codes.BuyingCommission)); }
		}

		public ZDecimal CalcOtherCommission
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(AUChargeCodeList.Codes.OtherCommission)); }
		}

		public ZGuid CalcOtherCommissionCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(AUChargeCodeList.Codes.OtherCommission)); }
		}
		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IUniversalRateCalcData

		DateTime IUniversalRateCalcData.DateOfValuation => DateOfValuation.ToDateTime();

		decimal IUniversalRateCalcData.ValueForDuty
		{
			get
			{
				var valueForDuty = 0m;
				var insurance = GetApplicableInsurance();
				if (insurance != null)
				{
					var targetIncoTerm = insurance.CCR_BasedOn;
					var targetCurrency = insurance.Currency;
					foreach (var invoice in JobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
					{
						valueForDuty += GetValueForDutyForInsurance(invoice, targetIncoTerm, targetCurrency);
					}
				}
				return valueForDuty;
			}
		}

		decimal GetValueForDutyForInsurance(JobComInvoiceHeader invoice, string insuranceIncoTerm, RefCurrency insuranceCurrency)
		{
			var invoiceAmountInInvoiceCurrency = 0m;
			switch (insuranceIncoTerm)
			{
				case IncoTerms.CostAndFreight:
					invoiceAmountInInvoiceCurrency = invoice.EffectiveFOBAmount + invoice.JZ_Calc_OFTInInvoiceCurrency;
					break;
				case IncoTerms.FreeOnBoard:
					invoiceAmountInInvoiceCurrency = invoice.EffectiveFOBAmount;
					break;
			}

			return CurrencyConverter.ConvertExact(new Money(invoiceAmountInInvoiceCurrency, invoice.Invoice_Currency), insuranceCurrency).Amount;
		}

		decimal IUniversalRateCalcData.CustomsValue => 0m;

		public IDictionary<string, decimal> UnitOfMeasureValueList => new Dictionary<string, decimal>();

		public IDictionary<string, decimal> CountrySpecificValueList => new Dictionary<string, decimal>();

		public IDictionary<string, string> MeursingExpressionList => new Dictionary<string, string>();

		public IList<Tuple<string, string>> AdditionalInformationList => new List<Tuple<string, string>>();

		#endregion

		public CusCalculationRule GetApplicableInsurance()
		{
			if (JobDeclaration is { } declaration && declaration.IsImport)
			{
				var valuationDate = DateOfValuation.ToOffset();
				return Factory.GetCachedValue($"GetApplicableInsurance_{declaration.JE_OH_Importer}_{declaration.JE_TransportMode}_{valuationDate}",
					() => new CusCalculationRule.Loader(Factory).LoadApplicableInsuranceRule(declaration.JE_OH_Importer, declaration.JE_TransportMode, valuationDate)
				);
			}

			return null;
		}
	}
}
