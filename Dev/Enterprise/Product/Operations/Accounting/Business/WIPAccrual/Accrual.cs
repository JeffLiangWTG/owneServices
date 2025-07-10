using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class Accrual : BaseWIPAccrual
	{
		public Accrual(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZDecimal ForeignAmountValue
		{
			get
			{
				if (RelatedJobCharge != null)
				{
					return RelatedJobCharge.JR_OSCostAmt;
				}
				else
				{
					return 0m;
				}
			}
		}

		public override ZString RelatedCurrencyCodeValue
		{
			get
			{
				if (RelatedJobCharge != null)
				{
					return RelatedJobCharge.JR_RX_NKCostCurrency;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#region Properties

		public ZBool ShouldReverese
		{
			get { return fShouldReverese; }
			set
			{
				bool hasChanged = SetNonPersistentPropertyValue(ShouldRevereseInfo, ref fShouldReverese, value);
				if (hasChanged && RecalculateAmountsEventSupporter != null)
				{
					RecalculateAmountsEventSupporter.RaiseRecalculateAmounts();
				}
			}
		}
		ZBool fShouldReverese = true;

		public ZPropertyInfo ShouldRevereseInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldReverese)); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AL_Desc = (NoResString)"Accrual";
		}

		internal protected override void UpdateJobChargeLink(Charge charge)
		{
			charge.ClearCostLink();
			charge.ClearCostAmount();
			ZGuid old_JR_E6 = charge.JR_E6;
			try
			{
				charge.JR_E6 = ZGuid.Empty;
				if (old_JR_E6.IsValid)
				{
					DeleteRelatedConsolCost(old_JR_E6);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				charge.JR_E6 = old_JR_E6;
				throw;
			}
		}

		void DeleteRelatedConsolCost(ZGuid costPK)
		{
			JobConsolCost cost = Factory.Load<JobConsolCost>(costPK);
			if (cost != null && !cost.IsPosted)
			{
				cost.Delete();
			}
		}

		protected override SchemaColumn JobChargeRelatedLineFilterField
		{
			get { return JobChargeSchema.JR_AL_APLine; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key, Default Values for FilterBusinessObject")]
		protected override OrgHeaderCollection GetOrganisationsCollection()
		{
			return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + "Accrual", () =>
			{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					query.AddSubQuery(subQuery, JoinCondition.And);
					var collection = new OrgHeaderCollection(Factory, query);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.True));

					return collection;
				});
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionLineTypes.Accrual; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		[DecimalPlaces(0)]
		public override ZDecimal DisplayMultiplier
		{
			get { return -1; }
		}

		public override void SetValues(Job job, BaseCharge charge)
		{
			base.SetValues(job, charge);
			AL_Sequence = charge.Accrual != null ? charge.Accrual.AL_Sequence + (ZShort)1 : (ZShort)1;
			AL_LineAmount = charge.JR_LocalCostAmt;
			AL_OSExTaxAmount = charge.JR_LocalCostAmt;

			if (charge.CostAccount != null && charge.Company != null && charge.CostAccount.IsCreditorForCompany(charge.Company))
			{
				AL_OH = charge.JR_OH_CostAccount;
			}
			else
			{
				#region SuppressResourceStringsCheckRegion
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet,
					() =>
					{
						var stringBuilder = new ZStringBuilder();
						stringBuilder.AppendLine("Cost Account PK: " + charge.JR_OH_CostAccount.ToString());
						stringBuilder.AppendLine("Charge Company PK: " + charge.JR_GC.ToString());
						stringBuilder.AppendLine("ACR branch Company PK: " + Branch?.GB_GC.ToString());

						if (charge.CostAccount != null && charge.Company != null)
						{
							stringBuilder.AppendLine("Is Creditor from charge.CostAccount: " + charge.CostAccount.IsCreditorForCompany(charge.Company).ToYesNoString());
						}

						var companyData = OrgCompanyData.Load(Factory, charge.JR_OH_CostAccount, charge.JR_GC);
						stringBuilder.AppendLine("Is OrgCompanyData found: " + (companyData != null).ToYesNoString());
						if (companyData != null)
						{
							stringBuilder.AppendLine("Is Creditor from dbo.OrgCompanyData: " + (companyData.OB_IsCreditor).ToYesNoString());
						}
						stringBuilder.AppendLine("Is Validation Suspended on Charge: " + charge.IsValidationSuspended.ToYesNoString());

						return stringBuilder.ToString();
					},
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				#endregion
			}
		}

		IRecalculateAmountsEventSupporter RecalculateAmountsEventSupporter
		{
			get
			{
				IRecalculateAmountsEventSupporter recalculateTotalsEventSupporter = null;
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					recalculateTotalsEventSupporter = collection as IRecalculateAmountsEventSupporter;
					if (recalculateTotalsEventSupporter != null)
					{
						break;
					}
				}
				return recalculateTotalsEventSupporter;
			}
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				var jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_AL_APLine = this.PK;

				AL_JH = RelatedJobCharge.JR_JH;
			}
		}

		public ZString JH_JobNum => Job?.JH_JobNum ?? ZString.Empty;

#endif
	}
}
