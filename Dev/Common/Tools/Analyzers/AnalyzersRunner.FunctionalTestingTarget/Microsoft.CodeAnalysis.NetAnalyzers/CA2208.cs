using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Instantiate argument exceptions correctly
	/// </summary>
	class CA2208
	{
		public void M1(string name = "Name 1")
		{
			throw new ArgumentException(nameof(name));
		}

		public void M2(string name = "Name 1")
		{
			throw new ArgumentException(nameof(name), "Invalid name");
		}

		public void M3(string name = "Name 1")
		{
			throw new ArgumentNullException("Invalid name", nameof(name));
		}

		public void M4(string name = "Name 1")
		{
			throw new ArgumentNullException("Invalid name");
		}
	}
}
