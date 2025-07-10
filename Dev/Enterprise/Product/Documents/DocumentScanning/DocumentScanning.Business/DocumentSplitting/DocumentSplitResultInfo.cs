using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitResultInfo : NonPersistentBusinessObject
	{
		public DocumentSplitResultInfo(DocumentSplitManager manager)
		{
			splitManager = manager;
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
				UpdateDescription();
			}
		}
		ZString documentType;

		public void UpdateDescription()
		{
			DescriptionType = DocumentType == RefDocTypes.MiscellaneousDocument ? splitManager.DocumentToSplit.SC_DescMultilingual : DocType_List.GetMultilingualDescriptionFromCode(DocumentType);
		}

		public ZString DescriptionType
		{
			get { return descriptionType; }
			set { SetNonPersistentPropertyValue(DescriptionTypeInfo, ref descriptionType, value); }
		}
		ZString descriptionType;

		public ZPropertyInfo DescriptionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DescriptionType); }
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

		public ZBool IsPublished
		{
			get { return isPublished; }
			set
			{
				SetNonPersistentPropertyValue(IsPublishedInfo, ref isPublished, value);
			}
		}
		ZBool isPublished;

		public ZPropertyInfo IsPublishedInfo
		{
			get { return GetZPropertyInfo(Schema.IsPublished); }
		}

		public DocumentSplitManager SplitManager
		{
			get
			{
				return splitManager;
			}
		}
		readonly DocumentSplitManager splitManager;

		public CodeDescriptionPairList DocType_List
		{
			get { return SplitManager.DocumentToSplit.SC_DocType_List; }
		}

		#endregion

	}
}
