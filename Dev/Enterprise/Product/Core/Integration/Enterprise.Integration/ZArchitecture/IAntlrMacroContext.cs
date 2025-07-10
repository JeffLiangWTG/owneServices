using System;
using System.Collections.Generic;
using CargoWise.Macros;

namespace Enterprise.Integration
{
	public interface IAntlrMacroContext : IDisposable
	{
		object Parent { get; }
		Type ParentType { get; }
		IMacroLibrary[] Libraries { get; }
		Dictionary<string, (object, Type)> Variables { get; }
		IMacroScope Scope { get; }
		Func<string, string> ErrorMessageExtender { get; }
	}
}
