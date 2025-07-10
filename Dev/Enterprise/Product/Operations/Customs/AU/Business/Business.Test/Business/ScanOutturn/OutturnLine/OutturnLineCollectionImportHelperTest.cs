using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OutturnLineCollectionImportHelperTest : TestCaseWithFactory
	{
		public void TestExportToScanner()
		{
			using (var tempFile = TempFile.New())
			{
				using (var writer = new StreamWriter(tempFile.Filename))
				{
					writer.Write(@"
CargoWise ScanCheckManifest,v1.0, Total Consignments:,3
ConsignmentRef,Status,ScannedDateTime,Count
Rederence1,Held,01-May-12 21:53:00,1
Rederence2,Clear,,2
Rederence3,Held,,3
".Trim());
					writer.Flush();
				}

				var collection = new AirOutturnLineCollection(Factory);
				var helper = new OutturnLineCollectionImportHelper(collection);
				helper.ImportFromScanner(tempFile.Filename);

				AssertEquals(3, collection.Count);
				AssertEquals("Rederence1", collection[0].ConsignmentRef);
				AssertEquals("Held", collection[0].Status);
				AssertEquals(new ZDateTime(2012, 5, 1, 21, 53, 00), collection[0].ScannedDateTime);
				AssertEquals(1, collection[0].Count);

				AssertEquals("Rederence2", collection[1].ConsignmentRef);
				AssertEquals("Clear", collection[1].Status);
				AssertEquals(2, collection[1].Count);

				AssertEquals("Rederence3", collection[2].ConsignmentRef);
				AssertEquals("Held", collection[2].Status);
				AssertEquals(3, collection[2].Count);
			}
		}
	}
}
