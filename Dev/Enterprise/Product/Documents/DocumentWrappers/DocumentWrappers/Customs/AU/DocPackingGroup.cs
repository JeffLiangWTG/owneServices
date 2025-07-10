using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocPackingGroup : DocumentWrapper
	{
		DocPackingGroup(PackingGroup packingGroup, BusinessObjectFactory factory)
			: base(packingGroup, factory)
		{
		}

		public static DocPackingGroup New(PackingGroup packingGroup, BusinessObjectFactory factory)
		{
			return (packingGroup == null) ? null : new DocPackingGroup(packingGroup, factory);
		}

		public override string ToString()
		{
			return HouseBillNumber;
		}

		public ZString MarksAndNumbers
		{
			get { return PackingGroup.MarksAndNumbers; }
		}

		public ZInt NumberOfPackages
		{
			get { return PackingGroup.TotalNumberOfPackages; }
		}

		public ZInt WarehouseNumberOfPackages
		{
			get { return PackingGroup.WarehouseNumberOfPackages; }
		}

		public ZInt PackingUnitCount
		{
			get { return PackingGroup.OuterPackingUnitCount; }
		}

		public ZString HouseBillNumber
		{
			get { return (Bill != null && Bill.IsHouseBill) ? Bill.CU_BillNum : ZString.Empty; }
		}

		public ZString MasterBillNumber
		{
			get { return (Bill != null) ? Bill.CU_MasterBill : ZString.Empty; }//will point to the parent master bill
		}

		public ZString PartShipConsignmentReference
		{
			get { return (Bill != null) ? Bill.CU_fPartShipConsignmentReference : ZString.Empty; }
		}

		public ZShort HouseContainerNumber
		{
			get { return PackingGroup.CR_HouseContainerNumber; }
		}

		public ZString ContainerNumber
		{
			get { return Container != null ? Container.CO_ContainerNumber : ZString.Empty; }
		}

		public ZString ContainerType
		{
			get { return Container != null ? Container.CO_FCL_LCL_AIR : ZString.Empty; }
		}

		public ZString FormattedContainerType
		{
			get
			{
				ZString containerDetails = ZString.Empty;
				if (Container != null)
				{
					string containerNo = Container.CO_ContainerNumber;
					string containerType = Container.CO_FCL_LCL_AIR;
					containerDetails = "(" + containerType + ")" + containerNo;
				}
				else if (PackingGroup.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.BreakBulk ||
					PackingGroup.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.Bulk ||
					PackingGroup.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.Liquid)
				{
					ZString containerType = PackingGroup.Declaration.JE_ContainerMode;
					containerDetails = "(" + containerType + ")";
				}

				return containerDetails;
			}
		}

		#region Implementation

		PackingGroup PackingGroup
		{
			get { return (PackingGroup)WrappedObject; }
		}

		protected CusContainer Container
		{
			get { return PackingGroup.Container; }
		}

		protected Bill Bill
		{
			get { return PackingGroup.Bill; }
		}

		#endregion
	}
}
