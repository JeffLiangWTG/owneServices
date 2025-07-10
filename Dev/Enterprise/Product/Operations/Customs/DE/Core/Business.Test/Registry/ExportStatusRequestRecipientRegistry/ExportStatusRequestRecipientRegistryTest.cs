using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(ExportStatusRequestRecipientRegistry))]
	class ExportStatusRequestRecipientRegistryTest : RegistryBusinessObjectTemplateTestCase<ExportStatusRequestRecipientRegistry>
	{
		public void TestMessageReceipientMandatory()
		{
			exportStatusRequestRecipientRegistry.ValidateMessageRecipient();
			AssertHasErrorContaining(exportStatusRequestRecipientRegistry.MessageRecipientInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestMessageRecipientFormat()
		{
			var warningMessage = "The length should be 8 characters for Recipient Code and start with 'DE'.";
			CombineAssertions(() =>
			{
				exportStatusRequestRecipientRegistry.MessageRecipient = "AA";
				AssertHasWarning("Invalid prefix", exportStatusRequestRecipientRegistry.MessageRecipientInfo, warningMessage);

				exportStatusRequestRecipientRegistry.MessageRecipient = "DE1234";
				AssertHasWarning("Short", exportStatusRequestRecipientRegistry.MessageRecipientInfo, warningMessage);

				exportStatusRequestRecipientRegistry.MessageRecipient = "DE123456";
				AssertNoWarning("Correct", exportStatusRequestRecipientRegistry.MessageRecipientInfo, warningMessage);
			});
		}

		public void TestSystemCodeReadOnly()
		{
			AssertEquals(true, exportStatusRequestRecipientRegistry.SystemCodeInfo.ReadOnly);
		}
		public void TestAtlasMessageRecipient()
		{
			AssertEquals("DE001348", ExportStatusRequestRecipientRegistry.CurrentAtlasMessageRecipient);
		}
		public void TestAESVersionNumber()
		{
			AssertEquals("DE001342", ExportStatusRequestRecipientRegistry.CurrentAESMessageRecipient);
		}
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override ExportStatusRequestRecipientRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override ExportStatusRequestRecipientRegistry GetBusinessObjectToSerialise()
		{
			BizObj.SystemCode = ExportStatusRequestRecipientRegistry.AtlasSystemCode;
			BizObj.MessageRecipient = ExportStatusRequestRecipientRegistry.CurrentAtlasMessageRecipient;
			return BizObj;
		}

		protected override void SetUp()
		{
			base.SetUp();
			exportStatusRequestRecipientRegistry = new ExportStatusRequestRecipientRegistry { SystemCode = ExportStatusRequestRecipientRegistry.AtlasSystemCode };
		}
		ExportStatusRequestRecipientRegistry exportStatusRequestRecipientRegistry;
	}

	[TestedType(typeof(ExportStatusRequestRecipientRegistryItem))]
	class ExportStatusRequestRecipientRegistryItemTest : StronglyTypedRegistryItemTestCase<ExportStatusRequestRecipientRegistryCollection>
	{
		protected override StronglyTypedRegistryItem<ExportStatusRequestRecipientRegistryCollection, ExportStatusRequestRecipientRegistryCollection> GetNewRegistryItem() => new ExportStatusRequestRecipientRegistryItem("", null, null, null, RegistryStorageFlags.All, new ExportStatusRequestRecipientRegistryCollection().DefaultCollection);
	}
}
