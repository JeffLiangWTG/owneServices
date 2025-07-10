using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class InwardProcessingProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<InwardProcessingProcedureProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", InwardProcessingProcedureProvider.NeworNull(null));
		}

		public void TestReferencedSequenceNumber()
		{
			AssertEquals(3, dataProvider.ReferencedSequenceNumber);
		}

		public void TestRegistrationNumber()
		{
			AssertEquals("REFNUMBER", dataProvider.RegistrationNumber);
		}

		public void TestAccessViaAtlasFlag()
		{
			AssertEquals("1", dataProvider.AccessViaAtlasFlag);
		}

		public void TestGoodsRelatedInformation()
		{
			AssertEquals("GoodsRelatedInformation", dataProvider.GoodsRelatedInformation);
		}

		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				inwardProcessingProcedure.Status = false;
				AssertEquals("Status is false", string.Empty, dataProvider.MRN);
				inwardProcessingProcedure.Status = true;
				AssertEquals("Status is true", string.Empty, dataProvider.MRN);
				inwardProcessingProcedure.CSI_ReferenceNumber = "123456789012345678";
				AssertEquals("Status is true and length of CSI_ReferenceNumber is 18", "123456789012345678", dataProvider.MRN);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			inwardProcessingProcedure = Factory.New<PreviousDocument>();
			inwardProcessingProcedure.CSI_LineNo = 3;
			inwardProcessingProcedure.CSI_ReferenceNumber = "REFNUMBER";
			inwardProcessingProcedure.Status = true;
			inwardProcessingProcedure.CSI_Description = "GoodsRelatedInformation";
			dataProvider = InwardProcessingProcedureProvider.NeworNull(inwardProcessingProcedure);
		}
		PreviousDocument inwardProcessingProcedure;
		IInwardProcessingProcedure dataProvider;

		protected override InwardProcessingProcedureProvider GetProvider() => (InwardProcessingProcedureProvider)dataProvider;
	}
}
