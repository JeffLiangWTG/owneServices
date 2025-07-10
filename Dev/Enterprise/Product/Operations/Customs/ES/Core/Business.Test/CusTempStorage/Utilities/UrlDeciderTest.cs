using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public class UrlDeciderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null TemporaryStorageHeader", () => new UrlDecider(null));
		}

		public void TestGetUrl()
		{
			var expectedMRN = "AH3RRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/CtrlG5Sede?op=detCab&mrn=" + expectedMRN;

			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
				header.MRN = expectedMRN;

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
				AssertEquals("The correct url has been launched with Customs Status = CLR and with MRN Number", expectedUrl, new UrlDecider(header).GetUrl());

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl;
				AssertEquals("The correct url has been launched with Customs Status = TUC and with MRN Number", expectedUrl, new UrlDecider(header).GetUrl());

				header.MRN = ZString.Empty;
				AssertNullOrEmpty("Empty because there is no MRN", new UrlDecider(header).GetUrl());
			});
		}
	}
}
