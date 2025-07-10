using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business
{
	public class RefundManager<T> : RefundManager
		where T : BusinessObject, IRefundEnquiry
	{
		public RefundManager(T owner)
			: base(owner)
		{
			Factory = owner.Factory;
		}
	}

	public abstract class RefundManager
	{
		protected RefundManager(IRefundEnquiry owner)
		{
			this.owner = owner;
		}
		protected IRefundEnquiry owner;

		#region Implementation

		public ClientRefund Refund
		{
			get
			{
				if (clientRefund == null)
				{
					var query = new ZQuery(OwnerFilter, RelatedOwnerFilter);
					query.IsNoLock = true;
					if ((clientRefund = Factory.LoadTop1<ClientRefund>(query)) == null)
					{
						if ((clientRefund = FindRefund(OwnerFilter, owner.RelatedOwner)) == null)
						{
							clientRefund = FindRefund(RelatedOwnerFilter, owner);
						}
					}
				}

				ZString controlNumber;
				if (clientRefund != null && clientRefund.T10_ControlNumber.IsEmpty && !(controlNumber = ControlNumer).IsEmpty)
				{
					clientRefund.T10_ControlNumber = controlNumber;
					clientRefund.Factory.Save();
				}
				return clientRefund;
			}
		}
		ClientRefund clientRefund;

		ZString ControlNumer
		{
			get
			{
				ZString result = ZString.Empty;
				if (owner is JobDeclaration)
				{
					result = GetControlNumberFromNotes(((JobDeclaration)owner).Notes);
				}
				return result;
			}
		}

		public void DeleteRefund()
		{
			if (owner.RelatedOwner == null && Refund != null)
			{
				Refund.Delete();
			}
		}

		ZQuery OwnerFilter
		{
			get { return ownerFilter ?? (ownerFilter = new ZQuery(owner.OwnerColumn, owner.PK)); }
		}
		ZQuery ownerFilter;

		ZQuery RelatedOwnerFilter
		{
			get { return relatedOwnerFilter ?? (relatedOwnerFilter = owner.RelatedOwner != null ? new ZQuery(owner.RelatedOwner.OwnerColumn, owner.RelatedOwner.PK) : null); }
		}
		ZQuery relatedOwnerFilter;

		public virtual ClientRefund CreateClientRefund()
		{
			ClientRefund refund = NewRefund();
			refund.T10_AmountRefundedToUPS = refund.ExtractRefundAmountFromCustoms(owner as JobDeclaration);
			refund.T10_RefundAmount = refund.T10_AmountRefundedToUPS;
			SetOwner(refund, owner);
			SetOwner(refund, owner.RelatedOwner);
			OnClientRefundCreated(refund);
			return refund;
		}

		public static ZString GetControlNumberFromNotes(Notes notes)
		{
			ZString result = ZString.Empty;

			StmNote[] refundNotes = notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description);

			if (refundNotes != null && refundNotes.Length > 0)
			{
				StmNote refundNote = refundNotes[0];

				ZString[] collection = refundNote.ST_NoteText.Trim(System.Environment.NewLine.ToCharArray()).Split(System.Environment.NewLine.ToCharArray());

				if (collection.Length > 0)
				{
					result = collection[0].SubstringSafe(collection[0].IndexOf(':') + 1, collection[0].Length);
				}
			}

			return result;
		}

		protected virtual ClientRefund NewRefund()
		{
			return Factory.New<ClientRefund>();
		}

		protected ClientRefund FindRefund(ZQuery filter, IRefundEnquiry newOwner)
		{
			ClientRefund refund = null;
			if (filter != null)
			{
				refund = Factory.LoadTop1<ClientRefund>(filter);
				SetOwner(refund, newOwner);
			}
			return refund;
		}

		void SetOwner(ClientRefund refund, IRefundEnquiry owner)
		{
			if (refund != null && owner != null)
			{
				typeof(ClientRefund).GetProperty(owner.OwnerColumn.Name).SetValue(refund, owner.PK, null);
			}
		}

		#endregion

		#region OnClientRefundCreated

		public delegate void ClientRefundCreated(ClientRefund refund);
		public event ClientRefundCreated OnRefundCreated;

		protected virtual void OnClientRefundCreated(ClientRefund refund)
		{
			if (OnRefundCreated != null)
			{
				OnRefundCreated(refund);
			}
		}

		#endregion

		protected BusinessObjectFactory Factory;
	}
}
