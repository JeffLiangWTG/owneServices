using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class MSXMessageSendingObjectAttachmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var attachment = msxMessageSendingObject.Attachments.AddNew();

			AssertEquals(1, attachment.Lookups.TypeList.Count);
			Assert(attachment.Lookups.TypeList.ContainsCode("OR"));
		}

		public void TestFileList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var attachment = msxMessageSendingObject.Attachments.AddNew();

			AssertEquals(1, attachment.Lookups.FileList.Count);
			AssertEquals(eDoc.UniqueKey, ((AvailableEDocList)attachment.Lookups.FileList)[0].PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType("JPDOC", "JPDOC", Core.Constants.CountryCodes.Japan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPDOC", "OR", startDate, endDate);
			Factory.Save();
		}
	}
}
