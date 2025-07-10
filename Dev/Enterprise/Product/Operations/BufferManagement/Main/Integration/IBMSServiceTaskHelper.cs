using System;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMSServiceTaskHelper
	{
		IDisposable GetTemporaryEnvironmentForServiceTaskBranch();
	}
}
