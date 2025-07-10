using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Constants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageTestDataHelper
	{
		public static void SetUpBillDocumentTypeCodeList(BusinessObjectFactory factory)
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
			helper.CreateCusCodeType(Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "Transport document Type", Constants.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C624", "Form 302", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C625", "Rhine Manifest", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C664", "CN22 declaration according to Article 237 of the Regulation (ECC) No 2454/93", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C665", "CN23 declaration according to Article 237 of the Regulation (ECC) No 2454/93", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N703", "House waybill", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N704", "Master bill of lading", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N705", "Bill of lading", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N720", "Consignment bill of lading", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N722", "Road list – SMGS", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N730", "Road consignment note", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N740", "Air waybill", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N741", "Master air waybill", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N750", "Movement by post including parcel post", startDate, endDate);
			helper.CreateCusCodeList(Constants.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "N760", "Multi-model / combined transport document", startDate, endDate);
			factory.Save();
		}
	}
}
