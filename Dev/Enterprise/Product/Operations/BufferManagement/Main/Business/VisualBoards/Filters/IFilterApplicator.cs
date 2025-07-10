using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public interface IFilterApplicator : IBoardFilter
	{
		bool IsApplicable(ICardContent cardContent, CellContent cell, BMBoardSectionViewModel boardSection);
		void Apply(IComponent control, bool isApplicable, IEnumerable<AppliedFilter> applicableApplicators);
	}
}
