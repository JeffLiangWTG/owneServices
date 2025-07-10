using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	internal class Electronic : DeliveryMethod
	{
		public Electronic(DocDeliveryContact docContact)
		{
			this.docContact = Argument.NotNull(docContact, "docContact");
		}

		readonly DocDeliveryContact docContact;

		protected override void DeliverCore(INotifications notification = null)
		{
			// Document Engine doesn't have an atomic save. 
			// In a way, it can't, as many of these export mechanisms interface with IO that doesn't support transactions
			// We ought to implement something like 3 phase commit in these cases but this has not been done.
			// This means that error handling is lacklustre.
			// Thankfully ManualDataExport doesn't have any external IO so in this case we can retry until it works.
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory
				{
					RefreshEnabled = false,
					NameForDebugging = "Electronic Delivery Factory"
				};

				foreach (var info in DeliveryInfos)
				{
					var dataExport = new ManualDataExport(factory, info.ElectronicData, UniversalDataType.UniversalShipment);
					dataExport.OverrideRecipient(docContact.OrgHeader);
					dataExport.SendData(Notifications);
				}

				factory.Save();
				Notifications.Add(new InfoNotification((NoResString)"Electronic Data Export succeeded.")); // Service task logs aren't res strings
			}, () => Notifications.AddWarning((NoResString)"Error during delivery. Retrying."));
		}

#if DEBUG
		protected virtual
#endif
		INotifications Notifications
		{
			get { return notifications ?? (notifications = new SilentNotifications()); }
		}
		INotifications notifications;

		class SilentNotifications : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}
	}
}
