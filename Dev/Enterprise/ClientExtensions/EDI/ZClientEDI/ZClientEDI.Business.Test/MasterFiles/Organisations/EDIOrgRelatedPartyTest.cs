using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgRelatedParty))]
	internal class EDIOrgRelatedPartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPR_PartyType()
		{
			EDIOrgRelatedParty relatedParty = Factory.New<EDIOrgRelatedParty>();
			AssertEquals("", relatedParty.PR_FreightDirection);
			relatedParty.PR_PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;
			AssertEquals("", relatedParty.PR_FreightDirection);
		}

		public void TestAllParentPartiesAndWARPDescription()
		{
			var header1 = Factory.New<EDIOrgHeader>();
			header1.OH_Code = "AAA";
			var header2 = Factory.New<EDIOrgHeader>();
			header2.OH_Code = "BBB";
			var header3 = Factory.New<EDIOrgHeader>();
			header3.OH_Code = "CCC";
			header1.AddRelatedParty(header2.PK, EDIOrgRelatedPartyLookups.WARPConstant, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			var loadedHeader = Factory.Load<EDIOrgHeader>(header2.PK);
			var relationship = loadedHeader.AllParentParties.Where(x => x.PR_PartyType == EDIOrgRelatedPartyLookups.WARPConstant).First();
			AssertEquals(header1.PK, relationship.PR_OH_Parent);
			AssertEquals(EDIOrgRelatedPartyLookups.WARPNominatedAgentPartyDescription, relationship.ParentPartyTypeDescription);
		}

		public void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeWRP()
		{
			OrganisationMergerTest.AssertAllowMultipleRelatedParties_Merged(EDIOrgRelatedPartyLookups.WARPConstant);
		}

		public void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeSRV()
		{
			OrganisationMergerTest.TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeSRV();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EDIOrgRelatedParty>();
		}

		#endregion
	}
}
