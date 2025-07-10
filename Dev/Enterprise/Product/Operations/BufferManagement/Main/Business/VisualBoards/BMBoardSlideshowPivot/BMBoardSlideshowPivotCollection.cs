using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowPivotCollection : ActiveBusinessObjectCollection<BMBoardSlideshowPivot>
	{
		public BMBoardSlideshowPivotCollection(BMBoardSlideshow slideShow)
			: base(slideShow.Factory, slideShow, new ZQuery(), BMBoardSlideshowPivotSchema.MC_MD_Slideshow)
		{
		}

		public BMBoardSlideshowPivotCollection(BMBoard board)
			: base(board.Factory, board, new ZQuery(), BMBoardSlideshowPivotSchema.MC_MB_Board)
		{
		}

		protected override void SetDefaultsForNewElementCore(BMBoardSlideshowPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (Count > 0)
			{
				newElement.MC_Sequence = ToArray().Max(x => x.MC_Sequence) + 1;
				newElement.SystemPK = ToArray().OrderBy(x => x.MC_Sequence).Last().SystemPK;
			}
			else
			{
				newElement.MC_Sequence = 1;
			}
		}
	}
}
