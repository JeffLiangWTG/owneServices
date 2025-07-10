using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPERefLocoMapLookups : RefLocoMapLookups
	{
		public UPERefLocoMapLookups(AutoRefLocoMap parent)
			: base(parent)
		{
		}

		new UPERefLocoMap Parent
		{
			get { return (UPERefLocoMap)base.Parent; }
		}

		public override CodeDescriptionPairList RY_SystemUsage_List
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (Parent.Country != null)
				{
					switch (Parent.Country.Code)
					{
						case Core.Constants.CountryCodes.Australia:
						case Core.Constants.CountryCodes.UnitedKingdom:
							result = new UPEAirSeaMailSystemUsageList();
							break;

						case Core.Constants.CountryCodes.UnitedStates:
							result = new UPEUSLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Iceland:
							result = new UPEISLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Singapore:
							result = new UPESGLocoMapSystemUsageList();
							break;

						default:
							result = new UPEOtherLocoMapSystemUsageList();
							break;
					}
				}
				else
				{
					result = new UPEOtherLocoMapSystemUsageList();
				}

				return result;
			}
		}
	}
}
