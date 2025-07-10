using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class AddInfoJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestSpecificCircumstanceIndicatorList()
		{
			AssertEquals("lookups.SpecificCircumstanceIndicatorList.CodesAsString", "E, A, D, C, B", declaration.AddInfoLookups.SpecificCircumstanceIndicatorList.CodesAsString);
		}

		public virtual void TestCommunityTransitStatusListImport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", "T, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM", declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
		}

		public virtual void TestCommunityTransitStatusListExport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", "C, F, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM, TD, TF, X", declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
		}

		public void TestCommunityTransitStatusListMiscellaneous()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", ZString.Empty, declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
		}

		public void TestCommunityTransitStatusListEmptyMessageType()
		{
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", ZString.Empty, declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
		}

		public void TestAuthorisationNumberList()
		{
			AssertNotNull(declaration.AddInfoLookups.AuthorisationNumberList);
			AssertEquals(0, declaration.AddInfoLookups.AuthorisationNumberList.Count);
		}

		public void TestRouteOfEntryList()
		{
			AssertEquals("lookups.RouteOfEntryList.CodesAsString", "", declaration.AddInfoLookups.RouteOfEntryList.CodesAsString);
		}

		public void TestSecurityTypeList()
		{
			AssertEquals("lookups.SecurityTypeList.CodesAsString", "0, 2", declaration.AddInfoLookups.SecurityTypeList.CodesAsString);
		}

		public virtual void TestRegionOfDestinationList()
		{
			AssertEquals("lookups.RegionOfDestinationList.CodesAsString", "", declaration.AddInfoLookups.RegionOfDestinationList.CodesAsString);
		}

		public void TestBox18TransportCountryList()
		{
			AssertType<RefCountryCollection>(declaration.Lookups.Box18TransportCountryList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
		}

		JobDeclaration declaration;
	}
}
