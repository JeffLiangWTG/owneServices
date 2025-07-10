using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIRefZonePivotValidationTest : RefZonePivotValidationTest
	{
		public void TestUniqueMembers_TrainingZone()
		{
			AssertLocationIsAllowed(true, "TRNZ", EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training, clearUNLOCOsAfterAsserting: false);
			AssertLocationIsAllowed(true, "ALLZ", RefZoneHeaderLookups.ZoneTypeCodes.All);
			AssertLocationIsAllowed(true, "RPTZ", RefZoneHeaderLookups.ZoneTypeCodes.Reporting);
			AssertLocationIsAllowed(true, "RATZ", RefZoneHeaderLookups.ZoneTypeCodes.Rating);
			AssertLocationIsAllowed(true, "IMPZ", RefZoneHeaderLookups.ZoneTypeCodes.RatingImport);
			AssertLocationIsAllowed(true, "EXPZ", RefZoneHeaderLookups.ZoneTypeCodes.RatingExport);
			AssertLocationIsAllowed(false, "TRN2", EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training);
		}

		protected virtual Type GetRefZoneHeaderType()
		{
			return typeof(EDIRefZoneHeader);
		}
	}
}