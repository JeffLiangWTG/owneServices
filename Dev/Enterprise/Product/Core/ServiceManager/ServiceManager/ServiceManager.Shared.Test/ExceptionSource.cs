using System;
using System.Collections.Generic;
using Enterprise.Environment;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public static class ExceptionSource
	{
		public static IEnumerable<Exception> NotCriticalExceptions => new[]
		{
			new Exception(),
			new InvalidOperationException(),
			new ArgumentException(),
		};

		public static IEnumerable<Exception> CriticalExceptions => new[]
		{
			new OutOfMemoryException(),
			new Exception(string.Empty, new OutOfMemoryException()),
			new AppDomainUnloadedException(),
			new Exception(string.Empty, new AppDomainUnloadedException()),
			new UserContextLostException(new UserContext(), new UserContext()),
			new Exception(string.Empty, new UserContextLostException(new UserContext(), new UserContext())),
		};
	}
}
