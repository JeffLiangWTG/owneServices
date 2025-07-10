using System;
using System.IO;

namespace AppDomainWrappers.Net
{
	public class ManagedDirectory : IDisposable
	{
		public string FullPath { get; }
		readonly TemporaryWorkspace parentWorkspace;
		bool disposed;

		public ManagedDirectory(TemporaryWorkspace parent)
		{
			parentWorkspace = parent ?? throw new ArgumentNullException(nameof(parent));

			// Create cryptographically secure directory name using a GUID
			var secureName = Guid.NewGuid().ToString("N");
			// Initialise FullPath here, so that it doesn't happen too early (as the parent wants to capture the path, too).
			FullPath = Path.Combine(parentWorkspace.RootDirectory, secureName);
			_ = Directory.CreateDirectory(FullPath);
			_ = parent.TryAdd(this);
		}

		// Finalizer (destructor)
		~ManagedDirectory()
		{
			// Finalizer calls Dispose(false)
			Dispose(false);
		}

		// Implement IDisposable
		public void Dispose()
		{
			Dispose(true);

			// Suppress finalization, since Dispose has already been called
			GC.SuppressFinalize(this);
		}

		// The core dispose method
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			// Cleanup unmanaged resources
			if (Directory.Exists(FullPath))
			{
				Directory.Delete(FullPath, recursive: true);
			}

			disposed = true;
		}

		// Implicit operator to allow conversion to string
		public static implicit operator string(ManagedDirectory managedDirectory) => managedDirectory.FullPath;
	}
}
