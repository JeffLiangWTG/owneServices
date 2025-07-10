using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.DocumentImaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class DocumentImageType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string UPSCode = "UPSCode";
			public const string DocTypeCode = "DocTypeCode";
			public const string Description = "Description";
			public const string MoveJobToClassOnImport = "MoveJobToClassOnImport";
			public const string NotifyOnImport = "NotifyOnImport";
		}

		#endregion

		public const string CommercialInvoiceUPSCode = "CI";

		#region Bound Properties

		#region UPSCode

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString UPSCode
		{
			get { return fUPSCode; }
			set
			{
				CheckMaximumLength(UPSCodeInfo, value);
				SetNonPersistentPropertyValue(UPSCodeInfo, ref fUPSCode, value);

				if (!IsValidationSuspended)
				{
					ValidateUPSCode();
				}
			}
		}
		ZString fUPSCode;

		public ZPropertyInfo UPSCodeInfo
		{
			get { return GetZPropertyInfo(Schema.UPSCode); }
		}

		void ValidateUPSCode()
		{
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(UPSCodeInfo);
			}
		}

		#endregion

		#region DocTypeCode

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString DocTypeCode
		{
			get { return fDocTypeCode; }
			set
			{
				CheckMaximumLength(DocTypeCodeInfo, value);
				SetNonPersistentPropertyValue(DocTypeCodeInfo, ref fDocTypeCode, value);

				if (!IsValidationSuspended)
				{
					ValidateDocTypeCode();
				}
			}
		}
		ZString fDocTypeCode;

		public ZPropertyInfo DocTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DocTypeCode); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		void ValidateDocTypeCode()
		{
			MandatoryValidation.CheckEntered(DocTypeCodeInfo);
			if (!DocTypeCode_List.ContainsCode(DocTypeCode))
			{
				DocTypeCodeInfo.AddError(
					"You must enter a valid CargoWise One document type code.\n" +
					"To add a new CargoWise One document type, go to 'Config->Reference Files->Document Type' and click 'New'.\n" +
					"The document type must exist either as a 'Common Document Type' reference type, or with a 'Declaration' reference type.");
			}
		}

		#endregion

		#region Description

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		void ValidateDescription()
		{
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		#endregion

		#region MoveJobToClassOnImport

		public ZBool MoveJobToClassOnImport
		{
			get { return fMoveJobToClassOnImport; }
			set
			{
				SetNonPersistentPropertyValue(MoveJobToClassOnImportInfo, ref fMoveJobToClassOnImport, value);
			}
		}
		ZBool fMoveJobToClassOnImport;

		public ZPropertyInfo MoveJobToClassOnImportInfo
		{
			get { return GetZPropertyInfo(Schema.MoveJobToClassOnImport); }
		}

		#endregion

		#region NotifyOnImport

		public ZBool NotifyOnImport
		{
			get { return fNotifyOnImport; }
			set
			{
				SetNonPersistentPropertyValue(NotifyOnImportInfo, ref fNotifyOnImport, value);
			}
		}
		ZBool fNotifyOnImport;

		public ZPropertyInfo NotifyOnImportInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyOnImport); }
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList DocTypeCode_List
		{
			get
			{
				if (fDocTypeCode_List == null)
				{
					fDocTypeCode_List = new CodeDescriptionPairList();

					RefDocTypeCollection collection = new RefDocTypeCollection(CurrentFactory, AllOrDeclarationDocTypeQuery);

					foreach (RefDocType type in collection)
					{
						if (!fDocTypeCode_List.ContainsCode(type.RT_DocType) &&
							(type.RT_ReferenceType == DocManagerReferenceTypes.All || type.RT_ReferenceType == DocManagerReferenceTypes.Declaration))
						{
							fDocTypeCode_List.AddPair(type.RT_DocType, type.RT_DescMultilingual);
						}
					}
				}
				return fDocTypeCode_List;
			}
		}
		CodeDescriptionPairList fDocTypeCode_List;

		ZQuery AllOrDeclarationDocTypeQuery
		{
			get
			{
				ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, DocManagerReferenceTypes.All);
				query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, DocManagerReferenceTypes.Declaration);
				return query;
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UPSCode, UPSCode);
			writer.WriteElementString(Schema.DocTypeCode, DocTypeCode);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.MoveJobToClassOnImport, XmlConvert.ToString(MoveJobToClassOnImport));
			writer.WriteElementString(Schema.NotifyOnImport, XmlConvert.ToString(NotifyOnImport));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UPSCode = reader.ReadElementString(Schema.UPSCode);
			DocTypeCode = reader.ReadElementString(Schema.DocTypeCode);
			Description = reader.ReadElementString(Schema.Description);
			MoveJobToClassOnImport = XmlConvert.ToBoolean(reader.ReadElementString(Schema.MoveJobToClassOnImport));
			NotifyOnImport = XmlConvert.ToBoolean(reader.ReadElementString(Schema.NotifyOnImport));

			RefreshBinding();
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentImageType();
		}

		#endregion
	}
}
