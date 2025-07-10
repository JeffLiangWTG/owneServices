using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer.Phase4;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.GB.Business.Testing
{
	class NctsHeaderDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReaderCreatesTheCorrectTypes()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_Description = "TEST DESCRIPTION";
			var writeManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(header);

			CombineAssertions(() =>
			{
				var reader = new NctsHeaderDataObjectReader(headerData, new TestErrorLogger(), Factory);
				var gbHeader = reader.ReadIntoBusinessObject();
				AssertType<NctsHeader>("Header Type", gbHeader);
				AssertType<NctsDepartureMovementHeader>("Movement Header Type", gbHeader.MovementHeader);
				AssertType<NctsDepartureCargoDesc>("Goods Item Type", gbHeader.MovementHeader.GoodsItems[0]);
			});
		}
	}
}
