using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class ElectronicTest : DeliveryMethodTest
	{
		public void TestElectronicDelivery()
		{
			var contact = new DocDeliveryContact(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			contact.OrgHeaderPK = orgHeader.PK;

			var workflowProvider = (IWorkflowProvider)Factory.New<IForwardingShipment>();

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			deliveryInfo.ElectronicData = workflowProvider;

			var method = new DummyElectronicDelivery(contact);
			method.AddFile(deliveryInfo);
			method.Deliver();

			const string triesToSendForOrgHeader = @"- No EDI Communications settings were found on the Recipient Organization [TESTORG].
Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
Failure was because:
Organization [TESTORG] for Company [EDI] has no matching Communication Modes.
- Electronic Data Export succeeded.
";
			AssertEquals(triesToSendForOrgHeader, method.log);
		}

		#region Classes

		class DummyElectronicDelivery : Electronic
		{
			public DummyElectronicDelivery(DocDeliveryContact contact)
				: base(contact)
			{
			}

			protected override void DeliverCore(INotifications notifications = null)
			{
				log = string.Empty;
				base.DeliverCore();
			}

			protected override INotifications Notifications
			{
				get { return notifications ?? (notifications = new DummyNotifications(this)); }
			}
			INotifications notifications;

			public string log;

			class DummyNotifications : INotifications
			{
				public DummyNotifications(DummyElectronicDelivery parent)
				{
					this.parent = parent;
				}
				readonly DummyElectronicDelivery parent;

				void INotifications.Add(INotification notification)
				{
					parent.log += string.Format("- {0}\r\n", notification.Message);
				}
			}
		}

		#endregion
	}
}
