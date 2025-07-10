using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentWrapper : NonPersistentBusinessObject
	{
		public SupportingDocumentWrapper(SupportingDocument supportingDocument)
		{
			SupportingDocument = supportingDocument;
		}

		public SupportingDocument SupportingDocument { get; }

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|IsSelected", Caption = "Is Selected")]
		public ZBool IsSelected
		{
			get => isSelected;
			set
			{
				SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsSelected();
				}
				IsSelectedInfo.RefreshBinding();
			}
		}
		ZBool isSelected;
		public ZPropertyInfo IsSelectedInfo => GetZPropertyInfo(nameof(IsSelected));

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|Type", Caption = "Type")]
		public ZString Type => SupportingDocument.CSI_Code;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|Description", Caption = "Description")]
		public ZString Description => SupportingDocument.CSI_ReferenceNumber;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|Status", Caption = "Status")]
		public ZString Status => SupportingDocument.CSI_Status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|CustomsNotes", Caption = "Customs Notes")]
		public ZString CustomsNotes => SupportingDocument.CSI_AdditionalDescription;

		public ZPropertyInfo CustomsNotesInfo => GetZPropertyInfo(nameof(CustomsNotes));

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.EDocList))]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.SupportingDocumentWrapper|EDoc", Caption = "eDoc")]
		public ZGuid EDoc => SupportingDocument.EDoc;

		public ZPropertyInfo EDocInfo => GetZPropertyInfo(nameof(EDoc));

		public SupportingDocumentWrapperValidation Validation => new SupportingDocumentWrapperValidation(this);

		public SupportingDocumentLookups Lookups => new SupportingDocumentLookups(SupportingDocument);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}
	}
}
