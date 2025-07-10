using NUnit.Framework;
namespace Enterprise.Customs.Common.US.Testing
{
	class SEBillProcessingResultListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsHold()
		{
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHold(SEBillProcessingResultList.CBPHold));
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHold(SEBillProcessingResultList.ManifestHoldAgriculture));
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHold(SEBillProcessingResultList.ManifestHoldCBP));
			NUnit.Framework.Assert.That(!SEBillProcessingResultList.IsHold(SEBillProcessingResultList.CBPManifestHoldRemoved));
			NUnit.Framework.Assert.That(!SEBillProcessingResultList.IsHold(SEBillProcessingResultList.AgricultureManifestHoldRemoved));
		}

		[ExpectNoExceptions]
		public void TestIsHoldRemoved()
		{
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHoldRemoved(SEBillProcessingResultList.CBPManifestHoldRemoved));
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHoldRemoved(SEBillProcessingResultList.AgricultureManifestHoldRemoved));
			NUnit.Framework.Assert.That(SEBillProcessingResultList.IsHoldRemoved(SEBillProcessingResultList.CBPHoldRemoved));
			NUnit.Framework.Assert.That(!SEBillProcessingResultList.IsHoldRemoved(SEBillProcessingResultList.ManifestHoldAgriculture));
			NUnit.Framework.Assert.That(!SEBillProcessingResultList.IsHoldRemoved(SEBillProcessingResultList.CBPHold));
		}
	}
}
