using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowPivotLookups : AutoBMBoardSlideshowPivotLookups
	{
		public BMBoardSlideshowPivotLookups(AutoBMBoardSlideshowPivot parent)
			: base(parent)
		{
		}

		new BMBoardSlideshowPivot Parent
		{
			get { return (BMBoardSlideshowPivot)base.Parent; }
		}

		public BMSystemCollection Systems
		{
			get { return new BMSystemCollection(Factory); }
		}

		public BMBoardCollection Boards
		{
			get
			{
				if (Parent.System != null)
				{
					return new BMBoardCollection(Parent.System, new ZQuery());
				}
				else
				{
					return new BMBoardCollection(Factory, ZQuery.NoResultQuery);
				}
			}
		}
	}
}
