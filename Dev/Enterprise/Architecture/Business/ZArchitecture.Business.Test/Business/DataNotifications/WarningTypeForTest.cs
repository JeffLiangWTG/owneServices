using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	sealed class WarningTypeForTest : WarningType
	{
		public WarningTypeForTest(string name, string message) : base(name, message)
		{
		}

		public WarningTypeForTest(string message) : this(message, message)
		{
		}
	}
}
