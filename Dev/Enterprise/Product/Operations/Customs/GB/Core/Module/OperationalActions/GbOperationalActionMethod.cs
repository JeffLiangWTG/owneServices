using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public abstract class GbOperationalActionMethod : OperationalActionMethod
	{
		public GbOperationalActionMethod(ZGuid guid)
		: base(guid)
		{ }

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
