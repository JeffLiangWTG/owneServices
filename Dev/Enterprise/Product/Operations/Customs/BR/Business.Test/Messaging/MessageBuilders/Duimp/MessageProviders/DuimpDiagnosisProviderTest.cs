using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DuimpDiagnosisProviderTest : TestCaseWithFactory
	{
		public void TestTotalItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();

			var sendingObject = new DuimpMessageSendingObject(entryHeader);
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.DIA;

			var dataProvider = new DuimpDiagnosisProvider(sendingObject);
			AssertEquals(2, dataProvider.TotalItem);
		}
	}
}
