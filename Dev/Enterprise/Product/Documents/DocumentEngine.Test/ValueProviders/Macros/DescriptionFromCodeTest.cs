using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DescriptionFromCode))]
	sealed class DescriptionFromCodeTest : ValueProviderTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacementWithIllegalExpression()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ReportCommand command = factory.New<ReportCommand>();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			DocumentPack pack = new DocumentPack(command);

			GlbGroup postMasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			factory.Save();

			using (Report report = new Report(pack, excelTemplate))
			{
				report.MenuItem.SU_BusinessContext = "Custom";
				report.MenuItem.SU_MenuName = "Delivery Order";
				report.MenuItem.SU_MenuPath = "Misc/";
				report.MenuItem.SU_FilterList = "CTY=GB";
				report.MenuItem.SU_IsSystemDefined = true;
				report.MenuItem.SU_IsClientSpecific = true;

				report.StTemplate = factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "DA893";
				report.StTemplate.SO_IsSystemDefined = false;
				report.StTemplate.SO_IsClientSpecific = false;
				report.StTemplate.SO_DataContext = "Shipping";
				report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";

				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertEquals("", ValueProviderToTest.GetReplacement("<DescriptionFromCode(UnknownType, FOR)>", report));
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in DescriptionFromCode Macro: Type UnknownType is not defined.]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<DescriptionFromCode>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DescriptionFromCode(Type, Code)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<DescriptionFromCode(, Code)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<DescriptionFromCode(Type, Code, Fallback Fallback)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DescriptionFromCode(Type, Code, Fallback)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(FreightDataRegistry.Instance.JobServices.Value.GetDescriptionFromCode("FOR"), ValueProviderToTest.GetReplacement("<DescriptionFromCode(JobServices, FOR)>", Report));
			AssertEquals(LinerAgencyDataRegistry.Instance.DisbursementSubGroups.Value.GetDescriptionFromCode("POC"), ValueProviderToTest.GetReplacement("<DescriptionFromCode(VoyageAccountingSubGroups, POC)>", Report));
			AssertEquals("Fallback", ValueProviderToTest.GetReplacement("<DescriptionFromCode(VoyageAccountingSubGroups, AAA, Fallback)>", Report));
			AssertEquals(null, ValueProviderToTest.GetReplacement("<DescriptionFromCode(VoyageAccountingSubGroups, )>", Report));
			AssertEquals("Fallback", ValueProviderToTest.GetReplacement("<DescriptionFromCode(VoyageAccountingSubGroups, , Fallback)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DescriptionFromCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			base.PrepareDataForExamplesEvaluate();
			var defaultValue = new CodeDescriptionPairList();
			defaultValue.AddPair("POC", "Port Charge");
			LinerAgencyDataRegistry.Instance.DisbursementSubGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
		}
	}
}
