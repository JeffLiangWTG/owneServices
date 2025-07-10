using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(Statement))]
	public class StatementStreamingBelowMaxPreviewCountTest : StatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Statement.TestAboveMaxPreviewCount = false;
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
