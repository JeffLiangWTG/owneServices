using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Testing
{
	class EDIInterchangeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_Default()
		{
			var dataRow = ((INeedRow)Factory.New<EDIInterchange>()).Row;

			AssertIsEDIInterchange(EDIInterchange.Direction.Receive, EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			AssertIsEDIInterchange(EDIInterchange.Direction.Transmit, EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ExportOriginal);
			AssertIsEDIInterchange(EDIInterchange.Direction.Transmit, EDIInterchange.ApplicationCodes.IECustomsUCC5Import, "ZZZ");

			void AssertIsEDIInterchange(string receiveTransmit, string applicationCode, string interchangeType)
			{
				dataRow[EDIInterchangeSchema.Constants.EI_ReceiveTransmit] = receiveTransmit;
				dataRow[EDIInterchangeSchema.Constants.EI_ApplicationCode] = applicationCode;
				dataRow[EDIInterchangeSchema.Constants.EI_InterchangeType] = interchangeType;
				AssertEquals($"EI_ReceiveTransmit {receiveTransmit}, EI_ApplicationCode {applicationCode}, EI_InterchangeType {interchangeType}", typeof(EDIInterchange), typeDecider.GetTypeForLoad(dataRow, Factory));
			}
		}

		public void TestGetTypeForLoad_AESOutbound()
		{
			var dataRow = ((INeedRow)Factory.New<EDIInterchange>()).Row;
			dataRow[EDIInterchangeSchema.Constants.EI_ReceiveTransmit] = EDIInterchange.Direction.Transmit;
			dataRow[EDIInterchangeSchema.Constants.EI_ApplicationCode] = EDIInterchange.ApplicationCodes.IECustomsExport;
			dataRow[EDIInterchangeSchema.Constants.EI_InterchangeType] = AESOutgoingMessageTypeList.Codes.DocumentUpload;
			AssertEquals("EI_ReceiveTransmit TRX, EI_ApplicationCode IEE", typeof(AESOutboundMessageDataProviderEDIInterchange), typeDecider.GetTypeForLoad(dataRow, Factory));
		}

		public void TestGetTypeForLoad_AISUCC5Outbound()
		{
			var dataRow = ((INeedRow)Factory.New<EDIInterchange>()).Row;
			dataRow[EDIInterchangeSchema.Constants.EI_ReceiveTransmit] = EDIInterchange.Direction.Transmit;
			dataRow[EDIInterchangeSchema.Constants.EI_ApplicationCode] = EDIInterchange.ApplicationCodes.IECustomsUCC5Import;
			dataRow[EDIInterchangeSchema.Constants.EI_InterchangeType] = AISUploadDocumentsMessageTypeList.Codes.IM483;
			AssertEquals("EI_ReceiveTransmit TRX, EI_ApplicationCode IE5", typeof(AISUCC5OutboundMessageDataProviderEDIInterchange), typeDecider.GetTypeForLoad(dataRow, Factory));
		}

		public void TestGetTypeForBinding()
		{
			AssertNull("GetTypeForBinding should return NULL.", typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull("GetTypeForNew should return NULL.", typeDecider.GetTypeForNew());
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIInterchangeTypeDecider();
		}

		EDIInterchangeTypeDecider typeDecider;
	}
}
