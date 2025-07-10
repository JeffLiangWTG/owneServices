using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5DigitizedDocumentWrapperTest : WrapperHelperTest<TNNNCTS5DigitizedDocumentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if nctsHeaderDepartureTNN is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "nctsHeaderDepartureTNN"), () => new TNNNCTS5DigitizedDocumentWrapper(null, nctsArrivalMovementHeader, document, "A"));

				AssertExceptionThrown("Constructor Throws Exception if arrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "arrivalMovementHeader"), () => new TNNNCTS5DigitizedDocumentWrapper(nctsHeaderDepartureTNN, null, document, "A"));
			});
		}

		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				const string docName = "FileName";
				const string docExtension = "PDF";

				nctsHeaderDepartureTNN.TNNDocumentType = "Z";
				document.SC_FileName = docName;
				document.SC_DataType = docExtension;
				AssertEquals("Expected docName without extension when DocType != A", docName, wrapper.ReferenceNumber);

				nctsHeaderDepartureTNN.TNNDocumentType = "A";
				AssertEquals("Expected MRN when DocType is A", "MRN000AH3", wrapper.ReferenceNumber);
			});
		}

		public void TestDocumentType()
		{
			nctsHeaderDepartureTNN.TNNDocumentType = "4";
			AssertEquals("Expected filled DocumentType", "4", wrapper.DocumentType);
		}

		public void TestDocumentLocation()
		{
			CombineAssertions(() =>
			{
				nctsArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES00999900";
				AssertEquals("Expected filled DocumentLocation", "ES00999900", wrapper.DocumentLocation);

				nctsArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES00999900DECO";
				AssertEquals("Expected filled DocumentLocation trimmed when longer than 10", "999900DECO", wrapper.DocumentLocation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			document = docFactory.New<StorageDocs>();

			var mrnCode = "MRN000AH3";

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMrnFromUser = mrnCode;
			nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = true;
			nctsArrivalMovementHeader = nctsHeaderArrival.ArrivalMovementHeader;

			nctsHeaderDepartureTNN = Factory.New<NctsHeader>();
			nctsHeaderDepartureTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderDepartureTNN.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
			nctsHeaderDepartureTNN.MovementHeader.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			Factory.Save();

			wrapper = new TNNNCTS5DigitizedDocumentWrapper(nctsHeaderDepartureTNN, nctsArrivalMovementHeader, document, "description");
		}
		StorageDocs document;
		NctsArrivalMovementHeader nctsArrivalMovementHeader;
		NctsHeader nctsHeaderDepartureTNN;
		TNNNCTS5DigitizedDocumentWrapper wrapper;

		protected override TNNNCTS5DigitizedDocumentWrapper GetProvider() => throw new NotImplementedException();
	}
}
