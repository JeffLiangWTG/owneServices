using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace AppDomainWrappers.Net
{
	public interface IAppDomainWrapper : IDisposable
	{
		Assembly[] GetAssemblies();
		object CreateInstanceAndUnwrap(string assemblyFile, string typeName, params object[] args);
		AppDomain CreateTestAppDomain(string name);
		void RunActionInTask(Action action, CancellationToken cancellationToken);
		string RunCodeInProcess(string codeToRun, List<string> dllFiles = null, List<string> imports = null);
		string RunCodeInProcess48(string codeToRun, List<string> dllFiles = null, List<string> imports = null);
		string RunMethodInProcess(ProcessConfig config);
		string RunMethodInProcess48(ProcessConfig config);
		Dictionary<string, object> RunActionInAppDomain(Action action, Dictionary<string, object> appDomainData = null, Dictionary<string, object> appDomainReturnData = null);
	}
}
