using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	class OpenSubComponentBoardMenuItem : ZToolStripMenuItem
	{
		public OpenSubComponentBoardMenuItem(ZGuid subComponentPK)
			: base(Res.GetString("292cb86e-f84d-45fb-adb5-81c8241a5412", "Open Sub-Component Board"))
		{
			this.subComponentPK = subComponentPK;
			BuildMenu();
		}

		readonly ZGuid subComponentPK;

		void BuildMenu()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "OpenSubComponentBoardMenuItem" };
			var boardQuery = new ZDBOnlyQuery(typeof(BMBoard));
			boardQuery.OrderBy = BMBoardSchema.MB_Name.Name;

			var sectionQuery = new ZDBOnlySubQuery(typeof(BMBoardSection), BMBoardSectionSchema.MS_MB_Board);
			sectionQuery.AddToFilter(BMBoardSectionSchema.MS_FC_Component, subComponentPK);

			boardQuery.AddSubQuery(sectionQuery, JoinCondition.And);
			var boards = factory.Load<BMBoard>(boardQuery);

			if (boards.Length == 0)
			{
				Click += (s, e) => Globals.Message.Show(Res.GetString("e6263bf6-f8b7-4490-869e-3be8d4e947b5", "There are no Visual Boards configured with this sub-component as a section."));
			}
			else
			{
				foreach (var board in boards)
				{
					var menuItem = new VisualBoardDropdownItem(board);
					DropDownItems.Add(menuItem);
				}
			}
		}

		sealed class VisualBoardDropdownItem : ZToolStripMenuItem
		{
			internal VisualBoardDropdownItem(IVisualBoardProvider board)
				: base(board.Name)
			{
				Click += (sender, e) => VisualBoardFormDisplayer.ShowBoard(board);
			}
		}
	}
}
