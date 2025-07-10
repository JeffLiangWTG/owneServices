using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.VisualBoards.Business
{
	public class BoardFilterBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BoardFilterBusinessObject(IBoardFilter filter, IFilterable filterable)
		{
			Filter = filter;
			Filterable = filterable;
		}

		public IBoardFilter Filter { get; private set; }
		public IFilterable Filterable { get; private set; }

		#region Properties

		[ResourceStringData("BoardFilterBusinessObject.FilterName", Caption = "Filter Name")]
		public ZString FilterName
		{
			get { return Filter.FilterName; }
		}

		[ResourceStringData("BoardFilterBusinessObject.ParentName", Caption = "Applied To")]
		public ZString ParentName
		{
			get { return Filterable.Name; }
		}

		#endregion
	}
}
