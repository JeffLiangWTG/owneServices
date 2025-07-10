using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class SailingSynchronisationTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2018, 1, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2018, 9, 1);

			voyage.GenerateSailings();
			sailing = voyage.Sailings[0];

			consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CR1";
			consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CE1";
			notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_Code = "NP1";

			fCLSailingBill = Factory.New<BillOfLading>();
			fCLSailingBill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			fCLSailingBill.JS_NKLoadPort = "AUSYD";
			fCLSailingBill.JS_JX = sailing.PK;
			fCLSailingBill.JS_HouseBill = "AAA";

			fCLSailingBill.JS_RL_NKOrigin = "AUSYD";
			fCLSailingBill.JS_RL_NKDestination = "SGSIN";

			fCLSailingBill.JS_INCO = "CLT";
			fCLSailingBill.JS_GoodsValue = 2018m;
			fCLSailingBill.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Singapore;

			fCLSailingBill.JS_MarksAndNumbers = "TEST MARKS ON FCLSailingBill";
			fCLSailingBill.JS_GoodsDescription = "TEST GOODS DESC ON FCLSailingBill";

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SL1";
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);

			fCLSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			fCLSailingBill.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			fCLSailingBill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			fCLSailingBill.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			rContainer = Factory.New<RefContainer>();

			sailingContainer = fCLSailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "ABCD1111";
			sailingContainer.JC_RC = rContainer.PK;
			sailingContainer.JC_SealNum = "123";
			sailingContainer.JC_AdditionalSealNum = "456";
			sailingContainer.JC_Additional2SealNum = "789";
			sailingContainer.JC_StowagePosition = "Darnassus";
			sailingContainer.JC_RH_NKContainerCommodityCode = "ABC";

			sailingPackLine = fCLSailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_HarmonisedCode = "01011000";
			sailingPackLine.JL_ActualWeight = 1000m;
			sailingPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			sailingPackLine.JL_F3_NKPackType = "PLT";
			sailingPackLine.JL_ActualVolume = 15m;
			sailingPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			sailingPackLine.JL_PackageCount = 11;
			sailingPackLine.JL_DetailedDescription = "DESCRIPTION";
			sailingPackLine.JL_MarksAndNumbers = "MARKS AND NUMBERS";

			sailingContainer.JC_GrossWeight = 982m;
			sailingContainer.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			rOROSailingBill = Factory.New<BillOfLading>();
			rOROSailingBill.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;

			rOROSailingBill.JS_NKLoadPort = "AUSYD";
			rOROSailingBill.JS_JX = sailing.PK;
			rOROSailingBill.JS_HouseBill = "BBB";
			rOROSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			rOROSailingBill.JS_ActualWeight = 1m;
			rOROSailingBill.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			rOROSailingBill.JS_ActualVolume = 2m;
			rOROSailingBill.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			rOROSailingBill.JS_OuterPacks = 3;
			rOROSailingBill.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;

			bulkSailingBill = Factory.New<BillOfLading>();
			bulkSailingBill.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			bulkSailingBill.JS_NKLoadPort = "AUSYD";
			bulkSailingBill.JS_JX = sailing.PK;
			bulkSailingBill.JS_HouseBill = "CCC";
			bulkSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			bulkSailingBill.JS_ActualWeight = 1m;
			bulkSailingBill.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			bulkSailingBill.JS_ActualVolume = 2m;
			bulkSailingBill.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			bulkSailingBill.JS_OuterPacks = 3;
			bulkSailingBill.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;

			topPack = bulkSailingBill.TopLevelPacks.AddNew();
			topPack.JC_HarmonisedCode = "01011002";
			topPack.JC_GrossWeight = 1m;
			topPack.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			topPack.JC_ContainerCount = 2;
			topPack.JC_Description = "TOP PACK";
			topPack.JC_MarksAndNumbers = "TOP PACK MAKRS & NUMBERS";
			topPack.JC_F3_NKPackType = Core.Constants.PkgUnit.Box;
			topPack.JC_GoodsValue = 100m;
			topPack.JC_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.MasterBill.ABL_BillNumber = "MASTER1";
			header.ChangeSailing(sailing.PK);
		}

		protected AsycudaManifestHeader header;
		protected JobVoyage voyage;
		protected JobSailing sailing;

		protected BillOfLading fCLSailingBill;
		protected BillOfLading rOROSailingBill;
		protected BillOfLading bulkSailingBill;

		protected OrgHeader consignor;
		protected OrgHeader consignee;
		protected OrgHeader notifyParty;

		protected RefContainer rContainer;
		protected BillOfLadingContainer sailingContainer;
		protected AgencyShipmentContainer topPack;
		protected BillOfLadingPackLine sailingPackLine;
	}
}
