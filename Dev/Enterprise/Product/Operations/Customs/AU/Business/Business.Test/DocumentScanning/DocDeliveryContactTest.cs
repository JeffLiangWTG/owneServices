using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DocDeliveryContact))]
	sealed class DocDeliveryContactTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNotifyModes()
		{
			var registryItem = (BooleanRegistryItem)RawDataRegistry.Instance.NEXDOCSDisableQRPView;

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = new DocDeliveryContact(Factory);
				var expectedCodes = new[] { "Print" };

				AssertArrayEqualsByElements("Should only return Print when the value of NEXDOCSDisableQRPView is true.", expectedCodes, contact.NotifyModes.GetAllCodes());
			}

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = new DocDeliveryContact(Factory);
				var expectedCodes = new[] { "E-Mail", "Fax", "Print", "ePrint" };

				AssertArrayEqualsByElements("Should only return default codes when the value of NEXDOCSDisableQRPView is false.", expectedCodes, contact.NotifyModes.GetAllCodes());
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new DocDeliveryContact(Factory);
	}
}
