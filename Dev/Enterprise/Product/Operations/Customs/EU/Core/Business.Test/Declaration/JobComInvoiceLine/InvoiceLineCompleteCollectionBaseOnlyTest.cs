using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestDefaultOfJI_Procedure()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "C1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "C1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				instruction.CEI_Procedure = "C1";

				var collection = new InvoiceLineCompleteCollection(dec);
				var invoiceLine = collection.AddNew();
				AssertEquals("First 2 Characters is set", "C1", invoiceLine.JI_Procedure);
			}
		}
	}
}
