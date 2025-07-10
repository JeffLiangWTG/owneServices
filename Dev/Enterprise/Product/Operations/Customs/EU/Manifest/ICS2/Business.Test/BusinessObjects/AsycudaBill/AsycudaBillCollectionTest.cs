using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaBillCollection))]
	public class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew() => CombineAssertions(() =>
		{
			AssertEquals("Precondition: ShouldSynchroniseWithConsol=false when no consol", false, header.ShouldSynchroniseWithConsol);
			AssertEquals("When ShouldSynchroniseWithConsol=false for no consol", true, header.Bills.AllowNew);
			header.SetParent(Factory.New<ForwardingConsol>());
			AssertEquals("Precondition: ShouldSynchroniseWithConsol=true", true, header.ShouldSynchroniseWithConsol);
			AssertEquals("When ShouldSynchroniseWithConsol=true", false, header.Bills.AllowNew);
			header.AMA_OverrideFreightDefaults = true;
			AssertEquals("Precondition: ShouldSynchroniseWithConsol=false when AMA_OverrideFreightDefaults = true", false, header.ShouldSynchroniseWithConsol);
			AssertEquals("When ShouldSynchroniseWithConsol=false for AMA_OverrideFreightDefaults = true", true, header.Bills.AllowNew);
		});

		public void TestDefaultTransportDocumentType_ForNewChild()
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F20;

			var bill = header.Bills.AddNew();
			AssertEquals(ASYCUDA.Business.TransportDocumentTypes.Codes.CL754_N703, bill.TransportDocumentType);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => header.Bills;

		public void TestMaxCountValidation()
		{
			var collection = header.Bills;
			var maxCountValidator = ((ISupportMaxCountValidation)collection).MaxCountValidator;

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
			AssertMaxCountIs1(EUICS2SpecificCircumstanceList.Codes.F15);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			AssertMaxCountIs1(EUICS2SpecificCircumstanceList.Codes.F16);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F17;
			AssertMaxCountIs1(EUICS2SpecificCircumstanceList.Codes.F17);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F25;
			AssertMaxCountIs1(EUICS2SpecificCircumstanceList.Codes.F25);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			CombineAssertions("SpecificCircumstanceIndicator = F40", () =>
			{
				AssertEquals(99, collection.MaxCount);
				AssertEquals("You may enter a maximum of 99 Bills.", maxCountValidator.Notification.Message);
				AssertEquals(NotificationType.Error, maxCountValidator.Notification.Type);
			});

			void AssertMaxCountIs1(ZString specificCircumstance)
			{
				CombineAssertions(string.Format("SpecificCircumstanceIndicator = {0}", specificCircumstance), () =>
				{
					AssertEquals(1, collection.MaxCount);
					AssertEquals(string.Format("For Specific Circumstance '{0}' only one house bill is allowed.", specificCircumstance), maxCountValidator.Notification.Message);
					AssertEquals(NotificationType.Error, maxCountValidator.Notification.Type);
				});
			}
		}

		public void TestDefaultPostalCharges_ForNewChild()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			var bill = header.Bills.AddNew();
			AssertEquals(0.00m, bill.ABL_FreightValue);
			AssertNullOrEmpty(bill.ABL_RX_NKFreightValueCurrency);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}
		AsycudaManifestHeader header;
	}
}
