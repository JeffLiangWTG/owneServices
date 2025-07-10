using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public static class AsycudaManifestHeaderTestHelper
	{
		public static void EnsureOrCreateManifestTypesDataInZZ(string countryCode, BusinessObjectFactory factory, bool alsoMakeRecordForNotNvc = false)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode);

			var triplets = new List<Tuple<ZString, ZString, ZString>>() {   new Tuple<ZString, ZString, ZString>("COH", Core.Constants.TransportModes.Sea, universalAlias.RefCusCodeListTypes.Codes.NVC),  // these will come from constants
																			new Tuple<ZString, ZString, ZString>("HAB", Core.Constants.TransportModes.Air, universalAlias.RefCusCodeListTypes.Codes.NVC),
																			new Tuple<ZString, ZString, ZString>("RFM", Core.Constants.TransportModes.Road, universalAlias.RefCusCodeListTypes.Codes.NVC) };
			if (alsoMakeRecordForNotNvc)
			{
				triplets.Add("DJC", Core.Constants.TransportModes.Sea, "");
			}
		}

		public static ZGuid TwentyFootNOR => new ZGuid("7405F604-9665-4A55-890D-113A4671308D");

		public static ZGuid FortyFootGP => new ZGuid("D18DC96B-3D9E-4DD9-9955-6BCA550D1393");

		public static AsycudaManifestHeader CreateNicelyPopulatedImportManifestForTesting(ZString localPortCode, ZString manifestType, BusinessObjectFactory factory) => CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>(localPortCode, manifestType, factory);

		public static T CreateNicelyPopulatedImportManifestForTesting<T>(ZString localPortCode, ZString manifestType, BusinessObjectFactory factory, string applicationCode = null)
			where T : AsycudaManifestHeader
		{
			var localCountryCode = localPortCode.Left(2);
			var carrierAddress = new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB");
			var shipperAddress = new ZGuid("8048D3F8-B21A-4B6B-9798-8602A27843A2");
			var consigneeAddress = new ZGuid("29501FE8-0244-4315-96B3-2852E66D4054");
			var notifyAddress = new ZGuid("FD6DF968-EB23-498B-B7FC-86870BFE093D");
			var shippersAgentAddressPK = new ZGuid("CAB45208-6744-4E9F-AF3C-86993F548B99");
			var shippersAgentAddress = factory.Load<OrgAddress>(shippersAgentAddressPK);
			applicationCode = string.IsNullOrEmpty(applicationCode) ? ApplicationCodeTypeList.Codes.Consolidator : applicationCode;

			var header = (T)AsycudaManifestHeaderHelper.CreateNew(factory, localCountryCode, manifestType, applicationCode);
			header.FillWithValidTestData();
			header.AMA_RN_NKCountry = localCountryCode;
			header.AMA_Nature = "IMP";

			var consol = factory.New<ForwardingConsol>();
			header.SetParent(consol);

			header.AMA_E_ARV = ZDateTime.BrettsBirthday;
			header.AMA_E_DEP = ZDateTime.BrettsBirthday.AddDays(-1);
			header.AMA_JobReference = "C1234";
			header.AMA_MasterInformation = "ABC123";
			header.AMA_OA_Carrier = carrierAddress;
			header.Carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "1234", localCountryCode);
			header.Carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "IRRE", "XX");
			header.AMA_TransportMode = "AIR";
			header.AMA_RL_NKPortOfDischarge = localPortCode;
			header.AMA_RL_NKPortOfLoading = "USATL";
			header.AMA_VesselName = "Wrights' Flyer";
			header.AMA_Voyage = "BA123";
			header.AMA_RN_NKConveyanceNationality = "GB";

			header.AMA_OA_ShippingAgent = shippersAgentAddress.PK;

			header.ShippingAgent.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "1234", localCountryCode);

			header.Messages.Sort(EDIMessageSchema.EM_MessageNum.Name);

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HAWB1234";
			bill.ABL_CarrierReference = "BA ABC";
			bill.ABL_CustomsValue = 1m;
			bill.ABL_FreightValue = 2m;
			bill.ABL_GoodsDescription = "Books";
			bill.ABL_GrossWeight = 3m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			bill.ABL_InsuranceValue = 4m;
			bill.ABL_RL_NKFinalDestination = header.AMA_RL_NKPortOfDischarge;
			bill.ABL_RL_NKOrigin = header.AMA_RL_NKPortOfLoading;

			bill.ABL_LocationInformation = "LocX";
			bill.ABL_GoodsLocation = "Shed 1";
			bill.ABL_ManifestQty = 5;
			bill.ABL_ManifestUQ = "PK";
			bill.ABL_MarksAndNumbers = "Marks";
			bill.ABL_PrepaidCollect = "PPD";

			bill.ABL_Remarks = "Remarks";
			bill.ABL_OA_NotifyParty = notifyAddress;
			bill.ABL_OA_Shipper = shipperAddress;
			bill.Shipper.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "SHIP123", localCountryCode);
			bill.ABL_OA_Consignee = consigneeAddress;
			bill.Consignee.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "CONS456", localCountryCode);
			bill.ABL_RL_NKFinalDestination = header.AMA_RL_NKPortOfDischarge;
			bill.ABL_RL_NKOrigin = header.AMA_RL_NKPortOfLoading;
			bill.ABL_RX_NKFreightValueCurrency = "USD";
			bill.ABL_RX_NKCustomsValueCurrency = "NZD";
			bill.ABL_RX_NKInsuranceValueCurrency = "GBP";
			bill.ABL_RX_NKTransportValueCurrency = "HKD";
			bill.ABL_TransportValue = 6m;
			bill.ABL_Volume = 7m;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicFeet;

			var cont = header.Containers.AddNew();
			cont.ACN_ContainerNumber = "DANU1234567";
			cont.ACN_RC_ContainerType = AsycudaManifestHeaderTestHelper.TwentyFootNOR;
			cont.ACN_Seal1 = "S1";
			cont.ACN_Seal2 = "S2";
			cont.ACN_Seal3 = "S3";
			cont.ACN_SealingPartyName = "Daniel";
			cont.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			cont.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Terminal;
			cont.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Customs;
			cont.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
			cont.ACN_NumberOfPackages = 69;

			return header;
		}
	}
}
