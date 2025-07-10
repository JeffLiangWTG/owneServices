using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(Statement))]
	public class StatementStreamingAboveMaxPreviewCountTest : StatementBaseTest
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
