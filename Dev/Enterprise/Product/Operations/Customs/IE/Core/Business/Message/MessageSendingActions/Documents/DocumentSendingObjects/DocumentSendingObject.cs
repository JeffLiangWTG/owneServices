using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentSendingObject : JobDeclarationSupportingDocSendingObject
	{
		public DocumentSendingObject(CusEntryHeader entryHeader, BusinessObject parentSendingObject) : base(entryHeader?.Declaration)
		{
			cusEntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			ParentSendingObject = parentSendingObject;
		}

		public virtual void ValidateAllAndRefreshBinding(object sender, EventArgs e)
		{
			Validation.ValidateAll();
			RefreshBinding();
		}

		public new DocumentSendingObjectValidation Validation => (DocumentSendingObjectValidation)base.Validation;

		protected override SupportingDocSendingObjectValidation GetNewValidation() => new DocumentSendingObjectValidation(this);

		readonly CusEntryHeader cusEntryHeader;

		public BusinessObject ParentSendingObject { get; }

		protected override Customs.Business.CusEntryHeader GetEntryHeader() => cusEntryHeader;

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

		#region FileName

		[ResourceStringData("E1E26F71-2738-45EA-845C-7A4B993F03DC", Caption = "Name")]
		public ZString FileName => Document?.FileName ?? ZString.Empty;

		public ZPropertyInfo FileNameInfo => GetZPropertyInfo(nameof(FileName));

		#endregion

		#region FileSizeInKB

		[ResourceStringData("3E63BA81-FF5F-4BDF-A73C-2275F8F3FC0C", Caption = "File Size")]
		public ZDecimal FileSizeInKB => Document?.ImageData.Length ?? 0;

		public ZPropertyInfo FileSizeInKBInfo => GetZPropertyInfo(nameof(FileSizeInKB));

		#endregion

		#region FileDescription

		[ResourceStringData("0AEE2A08-B0B3-4E3D-8224-305D5C36A677", Caption = "Description")]
		[Mandatory]
		[BusinessObjectEmptyStringTestExclude]
		public ZString FileDescription
		{
			get => fileDesctiptionCached;
			set
			{
				SetNonPersistentPropertyValue(FileDescriptionInfo, ref fileDesctiptionCached, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFileDescription();
				}
			}
		}
		ZString fileDesctiptionCached;

		public ZPropertyInfo FileDescriptionInfo => GetZPropertyInfo(nameof(FileDescription));

		void DefaultFileDescription()
		{
			var description = Document?.Description ?? ZString.Empty;
			if (!string.IsNullOrEmpty(description))
			{
				FileDescription = description;
			}
		}

		#endregion
	}
}
