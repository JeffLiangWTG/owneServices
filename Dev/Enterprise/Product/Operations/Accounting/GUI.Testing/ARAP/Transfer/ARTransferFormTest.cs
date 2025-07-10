using System;
using Enterprise.Accounting.Business.ARAP;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(TransferForm))]
	class ARTransferFormTest : TransferFormTest
	{
		public override Type transferType => typeof(APTransfer);
	}
}
