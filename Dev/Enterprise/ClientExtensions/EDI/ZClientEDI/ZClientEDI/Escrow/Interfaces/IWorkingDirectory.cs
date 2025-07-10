using System;

namespace Enterprise.Client.EDI.Escrow
{
	interface IWorkingDirectory : IDisposable
	{
		string DirectoryName { get; }
	}
}
