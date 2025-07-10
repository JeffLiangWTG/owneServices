using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.ZClientCCP.Kawasaki.Testing
{
	sealed class KawasakiInvoiceDataFileReaderTest : FileDataReaderTest
	{
		#region TestRecords
		public override void TestRecords()
		{
			AssertEquals(36, Reader.Records.Length);
		}

		#endregion
		#region TestAddRecordsForLine
		public void TestAddRecordsForLine()
		{
			AssertEquals("Number of file lines incorrect", 36, Reader.InternalFileLines.Count);
		}

		#endregion
		#region TestCreateInvoiceLine
		public void TestCreateInvoiceLine()
		{
			KawasakiInvoiceDataFileReaderTestClass reader = new KawasakiInvoiceDataFileReaderTestClass("");
			string[] testCSVLine = { new ZString("TAA464"), new ZString("552226"), new ZString("0001"), new ZString("1"), null, new ZString("56070-3876"), new ZString("0.70"), new ZString("1"), new ZString("LABEL-WARNING,DAMPER"), new ZString("USA"), new ZString("391990") };
			string[] testInvoiceLine = { new ZString("LINE"), null, new ZString("TAA464"), null, new ZString("56070-3876"), null, null, null, new ZString("LABEL-WARNING,DAMPER"), new ZString("1"), "NO", null, null, null, null, new ZString("0.70"), null, null, null, null, new ZString("US"), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
			string[] result = reader.CreateInvoiceLineFromBase(new OCsvLine(testCSVLine));
			for (int i = 0; i < result.Length; i++)
			{
				AssertEquals("The expected data was incorrect at position " + i, testInvoiceLine[i], result[i]);
			}
		}

		#endregion
		#region TestLinePrice
		public void TestLinePrice()
		{
			KawasakiInvoiceDataFileReaderTestClass reader = new KawasakiInvoiceDataFileReaderTestClass("");
			AssertEquals("Incorrect answer", "21.85", reader.LinePrice("9.5", "2.3"));
			AssertEquals("Incorrect answer", "25", reader.LinePrice("5", "5"));
			AssertEquals("Incorrect answer", "31639.133435", reader.LinePrice(" 500.263", " 63.245 "));
		}

		#endregion
		#region TestLookupSupplierPatternMatch
		public void TestLookupSupplierPatternMatch()
		{
			AssertEquals("Wrong country lookup found", "US", Reader.LookupSupplierPatternMatch("KAWMOTSYD", "USA"));
			AssertEquals("Should be String.Empty", String.Empty, Reader.LookupSupplierPatternMatch("KAWMOTSYD", "AUS"));
		}

		#endregion
		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader kawOrg = OrgHeader.New(factory);
			kawOrg.OH_RL_NKClosestPort = "AUSYD";
			kawOrg.MainAddress.OA_Address1 = "hahah st";
			kawOrg.OH_IsConsignee = true;
			kawOrg.OH_IsConsignor = true;
			kawOrg.OH_FullName = "KAWMOT";
			factory.Save();
			RefCountry unitedStates = RefCountry.LoadFromCountryCode(factory, "US");
			Match = factory.New<OrgPatternMatchOverride>();
			Match.OO_ForeignCode = "USA";
			Match.OO_OH = OrgHeader.LoadFromCode(factory, "KAWMOTSYD").PK;
			Match.OO_Relationship = "COU";
			Match.OO_LocalGuid = unitedStates.PK;
			factory.Save();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var path = resourceRetriever.SaveResourceToFile("Enterprise.Client.ZClientCCP.Testing.Kawasaki.TestFiles.KawasakiCustomsData.txt");
			Reader = new KawasakiInvoiceDataFileReaderTestClass(path);
		}

		EmbeddedResourceRetriever resourceRetriever;
		KawasakiInvoiceDataFileReaderTestClass Reader;
		OrgPatternMatchOverride Match;
		public class KawasakiInvoiceDataFileReaderTestClass : KawasakiInvoiceDataFileReader
		{
			public KawasakiInvoiceDataFileReaderTestClass(string fileName) : base(fileName)
			{
			}

			public void AddRecordsForLineFromBase(ArrayList recordsArrayOfStringArrays, OCsvLine csvLine)
			{
			}

			public string[] CreateInvoiceLineFromBase(OCsvLine invoiceLine)
			{
				return CreateInvoiceLine(invoiceLine);
			}

			public new string LookupSupplierPatternMatch(string org, string countryCode)
			{
				return base.LookupSupplierPatternMatch(org, countryCode);
			}

			public new ZString LinePrice(string price, string quantity)
			{
				return base.LinePrice(price, quantity);
			}
		}
		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
	}
}
