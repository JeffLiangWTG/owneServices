using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusMiscRequestLine))]
	sealed class CusMiscRequestLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetCusMiscRequestLineForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusMiscRequestLineForTest(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetCusMiscRequestLineForTest(Factory);

		CusMiscRequestLine GetCusMiscRequestLineForTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			header.CMR_RequestDate = ZDateTime.Today;
			header.CMR_CustomsOffice = "010";
			header.CMR_JobNumber = "1234567890123X";
			header.CMR_GB = GlbBranch.CurrentBranch.PK;
			var line = header.RequestLines.AddNew();
			line.CML_EntryType = "EXP";
			line.CML_EntryNumber = "12345";
			return line;
		}

		public void TestFormattedEntryNumber()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			header.CMR_RequestDate = ZDateTime.Today;
			header.CMR_CustomsOffice = "010";
			header.CMR_JobNumber = "1234567890123X";
			header.CMR_GB = GlbBranch.CurrentBranch.PK;

			var line1 = header.RequestLines.AddNew();
			line1.CML_EntryNumber = "NO";
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, line1.EntryType);

			var line2 = header.RequestLines.AddNew();
			line2.CML_EntryNumber = "6N00221000025X";
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, line2.EntryType);
			AssertEquals("6N002-21-000025X", line2.FormattedEntryNumber);

			var line3 = header.RequestLines.AddNew();
			line3.CML_EntryNumber = "13CSKAPH0123456";
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, line3.EntryType);
			AssertEquals("13CSKAPH012-3456", line3.FormattedEntryNumber);

			var line4 = header.RequestLines.AddNew();
			line4.CML_EntryNumber = "13CSKAPH01900100001";
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, line4.EntryType);
			AssertEquals("13CSKAPH019-0010-0001", line4.FormattedEntryNumber);
		}
	}
}
