using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEdiMessageDocManagerInfo))]
	class NctsEdiMessageDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestDocType()
		{
			var nctsEdiMessage = Factory.New<NctsEdiMessage>();
			var docManagerInfo = nctsEdiMessage.DocManagerInfo;
			AssertEquals(Enterprise.Core.Constants.DocManagerCodes.EDIMessage, docManagerInfo.DocManagerCode);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<NctsEdiMessage>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsEdiMessage = departure.Messages.AddNew();
			return nctsEdiMessage;
		}
	}
}
