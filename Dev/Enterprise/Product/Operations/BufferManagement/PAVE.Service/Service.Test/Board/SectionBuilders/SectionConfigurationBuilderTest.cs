using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.Service.Test
{
	public class SectionConfigurationBuilderTest : TestCaseWithFactory
	{
		public void TestGetConfiguration_ShouldReturnCorrectSectionConfigurationDTOs()
		{
			var board = CreateBoard();

			var sectionConfigurationDTOs = Service.GetConfiguration(board.PK.ToGuid());

			AssertEquals(5, sectionConfigurationDTOs.Sections.Length);

			foreach (var section in board.Sections)
			{
				var sectionConfigurationDTO = sectionConfigurationDTOs.Sections.First(s => s.SectionPK == section.PK);

				AssertEquals(section.SectionName, sectionConfigurationDTO.Name);
				AssertLayoutValues(section, sectionConfigurationDTO.Layout);
			}
		}

		void AssertLayoutValues(BMBoardSection section, SectionLayoutDTO sectionLayoutDTO)
		{
			AssertEquals(section.Row, sectionLayoutDTO.Row);
			AssertEquals(section.Column, sectionLayoutDTO.Column);
			AssertEquals(section.RowSpan, sectionLayoutDTO.RowSpan);
			AssertEquals(section.ColSpan, sectionLayoutDTO.ColSpan);
			AssertEquals(section.RowHeightPercent, sectionLayoutDTO.RowHeightPercent);
			AssertEquals(section.ColWidthPercent, sectionLayoutDTO.ColWidthPercent);
		}

		BMBoard CreateBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();

			CreateSection(board, "MOD", 5, 6, 7, 8);
			CreateSection(board, "WEB", 9, 10, 11, 12);
			CreateSection(board, "DIA", 13, 14, 15, 16);
			CreateSection(board, "MNT", 17, 18, 19, 20);
			CreateSection(board, "WSA", 21, 22, 23, 24);

			Factory.Save();

			return board;
		}

		void CreateSection(BMBoard board, string type, int row, int col, int rowHeightPercent, int colWidthPercent)
		{
			var section = board.Sections.AddNew();
			section.MS_SectionType = type;
			section.Row = row;
			section.Column = col;
			section.RowHeightPercent = rowHeightPercent;
			section.ColWidthPercent = colWidthPercent;
		}

		#region SetUp

		BoardService Service;

		protected override void SetUp()
		{
			base.SetUp();
			Service = new BoardService();
			BMSTestHelper.EnableBMSInRegistry();
		}
		#endregion
	}
}
