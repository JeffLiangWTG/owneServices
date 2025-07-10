using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2AmendedItemsHeader : NonPersistentBusinessObject
	{
		public ICS2AmendedItemsHeader(AsycudaManifestHeader header, params string[] requestTypes)
			: this(header.GetCustomsMessageType(true), header, requestTypes)
		{
		}

		public ICS2AmendedItemsHeader(ZString messageType, AsycudaManifestHeader header, params string[] requestTypes)
			: base(header.Factory)
		{
			ManifestHeader = Argument.NotNull(header, nameof(header));
			MessageType = Argument.NotNull(messageType, nameof(messageType));
			RequestTypes = Argument.NotNull(requestTypes, nameof(requestTypes));

			NeedReplyInformation = MessageType == MessageTypes.Codes.R02 || MessageType == MessageTypes.Codes.R03;
		}

		public static class Schema
		{
			public const string RequestHeaderPK = nameof(RequestHeaderPK);
		}

		public AsycudaManifestHeader ManifestHeader { get; }

		public string[] RequestTypes { get; }

		public string MessageType { get; }

		public bool NeedReplyInformation { get; }

		[ChildEditable]
		public ICS2AmendedItemCollection AmendedItems
		{
			get
			{
				if (amendedItems == null)
				{
					amendedItems = new ICS2AmendedItemCollection(Factory);

					using (SuspendSettingHasChanges())
					{
						foreach (var requestHeader in ManifestHeader.RequestHeaders.Cast<RequestHeader>())
						{
							if (RequestTypes.Any(c => c.Equals(requestHeader.EUS_Type, StringComparison.InvariantCultureIgnoreCase)))
							{
								var item = new ICS2AmendedItem(this, requestHeader);

								using (item.SuspendSettingHasChanges())
								{
									item.IsSelected = requestHeader.EUS_Status.IsEmpty && (!NeedReplyInformation || requestHeader.RequestResponses.Any());
								}

								amendedItems.Add(item);
							}
						}
					}

					RegisterEditableChildObject(amendedItems);
				}

				return amendedItems;
			}
		}
		ICS2AmendedItemCollection amendedItems;

		public bool HasAmendedItems()
		{
			return AmendedItems.Count > 0;
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();

			base.RunPreSaveValidationCore();
			if (!AmendedItems.Any(c => ((ICS2AmendedItem)c).IsSelected))
			{
				AddRowError(Res.GetString("8FD268FD-D561-4A7E-B746-8656801F87B1", "Please select at least one valid referral request to send the message."));
			}
		}
	}
}
