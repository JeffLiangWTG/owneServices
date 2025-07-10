using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EntryRequiresCIQStrategyTest : TestCaseWithFactory
	{
		public void TestValidateEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "B", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "A", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005101000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "MED", tariff2);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005109000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "CFCS", tariff3);
			anotherFactory.Save();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var result = EntryRequiresCIQStrategy.ValidateEntry(instruction);
			Assert(!result.HasErrors);
			invoiceLine.JI_Tariff = "8476900000";
			result = EntryRequiresCIQStrategy.ValidateEntry(instruction);
			Assert(result.Messages.Contains(EntryRequiresCIQStrategyRequirementB.DocumentBRequireCIQMessage));
			AssertEquals(1, result.ErrorCount);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "8476900000";
			result = EntryRequiresCIQStrategy.ValidateEntry(instruction);
			Assert(result.Messages.Contains(EntryRequiresCIQStrategyRequirementA.DocumentARequireCIQMessage));
			AssertEquals(1, result.ErrorCount);
			var otherPackage = instruction.OtherPackages.AddNew();
			otherPackage.CY_Code = PackageType.Codes.WoodBox;
			result = EntryRequiresCIQStrategy.ValidateEntry(instruction);
			Assert(result.Messages.Contains(EntryRequiresCIQStrategyRequirementA.DocumentARequireCIQMessage));
			Assert(result.Messages.Contains(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage));
			AssertEquals(2, result.ErrorCount);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.CusContainers.AddNew();
			var container = instruction.EntryHeader.Containers[0];
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			result = EntryRequiresCIQStrategy.ValidateEntry(instruction);
			Assert(result.Messages.Contains(EntryRequiresCIQStrategyLCL.LCLRequireCIQMessage));
			AssertEquals(3, result.ErrorCount);
			AssertEquals(@"This Entry Instruction requires inspection and quarantine as,
       some tariffs requires supporting document A
       the type of package or other packages include wooden packaging
       some contains are LCL", result.GetFormattedMessage());
		}
	}

	class TestEntryRequiresCIQStrategyValidationResult : TestCaseWithFactory
	{
		public void TestDefaults()
		{
			var result = new EntryRequiresCIQStrategyValidationResult();
			Assert(result.Messages.Contains(CusEntryInstructionValidation.GoodsRequireCIQMessage));
			AssertEquals(0, result.ErrorCount);
			Assert(!result.HasErrors);
			AssertEquals(CusEntryInstructionValidation.GoodsRequireCIQMessage, result.GetFormattedMessage());
		}
	}

	class TestEntryRequiresCIQStrategyRequirementB : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "B", tariff);

			var validation = new EntryRequiresCIQStrategyRequirementB();
			Assert(validation.Validate(instruction).IsEmpty);
			invoiceLine.JI_Tariff = "8476900000";
			AssertEquals(EntryRequiresCIQStrategyRequirementB.DocumentBRequireCIQMessage, validation.Validate(instruction));
		}
	}

	class TestEntryRequiresCIQStrategyRequirementA : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "A", tariff);

			var validation = new EntryRequiresCIQStrategyRequirementA();
			Assert(validation.Validate(instruction).IsEmpty);
			invoiceLine.JI_Tariff = "8476900000";
			AssertEquals(EntryRequiresCIQStrategyRequirementA.DocumentARequireCIQMessage, validation.Validate(instruction));
		}
	}

	class TestEntryRequiresCIQStrategyProcedureCode4561 : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var validation = new EntryRequiresCIQStrategyProcedureCode4561();
			Assert(validation.Validate(instruction).IsEmpty);
			instruction.CEI_Style = CNRefCusProcedure.Codes._4561;
			AssertEquals(EntryRequiresCIQStrategyProcedureCode4561.Procedure4561RequireCIQMessage, validation.Validate(instruction));
		}
	}

	class TestEntryRequiresCIQStrategy3612MED : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005101000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "MED", tariff);

			var validation = new EntryRequiresCIQStrategy3612MED();
			Assert(validation.Validate(instruction).IsEmpty);
			invoiceLine.JI_Tariff = "3005101000";
			instruction.CEI_Style = CNRefCusProcedure.Codes._3612;
			AssertEquals(EntryRequiresCIQStrategy3612MED.Procedure3612MEDRequireCIQMessage, validation.Validate(instruction));
		}
	}

	class TestEntryRequiresCIQStrategyCFCS : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005109000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "CFCS", tariff);

			var validation = new EntryRequiresCIQStrategyCFCS();
			Assert(validation.Validate(instruction).IsEmpty);
			invoiceLine.JI_Tariff = "3005109000";
			AssertEquals(EntryRequiresCIQStrategyCFCS.CFCSRequireCIQMessage, validation.Validate(instruction));
		}
	}

	class TestEntryRequiresCIQStrategyPackageType : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var validation = new EntryRequiresCIQStrategyPackageType();
			Assert(validation.Validate(instruction).IsEmpty);

			var otherPackage = instruction.OtherPackages.AddNew();

			otherPackage.CY_Code = PackageType.Codes.WoodBox;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			otherPackage.CY_Code = PackageType.Codes.WoodBarrel;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			otherPackage.CY_Code = PackageType.Codes.NaturalWood;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			otherPackage.CY_Code = PackageType.Codes.PlantAuxiliaryPadMaterial;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			otherPackage.CY_Code = ZString.Empty;
			Assert(validation.Validate(instruction).IsEmpty);

			instruction.CEI_PackageUQ = PackageType.Codes.WoodBox;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			instruction.CEI_PackageUQ = PackageType.Codes.WoodBarrel;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			instruction.CEI_PackageUQ = PackageType.Codes.NaturalWood;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			instruction.CEI_PackageUQ = PackageType.Codes.PlantAuxiliaryPadMaterial;
			AssertEquals(EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage, validation.Validate(instruction));

			instruction.CEI_PackageUQ = ZString.Empty;
			Assert(validation.Validate(instruction).IsEmpty);
		}
	}

	class TestEntryRequiresCIQStrategyCargoAttributes : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var cargoAttribute = Factory.New<CargoAttribute>();
			cargoAttribute.CY_ParentID = invoiceLine.PK;
			cargoAttribute.CY_ParentTableCode = invoiceLine.TablePrefix;
			cargoAttribute.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			invoiceLine.CargoAttributes.Add(cargoAttribute);

			var validation = new EntryRequiresCIQStrategyCargoAttributes();
			Assert(validation.Validate(instruction).IsEmpty);

			cargoAttribute.CY_Code = CargoAttributeList.Codes._21;
			AssertEquals(EntryRequiresCIQStrategyCargoAttributes.CargoAttributesRequireCIQMessage, validation.Validate(instruction));

			cargoAttribute.CY_Code = CargoAttributeList.Codes._11;
			Assert(validation.Validate(instruction).IsEmpty);

			cargoAttribute.CY_Code = CargoAttributeList.Codes._22;
			AssertEquals(EntryRequiresCIQStrategyCargoAttributes.CargoAttributesRequireCIQMessage, validation.Validate(instruction));

			cargoAttribute.CY_Code = ZString.Empty;
			Assert(validation.Validate(instruction).IsEmpty);
		}
	}

	class TestEntryRequiresCIQStrategyLCL : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.CusContainers.AddNew();

			var validation = new EntryRequiresCIQStrategyLCL();
			Assert(validation.Validate(instruction).IsEmpty);

			var container = instruction.EntryHeader.Containers[0];
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals(EntryRequiresCIQStrategyLCL.LCLRequireCIQMessage, validation.Validate(instruction));

			container.CO_FCL_LCL_AIR = ZString.Empty;
			Assert(validation.Validate(instruction).IsEmpty);
		}
	}

	class TestEntryRequiresCIQStrategyDangerousChemical : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "7664-41-7", "氨", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			anotherFactory.Save();

			var validation = new EntryRequiresCIQStrategyDangerousChemical();
			Assert(validation.Validate(instruction).IsEmpty);

			invoiceLine.JI_NameOfGoods = "氨";
			AssertEquals(EntryRequiresCIQStrategyDangerousChemical.DangerousChemicalRequireCIQMessage, validation.Validate(instruction));

			invoiceLine.JI_NameOfGoods = "";
			(invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage).AddNew(CargoAttributeList.Codes._31);
			AssertEquals(EntryRequiresCIQStrategyDangerousChemical.DangerousChemicalRequireCIQMessage, validation.Validate(instruction));
		}
	}
}
