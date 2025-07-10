using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using MimeKit;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivot : BaseCusStorageDocPivot, IDocManagerSupportProvider, Integration.Customs.AU.ICusStorageDocPivot
	{
		public CusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Schema : BaseCusStorageDocPivot.Schema
#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public new const int CSD_DescriptionMaxLength = 200;
			public const string MessageStatusDescription = "MessageStatusDescription";
		}

		protected override Customs.Business.CusStorageDocPivotLookups GetNewLookups()
		{
			return new CusStorageDocPivotLookups(this);
		}

		public new CusStorageDocPivotLookups Lookups => (CusStorageDocPivotLookups)base.Lookups;

		protected override Customs.Business.CusStorageDocPivotValidation GetNewValidation()
		{
			if (Parent is QuarantineColsHeader colsHeader)
			{
				return new ColsCusStorageDocPivotValidation(this);
			}
			else
			{
				return new CusStorageDocPivotValidation(this);
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var result = base.ReadOnly;
				if (!result && Parent is QuarantineColsHeader colsHeader)
				{
					result = CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse ||
							 CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;
				}
				return result;
			}
			set => base.ReadOnly = value;
		}

		public override bool CanDelete => base.CanDelete && (CSD_MessageStatus.IsEmpty || !(Parent is QuarantineColsHeader));

		public override MultilingualString ReasonForNotAbleToDelete => !CSD_MessageStatus.IsEmpty ? (NoResString)"An attachment that has been sent cannot be deleted." : base.ReasonForNotAbleToDelete;

		#region CSD_DocType

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot|CSD_DocType", Caption = "Document Type")]
		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AttachmentTypeList))]
		public override ZString CSD_DocType
		{
			get => base.CSD_DocType;
			set => base.CSD_DocType = value;
		}

		public ZString DocTypeDescription => Lookups.AttachmentTypeList.GetDescriptionFromCode(CSD_DocType);

		#endregion

		#region CSD_StorageDocReference

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot|CSD_StorageDocReference", Caption = "eDoc")]
		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AvailableEDocs), "PK", "Code", AllowOnlyTheseValues = true)]
		public override ZGuid CSD_StorageDocReference
		{
			get => base.CSD_StorageDocReference;
			set => base.CSD_StorageDocReference = value;
		}

		public ZString MimeType
		{
			get
			{
				var doc = Document;

				return doc != null
					? (ZString)MimeTypes.GetMimeType(doc.FileName)
					: ZString.Empty;
			}
		}

		#endregion

		#region CSD_Description

		[MaxLength(Schema.CSD_DescriptionMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot|CSD_Description", Caption = "Description")]
		public override ZString CSD_Description
		{
			get => base.CSD_Description;
			set => base.CSD_Description = value;
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot|CSD_MessageStatus", Caption = "Status")]
		public override ZString CSD_MessageStatus
		{
			get => base.CSD_MessageStatus;
			set => base.CSD_MessageStatus = value;
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot|MessageStatusDescription", Caption = "Status Description")]
		public ZString MessageStatusDescription => Lookups.COLSDocumentStatusList?.GetDescriptionFromCode(CSD_MessageStatus) ?? ZString.Empty;

		#endregion

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();

				result.Add(ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceLine>());
				result.Add(ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceHeader>());
				result.Add(ObjectFactory.GetType<Integration.Customs.AU.IQuarantineColsHeader>());

				return result;
			}
		}

		#region IDocManagerSupportProvider

		IEnumerable<IDocManagerSupport> IDocManagerSupportProvider.DocManagerSupports
		{
			get
			{
				if (Parent is QuarantineColsHeader colsHeader)
				{
					yield return colsHeader.JobDeclaration;
				}
			}
		}

		#endregion
	}
}
