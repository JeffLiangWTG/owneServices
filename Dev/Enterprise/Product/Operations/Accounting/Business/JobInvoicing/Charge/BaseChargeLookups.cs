using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class BaseChargeLookups : JobChargeLookups
	{
		public BaseChargeLookups(AutoJobCharge parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RelatedJobNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();

				foreach (var shipment in Charge.RelatedShipments())
				{
					var invSupporter = shipment.InvoicingSupporter;
					result.AddPair(shipment.JobNumber, Invariant($"{invSupporter.Origin?.Code} - {invSupporter.Destination?.Code}"));
				}

				return result;
			}
		}

		public CodeDescriptionPairList InvoiceTargetJobNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();

				foreach (var target in Charge.GetInvoiceTargetsIfEnabled())
				{
					var invSupporter = target.InvoicingSupporter;
					result.AddPair(target.JobNumber, Invariant($"{invSupporter.Origin?.Code} - {invSupporter.Destination?.Code}"));
				}

				return result;
			}
		}

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var chargeCodeDepartment = Charge.Department?.GE_Code ?? ZString.Empty;
				var chargeGroup = Charge.InvoicingJob?.PlugInData?.InvoicingSupporter?.DefaultChargeGroup ?? ZString.Empty;
				return Factory.GetCachedValue("BaseChargeLookups.ChargeCodes" + string.Format(".{0}.{1}.{2}", chargeCodeDepartment, chargeGroup, GlbCompany.CurrentCompany.PK.ToStringKey()),
					() =>
					{
						var result = NewChargeCodeCollection;
						if (!chargeCodeDepartment.IsEmpty)
						{
							result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Dept Filter", "Property", chargeCodeDepartment));
						}

						if (!chargeGroup.IsEmpty)
						{
							result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Charge Group", "Property", chargeGroup));
						}

						return result;
					});
			}
		}

		public CodeDescriptionPairList SupplyTypes
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.GetActiveCodeDescriptionPairList();
			}
		}

		#region Tax Branch

		public override GlbBranchCollection CostTaxBranches
		{
			get { return FindboxLookupCollections.GetActiveBranchForCompanyCollection(Factory); }
		}

		public override GlbBranchCollection SellTaxBranches
		{
			get { return FindboxLookupCollections.GetActiveBranchForCompanyCollection(Factory); }
		}

		#endregion

		#region Implementation

		BaseCharge Charge
		{
			get
			{
				if (charge == null)
				{
					charge = (BaseCharge)Parent;
				}
				return charge;
			}
		}
		BaseCharge charge;

		#endregion

	}
}
