using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentsAllowedForSigningItem : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string DocumentName = "DocumentName";
			public const string DocumentType = "DocumentType";
		}

		#endregion

		public DocumentsAllowedForSigningItem()
		{
		}

		public DocumentsAllowedForSigningItem(ZString documentName, ZString documentType)
		{
			this.documentName = documentName;
			this.documentType = documentType;
		}

		#region Properties

		public ZString DocumentName
		{
			get { return documentName; }
			set
			{
				SetNonPersistentPropertyValue(DocumentNameInfo, ref documentName, value);
				ValidateDocumentName();
				DocumentNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DocumentNameInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentName); }
		}
		ZString documentName;

		[List("DocumentTypePairList")]
		public ZString DocumentType
		{
			get { return documentType; }
			set
			{
				SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value);
				ValidateDocumentType();
				DocumentTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DocumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentType); }
		}
		ZString documentType;

		#endregion

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			if (!IsValidationSuspended)
			{
				ValidateDocumentName();
				ValidateDocumentType();
			}
			base.RunPreSaveValidationCore();
		}

		public void ValidateDocumentName()
		{
			DocumentNameInfo.ClearAllNotifications();
			ValidateNotEmpty();

			if (ParentCollection != null && ParentCollection.OfType<DocumentsAllowedForSigningItem>().Any(x => x != this && x.DocumentName == DocumentName))
			{
				DocumentNameInfo.AddError(Res.GetString("CC591F1E-B569-4781-9705-351DA07F1A17", "This document name already exists."));
			}
		}

		public void ValidateDocumentType()
		{
			DocumentTypeInfo.ClearAllNotifications();
			ValidateNotEmpty();
			ListValidation.ErrorIfInvalidCode(DocumentTypeInfo, DocumentTypePairList);
		}

		void ValidateNotEmpty()
		{
			if (DocumentName.IsEmpty)
			{
				DocumentNameInfo.AddError(Res.GetString("189368A0-FF3D-4D93-840A-8DEC570F95AF", "Please enter a valid document name."));
			}

			if (DocumentType.IsEmpty)
			{
				DocumentTypeInfo.AddError(Res.GetString("90F5B999-1F17-43D0-B2E1-A57F92D73B63", "Please enter a valid document type."));
			}
		}

		#endregion

		#region ParentCollection

		DocumentsAllowedForSigningItemCollection ParentCollection => GetParentCollection(this, typeof(DocumentsAllowedForSigningItemCollection)) as DocumentsAllowedForSigningItemCollection;

		#endregion

		#region Lookups

		CodeDescriptionPairList documentTypePairList;
		public CodeDescriptionPairList DocumentTypePairList
		{
			get
			{
				if (documentTypePairList == null)
				{
					documentTypePairList = new CodeDescriptionPairList();
					documentTypePairList.AddPair(DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes);
					documentTypePairList.AddPair(DocumentSigningRegistryConstants.DocumentAllowedForSigningType.No);
					documentTypePairList.AddPair(DocumentSigningRegistryConstants.DocumentAllowedForSigningType.All);
				}
				return documentTypePairList;
			}
		}

		#endregion

		#region Override Implements

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentsAllowedForSigningItem();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DocumentName = reader.ReadElementString(Schema.DocumentName);
			DocumentType = reader.ReadElementString(Schema.DocumentType);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DocumentName, DocumentName);
			writer.WriteElementString(Schema.DocumentType, DocumentType);
		}

		#endregion
	}
}
