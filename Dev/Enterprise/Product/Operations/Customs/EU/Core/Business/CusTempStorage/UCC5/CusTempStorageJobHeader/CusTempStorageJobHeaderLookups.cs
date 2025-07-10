using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderLookups : AutoCusTempStorageJobHeaderLookups
	{
		public CusTempStorageJobHeaderLookups(AutoCusTempStorageJobHeader parent)
			: base(parent)
		{
		}

		protected new CusTempStorageJobHeader Parent => (CusTempStorageJobHeader)base.Parent;

		public virtual CodeDescriptionPairList TransportModeList => Factory.GetCachedValue<TransportTypeList>();

		public virtual CodeDescriptionPairList TransportMeansList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList PreviousReferenceTypeList => new CodeDescriptionPairList();

		public virtual CustomsOfficeCodeCollection CustomsOfficeList => new CustomsOfficeCodeCollection(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

		public virtual CustomsOfficeCodeCollection EntryCustomsOfficeList => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory);

		public virtual OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);

		public virtual RefVesselCollection RefVesselList => new RefVesselCollection(Factory);
	}
}
