using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperDateTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		[TestDate(2021, 03, 10, 12, 00, 00)]
		public void TestAWBSendChileWrapperDate()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			IAWBRequest wrapper = new AWBSendChileWrapper(bill, WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot);

			IDocDates date1 = wrapper.DocDates.ElementAt(0);
			IDocDates date2 = wrapper.DocDates.ElementAt(1);
			IDocDates date3 = wrapper.DocDates.ElementAt(2);
			IDocDates date4 = wrapper.DocDates.ElementAt(3);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(4, wrapper.DocDates.Count);

				AssertEquals(WrappersConstants.DateName.Fem, date1.Name);
				AssertEquals("01-02-2021", date1.Value);
				AssertEquals(WrappersConstants.DateName.Fzarpe, date2.Name);
				AssertEquals("03-02-2021 12:00", date2.Value);
				AssertEquals(WrappersConstants.DateName.Farribo, date3.Name);
				AssertEquals("03-03-2021 12:00", date3.Value);
				AssertEquals(WrappersConstants.DateName.Fpres, date4.Name);
				AssertEquals("10-03-2021 12:00", date4.Value);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_E_ARV = new ZDate(2021, 03, 03);
			header.AMA_E_DEP = new ZDate(2021, 02, 03);
			header.AMA_MasterBillIssueDate = new ZDate(2021, 02, 01);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
		}
	}
}
