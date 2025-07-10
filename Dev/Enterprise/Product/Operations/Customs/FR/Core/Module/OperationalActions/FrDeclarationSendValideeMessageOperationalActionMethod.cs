using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class FrDeclarationSendValideeMessageOperationalActionMethod : FrOperationActionMethod
	{
		public FrDeclarationSendValideeMessageOperationalActionMethod() : base(new ZGuid("C02031B0-6468-4FD4-A361-68D086ADE414"))
		{
		}

		public override string Name => Res.GetString("0302DE4C-5676-4198-ACBC-8D1B0E2B3A2A", "Send validated");

		public override string Description => Res.GetString("AB6BC720-0366-4A95-B0A3-0DEAAD4F4AD2", "Send validated");

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FrDeclarationSendValideeMessageApplicator();
		}

		public override IComponent NewGuiControl() => new FrDeclarationSendValideeActionApplicatorControl();

		public override bool HasControl => true;

		public override bool HasSettings => false;
	}
}
