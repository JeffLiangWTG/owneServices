
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Module;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIRefZoneHeaderFilterBusinessObject : InternationalZonesFilterBusinessObject
	{
		protected override CodeDescriptionPairList ZoneTypeList
		{
			get
			{
				return new EDIRefZoneHeaderLookups(null).ZoneTypes;
			}
		}
	}
}
