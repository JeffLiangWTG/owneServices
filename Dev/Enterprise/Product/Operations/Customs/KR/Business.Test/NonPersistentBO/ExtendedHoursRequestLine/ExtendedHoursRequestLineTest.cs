using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendedHoursRequestLine))]
	sealed class ExtendedHoursRequestLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ExtendedHoursRequestLine(new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK));

		public void TestExportFormattedReferenceNumber()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var lineEXP = header.ExtendedHoursRequestLines.AddNew();
			lineEXP.ReferenceNumberType = ReferenceNumberTypeList.Codes.EXP;
			lineEXP.ReferenceNumber = "2292620082822M";
			var lineOther = header.ExtendedHoursRequestLines.AddNew();
			lineOther.ReferenceNumberType = "XXX";
			lineOther.ReferenceNumber = "2292620081234M";

			AssertEquals("22926-20-082822M", lineEXP.FormattedReferenceNumber);
			AssertEquals("2292620081234M", lineOther.FormattedReferenceNumber);

			lineEXP.FormattedReferenceNumber = "12345-20-000045M";
			AssertEquals("1234520000045M", lineEXP.ReferenceNumber);
		}

		public void TestImportFormattedReferenceNumber()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var lineIMP = header.ExtendedHoursRequestLines.AddNew();
			AssertEquals("When empty, should be IMP as it is a majority of the cases", ReferenceNumberTypeList.Codes.IMP, lineIMP.CustomsEntryType);

			lineIMP.ReferenceNumber = "NO";
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, lineIMP.CustomsEntryType);

			lineIMP.ReferenceNumber = "2292620082822M";
			AssertEquals(14, lineIMP.ReferenceNumber.Length);
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, lineIMP.CustomsEntryType);

			var lineCMN1 = header.ExtendedHoursRequestLines.AddNew();
			lineCMN1.ReferenceNumber = "13CSKAPH0190010";
			AssertNotEquals(14, lineCMN1.ReferenceNumber.Length);
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, lineCMN1.CustomsEntryType);

			var lineCMN2 = header.ExtendedHoursRequestLines.AddNew();
			lineCMN2.ReferenceNumber = "13CSKAPH01900100001";
			AssertNotEquals(14, lineCMN2.ReferenceNumber.Length);
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, lineCMN2.CustomsEntryType);

			AssertEquals("22926-20-082822M", lineIMP.FormattedReferenceNumber);
			AssertEquals("13CSKAPH019-0010", lineCMN1.FormattedReferenceNumber);
			AssertEquals("13CSKAPH019-0010-0001", lineCMN2.FormattedReferenceNumber);

			lineIMP.FormattedReferenceNumber = "12345-20-000045M";
			lineCMN1.FormattedReferenceNumber = "13CSKAPH019-9999";
			lineCMN2.FormattedReferenceNumber = "13CSKAPH019-0010-9999";

			AssertEquals("1234520000045M", lineIMP.ReferenceNumber);
			AssertEquals("13CSKAPH0199999", lineCMN1.ReferenceNumber);
			AssertEquals("13CSKAPH01900109999", lineCMN2.ReferenceNumber);
		}

		public void TestDefaultValuesWhen5AC()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var line = header.ExtendedHoursRequestLines.AddNew();

			AssertEquals(ReferenceNumberTypeList.Codes.EXP, line.ReferenceNumberType);
			AssertEquals(Core.Constants.Weight.Kilograms, line.UQ);
		}

		public void TestReferenceNumberTypeDefaultValueWhen5AC()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			header.CustomsOffice = "010";
			header.Department = "10";
			header.BranchPK = GlbBranch.CurrentBranch.PK;
			var line = header.ExtendedHoursRequestLines.AddNew();
			line.ReferenceNumber = "13CSKAPH0190010";

			AssertEquals(ReferenceNumberTypeList.Codes.EXP, line.ReferenceNumberType);

			var aMiscHeader = new CusMiscRequestHeaderCreator().Create(header);
			AssertNoExceptionThrown("A Misc Header created from NP ExtendedHoursRequestHeader should be able to be saved", () => Factory.Save());
			Assert(aMiscHeader.IsInDatabase);
		}
	}
}
