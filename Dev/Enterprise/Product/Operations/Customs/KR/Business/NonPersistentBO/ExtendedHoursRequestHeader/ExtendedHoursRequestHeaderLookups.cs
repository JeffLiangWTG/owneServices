using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestHeaderLookups : ZLookups
	{
		public ExtendedHoursRequestHeaderLookups(ExtendedHoursRequestHeader parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection CustomsDivisonList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, ZDateTime.Today);
		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<ElectronicDocumentTypeList>();
		public GlbBranchCollection Branches => new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, Parent.CompanyPK));

		protected new ExtendedHoursRequestHeader Parent => (ExtendedHoursRequestHeader)base.Parent;
	}
}
