using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC231C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC231CProviderTest : TestCaseWithFactory
	{
		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			AssertEquals("HolderIdentificationNumber", "HTPID123456", provider.HolderIdentificationNumber);
		}

		public void TestGRN()
		{
			AssertEquals("GRN", "12GRNCC055C012345A678901", provider.GRN);
		}

		public void TestInvalidityDate()
		{
			AssertEquals("Guarantee InvalidityDate", new ZDateTime(2023, 6, 20), provider.InvalidityDate);
		}

		public void TestInvalidityReasonText()
		{
			AssertEquals("Guarantee InvalidityReasonText", "Invalidity Reason Text", provider.InvalidityReasonText);
		}

		public void TestInvalidityReasonCode()
		{
			AssertEquals("Guarantee InvalidityReasonCode", "Invalidity Reason Code", provider.InvalidityReasonCode);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			AssertEquals("Office of Guarantee", "IEDUB100", provider.CustomsOfficeOfGuarantee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC231CProvider(new Cc231CType
			{
				GuaranteeReference = new GuaranteeReferenceType12
				{
					Grn = "12GRNCC055C012345A678901",
					InvalidityDate = new DateTime(2023, 6, 20),
					InvalidityReasonCode = "Invalidity Reason Code",
					InvalidityReasonText = "Invalidity Reason Text",
					CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
					{
						ReferenceNumber = "IEDUB100"
					},
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType01
				{
					IdentificationNumber = "HTPID123456"
				},
			});
		}
		CC231CProvider provider;
	}
}
