using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class SfdWizard : AutoSfdWizard
	{
		public SfdWizard(BusinessObjectFactory factory)
			: base(factory) { }

		[List(nameof(PackageTypes))]
		public override ZString PackageType
		{
			get { return base.PackageType; }
			set { base.PackageType = value; }
		}

		[List(nameof(Consignees))]
		public override ZGuid Consignee
		{
			get { return base.Consignee; }
			set { base.Consignee = value; }
		}

		public OrgHeaderCollection Consignees
		{
			get { return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsConsignee, true)); }
		}

		public CodeDescriptionPairList PackageTypes
		{
			get
			{
				return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, Enterprise.Customs.EU.Business.UniversalReferenceConstants.UNPackTypeStartDate);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPC = "06";
			MarksAndNumbers = "As addressed";
			PackageType = "PK";
		}
	}
}
