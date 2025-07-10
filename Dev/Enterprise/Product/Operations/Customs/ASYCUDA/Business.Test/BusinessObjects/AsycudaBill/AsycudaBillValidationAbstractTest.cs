using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public abstract class AsycudaBillValidationAbstractTest : BusinessObjectValidationTestCase
	{
		public void TestBillIsLinkToSailing()
		{
			var message = "Bill is not linked to the same Sailing. It may have been incorrectly added.";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2018, 1, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2018, 9, 1);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			billOfLading.JS_NKLoadPort = "AUSYD";
			billOfLading.JS_JX = sailing.PK;
			billOfLading.JS_HouseBill = "AAA";

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BBB";
			header.ChangeSailing(sailing.PK);
			bill.Validation.ValidateABL_BillNumber();
			AssertHasWarning("Should contain as the Sailing is not null on header but the synchronisation target is null on bill.", bill.ABL_BillNumberInfo, message);

			bill.ABL_BillNumber = "AAA";
			AssertNoWarning("Should not contain as the synchronisation target is not null on bill.", bill.ABL_BillNumberInfo, message);

			header.ClearSailing(true);
			bill.Validation.ValidateABL_BillNumber();
			AssertNoWarning("Should not contain as the Sailing is null on header.", bill.ABL_BillNumberInfo, message);
		}
	}
}
