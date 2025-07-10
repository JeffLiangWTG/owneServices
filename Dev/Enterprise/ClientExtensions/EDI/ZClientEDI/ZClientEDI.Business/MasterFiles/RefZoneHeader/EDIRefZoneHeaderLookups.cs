using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIRefZoneHeaderLookups : RefZoneHeaderLookups
	{
		public EDIRefZoneHeaderLookups(EDIRefZoneHeader parent) : base(parent) { }

		protected override ZoneTypeCodePairList GetZoneTypes()
		{
			return new EDIZoneTypeCodePairList();
		}

		public static class EDIZoneTypeCodes
		{
			public const string Training = "TRN";
		}
	}
}

