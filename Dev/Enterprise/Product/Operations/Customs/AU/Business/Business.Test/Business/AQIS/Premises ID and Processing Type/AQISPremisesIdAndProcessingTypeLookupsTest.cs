using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingTypeLookups;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISPremisesIdAndProcessingTypeLookupsTest : TestCaseWithFactory
	{
		public void TestAQISPremisesIdList()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var premisesId1 = CMRAqisPremises.New(Factory);
				premisesId1.QP_AQISPremisesIdentifier = "Code3";
				premisesId1.QP_AQISPremisesName = "A Description";
				var premisesId2 = CMRAqisPremises.New(Factory);
				premisesId2.QP_AQISPremisesIdentifier = "Code2";
				premisesId2.QP_AQISPremisesName = "B Description";
				var premisesId3 = CMRAqisPremises.New(Factory);
				premisesId3.QP_AQISPremisesIdentifier = "Code1";
				premisesId3.QP_AQISPremisesName = "C Description";
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var premiseIDList = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew().Lookups.AQISPremisesIdList as CMRAqisPremisesCollection;
				premiseIDList.Load();
				AssertEquals("Elements are sorted by description", "Code3Code2Code1", $"{premiseIDList[0].QP_AQISPremisesIdentifier}{premiseIDList[1].QP_AQISPremisesIdentifier}{premiseIDList[2].QP_AQISPremisesIdentifier}");
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var universalHelper = new UniversalReferenceTestDataHelper(newFactory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRAP, "AQIS Premises Id", Core.Constants.CountryCodes.Australia);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAP, "Code3", "A Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAP, "Code2", "B Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAP, "Code1", "C Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				newFactory.Save();

				var declaration = newFactory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var premiseIDList = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew().Lookups.AQISPremisesIdList as ZZRefCusCodeListCombinedCollection;
				premiseIDList.Load();
				AssertEquals("Elements are sorted by description", "Code3Code2Code1", $"{premiseIDList[0].ZZD_Code}{premiseIDList[1].ZZD_Code}{premiseIDList[2].ZZD_Code}");
				AssertEquals("Attribute Name is default in filter", true, premiseIDList.FilterBusinessObjectDefaults.ContainsDefaultFor($"{Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName}{FilterBusinessObjectDefault.FilterPropertyDelimiter}Property"));
				AssertEquals("Attribute Value is default in filter", true, premiseIDList.FilterBusinessObjectDefaults.ContainsDefaultFor($"{Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue}{FilterBusinessObjectDefault.FilterPropertyDelimiter}Property"));
			}
		}

		public void TestAQISProcessingTypeListDefault()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var fCLProcessingType = CMRAqisProcessingType.New(Factory);
				fCLProcessingType.QT_AQISProcessingType = "FCLType";
				fCLProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.FCL;
				fCLProcessingType.QT_AQISProcessingDescription = "FCL Description";

				var lCLProcessingType = CMRAqisProcessingType.New(Factory);
				lCLProcessingType.QT_AQISProcessingType = "LCLType";
				lCLProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.LCL;
				lCLProcessingType.QT_AQISProcessingDescription = "LCL Description";

				var airProcessingType = CMRAqisProcessingType.New(Factory);
				airProcessingType.QT_AQISProcessingType = "AIRType";
				airProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.AIR;
				airProcessingType.QT_AQISProcessingDescription = "Air Description";

				var breakBulkProcessingType = CMRAqisProcessingType.New(Factory);
				breakBulkProcessingType.QT_AQISProcessingType = "BBKType";
				breakBulkProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.BreakBulk;
				breakBulkProcessingType.QT_AQISProcessingDescription = "BBK Description";

				var bulkProcessingType = CMRAqisProcessingType.New(Factory);
				bulkProcessingType.QT_AQISProcessingType = "BLKType";
				bulkProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.Bulk;
				bulkProcessingType.QT_AQISProcessingDescription = "BLK Description";

				var liquidProcessingType = CMRAqisProcessingType.New(Factory);
				liquidProcessingType.QT_AQISProcessingType = "LQDType";
				liquidProcessingType.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.Liquid;
				liquidProcessingType.QT_AQISProcessingDescription = "LQD Description";

				Factory.Save();

				var declaration = JobDeclaration.New(Factory);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var processingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
				var lookups = processingType.Lookups;

				var processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.FCL", "FCLType", processingTypeList.GetCodeFromDescription("FCL Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.LCL", "LCLType", processingTypeList.GetCodeFromDescription("LCL Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.FCLMixedShipper", "FCLType", processingTypeList.GetCodeFromDescription("FCL Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.BreakBulk", "BBKType", processingTypeList.GetCodeFromDescription("BBK Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.Bulk", "BLKType", processingTypeList.GetCodeFromDescription("BLK Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("ContainerModes.Liquid", "LQDType", processingTypeList.GetCodeFromDescription("LQD Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("Has none in list", 0, processingTypeList.Count);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				processingTypeList = lookups.AQISProcessingTypeList;
				AssertEquals("TransportModes.Air", "AIRType", processingTypeList.GetCodeFromDescription("AIR Description"));
				AssertEquals("Has only one in list", 1, processingTypeList.Count);

				AssertSame("list is cached", processingTypeList, lookups.AQISProcessingTypeList);
			}
		}

		public void TestAQISProcessingTypeSortOrderDefault()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var fCLProcessingType1 = CMRAqisProcessingType.New(Factory);
				fCLProcessingType1.QT_AQISProcessingType = "FCLType";
				fCLProcessingType1.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.FCL;
				fCLProcessingType1.QT_AQISProcessingDescription = "FCL Description";

				var fCLProcessingType2 = CMRAqisProcessingType.New(Factory);
				fCLProcessingType2.QT_AQISProcessingType = "A FCLType";
				fCLProcessingType2.QT_AQISProcessingCargoType = AQISPremisesIdAndProcessingTypeLookups.AqisAepProcessingCategories.FCL;
				fCLProcessingType2.QT_AQISProcessingDescription = "A FCL Description";

				Factory.Save();

				var declaration = JobDeclaration.New(Factory);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var processingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();

				var processingTypeList = processingType.Lookups.AQISProcessingTypeList;
				AssertEquals("First description", "A FCL Description", processingTypeList[0].Description);
				AssertEquals("Second description", "FCL Description", processingTypeList[1].Description);
				AssertEquals("Has two in list", 2, processingTypeList.Count);
			}
		}

		public void TestAQISProcessingTypeListRefDatabase() => CombineAssertions(() =>
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var universalHelper = new UniversalReferenceTestDataHelper(Factory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRPT, "AQIS Processing Type", Core.Constants.CountryCodes.Australia);
				AddProcessingType(universalHelper, "CODE 1", "Description C", AqisAepProcessingCategories.FCL);
				AddProcessingType(universalHelper, "CODE 2", "Description A", AqisAepProcessingCategories.LCL);
				AddProcessingType(universalHelper, "CODE 3", "Description B", AqisAepProcessingCategories.FCL);
				Factory.Save();

				var declaration = JobDeclaration.New(Factory);
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var processingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
				var lookups = processingType.Lookups;

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("CargoType 'FCL'", "CODE 3 - Description B\r\nCODE 1 - Description C", lookups.AQISProcessingTypeList.ElementsAsString);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("CargoType 'LCL'", "CODE 2 - Description A", lookups.AQISProcessingTypeList.ElementsAsString);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
				AssertEquals(0, lookups.AQISProcessingTypeList.Count);
			}
		});

		static void AddProcessingType(UniversalReferenceTestDataHelper universalHelper, ZString code, ZString description, ZString cargoType)
		{
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPT, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, AUConstants.RefCusCodeAttributesNames.AQISProcessingCargoType, cargoType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			TestCaseHelper.ClearTable(CMRAqisProcessingType.Schema.TableName);
		}
	}
}
