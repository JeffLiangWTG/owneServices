using System.IO;
using Enterprise.Builder.Generator;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class FauxGeneratorOutputDirectory : GeneratorOutputDirectory
	{
		public FauxGeneratorOutputDirectory(SaveMode saveMode, string devSourceDirectory = "Q:\\SomeDevPath\\", string cwsharedSourceDirectory = "Q:\\SomeCWSharedPath")
			: base(saveMode, devSourceDirectory, cwsharedSourceDirectory)
		{
		}

		protected override void CheckInstanceIsCorrectType()
		{
			// Do nothing - we are the faux
		}

		public override GeneratorOutputDirectory WithCWShared(string cwshared) => new FauxGeneratorOutputDirectory(saveMode, base.DevSourceDirectory, cwshared);

		protected override string BaseDirectory => Path.Combine(TestingState.TempPath, "GeneratorOutputForTest");
		public override string DevSourceDirectory => MockSourceControl.MockWorkspacePath;
	}
}
