using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSAdjustmentLine))]
	public class CASSAdjustmentLineTest : CASSDataTest
	{
		protected override CASSData GetCASSData()
		{
			return new CASSAdjustmentLine();
		}
	}
}
