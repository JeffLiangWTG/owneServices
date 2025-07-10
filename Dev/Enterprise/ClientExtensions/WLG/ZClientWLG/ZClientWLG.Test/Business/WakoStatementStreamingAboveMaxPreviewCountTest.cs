using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.WLG
{
	[TestedType(typeof(WakoStatement))]
	sealed class WakoStatementStreamingAboveMaxPreviewCountTest : WakoStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Statement.TestAboveMaxPreviewCount = true;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Statement.TestAboveMaxPreviewCount = false;
		}
	}
}
