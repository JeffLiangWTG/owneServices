using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocARInvoiceTaxMessage))]
	sealed class DocARInvoiceTaxMessageTest : DocumentWrapperTestCase
	{
		public void TestEnglishMessage()
		{
			ZString message = "ENGLISHMESSAGE";
			Message.A9_EnglishMsg = message;
			AssertEquals(message, ((DocARInvoiceTaxMessage)GetDocumentWrappers()[0]).EnglishMessage);
		}

		public void TestLocalMessage()
		{
			ZString message = "LOCALMESSAGE";
			Message.A9_LocalMsg = message;
			AssertEquals(message, ((DocARInvoiceTaxMessage)GetDocumentWrappers()[0]).LocalLanguageMessage);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoiceTaxMessage.New(Message, Factory) };
		}

		AccInvMsg Message;

		protected override void SetUp()
		{
			Message = Factory.New<AccInvMsg>();
			base.SetUp();
		}
	}
}
