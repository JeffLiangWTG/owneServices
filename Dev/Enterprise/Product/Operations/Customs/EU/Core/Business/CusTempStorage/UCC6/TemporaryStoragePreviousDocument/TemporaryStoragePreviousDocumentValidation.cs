using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocumentValidation : CusSupportingInfoValidation
	{
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string ruleBR_PN_TS_053Message = Res.GetString("A85F9698-0C51-41B5-B913-5DDFC7D205B4", "The Previous Document should be entered at Bill level or Bill Item level, not both.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		public TemporaryStoragePreviousDocumentValidation(TemporaryStoragePreviousDocument parent)
			: base(parent)
		{
		}

		protected new TemporaryStoragePreviousDocument Parent => (TemporaryStoragePreviousDocument)base.Parent;

		public override void ValidateAll()
		{
			var parent = Parent;

			parent.ClearRowNotifications();
			base.ValidateAll();

			CheckRuleBR_PN_TS_053(parent);
			ValidatePreviousDocumentWhenIsReused();
		}

		protected virtual void ValidatePreviousDocumentWhenIsReused()
		{
			var previousDocument = Parent;

			if (previousDocument.Parent is TemporaryStoragePackedItem item)
			{
				if ((item.Bill?.Header?.IsENSReuse ?? false) && item.PreviousDocuments.Any() && item.Bill.PackedItems.All(x => x.PreviousDocuments.SequenceEqual(item.PreviousDocuments, new TemporaryStoragePreviousDocumentComparer())))
				{
					previousDocument.AddRowMessageError(Res.GetString("E078FFD6-098C-48C0-8745-356B8C0256EC", "As Previous Document in Bill Items are identical, it must be sent at Bill level. Previous Document in Bill Item tab or Header should be empty."));
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			CheckCSI_ReferenceNumberWhenENSIsReused();
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CSI_CodeInfo);
			CheckCSI_CodeWhenENSIsReused();

			var message_Header_Bill = Res.GetString("C3B3AF5F-DD5C-4EB4-B97E-9EC152390657", "Previous document can only be provided at Header or Bills level, not at both.");
			var message_Header_Item = Res.GetString("E65A5202-7965-49CE-AF42-6CB16B265CE7", "Previous document can only be provided at Header or Bill Item level, not at both.");
			if (!parent.CSI_Code.IsEmpty)
			{
				if (parent.Parent is TemporaryStorageBill)
				{
					if (parent.TemporaryStorageHeader.PreviousDocuments.Any())
					{
						parent.CSI_CodeInfo.AddMessageError(message_Header_Bill);
					}
				}
				else if (parent.Parent is TemporaryStorageHeader header)
				{
					if (header.Bills.Cast<TemporaryStorageBill>().Any(x => x.PreviousDocuments.Any()))
					{
						parent.CSI_CodeInfo.AddMessageError(message_Header_Bill);
					}

					if (header.Bills.Cast<TemporaryStorageBill>().Any(x => x.PackedItems.Any(p => p.PreviousDocuments.Any())))
					{
						parent.CSI_CodeInfo.AddMessageError(message_Header_Item);
					}
				}
				else if (parent.Parent is TemporaryStoragePackedItem packedItem)
				{
					if (packedItem.Bill.Header.PreviousDocuments.Any())
					{
						parent.CSI_CodeInfo.AddMessageError(message_Header_Item);
					}
				}
			}
		}

		protected void CheckCSI_CodeWhenENSIsReused()
		{
			var parent = Parent;
			var header = parent.TemporaryStorageHeader;
			if ((header?.IsENSReuse ?? ZBool.False) && !parent.CSI_Code.EqualsIgnoringCase(PreviousDocumentCodeList.Codes.N355))
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("DC51373D-0C1E-4B88-A4B8-66E4B549B96E", "Previous document type must be '{0}'.", PreviousDocumentCodeList.Codes.N355));
			}
		}

		protected void CheckCSI_ReferenceNumberWhenENSIsReused()
		{
			var parent = Parent;
			var header = parent.TemporaryStorageHeader;
			if (header != null)
			{
				if (parent.Parent is TemporaryStorageBill && header.IsENSReuse)
				{
					foreach (TemporaryStorageBill bill in header.Bills)
					{
						var previousDocument = bill.PreviousDocuments.FirstOrDefault();
						if (previousDocument != null && previousDocument.CSI_Code.EqualsIgnoringCase(PreviousDocumentCodeList.Codes.N355)
							&& !parent.CSI_ReferenceNumber.EqualsIgnoringCase(previousDocument.CSI_ReferenceNumber))
						{
							parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("4283107A-DEF8-4380-BAC5-75873F5D2D6D", "Reference Number must be the same as that of other previous documents under the master."));
							break;
						}
					}
				}
			}
		}

		protected virtual void CheckRuleBR_PN_TS_053(TemporaryStoragePreviousDocument previousDocument)
		{
			if (previousDocument.Parent is TemporaryStorageBill bill)
			{
				ValidateTemporaryStoragePackedItemsHasNoPreviousDocuments(previousDocument, bill);
			}
			else if (previousDocument.Parent is TemporaryStoragePackedItem packedItem)
			{
				ValidateTemporaryStorageBillHasNoPreviousDocuments(previousDocument, packedItem);
			}
		}

		void ValidateTemporaryStoragePackedItemsHasNoPreviousDocuments(TemporaryStoragePreviousDocument previousDocument, TemporaryStorageBill bill)
		{
			if (bill.PackedItems.Any(p => p.PreviousDocuments.Any()))
			{
				previousDocument.AddRowMessageError(ruleBR_PN_TS_053Message);
			}
		}

		void ValidateTemporaryStorageBillHasNoPreviousDocuments(TemporaryStoragePreviousDocument previousDocument, TemporaryStoragePackedItem packedItem)
		{
			if (packedItem.Bill.PreviousDocuments.Any())
			{
				previousDocument.AddRowMessageError(ruleBR_PN_TS_053Message);
			}
		}
	}
}
