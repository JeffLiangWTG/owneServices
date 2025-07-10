using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperIMOTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLSendChileWrapperIMO()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = FillOrgAddress("Carrier", "500").PK;

			CreateAndPopulateContainer();
			CreateAndPopulateHouseBill();

			AsycudaBill bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);
			IDocumentItem item = wrapper.Items.ElementAt(0);
			IIMO itemIMO = item.ItemsIMO.ElementAt(0);
			IItemContainer container = item.Containers.ElementAt(0);
			IIMO containerIMO = container.ContainersIMO.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("1.1A", itemIMO.Class);
				AssertEquals("0113", itemIMO.Number);

				AssertEquals("1.1A", containerIMO.Class);
				AssertEquals("0113", containerIMO.Number);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400";
			pack.APA_LineNo = 1;
			pack.APA_MarksAndNumbers = "S/I";
			pack.APA_PackQty = 1;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_Volume = 0.76;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			pack.APA_Weight = 170;
			pack.APA_WeightUQ = Core.Constants.Weight.Kilograms;
			pack.ContainerPK = header.Containers[0].PK;

			var dg = pack.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";
		}

		void CreateAndPopulateContainer()
		{
			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "22G0";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_EmptyFullIndicator = "FCL";
			container.ACN_GoodsWeight = 170;
			container.ACN_RC_ContainerType = refcontainer.PK;
		}

		OrgAddress FillOrgAddress(ZString orgType, ZString postcode)
		{
			var commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "96";
			commonOrgHeader.OH_FullName = "MEDITERRANEAN SHIPPING COMPANY CHILE S.A.";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "211110890017", Core.Constants.CountryCodes.Chile);

			OrgAddress orgAddress = commonOrgHeader.Addresses.AddNew();
			orgAddress.CompanyName = orgType;
			orgAddress.OA_Address1 = string.Concat(orgType, "Address1");
			orgAddress.OA_City = string.Concat(orgType, "City");
			orgAddress.OA_Email = string.Concat(orgType, "@wisetechglobal.com");
			orgAddress.OA_Phone = string.Concat(orgType, "Phone");
			orgAddress.OA_PostCode = postcode;

			return orgAddress;
		}
	}
}
