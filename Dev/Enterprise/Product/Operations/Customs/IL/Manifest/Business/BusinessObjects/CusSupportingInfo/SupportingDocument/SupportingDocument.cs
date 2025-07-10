using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocument : IL.Business.SupportingDocument
		, IAdditionalBusinessObjectFetchStrategyProvider
		, ICusCodeDataTypeSupporter
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.CSI_AdditionalDescription", Caption = "Customs Notes")]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ReadOnly(true)]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		public new SupportingDocumentMetaDataCollection SupportingDocumentMetadataItems => (SupportingDocumentMetaDataCollection)base.SupportingDocumentMetadataItems;
		protected override IL.Business.SupportingDocumentMetaDataCollection GetNewSupportingDocumentMetaDataCollection() => new SupportingDocumentMetaDataCollection(this);

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

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

		protected override CodeDescriptionPairList GetSupportingDocumentMetaDataCodeListCore()
		{
			var supportingDocumentCode = CSI_Code;
			if (supportingDocumentCode.IsEmpty)
			{
				return new CodeDescriptionPairList();
			}

			var result =
				Factory.GetCachedValue(
					$"Enterprise.Customs.IL.Manifest.Business.SupportingDocument.GetSupportingDocumentMetaDataCodeListCore_{supportingDocumentCode}",
					() =>
					{
						var list = new CodeDescriptionPairList();
						var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Israel, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILDocumentType }, supportingDocumentCode, ZDateTime.Today).FirstOrDefault();
						var refCusCodeListAttributes = refCusCodeList?.Attributes.Where(x => !x.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MetadataMandatory));
						refCusCodeListAttributes?.ForEach(x => list.AddPair(x.ZZE_ZXE_NKName, x.CodeListAttributeName?.OriginalLanguageDescription ?? x.ZZE_ZXE_NKName));
						list.Sort();
						return list;
					});
			return result;
		}

		protected override IEnumerable<IStorageDocsBaseCollection> GetEDocCollectionsCore
		{
			get
			{
				var asycudaBill = (AsycudaBill)Parent;
				var asycudaManifestHeader = asycudaBill.Header;

				if (asycudaManifestHeader?.Consol is IDocManagerSupport consolSupport)
				{
					return EDocsHelper.GetEDocCollections(consolSupport);
				}

				if (asycudaManifestHeader is IDocManagerSupport support)
				{
					return EDocsHelper.GetEDocCollections(support);
				}

				return Array.Empty<IStorageDocsBaseCollection>();
			}
		}
	}
}
