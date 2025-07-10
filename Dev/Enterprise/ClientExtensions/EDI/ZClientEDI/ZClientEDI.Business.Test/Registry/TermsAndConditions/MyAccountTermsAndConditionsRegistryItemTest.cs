using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TermsAndConditionsRegistryItem))]
	class MyAccountTermsAndConditionsRegistryItemTest : StronglyTypedRegistryItemTestCase<NotificationEmailTemplate>
	{
		protected override StronglyTypedRegistryItem<NotificationEmailTemplate, NotificationEmailTemplate> GetNewRegistryItem()
		{
			return new TermsAndConditionsRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(BusinessObject));
		}
	}

	[TestedType(typeof(TermsAndConditionsRegistryDataType))]
	class MyAccountTermsAndConditionsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TermsAndConditionsRegistryDataType>
	{
		#region Implementation

		protected override TermsAndConditionsRegistryDataType GetNewDataType()
		{
			return new TermsAndConditionsRegistryDataType(typeof(BusinessObject));
		}

		protected override string ExpectedEditorName
		{
			get { return "TermsAndConditionsRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			NotificationEmailTemplate sample = new NotificationEmailTemplate(typeof(BusinessObject), "version 1.0.0", "test content");

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,
				0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,32,0,49,0,46,0,48,0,46,0,48,0,60,0,47,0,69,0,109,0,97,
				0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,66,0,111,0,100,0,121,0,62,0,116,0,101,0,115,0,116,0,32,0,99,0,111,0,110,0,116,0,101,0,110,0,116,
				0,60,0,47,0,69,0,109,0,97,0,105,0,108,0,66,0,111,0,100,0,121,0,62,0,60,0,47,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}

		#endregion
	}
}
