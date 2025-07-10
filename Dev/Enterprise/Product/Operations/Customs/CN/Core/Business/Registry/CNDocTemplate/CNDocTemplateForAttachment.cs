using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CN.Business.XmlSerializers")]
	public class CNDocTemplateForAttachment : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public CNDocTemplateForAttachment() : base()
		{
		}

		public CNDocTemplateForAttachment(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory ?? new BusinessObjectFactory { NameForDebugging = "CNCustomsRegistryDocTemplate" };

		protected abstract class Schema
		{
			public const string OrganizationPK = "OrganizationPK";
			public const string DataContext = "DataContext";
			public const string DocumentTemplate = "DocumentTemplate";
			public const string DocumentType = "DocumentType";
			public const string DocumentDescription = "DocumentDescription";
			public const string AttachmentType = "AttachmentType";

			public const int DataContextMaxLength = 35;
			public const string DataContextDefaultValue = ".CustomsDeclarationDocument";
			public const int DocumentTemplateMaxLength = 135;
			public const int DocumentDescriptionMaxLength = 128;
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CNDocTemplateForAttachment(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			OrganizationPK = ZGuid.TryParse(reader.ReadElementString(Schema.OrganizationPK), out var orgPKValue) ? orgPKValue : ZGuid.Empty;
			DataContext = reader.ReadElementString(Schema.DataContext);
			DocumentTemplate = reader.ReadElementString(Schema.DocumentTemplate);
			DocumentType = reader.ReadElementString(Schema.DocumentType);
			DocumentDescription = reader.ReadElementString(Schema.DocumentDescription);
			AttachmentType = reader.ReadElementString(Schema.AttachmentType);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrganizationPK, OrganizationPK.ToString());
			writer.WriteElementString(Schema.DataContext, DataContext);
			writer.WriteElementString(Schema.DocumentTemplate, DocumentTemplate);
			writer.WriteElementString(Schema.DocumentType, DocumentType);
			writer.WriteElementString(Schema.DocumentDescription, DocumentDescription);
			writer.WriteElementString(Schema.AttachmentType, AttachmentType);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DataContext = Schema.DataContextDefaultValue;
		}

		#endregion

		#region Properties

		#region OrganizationPK

		[List(nameof(Lookups) + "." + nameof(CNDocTemplateForAttachmentLookups.Organizations))]
		[RelatedBusinessObject("Organization")]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|OrganizationPK", Caption = "Organization")]
		public ZGuid OrganizationPK
		{
			get => organizationPK;
			set
			{
				SetNonPersistentPropertyValue(OrganizationPKInfo, ref organizationPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOrganizationPK();
				}
			}
		}
		ZGuid organizationPK;

		public ZPropertyInfo OrganizationPKInfo => GetZPropertyInfo(Schema.OrganizationPK);

		public OrgHeader Organization => CurrentFactory.Load<OrgHeader>(OrganizationPK);

		#endregion

		[Mandatory]
		[MaxLength(Schema.DataContextMaxLength)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|DataContext", Caption = "Data Context")]
		public ZString DataContext
		{
			get => fDataContext;
			set
			{
				if (DataContext != value)
				{
					SetNonPersistentPropertyValue(DataContextInfo, ref fDataContext, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDataContext();
					}
					DataContextInfo.RefreshBinding();
				}
			}
		}
		ZString fDataContext;

		public ZPropertyInfo DataContextInfo => GetZPropertyInfo(Schema.DataContext);

		[Mandatory]
		[MaxLength(Schema.DocumentTemplateMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CNDocTemplateForAttachmentLookups.DocumentTemplateList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|DocumentTemplate", Caption = "Document Template")]
		public ZString DocumentTemplate
		{
			get => fDocumentTemplate;
			set
			{
				if (DocumentTemplate != value)
				{
					SetNonPersistentPropertyValue(DocumentTemplateInfo, ref fDocumentTemplate, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentTemplate();
					}

					DocumentTemplateInfo.RefreshBinding();
				}
			}
		}
		ZString fDocumentTemplate;

		public ZPropertyInfo DocumentTemplateInfo => GetZPropertyInfo(Schema.DocumentTemplate);

		[Mandatory]
		[List(nameof(Lookups) + "." + nameof(CNDocTemplateForAttachmentLookups.DocumentTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|DocumentType", Caption = "Document Type")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		public ZString DocumentType
		{
			get => fDocumentType;
			set
			{
				if (DocumentType != value)
				{
					SetNonPersistentPropertyValue(DocumentTypeInfo, ref fDocumentType, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentType();
					}

					DocumentTypeInfo.RefreshBinding();

					if (DocumentDescription.IsEmpty)
					{
						DocumentDescription = Lookups.DocumentTypeList.FirstOrDefault(x => x.RT_DocType == DocumentType)?.RT_Desc ?? ZString.Empty;
					}
				}
			}
		}
		ZString fDocumentType;

		public ZPropertyInfo DocumentTypeInfo => GetZPropertyInfo(Schema.DocumentType);

		[Mandatory]
		[MaxLength(Schema.DocumentDescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|DocumentDescription", Caption = "Document Description")]
		public ZString DocumentDescription
		{
			get => fDocumentDescription;
			set
			{
				if (DocumentDescription != value)
				{
					SetNonPersistentPropertyValue(DocumentDescriptionInfo, ref fDocumentDescription, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentDescription();
					}
					DocumentDescriptionInfo.RefreshBinding();
				}
			}
		}
		ZString fDocumentDescription;

		public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);

		[Mandatory]
		[List(nameof(Lookups) + "." + nameof(CNDocTemplateForAttachmentLookups.AttachmentTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNDocTemplateForAttachment|AttachmentType", Caption = "Attachment Type")]
		public ZString AttachmentType
		{
			get => fAttachmentType;
			set
			{
				if (AttachmentType != value)
				{
					SetNonPersistentPropertyValue(AttachmentTypeInfo, ref fAttachmentType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAttachmentType();
					}

					AttachmentTypeInfo.RefreshBinding();
				}
			}
		}
		ZString fAttachmentType;

		public ZPropertyInfo AttachmentTypeInfo => GetZPropertyInfo(Schema.AttachmentType);

		#endregion

		#region Collection

		CNDocTemplateForAttachmentCollection fCollection;
		public CNDocTemplateForAttachmentCollection Collection => fCollection ?? (fCollection = (CNDocTemplateForAttachmentCollection)base.GetParentCollection(this, typeof(CNDocTemplateForAttachmentCollection)));

		#endregion

		#region Validation

		CNDocTemplateForAttachmentValidation fValidation;
		public CNDocTemplateForAttachmentValidation Validation => fValidation ?? (fValidation = new CNDocTemplateForAttachmentValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup

		CNDocTemplateForAttachmentLookups fLookups;
		public CNDocTemplateForAttachmentLookups Lookups => fLookups ?? (fLookups = new CNDocTemplateForAttachmentLookups(this));
		#endregion
	}
}
