using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EntityFramework.ZNotificationCollector;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageSendingObjectParent : MessageSendingObjectParent<G3MessageSendingObject>
	{
		public G3MessageSendingObjectParent(EU.H7.Business.AsycudaManifestHeader manifestHeader, bool isRevoke = false) : base(manifestHeader)
		{
			this.isRevoke = isRevoke;
		}

		readonly bool isRevoke;

		[ResourceStringData("2a557284-4c70-4407-b6b0-4bd1814e42e7", Caption = "Override Revoke Reason default values")]
		public bool OverrideDefaultRevokeReason
		{
			get => overrideDefaultRevokeReason;
			set
			{
				overrideDefaultRevokeReason = value;
				if (!overrideDefaultRevokeReason)
				{
					RevokeReason = ZString.Empty;
					RevokeReasonDescription = ZString.Empty;
				}

				RefreshBinding();
			}
		}

		bool overrideDefaultRevokeReason;

		[ResourceStringData("b4cbe4bd-82f4-4950-827b-5bf890785a81", Caption = "Revoke Reason")]
		[List(nameof(RevokeReasonList))]
		[ReadOnlyMember(nameof(RevokeReason_ReadOnly))]
		public ZString RevokeReason
		{
			get => revokeReason;
			set
			{
				if (revokeReason != value)
				{
					revokeReason = value;
					SetRevokeReasonForAllBills(revokeReason);
				}
			}
		}

		ZString revokeReason;

		bool RevokeReason_ReadOnly => !OverrideDefaultRevokeReason;

		public CodeDescriptionPairList RevokeReasonList => Factory.GetCachedValue<ESH7G3RevokeReasonList>();

		void SetRevokeReasonForAllBills(ZString revokeReason)
		{
			foreach (G3MessageSendingObject messageSendingObject in SendingObjectsCollection)
			{
				messageSendingObject.RevokeReason = revokeReason.IsEmpty ? messageSendingObject.DefaultRevokeReason : revokeReason;
			}
		}

		[ResourceStringData("30e1db74-58ff-4950-963f-92e381051b0c", Caption = "Description")]
		[ReadOnlyMember(nameof(RevokeReasonDescription_ReadOnly))]
		[MaxLength(512)]
		public ZString RevokeReasonDescription
		{
			get => revokeReasonDescription;
			set
			{
				if (revokeReasonDescription != value)
				{
					SetNonPersistentPropertyValue(RevokeReasonDescriptionInfo, ref revokeReasonDescription, value);
					SetRevokeReasonDescriptionForAllBills(revokeReasonDescription);
				}
			}
		}

		ZString revokeReasonDescription;

		public ZPropertyInfo RevokeReasonDescriptionInfo => GetZPropertyInfo(nameof(RevokeReasonDescription));

		bool RevokeReasonDescription_ReadOnly => !OverrideDefaultRevokeReason;

		void SetRevokeReasonDescriptionForAllBills(ZString revokeReasonDescription)
		{
			foreach (G3MessageSendingObject messageSendingObject in SendingObjectsCollection)
			{
				messageSendingObject.RevokeReasonDescription = revokeReasonDescription;
			}
		}

		protected override NonPersistentBusinessObjectCollection<G3MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var messageSendingObjectCollection = new MessageSendingObjectCollection<G3MessageSendingObject>(base.Factory);

			foreach (var bill in Header.Bills)
			{
				if (!isRevoke || bill.HasAcceptedG3DMessage())
				{
					messageSendingObjectCollection.Add(new G3MessageSendingObject(bill, isRevoke));
				}
			}

			return messageSendingObjectCollection;
		}

		public AsycudaManifestHeader Header => TopLevelBusinessObject as AsycudaManifestHeader;

		public new IEnumerable<G3MessageSendingObject> SelectedSendingObjects => SendingObjectsCollection.Cast<G3MessageSendingObject>().Where(x => x.ShouldSend);

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(WarningsMessage.ToUniqueMessageListString);

		ZString GetNotificationsMessage(Func<string> getNotificationsMessage) => SelectedSendingObjects.Any() ? Regex.Replace(getNotificationsMessage(), "(?<!\r)\n", "\r\n") : string.Empty;

		IEnumerable<INotification> WarningsMessage => CustomsNotificationCollector.GetWarnings();

		CustomsNotificationCollector CustomsNotificationCollector => new CustomsNotificationCollector(Header, true, false, PropertyDescriptionType.HumanReadableName);
	}
}
