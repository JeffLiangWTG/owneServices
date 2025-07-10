using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class Box29OfficeCodeEvaluatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("entryHeader required", () => new Box29OfficeCodeEvaluator(null));
			AssertExceptionThrown<ArgumentNullException>("Declaration required", () => new Box29OfficeCodeEvaluator(Factory.New<CusEntryHeader>()));
			AssertNoExceptionThrown("Valid entryHeader", () => new Box29OfficeCodeEvaluator(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
		}

		public void TestEvaluate_ImportCase()
		{
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT018100", "Bari", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_CustomsOffice = "IT017000";
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT016199");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "IT018100");

			Factory.Save();

			var evaluator = new Box29OfficeCodeEvaluator(entryHeader);
			AssertEquals("Box29ExitOffice Import case", "IT018100 Bari", evaluator.Evaluate());
		}

		public void TestEvaluate_ExportCase()
		{
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT016199", "Taranto", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "IT017000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT016199");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "IT018100");

			var evaluator = new Box29OfficeCodeEvaluator(entryHeader);
			AssertEquals("Box29ExitOffice Export case", "IT016199 Taranto", evaluator.Evaluate());
		}

		public void TestEvaluateWithNoAvaliableOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "IT017000";

			var evaluator1 = new Box29OfficeCodeEvaluator(entryHeader);
			AssertEquals("Box29ExitOffice Export case", "", evaluator1.Evaluate());

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var evaluator2 = new Box29OfficeCodeEvaluator(entryHeader);
			AssertEquals("Box29ExitOffice Import case", "", evaluator2.Evaluate());
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		}
		UniversalReferenceTestDataHelper helper;

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper()
			{
				return new JobDeclarationCustomsOfficeRequirementHelperForTest(this);
			}
		}

		class JobDeclarationCustomsOfficeRequirementHelperForTest : JobDeclarationCustomsOfficeRequirementHelper
		{
			public JobDeclarationCustomsOfficeRequirementHelperForTest(IEuOfficeCodeProvider declaration) : base(declaration)
			{
			}

			protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
			{
				var result = base.GetOtherRequirements().ToList();

				result.Add(new CustomsOfficeRequirement("ENT", true, true));
				return result;
			}
		}
	}
}
