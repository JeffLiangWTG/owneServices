using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperChargeTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperCharge()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateBill();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Gral);
			IDocCharges docCharges = wrapper.DocCharges.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(wrapper.DocCharges.IsCountEqualTo(1));

				AssertEquals((ZDecimal)150, docCharges.Amount);
				AssertEquals("USD", docCharges.Currency);
				AssertEquals(AWBSendChileConstants.PaymentType.P, docCharges.PaymentCondition);
			});
		}

		void CreateAndPopulateBill()
		{
			var bill = header.Bills.AddNew();
			bill.ABL_FreightValue = 150;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_PrepaidCollect = "PPD";
		}
	}
}
