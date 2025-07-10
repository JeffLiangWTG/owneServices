using CargoWise.Types;
using Enterprise.DataConverters.Testing.Base;

namespace Enterprise.DataConverters.Testing.DataWriters
{
	sealed internal class VesselWriterTest : DataWriterTestCase
	{
		protected override void FillInRecordWithInvalidDetails(DataWriter writer)
		{
			var vesselWriter = (VesselDataWriter)writer;
			vesselWriter.VesselCode = ZString.Empty;
		}

		protected override void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer)
		{
			var vesselWriter = (VesselDataWriter)writer;
			vesselWriter.VesselCode = "Name";
			vesselWriter.VesselLloydsNo = "123456";
		}

		public override void TestAllFieldsInRecordAreImportedProperly()
		{
			var writer = (VesselDataWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);

			AssertEquals("Name", writer.VesselCode);
			AssertEquals("123456", writer.VesselLloydsNo);
		}

		public void TestRecordDescription()
		{
			var writer = (VesselDataWriter)GetNewDataWriter();
			writer.VesselCode = "BUNGA BIDARA";
			AssertEquals("Vessel: BUNGA BIDARA", writer.RecordDescription);
		}

		protected override DataWriter GetNewDataWriter()
		{
			return new VesselDataWriter(Factory);
		}
	}
}
