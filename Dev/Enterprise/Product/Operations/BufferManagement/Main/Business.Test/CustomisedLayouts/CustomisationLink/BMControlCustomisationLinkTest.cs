using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationLink))]
	class BMControlCustomisationLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUsageDescription()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			system.FS_Name = "WTGDEV";
			var buffer = BMSTestHelper.CreateBuffer(system, "Das Buffer");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "The Cool Kids";

			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var board = system.Boards.AddNew();
			board.MB_Name = "Der Board";
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var link = Factory.New<BMControlCustomisationLink>();
			AssertEquals(ZString.Empty, link.UsageDescription);

			link.FML_ParentId = system.PK;
			link.FML_ParentTableCode = BMSystemSchema.Constants.Prefix;
			AssertEquals("System 'WTGDEV'", link.UsageDescription);

			link.FML_ParentId = group.PK;
			link.FML_ParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertEquals("Release Group 'The Cool Kids'", link.UsageDescription);

			link.FML_ParentId = board.PK;
			link.FML_ParentTableCode = BMBoardSchema.Constants.Prefix;
			AssertEquals("Visual Board 'Der Board'", link.UsageDescription);

			link.FML_ParentId = section.PK;
			link.FML_ParentTableCode = BMBoardSectionSchema.Constants.Prefix;
			AssertEquals("Section 'Das Buffer' on Visual Board 'Der Board'", link.UsageDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var link = factory.NewWithValidTestData<BMControlCustomisationLink>();
			var customisation = factory.NewWithValidTestData<BMControlCustomisation>();
			link.FML_FM_ControlCustomisation = customisation.PK;
			return link;
		}

		#endregion
	}
}
