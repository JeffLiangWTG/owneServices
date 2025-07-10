using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionHeaderLookups : ZLookups
	{
		public FinalPriceReportByDateExtensionHeaderLookups(FinalPriceReportByDateExtensionHeader parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CargoWise.Types.ZDateTime.Today);
		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<ElectronicDocumentTypeList>();
		public GlbBranchCollection Branches => new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));

		protected new FinalPriceReportByDateExtensionHeader Parent => (FinalPriceReportByDateExtensionHeader)base.Parent;
	}
}
