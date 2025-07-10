using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BranchPortName))]
	sealed class BranchPortNameTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertIsResponsibleForReplacing("<BranchPortName>");
		}

		public void TestReplacement()
		{
			ZString oldPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
				AssertIsReplacedWith("Brisbane", "<BranchPortName>");

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
				AssertIsReplacedWith("", "<BranchPortName>");
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = oldPort;
			}
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new BranchPortName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
		}

		#endregion
	}
}
