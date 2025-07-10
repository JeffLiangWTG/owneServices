using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusAuthorizationUsageValueSetStrategyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSettingAGC_CPH_Authorization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.FillWithValidTestData();
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();

			cusAuthorizationUsage.AGC_Number = "123456";
			authorisationHeader.CPH_Number = "1234567";

			cusAuthorizationUsage.AGC_CPH_Authorization = ZGuid.Empty;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison), "AGC_CPH_Authorization is set empty => AGC_Number should keep his value.");

			cusAuthorizationUsage.AGC_CPH_Authorization = ZGuid.Invalid;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison), "AGC_CPH_Authorization is set to an invalid guid => AGC_Number should keep his value.");

			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, NUnit.Framework.Is.EqualTo(ZString.Empty), "AGC_CPH_Authorization is set to AuthorisationHeader PK => AGC_Number should be Empty.");
		}

		[ExpectNoExceptions]
		public void TestSettingAGC_Number()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.FillWithValidTestData();
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();

			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;

			cusAuthorizationUsage.AGC_Number = ZString.Empty;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_CPH_Authorization, NUnit.Framework.Is.EqualTo(authorisationHeader.PK), "AGC_Number is set empty => AGC_CPH_Authorization should keep his value.");

			cusAuthorizationUsage.AGC_Number = "123456";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_CPH_Authorization, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "AGC_Number is set to 123456 => AGC_CPH_Authorization should be Empty.");
		}

		[ExpectNoExceptions]
		public void TestSettingAGC_Code()
		{
			var cei = Factory.New<CusEntryInstructionForCusAuthTest>();
			var auth = cei.CusAuthorizationUsages.AddNew();

			auth.AGC_Code = "ABC";
			NUnit.Framework.Assert.That(cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled, NUnit.Framework.Is.EqualTo(false), "Entry Instruction not called - (missing AGC_OH_Owner)");

			auth.AGC_OH_Owner = ZGuid.NewZGuid();
			cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled = false;

			auth.AGC_Code = "QWE";
			NUnit.Framework.Assert.That(cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled, NUnit.Framework.Is.EqualTo(true), "Entry Instruction called - (AGC_OH_Owner filled)");
		}

		[ExpectNoExceptions]
		public void TestSettingAGC_OH_Owner()
		{
			var cei = Factory.New<CusEntryInstructionForCusAuthTest>();
			var auth = cei.CusAuthorizationUsages.AddNew();

			auth.AGC_OH_Owner = ZGuid.NewZGuid();
			NUnit.Framework.Assert.That(cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled, NUnit.Framework.Is.EqualTo(false), "Entry Instruction not called - (missing AGC_Code)");

			auth.AGC_Code = "ABC";
			cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled = false;

			auth.AGC_OH_Owner = ZGuid.NewZGuid();
			NUnit.Framework.Assert.That(cei.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled, NUnit.Framework.Is.EqualTo(true), "Entry Instruction called - (AGC_Code filled)");
		}
	}

	class CusEntryInstructionForCusAuthTest : Business.Declaration.CusEntryInstruction
	{
		public CusEntryInstructionForCusAuthTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled { get; set; }
		protected override void CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCore(CusAuthorizationUsage authUsage)
		{
			CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCalled = true;
		}
	}
}
