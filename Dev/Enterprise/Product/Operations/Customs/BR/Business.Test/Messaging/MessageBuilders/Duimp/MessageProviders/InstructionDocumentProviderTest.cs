using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class InstructionDocumentProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(InstructionDocumentProvider.New(null));
			AssertType<InstructionDocumentProvider>(InstructionDocumentProvider.New(Factory.New<DispatchInstructionNumber>()));
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var dispatchNumber = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;

			var dataProvider = InstructionDocumentProvider.New(dispatchNumber);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be ", "49", dataProvider.Type);
				AssertEquals("KeyWords count should be", 1, dataProvider.KeyWords.Count());
			});

			dispatchNumber.CE_EntryType = DispatchInstructionDocumentTypes.Codes._28;

			dataProvider = InstructionDocumentProvider.New(dispatchNumber);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be ", "30", dataProvider.Type);
				AssertEquals("KeyWords count should be", 1, dataProvider.KeyWords.Count());
			});
		}
	}
}
