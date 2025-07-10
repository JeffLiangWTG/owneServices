using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineValidationTest : BaseJobComInvoiceLineValidationTest
	{
		public void TestCheckJI_CustomsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "11111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warning = "The tariff does not require a unit of quantity";
				invoiceLine.JI_Tariff = "1111.11.11";
				invoiceLine.JI_CustomsUnitQty = "ZZ";
				AssertHasWarning(invoiceLine.JI_CustomsUnitQtyInfo, warning);

				invoiceLine.JI_CustomsUnitQty = ZString.Empty;
				AssertNoWarning(invoiceLine.JI_CustomsUnitQtyInfo, warning);
			}
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "11111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "22222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff2, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "ERR");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				invoiceLine.JI_Tariff = "1111.11.11";
				invoiceLine.JI_CustomsQuantity = 2;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'NR'", testInvoiceLine.JI_CustomsQuantityInfo);

				invoiceLine.JI_Tariff = "2222.22.22";
				invoiceLine.JI_CustomsQuantity = 3;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'ERR'", testInvoiceLine.JI_CustomsQuantityInfo);
			}
		}

		public void TestImportClassificationValidationWhenImport()
		{
			CheckImportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		}

		public void TestImportClassificationValidationWhenExWarehouse()
		{
			CheckImportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse);
		}

		public void TestImportClassificationValidationWhenWEA()
		{
			CheckImportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.WarehousedByExternalAgent);
		}

		public void TestImportClassificationValidationWhenDrawback()
		{
			CheckImportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.Drawback);
		}

		public void TestImportClassificationValidationWhenExternalBroker()
		{
			CheckImportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.ImportDeclarationByExternalBroker);
		}

		public void CheckImportClassificationValidation(string messageType)
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = messageType;
			JobComInvoiceLine line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			Classification @class = Classification.New(Factory);
			@class.CC_ClassificationType = Common.ClassificationType.IMP;
			line.JI_CC = @class.PK;
			AssertNoNotifications("IMP Class is valid for " + messageType, line.JI_CCInfo);
			@class.CC_ClassificationType = Common.ClassificationType.EXP;
			line.Validation.ValidateJI_CC();
			AssertHasNotifications("EXP Class is not valid for " + messageType, line.JI_CCInfo);
		}

		public void TestExportClassificationValidationWhenExport()
		{
			CheckExportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.Export);
		}

		public void TestExportClassificationValidationWhenQuarantine()
		{
			CheckExportClassificationValidation(Common.AU.AUJobMessageTypeList.Codes.Quarantine);
		}

		public void TestExportClassificationValidationWhenExternalBroker()
		{
			CheckExportClassificationValidation(Common.Shared.SharedJobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
		}

		public void CheckExportClassificationValidation(string messageType)
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = messageType;
			JobComInvoiceLine line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			Classification @class = Classification.New(Factory);
			@class.CC_ClassificationType = Common.ClassificationType.EXP;
			line.JI_CC = @class.PK;
			AssertNoNotifications("EXP Class is valid for " + messageType, line.JI_CCInfo);
			@class.CC_ClassificationType = Common.ClassificationType.IMP;
			line.Validation.ValidateJI_CC();
			AssertHasNotifications("IMP Class is not valid for " + messageType, line.JI_CCInfo);
		}

		public void TestChangingPartClearsValidationOnDescription()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.OH_IsConsignee, true));
			AUOrgSupplierPart product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.OP_Desc = "description";
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_Description = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, "Goods description is required for declaration");
			invoiceLine.JI_PartNo = "TestTEST";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, "Goods description is required for declaration");
		}

		public void TestCheckJI_WeightUQ()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_WeightUQ = "CS";
			AssertHasErrorContaining(invoiceLine.JI_WeightUQInfo, "Enter a valid Weight UQ");
			invoiceLine.JI_WeightUQ = "KG";
			AssertNoErrorContaining(invoiceLine.JI_WeightUQInfo, "Enter a valid Weight UQ");
		}

		public void TestCheckJI_NetWeightUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			AssertNoWarningContaining("Non quarantine job should not be validating this", invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");

			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.PoundsTroy;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilotonnes;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.MetricCarat;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.OuncesTroy;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.LongTons;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoWarningContaining(invoiceLine.JI_NetWeightUQInfo, "The selected Unit of Measure is not valid for NEXDOC and cannot be converted");
		}

		#region Implementation

		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLineValidationTestRig(invoiceLine);
		}
		#endregion
	}
}
