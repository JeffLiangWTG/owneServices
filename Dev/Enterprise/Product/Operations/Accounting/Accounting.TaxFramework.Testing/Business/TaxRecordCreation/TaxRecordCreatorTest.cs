using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class TaxRecordCreatorTest : TestCaseWithFactory
	{
		public void TestUpdatePostDate()
		{
			ITaxRecordCreator taxRecordCreator = new TaxRecordCreator();

			var originalPostDate = ZDate.Today.AddDays(5);
			var originalRealisationDate = ZDate.Today.AddDays(7);

			foreach (var basis in new TaxBasisList().GetAllCodes())
			{
				var taxParentMock = CreateITaxRecordParentMock(Factory);
				var taxRecordPK = ZGuid.NewZGuid();
				taxParentMock.SetupGet(x => x.PK).Returns(taxRecordPK);
				var taxRecord1 = Factory.New<AccTaxTransaction>();
				taxRecord1.ATT_AH = taxRecordPK;
				taxRecord1.ATT_PostDate = originalPostDate;
				taxRecord1.ATT_RealisationDate = originalRealisationDate;
				var taxRecord2 = Factory.New<AccTaxTransaction>();
				taxRecord2.ATT_AH = taxRecordPK;
				taxRecord2.ATT_PostDate = originalPostDate;
				taxRecord2.ATT_RealisationDate = originalRealisationDate;
				var taxRecord3 = Factory.New<AccTaxTransaction>();
				taxRecord3.ATT_AH = ZGuid.NewZGuid();
				taxRecord3.ATT_PostDate = originalPostDate;
				taxRecord3.ATT_RealisationDate = originalRealisationDate;

				var newPostDate = ZDate.Today;
				taxParentMock.SetupGet(x => x.PostDate).Returns(() => newPostDate);
				newPostDate = newPostDate.AddDays(-1);
				var newRealisationDate = basis == TaxBasisList.Posting.Code ? newPostDate : originalRealisationDate;
				taxRecord1.ATT_Basis = basis;
				taxRecord2.ATT_Basis = basis;
				taxRecord3.ATT_Basis = basis;
				taxRecordCreator.UpdatePostDate(taxParentMock.Object);

				AssertEquals($"Basis: {basis}", newPostDate, taxRecord1.ATT_PostDate);
				AssertEquals($"Basis: {basis}", newRealisationDate, taxRecord1.ATT_RealisationDate);

				AssertEquals($"Basis: {basis}", newPostDate, taxRecord2.ATT_PostDate);
				AssertEquals($"Basis: {basis}", newRealisationDate, taxRecord2.ATT_RealisationDate);

				AssertEquals($"Basis: {basis}", originalPostDate, taxRecord3.ATT_PostDate);
				AssertEquals($"Basis: {basis}", originalRealisationDate, taxRecord3.ATT_RealisationDate);
			}
		}

		public void TestTaxRecordParent_SetTransactionHeaderBranch()
		{
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today, GlbBranch.CurrentBranch, TestObjectCreator.OverheadChargeCode);

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock.Object });
			taxRecordParentMock.SetupGet(f => f.Org).Returns(TestObjectCreator.TestOrganisation);

			var taxRecordCalculatorMock = new Mock<ITaxRecordCalculator>();
			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalculatorMock.Object);

			bool isSetTransactionHeaderBranchCalled = false;
			bool whatTaxRecordCalculatorGets = false;

			taxRecordParentMock.Setup(x => x.SetTransactionHeaderBranch()).Callback(() => isSetTransactionHeaderBranchCalled = true);

			taxRecordCalculatorMock.Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>())).Callback(() => whatTaxRecordCalculatorGets = isSetTransactionHeaderBranchCalled);

			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);
			Assert("SetTransactionHeaderBranch is called before tax calculation", whatTaxRecordCalculatorGets);

			taxRecordParentMock.Verify(x => x.SetTransactionHeaderBranch(), Times.Once);
		}

		public void TestThresholdAmountProcessor()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;
			var ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			var companyData = org.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, ledger);
			Factory.Save();

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var taxFrameTaxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "Code1";
			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Desc";
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

			var taxFrameTaxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "Code2";
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Desc";
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode);

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource("TID");
			taxID1.SetRate_ForTestOnly(10, 1);

			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, jobType: AccountingMasterFilesConstants.JobTypes.NonJobRelated);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID1.PK, jobType: AccountingMasterFilesConstants.JobTypes.NonJobRelated);
			Factory.Save();

			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today, GlbBranch.CurrentBranch, chargeCode);
			lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(new AccChargeTaxOverrideMatcher.TaxCalculationParameters());
			lineTaxableTransactionMock.SetupGet(r => r.BaseOSAmount).Returns(120m);
			lineTaxableTransactionMock.SetupGet(r => r.LocalAmount).Returns(100m);

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(ledger);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(org);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock.Object });

			var taxRecordCreator = new TaxRecordCreator();
			var thresholdAmountProcessorMock = new Mock<IThresholdProcessor>(MockBehavior.Strict);
			var thresholdMethodCodes = new[] { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			AccTaxTransaction[] passedTaxRecords = null;
			thresholdAmountProcessorMock.Setup(x => x.Process(taxRecordCreator.TaxRecordPivotProcessor_ExposedForTestOnly, taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes.ToHashSet())).
				Callback((ITaxRecordPivotProcessor pivotProcessor, ITaxRecordParent parent, List<AccTaxTransaction> taxRecords, IEnumerable<string> thresholdMethods) => AssertPassedTaxRecords(parent, taxRecords));

			taxRecordCreator.SubstituteThresholdAmountProcessor_ForTestOnly(thresholdAmountProcessorMock.Object);
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);

			AssertEquals("Postcondition: tax transaction count", 2, passedTaxRecords.Length);
			taxRecordParentMock.VerifySet(x => x.OSTaxAmount = It.IsAny<ZDecimal>(), Times.Once);
			taxRecordParentMock.VerifySet(x => x.LocalTaxAmount = It.IsAny<ZDecimal>(), Times.Once);

			thresholdAmountProcessorMock.Verify(x => x.Process(taxRecordCreator.TaxRecordPivotProcessor_ExposedForTestOnly, taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes.ToHashSet()), Times.Once);

			void AssertPassedTaxRecords(ITaxRecordParent parent, List<AccTaxTransaction> taxRecords)
			{
				AssertEquals("Precondition: tax transaction count", 2, taxRecords.Count);
				AssertTaxRecordCreated(taxRecords[0]);
				AssertTaxRecordCreated(taxRecords[1]);
				taxRecordParentMock.VerifySet(x => x.OSTaxAmount = It.IsAny<ZDecimal>(), Times.Never);
				taxRecordParentMock.VerifySet(x => x.LocalTaxAmount = It.IsAny<ZDecimal>(), Times.Never);

				passedTaxRecords = taxRecords.ToArray();
			}

			void AssertTaxRecordCreated(AccTaxTransaction taxRecord)
			{
				AssertEquals("ATT_Ledger", LedgerTypes.AccountsPayable, taxRecord.ATT_Ledger);
				AssertEquals("ATT_AT_TaxID", taxID1.PK, taxRecord.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", taxConfigCompany.PK, taxRecord.ATT_ETC);
				AssertEquals("ATT_RateNumerator", 10, taxRecord.ATT_RateNumerator);
				AssertEquals("ATT_RateDenominator", 1, taxRecord.ATT_RateDenominator);
				AssertEquals("ATT_OSTaxAmount", 12m, taxRecord.ATT_OSTaxAmount);
				AssertEquals("ATT_LocalTaxAmount", 10m, taxRecord.ATT_LocalTaxAmount);
			}
		}

		#region GetEstimatedTaxRecords

		public void TestGetEstimatedTaxRecordsWhenThresholdAmountProcessor()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var linePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var linePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();
			var linePivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord4 = Factory.New<AccTaxTransaction>();
			var linePivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();

			var taxRecordsWithLinePivots_withoutTaxSystemCodesFilter = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots_withoutTaxSystemCodesFilter.Add((taxRecord1, linePivot1));
			taxRecordsWithLinePivots_withoutTaxSystemCodesFilter.Add((taxRecord2, linePivot2));

			var taxRecordsWithLinePivots_withTaxSystemCodesFilter = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots_withTaxSystemCodesFilter.Add((taxRecord3, linePivot3));
			taxRecordsWithLinePivots_withTaxSystemCodesFilter.Add((taxRecord4, linePivot4));

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null)).Returns(taxRecordsWithLinePivots_withoutTaxSystemCodesFilter);
			var taxSystemsFilter = new List<ZString> { "TAX" };
			taxRecordCalcualtorMock.Setup(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, taxSystemsFilter)).Returns(taxRecordsWithLinePivots_withTaxSystemCodesFilter);

			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			var thresholdProcessorMock = new Mock<IThresholdProcessor>();

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteThresholdAmountProcessor_ForTestOnly(thresholdProcessorMock.Object);

			var taxRecords_withoutTaxSystemCodesFilter = new List<AccTaxTransaction> { taxRecord1, taxRecord2 };
			thresholdProcessorMock.Setup(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, taxRecords_withoutTaxSystemCodesFilter, thresholdMethodCodes))
				.Callback((ITaxRecordParentBase parent, List<AccTaxTransaction> taxRecords, IEnumerable<string> thresholdMethods) => { AssertPassedTaxRecords(taxRecords, taxRecordsWithLinePivots_withoutTaxSystemCodesFilter); })
				.Returns(new HashSet<AccTaxTransaction>() { taxRecord2 });

			var taxRecords_withTaxSystemCodesFilter = new List<AccTaxTransaction> { taxRecord3, taxRecord4 };
			thresholdProcessorMock.Setup(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, taxRecords_withTaxSystemCodesFilter, thresholdMethodCodes))
				.Callback((ITaxRecordParentBase parent, List<AccTaxTransaction> taxRecords, IEnumerable<string> thresholdMethods) => { AssertPassedTaxRecords(taxRecords, taxRecordsWithLinePivots_withTaxSystemCodesFilter); })
				.Returns(new HashSet<AccTaxTransaction>() { taxRecord4 });

			var result = ((ITaxRecordCreator)taxRecordCreator).GetEstimatedTaxRecords(taxRecordParentMock.Object, null);
			AssertEquals("1 tuples should be returned", 1, result.Count);
			var expectedTupleList = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedTupleList.Add((taxRecord1, linePivot1));
			AssertContainsExactElementsInAnyOrder("GetApplicableTaxRecords", expectedTupleList, result);

			taxRecordCalcualtorMock.Verify(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null), Times.Once);
			thresholdProcessorMock.Verify(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, taxRecords_withoutTaxSystemCodesFilter, thresholdMethodCodes), Times.Once);

			result = ((ITaxRecordCreator)taxRecordCreator).GetEstimatedTaxRecords(taxRecordParentMock.Object, taxSystemsFilter);
			AssertEquals("1 tuples should be returned", 1, result.Count);
			expectedTupleList = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedTupleList.Add((taxRecord3, linePivot3));
			AssertContainsExactElementsInAnyOrder("GetApplicableTaxRecords", expectedTupleList, result);

			taxRecordCalcualtorMock.Verify(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, taxSystemsFilter), Times.Once);
			thresholdProcessorMock.Verify(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, taxRecords_withTaxSystemCodesFilter, thresholdMethodCodes), Times.Once);

			void AssertPassedTaxRecords(List<AccTaxTransaction> passedTaxRecords, List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> expectedResult)
			{
				AssertEquals("There should be 2 tax transactions", 2, passedTaxRecords.Count);
				AssertContainsExactElementsInAnyOrder(expectedResult.Select(x => x.Item1).ToList(), passedTaxRecords);
			}
		}

		public void TestGetEstimatedTaxRecords_WhenTaxRecordCalculatorReturnsEmptyListUsingThresholdAmountProcessor()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null)).Returns(new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>());

			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			var thresholdProcessorMock = new Mock<IThresholdProcessor>();
			thresholdProcessorMock.Setup(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes))
				.Returns(It.IsAny<HashSet<AccTaxTransaction>>());

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteThresholdAmountProcessor_ForTestOnly(thresholdProcessorMock.Object);

			var result = ((ITaxRecordCreator)taxRecordCreator).GetEstimatedTaxRecords(taxRecordParentMock.Object, null);
			AssertEquals("No tuples should be returned", 0, result.Count);

			taxRecordCalcualtorMock.Verify(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null), Times.Once);
			thresholdProcessorMock.Verify(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes), Times.Never);
		}

		public void TestGetEstimatedTaxRecords_WhenThresholdAmountProcessorReturnsEmptyList()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var linePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var linePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();
			var linePivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();

			var taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots.Add((taxRecord1, linePivot1));
			taxRecordsWithLinePivots.Add((taxRecord2, linePivot2));
			taxRecordsWithLinePivots.Add((taxRecord3, linePivot3));

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null)).Returns(taxRecordsWithLinePivots);

			HashSet<AccTaxTransaction> taxRecordsOutsideOfThreshold = new HashSet<AccTaxTransaction>();
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			var thresholdProcessorMock = new Mock<IThresholdProcessor>();
			thresholdProcessorMock.Setup(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes))
				.Returns(taxRecordsOutsideOfThreshold);

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteThresholdAmountProcessor_ForTestOnly(thresholdProcessorMock.Object);

			var result = ((ITaxRecordCreator)taxRecordCreator).GetEstimatedTaxRecords(taxRecordParentMock.Object, null);
			AssertEquals("3 tuples should be returned", 3, result.Count);

			var expectedTupleList = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedTupleList.Add((taxRecord1, linePivot1));
			expectedTupleList.Add((taxRecord2, linePivot2));
			expectedTupleList.Add((taxRecord3, linePivot3));
			AssertContainsExactElementsInAnyOrder("GetApplicableTaxRecords", expectedTupleList, result);

			taxRecordCalcualtorMock.Verify(x => x.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, null), Times.Once);
			thresholdProcessorMock.Verify(x => x.GetTaxRecordsOutsideOfThreshold(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes), Times.Once);
		}

		#endregion

		#region CreateTaxRecords

		public void TestCreateTaxRecordsValidatesDuplicatedTaxSystems()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();

			var taxRecords = new[] { taxRecord1, taxRecord2, taxRecord3 };

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(TestObjectCreator.TestOrganisation);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()))
				.Callback((ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecordList) => taxRecordList.AddRange(taxRecords));

			Func<IReadOnlyCollection<AccTaxTransaction>, bool> isAllTaxRatesPassed = passedCollection =>
			{
				AssertContainsExactElementsInAnyOrder(taxRecords, passedCollection);

				return true;
			};
			var taxRecordCollectionValidatorMock = new Mock<ITaxRecordCollectionValidator>();
			taxRecordCollectionValidatorMock.Setup(x => x.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(It.Is<IReadOnlyCollection<AccTaxTransaction>>(passedCollection => isAllTaxRatesPassed(passedCollection)))).Returns(new[] { taxRecord1, taxRecord3 });

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteTaxRecordCollectionValidator_ForTestOnly(taxRecordCollectionValidatorMock.Object);

			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);
			const string expectedError = "It is not allowed to have more than one Tax record for this Tax System per transaction.";
			AssertHasRowError(taxRecord1, expectedError);
			AssertNoRowErrors(taxRecord2);
			AssertHasRowError(taxRecord3, expectedError);

			taxRecordCollectionValidatorMock.Setup(x => x.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(taxRecords)).Returns(Array.Empty<AccTaxTransaction>());
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);
			AssertHasRowError("We do not clear error messages as it is not required. In real life TaxSystem is not changed. WE just create new tax records every time.", taxRecord1, expectedError);
			AssertNoRowErrors("We do not clear error messages as it is not required. In real life TaxSystem is not changed. WE just create new tax records every time.", taxRecord2);
			AssertHasRowError("We do not clear error messages as it is not required. In real life TaxSystem is not changed. WE just create new tax records every time.", taxRecord3, expectedError);

			taxRecord1.RemoveRowError(expectedError);
			taxRecord3.RemoveRowError(expectedError);
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);
			AssertNoRowErrors(taxRecord1);
			AssertNoRowErrors(taxRecord2);
			AssertNoRowErrors(taxRecord3);
		}

		public void TestCreateTaxRecords_TaxRecoveryLineCreator()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;
			var ledger = TaxConfigurationLedgers.AccountsPayable.Code;

			var companyData = org.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, ledger);
			Factory.Save();

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var taxFrameTaxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "Code1";
			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Desc";
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

			var taxFrameTaxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "Code2";
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Desc";
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode);

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource("TID");
			taxID1.SetRate_ForTestOnly(10, 1);

			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, jobType: AccountingMasterFilesConstants.JobTypes.NonJobRelated);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID1.PK, jobType: AccountingMasterFilesConstants.JobTypes.NonJobRelated);
			Factory.Save();

			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today, GlbBranch.CurrentBranch, chargeCode);
			lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(new AccChargeTaxOverrideMatcher.TaxCalculationParameters());
			lineTaxableTransactionMock.SetupGet(r => r.BaseOSAmount).Returns(120m);
			lineTaxableTransactionMock.SetupGet(r => r.LocalAmount).Returns(100m);

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(ledger);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(org);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock.Object });

			var taxRecordCreator = new TaxRecordCreator();

			var taxRecoveryLineCreator = new Mock<ITaxRecoveryLineCreator>(MockBehavior.Strict);
			AccTaxTransaction[] passedTaxRecords = null;
			taxRecoveryLineCreator.Setup(x => x.CreateLines(taxRecordCreator.TaxRecordPivotProcessor_ExposedForTestOnly, taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>())).
				Callback((ITaxRecordPivotProcessor pivotCreator, ITaxRecordParent parent, AccTaxTransaction[] taxRecords) => AssertPassedTaxRecords(parent, taxRecords));

			taxRecordCreator.SubstituteTaxRecoveryLineCreator_ForTestOnly(taxRecoveryLineCreator.Object);
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);

			AssertEquals("Postcondition: tax transaction count", 2, passedTaxRecords.Length);

			void AssertPassedTaxRecords(ITaxRecordParent parent, AccTaxTransaction[] taxRecords)
			{
				AssertEquals("Precondition: tax transaction count", 2, taxRecords.Length);
				AssertTaxRecordCreated(taxRecords[0]);
				AssertTaxRecordCreated(taxRecords[1]);

				passedTaxRecords = taxRecords;
			}

			void AssertTaxRecordCreated(AccTaxTransaction taxRecord)
			{
				AssertEquals("ATT_Ledger", LedgerTypes.AccountsPayable, taxRecord.ATT_Ledger);
				AssertEquals("ATT_AT_TaxID", taxID1.PK, taxRecord.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", taxConfigCompany.PK, taxRecord.ATT_ETC);
				AssertEquals("ATT_RateNumerator", 10, taxRecord.ATT_RateNumerator);
				AssertEquals("ATT_RateDenominator", 1, taxRecord.ATT_RateDenominator);
				AssertEquals("ATT_OSTaxAmount", 12m, taxRecord.ATT_OSTaxAmount);
				AssertEquals("ATT_LocalTaxAmount", 10m, taxRecord.ATT_LocalTaxAmount);

				var taxRecordPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK) { FetchOnlyFromLocalCache = true });
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { lineTaxableTransactionMock.Object.PK }, taxRecordPivots.Select(x => x.ATP_AL_TransactionLine).ToArray());
			}
		}

		public void TestCreateTaxRecordsValidatesCreatedRecords()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;
			var ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode, ledger, TaxRateSources.OrganisationOnly.Code, false, "OT1");

			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today, GlbBranch.CurrentBranch, chargeCode);
			lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(new AccChargeTaxOverrideMatcher.TaxCalculationParameters());

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(ledger);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(org);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock.Object });

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			taxRecordCreator.CreateTaxRecords(taxRecordParentMock.Object);

			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
			AssertEquals("Postcondition: tax transaction count", 1, taxRecords.Length);
			var taxRecord = taxRecords[0];
			AssertHasError(taxRecord.ATT_RateInfo, "No valid tax rate found for tax ID 'OT1'. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.");
		}

		[TestDate(2020, 01, 15)]
		public void TestCreateTaxRecords_WithInvoice()
		{
			SetupAndAssertTestForCreateTaxRecords(typeof(APInvoice), -1);
		}

		[TestDate(2020, 01, 15)]
		public void TestCreateTaxRecords_WithCreditNote()
		{
			SetupAndAssertTestForCreateTaxRecords(typeof(APCreditNote));
		}

		void SetupAndAssertTestForCreateTaxRecords(Type typeOfInvoice, int multiplier = 1)
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
			var companyData = org.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var numerator = 10;
			var denominator = 1;
			taxID1.SetRate_ForTestOnly(numerator, denominator);

			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeOfInvoice, "AP001", TestObjectCreator.AUD, 1M, org);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode1, 100M, TestObjectCreator.AUD, 1M);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode1, 200M, TestObjectCreator.AUD, 1M);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			CombineAssertions("Precondition: total invoice amounts before other tax calculation", () =>
			{
				AssertEquals("AH_OSTotalAmount", 330M, invoice.AH_OSTotalAmount);
				AssertEquals("AH_LocalTotalAmount", 330M, invoice.AH_LocalTotalAmount);
			});

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			taxRecordCreator.CreateTaxRecords(taxRecordParent);

			var expectedOSOtherTaxAmount = 30M * multiplier;
			var expectedLocalOtherTaxAmount = 30M * multiplier;
			var expectedTotalOSAmountForDisplay = 360M;
			var expectedTotalLocalAmountForDisplay = 360M;
			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
			AssertTaxRecordsCreated(taxRecords, taxSystem, taxConfigCompany, taxID1, numerator, denominator, taxRecordParent, invoice, new[] { line1, line2 }, expectedOSOtherTaxAmount, expectedLocalOtherTaxAmount
				, expectedTotalOSAmountForDisplay, expectedTotalLocalAmountForDisplay);

			taxRecordCreator.DeleteTaxRecordsNotInDB(taxRecordParent);

			CombineAssertions("Total invoice amounts goes back to before other tax calculation", () =>
			{
				AssertEquals("AH_OSTotalAmount", 330M, invoice.AH_OSTotalAmount);
				AssertEquals("AH_LocalTotalAmount", 330M, invoice.AH_LocalTotalAmount);
			});
		}

		[TestDate(2020, 01, 15)]
		public void TestCreateTaxRecords_AdditionalTaxRuleThatDoesNotApply()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
			var companyData = org.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var numerator1 = 10;
			var denominator1 = 1;
			taxID1.SetRate_ForTestOnly(numerator1, denominator1);

			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var numerator2 = 15;
			var denominator2 = 1;
			taxID2.SetRate_ForTestOnly(numerator2, denominator2);

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);

			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK, jobType: "FCN"); //this rule does not apply to the invoice

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode1, 100M, TestObjectCreator.AUD, 1M);

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			taxRecordCreator.CreateTaxRecords(taxRecordParent);

			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
			AssertTaxRecordsCreated(taxRecords, taxSystem, taxConfigCompany, taxID1, numerator1, denominator1, taxRecordParent, invoice, new[] { line }, -10M, -10M, 120M, 120M);
		}

		[TestDate(2020, 06, 30)]
		public void TestCreateTaxRecords_PopulatesOriginalValues()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;
			var ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			var currency = TestObjectCreator.AUD;
			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode, ledger, TaxRateSources.TaxIDOnly.Code, false, "OT1");

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), currency, 1M, org, ZDate.Today);
			TestObjectCreator.CreateInvoiceLine(apInvoice, chargeCode.PK, 100M, currency, 1M);

			var taxRecordCreator = new TaxRecordCreator();
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(apInvoice));

			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
			AssertEquals(1, taxRecords.Length);
			var taxRecord = taxRecords[0];

			var systemCalculatedValues = taxRecord.GetSystemCalculatedValuesIfAvailable();
			AssertNotNull(systemCalculatedValues);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("ATT_OSTaxBaseAmount populated", -100M, taxRecord.ATT_OSTaxBaseAmount);
				AssertEquals(taxRecord.ATT_OSTaxBaseAmount, systemCalculatedValues.OSTaxBaseAmount);

				AssertEquals("ATT_OSTaxAmount populated", -10M, taxRecord.ATT_OSTaxAmount);
				AssertEquals(taxRecord.ATT_OSTaxAmount, systemCalculatedValues.OSTaxAmount);

				AssertEquals("ATT_RateNumerator populated", 10, taxRecord.ATT_RateNumerator);
				AssertEquals(taxRecord.ATT_RateNumerator, systemCalculatedValues.RateNumerator);

				AssertEquals("ATT_RateDenominator populated", 1, taxRecord.ATT_RateDenominator);
				AssertEquals(taxRecord.ATT_RateDenominator, systemCalculatedValues.RateDenominator);

				AssertEquals("ATT_TaxDate populated", "30-Jun-20", taxRecord.ATT_TaxDate.ToShortDateString());
				AssertEquals(taxRecord.ATT_TaxDate, systemCalculatedValues.TaxDate);

				AssertEquals("ATT_TaxAuthorityServiceCode populated", "5.2020(809)", taxRecord.ATT_TaxAuthorityServiceCode);
				AssertEquals(taxRecord.ATT_TaxAuthorityServiceCode, systemCalculatedValues.TaxAuthorityServiceCode);

				AssertEquals("ATT_TaxAuthorityServiceCodeDescription populated", "Legislation 5.2020(809)", taxRecord.ATT_TaxAuthorityServiceCodeDescription);
				AssertEquals(taxRecord.ATT_TaxAuthorityServiceCodeDescription, systemCalculatedValues.TaxAuthorityServiceCodeDescription);
			});
		}

		public void TestCreateTaxRecords_WithGLAccountLines()
		{
			var company = GlbCompany.CurrentCompany;
			var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
			var companyData = org.GetCompanyDataForGlbCompany(company);
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");

			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode1, LedgerTypes.AccountsPayable);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 0M, TestObjectCreator.GLHeader1.PK);

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			AssertNoExceptionThrown("No exception should be thrown", () => taxRecordCreator.CreateTaxRecords(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice)));

			AssertNoTaxRecordsCreated(invoice, line);
		}

		void AssertTaxRecordsCreated(AccTaxTransaction[] returnedTaxRecrods, TaxSystemsConfiguration taxSystem, AccTaxConfiguration taxConfigCompany, AccTaxRate taxID1, int numerator, int denominator
			, ITaxRecordParent expectedTaxParent, InvoicingBase invoice, InvoicingLineBase[] lines, ZDecimal expectedOSOtherTaxAmount, ZDecimal expectedLocalOtherTaxAmount, ZDecimal expectedOSTotalAmount, ZDecimal expectedOSLocalTotal)
		{
			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());

			AssertEquals("1 tax transaction should be created", 1, taxRecords.Length);
			AssertContainsExactElementsInAnyOrder(nameof(returnedTaxRecrods), taxRecords, returnedTaxRecrods);
			var taxRecord = taxRecords[0];

			AssertEquals("TaxParent", expectedTaxParent, taxRecord.TaxParent);
			AssertEquals("ATT_AH", invoice.PK, taxRecord.ATT_AH);
			AssertEquals("ATT_GC", invoice.AH_GC, taxRecord.ATT_GC);
			AssertEquals("ATT_Ledger", LedgerTypes.AccountsPayable, taxRecord.ATT_Ledger);
			AssertEquals("ATT_AT_TaxID", taxID1.PK, taxRecord.ATT_AT_TaxID);
			AssertEquals("ATT_ETC", taxConfigCompany.PK, taxRecord.ATT_ETC);
			AssertEquals("ATT_TaxSystemCode", taxSystem.Code, taxRecord.ATT_TaxSystemCode);
			AssertEquals("ATT_AffectsSourceTransactionTotal", taxSystem.IncludeInInvoceTotal, taxRecord.ATT_AffectsSourceTransactionTotal);
			AssertEquals("ATT_RateNumerator", numerator, taxRecord.ATT_RateNumerator);
			AssertEquals("ATT_RateDenominator", denominator, taxRecord.ATT_RateDenominator);
			AssertEquals("ATT_OSTaxAmount", expectedOSOtherTaxAmount, taxRecord.ATT_OSTaxAmount);
			AssertEquals("ATT_LocalTaxAmount", expectedLocalOtherTaxAmount, taxRecord.ATT_LocalTaxAmount);

			CombineAssertions("Other tax amounts after other tax calculation", () =>
			{
				AssertEquals("AH_OSTaxAmountOtherTaxes", expectedOSOtherTaxAmount, invoice.AH_OSTaxAmountOtherTaxes);
				AssertEquals("AH_LocalTaxAmountOtherTaxes", expectedLocalOtherTaxAmount, invoice.AH_LocalTaxAmountOtherTaxes);
			});

			CombineAssertions("Total invoice amounts after other tax calculation", () =>
			{
				AssertEquals("AH_OSTotalAmount", expectedOSTotalAmount, invoice.AH_OSTotalAmount);
				AssertEquals("AH_LocalTotalAmount", expectedOSLocalTotal, invoice.AH_LocalTotalAmount);
			});

			var taxRecordPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
			AssertEquals("Number of AccTaxRecordTransactionLinePivot created are correct", lines.Length, taxRecordPivots.Length);
			var actualPivotLinePKs = taxRecordPivots.Select(x => x.ATP_AL_TransactionLine).ToArray();
			var expectedLinePKs = lines.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedLinePKs, actualPivotLinePKs);
		}

		void AssertNoTaxRecordsCreated(InvoicingBase invoice, InvoicingLineBase line)
		{
			var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, invoice.PK));
			AssertEquals("No tax transaction should be created", 0, taxRecords.Length);

			var taxRecordPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, line.PK));
			AssertEquals("No AccTaxRecordTransactionLinePivot should be created", 0, taxRecordPivots.Length);
		}

		#endregion

		#region DeleteTaxRecordsNotInDB

		public void TestDeleteTaxRecordsNotInDB_DeleteAllAddedTaxRecoveryLines()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();

			var taxParentMock = CreateITaxRecordParentMock(Factory);
			taxParentMock.SetupGet(f => f.PK).Returns(taxRecord.ATT_AH);
			var taxRecordCreator = new TaxRecordCreator();
			var recoveryLineCreatorMock = new Mock<ITaxRecoveryLineCreator>();
			taxRecordCreator.SubstituteTaxRecoveryLineCreator_ForTestOnly(recoveryLineCreatorMock.Object);
			((ITaxRecordCreator)taxRecordCreator).DeleteTaxRecordsNotInDB(taxParentMock.Object);
			Assert("taxRecordNotSaved.IsDeleted", taxRecord.IsDeleted);
			recoveryLineCreatorMock.Verify(x => x.DeleteLines(taxParentMock.Object));
		}

		public void TestDeleteTaxRecordsNotInDB()
		{
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;
			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsReceivable, isJobRelated: false);

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: org);
			TestObjectCreator.CreateInvoiceLine(invoice1, chargeCode.PK, 100);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: org);
			TestObjectCreator.CreateInvoiceLine(invoice2, chargeCode.PK, 200);

			var taxRecordParent1 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice1);
			var taxRecordParent2 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice2);

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			taxRecordCreator.CreateTaxRecords(taxRecordParent1);
			taxRecordCreator.CreateTaxRecords(taxRecordParent2);

			AssertTaxRecords("Precondition: invoice1", invoice1, true);
			AssertTaxRecords("Precondition: invoice2", invoice2, true);

			taxRecordCreator.DeleteTaxRecordsNotInDB(taxRecordParent1);
			AssertTaxRecords("invoice1 deleted: invoice1", invoice1, false);
			AssertTaxRecords("invoice1 deleted: invoice2", invoice2, true);

			taxRecordCreator.DeleteTaxRecordsNotInDB(taxRecordParent2);
			AssertTaxRecords("invoice2 deleted: invoice2", invoice2, false);

			AssertNoExceptionThrown("SQL server allows to delete records in db", () => Factory.Save());

			void AssertTaxRecords(string message, InvoicingBase invoice, bool taxRecordsCreated)
			{
				var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, new[] { invoice.PK }));
				AssertEquals(message + " taxRecords.Length", taxRecordsCreated ? 1 : 0, taxRecords.Length);
				if (taxRecordsCreated)
				{
					var taxRecord = taxRecords[0];
					var taxRecordPivots = taxRecord.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
					AssertEquals(message + " taxRecordPivots.Length", 1, taxRecordPivots.Length);
					var taxRecordPivot = taxRecordPivots[0];
					AssertEquals(message + " ATP_AL_TransactionLine", invoice.Lines[0].PK, taxRecordPivot.ATP_AL_TransactionLine);
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestDeleteTaxRecordsNotInDB_ThrowsExceptionIfRecordsInDB()
		{
			var taxRecordSaved = Factory.NewWithValidTestData<AccTaxTransaction>();
			Factory.Save();
			var taxRecordNotSaved = Factory.NewWithValidTestData<AccTaxTransaction>();

			var taxParent = CreateITaxRecordParentMock(Factory);
			taxParent.Setup(x => x.IsPosted).Returns(true);
			taxParent.SetupGet(f => f.PK).Returns(taxRecordSaved.ATT_AH);
			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			AssertExceptionThrown<InvalidOperationException>("", "Tax records for posted transaction cannot be deleted.", () => taxRecordCreator.DeleteTaxRecordsNotInDB(taxParent.Object));
			Assert("taxRecordSaved.IsDeleted", !taxRecordSaved.IsDeleted);

			taxParent.Setup(x => x.IsPosted).Returns(false);
			taxParent.SetupGet(f => f.PK).Returns(taxRecordNotSaved.ATT_AH);
			taxRecordCreator.DeleteTaxRecordsNotInDB(taxParent.Object);
			Assert("taxRecordNotSaved.IsDeleted", taxRecordNotSaved.IsDeleted);
		}

		[SuspendCriticalValidation]
		public void TestDeleteTaxRecordNotInDB()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice) as ITaxRecordParent;

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			taxRecord1.ATT_AffectsSourceTransactionTotal = true;
			taxRecord1.ATT_OSTaxAmount = 100m;
			taxRecord1.ATT_LocalTaxAmount = 200m;
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			taxRecord2.ATT_AffectsSourceTransactionTotal = true;
			taxRecord2.ATT_OSTaxAmount = 500m;
			taxRecord2.ATT_LocalTaxAmount = 1000m;

			var pivot1 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord1.PK;
			var pivot3 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord2.PK;

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			taxRecordCreator.DeleteTaxRecordNotInDB(taxRecordParent, taxRecord1);
			Assert(taxRecord1.IsDeleted);
			Assert(!taxRecord2.IsDeleted);
			Assert(pivot1.IsDeleted);
			Assert(pivot2.IsDeleted);
			Assert(!pivot3.IsDeleted);
			AssertEquals(500m, taxRecordParent.OSTaxAmount);
			AssertEquals(1000m, taxRecordParent.LocalTaxAmount);

			Factory.Save();
			taxRecordCreator.DeleteTaxRecordNotInDB(taxRecordParent, taxRecord2);
			Assert(!taxRecord2.IsDeleted);
			Assert(!pivot3.IsDeleted);
		}

		#endregion

		public void TestCreateCopiesOfTaxRecords()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice1.PK;
			taxRecord1.ATT_OSTaxBaseAmount = 0; // To allow setting ATT_RX_NKOSTaxCurrency.
			taxRecord1.ATT_OSTaxAmount = 0;     // To allow setting ATT_RX_NKOSTaxCurrency and ATT_ETC.
			taxRecord1.ATT_LocalTaxAmount = 0;  // To allow setting ATT_ETC.
			var existingPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord1.PK));
			var pivot1 = existingPivots.Length == 0 ? Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>() : existingPivots[0];
			pivot1.ATP_ATT = taxRecord1.PK;
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord1.PK;

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice2.PK;
			taxRecord2.ATT_OSTaxBaseAmount = 0; // To allow setting ATT_RX_NKOSTaxCurrency.
			taxRecord2.ATT_OSTaxAmount = 0;     // To allow setting ATT_RX_NKOSTaxCurrency and ATT_ETC.
			taxRecord2.ATT_LocalTaxAmount = 0;  // To allow setting ATT_ETC.
			existingPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord2.PK));
			var pivot3 = existingPivots.Length == 0 ? Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>() : existingPivots[0];
			pivot3.ATP_ATT = taxRecord2.PK;

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();

			var includedFields = new[]
			{
				AccTaxTransactionSchema.ATT_A9_TaxMessage.Name,
				AccTaxTransactionSchema.ATT_AffectsSourceTransactionTotal.Name,
				AccTaxTransactionSchema.ATT_AG_LedgerControlAccount.Name,
				AccTaxTransactionSchema.ATT_AG_TaxControlAccount.Name,
				AccTaxTransactionSchema.ATT_AG_TaxExpenseAccount.Name,
				AccTaxTransactionSchema.ATT_AG_TaxPendingControlAccount.Name,
				AccTaxTransactionSchema.ATT_AT_TaxID.Name,
				AccTaxTransactionSchema.ATT_Basis.Name,
				AccTaxTransactionSchema.ATT_ETC.Name,
				AccTaxTransactionSchema.ATT_GB.Name,
				AccTaxTransactionSchema.ATT_GC.Name,
				AccTaxTransactionSchema.ATT_GE_Department.Name,
				AccTaxTransactionSchema.ATT_IsCancelled.Name,
				AccTaxTransactionSchema.ATT_Ledger.Name,
				AccTaxTransactionSchema.ATT_RX_NKOSTaxCurrency.Name,
				AccTaxTransactionSchema.ATT_OSTaxAmount.Name,
				AccTaxTransactionSchema.ATT_OSTaxBaseAmount.Name,
				AccTaxTransactionSchema.ATT_LocalTaxAmount.Name,
				AccTaxTransactionSchema.ATT_LocalTaxBaseAmount.Name,
				AccTaxTransactionSchema.ATT_RateDenominator.Name,
				AccTaxTransactionSchema.ATT_RateNumerator.Name,
				AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode.Name,
				AccTaxTransactionSchema.ATT_TaxAuthorityServiceCodeDescription.Name,
				AccTaxTransactionSchema.ATT_TaxDate.Name,
				AccTaxTransactionSchema.ATT_TaxSuperType.Name,
				AccTaxTransactionSchema.ATT_TaxSystemCode.Name
			};

			var excludedFields = new[]
			{
				AccTaxTransactionSchema.ATT_AH.Name,
				AccTaxTransactionSchema.ATT_AH_MatchTransaction.Name,
				AccTaxTransactionSchema.ATT_RealisationDate.Name,
				AccTaxTransactionSchema.ATT_PostDate.Name,
				AccTaxTransactionSchema.ATT_SystemCreateTimeUtc.Name,
				AccTaxTransactionSchema.ATT_SystemCreateUser.Name,
				AccTaxTransactionSchema.ATT_SystemLastEditTimeUtc.Name,
				AccTaxTransactionSchema.ATT_SystemLastEditUser.Name,
			};

			var expectedFieldValuesByNamesArray = new Dictionary<string, IZType>[2];
			var expectedFieldValuesByNames = new Dictionary<string, IZType>();
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1));
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line2));
			var taxRate = TestObjectCreator.CreateTaxRate("TR1", "TR1 desc", 5);
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA1");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS1");
			var taxMessage = TestObjectCreator.TaxMsg1;

			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_A9_TaxMessage.Name, taxMessage.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AffectsSourceTransactionTotal.Name, ZBool.True);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_LedgerControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxExpenseAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxPendingControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AH.Name, invoice.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AH_MatchTransaction.Name, invoice.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AT_TaxID.Name, taxRate.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_Basis.Name, new ZString(TaxBasisList.PostingOnMatching.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_ETC.Name, taxConfig.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GB.Name, GlbBranch.CurrentBranch.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GC.Name, GlbCompany.CurrentCompany.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GE_Department.Name, GlbDepartment.CurrentDepartment.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_IsCancelled.Name, ZBool.False);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_Ledger.Name, new ZString(TaxConfigurationLedgers.AccountsPayable.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_LocalTaxAmount.Name, new ZDecimal(10m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_LocalTaxBaseAmount.Name, new ZDecimal(100m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RX_NKOSTaxCurrency.Name, new ZString(TestObjectCreator.USD.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_OSTaxAmount.Name, new ZDecimal(20m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_OSTaxBaseAmount.Name, new ZDecimal(200m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_PostDate.Name, new ZDate("2019-03-03"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RateDenominator.Name, new ZInt(7));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RateNumerator.Name, new ZInt(22));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RealisationDate.Name, new ZDate("2019-03-05"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode.Name, new ZString(taxAuthority.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCodeDescription.Name, new ZString("TA1 Desc"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxDate.Name, new ZDate("2019-03-07"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxSuperType.Name, new ZString(TaxSuperTypeList.StandardPaymentRetention.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxSystemCode.Name, new ZString(taxSystem.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemCreateTimeUtc.Name, ZDateTime.Now.AddSeconds(-10));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemCreateUser.Name, new ZString("CU1"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemLastEditTimeUtc.Name, ZDateTime.Now);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemLastEditUser.Name, new ZString("EU1"));
			expectedFieldValuesByNamesArray[0] = expectedFieldValuesByNames;

			expectedFieldValuesByNames = new Dictionary<string, IZType>();
			invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line3));
			taxRate = TestObjectCreator.CreateTaxRate("TR2", "TR2 desc", 50);
			taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA2");
			taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS2");
			taxMessage = TestObjectCreator.TaxMsg2;

			expectedFieldValuesByNames = new Dictionary<string, IZType>();
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_A9_TaxMessage.Name, taxMessage.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AffectsSourceTransactionTotal.Name, ZBool.False);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_LedgerControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxExpenseAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AG_TaxPendingControlAccount.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AH.Name, invoice.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AH_MatchTransaction.Name, invoice.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_AT_TaxID.Name, taxRate.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_Basis.Name, new ZString(TaxBasisList.Matching.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_ETC.Name, taxConfig.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GB.Name, TestObjectCreator.NonCurrentBranch.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GC.Name, TestObjectCreator.NonCurrentCompany.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_GE_Department.Name, TestObjectCreator.MiscDepartment.PK);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_IsCancelled.Name, ZBool.False);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_Ledger.Name, new ZString(TaxConfigurationLedgers.AccountsReceivable.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_LocalTaxAmount.Name, new ZDecimal(105m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_LocalTaxBaseAmount.Name, new ZDecimal(1005m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RX_NKOSTaxCurrency.Name, new ZString(TestObjectCreator.AUD.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_OSTaxAmount.Name, new ZDecimal(205m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_OSTaxBaseAmount.Name, new ZDecimal(2005m));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_PostDate.Name, new ZDate("2019-08-03"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RateDenominator.Name, new ZInt(7));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RateNumerator.Name, new ZInt(22));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_RealisationDate.Name, new ZDate("2019-08-05"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode.Name, new ZString(taxAuthority.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCodeDescription.Name, new ZString("TA2 Desc"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxDate.Name, new ZDate("2019-08-07"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxSuperType.Name, new ZString(TaxSuperTypeList.Perceptions.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_TaxSystemCode.Name, new ZString(taxSystem.Code));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemCreateTimeUtc.Name, ZDateTime.Now.AddSeconds(-10));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemCreateUser.Name, new ZString("CU2"));
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemLastEditTimeUtc.Name, ZDateTime.Now);
			expectedFieldValuesByNames.Add(AccTaxTransactionSchema.ATT_SystemLastEditUser.Name, new ZString("EU2"));
			expectedFieldValuesByNamesArray[1] = expectedFieldValuesByNames;

			IReadOnlyCollection<(AccTaxTransaction TaxRecord, List<AccTaxRecordTransactionLinePivot> Pivots)> taxRecordCopies = null;
			this.AssertCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(new[] { taxRecord1, taxRecord2 }, includedFields, excludedFields, expectedFieldValuesByNamesArray, (source) =>
			{
				taxRecordCopies = taxRecordCreator.CreateCopiesOfTaxRecords(null, source);
				return taxRecordCopies.Select(item => item.TaxRecord).ToArray();
			});

			var pivots = taxRecordCopies.ElementAt(0).Pivots;
			AssertEquals(2, pivots.Count);
			AssertCollectionNotContains(pivot1, pivots);
			AssertCollectionNotContains(pivot2, pivots);

			pivots = taxRecordCopies.ElementAt(1).Pivots;
			AssertEquals(1, pivots.Count);
			AssertCollectionNotContains(pivot3, pivots);
		}

		public void TestCreateCopiesOfTaxRecords_WithRelinkingLines()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var pivot1 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.ATP_LocalTaxAmount = 1;
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord1.PK;
			pivot2.ATP_LocalTaxAmount = 2;
			taxRecord1.FillWithValidTestData();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var pivot3 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord2.PK;
			pivot3.ATP_LocalTaxAmount = 3;
			taxRecord2.FillWithValidTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100m);
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1));
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line2));
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line4));

			var newLinePKs = Enumerable.Repeat(0, 4).Select(x => ZGuid.NewZGuid()).ToArray();
			var lineDictionary = new Dictionary<ZGuid, ZGuid>();
			lineDictionary[line1.PK] = newLinePKs[0];
			lineDictionary[line2.PK] = newLinePKs[1];
			lineDictionary[line3.PK] = newLinePKs[2];
			lineDictionary[line4.PK] = newLinePKs[3];

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			var taxRecordCopies = taxRecordCreator.CreateCopiesOfTaxRecords(lineDictionary, taxRecord1, taxRecord2);

			var pivots = taxRecordCopies.ElementAt(0).Pivots;
			AssertEquals(2, pivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { newLinePKs[0] }, pivots.Where(x => x.ATP_LocalTaxAmount == 1).Select(x => x.ATP_AL_TransactionLine));
			AssertContainsExactElementsInAnyOrder(new[] { newLinePKs[1] }, pivots.Where(x => x.ATP_LocalTaxAmount == 2).Select(x => x.ATP_AL_TransactionLine));

			pivots = taxRecordCopies.ElementAt(1).Pivots;
			AssertEquals(1, pivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { newLinePKs[3] }, pivots.Where(x => x.ATP_LocalTaxAmount == 3).Select(x => x.ATP_AL_TransactionLine));
		}

		public void TestCreateCopiesOfTaxRecordCopiesLocalAmountsCorrectly()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			var copyOfTaxRecords = taxRecordCreator.CreateCopiesOfTaxRecords(null, taxRecord);
			var copyOfTaxRecord = copyOfTaxRecords.ElementAt(0).TaxRecord;

			CombineAssertions(() =>
			{
				AssertEquals(taxRecord.ATT_OSTaxBaseAmount, copyOfTaxRecord.ATT_OSTaxBaseAmount);
				AssertEquals(taxRecord.ATT_OSTaxAmount, copyOfTaxRecord.ATT_OSTaxAmount);
				AssertEquals(taxRecord.ATT_LocalTaxBaseAmount, copyOfTaxRecord.ATT_LocalTaxBaseAmount);
				AssertEquals(taxRecord.ATT_LocalTaxAmount, copyOfTaxRecord.ATT_LocalTaxAmount);
			});
		}

		public void TestCreateCopiesOfTaxRecordCopiesWhenTaxRecordHasNoPivots()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
			AssertEquals("Pre-condition : TaxRecord has no pivots", 0, pivots.Length);

			var taxRecordCreator = (ITaxRecordCreator)new TaxRecordCreator();
			var copyOfTaxRecords = taxRecordCreator.CreateCopiesOfTaxRecords(null, taxRecord);

			AssertEquals(0,copyOfTaxRecords.ElementAt(0).Pivots.Count);
		}

		#region UpdateParentTotals

		public void TestUpdateParentTotals()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			taxRecord1.ATT_OSTaxAmount = 84;
			taxRecord1.ATT_LocalTaxAmount = 126;
			taxRecord1.ATT_AffectsSourceTransactionTotal = true;
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			taxRecord2.ATT_OSTaxAmount = 100;
			taxRecord2.ATT_LocalTaxAmount = 100;
			taxRecord2.ATT_AffectsSourceTransactionTotal = false;
			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = invoice.PK;
			taxRecord3.ATT_OSTaxAmount = 20;
			taxRecord3.ATT_LocalTaxAmount = 30;
			taxRecord3.ATT_AffectsSourceTransactionTotal = true;

			var taxRecords = new[] { taxRecord1, taxRecord2, taxRecord3 };

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupProperty(f => f.OSTaxAmount);
			taxRecordParentMock.SetupProperty(f => f.LocalTaxAmount);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(TestObjectCreator.TestOrganisation);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()))
				.Callback((ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecordList) => taxRecordList.AddRange(taxRecords));

			var taxRecoveryLineCreatorMock = new Mock<ITaxRecoveryLineCreator>();
			AccTaxTransaction[] taxRecordsPassedIntoCreateLines = null;
			taxRecoveryLineCreatorMock.Setup(x => x.CreateLines(It.IsAny<ITaxRecordPivotProcessor>(), taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>()))
				.Callback((ITaxRecordPivotProcessor processor, ITaxRecordParent parent, AccTaxTransaction[] taxes) => taxRecordsPassedIntoCreateLines = taxes);

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteTaxRecoveryLineCreator_ForTestOnly(taxRecoveryLineCreatorMock.Object);

			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);

			AssertEquals("OSTaxAmount must be the sum of taxRecord1.ATT_OSTaxAmount (84) + taxRecord2.ATT_OSTaxAmount (0) +  taxRecord3.ATT_OSTaxAmount (20)", 104m, taxRecordParentMock.Object.OSTaxAmount);
			AssertEquals("LocalTaxAmount must be the sum of taxRecord1.ATT_LocalTaxAmount (126) + taxRecord2.ATT_LocalTaxAmount (0) + taxRecord3.ATT_LocalTaxAmount (30)", 156m, taxRecordParentMock.Object.LocalTaxAmount);
			AssertArrayEqualsByElements(nameof(taxRecordsPassedIntoCreateLines), taxRecords, taxRecordsPassedIntoCreateLines);

			taxRecordCalcualtorMock.Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()))
				.Callback((ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecordList) => taxRecordList.Add(taxRecord2));
			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);
			AssertEquals("LocalTaxAmount", 0m, taxRecordParentMock.Object.LocalTaxAmount);
			AssertEquals("OSTaxAmount", 0m, taxRecordParentMock.Object.OSTaxAmount);
		}

		public void TestUpdateParentTotalsForTaxRecovery()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			taxRecord1.ATT_OSTaxAmount = 84;
			taxRecord1.ATT_LocalTaxAmount = 126;
			taxRecord1.ATT_AffectsSourceTransactionTotal = true;
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			taxRecord2.ATT_OSTaxAmount = 100;
			taxRecord2.ATT_LocalTaxAmount = 100;
			taxRecord2.ATT_AffectsSourceTransactionTotal = true;

			var taxRecords = new[] { taxRecord1, taxRecord2 };

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupProperty(f => f.OSTaxAmount);
			taxRecordParentMock.SetupProperty(f => f.LocalTaxAmount);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(TestObjectCreator.TestOrganisation);

			var taxRecordCalcualtorMock = new Mock<ITaxRecordCalculator>();
			taxRecordCalcualtorMock.Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()))
				.Callback((ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecordList) => taxRecordList.AddRange(taxRecords));

			var taxRecoveryLineCreatorMock = new Mock<ITaxRecoveryLineCreator>();
			AccTaxTransaction[] taxRecordsPassedIntoCreateLines = null;
			taxRecoveryLineCreatorMock.Setup(x => x.CreateLines(It.IsAny<ITaxRecordPivotProcessor>(), taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>()))
				.Callback((ITaxRecordPivotProcessor processor, ITaxRecordParent parent, AccTaxTransaction[] taxes) => taxRecordsPassedIntoCreateLines = taxes);

			var taxRecordCreator = new TaxRecordCreator();
			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalcualtorMock.Object);
			taxRecordCreator.SubstituteTaxRecoveryLineCreator_ForTestOnly(taxRecoveryLineCreatorMock.Object);

			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);

			AssertEquals("OSTaxAmount must be the sum of taxRecord1.ATT_OSTaxAmount (84) + taxRecord2.ATT_OSTaxAmount (100)", 184m, taxRecordParentMock.Object.OSTaxAmount);
			AssertEquals("LocalTaxAmount must be the sum of taxRecord1.ATT_LocalTaxAmount (126) + taxRecord2.ATT_LocalTaxAmount (100)", 226m, taxRecordParentMock.Object.LocalTaxAmount);
			AssertArrayEqualsByElements(nameof(taxRecordsPassedIntoCreateLines), taxRecords, taxRecordsPassedIntoCreateLines);
		}

		#endregion

		#region Setup Helpers

		AccTaxOverrideGroup CreateTaxOverrideAndPivot(GlbCompany company, AccTaxConfiguration taxConfig, AccChargeCode chargeCode)
		{
			var taxFrameTaxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup, chargeCode);

			return taxFrameTaxOverrideGroup;
		}

		static Mock<ITaxRecordParent> CreateITaxRecordParentMock(BusinessObjectFactory factory)
		{
			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupGet(f => f.Factory).Returns(factory);
			taxParent.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			taxParent.SetupGet(f => f.Company).Returns(GlbCompany.CurrentCompany);
			taxParent.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);
			taxParent.SetupGet(f => f.Department).Returns(GlbDepartment.CurrentDepartment);
			taxParent.SetupGet(f => f.IsPosted).Returns(false);

			return taxParent;
		}

		static Mock<ITaxableTransactionLine> CreateITaxableTransactionLineMock(BusinessObjectFactory factory, ZGuid pk, ZString currency, ZDate taxDate, GlbBranch branch = null, AccChargeCode chargeCode = null, AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters = null)
		{
			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(pk);
			lineTaxableTransactionMock.SetupGet(f => f.Currency).Returns(currency);
			lineTaxableTransactionMock.SetupGet(f => f.TaxDate).Returns(taxDate);
			if (branch != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.Branch).Returns(branch);
			}
			if (chargeCode != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.ChargeCode).Returns(chargeCode);
			}

			if (parameters != null)
			{
				lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			}

			return lineTaxableTransactionMock;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		#endregion

		[ExpectNoExceptions]
		public void TestProcessingSequence()
		{
			var taxRecordCalculatorMock = new Mock<ITaxRecordCalculator>(MockBehavior.Strict);
			var thresholdProcessorMock = new Mock<IThresholdProcessor>(MockBehavior.Strict);
			var taxRecoveryLineCreatorMock = new Mock<ITaxRecoveryLineCreator>(MockBehavior.Strict);
			var taxRecordCollectionValidatorMock = new Mock<ITaxRecordCollectionValidator>(MockBehavior.Strict);
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			var taxRecordCreator = new TaxRecordCreator();
			var sequence = new MockSequence();

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();

			var taxRecords = new[] { taxRecord1, taxRecord2 };

			var thresholdMethodCodes = new[] { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			taxRecordCalculatorMock.InSequence(sequence).Setup(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()))
				.Callback((ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecordList) => taxRecordList.AddRange(taxRecords));
			taxRecoveryLineCreatorMock.InSequence(sequence).Setup(x => x.CreateLines(It.IsAny<ITaxRecordPivotProcessor>(), taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>()));
			thresholdProcessorMock.InSequence(sequence).Setup(x => x.Process(taxRecordCreator.TaxRecordPivotProcessor_ExposedForTestOnly, taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes.ToHashSet()));
			taxRecordCollectionValidatorMock.InSequence(sequence).Setup(x => x.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(It.IsAny<IReadOnlyCollection<AccTaxTransaction>>())).Returns(new[] { taxRecord1, taxRecord2 });

			taxRecordCreator.SubstituteTaxRecordCalculator_ForTestOnly(taxRecordCalculatorMock.Object);
			taxRecordCreator.SubstituteTaxRecoveryLineCreator_ForTestOnly(taxRecoveryLineCreatorMock.Object);
			taxRecordCreator.SubstituteThresholdAmountProcessor_ForTestOnly(thresholdProcessorMock.Object);
			taxRecordCreator.SubstituteTaxRecordCollectionValidator_ForTestOnly(taxRecordCollectionValidatorMock.Object);

			((ITaxRecordCreator)taxRecordCreator).CreateTaxRecords(taxRecordParentMock.Object);

			taxRecordCalculatorMock.Verify(x => x.CalculateTaxRecords(taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>()), Times.Once);
			taxRecoveryLineCreatorMock.Verify(x => x.CreateLines(It.IsAny<ITaxRecordPivotProcessor>(), taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>()), Times.Once);
			thresholdProcessorMock.Verify(x => x.Process(taxRecordCreator.TaxRecordPivotProcessor_ExposedForTestOnly, taxRecordParentMock.Object, It.IsAny<List<AccTaxTransaction>>(), thresholdMethodCodes.ToHashSet()), Times.Once);
			taxRecordCollectionValidatorMock.Verify(x => x.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(It.IsAny<IReadOnlyCollection<AccTaxTransaction>>()), Times.Once);
		}
	}
}
