using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingAdditionalCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalCodesList()
		{
			var additionalCodesList = lookups.AdditionalCodesList;
			AssertType<ZZRefCusCodeListCombinedCollection>(additionalCodesList);
			AssertEquals("Code List Type should be AdditionalCodes", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, additionalCodesList.CodeTypes.First());
		}

		public void TestAdditionalCodeDescription_PreferredLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("FR", "French");
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkingLanguage = "FR";
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AdditionalCodes");
			var additionalCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "C44", "In English", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeListLanguage(additionalCode, "FR", "In French");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
			{
				var gdmAdditionalCode = new GuidedDecisionMakingAdditionalCode(gDMBasic);
				var lookups = gdmAdditionalCode.Lookups;
				gdmAdditionalCode.AdditionalCode = "C44";

				var descriptionsFR = lookups.CachedListOfAdditionalCodeDescriptions;
				var descFR = descriptionsFR.GetDescriptionFromCode("C44");
				AssertEquals("We should have the description in French, as the users preferred language.", "In French", descFR);
			}

			var gdmAdditionalCodeLV = new GuidedDecisionMakingAdditionalCode(gDMBasic);
			lookups = gdmAdditionalCodeLV.Lookups;
			gdmAdditionalCodeLV.AdditionalCode = "C44";

			var descriptions = lookups.CachedListOfAdditionalCodeDescriptions;
			var desc = descriptions.GetDescriptionFromCode("C44");
			AssertEquals("We should have the description in english as the default user preferred language.", "In English", desc);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var additionalCode = new GuidedDecisionMakingAdditionalCode(gDMBasic);
			lookups = additionalCode.Lookups;
		}

		GuidedDecisionMakingAdditionalCodeLookups lookups;
	}
}
