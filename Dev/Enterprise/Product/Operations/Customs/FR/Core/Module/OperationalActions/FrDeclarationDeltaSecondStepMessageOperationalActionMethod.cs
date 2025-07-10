using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class FrDeclarationDeltaSecondStepMessageOperationalActionMethod : FrOperationActionMethod
	{
		public FrDeclarationDeltaSecondStepMessageOperationalActionMethod() : base(new ZGuid("C02031B0-6468-4FD4-A361-68D086DEC412"))
		{
		}

		public override string Name => Res.GetString("2DE7CEA6-CAD8-4AF7-BF0E-13A77D4DCBA2", "Send second step message(s) to Delta G2");

		public override string Description => Res.GetString("FEC7C025-736A-4EEF-981A-CF291C7A9027", "Send second step message(s) to Delta G2");

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FrDeclarationDeltaSecondStepMessageApplicator();
		}

		public override IComponent NewGuiControl()
		{
			throw new System.NotSupportedException();
		}

		public override bool HasControl => false;

		public override bool HasSettings => false;
	}
}
