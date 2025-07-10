using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.EU.H7.Business
{
	public class DocumentSendingObject : SupportingDocSendingObject
	{
		public DocumentSendingObject(AdditionalInfoSendingObject parentSendingObject) : base(parentSendingObject?.Bill)
		{
			ParentSendingObject = parentSendingObject;
		}

		public BusinessObject ParentSendingObject { get; }

		public ZPropertyInfo FileDescriptionInfo => GetZPropertyInfo(nameof(FileDescription));

		[ResourceStringData("2225BF55-9773-4208-9358-610F1C26882F", Caption = "Name")]
		public ZString FileName => Document?.FileName ?? ZString.Empty;
		public ZPropertyInfo FileNameInfo => GetZPropertyInfo(nameof(FileName));

		[ResourceStringData("11E2E532-6EC6-406B-8327-4E83E78D63A5", Caption = "File Size")]
		public ZDecimal FileSizeInKB => Document?.ImageData.Length ?? 0;
		public ZPropertyInfo FileSizeInKBInfo => GetZPropertyInfo(nameof(FileSizeInKB));

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

		[ResourceStringData("E29BBF61-745B-4B7E-B374-D1A6B5EE42C6", Caption = "Description")]
		[Mandatory]
		[BusinessObjectEmptyStringTestExclude]
		public ZString FileDescription
		{
			get => fileDesctiptionCached;
			set
			{
				SetNonPersistentPropertyValue(FileDescriptionInfo, ref fileDesctiptionCached, value);
			}
		}
		ZString fileDesctiptionCached;

		void DefaultFileDescription()
		{
			var description = Document?.Description ?? ZString.Empty;
			if (!string.IsNullOrEmpty(description))
			{
				FileDescription = description;
			}
		}

		protected override SupportingDocSendingObjectValidation GetNewValidation()
		{
			return new DocumentSendingObjectValidation(this);
		}

		protected override IEnumerable<IStorageDocsBaseCollection> AllEDocsList => new[] { SupportingDocObject.DocManagerInfo?.EDocsView };
	}
}
