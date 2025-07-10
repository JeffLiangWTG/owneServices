using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.PackedItems))]
	public class AsycudaPackedItem : EU.H7.Business.AsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnly(true)]
		public override ZString API_GrossWeightUQ { get => base.API_GrossWeightUQ; set => base.API_GrossWeightUQ = value; }

		[ReadOnly(true)]
		public override ZString API_NetWeightUQ { get => base.API_NetWeightUQ; set => base.API_NetWeightUQ = value; }

		[MaxLength(16)]
		public override ZString API_FormattedTariff { get => base.API_FormattedTariff; set => base.API_FormattedTariff = value; }

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		public ValidationMessage ValidationMessage => Bill.Header.ValidationConfiguration.ValidationMessage;

		public new IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments => (IAdditionalDocumentCollection<AdditionalDocument>)base.AdditionalDocuments;

		public new ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

		public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override IAdditionalDocumentCollection<EU.H7.Business.AdditionalDocument> CreateNewAdditionalDocumentCollection() => new AdditionalDocumentCollection<AdditionalDocument>(this);

		protected override ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

		protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		public bool BillHasTransportDocument => Bill.AdditionalDocuments.Any(document => document.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);

		public bool AdditionalDocumentsContainTransportDocument => AdditionalDocuments.Any(document => document.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);

		#region ICusSupportingInfoTypeSupporter

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[H7CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[H7CusSupportingInfoTypeList.Codes.AdditionalDocument] = typeof(AdditionalDocument);
			result[H7CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[H7CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		#endregion
	}
}
