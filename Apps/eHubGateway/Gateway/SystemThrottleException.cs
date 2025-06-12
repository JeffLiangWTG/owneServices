using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Gateway
{
	public class SystemThrottleException : SystemException
	{
		public SystemThrottleException(string message)
			: base(message)
		{
		}
	}
}