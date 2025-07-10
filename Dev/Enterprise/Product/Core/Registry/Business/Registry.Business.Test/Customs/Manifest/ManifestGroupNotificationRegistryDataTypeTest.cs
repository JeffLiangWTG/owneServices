using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.Customs.ManifestGroupNotificationRegistryItem;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(ManifestGroupNotificationRegistryDataType))]
	sealed class ManifestGroupNotificationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ManifestGroupNotificationRegistryDataType>
	{
		protected override ManifestGroupNotificationRegistryDataType GetNewDataType()
		{
			return new ManifestGroupNotificationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var manifestGroupNotification1 = ManifestGroupNotification.Default;
			var manifestGroupNotification2 = new ManifestGroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty, true);

			return
			[
				new ValidSampleAndBinaryValueInDB(manifestGroupNotification1, new ManifestGroupNotificationRegistryDataType().Serialise(manifestGroupNotification1)),
				new ValidSampleAndBinaryValueInDB(manifestGroupNotification2, new ManifestGroupNotificationRegistryDataType().Serialise(manifestGroupNotification2))
			];
		}

		protected override string ExpectedEditorName
		{
			get { return "ManifestGroupNotificationRegistryItemEditor"; }
		}
	}
}
