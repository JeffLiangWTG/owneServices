using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeletedPackingGroupAmendment : IPackingGroup
	{
		public DeletedPackingGroupAmendment(ZShort number)
		{
			this.number = number;
		}
		readonly ZShort number;

		#region IPackingGroup Members

		ZShort IPackingGroup.HouseContainerNumber
		{
			get { return number; }
		}

		ZString IPackingGroup.ActionCodeForMessage(CusEntryHeader entryHeader)
		{
			return LineAction.Delete;
		}

		JobDeclaration IPackingGroup.Declaration
		{
			get { return null; }
		}

		ZInt IPackingGroup.NumberOfPackages
		{
			get { return ZInt.Zero; }
		}

		ZInt IPackingGroup.WarehouseNumberOfPackages
		{
			get { return ZInt.Zero; }
		}

		ZInt IPackingGroup.PackingUnitCount
		{
			get { return ZInt.Zero; }
		}

		ZString IPackingGroup.ContainerMode
		{
			get { return ZString.Empty; }
		}

		ZString IPackingGroup.MasterBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IPackingGroup.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IPackingGroup.ContainerNumber
		{
			get { return ZString.Empty; }
		}

		ZString IPackingGroup.MarksAndNumbers
		{
			get { return ZString.Empty; }
		}

		ZString IPackingGroup.ConsignRefNumber
		{
			get { return ZString.Empty; }//Leon please confirm if this is right. When we send 'delete' of packing line, we should still send a line number only?
		}

		#endregion
	}
}
