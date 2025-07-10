using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ProcessInfo : IProcessInfo
	{
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We ignore exceptions related to getting current process Id")]
		public string ProcessId
		{
			get
			{
				try
				{
					return Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return "?";
				}
			}
		}
	}
}
