using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business.BusinessObjects.MessageSending;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalInfoSendingObject : AutoAdditionalInfoSendingObject, IDataGroupingProvider
	{
		public AdditionalInfoSendingObject(AsycudaBill bill, UploadDocumentsSendingAction action, RequestedDocument document) : base(bill.Factory)
		{
			Bill = bill;
			Action = action;
			Document = document;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetNonPersistentObjectDefaultValues();
			}
		}

		public AsycudaBill Bill { get; }
		public RequestedDocument Document { get; }
		public UploadDocumentsSendingAction Action { get; }

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoSendingObjectLookups.DocumentTypeList))]
		public override ZString DocumentType
		{
			get => base.DocumentType;
			set => base.DocumentType = value;
		}

		DocumentSendingObjectCollection eDocsCollection;

		public DocumentSendingObjectCollection EDocsCollection
		{
			get
			{
				if (eDocsCollection == null)
				{
					eDocsCollection = new DocumentSendingObjectCollection(Bill, this);
					RegisterEditableChildObject(eDocsCollection);
					RegisterEditableChildObject(eDocsCollection);
				}
				return eDocsCollection;
			}
		}

		public AdditionalInfoSendingObjectLookups Lookups => lookups ?? (lookups = new AdditionalInfoSendingObjectLookups(this));

		public ZString DataGrouping => ((IDataGroupingProvider)Bill).DataGrouping;

		AdditionalInfoSendingObjectLookups lookups;

		void SetNonPersistentObjectDefaultValues()
		{
			DocumentType = Document?.CSI_Code ?? ZString.Empty;
			DocumentInformation = Document?.RequestInformation ?? ZString.Empty;
			ReferenceNumber = Document?.CSI_ReferenceNumber ?? ZString.Empty;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0EA38350-4080-4B8A-8BF4-F32D0E8B156C", "Additional Info");
	}
}
