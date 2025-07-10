using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocCMRREFACCMessageTest : DocumentWrapperTest
	{
		public void TestREFACCInfoProvider()
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

				ZString response = Enterprise.Customs.AU.Declaration.Business.CMRImportDeclarationTestData.REFACC;
				var message = Factory.New<CMRREFACCMessage>();
				message.EM_MessageText = response;
				entryHeader.Messages.Add(message);

				DocCMRREFACCMessage messageWrapper = DocCMRREFACCMessage.New(message, Factory);
				AssertNotNull("REFACCInfoProvider should not be null", messageWrapper.InfoProvider);
				AssertNotNull("Entry Header should not be null", messageWrapper.EntryHeader);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}
	}
}
