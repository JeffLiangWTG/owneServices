using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.NCTS.DocumentWrappers;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderDocumentWrapper))]
	sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			var header = Factory.New<NctsHeader>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));
				AssertNotNull("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
			});
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.HolderOfTheTransitProcedureIdentificationNumber), wrapper.HolderOfTheTransitProcedureIdentificationNumber);

				SetUpPrincipal(Factory, header);
				AssertEquals(nameof(wrapper.HolderOfTheTransitProcedureIdentificationNumber), "BEHolderID", wrapper.HolderOfTheTransitProcedureIdentificationNumber);
			});
		}

		public void TestTIRHolderIdentificationNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.HolderOfTheTransitProcedureTIRNumber), wrapper.HolderOfTheTransitProcedureTIRNumber);

				var entryNum = CusEntryNumber.LoadOrCreate(header.MovementHeader, CusEntryNumberTypes.EU.TIRCarnetNumber, header.CountryCode);
				entryNum.CE_EntryNum = "21FR00007411BBC885";
				AssertEquals(nameof(wrapper.HolderOfTheTransitProcedureTIRNumber), "21FR00007411BBC885", wrapper.HolderOfTheTransitProcedureTIRNumber);
			});
		}

		public void TestTransitProcedureHolderName()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.HolderOfTheTransitProcedureName), wrapper.HolderOfTheTransitProcedureName);

				SetUpPrincipal(Factory, header);
				AssertEquals(nameof(wrapper.HolderOfTheTransitProcedureName), "OrgHeaderFullName", wrapper.HolderOfTheTransitProcedureName);
			});
		}

		public void TestLRN()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.LRN), wrapper.LRN);

				ZString expectedLRN = "1234V01";
				header.MovementHeader.BM_PaperlessInbondNum = expectedLRN;
				AssertEquals(nameof(wrapper.LRN), expectedLRN, wrapper.LRN);
			});
		}

		public void TestMRN()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.Mrn), wrapper.Mrn);

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
				entryNum.CE_EntryNum = "21FR00007411BBC885";
				AssertEquals(nameof(wrapper.Mrn), "21FR00007411BBC885", wrapper.Mrn);
			});
		}

		public void TestCancellationDate()
		{
			var header = Factory.New<NctsHeader>();
			var wrapper = GetWrapper(header);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Empty " + nameof(wrapper.CancellationDate), wrapper.CancellationDate);

				SetUpCancellationEvent(header);
				AssertEquals(nameof(wrapper.CancellationDate), "12/03/2021", wrapper.CancellationDate);
			});
		}

		public void TestLines()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			bill.B0_ReferenceID = "sbb1";
			var billItem1 = bill.GoodsItems.AddNew();
			billItem1.BY_LineNo = 1;
			billItem1.BY_DeclarationGoodsItemNumber = 12;
			var billItem2 = bill.GoodsItems.AddNew();
			billItem2.BY_LineNo = 2;
			billItem2.BY_DeclarationGoodsItemNumber = 6;
			Factory.Save();
			var wrapper = (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);
			var lines = wrapper.Lines;
			AssertEquals(2, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("BOX32ITEM should get correct item numbers from Bills.GoodsItems[0].BY_DeclarationGoodsItemNumber", "6", lines[0].BOX32ITEM);
				AssertEquals("BOX32ITEM should get correct item numbers from Bills.GoodsItems[1].BY_DeclarationGoodsItemNumber", "12", lines[1].BOX32ITEM);
			});
		}

		public void TestWrapperProperties()
		{
			CombineAssertions(() =>
			{
				NctsHeaderDocumentWrapper wrapper = SetupData();
				AssertEquals(nameof(wrapper.BOX7UCR), "UniqueConsignmentReference", wrapper.BOX7UCR);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTFLAG), "SB", wrapper.BOX21BORDERTRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTID), "EP-666-EW", wrapper.BOX21BORDERTRANSPORTID);
				AssertEquals(nameof(wrapper.BOX25BORDERTRANSPORTMODE), "NB", wrapper.BOX25BORDERTRANSPORTMODE);
				AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "CaoXian ShenyangDaJie", wrapper.BOX30LOCATIONOFGOODS);
				AssertEquals(nameof(wrapper.BOX50REPRESENTATIVE), "BE325820751000809", wrapper.BOX50REPRESENTATIVE);
				AssertEquals(nameof(wrapper.BOX7REFERENCENUMBERS), "SB123456", wrapper.BOX7REFERENCENUMBERS);
				AssertEquals(nameof(wrapper.BOXS10CONVEYANCE), "C000001", wrapper.BOXS10CONVEYANCE);
				AssertEquals(nameof(wrapper.BOXS12FIRSTARRIVALTIME), "---", wrapper.BOXS12FIRSTARRIVALTIME);
				AssertEquals(nameof(wrapper.BOXS13ROUTING), "BE;NL", wrapper.BOXS13ROUTING);
				AssertEquals(nameof(wrapper.BOXS17PLACEOFLOADING), "LOADG", wrapper.BOXS17PLACEOFLOADING);
				AssertEquals(nameof(wrapper.BOXS18PLACEOFUNLOADING), "CaoXian", wrapper.BOXS18PLACEOFUNLOADING);
				AssertEquals(nameof(wrapper.BOXS28SEALSNUMBER), "66", wrapper.BOXS28SEALSNUMBER);
				AssertEquals(nameof(wrapper.BOXS29TRANSPORTCHARGESMOP), "B", wrapper.BOXS29TRANSPORTCHARGESMOP);
				AssertEquals(nameof(wrapper.BOXS32OTHERSCI), "A", wrapper.BOXS32OTHERSCI);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOR), "SECURITYCONSIGNOR NAME\nSECURITYCONSIGNOR STREET\n666666666 SECURITYCONSIGNOR CITY\nBE", wrapper.BOXS4SECURITYCONSIGNOR);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOREORI), "BE666666666666666", wrapper.BOXS4SECURITYCONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEE), "SECURITYCONSIGNEE NAME\nSECURITYCONSIGNEE STREET\n555555555 SECURITYCONSIGNEE CITY\nTR", wrapper.BOXS6SECURITYCONSIGNEE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEEEORI), "TR555555555555555", wrapper.BOXS6SECURITYCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOXS7CARRIER), "CIRCUIT EQUIPAMENTOS ESPORTIVOS LTD\nCARRIER STREET SAO PAULA SP, BRASIL\n666666666 CARRIER CITY\nBR", wrapper.BOXS7CARRIER);
				AssertEquals(nameof(wrapper.BOXS7CARRIEREORI), "GB954131533000", wrapper.BOXS7CARRIEREORI);
				AssertEquals(nameof(wrapper.PRESENTATIONOFGOODSDATETIME), "", wrapper.PRESENTATIONOFGOODSDATETIME);
				AssertEquals(nameof(wrapper.BOX44AUTHORISATIONS), "OPO - 1234\r\nDIO - 4321", wrapper.BOX44AUTHORISATIONS);
				AssertEquals(nameof(wrapper.BOXS00SECURITY), true, wrapper.BOXS00SECURITY);
			});
		}

		public void TestWrapperPropertiesWhenSourceIsEmptyObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);

			CombineAssertions("Test properties when Source is empty", () =>
			{
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTFLAG), "---", wrapper.BOX21BORDERTRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTID), "---", wrapper.BOX21BORDERTRANSPORTID);
				AssertEquals(nameof(wrapper.BOX25BORDERTRANSPORTMODE), "---", wrapper.BOX25BORDERTRANSPORTMODE);
				AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "", wrapper.BOX30LOCATIONOFGOODS);
				AssertEquals(nameof(wrapper.BOX50REPRESENTATIVE), "", wrapper.BOX50REPRESENTATIVE);
				AssertEquals(nameof(wrapper.BOXS10CONVEYANCE), "---", wrapper.BOXS10CONVEYANCE);
				AssertEquals(nameof(wrapper.BOXS13ROUTING), "", wrapper.BOXS13ROUTING);
				AssertEquals(nameof(wrapper.BOXS17PLACEOFLOADING), "---", wrapper.BOXS17PLACEOFLOADING);
				AssertEquals(nameof(wrapper.BOXS18PLACEOFUNLOADING), "---", wrapper.BOXS18PLACEOFUNLOADING);
				AssertEquals(nameof(wrapper.BOXS28SEALSNUMBER), "0", wrapper.BOXS28SEALSNUMBER);
				AssertEquals(nameof(wrapper.BOXS29TRANSPORTCHARGESMOP), "---", wrapper.BOXS29TRANSPORTCHARGESMOP);
				AssertEquals(nameof(wrapper.BOXS32OTHERSCI), "---", wrapper.BOXS32OTHERSCI);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOR), "", wrapper.BOXS4SECURITYCONSIGNOR);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOREORI), "", wrapper.BOXS4SECURITYCONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEE), "", wrapper.BOXS6SECURITYCONSIGNEE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEEEORI), "", wrapper.BOXS6SECURITYCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOXS7CARRIER), "", wrapper.BOXS7CARRIER);
				AssertEquals(nameof(wrapper.BOXS7CARRIEREORI), "", wrapper.BOXS7CARRIEREORI);
				AssertEquals(nameof(wrapper.PRESENTATIONOFGOODSDATETIME), "", wrapper.BOXS7CARRIEREORI);
				AssertEquals(nameof(wrapper.BOX44AUTHORISATIONS), "", wrapper.BOX44AUTHORISATIONS);
				AssertEquals(nameof(wrapper.BOXS00SECURITY), false, wrapper.BOXS00SECURITY);
			});
		}

		NctsHeaderDocumentWrapper SetupData()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.LocalReferenceNumber = "SB123456";
			SetItineraryForNctsHeader(header);//11;22;33
			SetUpSecurityConsigneeForNctsHeader(header);
			SetUpSecurityConsignorForNctsHeader(header);
			SetupCarrierForNctsHeader(header);
			SetUpAuthorisationForNctsHeader(header);
			SetupDataForMovementHeader(header);
			return (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);
		}

		void SetupDataForMovementHeader(NctsHeader header)
		{
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "ORG001";
			representative.OH_FullName = "WeiLong";
			var eori = representative.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = "BE";
			eori.OK_CustomsRegNo = "325820751000809";
			var movementHeader = header.MovementHeader;
			movementHeader.Representative.OrganisationPK = representative.PK;

			movementHeader.BM_UniqueConsignmentReference = "UniqueConsignmentReference";
			movementHeader.BM_TOLCarrierCode = "SB";
			movementHeader.BM_ExportTransportMode = "NB";
			movementHeader.BM_TOLCarrierID = "EP-666-EW";
			movementHeader.BM_LocationOfGoods = "CaoXian";
			movementHeader.BM_CustomsSubPlace = "ShenyangDaJie";
			movementHeader.BM_ConveyanceNumber = "C000001";
			movementHeader.BM_RL_NKForeignDestPort = "LOADG";
			movementHeader.BM_PlaceOfUnloading = "CaoXian";
			movementHeader.BM_SealQty = 66;
			movementHeader.BM_MethodOfPayment = "B";
			movementHeader.BM_BTAIndicator = "A";
			movementHeader.BM_TypeOfSecurity = "ENT";
		}

		void SetItineraryForNctsHeader(NctsHeader header)
		{
			var a = header.Itinerary.AddNew();
			a.CountryCode = Core.Constants.CountryCodes.Belgium;

			var b = header.Itinerary.AddNew();
			b.CountryCode = Core.Constants.CountryCodes.Netherlands;
		}

		void SetupCarrierForNctsHeader(NctsHeader header)
		{
			var carrier = header.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CIREQU");
			var carrierAddress = carrier.MainAddress;
			carrierAddress.Address1 = "CARRIER STREET";
			carrierAddress.City = "CARRIER CITY";
			carrierAddress.Postcode = "666666666";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB954131533000", Core.Constants.CountryCodes.UnitedKingdom);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "GBR/ABC1234", Core.Constants.CountryCodes.UnitedKingdom);
			header.BH_OH_Carrier = carrier.PK;
			var eori = carrier.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Switzerland;
			eori.OK_CustomsRegNo = "66666666666666666";
			header.Factory.Save();
		}

		void SetUpSecurityConsignorForNctsHeader(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNOR NAME";
			address.OA_Address1 = "SECURITYCONSIGNOR STREET";
			address.OA_PostCode = "666666666";
			address.OA_City = "SECURITYCONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			header.SecurityConsignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			eori.OK_CustomsRegNo = "666666666666666";
		}

		void SetUpSecurityConsigneeForNctsHeader(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNEE NAME";
			address.OA_Address1 = "SECURITYCONSIGNEE STREET";
			address.OA_PostCode = "555555555";
			address.OA_City = "SECURITYCONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			header.SecurityConsignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Turkey;
			eori.OK_CustomsRegNo = "555555555555555";
		}

		void SetUpAuthorisationForNctsHeader(NctsHeader header)
		{
			var a = header.MovementHeader.CusAuthorizationUsages.AddNew();
			a.AGC_Code = "OPO";
			a.AGC_Number = "1234";

			var b = header.MovementHeader.CusAuthorizationUsages.AddNew();
			b.AGC_Code = "DIO";
			b.AGC_Number = "4321";
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
		}

		static void SetUpCancellationEvent(NctsHeader header)
		{
			var log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				log.SL_Reference = NctsHeaderDocumentWrapper.CancellationLogReferenceCode;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}
		}

		static void SetUpPrincipal(BusinessObjectFactory factory, NctsHeader nctsHeader)
		{
			var principal = factory.New<OrgHeader>();
			principal.OH_FullName = "HolderName";
			principal.OH_Code = "HOLDER";
			var principalAddress = principal.Addresses.AddNew();
			principalAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			principalAddress.OA_Address1 = "Wapenstilstandlaan 47";
			principalAddress.OA_City = "Antwerpen";

			var orgCusCodes = new List<OrgCusCode>
			{
				CreateOrgCusCode(factory, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "HolderID", Core.Constants.CountryCodes.Belgium),
				CreateOrgCusCode(factory, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			principal.CustomsCodes.AddRange(orgCusCodes);

			CreateJobDocAddress(factory, AutoDocAddressTypes.Codes.Principal, orgHeaderFullName: "OrgHeaderFullName", contactName: "ContactName", contactEmail: "ContactEmail", contactPhone: "ContactPhone", orgCusCodes: orgCusCodes, orgHeader: principal, parent: nctsHeader, orgAddress: principalAddress, jobDocAddress: nctsHeader.Principal);
		}

		static OrgCusCode CreateOrgCusCode(BusinessObjectFactory factory, string codeType, string customsRegNo, string country)
		{
			var orgCusCode = factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_CustomsRegNo = customsRegNo;
			orgCusCode.OK_RN_NKCodeCountry = country;
			return orgCusCode;
		}

		static JobDocAddress CreateJobDocAddress(BusinessObjectFactory factory, string addressType = null, string orgHeaderFullName = null, string contactName = null, string contactPhone = null, string contactEmail = null
			, JobDocAddress jobDocAddress = null
			, OrgHeader orgHeader = null
			, OrgAddress orgAddress = null
			, IEnumerable<OrgCusCode> orgCusCodes = null, BusinessObject parent = null)
		{
			jobDocAddress = jobDocAddress ?? factory.New<JobDocAddress>();
			orgHeader = orgHeader ?? factory.New<OrgHeader>();
			var orgContact = factory.New<OrgContact>();

			orgHeader.OH_FullName = orgHeaderFullName;
			jobDocAddress.E2_AddressType = addressType;

			jobDocAddress.E2_Contact = contactName;
			jobDocAddress.E2_Phone = contactPhone;
			jobDocAddress.E2_Email = contactEmail;

			orgContact.OC_ContactName = contactName;
			orgContact.OC_Phone = contactPhone;
			orgContact.OC_Email = contactEmail;

			if (orgAddress != null)
			{
				jobDocAddress.E2_OA_Address = orgAddress.PK;
				orgAddress.OA_OH = orgHeader.PK;
			}

			jobDocAddress.OrganisationPK = orgHeader.PK;
			if (parent != null)
			{
				jobDocAddress.E2_ParentID = parent.PK;
				jobDocAddress.E2_ParentTableCode = parent.TablePrefix;
			}
			jobDocAddress.ContactPK = orgContact.PK;

			orgHeader.CustomsCodes.AddRange(orgCusCodes ?? Enumerable.Empty<OrgCusCode>());
			return jobDocAddress;
		}

		NctsHeaderDocumentWrapper GetWrapper(NctsHeader header)
		{
			return (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);
		}
	}
}
