using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EConversationMessageEmailTemplateRegistryDataType))]
	sealed class EConversationMessageEmailTemplateRegistryDataTypeTest : NotificationEmailTemplateRegistryDataTypeTest
	{
		protected override NotificationEmailTemplateRegistryDataType GetNewDataType()
		{
			return new EConversationMessageEmailTemplateRegistryDataType(typeof(BusinessObject));
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var value = new NotificationEmailTemplate(typeof(BusinessObject), "Email Subject For (*ID*)", "Email Body (*EmailIdentifier*)");
			var anotherValue = new NotificationEmailTemplate(typeof(BusinessObject), "(*ID*) - New messages", "(*EmailIdentifier*)");

			var dataType = new EConversationMessageEmailTemplateRegistryDataType(typeof(BusinessObject));

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(value, dataType.Serialise(value)),
				new ValidSampleAndBinaryValueInDB(anotherValue, dataType.Serialise(anotherValue)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			var noField = new NotificationEmailTemplate(typeof(BusinessObject), "No ID special field in subject", "Email Body (*EmailIdentifier*)");
			var noEmailIdentifier = new NotificationEmailTemplate(typeof(BusinessObject), "Email Subject For (*ID*)", "Email Body");
			var emptySubject = new NotificationEmailTemplate(typeof(BusinessObject), string.Empty, "Email Body");

			return new[] { noField, noEmailIdentifier, emptySubject };
		}
	}
}
