using System;
using System.Collections.Concurrent;
using System.IO;

namespace AppDomainWrappers.Net
{
	public class TemporaryWorkspace : IDisposable
	{
#pragma warning disable IDE0002    // Explicitly providing the namespace for Temp here, to highlight that it's not the CargoWise.IO version.
		static string BaseDirectory => AppDomainWrappers.Net.Temp.TempPathWithoutCreating;
#pragma warning restore IDE0002

		public static string GenerateTempDirectoryPathWithoutCreation() => Path.Combine(BaseDirectory, DirectoryHelper.GenerateRandomNumber(10000, 100000));

		protected readonly DirectoryInfo rootDirectory;
		readonly ConcurrentDictionary<string, ManagedDirectory> managedDirectories = new();
		bool disposed;

		public string RootDirectory => rootDirectory.FullName;

		/// <summary>
		/// Create a new temporary directory.
		/// </summary>
		/// <param name="directoryName">You can provide a sub-directory name, which will create the directory at your desired directory level.</param>
		/// <param name="rootName">This will be prefilled with a random number, but can be set if a different root name is desired.</param>
		public TemporaryWorkspace(string directoryName = null, string rootName = null)
		{
			// Ensure directory exists
			var root = string.IsNullOrWhiteSpace(rootName) ? GenerateTempDirectoryPathWithoutCreation() : Path.Combine(BaseDirectory, rootName);

			if (!string.IsNullOrWhiteSpace(directoryName))
			{
				root = Path.Combine(BaseDirectory, directoryName);
			}

			rootDirectory = DirectoryHelper.CreateLowSecurityDirectory(root);
		}

		public ManagedDirectory CreateManagedDirectory()
		{
			var managedDirectory = new ManagedDirectory(this);
			_ = TryAdd(managedDirectory);
			return managedDirectory;
		}

		internal bool TryAdd(ManagedDirectory managedDirectory)
		{
			return managedDirectories.TryAdd(managedDirectory.FullPath, managedDirectory);
		}

		// Finalizer (destructor)
		~TemporaryWorkspace()
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

			if (disposing)
			{
				// Cleanup managed resources (e.g., other IDisposable objects)
				foreach (var managedDir in managedDirectories.Values)
				{
					// Dispose the managed directory
					managedDir.Dispose();
				}

				// Clear the managed directories dictionary
				managedDirectories.Clear();
			}

			// Delete the root directory
			if (rootDirectory.Exists)
			{
				DirectoryHelper.DeleteDirectoryContents(rootDirectory);
			}

			disposed = true;
		}
	}
}
