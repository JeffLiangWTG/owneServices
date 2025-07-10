using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocCMRPAYRECMessageTest : DocumentWrapperTest
	{
		public void TestDocCMRPAYRECMessage()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("AU");

			try
			{
				JobDeclaration declaration = JobDeclaration.New(Factory);
				declaration.JE_DeclarationReference = "B00122382";

				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "B00122382/1";

				Factory.Save();

				ZString response = Enterprise.Customs.AU.Declaration.Business.CMRImportDeclarationTestData.PAYREC;
				var message = Factory.New<CMRPAYRECMessage>();
				message.EM_MessageText = response;
				entryHeader.Messages.Add(message);

				DocCMRPAYRECMessage messageWrapper = DocCMRPAYRECMessage.New(message, Factory);
				AssertNotNull("PAYRECInfoProvider should not be null", messageWrapper.InfoProvider);
				AssertNotNull("Entry Header should not be null", messageWrapper.EntryHeader);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}
	}
}
