using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.FLF.Testing
{
	[TestedType(typeof(FLFStatement))]
	sealed class FLFStatementStreamingBelowMaxPreviewCountTest : FLFStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			FLFStatement.TestAboveMaxPreviewCount = false;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
