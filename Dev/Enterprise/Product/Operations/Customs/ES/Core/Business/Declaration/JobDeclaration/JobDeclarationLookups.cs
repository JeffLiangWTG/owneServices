using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public CodeDescriptionPairList RepresentationTypeList => Factory.GetCachedValue<ESRepresentationTypeList>();

		protected override ICollection LocationsCore => LocationsHelper.GetESLocationsCusCodeList(Factory);

		protected override ZQuery FinalDestinationPortFilter()
		{
			if (Parent.IsExport && Parent.JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
			{
				return PortFilter(null, null);
			}
			else
			{
				return base.FinalDestinationPortFilter();
			}
		}

		protected override ZQuery DischargePortFilter()
		{
			if (Parent.IsExport && Parent.JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
			{
				return PortFilter(null, null);
			}
			else
			{
				return base.DischargePortFilter();
			}
		}

		public CodeDescriptionPairList CertificateNames => CertificateHelper.CertificateNames(Factory, Parent.CusAgent, GetType().Name);
	}
}
