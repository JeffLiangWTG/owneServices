using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public delegate void ZPropertyValueChangedEventHandler(object sender, ZPropertyValueChangedEventArgs e);
	public class ZPropertyValueChangedEventArgs : EventArgs
	{
		public ZPropertyValueChangedEventArgs(ZPropertyInfo property, IZType oldValue)
		{
			this.property = property;
			this.oldValue = oldValue;
		}

		public ZPropertyInfo Property
		{
			get { return property; }
		}
		readonly ZPropertyInfo property;

		public IZType OldValue
		{
			get { return oldValue; }
		}
		readonly IZType oldValue;
	}

	public static class PropertyChangeSubscription
	{
		public static event ZPropertyValueChangedEventHandler PropertyChanged;

		public static void NotifyPropertyChanged(ZPropertyInfo property, IZType oldValue)
		{
			PropertyChanged?.Invoke(null, new ZPropertyValueChangedEventArgs(property, oldValue));
		}
	}
}
