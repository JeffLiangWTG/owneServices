using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	class ErrorTypeForTest : ErrorType
	{
		public ErrorTypeForTest(string name) : base(name)
		{
		}

		public ErrorTypeForTest(string name, string message) : base(name, message)
		{
		}
	}
}
