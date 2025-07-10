using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Test
{
	[TestedType(typeof(MFIStatement))]
	sealed class MFIStatementLegacyTest : MFIStatementBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
