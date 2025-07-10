using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateExpiryNotificationGroupRegistryDataType))]
	class EInvoicingCertificateExpiryNotificationGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EInvoicingCertificateExpiryNotificationGroupRegistryDataType>
	{
		protected override EInvoicingCertificateExpiryNotificationGroupRegistryDataType GetNewDataType()
		{
			return new EInvoicingCertificateExpiryNotificationGroupRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "EInvoicingCertificateExpiryNotificationGroupRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			var sample1 = new ValidSampleAndBinaryValueInDB(new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroup.PK }
				, Encoding.Unicode.GetBytes($"<?xml version=\"1.0\" encoding=\"utf-16\"?><EInvoicingCertificateExpiryNotificationGroup><AlertDays>10</AlertDays><NotificationGroup>{notificationGroup.PK}</NotificationGroup></EInvoicingCertificateExpiryNotificationGroup>"));

			var sample2 = new ValidSampleAndBinaryValueInDB(new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 0, NotificationGroup = ZGuid.Empty }
				, Encoding.Unicode.GetBytes($"<?xml version=\"1.0\" encoding=\"utf-16\"?><EInvoicingCertificateExpiryNotificationGroup><AlertDays>0</AlertDays><NotificationGroup>{ZGuid.Empty}</NotificationGroup></EInvoicingCertificateExpiryNotificationGroup>"));

			return new[] { sample1, sample2 };
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}