using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public override BusinessObjectCollection Tariffs => Parent.IsHTS ? new CACClassCollection(Factory) : new CACExportTariffCollection(Factory);

		public override CodeDescriptionPairList ClassificationTypes => Factory.GetCachedValue<ClassificationTypeList>();

		public new IBaseClassificationCollection<CusClassification> ClassificationList =>
			Parent.IsHTS
				? new HTSClassificationCollection(Factory)
				: new ExportClassificationCollection(Factory);

		public OrgHeaderCollection Manufacturers => new OrgHeaderCollection(Factory);

		public CACusRulingFindBoxCollection AuthorityNumberList
		{
			get
			{
				var owners = Parent.Part?.RelatedOrganisations?.BuyerRelations;
				return new CACusRulingFindBoxCollection(Factory, Parent.CI_CC_CA_AuthorityNumber, owners?.FirstOrDefault()?.Organisation, owners?.Select(o => o.OU_OH));
			}
		}

		public RefCountryCollection DefaultOrigins => new RefCountryCollection(Factory);

		public CodeDescriptionPairList StatesOfExport => Factory.GetCachedValue<USStatesList>();
	}
}
