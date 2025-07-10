using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class UCRHelperTest : TestCaseWithFactory
	{
		public void TestComposeUcrBlock()
		{
			var ucrBlock = UCRHelper.ComposeUcrBlock("DUCR", "123", ucrType.D);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(ucrBlock.ucr), "DUCR", ucrBlock.ucr);
				AssertEquals(nameof(ucrBlock.ucrPartNo), "123", ucrBlock.ucrPartNo);
				AssertEquals(nameof(ucrBlock.ucrType), ucrType.D, ucrBlock.ucrType);
			});

			var ucrBlock1 = UCRHelper.ComposeUcrBlock("DUCR", ZString.Empty, ucrType.D);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(ucrBlock1.ucr), "DUCR", ucrBlock1.ucr);
				AssertEquals(nameof(ucrBlock1.ucrPartNo), null, ucrBlock1.ucrPartNo);
				AssertEquals(nameof(ucrBlock1.ucrType), ucrType.D, ucrBlock1.ucrType);
			});

			var ucrBlock2 = UCRHelper.ComposeUcrBlock("MUCR", ZString.Empty, ucrType.M);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(ucrBlock2.ucr), "MUCR", ucrBlock2.ucr);
				AssertEquals(nameof(ucrBlock2.ucrPartNo), null, ucrBlock2.ucrPartNo);
				AssertEquals(nameof(ucrBlock2.ucrType), ucrType.M, ucrBlock2.ucrType);
			});
		}

		public void TestUcrBlockToString()
		{
			var ucrBlock = new ucrBlock
			{
				ucr = "DUCR",
				ucrPartNo = "1",
				ucrType = ucrType.D,
			};
			var ucrBlock1 = new ucrBlock
			{
				ucr = "DUCR001",
				ucrType = ucrType.D,
			};
			var ucrBlock2 = new ucrBlock
			{
				ucr = "MUCR",
				ucrType = ucrType.M,
			};
			CombineAssertions(() =>
			{
				AssertEquals("DUCR/1", UCRHelper.UcrBlockToString(ucrBlock));
				AssertEquals("DUCR/1", UCRHelper.UcrBlockToString(ucrBlock.ucr, ucrBlock.ucrPartNo, ucrBlock.ucrType.ToString()));
				AssertEquals("DUCR001", UCRHelper.UcrBlockToString(ucrBlock1));
				AssertEquals("DUCR001", UCRHelper.UcrBlockToString(ucrBlock1.ucr, ucrBlock1.ucrPartNo, ucrBlock1.ucrType.ToString()));
				AssertEquals("MUCR", UCRHelper.UcrBlockToString(ucrBlock2));
				AssertEquals("MUCR", UCRHelper.UcrBlockToString(ucrBlock2.ucr, ucrBlock2.ucrPartNo, ucrBlock2.ucrType.ToString()));
			});
		}

		public void TestGetUCRKeyValuePairs()
		{
			AssertArrayEqualsByElements("Key Value Pairs", new (ZString, ZString)[]
			{
				("UCR", "AR1"),
				("UCR Part No", "999"),
				("UCR Type", "D")
			}, UCRHelper.GetUCRKeyValuePairs(UCRHelper.ComposeUcrBlock("AR1", "999", ucrType.D)).ToArray());
		}
	}
}
