using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionAcceptabilityBandCollection : NonPersistentBusinessObjectCollection<BoardSectionAcceptabilityBand>
	{
		public BoardSectionAcceptabilityBandCollection(BMBoardSection section)
			: base(section.Factory)
		{
			this.section = Argument.NotNull(section, "section");
		}

		readonly BMBoardSection section;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BoardSectionAcceptabilityBand(section);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var sectionBand = (BoardSectionAcceptabilityBand)child;
			var maxItem = this.Cast<BoardSectionAcceptabilityBand>().MaxBySafe(b => b.DisplaySequence);
			sectionBand.DisplaySequence = maxItem != null ? maxItem.DisplaySequence + (ZShort)1 : (ZShort)1;
		}
	}
}
