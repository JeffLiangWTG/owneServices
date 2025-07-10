using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public class GbDeclarationOperationalActionMethod : GbOperationalActionMethod
	{
		public GbDeclarationOperationalActionMethod()
			: base(new ZGuid("67FFEC00-79B4-4a6a-97D9-2D2EA76D5043")) { }

		public override string Name
		{
			get { return "GB MUCR functions operational action"; }
		}

		public override string Description
		{
			get { return "MUCR management functions (GB)"; }
		}

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GbDeclarationActionMethodApplicator();
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new GbDeclarationActionApplicatorControl();
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
