using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.Billing.Business.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service.Test
{
	public class BoardServiceTest : TestCaseWithFactory
	{
		public void TestGetConfiguration_ShouldReturnNull_WhenCannotLoadBoardByPK()
		{
			var result = Service.GetConfiguration(Guid.NewGuid());

			AssertNull(result);
		}

		public void TestGetConfiguration_ShouldReportUsage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.CreateBoardSection(buffer, board);

			BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessTasks, board);

			Factory.Save();

			new BoardService().GetConfiguration(board.PK.ToGuid());

			var messages = new UsageCollectorTestHelper(Factory).LoadUsageMessages();
			AssertEquals("Expected 1 message to be present, but was: " + messages.Length, 1, messages.Length);
			var currentBranch = Env.CurrentBranch.Code;
			var message = messages.First();
			var messageText = message.EM_MessageText;

			AssertContains("Message must contain FeatureCode: VBO", "\"FeatureCode\": \"VBO\"", messageText);
			AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", messageText);
			AssertContains("Message must contain BranchCode: " + currentBranch, currentBranch, message.Branch.Code);
			AssertContains("Message must contain PK:" + board.PK, $"\"PK\": \"{board.PK}\"", messageText);
			AssertContains("Message must contain Name:" + board.MB_Name, $"\"Name\": \"{board.MB_Name}\"", messageText);
			AssertContains("Message must contain CMP: 1", "\"CMP\": \"1\"", messageText);
			AssertContains("Message must contain MOD: 2", "\"MOD\": \"2\"", messageText);
			AssertContains("Message must contain BUF: 1", "\"BUF\": \"1\"", messageText);
			AssertContains("Message must contain WEB: 1", "\"FromWEB\": \"YES\"", messageText);

			Assert(true);
		}

		public void TestGetConfigurationTriggersLicenseUsage()
		{
			VisualBoardLicensedComponent.ResetBMLicencing_ForTest();
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			section.MS_SectionType = "CMP";
			Factory.Save();

			var query = new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.BufferManagement.Name);

			var activityRecord = Factory.LoadTop1<StmActivityLog>(query);
			AssertNull(activityRecord);

			new BoardService().GetConfiguration(board.PK.ToGuid());

			activityRecord = Factory.LoadTop1<StmActivityLog>(query);
			AssertNotNull(activityRecord);
		}

		public void TestGetConfiguration_ShouldCorrectType()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			section.MS_SectionType = "CMP";
			Factory.Save();

			var sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			var sectionConfigDTO = sectionConfigDTOs.First();
			AssertType<ComponentSectionConfigurationDTO>(sectionConfigDTO);
			AssertEquals(sectionConfigDTO.Type, SectionType.Component);

			section.MS_SectionType = "MOD";
			Factory.Save();

			sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			sectionConfigDTO = sectionConfigDTOs.First();
			AssertType<SectionConfigurationDTO>(sectionConfigDTO);
			AssertEquals(sectionConfigDTO.Type, SectionType.ModuleGrid);

			section.MS_SectionType = "WEB";
			Factory.Save();

			sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			sectionConfigDTO = sectionConfigDTOs.First();
			AssertType<WebBrowserSectionConfigurationDTO>(sectionConfigDTO);
			AssertEquals(sectionConfigDTO.Type, SectionType.WebBrowser);

			section.MS_SectionType = "DIA";
			Factory.Save();

			sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			sectionConfigDTO = sectionConfigDTOs.First();
			AssertType<SectionConfigurationDTO>(sectionConfigDTO);
			AssertEquals(sectionConfigDTO.Type, SectionType.NetworkDiagram);

			section.MS_SectionType = "MNT";
			Factory.Save();

			sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			sectionConfigDTO = sectionConfigDTOs.First();
			AssertType<SectionConfigurationDTO>(sectionConfigDTO);
			AssertEquals(sectionConfigDTO.Type, SectionType.Chart);
		}

		public void TestGetConfiguration_RefreshIntervalMinutes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 200);

			Factory.Save();

			var interval = Service.GetConfiguration(board.PK.ToGuid()).RefreshIntervalMinutes;
			AssertEquals(200, interval);
		}

		#region SetUp

		protected BoardService Service;

		protected override void SetUp()
		{
			base.SetUp();
			Service = new BoardService();
			BMSTestHelper.EnableBMSInRegistry();
		}
		#endregion
	}
}
