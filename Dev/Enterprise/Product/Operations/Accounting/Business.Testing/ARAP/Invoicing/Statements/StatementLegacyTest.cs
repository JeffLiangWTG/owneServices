using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(Statement))]
	public class StatementLegacyTest : StatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
