using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsArrivalCargoDesc : EU.NCTS.Business.NctsArrivalCargoDesc
		, Integration.Customs.DE.IArrivalCargoDesc
	{
		public NctsArrivalCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
		protected override bool ShouldSetSupportingDocumentsReadOnly => true;

		public new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
		protected override bool ShouldSetAdditionalInfosReadOnly => true;

		public new NctsPackageCollection Packages => (NctsPackageCollection)base.Packages;

		protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection(this);

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);
		protected override bool IsUnloadedCommodityCodeRequiredCore => !BY_HarmonisedTariff.IsEmpty;
	}
}
