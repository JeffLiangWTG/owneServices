using System;

namespace Enterprise.ZArchitecture.Modules
{
	public struct CheckpointLookupKey : IEquatable<CheckpointLookupKey>
	{
		readonly string code;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Suppress the warning.")]
		public static readonly CheckpointLookupKey Empty = new CheckpointLookupKey();
		readonly Guid itemGuid;

		public CheckpointLookupKey(string code, Guid itemGuid)
		{
			this.code = code;
			this.itemGuid = itemGuid;
		}

		public CheckpointLookupKey(string code)
			: this(code, Guid.Empty)
		{
		}

		public static bool operator !=(CheckpointLookupKey a, CheckpointLookupKey b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(CheckpointLookupKey a, CheckpointLookupKey b)
		{
			return a.Equals(b);
		}

		public string Code
		{
			get { return code ?? ""; }
		}

		public bool IsEmpty
		{
			get { return Equals(Empty); }
		}

		public Guid ItemGuid
		{
			get { return itemGuid; }
		}

		public bool CodeEquals(string otherCode)
		{
			return string.Equals(Code, (otherCode ?? ""), StringComparison.OrdinalIgnoreCase);
		}

		public bool Equals(CheckpointLookupKey other)
		{
			return CodeEquals(other.Code) && ItemGuid.Equals(other.ItemGuid);
		}

		public override bool Equals(object obj)
		{
			return (obj is CheckpointLookupKey) && Equals((CheckpointLookupKey)obj);
		}

		public override int GetHashCode()
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(Code) ^ ItemGuid.GetHashCode();
		}
	}
}
