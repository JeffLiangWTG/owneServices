using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList NatSimplificationIndicator => Factory.GetCachedValue<NationalSimplificationIndicatorList>();

		public CodeDescriptionPairList NCTS5ArrivalTNNTypeList => Factory.GetCachedValue<ESNCTS5ArrivalTNNTypeList>();

		public CodeDescriptionPairList TADPrintProcedureList => Factory.GetCachedValue<TADPrintProcedureList>();

		public OrganisationsFindBoxCollection OrgAddressesList => new OrganisationsFindBoxCollection(Factory);

		public override CodeDescriptionPairList NctsTransitStatusList => Factory.GetCachedValue<NctsTransitStatusList>();

		public CodeDescriptionPairList CertificateNames
		{
			get
			{
				var broker = Parent.IsArrivalMovement ? Parent.ArrivalMovementHeader?.CusAgent : Parent.MovementHeader?.CusAgent;
				return CertificateHelper.CertificateNames(Factory, broker, GetType().Name);
			}
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;
	}
}
