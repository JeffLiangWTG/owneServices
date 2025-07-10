using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyUnitLookups
	{
		public CodeDescriptionPairList VolumeUnits
		{
			get
			{
				if (volumeUnits == null)
				{
					volumeUnits = new CodeDescriptionPairList();
					volumeUnits.AddPair(Constants.Volume.CubicCentimeters);
					volumeUnits.AddPair(Constants.Volume.CubicMetres);
					volumeUnits.AddPair(Constants.Volume.CubicInches);
				}

				return volumeUnits;
			}
		}

		CodeDescriptionPairList volumeUnits;
	}
}
