using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class FrDeclarationSendPrelodgeAmendmentOperationalActionMethod : FrOperationActionMethod
	{
		public FrDeclarationSendPrelodgeAmendmentOperationalActionMethod() : base(new ZGuid("4050C582-14D5-4BC5-9B03-DAB328A9A1A0"))
		{
		}

		public override string Name => Res.GetString("51390E4D-D7E5-46A3-A908-76665842EE92", "Send pre-lodge amendment");

		public override string Description => Res.GetString("FCD93D0D-ADD7-4CDE-8D1C-010A2D80707D", "Send pre-lodge amendment");

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FrDeclarationSendPrelodgeAmendmentMessageApplicator();
		}

		public override IComponent NewGuiControl() => new FrDeclarationSendPrelodgeAmendmentActionApplicatorControl();

		public override bool HasControl => true;

		public override bool HasSettings => false;
	}
}
