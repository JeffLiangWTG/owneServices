namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;

	public class CusSCAPivotCollectionForHouse : ActiveBusinessObjectCollection<CusSCAPivot>, IBusinessObjectCollection, Customs.Business.IOverrideDefaultValuesCollection
	{
		public CusSCAPivotCollectionForHouse(CusSCAHouse cusSCAHouse)
			: base(cusSCAHouse)
		{
			master = cusSCAHouse;
		}

		readonly CusSCAHouse master;

		public CusSCAPivot FindByContainerAndShipmentPackLine(ZString containerNumber, PackLine shipmentPackLine)
		{
			return this.FirstOrDefault(pivot => pivot.CV_AssociatedContainer == containerNumber
																			 && pivot.CV_PackageType == D96AMessageUtilities.ConvertPackUnitToACROSSUnit(shipmentPackLine.JL_F3_NKPackType)
																			 && pivot.CV_GoodsDescription == CusSCAPivotSynchroniser.GetDescriptionFromShipmentPackLine(shipmentPackLine)
																			 && pivot.UNDGs.SequenceEqual(shipmentPackLine.UNDGs, new UNDGDataItemComparer()));
		}

		public void SetUNDGsReadOnly(bool isReadOnly)
		{
			foreach (CusSCAPivot pivot in this)
			{
				pivot.UNDGs.SetReadOnlyIncludingChildren(isReadOnly);
				pivot.UNDGs.FirstItemForBinding.SetReadOnlyIncludingChildren(isReadOnly);
			}
		}

		bool Customs.Business.IOverrideDefaultValuesCollection.IsOverrideDefaultValuesEnabled => master.IsAttachedToShipment;
		ZPropertyInfoBool Customs.Business.IOverrideDefaultValuesCollection.OverrideDefaultValuesInfo => (ZPropertyInfoBool)master.CA_OverrideFreightDefaultsInfo;
	}

	class UNDGDataItemComparer : IEqualityComparer<UNDGDataItem>
	{
		#region Implementation of IEqualityComparer<UNDGDataItem>

		bool IEqualityComparer<UNDGDataItem>.Equals(UNDGDataItem x, UNDGDataItem y)
		{
			return x.DI_DG == y.DI_DG
						 && x.DI_DGFlashPoint == y.DI_DGFlashPoint
						 && x.DI_OC_DGContact == y.DI_OC_DGContact;
		}

		int IEqualityComparer<UNDGDataItem>.GetHashCode(UNDGDataItem obj)
		{
			return obj.DI_DG.GetHashCode()
						 ^ obj.DI_DGFlashPoint.GetHashCode()
						 ^ obj.DI_OC_DGContact.GetHashCode();
		}
		#endregion
	}
}
