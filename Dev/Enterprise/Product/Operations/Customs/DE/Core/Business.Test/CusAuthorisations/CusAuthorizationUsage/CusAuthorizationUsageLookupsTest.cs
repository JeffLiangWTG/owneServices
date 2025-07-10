using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCodeList_IsSDEExportOrSDEOutwardProcessing()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("SDE"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_IsCCLExportAndIsOPOOutwardProcessing()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("CCL, OPO"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_IsCCLExportAndStyle2ndDigitIs0()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("CCL, CWP, CW1, CW2"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_IsOPOOutwardProcessing()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110110;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("OPO"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_IsEIRExport()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("EIR"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_Overlapped()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111410;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo("SDE, CCL, OPO, EIR"));
		}

		[ExpectNoExceptions]
		public void TestCodeList_NonSpecial()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(declaration.CountryCode);
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo(provider.GetAuthorisationTypeList(Factory).CodesAsString));
		}

		[ExpectNoExceptions]
		public void TestCodeList_EntryInstructionIsNull()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusAuthorizationUsageInLine = invoiceLine.CusAuthorizationUsages.AddNew();
			var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(declaration.CountryCode);
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)cusAuthorizationUsageInLine.Lookups.CodeList).CodesAsString, Is.EqualTo(provider.GetAuthorisationTypeList(Factory).CodesAsString));
		}

		[ExpectNoExceptions]
		public void TestCodeList_IsImport()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(declaration.CountryCode);
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, Is.EqualTo(provider.GetAuthorisationTypeList(Factory).CodesAsString));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			lookups = cusAuthorizationUsage.Lookups;
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusAuthorizationUsage cusAuthorizationUsage;
		CusAuthorizationUsageLookups lookups;
	}
}
