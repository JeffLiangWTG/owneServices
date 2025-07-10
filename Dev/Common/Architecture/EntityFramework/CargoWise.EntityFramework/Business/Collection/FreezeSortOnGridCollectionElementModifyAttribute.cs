using System;
using System.Collections;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class FreezeSortOnGridCollectionElementModifyAttribute : Attribute
	{
		public FreezeSortOnGridCollectionElementModifyAttribute(bool enabled)
		{
			this.Enabled = enabled;
		}

		public static bool IsEnabled(IList list)
		{
			FreezeSortOnGridCollectionElementModifyAttribute attr = (FreezeSortOnGridCollectionElementModifyAttribute)TypeDescriptor.GetAttributes(list)[typeof(FreezeSortOnGridCollectionElementModifyAttribute)];
			return attr != null && attr.Enabled;
		}

		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}
		bool enabled;
	}
}
