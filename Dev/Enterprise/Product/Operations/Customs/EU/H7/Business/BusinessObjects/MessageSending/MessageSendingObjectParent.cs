using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class MessageSendingObjectParent<TMessageSendingObject> : BaseMessageSendingObjectParent<TMessageSendingObject>
		where TMessageSendingObject : MessageSendingObject
	{
		public MessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader.Factory)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		readonly AsycudaManifestHeader manifestHeader;

		public override BusinessObject TopLevelBusinessObject => manifestHeader;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuH7;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			overrideAmendmentReasonReadOnly = true;
			overrideCancellationReasonReadOnly = true;
		}

		protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject bo)
		{
			base.HookMessageSendingObjectEvents(bo);
			if (bo is TMessageSendingObject messageSendingObject)
			{
				messageSendingObject.ActionInfo.ValueChanged -= OnSendingObjectActionChanged;
				messageSendingObject.ActionInfo.ValueChanged += OnSendingObjectActionChanged;
			}
		}

		[ResourceStringData("9a17b323-f92a-4a03-b0df-a2fe38ca9b49", Caption = "Message Type")]
		[List(nameof(ActionList))]
		[ReadOnlyMember(nameof(ActionList_ReadOnly))]
		public ZString Action
		{
			get => action;
			set
			{
				if (action != value)
				{
					action = value;
					SetMessageTypeOverrideForAllMessageSendingObjects(action);
				}
			}
		}
		ZString action;

		bool ActionList_ReadOnly => !OverrideDefaultAction;

		[ResourceStringData("f59f1779-1ad8-4b1c-b9cf-e1557011104b", Caption = "Override Message Type default values")]
		public ZBool OverrideDefaultAction
		{
			get => overrideDefaultAction;
			set
			{
				SetNonPersistentPropertyValue(OverrideDefaultActionInfo, ref overrideDefaultAction, value);
				if (!overrideDefaultAction)
				{
					Action = ZString.Empty;
				}
			}
		}
		ZBool overrideDefaultAction;

		public ZPropertyInfo OverrideDefaultActionInfo => GetZPropertyInfo(nameof(OverrideDefaultAction));

		public CodeDescriptionPairList ActionList => SendingObjectsCollection.FirstOrDefault() is MessageSendingObject messageSendingObject
			? messageSendingObject.ActionList
			: new CodeDescriptionPairList();

		void SetMessageTypeOverrideForAllMessageSendingObjects(ZString messageType)
		{
			foreach (var messageSendingObject in SendingObjectsCollection.Cast<TMessageSendingObject>())
			{
				messageSendingObject.Action = messageType.IsEmpty ? messageSendingObject.DefaultAction : messageType;
			}
		}

		protected override NonPersistentBusinessObjectCollection<TMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new MessageSendingObjectCollection<TMessageSendingObject>(Factory);
			foreach (AsycudaBill bill in manifestHeader.Bills)
			{
				if (!bill.HasBeenConvertedToStandaloneDeclaration)
				{
					result.Add(CreateNewMessageSendingObject(bill));
				}
			}

			return result;
		}

		[ResourceStringData("af2674a6-cbfe-4b07-96aa-ad0ac46afd16", Caption = "Amendment Reason")]
		[List(nameof(AmendmentReasonList))]
		public ZString AmendmentReason
		{
			get => amendmentReason;
			set
			{
				if (amendmentReason != value)
				{
					SetNonPersistentPropertyValue(AmendmentReasonInfo, ref amendmentReason, value);

					SendingObjectsCollection.Where(x => x.IsAmendmentAction).ForEach(x =>
					{
						x.AmendmentReasonCode = value;
					});
				}
			}
		}

		ZString amendmentReason;

		public ZPropertyInfo AmendmentReasonInfo => GetZPropertyInfo(nameof(AmendmentReason));

		protected virtual bool AmendmentReason_ReadOnly => OverrideAmendmentReason_ReadOnly || !OverrideAmendmentReason;

		[ResourceStringData("8ab223b4-512b-4942-a542-7e8d16e9ee78", Caption = "Override Amendment Reason default values")]
		public ZBool OverrideAmendmentReason
		{
			get => overrideAmendmentReason;
			set
			{
				if (overrideAmendmentReason != value)
				{
					SetNonPersistentPropertyValue(OverrideAmendmentReasonInfo, ref overrideAmendmentReason, value);

					if (!overrideAmendmentReason)
					{
						AmendmentReason = ZString.Empty;
					}
				}
			}
		}

		ZBool overrideAmendmentReason;

		public ZPropertyInfo OverrideAmendmentReasonInfo => GetZPropertyInfo(nameof(OverrideAmendmentReason));

		protected virtual bool OverrideAmendmentReason_ReadOnly
		{
			get => overrideAmendmentReasonReadOnly;
			set
			{
				if (overrideAmendmentReasonReadOnly != value)
				{
					overrideAmendmentReasonReadOnly = value;

					if (overrideAmendmentReasonReadOnly)
					{
						OverrideAmendmentReason = false;
					}

					OverrideAmendmentReasonInfo.RefreshBinding();
				}
			}
		}

		bool overrideAmendmentReasonReadOnly;

		public CodeDescriptionPairList AmendmentReasonList => SendingObjectsCollection.Cast<TMessageSendingObject>().FirstOrDefault(x => x.IsAmendmentAction)?.AmendmentReasonCodeList ?? new CodeDescriptionPairList();

		[ResourceStringData("d48e4135-4548-43ea-b70b-1ab3089dd984", Caption = "Cancellation Reason")]
		[List(nameof(CancellationReasonList))]
		public ZString CancellationReason
		{
			get => cancellationReason;
			set
			{
				if (cancellationReason != value)
				{
					SetNonPersistentPropertyValue(CancellationReasonInfo, ref cancellationReason, value);

					SendingObjectsCollection.Where(x => x.IsCancellationAction).ForEach(x =>
					{
						x.AmendmentReasonCode = value;
					});
				}
			}
		}

		ZString cancellationReason;

		public ZPropertyInfo CancellationReasonInfo => GetZPropertyInfo(nameof(CancellationReason));

		protected virtual bool CancellationReason_ReadOnly => OverrideCancellationReason_ReadOnly || !OverrideCancellationReason;

		[ResourceStringData("c0dea2a9-e450-4c91-b91d-a5053b48e4f6", Caption = "Override Cancellation Reason default values")]
		public ZBool OverrideCancellationReason
		{
			get => overrideCancellationReason;
			set
			{
				if (overrideCancellationReason != value)
				{
					SetNonPersistentPropertyValue(OverrideCancellationReasonInfo, ref overrideCancellationReason, value);

					if (!overrideCancellationReason)
					{
						CancellationReason = ZString.Empty;
					}
				}
			}
		}

		ZBool overrideCancellationReason;

		public ZPropertyInfo OverrideCancellationReasonInfo => GetZPropertyInfo(nameof(OverrideCancellationReason));

		protected virtual bool OverrideCancellationReason_ReadOnly
		{
			get => overrideCancellationReasonReadOnly;
			set
			{
				if (overrideCancellationReasonReadOnly != value)
				{
					overrideCancellationReasonReadOnly = value;

					if (overrideCancellationReasonReadOnly)
					{
						OverrideCancellationReason = false;
					}

					OverrideCancellationReasonInfo.RefreshBinding();
				}
			}
		}

		bool overrideCancellationReasonReadOnly;

		public CodeDescriptionPairList CancellationReasonList => SendingObjectsCollection.Cast<TMessageSendingObject>().FirstOrDefault(x => x.IsCancellationAction)?.AmendmentReasonCodeList ?? new CodeDescriptionPairList();

		TMessageSendingObject CreateNewMessageSendingObject(AsycudaBill bill)
		{
			return Activator.CreateInstance(typeof(TMessageSendingObject), bill) as TMessageSendingObject;
		}

		void OnSendingObjectActionChanged(object sender, EventArgs e)
		{
			var sendingObjects = SendingObjectsCollection.Cast<TMessageSendingObject>();
			OverrideAmendmentReason_ReadOnly = !sendingObjects.Any(x => x.IsAmendmentAction);
			OverrideCancellationReason_ReadOnly = !sendingObjects.Any(x => x.IsCancellationAction);
		}
	}
}
