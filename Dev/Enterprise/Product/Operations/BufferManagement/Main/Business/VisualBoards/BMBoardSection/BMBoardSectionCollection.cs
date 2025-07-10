using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionCollection : ActiveBusinessObjectCollection<BMBoardSection>
	{
		public BMBoardSectionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BMBoardSectionCollection(BMBoard board)
			: base(board.Factory, board, new ZQuery(), BMBoardSectionSchema.MS_MB_Board)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.IncludeBlob(BMBoardSectionSchema.MS_LayoutData);
			return filter;
		}
	}
}
