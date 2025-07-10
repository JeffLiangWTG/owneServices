using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class CHGSLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			AssertEquals(DefaultExpectedLineAsString, Line.LineAsString);
		}

		public void TestLineAsStringWhenFieldsExceedMaxLength()
		{
			SetChargeCode("23456");
			// Restore values reset on setting ChargeCode
			JobCharge.JR_RX_NKSellCurrency = "USD";
			JobCharge.JR_OSSellAmt = 29.2M;
			JobCharge.JR_Desc = "12345678901234567890123456";
			AssertEquals(ExpectedLineAsStringWhenFieldsExceedMaxLength, Line.LineAsString);
		}

		public void TestPrepaidOrCollect()
		{
			JobCharge.JR_JH = ZGuid.Empty;
			AssertEquals("By default should be set to Collect when JobHeader is null", DefaultExpectedLineAsString, Line.LineAsString);
			JobCharge.JR_JH = JobHeader.PK;
			JobCharge.JR_OH_SellAccount = JobHeader.LocalChargesPK;
			// Restore values reset on setting SellAccount
			JobCharge.JR_RX_NKSellCurrency = "USD";
			JobCharge.JR_OSSellAmt = 29.2M;
			AssertEquals("Should be prepaid", DefaultExpectedLineAsString.Replace(";C;", ";P;"), Line.LineAsString);
			JobHeader.LocalChargesPK = ZGuid.Empty;
			AssertEquals("By default should be set to Collect when JobHeader.LocalCharges is null", DefaultExpectedLineAsString, Line.LineAsString);
		}

		#region Implementation
		protected override int ExpectedFieldCount
		{
			get
			{
				return 5;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "CHGS";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new CHGSLine(JobCharge);
		}

		#region Data for Test
		JobCharge JobCharge
		{
			get
			{
				if (fJobCharge == null)
				{
					fJobCharge = Factory.New<JobCharge>();
					fJobCharge.JR_JH = JobHeader.PK;
					SetDefaultValuesForJobCharge();
				}

				return fJobCharge;
			}
		}

		JobHeader JobHeader
		{
			get
			{
				if (fJobHeader == null)
				{
					fJobHeader = Factory.NewJobForTesting<JobHeader>();
					SetDefaultValuesForJobHeader();
				}

				return fJobHeader;
			}
		}

		void SetDefaultValuesForJobCharge()
		{
			SetChargeCode("ABCD");
			JobCharge.JR_OH_SellAccount = JobHeader.AgentCollectPK;
			JobCharge.JR_Desc = "ASDF 123";
			JobCharge.JR_RX_NKSellCurrency = "USD";
			JobCharge.JR_OSSellExRate = 1m;
			JobCharge.JR_OSSellAmt = 29.2M;
		}

		void SetDefaultValuesForJobHeader()
		{
			OrgHeader agentCollect = Factory.New<OrgHeader>();
			OrgHeader localCharges = Factory.New<OrgHeader>();
			localCharges.Addresses.AddNew(OrgAddressType.Office, true);
			JobHeader.AgentCollectPK = agentCollect.PK;
			JobHeader.LocalChargesPK = localCharges.PK;
		}

		void SetChargeCode(ZString chargeCode)
		{
			AccChargeCode accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = chargeCode;
			JobCharge.JR_AC = accChargeCode.PK;
		}

		JobHeader fJobHeader;
		JobCharge fJobCharge;
		#endregion
		#region Expected LineAsStrings
		const string DefaultExpectedLineAsString = "CHGS3100;ABCD;ASDF 123;29.2;C;USD";
		const string ExpectedLineAsStringWhenFieldsExceedMaxLength = "CHGS3100;2345;1234567890123456789012345;29.2;C;USD";
		#endregion
		#endregion
	}
}
