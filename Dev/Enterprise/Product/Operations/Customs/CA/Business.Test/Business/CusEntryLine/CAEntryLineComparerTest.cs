using System.Collections;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAEntryLineComparerTest : TestCaseWithFactory
	{
		public void TestCompareSequenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLines = entry.AllEntryLines as AllCusEntryLineCollection<CusEntryLine>;
			CreateEntryLine(entryLines, 1, 1);
			CreateEntryLine(entryLines, 1, 2);
			CreateEntryLine(entryLines, 11, 1);
			CreateEntryLine(entryLines, 11, 2);
			CreateEntryLine(entryLines, 2, 1);
			CreateEntryLine(entryLines, 2, 2);

			const string expectedText =
				"[1,1]\n" +
				"[1,2]\n" +
				"[2,1]\n" +
				"[2,2]\n" +
				"[11,1]\n" +
				"[11,2]\n" +
				"";

			const string expectedTextInverted =
				"[11,2]\n" +
				"[11,1]\n" +
				"[2,2]\n" +
				"[2,1]\n" +
				"[1,2]\n" +
				"[1,1]\n" +
				"";

			entryLines.Sort((IComparer)new CAEntryLineComparer(false));
			AssertMultilineASCIIEquals("Normal", expectedText, OrderAsString(entryLines));

			entryLines.Sort((IComparer)new CAEntryLineComparer(true));
			AssertMultilineASCIIEquals("Inverted", expectedTextInverted, OrderAsString(entryLines));
		}

		void CreateEntryLine(AllCusEntryLineCollection<CusEntryLine> entryLines, ZShort goodsShipmentSequence, ZShort commoditySequence)
		{
			var line = entryLines.AddNew();
			line.CL_GoodsShipmentSequence = goodsShipmentSequence;
			line.CL_CommoditySequence = commoditySequence;
		}

		string OrderAsString(AllCusEntryLineCollection<CusEntryLine> entryLines)
		{
			var builder = new StringBuilder();

			foreach (var line in entryLines)
			{
				builder.AppendFormat("{0}\n", line.SequenceNumber);
			}

			return builder.ToString();
		}
	}
}
