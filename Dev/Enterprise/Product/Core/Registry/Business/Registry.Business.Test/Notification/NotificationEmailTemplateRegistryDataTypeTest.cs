using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationEmailTemplateRegistryDataType))]
	class NotificationEmailTemplateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NotificationEmailTemplateRegistryDataType>
	{
		protected override NotificationEmailTemplateRegistryDataType GetNewDataType()
		{
			return new NotificationEmailTemplateRegistryDataType(typeof(BusinessObject));
		}

		protected override string ExpectedEditorName
		{
			get { return "NotificationEmailTemplateRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new NotificationEmailTemplate(typeof(BusinessObject), "test email subject", "test notification email body");
			var dataType = new NotificationEmailTemplateRegistryDataType(typeof(BusinessObject));

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, dataType.Serialise(sample)),
			};
		}
	}
}
