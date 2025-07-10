using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class WIP : BaseWIPAccrual
	{
		public WIP(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal protected override void UpdateJobChargeLink(Charge charge)
		{
			charge.ClearRevenueLink();
			charge.ClearRevenueAmount();
		}

		public override ZDecimal ForeignAmountValue
		{
			get
			{
				if (RelatedJobCharge != null)
				{
					return RelatedJobCharge.JR_OSSellAmt;
		}
				else
		{
					return 0m;
		}
			}
		}

		protected override bool ShouldReverseWhenChargeIsPartOfApportionedCost(bool processApportionedCharge)
		{
			return true;
		}

		public override ZString RelatedCurrencyCodeValue
		{
			get
			{
				if (RelatedJobCharge != null)
				{
					return RelatedJobCharge.JR_RX_NKSellCurrency;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		protected override SchemaColumn JobChargeRelatedLineFilterField
		{
			get { return JobChargeSchema.JR_AL_ARLine; }
		}

		protected override OrgHeaderCollection GetOrganisationsCollection()
		{
			return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + "WIP", () =>
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);
				var collection = new OrgHeaderCollection(Factory, query);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));

				return collection;
			});
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AL_Desc = "WIP";
		}

		public override ZDecimal AL_LineAmount
		{
			get { return base.AL_LineAmount; }
			set
			{
				base.AL_LineAmount = value;

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount, () =>
				{
					if (RelatedJobCharge != null)
					{
						var chargeAmount = RelatedJobCharge.JR_LocalSellInvoiceAmt;
						var chargeCFX = RelatedJobCharge.JR_CFXAmt;
						var chargeTotal = chargeAmount - chargeCFX;
						var lineAmount = -AL_LineAmount;

						if (chargeTotal != lineAmount)
						{
							var developerInfo = new ZStringBuilder();
							developerInfo.AppendLine(Invariant($"{chargeAmount} - {chargeCFX} = {chargeTotal} != {lineAmount}"));
							developerInfo.AppendLine(Invariant($"Stacktrace --> {new StackTrace(true)}"));
							return developerInfo.ToString();
						}
					}
					else
					{
						return Invariant($"Related charge is not linked yet. AL_LineAmount: {AL_LineAmount}");
					}

					return null;
				});
			}
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionLineTypes.WIP; }
		}

		public override void SetValues(Job job, BaseCharge charge)
		{
			base.SetValues(job, charge);
			AL_Sequence = charge.WIP != null ? charge.WIP.AL_Sequence + (ZShort)1 : (ZShort)1;
			AL_OSExTaxAmount = charge.JR_LocalSellInvoiceAmt - charge.JR_CFXAmt;
			CollectLocalSellInvoiceAmtForCriticalValidation(charge);

			if (charge.SellAccount != null && charge.Company != null && charge.SellAccount.IsDebtorForCompany(charge.Company))
			{
				AL_OH = charge.JR_OH_SellAccount;
			}
			else
			{
				#region SuppressResourceStringsCheckRegion
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet,
					() =>
					{
						var stringBuilder = new ZStringBuilder();
						stringBuilder.AppendLine("Sell Account PK: " + charge.JR_OH_SellAccount.ToString());
						stringBuilder.AppendLine("Charge Company PK: " + charge.JR_GC.ToString());
						stringBuilder.AppendLine("WIP branch Company PK: " + Branch?.GB_GC.ToString());

						if (charge.SellAccount != null && charge.Company != null)
						{
							stringBuilder.AppendLine("Is Debtor from charge.SellAccount: " + charge.SellAccount.IsDebtorForCompany(charge.Company).ToYesNoString());
						}

						var companyData = OrgCompanyData.Load(Factory, charge.JR_OH_SellAccount, charge.JR_GC);
						stringBuilder.AppendLine("Is OrgCompanyData found: " + (companyData != null).ToYesNoString());
						if (companyData != null)
						{
							stringBuilder.AppendLine("Is Debtor from dbo.OrgCompanyData: " + (companyData.OB_IsDebtor).ToYesNoString());
						}
						stringBuilder.AppendLine("Is Validation Suspended on Charge: " + charge.IsValidationSuspended.ToYesNoString());

						return stringBuilder.ToString();
					},
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				#endregion
			}
		}

		void CollectLocalSellInvoiceAmtForCriticalValidation(BaseCharge charge)
		{
			#region SuppressResourceStringsCheckRegion
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(charge.PK, CriticalValidationInfoCollectorServiceKeyType.WIPLocalSellInvoiceAmtInfo,
				() => FormattableString.Invariant($@"
Values When Creating WIP:
  JR_LocalSellInvoiceAmt: {charge.JR_LocalSellInvoiceAmt}
  Company.GC_RX_NKLocalCurrency: {Company.GC_RX_NKLocalCurrency}
  Company.LocalCurrency.Decimals: {Company.LocalCurrency.Decimals}
  BillInInvoiceCurrencyWithLocalSellCurrency: {charge.BillInInvoiceCurrencyWithLocalSellCurrency}
  BillInInvoiceCurrency: {charge.BillInInvoiceCurrency}
  IsSellLocal: {charge.IsSellLocal}
  SellInvoiceExchangeRate:
{charge.SellInvoiceExchangeRate.PropertiesToString(indentCharDepth: 4)}
")
				,
				CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession
			);
			#endregion
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				var jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_AL_ARLine = this.PK;

				AL_JH = RelatedJobCharge.JR_JH;
			}
		}

#endif
	}
}
