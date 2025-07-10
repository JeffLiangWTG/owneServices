using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DispatchInstructionDocumentTypesTest : TestCase
	{
		public void TestDispatchInstructionDocumentTypes()
		{
			AssertEquals("Customs Code should be ", "49", DispatchInstructionDocumentTypes.MapToCustomsCode(DispatchInstructionDocumentTypes.Codes._01));
			AssertEquals("Customs Code should be ", "30", DispatchInstructionDocumentTypes.MapToCustomsCode(DispatchInstructionDocumentTypes.Codes._28));
			Assert("Customs Code should be empty", DispatchInstructionDocumentTypes.MapToCustomsCode(DispatchInstructionDocumentTypes.Codes._02).IsEmpty());
			Assert("Customs Code should be empty", DispatchInstructionDocumentTypes.MapToCustomsCode(string.Empty).IsEmpty());
		}
	}
}
