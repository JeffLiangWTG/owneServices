using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Test
{
	[TestedType(typeof(MFIStatement))]
	sealed class MFIStatementStreamingBelowMaxPreviewCountTest : MFIStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			MFIStatement.TestAboveMaxPreviewCount = false;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
