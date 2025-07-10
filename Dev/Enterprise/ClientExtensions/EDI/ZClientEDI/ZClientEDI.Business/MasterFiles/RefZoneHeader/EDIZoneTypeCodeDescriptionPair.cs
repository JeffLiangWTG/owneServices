using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIZoneTypeCodePairList : ZoneTypeCodePairList
	{
		public EDIZoneTypeCodePairList()
			: base()
		{
			Add(EDIZoneTypeCodeDescriptionPair.Training);
		}
	}

	public class EDIZoneTypeCodeDescriptionPair : ZoneTypeCodeDescriptionPair
	{
		protected EDIZoneTypeCodeDescriptionPair(object code, string description) : base(code, (NoResString)description) { }

		public static readonly ZoneTypeCodeDescriptionPair Training = new EDIZoneTypeCodeDescriptionPair(EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training, "Training Module Only");
	}
}

