using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class SerializableNoteElementAttribute : Attribute
	{
		public SerializableNoteElementAttribute(string displayName)
		{
			Argument.NotNull(displayName, "displayName");
			DisplayName = displayName;
		}

		public SerializableNoteElementAttribute(bool hiddenElement)
		{
			HiddenElement = hiddenElement;
			DisplayName = string.Empty;
		}

		public bool HiddenElement { get; set; }
		public string DisplayName { get; set; }
	}
}
