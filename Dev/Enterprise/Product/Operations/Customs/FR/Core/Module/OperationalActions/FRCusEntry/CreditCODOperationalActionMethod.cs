using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class CreditCODOperationalActionMethod : FrOperationActionMethod
	{
		public CreditCODOperationalActionMethod() : base(new ZGuid("E557377C-B46C-4746-89F4-F92A9ABBB348"))
		{
		}

		public override string Name => Res.GetString("063CF113-E14F-400B-AAEB-85EFE17F0A9E", "Credit COD");

		public override string Description => Res.GetString("4060571B-B103-45CE-8F97-52AA5E551979", "Credit COD");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FrCreditCODApplicator(factory);
		}

		public override IComponent NewGuiControl()
		{
			return new CreditCODOperationalApplicatorControl();
		}

		public override bool HasControl => true;
	}
}
