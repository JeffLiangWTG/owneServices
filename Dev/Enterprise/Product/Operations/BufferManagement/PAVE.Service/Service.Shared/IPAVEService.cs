using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IPAVEService
	{
		void Process(IEnumerable<Guid> processedPKs, ILogger logger, [CallerMemberName] string callerMemberName = "");
	}
}
