using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocument : CusSupportingInfo
		, ICusCodeDataTypeSupporter
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Proxied Properties

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.EDocList))]
		[ResourceStringData("Enterprise.Customs.IL.Business.EDoc", Caption = "eDoc")]
		public virtual ZGuid EDoc
		{
			get
			{
				LoadStorageDocPivotIfNeeded();
				return cusStorageDocPivot?.CSD_StorageDocReference ?? ZGuid.Empty;
			}
			set
			{
				if (EDoc != value)
				{
					if (GetDocument(value) is IeDoc currentDocument)
					{
						var filename = currentDocument.FileName;
						if (cusStorageDocPivot == null)
						{
							cusStorageDocPivot = Factory.New<BaseCusStorageDocPivot>();
							cusStorageDocPivot.CSD_ParentID = PK;
							cusStorageDocPivot.CSD_ParentTableCode = TablePrefix;
						}
						cusStorageDocPivot.CSD_StorageDocReference = value;
						cusStorageDocPivot.CSD_DocType = Path.GetExtension(filename).TrimStart('.').ToUpperInvariant();
						fDocument = currentDocument;
					}
					else
					{
						cusStorageDocPivot?.Delete();
						cusStorageDocPivot = null;
						fDocument = null;
					}

					EDocInfo.RefreshBinding();
				}
			}
		}
		BaseCusStorageDocPivot cusStorageDocPivot;
		bool isLoadedCusStorageDocPivot;

		public virtual ZPropertyInfo EDocInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(nameof(EDoc));
			}
		}

		public IeDoc Document
		{
			get
			{
				var docRef = EDoc;
				if (fDocument == null && docRef.IsValid)
				{
					fDocument = GetDocument(docRef);
				}
				return fDocument;
			}
		}
		IeDoc fDocument;

		public IStorageDocsBaseCollection[] EDocCollections()
			=> GetEDocCollectionsCore.ToArray() ?? Array.Empty<IStorageDocsBaseCollection>();
		protected virtual IEnumerable<IStorageDocsBaseCollection> GetEDocCollectionsCore
		{
			get
			{
				if (Parent is IDocManagerSupport support)
				{
					return EDocsHelper.GetEDocCollections(support);
				}

				return Array.Empty<IStorageDocsBaseCollection>();
			}
		}

		#endregion

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;

				if (!IsCopying && oldValue != CSI_Code)
				{
					UpdateSupportingDocumentMetaDatas();
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.IL.Business.SupportingDocument", Caption = "Customs Doc ID")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		public new SupportingDocumentLookups Lookups
			=> (SupportingDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups()
			=> new SupportingDocumentLookups(this);

		public new SupportingDocumentValidation Validation
			=> (SupportingDocumentValidation)base.Validation;
		protected override CusSupportingInfoValidation GetNewValidation()
			=> new SupportingDocumentValidation(this);

		public override void Delete()
		{
			RemoveCusStorageDocPivotIfNeeded();
			base.Delete();
		}

		[ChildEditable(true)]
		public SupportingDocumentMetaDataCollection SupportingDocumentMetadataItems
		{
			get
			{
				if (supportingDocumentMetadataItems == null)
				{
					supportingDocumentMetadataItems = GetNewSupportingDocumentMetaDataCollection();
					RegisterEditableChildObject(supportingDocumentMetadataItems);
					supportingDocumentMetadataItems.Load();
				}

				return supportingDocumentMetadataItems;
			}
		}
		SupportingDocumentMetaDataCollection supportingDocumentMetadataItems;

		protected virtual SupportingDocumentMetaDataCollection GetNewSupportingDocumentMetaDataCollection() => new SupportingDocumentMetaDataCollection(this);

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.SupportingDocument, typeof(SupportingDocumentMetaData) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		void RemoveCusStorageDocPivotIfNeeded()
		{
			LoadStorageDocPivotIfNeeded();
			cusStorageDocPivot?.Delete();
		}

		void LoadStorageDocPivotIfNeeded()
		{
			if (!isLoadedCusStorageDocPivot)
			{
				var zQuery = new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, PK);
				zQuery.AddToFilter(CusStorageDocPivotSchema.CSD_ParentTableCode, TablePrefix);
				cusStorageDocPivot = Factory.LoadTop1<BaseCusStorageDocPivot>(zQuery);
				isLoadedCusStorageDocPivot = true;
			}
		}

		IeDoc GetDocument(ZGuid docRef)
			=> docRef.IsValid
			? EDocCollections().Select(x => x.GetFromUniqueKey(docRef.ToGuid())).FirstOrDefault(x => x != null)
			: null;

		public CodeDescriptionPairList GetSupportingDocumentMetaDataCodeList() => GetSupportingDocumentMetaDataCodeListCore();

		protected virtual CodeDescriptionPairList GetSupportingDocumentMetaDataCodeListCore()
		{
			var supportingDocumentCode = CSI_Code;
			if (supportingDocumentCode.IsEmpty)
			{
				return new CodeDescriptionPairList();
			}

			var result =
				Factory.GetCachedValue(
					$"Enterprise.Customs.IL.Business.SupportingDocument.GetSupportingDocumentMetaDataCodeList_{supportingDocumentCode}",
					() =>
					{
						var list = new CodeDescriptionPairList();
						var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Israel, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILDocumentType }, supportingDocumentCode, ZDateTime.Today).FirstOrDefault();
						var refCusCodeListAttributes = refCusCodeList?.Attributes.Where(x => !x.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MetadataMandatory));
						refCusCodeListAttributes?.Cast<ZZRefCusCodeListAttributeCombined>().ForEach(x => list.AddPair(x.ZZE_ZXE_NKName, x.NameDescription));
						list.Sort();
						return list;
					});
			return result;
		}

		void UpdateSupportingDocumentMetaDatas()
		{
			var metaDataCodeList = this.GetSupportingDocumentMetaDataCodeList().GetAllCodesZString();

			var metasNotValid = SupportingDocumentMetadataItems.Where(x => !metaDataCodeList.Contains(x.CY_Code)).ToArray();
			foreach (var metaData in metasNotValid)
			{
				SupportingDocumentMetadataItems.RemoveAndDelete(metaData);
			}

			foreach (var metaData in metaDataCodeList)
			{
				var supportingDocumentMetaData = SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().FirstOrDefault(x => x.CY_Code == metaData) ?? SupportingDocumentMetadataItems.AddNew();
				supportingDocumentMetaData.CY_Code = metaData;
			}
		}
	}
}
