using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.H7.Business
{
	public class DocumentSendingObject : SupportingDocSendingObject, ISupportingDocumentMessageDataProvider
	{
		public DocumentSendingObject(UploadDocumentsSendingAction parentSendingObject) : base(parentSendingObject?.Bill)
		{
			ParentSendingObject = parentSendingObject;
			bill = parentSendingObject.Bill as AsycudaBill;
			LocalReferenceNumber = (!bill.MovementReferenceNumber.IsEmpty ? bill.MovementReferenceNumber : bill.LocalReferenceNumber);
		}

		readonly AsycudaBill bill;

		public UploadDocumentsSendingAction ParentSendingObject { get; }

		[ResourceStringData("0b4a6625-ee89-40d6-8704-65809302d504", Caption = "Name")]
		public ZString FileName => Document?.FileName ?? ZString.Empty;

		public ZPropertyInfo FileNameInfo => GetZPropertyInfo(nameof(FileName));

		[ResourceStringData("bd0da0a8-a63d-432d-a264-3778710a0dce", Caption = "Document Type", FullDescription = "Type of the supporting document.", MediumCaption = "Doc. Type", ShortCaption = "Doc. T.")]
		public override ZString DocumentType
		{
			get => base.DocumentType;
			set => base.DocumentType = value;
		}

		[ResourceStringData("86cc555c-19ef-4501-9ad5-4ec9dccd1702", Caption = "File Size")]
		public ZDecimal FileSizeInKB => Document?.ImageData.Length ?? 0;

		public ZPropertyInfo FileSizeInKBInfo => GetZPropertyInfo(nameof(FileSizeInKB));

		[BusinessObjectTestExclude]
		public override ZBool ShouldSend { get => ParentSendingObject.ShouldSend; }

		public override ZGuid EDoc
		{
			get => base.EDoc;
			set
			{
				ZGuid zGuid = EDoc;
				base.EDoc = value;
				if (zGuid != EDoc)
				{
					DefaultFileDescription();
				}
			}
		}

		[ResourceStringData("019a9928-ff52-46ae-a945-45fb966b6703", Caption = "Description")]
		[BusinessObjectEmptyStringTestExclude]
		public ZString FileDescription
		{
			get => fileDescriptionCached;
			set
			{
				SetNonPersistentPropertyValue(FileDescriptionInfo, ref fileDescriptionCached, value);
				Validation.ValidateFileDescription();
			}
		}
		ZString fileDescriptionCached;

		public ZPropertyInfo FileDescriptionInfo => GetZPropertyInfo(nameof(FileDescription));

		void DefaultFileDescription()
		{
			var description = Document?.Description ?? ZString.Empty;
			if (!string.IsNullOrEmpty(description))
			{
				FileDescription = description;
			}
		}

		public new DocumentSendingObjectValidation Validation => (DocumentSendingObjectValidation)base.Validation;

		protected override SupportingDocSendingObjectValidation GetNewValidation() => new DocumentSendingObjectValidation(this);

		protected override IEnumerable<IStorageDocsBaseCollection> AllEDocsList => new[] { SupportingDocObject.DocManagerInfo?.EDocsView };

		public override SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder()
		{
			return new CDSH7SupportingDocUniversalEventBuilder(this); 
		}

		#region ISupportingDocumentMessageDataProvider

		EnterpriseBusinessObject ISupportingDocumentMessageDataProvider.BusinessObject => bill;

		DataContextType ISupportingDocumentMessageDataProvider.ContextType => DataContextType.AsycudaBill;

		ZString ISupportingDocumentMessageDataProvider.ContextReference => bill.ABL_BillNumber;

		ZString ISupportingDocumentMessageDataProvider.CountryCode => SupportingDocObject.CountryCode;

		CusEntryHeader ISupportingDocumentMessageDataProvider.Header => throw new System.NotImplementedException();

		ForwardingShipment ISupportingDocumentMessageDataProvider.Shipment => throw new System.NotImplementedException();

		#endregion
	}
}
