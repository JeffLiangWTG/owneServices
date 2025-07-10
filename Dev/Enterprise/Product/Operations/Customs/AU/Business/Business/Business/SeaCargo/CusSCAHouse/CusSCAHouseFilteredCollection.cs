using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseFilteredCollection : FilteredCollection<CusSCAHouse>
	{
		public CusSCAHouseFilteredCollection(CusSCAOceanBill oceanBill)
			: base(oceanBill.HouseBills)
		{
			this.oceanBill = oceanBill;
			oceanBill.HouseBills.DeletingHouseBillWhenDisallowed += OnHouseBillsDeletingHouseBillWhenDisallowed;
		}

		public event EventHandler DeletingHouseBillWhenDisallowed;

		#region Implementation

		readonly CusSCAOceanBill oceanBill;

		protected override bool IsThisPartOfTheCollection(BusinessObject bObject)
		{
			CusSCAHouse house = bObject as CusSCAHouse;
			return house != null && IsMatchingFilter(house);
		}

		protected override void RebuildCore()
		{
			if (oceanBill.InvalidHouseBillsOnlyFilter)
			{
				oceanBill.HouseBills.RunPreSaveValidation();
			}
			base.RebuildCore();
		}

		protected override bool IsFilterEmpty
		{
			get
			{
				return
					oceanBill.CustomsShipmentStatusFilter.IsEmpty &&
					oceanBill.CustomsMessageStatusFilter.IsEmpty &&
					!oceanBill.InvalidHouseBillsOnlyFilter;
			}
		}

		protected override void ClearFilterCore()
		{
			oceanBill.CustomsShipmentStatusFilter = ZString.Empty;
			oceanBill.CustomsMessageStatusFilter = ZString.Empty;
			oceanBill.InvalidHouseBillsOnlyFilter = false;
		}

		protected virtual bool IsInvalidHouseBillsOnlyMatching(CusSCAHouse houseBill)
		{
			return houseBill.HasErrors || houseBill.HasMessageErrors;
		}

		protected virtual bool IsMatchingFilter(CusSCAHouse houseBill)
		{
			bool result = IsFilterEmpty;

			if (!oceanBill.CustomsShipmentStatusFilter.IsEmpty)
			{
				result |= IsCustomsShipmentStatusMatching(houseBill, oceanBill.CustomsShipmentStatusFilter);
			}
			if (!oceanBill.CustomsMessageStatusFilter.IsEmpty)
			{
				result |= IsCustomsMessageStatusMatching(houseBill, oceanBill.CustomsMessageStatusFilter);
			}
			if (oceanBill.InvalidHouseBillsOnlyFilter)
			{
				result |= IsInvalidHouseBillsOnlyMatching(houseBill);
			}

			return result;
		}

		void OnHouseBillsDeletingHouseBillWhenDisallowed(object sender, EventArgs e)
		{
			if (sender is CusSCAHouse)
			{
				RaiseDeletingHouseBillWhenDisallowed(EventArgs.Empty);
			}
		}

		void RaiseDeletingHouseBillWhenDisallowed(EventArgs e)
		{
			if (DeletingHouseBillWhenDisallowed != null)
			{
				DeletingHouseBillWhenDisallowed(this, e);
			}
		}

		protected bool IsCustomsShipmentStatusMatching(CusSCAHouse houseBill, ZString status)
		{
			bool result;

			switch (status)
			{
				case CMRConsolidatedCargoStatuses.Filter.Codes.NotClear:
					result = true;
					foreach (CodeDescriptionPair clearStatus in CMRConsolidatedCargoStatuses.AllClearStatus)
					{
						if (houseBill.CA_ShipmentStatus == clearStatus.Code)
						{
							result = false;
							break;
						}
					}
					break;
				default:
					result = houseBill.CA_ShipmentStatus == status;
					break;
			}

			return result;
		}

		protected bool IsCustomsMessageStatusMatching(CusSCAHouse houseBill, ZString status)
		{
			bool result;

			switch (status)
			{
				case CMRBaseStatuses.Codes.NotSent:
					result =
						houseBill.CA_MessageStatus == status ||
						houseBill.CA_MessageStatus == ZString.Empty;
					break;
				default:
					result = houseBill.CA_MessageStatus == status;
					break;
			}

			return result;
		}

		#endregion // Implementation
	}
}
