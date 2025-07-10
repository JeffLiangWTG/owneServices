using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageSupportingDocumentValidation : SupportingDocumentValidation
	{
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string ruleBR_PN_TS_025Message = Res.GetString("3D823DCF-96DF-44DC-946F-F15277C39FC0", "The Supporting Document should be entered at Bill level or Bill Item level, not both.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		public TemporaryStorageSupportingDocumentValidation(TemporaryStorageSupportingDocument parent) : base(parent)
		{
		}
		protected new TemporaryStorageSupportingDocument Parent => (TemporaryStorageSupportingDocument)base.Parent;

		public override void ValidateAll()
		{
			var parent = Parent;

			parent.ClearRowNotifications();
			base.ValidateAll();

			CheckRuleBR_PN_TS_025(parent);
			ValidateMaxCount(parent);
		}

		static void ValidateMaxCount(TemporaryStorageSupportingDocument parent)
		{
			ISupportMaxCountValidation supportMaxCountValidation = null;
			switch (parent.Parent)
			{
				case TemporaryStorageBill bill:
					supportMaxCountValidation = bill.SupportingDocuments as ISupportMaxCountValidation;
					break;
				case TemporaryStoragePackedItem packedItem:
					supportMaxCountValidation = packedItem.SupportingDocuments as ISupportMaxCountValidation;
					break;
			}

			supportMaxCountValidation?.MaxCountValidator.Refresh();
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;

			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_CodeInfo);
			ListValidation.MessageErrorIfInvalidCode(parent.CSI_CodeInfo);
		}

		void CheckRuleBR_PN_TS_025(TemporaryStorageSupportingDocument parent)
		{
			if (parent.Parent is TemporaryStorageBill bill)
			{
				ValidateTemporaryStoragePackedItemsHasNoSupportingDocuments(parent, bill);
			}
			else if (parent.Parent is TemporaryStoragePackedItem packedItem)
			{
				ValidateTemporaryStorageBillHasNoSupportingDocuments(parent, packedItem);
			}
		}

		void ValidateTemporaryStoragePackedItemsHasNoSupportingDocuments(TemporaryStorageSupportingDocument additionalInfo, TemporaryStorageBill bill)
		{
			if (bill.PackedItems.Any(p => p.SupportingDocuments.Any()))
			{
				additionalInfo.AddRowMessageError(ruleBR_PN_TS_025Message);
			}
		}

		void ValidateTemporaryStorageBillHasNoSupportingDocuments(TemporaryStorageSupportingDocument additionalInfo, TemporaryStoragePackedItem packedItem)
		{
			if (packedItem.Bill.SupportingDocuments.Any())
			{
				additionalInfo.AddRowMessageError(ruleBR_PN_TS_025Message);
			}
		}
	}
}
