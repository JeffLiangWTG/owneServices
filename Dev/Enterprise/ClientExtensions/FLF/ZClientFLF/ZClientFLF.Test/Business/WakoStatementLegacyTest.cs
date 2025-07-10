using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.FLF.Testing
{
	[TestedType(typeof(FLFStatement))]
	sealed class WakoStatementLegacyTest : FLFStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
