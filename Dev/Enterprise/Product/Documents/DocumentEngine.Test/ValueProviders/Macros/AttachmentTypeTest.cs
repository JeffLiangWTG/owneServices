using System;
using System.IO;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AttachmentType))]
	sealed class AttachmentTypeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <Attachment Type>", ValueProviderToTest.IsResponsibleForReplacing("<Attachment Type>", Passes.FirstPass));
			Assert("should match < attachment      type       >", ValueProviderToTest.IsResponsibleForReplacing("< attachment      type       >", Passes.FirstPass));
			Assert("should match <AttachmentType>", ValueProviderToTest.IsResponsibleForReplacing("<AttachmentType>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			using (var fileContent = new MemoryStream())
			{
				var deliveryContact = new DocDeliveryContact(Factory);
				var mostOfficialContact = new DocDeliveryContact(Factory);

				deliveryContact.Name = "Sango Test";
				deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				deliveryContact.Email = "sango@test.com";

				DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
				Report.Save(deliveryContact, mostOfficialContact, fileContent);
				AssertEquals("PDF", ValueProviderToTest.GetReplacement("<AttachmentType>", Report));

				deliveryContact.AttachmentType = Core.Constants.FileFormats.HTML;
				DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
				Report.Save(deliveryContact, mostOfficialContact, fileContent);

				AssertEquals("HTML", ValueProviderToTest.GetReplacement("<AttachmentType>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider() => new AttachmentType();

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var fileContent = new MemoryStream();
			var deliveryContact = new DocDeliveryContact(Factory);
			var mostOfficialContact = new DocDeliveryContact(Factory);
			deliveryContact.Name = "Example Test";
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			deliveryContact.Email = "example@test.com";
			deliveryContact.AttachmentType = "PDF";
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			Report.Save(deliveryContact, mostOfficialContact, fileContent);
		}
	}
}
