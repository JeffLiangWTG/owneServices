using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class ImportJobComInvoiceLineValidationTest : BaseJobComInvoiceLineValidationTest
	{
		//		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine InvoiceLine)
		//		{
		//			return new ImportJobComInvoiceLineValidation(InvoiceLine);
		//		}

		public void TestInstrumentCode()
		{
			AssertEquals("InstrumentCode", ZString.Empty, testInvoiceLine.InstrumentCode);
			testInvoiceLine.InstrumentCode = "123";
			AssertEquals("InstrumentCode", "123", testInvoiceLine.InstrumentCode);
			testInvoiceLine.InstrumentType = "MD1";
			AssertEquals("InstrumentCode", "123", testInvoiceLine.InstrumentCode);
			testInvoiceLine.InstrumentType = "";
			AssertEquals("InstrumentCode", "123", testInvoiceLine.InstrumentCode);
			testInvoiceLine.InstrumentCode = "";
			AssertEquals("InstrumentCode", ZString.Empty, testInvoiceLine.InstrumentCode);
		}

		public void TestInstrumentType()
		{
			testInvoiceLine.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AssertEquals("InstrumentType", ZString.Empty, testInvoiceLine.InstrumentType);
			testInvoiceLine.InstrumentCode = "123";
			AssertEquals("InstrumentType", ZString.Empty, testInvoiceLine.InstrumentType);
			testInvoiceLine.InstrumentType = "MD1";
			AssertEquals("InstrumentType", "MD1", testInvoiceLine.InstrumentType);
			testInvoiceLine.InstrumentType = "";
			AssertEquals("InstrumentType", ZString.Empty, testInvoiceLine.InstrumentType);
			testInvoiceLine.InstrumentCode = "";
			AssertEquals("InstrumentType", ZString.Empty, testInvoiceLine.InstrumentType);

			testInvoiceLine.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testInvoiceLine.InstrumentType = "XX";//CMR code
			AssertEquals("Instrument type is valid", false, testInvoiceLine.InstrumentTypeInfo.HasMessageErrors());
		}

		public void TestJI_ConcessionOrder()
		{
			((INeedRow)testInvoiceLine).Row[JobComInvoiceLine.Schema.JI_ConcessionOrder] = "|";
			testInvoiceLine.Validation.ValidateJI_ConcessionOrder();
			AssertEquals("Message error should exist", true, testInvoiceLine.JI_ConcessionOrderInfo.HasMessageErrors());
		}

		public virtual void TestValidateDescriptionIsNotTooLong()
		{
			jobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testInvoiceLine.JI_Description = new string('a', 70);
			Assert("!HasMessageErrors", !testInvoiceLine.JI_DescriptionInfo.HasWarnings());
			testInvoiceLine.JI_Description = new string('a', 71);
			Assert("HasMessageErrors", testInvoiceLine.JI_DescriptionInfo.HasWarnings());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDec.JE_DateOfArrival = new ZDateTime(2004, 1, 1);
		}

		#endregion

	}
}
