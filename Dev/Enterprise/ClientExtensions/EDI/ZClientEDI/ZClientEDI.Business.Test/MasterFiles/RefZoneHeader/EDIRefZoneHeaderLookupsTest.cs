using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIRefZoneHeaderLookupsTest : RefZoneHeaderLookupsTest
	{
		#region Implementation

		protected override RefZoneHeader GetRefZoneHeader()
		{
			return Factory.New<EDIRefZoneHeader>();
		}

		protected override Type ExpectedZoneTypesListType
		{
			get { return typeof(EDIZoneTypeCodePairList); }
		}

		#endregion Implementation
	}
}