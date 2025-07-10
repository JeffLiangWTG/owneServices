using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocEstablishmentAndTime))]
	sealed class QuarantineExDocEstablishmentAndTimeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEE_TreatmentTemperature_Caption()
		{
			AssertEquals("Treatment Temperature", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentTemperature)).Caption);
		}

		public void TestEE_TreatmentTemperatureUQ_Caption()
		{
			AssertEquals("Treatment Temperature UQ", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentTemperatureUQ)).Caption);
		}

		public void TestEE_TreatmentDuration_Caption()
		{
			AssertEquals("Treatment Duration", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentDuration)).Caption);
		}

		public void TestEE_TreatmentDurationUQ_Caption()
		{
			AssertEquals("Treatment Duration UQ", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentDurationUQ)).Caption);
		}
		public void TestEE_TreatmentConcentration_Caption()
		{
			AssertEquals("Treatment Concentration", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentConcentration)).Caption);
		}

		public void TestEE_TreatmentConcentrationUQ_Caption()
		{
			AssertEquals("Treatment Concentration UQ", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocEstablishmentAndTime), nameof(QuarantineExDocEstablishmentAndTime.EE_TreatmentConcentrationUQ)).Caption);
		}

		public void TestEE_ProcessingType_List()
		{
			AssertEquals("Lookups.ProcessingType", quarantineProcess.EE_ProcessingTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestEE_LeaseNumber_MaxLength()
		{
			quarantineProcess.EE_LeaseNumber = "Test1";
			AssertEquals("Test1", quarantineProcess.EE_LeaseNumber);

			AssertEquals(35, quarantineProcess.EE_LeaseNumberInfo.MaxLength);
			AssertNoExceptionThrown(() => quarantineProcess.EE_LeaseNumber = new ZString('X', 35));
			AssertExceptionThrown<MaxLengthExceededException>(() => quarantineProcess.EE_LeaseNumber = new ZString('X', 36));
			ErrorReporter.Clear();
		}

		public void TestIngredientsMaxCount()
		{
			var collection = quarantineProcess.Ingredients;
			for (int i = 0; i < 8; i++)
			{
				collection.AddNew();
			}

			CombineAssertions(() =>
			{
				var ingredientsErrors = collection.GetErrors();
				Assert("8 objects", !ingredientsErrors.Contains("Error - CusCodeData: You are only allowed a maximum of 8 Treatment Active Ingredient here."));
				collection.AddNew();
				ingredientsErrors = collection.GetErrors();
				Assert("To many objects", ingredientsErrors.Contains("Error - CusCodeData: You are only allowed a maximum of 8 Treatment Active Ingredient here."));
			});
		}

		public void TestIngredientsSortOrder()
		{
			var ingredient1 = quarantineProcess.Ingredients.AddNew();
			ingredient1.CY_Code = "21";
			ingredient1.CY_Order = 1;
			var ingredient2 = quarantineProcess.Ingredients.AddNew();
			ingredient2.CY_Code = "22";
			ingredient2.CY_Order = 3;
			var ingredient3 = quarantineProcess.Ingredients.AddNew();
			ingredient3.CY_Code = "23";
			ingredient3.CY_Order = 2;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var quarantineProcessInOtherFactory = otherFactory.Load<QuarantineExDocEstablishmentAndTime>(quarantineProcess.PK);

			AssertEquals("21, 23, 22", string.Join(", ", quarantineProcessInOtherFactory.Ingredients.Cast<TreatmentActiveIngredient>().Select(x => x.CY_Code)));
		}

		public void TestLookups()
		{
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var lookups = quarantineProcess.Lookups;
			AssertType(typeof(EXDOCSQuarantineExDocEstablishmentAndTimeLookups), lookups);
			AssertSame(declaration.Lookups, declaration.Lookups);
			AssertSame("Should be cached.", quarantineProcess.Lookups, quarantineProcess.Lookups);

			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			lookups = quarantineProcess.Lookups;
			AssertType(typeof(NEXDOCSQuarantineExDocEstablishmentAndTimeLookups), lookups);
			AssertSame("Should be cached.", lookups, quarantineProcess.Lookups);
		}

		public void TestValidation()
		{
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertType<QuarantineExDocEstablishmentAndTimeValidation>(quarantineProcess.Validation);

			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertType<NEXDOCSQuarantineExDocEstablishmentAndTimeValidation>(quarantineProcess.Validation);
		}

		public void TestFindOrCreateAddressForOrganisation()
		{
			IDocAddresses addresses = declaration;
			var blankAddress = addresses.DocAddresses.CreateWithRequirement(addresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
			Assert(blankAddress.IsEmpty);
			var realAddress = addresses.DocAddresses.CreateWithRequirement(addresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
			realAddress.OrganisationPK = testEstablishment.PK;
			Assert(!realAddress.IsEmpty);

			quarantineProcess.EE_AuthorisationEstablishmentID = "12345";
			AssertEquals("Establishment organisation is mapped to existing DocAddress", realAddress.PK, quarantineProcess.EE_E2_Address);
		}

		public void TestSyncAddress_CommercialInvoiceOnFakeDeclaration()
		{
			var commercialInvoice = Factory.New<JobComInvoiceHeader>();
			commercialInvoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var line1 = (JobComInvoiceLine)commercialInvoice.InvoiceLines.AddNew();
			var quarantineLine = line1.QuarantineExDocLine;
			var process = quarantineLine.Processes.AddNew();
			process.EE_AuthorisationEstablishmentID = "12345";
			AssertNull("Establishment organisation address is empty as there is no DocAddress collection", process.Address);
			Factory.Save();

			var fakeDec = (JobDeclaration)new FakeDeclarationCreatorForInvoice(commercialInvoice).HeaderData;
			AssertEquals("Establishment organisation is updated as the ID is linked", testEstablishment.PK, process.Address.OrganisationPK);
			AssertEquals("Address was added to shipment", process.Address, fakeDec.DocAddresses.FindByDocAddressType(DocAddressType.AQISProcessingEstablishment));
			AssertEquals(false, process.HasChanges);
		}

		public void TestSyncAddress_Declaration()
		{
			AssertSyncAddress(declaration);
		}

		public void TestSyncAddress_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertSyncAddress(shipment);
		}

		public void TestSyncAuthorisationEstablishmentNumberDeclaration()
		{
			AssertSyncAuthorisationEstablishmentNumber(declaration);
		}

		public void TestSyncAuthorisationEstablishmentNumberShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertSyncAuthorisationEstablishmentNumber(shipment);
		}

		public void TestQuarantineExDocLine()
		{
			var quarantineLine = quarantineProcess.QuarantineExDocLine;
			AssertNotNull("QurantineExdocLine is retreiveable from Process", quarantineLine);
			AssertEquals("PK's are the same for both", this.quarantineLine.PK, quarantineLine.PK);
		}

		public void TestQuarantineExDocHeader()
		{
			var quarantineHeader = quarantineProcess.QuarantineExDocHeader;
			AssertNotNull("QuaratineExdocHeader Must Exist if there is a line", quarantineHeader);
			AssertEquals("PK's are the same for both", invoiceHeader.QuarantineExDocHeader.PK, quarantineHeader.PK);
		}

		public void TestTemplateCopy()
		{
			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
			quarantineProcess.EE_AuthorisationEstablishmentID = "1234";
			quarantineProcess.EE_EstablishmentIndicator = "PC";

			ITemplateCopyable template = quarantineProcess;
			var copy = (QuarantineExDocEstablishmentAndTime)template.TemplateCopy();

			AssertEquals("Copied correctly", quarantineProcess.EE_AuthorisationEstablishmentID, copy.EE_AuthorisationEstablishmentID);
			AssertEquals("Copied correctly", quarantineProcess.EE_EstablishmentIndicator, copy.EE_EstablishmentIndicator);
			AssertEquals("Copied correctly", ZString.Empty, copy.EE_EstablishmentPostedStatus);
		}

		public void TestEE_EstablishmentPostedStatus()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;

			quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
			AssertEquals("Not Sent", quarantineProcess.EE_EstablishmentPostedStatusDescription);
			AssertEquals(false, quarantineProcess.IsLodged);
			AssertEquals(false, quarantineProcess.IsDeletePending);

			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
			AssertEquals("Lodged", quarantineProcess.EE_EstablishmentPostedStatusDescription);
			AssertEquals(true, quarantineProcess.IsLodged);
			AssertEquals(false, quarantineProcess.IsDeletePending);

			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
			AssertEquals("Delete Pending", quarantineProcess.EE_EstablishmentPostedStatusDescription);
			AssertEquals(false, quarantineProcess.IsLodged);
			AssertEquals(true, quarantineProcess.IsDeletePending);

			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = ZString.Empty;

			quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
			AssertEquals("", quarantineProcess.EE_EstablishmentPostedStatusDescription);
			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
			AssertEquals("", quarantineProcess.EE_EstablishmentPostedStatusDescription);
			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
			AssertEquals("", quarantineProcess.EE_EstablishmentPostedStatusDescription);
		}

		public void TestReadOnly()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				AssertEquals(false, quarantineProcess.ReadOnly);
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				AssertEquals(true, quarantineProcess.ReadOnly);
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
				AssertEquals(true, quarantineProcess.ReadOnly);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				AssertEquals(false, quarantineProcess.ReadOnly);
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				AssertEquals(false, quarantineProcess.ReadOnly);
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
				AssertEquals(false, quarantineProcess.ReadOnly);
			}
		}

		public void TestValidationEnabled()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				AssertEquals(true, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				AssertEquals(false, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
				AssertEquals(false, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				AssertEquals(true, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				AssertEquals(true, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
				AssertEquals(true, quarantineProcess.IsValidationEnabled(quarantineProcess.EE_HarvestAreaInfo));
			}
		}

		protected override BusinessObject GetNewBusinessObject() => quarantineProcess;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => quarantineProcess;

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => quarantineProcess;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			invoiceHeader = helper.Header1;
			quarantineLine = helper.Line1.QuarantineExDocLine;
			quarantineProcess = quarantineLine.Processes.AddNew();
			testEstablishment = Factory.New<OrgHeader>();
			testEstablishment.OH_Code = "TESTEST";
			var estCode = testEstablishment.CustomsCodes.AddNew();
			estCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			estCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			estCode.OK_CustomsRegNo = "12345";
			Factory.Save();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		QuarantineExDocLine quarantineLine;
		OrgHeader testEstablishment;
		QuarantineExDocEstablishmentAndTime quarantineProcess;

		void AssertSyncAddress(IDocAddresses addresses)
		{
			quarantineProcess.EE_AuthorisationEstablishmentID = "83745";
			AssertNull("Establishment organisation address is empty as there are none linked to that ID", quarantineProcess.Address);
			quarantineProcess.EE_AuthorisationEstablishmentID = "12345";
			AssertEquals("Establishment organisation is updated as the ID is linked", testEstablishment.PK, quarantineProcess.Address.OrganisationPK);
			AssertEquals("Address was added to shipment", quarantineProcess.Address, addresses.DocAddresses.FindByDocAddressType(DocAddressType.AQISProcessingEstablishment));
		}

		void AssertSyncAuthorisationEstablishmentNumber(IDocAddresses addresses)
		{
			quarantineProcess.EE_E2_Address = ZGuid.NewZGuid();
			AssertEquals("Establishment ID is empty as there are none linked to that Organisation", ZString.Empty, quarantineProcess.EE_AuthorisationEstablishmentID);

			var address = addresses.DocAddresses.CreateWithRequirement(addresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
			address.OrganisationPK = testEstablishment.PK;
			quarantineProcess.EE_E2_Address = address.PK;
			AssertEquals("Establishment ID is updated as the organisation is linked", "12345", quarantineProcess.EE_AuthorisationEstablishmentID);
		}
	}
}
