
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class FindBoxColumnModuleShowingEventArgs : ModuleShowingEventArgs
	{
		public FindBoxColumnModuleShowingEventArgs(ZGridColumnStyle columnStyle, BusinessObject currentBusinessObject, ModuleIdentifier moduleID)
			: base(moduleID)
		{
			this.columnStyle = columnStyle;
			this.currentBusinessObject = currentBusinessObject;
		}

		public ZGridColumnStyle ColumnStyle
		{
			get { return columnStyle; }
		}

		public BusinessObject CurrentBusinessObject
		{
			get { return currentBusinessObject; }
		}

		readonly ZGridColumnStyle columnStyle;
		readonly BusinessObject currentBusinessObject;
	}
}
