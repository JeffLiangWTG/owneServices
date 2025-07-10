using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public partial class JobSummaryAdapterTest : TestCaseWithFactory
	{
		public void TestImportChargesWhenDeactiveSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertSupplyTypeImportSenario(
				new AddChargesDelegate[] { AddChargeWithValidSupplyType, AddChargeWithEmptySupplyType, AddChargeWithInvalidSupplyType }
			, null, null, chargeCounts: new int[] { 1, 1, 1 });
		}

		public void TestImportChargesWithSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertSupplyTypeImportSenario(
				new AddChargesDelegate[] { AddChargeWithValidSupplyType, AddChargeWithEmptySupplyType }
			, null, null, chargeCounts: new int[] { 1, 1 });
		}

		public void TestImportChargesWithSupplyType_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=PSS Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Error - Cost Supply Type: Enter a valid Cost Supply Type.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Sell Reference: The system will group and post multiple invoices by Sell Reference when a value is entered.
Error - Sell Supply Type: Enter a valid Sell Supply Type.";

			AssertSupplyTypeImportSenario(
				new AddChargesDelegate[] { AddChargeWithInvalidSupplyType }
			, typeof(DataObjectReadFailureException), exceptionMessage, chargeCounts: new int[] { 1 });
		}

		public void TestImportChargesWithSupplyTypeMandatory()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertSupplyTypeImportSenario(
				new AddChargesDelegate[] { AddChargeWithValidSupplyType, AddChargeWithEmptySupplyType }
			, null, null, chargeCounts: new int[] { 1, 1 });
		}

		public void TestImportChargesWithSupplyTypeMandatory_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=PSS Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Error - Cost Supply Type: Enter a valid Cost Supply Type.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Sell Reference: The system will group and post multiple invoices by Sell Reference when a value is entered.
Error - Sell Supply Type: Enter a valid Sell Supply Type.";

			AssertSupplyTypeImportSenario(
					new AddChargesDelegate[] { AddChargeWithInvalidSupplyType }
				, typeof(DataObjectReadFailureException), exceptionMessage, chargeCounts: new int[] { 1 });
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

		void AssertSupplyTypeImportSenario(ICollection<AddChargesDelegate> addChargesDelegates, Type expectedExceptionType, string expectedExceptionMessage, string expectedExceptionCaption = null, int[] chargeCounts = null, bool lockShipmentBeforeImport = false, bool isJobClosedScenario = false, Action<BusinessObjectFactory> additionalAssertion = null, bool expectNoWarningsOnCharges = false)
		{
			testImportCharges((factory, creator, job, chargeLineCollection) =>
			{
				foreach (var addChargesDelegate in addChargesDelegates)
				{
					addChargesDelegate(factory, creator, job, chargeLineCollection);
				}
			}, expectedExceptionType, expectedExceptionMessage, expectedExceptionCaption, chargeCounts, lockShipmentBeforeImport, isJobClosedScenario, additionalAssertion = null, expectNoWarningsOnCharges);
		}

		void AddChargeWithValidSupplyType(BusinessObjectFactory factory, Business.TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_SellReference = "ABC123";

			ChargeLine chargeLine = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine.SellReference = "ABC456";
			chargeLine.CostSupplyType = new UniversalCodeDescriptionPair { Code = "DSB" };
			chargeLine.SellSupplyType = new UniversalCodeDescriptionPair { Code = "LOC" };
			chargeLineCollection.Add(chargeLine);

			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());

			var matchingCriteria = new MatchingCriteria();
			matchingCriteria.FieldName = "ChargeCode";
			matchingCriteria.Value = "FRT";
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() { matchingCriteria });
		}

		void AddChargeWithInvalidSupplyType(BusinessObjectFactory factory, Business.TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("PSS", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_SellReference = "ABC124";

			ChargeLine chargeLine = GetChargeLine("SYD", "PSS", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine.SellReference = "ABC456";
			chargeLine.CostSupplyType = new UniversalCodeDescriptionPair { Code = "INT" };
			chargeLine.SellSupplyType = new UniversalCodeDescriptionPair { Code = "AAA" };
			chargeLineCollection.Add(chargeLine);

			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());

			var matchingCriteria = new MatchingCriteria();
			matchingCriteria.FieldName = "ChargeCode";
			matchingCriteria.Value = "PSS";
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() { matchingCriteria });
		}

		void AddChargeWithEmptySupplyType(BusinessObjectFactory factory, Business.TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("WAR", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_SellReference = "ABC125";

			ChargeLine chargeLine = GetChargeLine("SYD", "WAR", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine.SellReference = "ABC456";
			chargeLine.CostSupplyType = null;
			chargeLine.SellSupplyType = new UniversalCodeDescriptionPair { Code = "" };
			chargeLineCollection.Add(chargeLine);

			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());

			var matchingCriteria = new MatchingCriteria();
			matchingCriteria.FieldName = "ChargeCode";
			matchingCriteria.Value = "WAR";
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() { matchingCriteria });
		}
	}
}
