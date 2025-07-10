using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondContainer))]
	sealed class CusInBondContainerTest : Customs.Business.Testing.CusInBondContainerTest<CusInBondContainer>
	{
		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return GetContainer(factory);
		}

		CusInBondContainer GetContainer(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveHeader = header.ArrivalMovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return (CusInBondContainer)moveDetail.Containers.AddNew();
		}
	}
}
