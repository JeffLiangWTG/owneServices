using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSlideshow))]
	public class BMBoardSlideshowTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var slideshow = Factory.New<BMBoardSlideshow>();
			slideshow.MD_Name = "Dat slideshow";

			AssertEquals("Dat slideshow", slideshow.HumanReadableName);
		}

		public void TestBoardPivots()
		{
			var board1 = Factory.NewWithValidTestData<BMBoard>();
			var board2 = Factory.NewWithValidTestData<BMBoard>();

			var boardSlideShow = Factory.NewWithValidTestData<BMBoardSlideshow>();
			boardSlideShow.MD_Name = "Zayden";

			var boardPivot1 = boardSlideShow.BoardPivots.AddNew();
			boardPivot1.MC_MB_Board = board1.PK;
			boardPivot1.MC_Sequence = 2;

			var boardPivot2 = boardSlideShow.BoardPivots.AddNew();
			boardPivot2.MC_MB_Board = board2.PK;
			boardPivot2.MC_Sequence = 1;

			Factory.Save();

			var reloadedSlideShow = new BusinessObjectFactory().Load<BMBoardSlideshow>(boardSlideShow.PK);
			AssertEquals(2, reloadedSlideShow.BoardPivots.Count);
			AssertEquals(board2.PK, reloadedSlideShow.BoardPivots[0].MC_MB_Board);
			AssertEquals(board1.PK, reloadedSlideShow.BoardPivots[1].MC_MB_Board);
		}

		public void TestBufferManagementVisualBoardProvider()
		{
			var board1 = Factory.New<BMBoard>();
			var board2 = Factory.New<BMBoard>();

			var boardSlideShow = Factory.New<BMBoardSlideshow>();
			boardSlideShow.MD_Name = "Zayden slide show";

			var boardPivot1 = boardSlideShow.BoardPivots.AddNew();
			boardPivot1.MC_MB_Board = board1.PK;

			var boardPivot2 = boardSlideShow.BoardPivots.AddNew();
			boardPivot2.MC_MB_Board = board2.PK;

			var boardProvider = (IVisualBoardProvider)boardSlideShow;
			AssertEquals("Zayden slide show", boardProvider.Name);
			AssertEquals("Zayden slide show", boardProvider.Description);
			AssertEquals(boardSlideShow.PK, boardProvider.PK);
			AssertEquals(2, boardProvider.Boards.Count());
			AssertEquals(true, boardProvider.Boards.Any(x => x.BoardPK == board1.PK));
			AssertEquals(true, boardProvider.Boards.Any(x => x.BoardPK == board2.PK));

			boardSlideShow.MD_Name = "Lola slide show";
			AssertEquals("Lola slide show", boardProvider.Name);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var system = factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			var slideShow = factory.NewWithValidTestData<BMBoardSlideshow>();
			slideShow.BoardPivots.AddNew().MC_MB_Board = board.PK;

			return slideShow;
		}

		public void TestGetAllComponentsAcrossMultipleBoards_DBHits()
		{
			const int numBoards = 6;
			const int numSections = 2;
			const int numRelationships = 2;
			const int numBuffers = 2;
			var boards = MakeBoardsWithRelationships(numBoards, numSections, numRelationships, numBuffers);
			var newFactory = Factory.CreateNewFactory();

			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMBoardSectionSchema.Constants.TableName, 1 },
				{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 4 }, // 2 for newfactory, 2 each due to the number of fetch hints exceeding the max.
				{ BMComponentLinkSchema.Constants.TableName, 1 },
			};

			var loadedBoards = boards.Select(board => newFactory.Load<BMBoard>(board.PK)).ToArray();

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true))
			{
				var allComponents = BMBoardSlideshow.GetAllComponentsAcrossMultipleBoards(loadedBoards);

				AssertEquals("Failed to load all components across multiple boards", numBoards * numSections * numRelationships * numBuffers + (numBoards * numSections), allComponents.Count());
			}
		}

		#region Logs

		public void TestNoStmALogs()
		{
			var slideshow = Factory.NewWithValidTestData<BMBoardSlideshow>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, slideshow.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			slideshow.MD_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			slideshow.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		IEnumerable<BMBoard> MakeBoardsWithRelationships(int numBoards, int numSections, int numRelationships, int numBuffers)
		{
			var boards = new List<BMBoard>();

			for (int i = 0; i < numBoards; i++)
			{
				var system = BMSTestHelper.CreateSystem(Factory);
				boards.Add(BMSTestHelper.CreateBoard(system, name: "board " + i));

				for (int j = 0; j < numSections; j++)
				{
					var primaryBuffer = BMSTestHelper.CreateBuffer(system, name: "primary buffer " + j);
					var section = BMSTestHelper.CreateBoardSection(primaryBuffer, boards[i]);

					for (int k = 0; k < numRelationships; k++)
					{
						var relationship = BMSTestHelper.CreateComponentRelationship(Factory, name: "relationship " + i + j + k);

						for (int l = 0; l < numBuffers; l++)
						{
							var buffer = BMSTestHelper.CreateBuffer(system, name: "buffer " + j + k + l);
							BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, buffer);
						}

						BMSTestHelper.CreateAdditionalComponent(section, relationship);
					}
				}
			}

			return boards;
		}

		#endregion
	}
}
