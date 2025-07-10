using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	sealed class GLHeaderCSVFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			Xsd.GLHeadersGLHeaderCollection valueObject = new Xsd.GLHeadersGLHeaderCollection();
			GLHeaderCSVFlatFileConverter converter = new GLHeaderCSVFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\GLHeader\Testing\ValidTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("4710.00.00", valueObject[0].AccNumber.ToString());
			AssertEquals("CR", valueObject[0].DebitCredit.ToString());
			AssertEquals("EXCHANGE DIFFERENCES", valueObject[0].Description.ToString());
			AssertEquals("PL", valueObject[0].AccountType.ToString());
			AssertEquals("8370.00.00", valueObject[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject[0].ConsolidationNum.ToString());

			AssertEquals("3150.00.00", valueObject[0].AlternateNum.ToString());
			AssertEquals("9799.00.00", valueObject[0].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(10), valueObject[0].TotalLevel);
			AssertEquals("Y", valueObject[0].ControlAccount.ToString());
			AssertEquals("N", valueObject[0].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(10), valueObject[0].PrintSequence);
			AssertEquals("TS", valueObject[0].Section);
			AssertEquals("SEG", valueObject[0].SubAccountType);
			AssertEquals("N", valueObject[0].IsSubAccountMandatory);
			AssertEquals("O01", valueObject[0].CashFlowType);
			AssertEquals("EDI, SIN", valueObject[0].CompanyFilterList);
			AssertEquals("KWH", valueObject[0].StatisticalUnits);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MoreThanOneGLHeaders()
		{
			Xsd.GLHeadersGLHeaderCollection valueObject = new Xsd.GLHeadersGLHeaderCollection();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLHeaderCSVFlatFileConverter converter = new GLHeaderCSVFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\GLHeader\Testing\Valid2GLHeaders.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(false, notificationBuffer.HasErrors);
			AssertEquals(2, valueObject.Count);

			AssertEquals("4710.00.00", valueObject[0].AccNumber.ToString());
			AssertEquals("CR", valueObject[0].DebitCredit.ToString());
			AssertEquals("EXCHANGE DIFFERENCES", valueObject[0].Description.ToString());
			AssertEquals("PL", valueObject[0].AccountType.ToString());
			AssertEquals("8370.00.00", valueObject[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject[0].ConsolidationNum.ToString());

			AssertEquals("3150.00.00", valueObject[0].AlternateNum.ToString());
			AssertEquals("9799.00.00", valueObject[0].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(10), valueObject[0].TotalLevel);
			AssertEquals("Y", valueObject[0].ControlAccount.ToString());
			AssertEquals("N", valueObject[0].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(10), valueObject[0].PrintSequence);
			AssertEquals("TS", valueObject[0].Section);
			AssertEquals("ORG", valueObject[0].SubAccountType.ToString());
			AssertEquals("Y", valueObject[0].IsSubAccountMandatory.ToString());
			AssertEquals("XXX", valueObject[0].CashFlowType.ToString());
			AssertEquals("EDI, SIN", valueObject[0].CompanyFilterList.ToString());
			AssertEquals("KG", valueObject[0].StatisticalUnits);

			AssertEquals("4110.00.00", valueObject[1].AccNumber.ToString());
			AssertEquals("DR", valueObject[1].DebitCredit.ToString());
			AssertEquals("PORT & TERMINAL REVENUE ACTUAL", valueObject[1].Description.ToString());
			AssertEquals("PL", valueObject[1].AccountType.ToString());
			AssertEquals("1370.00.00", valueObject[1].PercentNum.ToString());
			AssertEquals("1050.99.00", valueObject[1].ConsolidationNum.ToString());

			AssertEquals("3190.00.00", valueObject[1].AlternateNum.ToString());
			AssertEquals("1339.00.00", valueObject[1].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(15), valueObject[1].TotalLevel);
			AssertEquals("N", valueObject[1].ControlAccount.ToString());
			AssertEquals("Y", valueObject[1].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(15), valueObject[1].PrintSequence);
			AssertEquals("AS", valueObject[1].Section);
			AssertEquals("SGP", valueObject[1].SubAccountType.ToString());
			AssertEquals("N", valueObject[1].IsSubAccountMandatory.ToString());
			AssertEquals("F01", valueObject[1].CashFlowType.ToString());
			AssertEquals("EDI", valueObject[1].CompanyFilterList.ToString());
			AssertEquals("TON", valueObject[1].StatisticalUnits);
		}
	}
}
