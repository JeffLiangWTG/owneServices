using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDSegmentGroup30BuilderTest : TestCaseWithFactory
	{
		public void TestPopulateSegment_ZeroAmounts()
		{
			TestValues(
				1,
				"3406.00.00",
				"1",
				"DESCRIPTION",
				10,
				10,
				"KG",
				"12345",
				"AU",
				"NSW",
				0,
				"0"
				);
		}

		public void TestPopulateSegment_MultiplePermitNumberAndAmounts()
		{
			TestValues(
				1,
				"3406.00.00",
				"1",
				"DESCRIPTION",
				10,
				10,
				"KG",
				"12345,54321",
				"AU",
				"NSW",
				0.4,
				"1"
				);
		}

		public void TestPopulateSegment_NotAUState()
		{
			TestValues(
				1,
				"3406.00.00",
				"1",
				"DESCRIPTION",
				10,
				10,
				"KG",
				"12345,54321",
				"NZ",
				"YY-FO",
				1.6,
				"2"
				);
		}

		void TestValues(short lineNumber, string tariffRefString, string tempImportNumber, string description, int totalWeight, int invoiceQuantity, string unitOfQuantityString, string permitNumber, string countryOfOriginString, string aUState, ZDecimal invoiceLineAmount, string expectedFOBAmount)
		{
			var group30 = new SegmentGroup30();

			var jobComInvLine = GetTestJobComInvLine(
				lineNumber,
				tariffRefString,
				tempImportNumber,
				description,
				totalWeight,
				invoiceQuantity,
				unitOfQuantityString,
				permitNumber,
				countryOfOriginString,
				aUState,
				invoiceLineAmount
				);

			var myBuilder = new EXDSegmentGroup30Builder(jobComInvLine, group30, 0);
			myBuilder.PopulateSegment();

			var permitNumberPart = "";
			foreach (var permitNum in permitNumber.Split(','))
			{
				permitNumberPart += "RFF+EP:" + permitNum + "'";
			}

			var expectedString = "CST+" + lineNumber + "+I::95'FTX+AAA+++" + description + "'LOC+27+" + (countryOfOriginString != "AU" ? (countryOfOriginString + "::5+" + "YY-FO") : ("+" + GetState(aUState))) + "::6'MEA+WT++KG:" + totalWeight + "'MEA+ABW++" + unitOfQuantityString + ":" + invoiceQuantity + "'MOA+63:" + expectedFOBAmount + "'RFF+HS:34060000'" + permitNumberPart + "RFF+AGM:" + tempImportNumber + "'";
			AssertEquals("SegmentValue", expectedString, group30.ToString(new UNOCCMRCharacterSet()));
		}

		string GetState(string aUState)
		{
			switch (aUState)
			{
				case "ACT":
					return "AU-CT";
				case "NSW":
					return "AU-NS";
				case "QLD":
					return "AU-QL";
				case "SA":
					return "AU-SA";
				case "VIC":
					return "AU-VI";
				case "WA":
					return "AU-WA";
				case "TAS":
					return "AU-TS";
				case "NT":
					return "AU-NT";
				default:
					return aUState;
			}
		}

		JobComInvoiceLine GetTestJobComInvLine(short lineNumber, string tariff, string tempImportNumber, string description, int totalWeight, int invoiceQuantity, string unitOfQuantity, string permitNumber, ZString countryOfOrigin, string aUState, ZDecimal invoiceLineAmount)
		{
			var factory = new BusinessObjectFactory();

			var jobDec = factory.New<JobDeclaration>();
			var jobComInvHeader = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			jobComInvHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var jobComInvLine = jobComInvHeader.JobComInvoiceLines.AddNew();

			jobComInvLine.JI_LineNo = lineNumber;
			jobComInvLine.JI_Tariff = tariff;
			jobComInvLine.JI_TempImportNum = tempImportNumber;
			jobComInvLine.JI_Description = description;
			jobComInvLine.JI_Weight = totalWeight;
			jobComInvLine.JI_CustomsQuantity = invoiceQuantity;
			jobComInvLine.JI_CustomsUnitQty = unitOfQuantity;

			jobComInvLine.AddInfo.ZA_PermitNumbers_Hidden = permitNumber;
			jobComInvLine.JI_CountryOfOrigin = countryOfOrigin;
			jobComInvLine.JI_AUState = aUState;
			jobComInvLine.JI_LinePrice = invoiceLineAmount;
			return jobComInvLine;
		}
	}
}
