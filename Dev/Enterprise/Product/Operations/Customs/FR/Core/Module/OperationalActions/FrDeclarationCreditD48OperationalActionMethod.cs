using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class FrDeclarationCreditD48OperationalActionMethod : FrOperationActionMethod
	{
		public FrDeclarationCreditD48OperationalActionMethod() : base(new ZGuid("7D26288A-FEA2-417F-A35F-2D61143518E1"))
		{
		}

		public override string Name => Res.GetString("7C8F7CE5-AE8E-46C6-9A7D-0197E2E7708F", "Credit D48");

		public override string Description => Res.GetString("{37169BEF-D535-4DD6-BED3-51420DDDB281}", "Credit D48");

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FrDeclarationCreditD48Applicator(factory);
		}

		public override IComponent NewGuiControl() => new FrDeclarationCreditD48ActionApplicatorControl();

		public override bool HasControl => true;

		public override bool HasSettings => false;
	}
}
