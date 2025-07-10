using System;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Apply this attribute to a property that is visible on a designer surface to enable it to be shown
	/// by default in a smart-tag.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class SmartTagVisibleAttribute : Attribute
	{
		public SmartTagVisibleAttribute()
		{
		}

		public SmartTagVisibleAttribute(int orderIndex)
		{
			OrderIndex = orderIndex;
		}

		public int OrderIndex { get; private set; }

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as SmartTagVisibleAttribute;
			return
				rhs != null &&
				OrderIndex == rhs.OrderIndex;
		}

		public override int GetHashCode()
		{
			return typeof(SmartTagVisibleAttribute).GetHashCode() ^ OrderIndex.GetHashCode();
		}

		#endregion
	}
}
