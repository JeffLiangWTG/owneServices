using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromNCTS))]
	sealed class FreightWrapperFromNCTSTest : FreightWrapperTest
	{
		public void TestWrapperMapping()
		{
			#region Setup

			SetupCustomsOffices();

			var (departureHeaderPhase4, arrivalHeaderPhase4) = SetUpNctsHeaders(CusInBondApplicationCodeList.Codes.NCTS4);
			var (departureHeaderPhase5, arrivalHeaderPhase5) = SetUpNctsHeaders(CusInBondApplicationCodeList.Codes.NCTS5);

			#endregion

			AssertFreightWrapperFromNCTS(departureHeaderPhase4, arrivalHeaderPhase4);
			AssertFreightWrapperFromNCTS(departureHeaderPhase5, arrivalHeaderPhase5);

			(NctsHeader, NctsHeader) SetUpNctsHeaders(ZString appCode)
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = appCode;
				nctsHeader.SetMovementType("D");
				var entryNum = Factory.New<CusEntryNumber>();
				entryNum.CE_ParentID = nctsHeader.PK;
				entryNum.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
				entryNum.CE_EntryNum = "MRN123";
				entryNum.CE_EntryType = "MRN";

				nctsHeader.Consignor.E2_OA_Address = GetOrgHeader("IMPORTER").MainAddress.PK;
				nctsHeader.Consignee.E2_OA_Address = GetOrgHeader("SUPPLIER").MainAddress.PK;

				nctsHeader.MovementHeader.BM_InBondEntryType = "T1";

				nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
				nctsHeader.MovementHeader.BM_TransportAtDeparture = "AAA";
				nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "GB";

				nctsHeader.MovementHeader.BM_ExportTransportMode = "2";
				nctsHeader.MovementHeader.BM_TOLCarrierID = "BBB";
				nctsHeader.MovementHeader.BM_TOLCarrierCode = "FR";

				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "CCC";
				nctsHeader.MovementHeader.BM_LocationOfGoods = "DDD";

				nctsHeader.BH_JobReference = "NCT00001234";

				var departureOffice = (appCode == CusInBondApplicationCodeList.Codes.NCTS4 ? nctsHeader.CustomsOffices : nctsHeader.MovementHeader.CustomsOffices).Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
				departureOffice.CY_Data = "GB000001";

				var destinationOffice = (appCode == CusInBondApplicationCodeList.Codes.NCTS4 ? nctsHeader.CustomsOffices : nctsHeader.MovementHeader.CustomsOffices).Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
				destinationOffice.CY_Data = "FR000002";

				var arrivalHeader = Factory.New<NctsHeader>();
				arrivalHeader.BH_ApplicationCode = appCode;
				arrivalHeader.SetMovementType("A");
				var arrivalEntryNum = Factory.New<CusEntryNumber>();
				arrivalEntryNum.CE_ParentID = arrivalHeader.PK;
				arrivalEntryNum.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
				arrivalHeader.ArrivalMovementHeader.BM_InBondEntryType = "T2";
				arrivalHeader.ArrivalMovementHeader.BM_TransportAtDeparture = "TAD";
				arrivalHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "PT";
				arrivalHeader.ArrivalMovementHeader.BM_InlandTransportMode = "1";
				arrivalHeader.ArrivalMovementHeader.BM_TOLCarrierID = "AAA";
				arrivalHeader.ArrivalMovementHeader.BM_TOLCarrierCode = "GB";
				arrivalHeader.ArrivalMovementHeader.BM_ExportTransportMode = "2";
				arrivalHeader.ArrivalMovementHeader.BM_PlaceOfUnloading = "ABC";

				return (nctsHeader, arrivalHeader);
			}

			void AssertFreightWrapperFromNCTS(NctsHeader nctsDepartureHeader, NctsHeader arrivalHeader)
			{
				CombineAssertions(nctsDepartureHeader.IsPhase5 ? "Phase5" : "Phase4", () =>
				{
					var expectedDepartureDeclarationType = nctsDepartureHeader.IsPhase5 ? "T1 - Goods not having the customs status of Union goods, which are placed under the common transit procedure" : "T1 - T1 Desc";

					FreightWrapperFromNCTS fullWrapper = FreightWrapperFromNCTS.New(nctsDepartureHeader, Factory);
					AssertType<NctsDepartureMovementHeader>("MovementHeader Is NctsDepartureMovementHeader", fullWrapper.MovementHeader);
					AssertEquals("fullWrapper.CustomsEntryNumber", "MRN123", fullWrapper.CustomsEntryNumber);
					AssertEquals("fullWrapper.Consignee.CompanyName", "SUPPLIER", fullWrapper.Consignee.CompanyName);
					AssertEquals("fullWrapper.Consignor.CompanyName", "IMPORTER", fullWrapper.Consignor.CompanyName);
					AssertEquals("fullWrapper.NCTSDeclarationType", expectedDepartureDeclarationType, fullWrapper.NCTSDeclarationType.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSDepartureTransportID", "AAA", fullWrapper.NCTSDepartureTransportID);
					AssertEquals("fullWrapper.NCTSDepartureTransportCountry.", "GB - United Kingdom", fullWrapper.NCTSDepartureTransportCountry.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSDepartureTransportMode.", "1 - Sea Transport", fullWrapper.NCTSDepartureTransportMode.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSFrontierTransportID.", "BBB", fullWrapper.NCTSFrontierTransportID);
					AssertEquals("fullWrapper.NCTSFrontierTransportCountry.", "FR - France", fullWrapper.NCTSFrontierTransportCountry.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSFrontierTransportMode.", "2 - Rail Transport", fullWrapper.NCTSFrontierTransportMode.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSGoodsLocationCode", "CCC", fullWrapper.NCTSGoodsLocationCode);
					AssertEquals("fullWrapper.NCTSGoodsLocation", "DDD", fullWrapper.NCTSGoodsLocation);
					AssertEquals("fullWrapper.NCTSDepartureOffice", "GB000001 - BRITISH OFFICE Of Departure", fullWrapper.NCTSDepartureOffice.CodeAndDescription);
					AssertEquals("fullWrapper.NCTSDestinationOffice", "FR000002 - FRENCH OFFICE Of Destination", fullWrapper.NCTSDestinationOffice.CodeAndDescription);
					AssertEquals("fullWrapper.JobNumber", "NCT00001234", fullWrapper.JobNumber);

					fullWrapper = FreightWrapperFromNCTS.New(arrivalHeader, Factory);
					AssertType<NctsArrivalMovementHeader>("MovementHeader Is NctsArrivalMovementHeader", fullWrapper.MovementHeader);
					AssertEquals("Arrival fullWrapper.NCTSDeclarationType", "T2 - T2 Desc", fullWrapper.NCTSDeclarationType.CodeAndDescription);
					AssertEquals("Arrival fullWrapper.NCTSDepartureTransportID", "TAD", fullWrapper.NCTSDepartureTransportID);
					AssertEquals("Arrival fullWrapper.NCTSDepartureTransportCountry.", "PT - Portugal", fullWrapper.NCTSDepartureTransportCountry.CodeAndDescription);
					AssertEquals("Arrival fullWrapper.NCTSDepartureTransportMode.", "1", fullWrapper.NCTSDepartureTransportMode.CodeAndDescription);
					AssertEquals("Arrival fullWrapper.NCTSFrontierTransportID.", "AAA", fullWrapper.NCTSFrontierTransportID);
					AssertEquals("Arrival fullWrapper.NCTSFrontierTransportCountry.", "GB - United Kingdom", fullWrapper.NCTSFrontierTransportCountry.CodeAndDescription);
					AssertEquals("Arrival fullWrapper.NCTSFrontierTransportMode.", "2", fullWrapper.NCTSFrontierTransportMode.CodeAndDescription);
					AssertEquals("Arrival fullWrapper.NCTSGoodsLocationCode", ZString.Empty, fullWrapper.NCTSGoodsLocationCode);
					AssertEquals("Arrival fullWrapper.NCTSGoodsLocation", "ABC", fullWrapper.NCTSGoodsLocation);
				});
			}
		}

		public void TestESWrapperMapping()
		{
			var esNctsHeaderType = ObjectFactory.GetType("ES.ICusInBondHeader");
			var esNctsHeaderDocumentWrapperType = Customs.EU.NCTS.NctsHeaderDocumentWrapper.GetDocNctsType(esNctsHeaderType);

			AssertNotNull("ES NctsHeaderDocumentWrapper type", esNctsHeaderDocumentWrapperType);
		}

		public void TestITWrapperMapping()
		{
			var itNctsHeaderType = ObjectFactory.GetType("IT.ICusInBondHeader");
			var itNctsHeaderDocumentWrapperType = Customs.EU.NCTS.NctsHeaderDocumentWrapper.GetDocNctsType(itNctsHeaderType);

			AssertNotNull("IT NctsHeaderDocumentWrapper type", itNctsHeaderDocumentWrapperType);
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
NCTSDeclarationType : 
NCTSDepartureOffice : 
NCTSDepartureTransportCountry : 
NCTSDepartureTransportMode : 
NCTSDestinationOffice : 
NCTSFrontierTransportCountry : 
NCTSFrontierTransportMode : ";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType("D");
			return nctsHeader;
		}
		void SetupCustomsOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "UnitedKingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "BRITISH OFFICE Of Departure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "FRENCH OFFICE Of Destination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Latvia);

			var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T1", "T1 Desc", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2", "T2 Desc", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T-", "T- Desc", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "TIR", "TIR Desc", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2SM", "T2SM Desc", startDate, endDate);

			Factory.Save();
		}
	}
}
