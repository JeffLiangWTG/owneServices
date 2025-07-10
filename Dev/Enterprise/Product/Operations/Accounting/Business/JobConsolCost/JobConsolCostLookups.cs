using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostLookups : AutoJobConsolCostLookups
	{
		public JobConsolCostLookups(AutoJobConsolCost parent)
			: base(parent)
		{
		}

		public override AccInvMsgCollection VATClasses
		{
			get { return new AccInvMsgCollection(Factory, Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty); }
		}

		public override OrgHeaderCollection Creditors
		{
			get { return new CreditorCollection(Parent.Factory); }
		}

		public CodeDescriptionPairList PrepaidCollectList
		{
			get
			{
				return Factory.GetCachedValue("PrepaidCollectList", () => Parent.Consol != null ? Parent.Consol.PrepaidCollectList : new CodeDescriptionPairList());
			}
		}

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var fChargeCodes = new AccChargeCodeCollection(Factory, ChargeCodeCollectionFilter, GlbCompany.CurrentCompany.PK.ToGuid());
				fChargeCodes.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Dept Filter", "Property", ChargeCodeDepartmentFilterList));

				return fChargeCodes;
			}
		}

		protected ZQuery ChargeCodeCollectionFilter
		{
			get
			{
				ZQuery result = new ZQuery(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Margin);
				result.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Disbursement);
				result.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.ManualJobAccrual);
				return result;
			}
		}

		ZString ChargeCodeDepartmentFilterList
		{
			get
			{
				ZString result = "";

				foreach (ApportionSplitCharge charge in Parent.ApportionmentCharges)
				{
					if (charge.Department != null && !result.Contains(charge.Department.GE_Code))
					{
						result += (charge.Department.GE_Code + ", ");
					}
				}
				result += "ALL";
				return result;
			}
		}

		public CodeDescriptionPairList ApportionmentMethodList
		{
			get
			{
				var methods = new CodeDescriptionPairList(OLookUpEditType.AllocationMethod);

				var excludedMethods = Parent.Consol?.CostSupporter?.ExcludedApportionmentMethods;
				if (excludedMethods != null)
				{
					excludedMethods.ForEach(method => methods.RemoveCode(method));
				}

				return methods;
			}
		}

		public CodeDescriptionPairList PaymentMethodList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentMethod); }
		}

		#region PlacesOfSupply

		public ReadOnlyCodeDescriptionPairList PlacesOfSupply => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Parent.Company ?? GlbCompany.CurrentCompany);

		public ReadOnlyCodeDescriptionPairList PlaceOfSupplyTypes => PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(Parent.Company ?? GlbCompany.CurrentCompany);

		#endregion

		#region Rating Behaviours

		public CodeDescriptionPairList RatingBehaviourList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.IsPosted)
				{
					result.AddPair(RatingBehaviours.CreateNewCharge, RatingBehaviours.Descriptions.CreateNewCharge);
					result.AddPair(RatingBehaviours.StopFromAutorating, RatingBehaviours.Descriptions.StopFromAutorating);
					return result;
				}

				if (RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.Value && RatingBehaviours.IsSpotBehaviour(Parent.E6_RatingBehaviour))
				{
					result.AddRange(RatingBehaviours.GetSpotRatingBehavioursList());
				}

				result.AddPair(RatingBehaviours.CreateNewCharge, RatingBehaviours.Descriptions.CreateNewCharge);
				result.AddPair(RatingBehaviours.ReAutorateCharge, RatingBehaviours.Descriptions.ReAutorateCharge);

				if (!RatingBehaviours.IsSpotBehaviour(Parent.E6_RatingBehaviour))
				{
					result.AddPair(RatingBehaviours.StopFromAutorating, RatingBehaviours.Descriptions.StopFromAutorating);
				}

				return result;
			}
		}

		#endregion

		public APInvoiceConsolCollection Consols
		{
			get
			{
				return new APInvoiceConsolCollection(Factory);
			}
		}

		public override AccBankAccountCollection BankAccounts
		{
			get
			{
				return new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
			}
		}

		public override AccChequeBookCollection ChequeBooks
		{
			get
			{
				ActiveChequeBookCollection cheques = new ActiveChequeBookCollection(Factory, Parent.BankAccount);
				cheques.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("73f903d3-95b4-4070-9980-26dc8ea946fe", "This cheque book cannot be chosen because it belongs to another bank account or/and is inactive. Please choose another cheque book"));
				return cheques;
			}
		}

		public AccWithholdingCollection WithholdingTaxes
		{
			get
			{
				return new AccWithholdingCollection(Factory, GlbCompany.CurrentCompany);
			}
		}

		new JobConsolCost Parent
		{
			get { return (JobConsolCost)base.Parent; }
		}

		#region TaxRates

		public override AccTaxRateCollection TaxRates
		{
			get
			{
				ZQuery taxRatesFilter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
				return new VATAccTaxRateCollection(Factory, taxRatesFilter);
			}
		}

		#endregion

		public CodeDescriptionPairList SupplyTypes
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.GetActiveCodeDescriptionPairList();
			}
		}

		public override GlbBranchCollection CostTaxBranches
		{
			get { return FindboxLookupCollections.GetActiveBranchForCompanyCollection(Factory); }
		}
	}
}
