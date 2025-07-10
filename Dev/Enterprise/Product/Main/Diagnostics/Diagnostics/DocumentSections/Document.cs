using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Diagnostics
{
	public class Document : NonPersistentBusinessObject
	{
		public ZString Context { get; set; }
		public ZString MenuItemName { get; set; }
		public ZString DocumentTitle { get; set; }
		public ZString PrintOrder { get; set; }
		public ZString SectionType { get; set; }
		public ZString FilterList { get; set; }
		public ZBool IsSystemDefined { get; set; }
	}
}
