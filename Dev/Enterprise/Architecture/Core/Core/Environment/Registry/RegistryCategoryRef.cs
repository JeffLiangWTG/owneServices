using System.Diagnostics;

namespace Enterprise.ZArchitecture.Core.Environment.Registry
{
	[DebuggerDisplay("Ref: {DisplayText}")]
	public readonly struct RegistryCategoryRef
	{
		public RegistryCategoryRef(object key, string displayText)
		{
			Key = key;
			DisplayText = displayText;
		}

		public object Key { get; }
		public string DisplayText { get; }
	}
}
