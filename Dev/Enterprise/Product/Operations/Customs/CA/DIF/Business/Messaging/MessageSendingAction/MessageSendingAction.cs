using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA.DIF;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class MessageSendingAction : AutoMessageSendingAction, IMessageSendingActionBase
	{
		public MessageSendingAction(DIFDocument difDocument)
			: base(difDocument.Factory)
		{
			this.difDocument = difDocument;
		}

		readonly DIFDocument difDocument;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoMessageSendingAction.Schema
		{
			public const string SendAmendment = "SendAmendment";
			public const string MessageType = "MessageType";
		}

		IDISDocumentBase IMessageSendingActionBase.DisDocument => difDocument;

		ZBool IMessageSendingActionBase.HasAnyDocumentsToSend => Send || SendWithdrawal || SendAmendment;

		public ZString DocumentDescription
		{
			get { return difDocument.EDoc != null ? difDocument.EDoc.FileName : ZString.Empty; }
		}

		public ZString Status
		{
			get { return difDocument.Status; }
		}

		public ZString StatusDescription
		{
			get { return difDocument.StatusDescription; }
		}

		public override ZBool Send
		{
			get { return base.Send; }
			set
			{
				base.Send = value;

				if (Send)
				{
					SendAmendment = false;
					SendWithdrawal = false;
				}
			}
		}

		[ReadOnlyMember(nameof(CannotSendAmendment))]
		public ZBool SendAmendment
		{
			get { return sendAmendment; }
			set
			{
				sendAmendment = value;
				if (sendAmendment)
				{
					Send = false;
					SendWithdrawal = false;
					SendAmendmentInfo.RefreshBinding();
				}
			}
		}
		ZBool sendAmendment;

		bool CannotSendAmendment
		{
			get { return !HasBeenLodgedAtCustoms && !HasResponseFromCBSA; }
		}

		public ZPropertyInfo SendAmendmentInfo
		{
			get { return GetZPropertyInfo(Schema.SendAmendment); }
		}

		[ReadOnlyMember(nameof(CannotSendWithdrawal))]
		public override ZBool SendWithdrawal
		{
			get { return base.SendWithdrawal; }
			set
			{
				base.SendWithdrawal = value;

				if (SendWithdrawal)
				{
					Send = false;
					SendAmendment = false;
				}
			}
		}

		bool CannotSendWithdrawal
		{
			get { return !HasBeenLodgedAtCustoms && !HasResponseFromCBSA; }
		}

		bool HasBeenLodgedAtCustoms
		{
			get { return StatusList.HasBeenLodgedAtCustoms(difDocument.Status); }
		}

		bool HasResponseFromCBSA
		{
			get { return StatusList.HasResponseFromCBSA(difDocument.Status); }
		}

		public ZString MessageType
		{
			get
			{
				if (SendWithdrawal)
				{
					return "Withdrawal";
				}
				else if (SendAmendment)
				{
					return "Amendment";
				}
				else if (HasBeenLodgedAtCustoms)
				{
					return "Change";
				}
				else
				{
					return "Add";
				}
			}
		}

		public ZPropertyInfo MessageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.MessageType); }
		}

		internal bool IsWaitingForResponse
		{
			get { return StatusList.IsWaitingForResponse(difDocument.Status); }
		}

		public ZString MessageSendingError
		{
			get { return difDocument.MessageSendingError; }
		}
	}
}
