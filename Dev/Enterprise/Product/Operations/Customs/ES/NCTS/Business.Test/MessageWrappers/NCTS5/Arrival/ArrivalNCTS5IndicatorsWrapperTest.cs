using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5IndicatorsWrapperTest : WrapperHelperTest<ArrivalNCTS5IndicatorsWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestGoodsDirectlyShipped()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_AutomaticTranshipment = true;
				AssertEquals("Expected filled GoodsDirectlyShipped true", "1", wrapper.GoodsDirectlyShipped);

				nctsHeader.ESNctsHeader.CEN_AutomaticTranshipment = false;
				AssertEquals("Expected filled GoodsDirectlyShipped false", "0", wrapper.GoodsDirectlyShipped);
			});
		}

		public void TestAutomaticCompletion()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_AutomaticCompletion = true;
				AssertEquals("Expected filled AutomaticCompletion true", "U", wrapper.AutomaticCompletion);

				nctsHeader.ESNctsHeader.CEN_AutomaticCompletion = false;
				AssertEquals("Expected filled AutomaticCompletion false", ZString.Empty, wrapper.AutomaticCompletion);
			});
		}

		public void TestTIRPageCompletion()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_TIRArrival = true;
				nctsHeader.ESNctsHeader.CEN_TIRCarnetPage = 10;
				AssertEquals("Expected filled TIRPageCompletion", "10", wrapper.TIRPageCompletion);

				nctsHeader.ESNctsHeader.CEN_TIRArrival = false;
				AssertEquals("Expected filled TIRPageCompletion empty when CEN_TIRArrival is false", ZString.Empty, wrapper.TIRPageCompletion);
			});
		}

		public void TestTIRParcialTotalUnloading()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_TIRArrival = true;
				nctsHeader.ESNctsHeader.CEN_TIRPartialUnloading = true;
				AssertEquals("Expected filled TIRParcialTotalUnloading true", "P", wrapper.TIRParcialTotalUnloading);

				nctsHeader.ESNctsHeader.CEN_TIRPartialUnloading = false;
				AssertEquals("Expected filled TIRParcialTotalUnloading false", "T", wrapper.TIRParcialTotalUnloading);

				nctsHeader.ESNctsHeader.CEN_TIRArrival = false;
				AssertEquals("Expected filled TIRParcialTotalUnloading empty when CEN_TIRArrival is false", ZString.Empty, wrapper.TIRParcialTotalUnloading);
			});
		}

		public void TestReceptionSummary()
		{
			nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = "AABBCCDD";
			AssertEquals("Expected filled ReceptionSummary", "AABBCCDD", wrapper.ReceptionSummary);
		}

		public void TestSummaryTypeIndicator()
		{
			nctsHeader.ESNctsHeader.CEN_SummaryType = SummaryTypeList.Codes.GenerateSummaryDeclarationOnReception;
			AssertEquals("Expected filled SummaryTypeIndicator", "SG", wrapper.SummaryTypeIndicator);
		}

		public void TestPreviousG4()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousG4 when no data declared", 0, wrapper.PreviousG4.Count);

				var doc1 = nctsHeader.ArrivalMovementHeader.G4PreviousDocuments.AddNew();
				doc1.CSI_ReferenceNumber = "mrn1";

				var doc2 = nctsHeader.ArrivalMovementHeader.G4PreviousDocuments.AddNew();
				doc2.CSI_ReferenceNumber = "mrn2";

				wrapper = GetWrapper(nctsHeader);
				var previousG4 = wrapper.PreviousG4;
				AssertEquals("Expected 2 PreviousG4 when declared", 2, previousG4.Count);
				AssertSame("Cached PreviousG4", wrapper.PreviousG4, previousG4);

				var previousG4Array = previousG4.ToArray();

				AssertEquals("For first PreviousG4 expected filled PreviousG4MRN", "mrn1", previousG4Array[0].PreviousG4MRN);
				AssertEquals("For first PreviousG4 expected filled SequenceNumber", "1", previousG4Array[0].SequenceNumber);

				AssertEquals("For second PreviousG4 expected filled PreviousG4MRN", "mrn2", previousG4Array[1].PreviousG4MRN);
				AssertEquals("For second PreviousG4 expected filled SequenceNumber", "2", previousG4Array[1].SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		ArrivalNCTS5IndicatorsWrapper wrapper;
		NctsHeader nctsHeader;

		ArrivalNCTS5IndicatorsWrapper GetWrapper(NctsHeader header) => new ArrivalNCTS5IndicatorsWrapper(header);

		protected override ArrivalNCTS5IndicatorsWrapper GetProvider() => wrapper;
	}
}
