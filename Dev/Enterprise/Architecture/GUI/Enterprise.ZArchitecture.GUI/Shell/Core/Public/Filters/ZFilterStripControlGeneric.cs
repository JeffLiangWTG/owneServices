using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ZFilterStripControl<T> : ZFilterStripControl where T : ZFilterStrip, new()
	{
		protected internal override ZFilterStrip NewZFilterStrip()
		{
			return new T();
		}

		public ZFilterStripControl()
		{
		}

		public ZFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
