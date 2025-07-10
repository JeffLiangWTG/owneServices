using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADRegion : ICADMessageRegion
{
	public CADRegion(ZString countryCode, ZString state)
	{
		this.countryCode = countryCode;
		this.state = state;
	}

	readonly ZString countryCode;
	readonly ZString state;

	#region ICADRegion

	string ICADMessageRegion.CountryCode => countryCode;

	string ICADMessageRegion.RegionID
	{
		get
		{
			var result = ZString.Empty;
			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				result = state;
			}
			return result;
		}
	}

	#endregion
}
