using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitConfigInfo : NonPersistentBusinessObject
	{
		public DocumentSplitConfigInfo(DocumentSplitManager manager, int pSourceDocumentPageCount)
		{
			SplitManager = manager;
			SourceDocumentPageCount = pSourceDocumentPageCount;
		}

		#region Schema

		public static class Schema
		{
			public const string DocumentName = "DocumentName";
			public const string DocumentType = "DocumentType";
			public const string DescriptionType = "DescriptionType";
			public const string StartPage = "StartPage";
			public const string EndPage = "EndPage";
			public const string ActualSizeInKb = "ActualSizeInKb";
			public const string DocumentData = "DocumentData";
			public const string SourceDocumentPageCount = "SourceDocumentPageCount";
			public const string AppendPageNumber = "AppendPageNumber";
			public const string IsPublished = "IsPublished";
		}

		#endregion

		#region Properties

		public ZString DocumentName
		{
			get { return documentName; }
			set
			{
				SetNonPersistentPropertyValue(DocumentNameInfo, ref documentName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDocumentName();
				}
				NeedsSplitting = true;
			}
		}

		ZString documentName;

		public ZPropertyInfo DocumentNameInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentName); }
		}

		public ZString DocumentType
		{
			get { return documentType; }
			set
			{
				SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value);
				if (!IsValidationSuspended)
				{
					UpdateDescriptionType();
					UpdateIsPublished();
					Validation.ValidateDocumentType();
				}
				NeedsSplitting = true;
			}
		}
		ZString documentType;

		void UpdateDescriptionType()
		{
			DescriptionType = DocumentType == RefDocTypes.MiscellaneousDocument ? SplitManager.DocumentToSplit.SC_DescMultilingual : DocType_List.GetMultilingualDescriptionFromCode(DocumentType);
		}

		void UpdateIsPublished()
		{
			IsPublished = DocType?.RT_IsPublished ?? SplitManager.DocumentToSplit.SC_IsPublished;
		}
#if DEBUG
		public
#else
		internal
#endif
		RefDocType DocType
		{
			get
			{
				var storage = SplitManager.DocumentToSplit.ParentMain;
				if (storage != null)
				{
					var docFactory = (DocumentFactory)storage.Factory;
					var query = new ZQuery(RefDocTypeSchema.RT_DocType, documentType)
						.AddToFilter(DocScanningHelper.GetDocTypeCategoryQuery(storage.SM_Type, docFactory));

					return docFactory.LoadTop1<RefDocType>(query);
				}

				return null;
			}
		}

		public ZPropertyInfo DocumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentType); }
		}

		public ZInt StartPage
		{
			get { return startPage; }
			set
			{
				SetNonPersistentPropertyValue(StartPageInfo, ref startPage, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartPage();
					Validation.ValidateEndPage();
					Validation.ValidateDocumentName();
				}
				NeedsSplitting = true;
			}
		}
		ZInt startPage;

		public ZPropertyInfo StartPageInfo
		{
			get { return GetZPropertyInfo(Schema.StartPage); }
		}

		public ZInt EndPage
		{
			get { return endPage; }
			set
			{
				SetNonPersistentPropertyValue(EndPageInfo, ref endPage, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartPage();
					Validation.ValidateEndPage();
					Validation.ValidateDocumentName();
				}
				NeedsSplitting = true;
			}
		}
		ZInt endPage;

		public ZPropertyInfo EndPageInfo
		{
			get { return GetZPropertyInfo(Schema.EndPage); }
		}

		public ZInt ActualSizeInKb
		{
			get { return actualSizeInKb; }
			set
			{
				SetNonPersistentPropertyValue(ActualSizeInKbInfo, ref actualSizeInKb, value);
			}
		}
		ZInt actualSizeInKb;

		public ZPropertyInfo ActualSizeInKbInfo
		{
			get { return GetZPropertyInfo(Schema.ActualSizeInKb); }
		}

		public ZBlob DocumentData
		{
			get { return documentData; }
			set
			{
				SetNonPersistentPropertyValue(DocumentDataInfo, ref documentData, value);
			}
		}
		ZBlob documentData;

		public ZPropertyInfo DocumentDataInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentData); }
		}

		public DocumentSplitManager SplitManager { get; }

		public CodeDescriptionPairList DocType_List
		{
			get { return SplitManager.DocumentToSplit.SC_DocType_List; }
		}

		[ReadOnlyMember(nameof(DescriptionType_ReadOnly))]
		public ZString DescriptionType
		{
			get { return descriptionType; }
			set { SetNonPersistentPropertyValue(DescriptionTypeInfo, ref descriptionType, value); }
		}
		ZString descriptionType;

		public ZPropertyInfo DescriptionTypeInfo => GetZPropertyInfo(Schema.DescriptionType);

		bool DescriptionType_ReadOnly => DocumentType != RefDocTypes.MiscellaneousDocument;

		public ZInt SourceDocumentPageCount { get; }

		public ZBool NeedsSplitting { get; set; }

		public ZBool AppendPageNumber
		{
			get
			{
				return appendPageNumber;
			}
			set
			{
				SetNonPersistentPropertyValue(AppendPageNumberInfo, ref appendPageNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDocumentName();
				}
				NeedsSplitting = true;
			}
		}
		ZBool appendPageNumber;

		public ZPropertyInfo AppendPageNumberInfo
		{
			get { return GetZPropertyInfo(Schema.AppendPageNumber); }
		}

		public ZBool IsPublished
		{
			get
			{
				return isPublished;
			}
			set
			{
				SetNonPersistentPropertyValue(IsPublishedInfo, ref isPublished, value);
				NeedsSplitting = true;
			}
		}
		ZBool isPublished;

		public ZPropertyInfo IsPublishedInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.IsPublished);
			}
		}

		public bool IsPublished_ReadOnly => DocType == null || !DocType.RT_IsPublishUpdatable;

#endregion

		#region Validation

		public DocumentSplitConfigInfoValidation Validation
		{
			get
			{
				return new DocumentSplitConfigInfoValidation(this);
			}
		}

		#endregion
	}
}
