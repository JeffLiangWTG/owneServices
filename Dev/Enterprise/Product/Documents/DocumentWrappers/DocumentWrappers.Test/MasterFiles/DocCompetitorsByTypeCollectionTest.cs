using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCompetitorsByTypeCollection))]
	public class DocCompetitorsByTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCompetitorsByTypeCollection>
	{
		public void TestDocCompetitorsByTypeCollectionConstructor()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";
			org2.OH_FullName = "Org2";
			org3.OH_FullName = "Org3";
			var competitor1 = Factory.NewWithValidTestData<OrgCompetitor>();
			var competitor2 = Factory.NewWithValidTestData<OrgCompetitor>();
			var competitor3 = Factory.NewWithValidTestData<OrgCompetitor>();
			competitor1.OCP_OH_Parent = orgHeader.PK;
			competitor2.OCP_OH_Parent = orgHeader.PK;
			competitor3.OCP_OH_Parent = orgHeader.PK;
			competitor1.OCP_OH_Competitor = org1.PK;
			competitor2.OCP_OH_Competitor = org2.PK;
			competitor3.OCP_OH_Competitor = org3.PK;
			competitor1.OCP_Type = CompetitorTypeList.Codes.Forwarding;
			competitor2.OCP_Type = CompetitorTypeList.Codes.Forwarding;
			competitor3.OCP_Type = "TST";

			Factory.Save();

			AssertEquals("3 competitors added", 3, orgHeader.Competitors.Count);

			var docCompetitorsByTypeCollection = new DocCompetitorsByTypeCollection(orgHeader);
			AssertEquals("2 type groups it has", 2, docCompetitorsByTypeCollection.Count);
			AssertEquals("Types in registry have full description", "CMF - Forwarding", docCompetitorsByTypeCollection[0].TypeCodeAndDescription);
			AssertEquals("Types not in registry have only code", "TST", docCompetitorsByTypeCollection[1].TypeCodeAndDescription);
			AssertEquals("2 competitors are together", 2, docCompetitorsByTypeCollection[0].Competitors.Count);
			AssertEquals("1 is in TST", 1, docCompetitorsByTypeCollection[1].Competitors.Count);
			AssertEquals("Org name is correct for TST", org3.OH_FullName, docCompetitorsByTypeCollection[1].Competitors[0].CompetitorName);
			AssertArrayEqualsByElements("Org name is correct for CMF", new ZString[2] { org1.OH_FullName, org2.OH_FullName }, docCompetitorsByTypeCollection[0].Competitors.OfType<DocCompetitor>().Select(c => c.CompetitorName).ToArray());
		}

		#region Implementation

		protected override DocCompetitorsByTypeCollection GetCollectionToTest()
		{
			return new DocCompetitorsByTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocCompetitorsByType();
		}

		#endregion
	}
}
