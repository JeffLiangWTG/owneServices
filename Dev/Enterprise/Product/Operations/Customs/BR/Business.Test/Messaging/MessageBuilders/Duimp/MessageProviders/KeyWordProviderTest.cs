using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class KeyWordProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(KeyWordProvider.New(null));
			AssertType<KeyWordProvider>(KeyWordProvider.New(Factory.New<DispatchInstructionNumber>()));
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var dispatchNumber = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber.CE_EntryType = DispatchInstructionDocumentTypes.Codes._28;
			dispatchNumber.CE_EntryNum = "FAT00001";

			var dataProvider = KeyWordProvider.New(dispatchNumber);
			CombineAssertions(() =>
			{
				AssertEquals("Code should be ", 1, dataProvider.Code);
				AssertEquals("Value should be ", "FAT00001", dataProvider.Value);
			});
		}
	}
}
