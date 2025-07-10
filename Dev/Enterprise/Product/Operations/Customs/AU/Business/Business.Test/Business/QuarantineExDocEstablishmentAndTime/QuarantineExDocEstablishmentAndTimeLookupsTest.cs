using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocEstablishmentAndTimeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTreatmentDurationUQ()
		{
			var durationLookups = lookups.TreatmentDurationUQ;
			AssertEquals("SEC, MIN, HUR, DAY", durationLookups.CodesAsString);
		}

		public void TestTreatmentTemperatureUQ()
		{
			var temperatureLookups = lookups.TreatmentTemperatureUQ;
			AssertEquals("CEL, FAH", temperatureLookups.CodesAsString);
		}

		public void TestTreatmentConcentrationUQ()
		{
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TreatmentConcentration;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(codeType, "Desc", Core.Constants.CountryCodes.Australia);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Australia,
				codeType,
				"123", "123 Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Australia,
				codeType,
				"321", "321 Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			AssertEquals("123, 321", lookups.TreatmentConcentrationUQ.ToCodeStrings());
		}

		public void TestAddressesDeclaration()
		{
			TestAddresses(declaration);
		}

		public void TestAuthorisationEstablishment()
		{
			var authEstablishment = lookups.AuthorisationEstablishment;
			AssertSame(authEstablishment, lookups.AuthorisationEstablishment);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			CreateRegCode(org1, "ESN");
			CreateRegCode(org2, "NSN");

			Factory.Save();

			authEstablishment.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1 }, authEstablishment);
			AssertEquals("Organisation does not contain a registration number / code for: (ESN) EXDOC Establishment Number", authEstablishment.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
		}

		public void TestAddressesShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			TestAddresses(shipment);
		}

		public void TestAddresses(IDocAddresses addresses)
		{
			var address1 = addresses.DocAddresses.CreateWithAddressType(DocAddressType.AQISProcessingEstablishment);
			address1.E2_OA_Address = Factory.New<OrgAddress>().PK;
			var address2 = addresses.DocAddresses.CreateWithAddressType(DocAddressType.AQISProcessingEstablishment);
			address2.E2_AddressOverride = true;
			address2.E2_CompanyName = "Company";
			addresses.DocAddresses.CreateWithAddressType(DocAddressType.AQISProcessingEstablishment);

			AssertContainsExactElementsInAnyOrder(new[] { address1, address2 }, lookups.Addresses);
		}

		public void TestProcessingTypeForExDoc()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var exDocHeader = invoiceHeader.QuarantineExDocHeader;
			var exDocMessage = exDocHeader.Messages.AddNew();
			exDocMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			var process = quarantineExDocLine.Processes.AddNew();
			var edocxLookups = process.Lookups;

			exDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var processingTypes = edocxLookups.ProcessingType;
			AssertEquals("AQ, CT, FR, HA, LO, PC, PK, SL, ST, TR", processingTypes.CodesAsString);
			AssertSame(processingTypes, edocxLookups.ProcessingType);

			exDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			AssertEquals("AQ, CT, FF, FR, FV, HA, IR, LO, PC, PK, SL, ST, TR, TV", edocxLookups.ProcessingType.CodesAsString);
		}

		JobDeclaration declaration;
		QuarantineExDocEstablishmentAndTimeLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			var process = quarantineExDocLine.Processes.AddNew();
			lookups = process.Lookups;
		}

		void CreateRegCode(OrgHeader org, params string[] codes)
		{
			foreach (var code in codes)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				cusCode.OK_CodeType = code;
				cusCode.OK_CustomsRegNo = "1234";
			}
		}
	}
}
