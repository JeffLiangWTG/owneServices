using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(MessageSettingsExport))]
	sealed class MessageSettingsExportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationRecipient()
		{
			var dataExport = new MessageSettingsExport(Factory, DummyWorkflowDescriptor.Instance.Code);

			dataExport.RecipientType = "?!?";
			AssertHasErrors(dataExport.RecipientTypeInfo);
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoErrors(dataExport.RecipientTypeInfo);
			dataExport.RecipientType = "";
			AssertHasErrors(dataExport.RecipientTypeInfo);
		}

		public void TestValidationPurposeCode()
		{
			var dataExport = new MessageSettingsExport(Factory, DummyWorkflowDescriptor.Instance.Code);

			dataExport.PurposeCode = "!?!";
			AssertHasErrors(dataExport.PurposeCodeInfo);
			dataExport.PurposeCode = "INV";
			AssertNoErrors(dataExport.PurposeCodeInfo);
			dataExport.PurposeCode = "";
			AssertNoErrors(dataExport.PurposeCodeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MasterFiles.Business.MessageRecipientPartyType.OrgProxy;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageSettingsExport(
				Factory,
				DummyWorkflowDescriptor.Instance.Code);
		}
	}
}
