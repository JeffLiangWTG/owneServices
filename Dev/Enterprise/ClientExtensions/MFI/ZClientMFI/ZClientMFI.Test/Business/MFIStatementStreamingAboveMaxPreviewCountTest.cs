using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Test
{
	[TestedType(typeof(MFIStatement))]
	sealed class MFIStatementStreamingAboveMaxPreviewCountTest : MFIStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			MFIStatement.TestAboveMaxPreviewCount = true;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			MFIStatement.TestAboveMaxPreviewCount = false;
		}
	}
}
