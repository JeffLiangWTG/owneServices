using System.Diagnostics;

namespace Enterprise.Security.ActiveDirectory
{
	[DebuggerDisplay("Property: {PropertyName} AD: {ADStartingValue} EDI: {EnterpriseStartingValue} Sync: {SynchronisedValue}")]
	public class SyncEvent : ISyncEvent
	{
		public string PropertyName { get; set; }
		public object ADStartingValue { get; set; }
		public object EnterpriseStartingValue { get; set; }
		public object SynchronisedValue { get; set; }
		public bool IsForcedToShowInReport { get; set; }

		#region Equality

		public override bool Equals(object obj)
		{
			var other = obj as SyncEvent;
			return other != null
				&& AreEqual(other.PropertyName, PropertyName)
				&& AreEqual(other.ADStartingValue, ADStartingValue)
				&& AreEqual(other.EnterpriseStartingValue, EnterpriseStartingValue)
				&& AreEqual(other.SynchronisedValue, SynchronisedValue)
				&& AreEqual(other.IsForcedToShowInReport, IsForcedToShowInReport);
		}

		static bool AreEqual(object obj1, object obj2)
		{
			return (obj1 ?? string.Empty).ToString() == (obj2 ?? string.Empty).ToString();
		}

		public override int GetHashCode()
		{
			return GetHash(PropertyName) ^ GetHash(ADStartingValue) ^ GetHash(EnterpriseStartingValue) ^ GetHash(SynchronisedValue) ^ GetHash(IsForcedToShowInReport);
		}

		static int GetHash(object obj)
		{
			return (obj ?? string.Empty).GetHashCode();
		}

		#endregion
	}
}
