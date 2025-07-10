using System.ComponentModel;

namespace Enterprise.RemotePrinting.Engine
{
	[ImmutableObject(true)]
	public sealed class PrinterInfo
	{
		public PrinterInfo(string name, bool isValid, bool isSuspectedSurrogate)
		{
			Name = name;
			IsValid = isValid;
			IsSuspectedSurrogate = isSuspectedSurrogate;
		}

		public string Name { get; private set; }
		public bool IsValid  { get; private set; }
		public bool IsSuspectedSurrogate { get; private set; }
	}
}
