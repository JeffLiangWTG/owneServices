using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderDocManagerInfo))]
	class NctsHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestDocType()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var info = header.DocManagerInfo;
			AssertEquals(Enterprise.Core.Constants.DocManagerCodes.NctsInBond, info.DocManagerCode);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Messages.AddNew();
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			item.BY_Description = "X";
			return header;
		}

		public void TestRelatedObjects_Phase5Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var relatedObjects = ((IDocManagerSupport)header).DocManagerInfo.RelatedObjects;

			AssertCollectionContains(header.MovementHeader, relatedObjects);
		}
	}
}
