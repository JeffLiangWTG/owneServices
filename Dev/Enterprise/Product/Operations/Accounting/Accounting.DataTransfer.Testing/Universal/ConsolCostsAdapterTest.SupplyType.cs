using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public partial class ConsolCostsAdapterTest : TestCaseWithFactory
	{
		public void TestImportChargesWhenDeactiveSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertSupplyTypeImportSenario(
				new AddConsolCostsDelegate[] { AddConsolCostWithValidSupplyType, AddConsolCostWithEmptySupplyType, AddConsolCostWithNullSupplyType, AddConsolCostWithInvalidSupplyType }
				, null);
			Assert("Has No Errors", !Logger.HasErrors);
		}

		public void TestImportChargesWithSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertSupplyTypeImportSenario(
				new AddConsolCostsDelegate[] { AddConsolCostWithValidSupplyType, AddConsolCostWithEmptySupplyType, AddConsolCostWithNullSupplyType }
			, null);
			Assert("Has No Errors", !Logger.HasErrors);
		}

		public void TestImportChargesWithSupplyType_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=WAR Creditor=AALSHI, Cost OS Amount=100.00
Error - Cost Supply Type: Enter a valid Cost Supply Type.
Warning - Sell Supply Type: The Sell Supply Type is not specified. Please check if a supply type is needed before posting.";
			AssertSupplyTypeImportSenario(
					new AddConsolCostsDelegate[] { AddConsolCostWithInvalidSupplyType }
				, exceptionMessage);
		}

		public void TestImportChargesWithSupplyTypeMandatory()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertSupplyTypeImportSenario(
				new AddConsolCostsDelegate[] { AddConsolCostWithValidSupplyType, AddConsolCostWithEmptySupplyType, AddConsolCostWithNullSupplyType }
			, null);
			Assert("Has No Errors", !Logger.HasErrors);
		}

		public void TestImportChargesWithSupplyTypeMandatory_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=WAR Creditor=AALSHI, Cost OS Amount=100.00
Error - Cost Supply Type: Enter a valid Cost Supply Type.
Warning - Sell Supply Type: The Sell Supply Type is not specified. Please check if a supply type is needed before posting.";
			AssertSupplyTypeImportSenario(
					new AddConsolCostsDelegate[] { AddConsolCostWithInvalidSupplyType }
				, exceptionMessage);
		}

		void DeactiveSomeSupplyType(params string[] supplyTypeCodes)
		{
			var settingCollection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;
			foreach (var supplyTypeCode in supplyTypeCodes)
			{
				var target = settingCollection.FindByCode(supplyTypeCode) as CodeDescriptionBool;
				if (target != null)
				{
					target.Bool = false;
				}
			}

			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settingCollection);
		}

		void AssertSupplyTypeImportSenario(ICollection<AddConsolCostsDelegate> addConsolCostDelegates, string exceptionMessage)
		{
			TestImportConsolCosts((factory, creator, job, chargeLineCollection) =>
			{
				foreach (var addConsolCostDelegate in addConsolCostDelegates)
				{
					addConsolCostDelegate(factory, creator, job, chargeLineCollection);
				}
			}, exceptionMessage);
		}

		void AddConsolCostWithValidSupplyType(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False,
																"JH", PlaceOfSupplyTypes.State.Code);
			consolCostLine.SupplyType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "DSB" };
			consolCostLineCollection.Add(consolCostLine);

			consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddConsolCostWithInvalidSupplyType(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine = GetConsolCostLine("WAR", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False,
																"JH", PlaceOfSupplyTypes.State.Code);
			consolCostLine.SupplyType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "INT" };
			consolCostLineCollection.Add(consolCostLine);

			consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddConsolCostWithNullSupplyType(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine = GetConsolCostLine("PSS", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False,
																"JH", PlaceOfSupplyTypes.State.Code);
			consolCostLine.SupplyType = null;
			consolCostLineCollection.Add(consolCostLine);

			consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddConsolCostWithEmptySupplyType(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine = GetConsolCostLine("BAF", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False,
																"JH", PlaceOfSupplyTypes.State.Code);
			consolCostLine.SupplyType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "" };
			consolCostLineCollection.Add(consolCostLine);

			consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;
		}
	}
}
