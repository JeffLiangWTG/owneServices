using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	internal sealed class RatingEntryTransportMode : ReadOnlyCodeDescriptionPairList
	{
		public RatingEntryTransportMode()
		{
			Elements.Add(new CodeDescriptionPair(Constants.TransportModes.Sea, ResString.GetMultilingualString("72e10853-1de6-4504-92f7-2a6a06c5f0d5", "Sea")));
			Elements.Add(new CodeDescriptionPair(Constants.TransportModes.Air, ResString.GetMultilingualString("5fa2ee6e-dbe3-4c0d-895b-e80de2d79691", "Air")));
			Elements.Add(new CodeDescriptionPair(Constants.TransportModes.Road, ResString.GetMultilingualString("31f1d852-ed10-46d1-8813-cedb0147b8e2", "Road")));
			Elements.Add(new CodeDescriptionPair(Constants.TransportModes.Rail, ResString.GetMultilingualString("0c5b3cd0-fb07-4f7e-8b98-ac05e7421d2a", "Rail")));
		}
	}
}
