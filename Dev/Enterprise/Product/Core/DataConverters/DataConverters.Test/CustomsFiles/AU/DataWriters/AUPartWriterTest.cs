using CargoWise.Types;
using Enterprise.DataConverters.Testing.DataWriters;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using PartWriter = Enterprise.DataConverters.CustomsFiles.AU.PartWriter;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU.DataWriters
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed internal class AUPartWriterTest : PartWriterTestBase
	{
		public void TestLine()
		{
			var expectedLine = new ZString("UNIQUEPART,Hopefully a Unique Part Code,KG,UNIQUELOOK,UNIQUELOOK,IMPORTER,SUPPLIER,,UNT");

			var writer = (PartWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			AssertEquals(expectedLine, writer.CSVOutputLine);
		}

		protected override void AdditionalCountrySpecificSetup(DataConverters.CustomsFiles.PartWriter part)
		{
		}

		protected override void AssertCountrySpecificData(DataConverters.CustomsFiles.PartWriter part, OrgSupplierPart bizO)
		{
			Assert(true);
		}

		protected override DataWriter GetNewDataWriter()
		{
			return new PartWriter(Factory);
		}
	}
}
