using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInfoSendingObject : AutoAdditionalInfoSendingObject
	{
		public AdditionalInfoSendingObject(Declaration.CusEntryHeader entryHeader, CusEntryHeaderMessageSendingAction action, RequestedDocument document)
			: this(entryHeader.Factory, ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)entryHeader.Declaration).DataGroupingCode)
		{
			EntryHeader = entryHeader;
			Action = action;
			Document = document;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetNonPersistentObjectDefaultValues();
			}
		}

		public AdditionalInfoSendingObject(BusinessObjectFactory factory, string dataGroupgingCode)
			: base(factory)
		{
			DataGroupgingCode = Argument.NotNullOrEmpty(dataGroupgingCode, nameof(dataGroupgingCode));
		}
		public readonly string DataGroupgingCode;

		public virtual void ValidateAllAndRefreshBinding(object sender, EventArgs e)
		{
			Validation.ValidateAll();
			RefreshBinding();
		}

		public Declaration.CusEntryHeader EntryHeader { get; }

		public CusEntryHeaderMessageSendingAction Action { get; }

		public RequestedDocument Document { get; }

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoSendingObjectLookups.DocumentTypeList))]
		public override ZString DocumentType
		{
			get => base.DocumentType;
			set => base.DocumentType = value;
		}

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoSendingObjectLookups.CL010CountryCodes))]
		public override ZString CCQualifier
		{
			get => base.CCQualifier;
			set => base.CCQualifier = value;
		}

		DocumentSendingObjectCollection eDocsCollection;
		public DocumentSendingObjectCollection EDocsCollection
		{
			get
			{
				if (eDocsCollection == null)
				{
					eDocsCollection = new DocumentSendingObjectCollection(EntryHeader, this);
					RegisterEditableChildObject(eDocsCollection);
				}
				return eDocsCollection;
			}
		}

		public AdditionalInfoSendingObjectLookups Lookups => lookups ?? (lookups = new AdditionalInfoSendingObjectLookups(this));
		AdditionalInfoSendingObjectLookups lookups;

		void SetNonPersistentObjectDefaultValues()
		{
			DocumentType = Document?.CSI_Code ?? ZString.Empty;
			DocumentInformation = Document?.RequestInformation ?? ZString.Empty;
			ReferenceNumber = Document?.CSI_ReferenceNumber ?? ZString.Empty;
			CCQualifier = Document?.CSI_RN_NKCountryCode ?? ZString.Empty;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("1B861F3F-0161-4029-9FDC-F4AC9845AAA9", "Additional Info");
	}
}
