using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ValueChangedEventArgs : InfoEventArgs
	{
		public ValueChangedEventArgs(IZType oldValue, ZPropertyInfo info)
			: base(info)
		{
			OldValue = oldValue;
		}

		public IZType NewValue
		{
			get { return Info.Value; }
		}

		/// <summary>
		/// The value before the change
		/// </summary>
		public IZType OldValue { get; }
	}
}
