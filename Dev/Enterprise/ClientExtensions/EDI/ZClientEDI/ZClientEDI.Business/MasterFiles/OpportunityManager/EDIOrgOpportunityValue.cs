using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgOpportunityValue : OrgOpportunityValue
	{
		public EDIOrgOpportunityValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Estimated Value Used Value

		protected override ZDecimal EstimatedValueUsedValue
		{
			get { return CapitalisedValue; }
		}

		#endregion

		#region PV_Value

		public override ZDecimal PV_Value
		{
			get { return base.PV_Value; }
			set
			{
				base.PV_Value = value;
				UpdateMaintenanceItem();
			}
		}

		#endregion

		#region PV_DiscountPercent

		public override ZDecimal PV_DiscountPercent
		{
			get { return base.PV_DiscountPercent; }
			set
			{
				base.PV_DiscountPercent = value;
				UpdateMaintenanceItem();
			}
		}

		#endregion

		#region Maintenance Item

		void UpdateMaintenanceItem()
		{
			if (Opportunity != null && PV_RevenueType == EDIOrgOpportunityValueLookups.ValueTypeConstants.Sales)
			{
				MaintenanceValueItem.PV_Value = PV_Value * 0.2m;
			}
		}

		OrgOpportunityValue MaintenanceValueItem
		{
			get
			{
				OrgOpportunityValue result = null;
				foreach (OrgOpportunityValue value in ValueCollection)
				{
					if (value.PV_RevenueType == EDIOrgOpportunityValueLookups.ValueTypeConstants.Maintenance)
					{
						result = value;
						break;
					}
				}

				if (result == null)
				{
					result = ValueCollection.AddNew();
					result.PV_RevenueType = EDIOrgOpportunityValueLookups.ValueTypeConstants.Maintenance;
				}

				return result;
			}
		}

		#endregion

		#region Capitalised Value

		public ZDecimal CapitalisedValue
		{
			get { return ValueAfterDiscount; }
		}

		public ZPropertyInfo CapitalisedValueInfo
		{
			get { return GetZPropertyInfo(nameof(CapitalisedValue)); }
		}

		#endregion

		#endregion

		#region Lookups

		protected override OrgOpportunityValueLookups GetNewLookups()
		{
			return new EDIOrgOpportunityValueLookups(this);
		}

		public new EDIOrgOpportunityValueLookups Lookups
		{
			get { return (EDIOrgOpportunityValueLookups)base.Lookups; }
		}

		#endregion

		#region Parent Values Collection

		OrgOpportunityValueCollection ValueCollection
		{
			get { return Opportunity == null ? null : Opportunity.ValueItems; }
		}

		#endregion
	}
}

