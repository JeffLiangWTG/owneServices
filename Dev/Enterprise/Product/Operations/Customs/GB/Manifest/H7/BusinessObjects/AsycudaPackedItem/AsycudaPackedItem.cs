using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.PackedItems))]
	public class AsycudaPackedItem : EU.H7.Business.AsycudaPackedItem, ICanBeImportOrExport, ITariffFormatProvider
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TariffList))]
		public override ZString API_Tariff
		{
			get => base.API_Tariff;
			set => base.API_Tariff = TariffFormatter.Format(value);
		}

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		ITariffFormatter TariffFormatter => tariffFormatter ??= EU.Business.TariffFormatter.New(CountryCode);
		ITariffFormatter tariffFormatter;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#region Properties

		[MaxLength(13)]
		[ResourceStringData("GBH7.AsycudaPackedItem.API_FormattedTariff", Caption = "Tariff", ShortCaption = "Tariff", MediumCaption = "Tariff", FullDescription = "Commodity Code (Combined Nomenclature Code) associated with the item.")]
		public override ZString API_FormattedTariff { get => base.API_FormattedTariff; set => base.API_FormattedTariff = value; }

		#endregion

		#region Additional Info

		public new EU.H7.Business.IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => (EU.H7.Business.IAdditionalInfoCollection<AdditionalInfo>)base.AdditionalInfos;

		protected override EU.H7.Business.IAdditionalInfoCollection<EU.H7.Business.AdditionalInfo> CreateAdditionalInfoCollection() => new EU.H7.Business.AdditionalInfoCollection<AdditionalInfo>(this);

		#endregion

		#region Supporting Documents
		public new EU.H7.Business.ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (EU.H7.Business.ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

		protected override EU.H7.Business.ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new EU.H7.Business.SupportingDocumentCollection<SupportingDocument>(this);

		#endregion

		#region Previous Documents

		public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		#endregion

		#region ICanBeImportOrExport 

		ZBool ICanBeImportOrExport.IsImport => Bill.IsImport;

		ZBool ICanBeImportOrExport.IsExport => Bill.IsExport;

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item;

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;

		string ICanBeImportOrExport.DataGroupingCode => Header.DataGrouping;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		#endregion

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[H7CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[H7CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[H7CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}
	}
}
