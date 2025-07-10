using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ConcurrencyValueChangedEventArgs : ValueChangedEventArgs
	{
		public ConcurrencyValueChangedEventArgs(IZType oldValue, object currentValue, ZPropertyInfo info, object origValue, string lastModifiedBy) : base(oldValue, info)
		{
			CurrentValue = currentValue;
			OriginalValue = origValue;
			LastModified = lastModifiedBy;
		}

		public string LastModified { get; private set; }

		public object OriginalValue { get; private set; }

		public object CurrentValue { get; private set; }

		/// <summary>
		/// Custom merge value
		/// </summary>
		public object FinalValue { get; set; }
	}
}
