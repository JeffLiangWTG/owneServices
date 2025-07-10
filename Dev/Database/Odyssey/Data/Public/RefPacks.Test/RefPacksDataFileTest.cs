using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefPacksDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefPacksDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRefPacksDataUpgrade()
		{
			var upgradeTask = new RefPacksUpgradeTask();
			upgradeTask.Run();

			var refPackDataFile = new RefPacksDataFile();
			var dataFromDb = refPackDataFile.LoadDataFromDatabase();
			var dataFromFile = refPackDataFile.LoadDataFromFile();

			AssertEquals(dataFromFile.Tables["RefPacks"].Rows.Count, dataFromDb.Tables["RefPacks"].Rows.Count);
			var vuRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "VU");
			AssertEquals("Added VU RefPacks makes a new total VU records", 40, vuRefPacks.Count());

			var sgRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "SG");
			AssertEquals("Added SG RefPacks makes a new total SG records", 32, sgRefPacks.Count());

			var mgRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "MG");
			AssertEquals("Added MG RefPacks makes a new total MG records", 38, mgRefPacks.Count());

			var cnRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "CN");
			AssertEquals("Added CN RefPacks makes a new total CN records", 45, cnRefPacks.Count());

			var twRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "TW");
			AssertEquals("Added TW RefPacks makes a new total TW records", 38, twRefPacks.Count());

			var uyRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "UY");
			AssertEquals("Added UY RefPacks makes a new total UY records", 11, uyRefPacks.Count());

			var mxRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "MX");
			AssertEquals("Added MX RefPacks makes a new total MX records", 31, mxRefPacks.Count());

			var clRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "CL");
			AssertEquals("Added CL RefPacks makes a new total CL records", 31, clRefPacks.Count());

			var arRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "AR");
			AssertEquals("Added AR RefPacks makes a new total AR records", 15, arRefPacks.Count());

			var ckRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "CK");
			AssertEquals("Added CK RefPacks makes a new total CK records", 22, ckRefPacks.Count());

			var coRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "CO");
			AssertEquals("Added CO RefPacks makes a new total CO records", 22, coRefPacks.Count());

			var nrRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "NR");
			AssertEquals("Added NR RefPacks makes a new total of Nauru records", 22, nrRefPacks.Count());

			var toRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "TO");
			AssertEquals("Added TO RefPacks makes a new total of Tonga records", 22, toRefPacks.Count());

			var brRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "BR");
			AssertEquals("Added BR RefPacks makes a new total of Brazilian records", 24, brRefPacks.Count());

			var peRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "PE");
			AssertEquals("Added PE RefPacks makes a new total of PE records", 27, peRefPacks.Count());

			var inRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "IN");
			AssertEquals("Added IN RefPacks makes a new total of IN records", 49, inRefPacks.Count());

			var euRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "EU");
			AssertEquals("Added EU RefPacks makes a new total of EU records", 52, euRefPacks.Count());

			var jpRefPacks = dataFromDb.Tables["RefPacks"].Rows.Cast<DataRow>().Where(x => x["RP_CustomsCountry"].ToString() == "JP");
			AssertEquals("Added JP RefPacks makes a new total of JP records", 22, jpRefPacks.Count());
		}
	}
}
