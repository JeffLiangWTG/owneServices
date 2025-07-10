using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.DropOff
{
	public class GbDeclarationDropOffMessageOperationalActionMethod : GbOperationalActionMethod
	{
		public GbDeclarationDropOffMessageOperationalActionMethod()
			: base(new ZGuid("12345678-79B4-4a6a-97D9-2D2EA76D5043")) { }

		public override string Name
		{
			get { return "Drop Off (DRP) Message"; }
		}

		public override string Description
		{
			get { return "GB Export Drop Off Message Operational Action"; }
		}

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GbDeclarationDropOffMessageActionMethodApplicator();
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new GbDeclarationDropOffMessageActionApplicatorControl();
		}

		public override bool HasSettings
		{
			get { return false; }
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			FilterRequirementList result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new string[]
				{
						Constants.CountryCodes.UnitedKingdom
				});

			return result;
		}
	}
}
