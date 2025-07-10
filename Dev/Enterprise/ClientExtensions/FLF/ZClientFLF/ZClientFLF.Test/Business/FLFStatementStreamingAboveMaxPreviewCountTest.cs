using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.FLF.Testing
{
	[TestedType(typeof(FLFStatement))]
	sealed class FLFStatementStreamingAboveMaxPreviewCountTest : FLFStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			FLFStatement.TestAboveMaxPreviewCount = true;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			FLFStatement.TestAboveMaxPreviewCount = false;
		}
	}
}
