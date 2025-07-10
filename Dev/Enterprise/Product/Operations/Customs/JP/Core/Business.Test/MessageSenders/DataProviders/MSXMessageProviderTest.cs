using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MSXMessageProvider))]
	sealed class MSXMessageProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1024], "invoice.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1024 * 1024 * 10], "invoice2.pdf", "CIV");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "12345678901";
			entryHeader.CH_PhaseStatus = JPProcedureCodeList.Codes.EDC;
			var msxMessageSendingObject = new MSXMessageSendingObject(entryHeader);
			msxMessageSendingObject.RegistrationType = true;
			msxMessageSendingObject.Communication = "Test";
			var attachment = msxMessageSendingObject.Attachments.AddNew();
			attachment.File = eDoc.UniqueKey;
			attachment.Type = "OR";
			var attachment2 = msxMessageSendingObject.Attachments.AddNew();
			attachment2.File = eDoc2.UniqueKey;
			attachment2.Type = "OR";
			var provider = new MSXMessageProvider(msxMessageSendingObject);

			AssertEquals("File Name", "invoice.pdf", provider.Files.ToArray()[0].FileName);
			AssertEquals("File Size", new decimal(1), provider.Files.ToArray()[0].FileSize);
			AssertEquals("File Size when 10 MB", new decimal(9999), provider.Files.ToArray()[1].FileSize);
			AssertEquals("Document Type", "OR", provider.Files.ToArray()[0].DocumentType);
			AssertEquals("Declaration Number", "12345678901", provider.DeclarationNumber);
			AssertEquals("Declaration Type", JPProcedureCodeList.Codes.EDC, provider.DeclarationType);
			AssertEquals("Registration Type", "T", provider.RegistrationType);
			AssertEquals("Communication Column", "Test", provider.CommunicationColumn);

			msxMessageSendingObject.RegistrationType = false;
			provider = new MSXMessageProvider(msxMessageSendingObject);
			AssertEquals("Registration Type", string.Empty, provider.RegistrationType);
		}
	}
}
