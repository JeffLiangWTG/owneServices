using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineFeeComparerTest : TestCaseWithFactory
{
	public void TestCompare()
	{
		var entryLine = Factory.New<CusEntryLine>();

		#region SetUp Fees in Alphabetical order

		entryLine.Fees.AddOrUpdate("110", 0m);
		entryLine.Fees.AddOrUpdate("110", 0m);
		entryLine.Fees.AddOrUpdate("116", 0m);
		entryLine.Fees.AddOrUpdate("120", 0m);
		entryLine.Fees.AddOrUpdate("121", 0m);
		entryLine.Fees.AddOrUpdate("122", 0m);
		entryLine.Fees.AddOrUpdate("125", 0m);
		entryLine.Fees.AddOrUpdate("127", 0m);
		entryLine.Fees.AddOrUpdate("128", 0m);
		entryLine.Fees.AddOrUpdate("131", 0m);
		entryLine.Fees.AddOrUpdate("138", 0m);
		entryLine.Fees.AddOrUpdate("142", 0m);
		entryLine.Fees.AddOrUpdate("144", 0m);
		entryLine.Fees.AddOrUpdate("145", 0m);
		entryLine.Fees.AddOrUpdate("148", 0m);
		entryLine.Fees.AddOrUpdate("149", 0m);
		entryLine.Fees.AddOrUpdate("150", 0m);
		entryLine.Fees.AddOrUpdate("152", 0m);
		entryLine.Fees.AddOrUpdate("155", 0m);
		entryLine.Fees.AddOrUpdate("156", 0m);
		entryLine.Fees.AddOrUpdate("160", 0m);
		entryLine.Fees.AddOrUpdate("165", 0m);
		entryLine.Fees.AddOrUpdate("171", 0m);
		entryLine.Fees.AddOrUpdate("172", 0m);
		entryLine.Fees.AddOrUpdate("201", 0m);
		entryLine.Fees.AddOrUpdate("270", 0m);
		entryLine.Fees.AddOrUpdate("275", 0m);
		entryLine.Fees.AddOrUpdate("276", 0m);
		entryLine.Fees.AddOrUpdate("277", 0m);
		entryLine.Fees.AddOrUpdate("301", 0m);
		entryLine.Fees.AddOrUpdate("307", 0m);
		entryLine.Fees.AddOrUpdate("405", 0m);
		entryLine.Fees.AddOrUpdate("406", 0m);
		entryLine.Fees.AddOrUpdate("407", 0m);
		entryLine.Fees.AddOrUpdate("423", 0m);
		entryLine.Fees.AddOrUpdate("515", 0m);
		entryLine.Fees.AddOrUpdate("516", 0m);
		entryLine.Fees.AddOrUpdate("552", 0m);
		entryLine.Fees.AddOrUpdate("553", 0m);
		entryLine.Fees.AddOrUpdate("556", 0m);
		entryLine.Fees.AddOrUpdate("703", 0m);
		entryLine.Fees.AddOrUpdate("901", 0m);
		entryLine.Fees.AddOrUpdate("904", 0m);
		entryLine.Fees.AddOrUpdate("905", 0m);
		entryLine.Fees.AddOrUpdate("909", 0m);
		entryLine.Fees.AddOrUpdate("910", 0m);
		entryLine.Fees.AddOrUpdate("911", 0m);
		entryLine.Fees.AddOrUpdate("912", 0m);
		entryLine.Fees.AddOrUpdate("913", 0m);
		entryLine.Fees.AddOrUpdate("914", 0m);
		entryLine.Fees.AddOrUpdate("915", 0m);
		entryLine.Fees.AddOrUpdate("916", 0m);
		entryLine.Fees.AddOrUpdate("917", 0m);
		entryLine.Fees.AddOrUpdate("921", 0m);
		entryLine.Fees.AddOrUpdate("931", 0m);
		entryLine.Fees.AddOrUpdate("933", 0m);
		entryLine.Fees.AddOrUpdate("934", 0m);
		entryLine.Fees.AddOrUpdate("935", 0m);
		entryLine.Fees.AddOrUpdate("936", 0m);
		entryLine.Fees.AddOrUpdate("937", 0m);
		entryLine.Fees.AddOrUpdate("A00", 0m);
		entryLine.Fees.AddOrUpdate("A20", 0m);
		entryLine.Fees.AddOrUpdate("A30", 0m);
		entryLine.Fees.AddOrUpdate("A35", 0m);
		entryLine.Fees.AddOrUpdate("A40", 0m);
		entryLine.Fees.AddOrUpdate("A45", 0m);
		entryLine.Fees.AddOrUpdate("B00", 0m);

		#endregion

		var orderedFees = entryLine.Fees.Cast<CusEntryLineFee>().OrderBy(x => x, new CusEntryLineFeeComparer());
		var feesInCustomsCompliantOrder = orderedFees.Select(x => x.CF_ChargeType).ToArray();

		#region Assertion for Customs Compliant order

		AssertArrayEqualsByElements("Fees are in customs compliant order",
			new ZString[]
			{   "270",
				"275",
				"276",
				"277",
				"A00",
				"A20",
				"A30",
				"A35",
				"A40",
				"A45",
				"110",
				"116",
				"120",
				"121",
				"122",
				"125",
				"127",
				"128",
				"131",
				"138",
				"142",
				"144",
				"145",
				"148",
				"149",
				"150",
				"152",
				"155",
				"156",
				"160",
				"165",
				"171",
				"172",
				"201",
				"301",
				"307",
				"423",
				"515",
				"516",
				"552",
				"553",
				"556",
				"703",
				"901",
				"904",
				"905",
				"909",
				"910",
				"911",
				"912",
				"913",
				"914",
				"915",
				"916",
				"917",
				"921",
				"931",
				"933",
				"934",
				"935",
				"936",
				"937",
				"B00",
				"405",
				"406",
				"407"
			}
		, feesInCustomsCompliantOrder);

		#endregion
	}
}
