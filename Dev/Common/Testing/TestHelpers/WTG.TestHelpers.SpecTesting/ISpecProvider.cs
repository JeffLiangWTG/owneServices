using System.Collections.Generic;

namespace WTG.TestHelpers.SpecTesting
{
	public interface ISpecProvider
	{
		/// <summary>
		/// The folder that the spec files will be written to, relative to the repository root
		/// </summary>
		public string SpecFolderPath { get; }

		/// <summary>
		/// The namespace prefix for the embedded spec file resources, often mirroring the spec testing project namespace plus the child folder path
		/// </summary>
		public string SpecNamespacePrefix { get; }

		/// <summary>
		/// The executable that needs to be run to regenerate the spec files. This is displayed in the error message if the spec is broken
		/// </summary>
		public string RegenExecutableName { get; }

		/// <summary>
		/// Get a list of spec files for comparing or regenerating
		/// </summary>
		public IEnumerable<SpecFile> GetSpecFiles();
	}
}
