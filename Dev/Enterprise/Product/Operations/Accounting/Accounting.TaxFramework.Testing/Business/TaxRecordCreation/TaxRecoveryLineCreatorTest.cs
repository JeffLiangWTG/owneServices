using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecoveryLineCreatorTest : TestCaseWithFactory
	{
		public void TestUsedAsDependency()
		{
			AssertType<TaxRecoveryLineCreator>(new TaxRecordCreator().TaxRecoveryLineCreator_ExposedForTestOnly);
		}

		#region AnyFirstFoundLineSupplyTypeIsSetOnRecoveryLine

		[ExpectNoExceptions]
		public void TestAnyFirstFoundLineSupplyTypeIsSetOnRecoveryLine_MultipleLinesWithDifferentSupplyTypes()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Australia;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);

			var (pivot11, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 30m, 30m);
			var (pivot12, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 30m, 30m);
			var (pivot13, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 40m, 40m);
			var (pivot21, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);

			pivot11.TransactionLine.AL_SupplyType = "";
			pivot12.TransactionLine.AL_SupplyType = "AAA";
			pivot13.TransactionLine.AL_SupplyType = "BBB";
			pivot21.TransactionLine.AL_SupplyType = "CCC";

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2);
			CombineAssertions("PostConditions", () =>
			{
				pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object));
				pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object));
			});
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.Is<ZString>(y => y == "AAA" || y == "BBB" || y == "CCC")));
		}

		[ExpectNoExceptions]
		public void TestAnyFirstFoundLineSupplyTypeIsSetOnRecoveryLine_MultipleLinesWithTheSameSupplyType()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Australia;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);

			var (pivot11, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 50m, 50m);
			var (pivot12, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 50m, 50m);
			var (pivot21, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);

			pivot11.TransactionLine.AL_SupplyType = "BBB";
			pivot12.TransactionLine.AL_SupplyType = "BBB";
			pivot21.TransactionLine.AL_SupplyType = "BBB";

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2);
			CombineAssertions("PostConditions", () =>
			{
				pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object));
				pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object));
			});
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), "BBB"));
		}

		[ExpectNoExceptions]
		public void TestAnyFirstFoundLineSupplyTypeIsSetOnRecoveryLine_SupplyTypeIsSetOnlyFromRelatedLines()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Australia;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 200, 200, -20, -20, -0.1);

			var (pivot1, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			var (pivot2, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 200m, 200m);

			pivot1.TransactionLine.AL_SupplyType = "CCC";
			pivot2.TransactionLine.AL_SupplyType = "";

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2);
			CombineAssertions("PostConditions", () =>
			{
				pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object));
				pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object));
			});
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), "CCC"));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), ""));
		}

		#endregion

		[ExpectNoExceptions]
		public void TestDeleteLines()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();
			lineCreator.DeleteLines(taxParentMock.Object);
			taxParentMock.Verify(x => x.DeleteAllAddedTaxRecoveryLines());
		}

		public void TestChargeCodeSetValidation()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Aruba;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var (pivot11, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC1, lines, 50m, 50m);
			var (pivot12, _) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 50m, 50m);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var (pivot21, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC1, lines, 30m, 30m);
			var (pivot22, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 30m, 30m);
			var (pivot23, _) = CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 40m, 40m);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1);
			var (pivot31, _) = CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC3, lines, 50m, 50m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			var expectedErrorMessage = "Each tax for recovery line must have the same set of charge codes on linked lines.";
			AssertExceptionThrown<TaxFrameworkInvalidDataException>("", expectedErrorMessage, () => lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);

			pivot31.TransactionLine.AL_AC = TestObjectCreator.CC2.PK;
			AssertExceptionThrown<TaxFrameworkInvalidDataException>("", expectedErrorMessage, () => lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);

			var (pivot32, _) = CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC1, lines, 50m, 50m);
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));

			taxParentMock.Invocations.Clear();
			pivotCreatorMock.Invocations.Clear();
			pivot21.TransactionLine.AL_AC = TestObjectCreator.CC2.PK;
			AssertExceptionThrown<TaxFrameworkInvalidDataException>("", expectedErrorMessage, () => lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestGroupByLocalTaxBase_NoRecoveryLinesCreated_WhenLocalTaxBaseAmountIsZero()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecordWithZeroLocalBase = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 0, 0, 0, 0, -0.1);

			CreatePivotToNewLine(taxRecordWithZeroLocalBase, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecordWithZeroLocalBase);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestGroupByLocalTaxBase_CreatesOneRecoveryLine_WhenSameLocalTaxBaseAmountOnAllTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 101, 100, -11, -14, -0.1, ZDate.Today.AddDays(-1));
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 102, 100, -12, -15, -0.1, ZDate.Today.AddDays(-2));
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 103, 100, -13, -16, -0.1, ZDate.Today.AddDays(1));
			var taxRecordWithZeroLocalBase = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 103, 0, -13, -16, -0.1);

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2, taxRecord3 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecordWithZeroLocalBase);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 81.82m, ZDate.Today.AddDays(-2), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecordWithZeroLocalBase, lineMocks[0].Object), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestGroupByLocalTaxBase_CreatesMultipleRecoveryLines_WhenDifferentLocalTaxBaseAmountOnTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 101, 100, -11, -14, -0.1, ZDate.Today.AddDays(-1));
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 101, 100, -13, -16, -0.1, ZDate.Today.AddDays(1));
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 102, 101, -12, -15, -0.1, ZDate.Today.AddDays(-2));
			var taxRecord4 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 103, 102, -13, -16, -0.1, ZDate.Today.AddDays(1));

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 102m, 101m);
			CreatePivotToNewLine(taxRecord4, ZGuid.Empty, TestObjectCreator.CC2, lines, 103m, 102m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 42.86m, ZDate.Today.AddDays(-1), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 17.62m, ZDate.Today.AddDays(-2), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 18.98m, ZDate.Today.AddDays(1), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(4));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord4, lineMocks[2].Object), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestGroupByBranch_CreatesOneRecoveryLine_WhenSameBranchOnAllTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today.AddDays(1));
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today.AddDays(2));
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today);

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2, taxRecord3 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 42.86m, ZDate.Today, TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[0].Object), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestGroupByBranch_CreatesMultipleRecoveryLines_WhenDifferentBranchOnTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Department = TestDept, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(1) });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Department = TestDept, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -12, LocalTaxAmount = -12, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(1) });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Department = TestDept, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(2) });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Department = TestDept, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today });

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivotToNewLine(taxRecord4, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			taxRecord1.ATT_GB = taxRecord2.ATT_GB = ZGuid.NewZGuid();
			taxRecord3.ATT_GB = ZGuid.NewZGuid();
			taxRecord4.ATT_GB = ZGuid.NewZGuid();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, taxRecord1.ATT_GB, TestDept.PK, TestCurrencyCode, 28.21m, ZDate.Today.AddDays(1), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, taxRecord3.ATT_GB, TestDept.PK, TestCurrencyCode, 11.11m, ZDate.Today.AddDays(2), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, taxRecord4.ATT_GB, TestDept.PK, TestCurrencyCode, 11.11m, ZDate.Today, TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(4));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord4, lineMocks[2].Object), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestGroupByDepartment_CreatesOneRecoveryLine_WhenSameDeptOnAllTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today.AddDays(1));
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today.AddDays(2));
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -10, -10, -0.1, ZDate.Today.AddDays(3));

			CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 42.86m, ZDate.Today.AddDays(1), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[0].Object), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestGroupByDepartment_CreatesMultipleRecoveryLines_WhenDifferentDeptOnTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Branch = TestBranch, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(1) });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Branch = TestBranch, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -12, LocalTaxAmount = -12, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(1) });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Branch = TestBranch, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(2) });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Branch = TestBranch, CurrencyCode = TestCurrencyCode, TaxConfiguration = taxConfigs[0], OsTaxBaseAmount = 100, LocalTaxBaseAmount = 100, OsTaxAmount = -10, LocalTaxAmount = -10, EffectiveRate = -0.1, TaxDate = ZDate.Today.AddDays(3) });

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);
			CreatePivotToNewLine(taxRecord4, ZGuid.Empty, TestObjectCreator.CC2, lines, 100m, 100m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			taxRecord1.ATT_GE_Department = taxRecord2.ATT_GE_Department = ZGuid.NewZGuid();
			taxRecord3.ATT_GE_Department = ZGuid.NewZGuid();
			taxRecord4.ATT_GE_Department = ZGuid.NewZGuid();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, taxRecord1.ATT_GE_Department, TestCurrencyCode, 28.21m, ZDate.Today.AddDays(1), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, taxRecord3.ATT_GE_Department, TestCurrencyCode, 11.11m, ZDate.Today.AddDays(2), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, taxRecord4.ATT_GE_Department, TestCurrencyCode, 11.11m, ZDate.Today.AddDays(3), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(4));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord4, lineMocks[2].Object), Times.Exactly(1));
		}

		public void TestLocalTaxTotalZeroCheckAndChargeCodeRegistryValidationAfterThat()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedCompany = TestObjectCreator.NonCurrentCompany;
			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Aruba;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();
			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(expectedCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 101, 100, -10, -10, 10);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 101, 100, -10, -10, 10);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 101, 100, 20.1, 20, 5);

			CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);
			CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 101m, 100m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, expectedLinePK) { FetchOnlyFromLocalCache = true });
			AssertEquals(0, pivots.Length);

			taxRecord3.ATT_LocalTaxAmount = 20.1;
			var expectedErrorMessage = "A Charge Code must be set in the registry 'Accounting -> Tax Configurations -> Revenue Tax Expense Recovery Charge Code'.";
			AssertExceptionThrown<TaxFrameworkInvalidDataException>("", expectedErrorMessage, () => lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Never);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Never);

			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(expectedCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToGuid());
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(expectedLineChargeCode.PK, ZGuid.Empty, expectedBranch.PK, expectedDepartment.PK, expectedCurrencyCode, It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), expectedOrganisation.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
		}

		[ExpectNoExceptions]
		public void TestCreateLines_CreatesAllTaxRecordsPivots_ForTaxExpenseRecoveryChargeLine()
		{
			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			var expectedLinePK = ZGuid.NewZGuid();
			var expectedCurrencyCode = CurrencyCodes.UnitedStates;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);

			var lines = new List<ITaxableTransactionLine>();
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();
			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 80, 480.61, -1.32, -7.93, -0.0165);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[1], 80, 480.61, -6.08, -36.53, -0.076);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[2], 80, 480.61, -2.4, -14.42, -0.03);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3 }, ZGuid.Empty, TestObjectCreator.CC2, lines, null, 80m, 480.61m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(expectedLineChargeCode.PK, ZGuid.Empty, expectedBranch.PK, expectedDepartment.PK, expectedCurrencyCode, It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), expectedOrganisation.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMock.Object), Times.Once);
		}

		public void TestCreateLines_AmountCalculations_Case1()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.UnitedStates;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 80, 480.61, -1.32, -7.93, -0.0165);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 80, 480.61, -6.08, -36.53, -0.076);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 80, 480.61, -2.4, -14.42, -0.03);
			var taxRecord4 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[1], 80, 480.61, -1.32, -10, -0.0208);
			var taxRecord5 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[2], 80, 480.61, -1.66, -10, -0.0208);

			CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 480.61m);
			CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 480.61m);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 480.61m);
			CreatePivotToNewLine(taxRecord4, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 480.61m);
			CreatePivotToNewLine(taxRecord5, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 480.61m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(expectedLineChargeCode.PK, ZGuid.Empty, expectedBranch.PK, expectedDepartment.PK, expectedCurrencyCode, 67.10, It.IsAny<ZDate>(), expectedOrganisation.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMock.Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 91.17, 547.71, -1.5, -9.04);
			AssertLocalAmounts(taxRecord2, 91.17, 547.71, -6.93, -41.63);
			AssertLocalAmounts(taxRecord3, 91.17, 547.71, -2.74, -16.43);
			AssertLocalAmounts(taxRecord4, 80, 480.61, -1.32, -10);
			AssertLocalAmounts(taxRecord5, 80, 480.61, -1.66, -10);
		}

		public void TestCreateLines_AmountCalculations_Case2()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Brazil;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 35, 201.72, -0.58, -3.33, -0.0165);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 35, 201.72, -2.66, -15.33, -0.076);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 35, 201.72, -1.05, -6.05, -0.03);

			CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 35m, 201.72m);
			CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 35m, 201.72m);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 35m, 201.72m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(expectedLineChargeCode.PK, ZGuid.Empty, expectedBranch.PK, expectedDepartment.PK, expectedCurrencyCode, 28.16, It.IsAny<ZDate>(), expectedOrganisation.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMock.Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 39.89, 229.88, -0.66, -3.79);
			AssertLocalAmounts(taxRecord2, 39.89, 229.88, -3.03, -17.47);
			AssertLocalAmounts(taxRecord3, 39.89, 229.88, -1.2, -6.9);
		}

		public void TestCreateLines_AmountCalculations_Case3()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			var expectedBranch = TestObjectCreator.NonCurrentBranch;
			var expectedDepartment = TestObjectCreator.NonCurrentDepartment;
			var expectedOrganisation = TestObjectCreator.TestOrganisation;
			taxParentMock.SetupGet(x => x.Org).Returns(expectedOrganisation);

			var lineMock = new Mock<ITaxableTransactionLine>();
			var expectedLinePK = ZGuid.NewZGuid();
			lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMock.Object);

			var expectedCurrencyCode = CurrencyCodes.Australia;
			var expectedLineChargeCode = TestObjectCreator.CC1;

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 150, 150, -2.48, -2.48, -0.0165);
			var taxRecord2 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 150, 150, -11.4, -11.4, -0.076);
			var taxRecord3 = GetTaxTransaction(expectedBranch, expectedDepartment, expectedCurrencyCode, taxConfigs[0], 150, 150, -7.5, -7.5, -0.05);

			CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 150m, 150m);
			CreatePivotToNewLine(taxRecord2, ZGuid.Empty, TestObjectCreator.CC2, lines, 150m, 150m);
			CreatePivotToNewLine(taxRecord3, ZGuid.Empty, TestObjectCreator.CC2, lines, 150m, 150m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(expectedLineChargeCode.PK, ZGuid.Empty, expectedBranch.PK, expectedDepartment.PK, expectedCurrencyCode, 24.93, It.IsAny<ZDate>(), expectedOrganisation.PK, It.IsAny<ZString>()), Times.Once);
			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMock.Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 174.93, 174.93, -2.89, -2.89);
			AssertLocalAmounts(taxRecord2, 174.93, 174.93, -13.29, -13.29);
			AssertLocalAmounts(taxRecord3, 174.93, 174.93, -8.75, -8.75);
		}

		public void TestCreateLines_AmountCalculations_Case4_AmountsForUnrelatedTaxRecords()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 80, 240, -7.88, -23.64, -0.0165);
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 80, 240, -6.08, -18.24, -0.076);
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 300, -2.4, -14.42, -0.03);
			var taxRecord4 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 300, -3.46, -13.54, -0.0208);

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 240m);
			CreatePivotToPassedLine(taxRecord2, firstLine);
			CreatePivots(new AccTaxTransaction[] { taxRecord3, taxRecord4 }, ZGuid.Empty, TestObjectCreator.CC2, lines, null, 100m, 300m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 26.22, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(2));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 88.74, 266.22, -1.46, -4.39);
			AssertLocalAmounts(taxRecord2, 88.74, 266.22, -6.74, -20.23);
			AssertLocalAmounts(taxRecord3, 100, 300, -2.4, -14.42);
			AssertLocalAmounts(taxRecord4, 100, 300, -3.46, -13.54);
		}

		public void TestCreateLines_AmountCalculations_Case5_AmountsForTaxRecords_MulitpleRecoveryLines()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(2);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 80, 240, -1.32, -7.93, -0.0165);
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 80, 240, -6.08, -36.53, -0.076);
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 80, 240, -2.4, -14.42, -0.03);
			var taxRecord4 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 300, -4.56, -13.68, -0.0208);
			var taxRecord5 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 300, -8.3, -24.9, -0.063);
			var taxRecord6 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 300, -3.31, -9.93, -0.041);

			var (_, firstLine) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 240m);
			CreatePivots(new AccTaxTransaction[] { taxRecord2, taxRecord3 }, ZGuid.Empty, TestObjectCreator.CC2, lines, firstLine);
			var (_, secondLine) = CreatePivotToNewLine(taxRecord4, ZGuid.Empty, TestObjectCreator.CC2, lines, 80m, 240m);
			CreatePivots(new AccTaxTransaction[] { taxRecord5, taxRecord6 }, ZGuid.Empty, TestObjectCreator.CC2, lines, secondLine);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();

			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(2));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 54.57, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 14.33, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(6));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord4, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord5, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord6, lineMocks[1].Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 98.19, 294.57, -1.62, -4.86);
			AssertLocalAmounts(taxRecord2, 98.19, 294.57, -7.46, -22.39);
			AssertLocalAmounts(taxRecord3, 98.19, 294.57, -2.95, -8.84);
			AssertLocalAmounts(taxRecord4, 104.78, 314.33, -2.18, -6.54);
			AssertLocalAmounts(taxRecord5, 104.78, 314.33, -6.60, -19.80);
			AssertLocalAmounts(taxRecord6, 104.78, 314.33, -4.30, -12.89);
		}

		static void AssertLocalAmounts(AccTaxTransaction taxRecord, ZDecimal osTaxBase, ZDecimal localTaxBase, ZDecimal osTax, ZDecimal localTax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ATT_OSTaxBaseAmount", osTaxBase, taxRecord.ATT_OSTaxBaseAmount);
				AssertEquals("ATT_LocalTaxBaseAmount", localTaxBase, taxRecord.ATT_LocalTaxBaseAmount);
				AssertEquals("ATT_OSTaxAmount", osTax, taxRecord.ATT_OSTaxAmount);
				AssertEquals("ATT_LocalTaxAmount", localTax, taxRecord.ATT_LocalTaxAmount);
			});
		}

		[ExpectNoExceptions]
		public void TestTaxRecoveryLinePerJobGroup()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 70, 140, -3.85, -7.7, -0.055);

			var job1Pk = ZGuid.NewZGuid();
			var job2Pk = ZGuid.NewZGuid();

			var (_, line1) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 10m, 20m);
			var (_, line2) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 5m, 10m);
			var (_, line3) = CreatePivotToNewLine(taxRecord1, job2Pk, TestObjectCreator.CC2, lines, 15m, 30m);
			var (_, line4) = CreatePivotToNewLine(taxRecord1, job2Pk, TestObjectCreator.CC2, lines, 20m, 40m);
			var (_, line5) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 12.5m, 25m);
			var (_, line6) = CreatePivotToNewLine(taxRecord1, ZGuid.Empty, TestObjectCreator.CC2, lines, 7.5m, 15m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job1Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 1.75, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job2Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 4.07, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, ZGuid.Empty, TestBranch.PK, TestDept.PK, TestCurrencyCode, 2.33, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[2].Object), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestTaxRecoveryLinePerJobGroupPerTaxBaseAmountGroup()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 70, 140, -3.85, -7.7, -0.055);
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 200, -4, -8, -0.04);

			var job1Pk = ZGuid.NewZGuid();
			var job2Pk = ZGuid.NewZGuid();
			var job3Pk = ZGuid.NewZGuid();

			var (_, line1) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 30m, 60m);
			var (_, line2) = CreatePivotToNewLine(taxRecord1, job2Pk, TestObjectCreator.CC2, lines, 40m, 80m);
			var (_, line3) = CreatePivotToNewLine(taxRecord2, job3Pk, TestObjectCreator.CC2, lines, 100m, 200m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job1Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 3.49, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job2Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 4.66, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job3Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 8.33, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(3));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[1].Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMocks[2].Object), Times.Exactly(1));
		}

		public void TestTaxBaseAmounts_When_RecoveryLinePerJobPerTaxBaseGroup()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 200, 400, -20, -40, -0.1);
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 200, -5, -10, -0.05);
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 100, 200, -2, -4, -0.02);
			var taxRecord4 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 80, 160, -4, -8, -0.05);
			var taxRecord5 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 80, 160, -8, -16, -0.1);

			var job1Pk = ZGuid.NewZGuid();
			var job2Pk = ZGuid.NewZGuid();
			var job3Pk = ZGuid.NewZGuid();

			var recoveryLineForJob1 = lineMocks[0];
			var recoveryLineForJob2 = lineMocks[1];
			var recoveryLineForJob3 = lineMocks[2];

			var (_, line1) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 50m, 100m);
			var (_, line2) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 50m, 100m);
			CreatePivotToPassedLine(taxRecord2, line1);
			CreatePivotToPassedLine(taxRecord2, line2);

			var (_, line3) = CreatePivotToNewLine(taxRecord1, job2Pk, TestObjectCreator.CC2, lines, 100m, 200m);
			CreatePivotToPassedLine(taxRecord3, line3);

			var (_, line4) = CreatePivotToNewLine(taxRecord4, job3Pk, TestObjectCreator.CC2, lines, 40m, 80m);
			var (_, line5) = CreatePivotToNewLine(taxRecord4, job3Pk, TestObjectCreator.CC2, lines, 40m, 80m);
			CreatePivotToPassedLine(taxRecord5, line4);
			CreatePivotToPassedLine(taxRecord5, line5);

			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), job1Pk, It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(recoveryLineForJob1.Object);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), job2Pk, It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(recoveryLineForJob2.Object);
			taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), job3Pk, It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(recoveryLineForJob3.Object);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job1Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 22.22, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job2Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 22.22, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job3Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 8.42, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(6));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, recoveryLineForJob1.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord2, recoveryLineForJob1.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, recoveryLineForJob2.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord3, recoveryLineForJob2.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord4, recoveryLineForJob3.Object), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord5, recoveryLineForJob3.Object), Times.Exactly(1));

			AssertLocalAmounts(taxRecord1, 222.22, 444.44, -22.22, -44.44);
			AssertLocalAmounts(taxRecord2, 111.11, 222.22, -5.56, -11.11);
			AssertLocalAmounts(taxRecord3, 111.11, 222.22, -2.22, -4.44);
			AssertLocalAmounts(taxRecord4, 84.21, 168.42, -4.21, -8.42);
			AssertLocalAmounts(taxRecord5, 84.21, 168.42, -8.42, -16.84);
		}

		[ExpectNoExceptions]
		public void TestTaxRecoveryLine_When_TotalLineAmoutForAJobIsZero()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 200, -20, -40, -0.2);

			var job1Pk = ZGuid.NewZGuid();
			var job2Pk = ZGuid.NewZGuid();

			var (_, line1) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 50m, 100m);
			var (_, line2) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, -50m, -100m);
			var (_, line3) = CreatePivotToNewLine(taxRecord1, job2Pk, TestObjectCreator.CC2, lines, 100m, 200m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(1));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job2Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 50, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(1));
			pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMocks[0].Object), Times.Exactly(1));
		}

		public void TestTaxRecoveryLine_When_TotalLineAmoutForTaxBaseGroupIsZero()
		{
			// This is not a real case now but this test is necessary if in case the way TaxBase amount calculation changes in the future in such a way that it can be 0. Tax recovery line creation should not end up throwing a 'Divide by Zero' exception.
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(1);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 100, 100, -20, -20, -0.2);

			var job1Pk = ZGuid.NewZGuid();

			var (_, line1) = CreatePivotToNewLine(taxRecord1, job1Pk, TestObjectCreator.CC2, lines, 0m, 0m);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(0));

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(0));

			AssertEquals("TotalLocalAmountOfLinesForTaxBaseGroupIsZero", ErrorReporter.LastKeyReported);
			AssertEquals("Total of line amounts for tax base group is 0. Skipping creation of tax expense recovery line for the group.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTaxRecoveryLinePerJob_ExampleScenarioFromHLD()
		{
			ITaxRecoveryLineCreator lineCreator = new TaxRecoveryLineCreator();

			var lines = new List<ITaxableTransactionLine>();
			var taxParentMock = MockTaxParent(lines);
			var (lineMocks, _) = MockRecoveryLines(3);
			SetupAddTaxRecoveryLineInvocationMock(taxParentMock, lineMocks);

			var taxConfigs = SetupTaxRecoveryTaxConfigs();

			var taxRecord1 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 6021, 6021, -457.60, -457.60, -0.076);
			var taxRecord2 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 6021, 6021, -99.35, -99.35, -0.0165);
			var taxRecord3 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[0], 6021, 6021, -301.05, -301.05, -0.05);
			var taxRecord4 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 6021, 6021, -39.14, -39.14, -0.0065);
			var taxRecord5 = GetTaxTransaction(TestBranch, TestDept, TestCurrencyCode, taxConfigs[1], 6021, 6021, -90.32, -90.32, -0.015);

			var job1Pk = ZGuid.NewZGuid();
			var job2Pk = ZGuid.NewZGuid();
			var job3Pk = ZGuid.NewZGuid();

			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job1Pk, TestObjectCreator.CC2, lines, osAmount: 1001, localAmount: 1001);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job1Pk, TestObjectCreator.CC2, lines, osAmount: 1002, localAmount: 1002);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job2Pk, TestObjectCreator.CC2, lines, osAmount: 1003, localAmount: 1003);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job2Pk, TestObjectCreator.CC2, lines, osAmount: 1004, localAmount: 1004);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job3Pk, TestObjectCreator.CC2, lines, osAmount: 1005, localAmount: 1005);
			CreatePivots(new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5 }, job3Pk, TestObjectCreator.CC2, lines, osAmount: 1006, localAmount: 1006);

			var pivotCreatorMock = new Mock<ITaxRecordPivotProcessor>();
			lineCreator.CreateLines(pivotCreatorMock.Object, taxParentMock.Object, taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5);

			taxParentMock.Verify(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.Exactly(3));
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job1Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 332.86, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job2Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 333.53, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);
			taxParentMock.Verify(x => x.AddTaxRecoveryLine(TestRecoveryLineChargeCode.PK, job3Pk, TestBranch.PK, TestDept.PK, TestCurrencyCode, 334.19, It.IsAny<ZDate>(), TestOrgHeader.PK, It.IsAny<ZString>()), Times.Once);

			pivotCreatorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLine>()), Times.Exactly(15));
			foreach (var lineMock in lineMocks)
			{
				pivotCreatorMock.Verify(x => x.Create(taxRecord1, lineMock.Object), Times.Exactly(1));
				pivotCreatorMock.Verify(x => x.Create(taxRecord2, lineMock.Object), Times.Exactly(1));
				pivotCreatorMock.Verify(x => x.Create(taxRecord3, lineMock.Object), Times.Exactly(1));
				pivotCreatorMock.Verify(x => x.Create(taxRecord4, lineMock.Object), Times.Exactly(1));
				pivotCreatorMock.Verify(x => x.Create(taxRecord5, lineMock.Object), Times.Exactly(1));
			}

			AssertLocalAmounts(taxRecord1, 7021.58, 7021.58, -533.64, -533.64);
			AssertLocalAmounts(taxRecord2, 7021.58, 7021.58, -115.86, -115.86);
			AssertLocalAmounts(taxRecord3, 7021.58, 7021.58, -351.08, -351.08);
			AssertLocalAmounts(taxRecord4, 7021.58, 7021.58, -45.64, -45.64);
			AssertLocalAmounts(taxRecord5, 7021.58, 7021.58, -105.32, -105.32);
		}

		AccTaxConfiguration[] SetupTaxRecoveryTaxConfigs()
		{
			var company = TestObjectCreator.NonCurrentCompany;

			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToGuid());

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS1");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS2");
			var taxSystem3 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS3");

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);
			taxSystemsConfigCollection.Add(taxSystem3);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var org = TestObjectCreator.TestOrganisation;
			var ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			var companyData = org.GetCompanyDataForGlbCompany(company);

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem1, ledger);
			taxConfig1.ETC_RecoveryMethod = TaxRecoveryMethods.RecoverTaxExpense.Code;
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem2, ledger);
			var taxConfig3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem3, ledger);
			taxConfig3.ETC_RecoveryMethod = TaxRecoveryMethods.RecoverTaxExpense.Code;
			Factory.Save();

			var orgTaxConfig1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData);
			orgTaxConfig1.OTC_RecoverTax = true;

			var orgTaxConfig2 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData);
			orgTaxConfig2.OTC_RecoverTax = true;

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig3, companyData);
			Factory.Save();

			return new[] { taxConfig1, taxConfig2, taxConfig3 };
		}

		public Mock<ITaxRecordParent> MockTaxParent(List<ITaxableTransactionLine> lines)
		{
			var taxParentMock = new Mock<ITaxRecordParent>();

			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestOrgHeader);
			taxParentMock.Setup(x => x.GetLines()).Returns(lines);

			return taxParentMock;
		}

		public (Mock<ITaxableTransactionLine>[], ZGuid[]) MockRecoveryLines(int numberOfRecoveryLines)
		{
			var lineMocks = new List<Mock<ITaxableTransactionLine>>();
			var linePKs = new List<ZGuid>();

			for (int i = 0; i < numberOfRecoveryLines; i++)
			{
				var lineMock = new Mock<ITaxableTransactionLine>();
				var expectedLinePK = ZGuid.NewZGuid();
				lineMock.SetupGet(x => x.PK).Returns(expectedLinePK);

				lineMocks.Add(lineMock);
				linePKs.Add(expectedLinePK);
			}

			return (lineMocks.ToArray(), linePKs.ToArray());
		}

		public void SetupAddTaxRecoveryLineInvocationMock(Mock<ITaxRecordParent> taxParentMock, params Mock<ITaxableTransactionLine>[] lineMocks)
		{
			if (lineMocks.Length > 1)
			{
				var objects = lineMocks.Select(x => x.Object).ToArray();
				var lineMockQueue = new Queue<ITaxableTransactionLine>(objects);

				taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(() => lineMockQueue.Dequeue());
			}
			else
			{
				taxParentMock.Setup(x => x.AddTaxRecoveryLine(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDate>(), It.IsAny<ZGuid>(), It.IsAny<ZString>())).Returns(lineMocks[0].Object);
			}
		}

		(AccTaxRecordTransactionLinePivot, ITaxableTransactionLine) CreatePivotToNewLine(AccTaxTransaction taxRecord, ZGuid jobPK, AccChargeCode chargeCode, List<ITaxableTransactionLine> lines, decimal osAmount, decimal localAmount)
		{
			var line = Factory.New<ARInvoiceLine>();
			line.AL_AC = chargeCode.PK;
			line.AL_JH = jobPK;
			line.AL_LineAmount = localAmount;
			line.AL_OSAmount = osAmount;

			var taxableLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			lines.Add(taxableLine);

			return (TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxRecord.PK, taxableLine), taxableLine);
		}

		AccTaxRecordTransactionLinePivot CreatePivotToPassedLine(AccTaxTransaction taxRecord, ITaxableTransactionLine taxableLine)
		{
			return TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxRecord.PK, taxableLine);
		}

		AccTaxRecordTransactionLinePivot[] CreatePivots(AccTaxTransaction[] taxRecords, ZGuid jobPK, AccChargeCode chargeCode, List<ITaxableTransactionLine> lines, ITaxableTransactionLine line = null, decimal osAmount = 0m, decimal localAmount = 0m)
		{
			var pivots = new List<AccTaxRecordTransactionLinePivot>();

			foreach (var taxRecord in taxRecords)
			{
				if (line == null)
				{
					var (pivot, taxableLine) = CreatePivotToNewLine(taxRecord, jobPK, chargeCode, lines, osAmount, localAmount);
					pivots.Add(pivot);
					line = taxableLine;
				}
				else
				{
					pivots.Add(CreatePivotToPassedLine(taxRecord, line));
				}
			}

			return pivots.ToArray();
		}

		AccTaxTransaction GetTaxTransaction(GlbBranch branch, GlbDepartment department, ZString currencyCode, AccTaxConfiguration taxConfig,
											ZDecimal osTaxBaseAmount, ZDecimal localTaxBaseAmount, ZDecimal osTaxAmount, ZDecimal localTaxAmount, ZDecimal effectiveRate, ZDate? taxDate = null)
		{
			return TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				Branch = branch,
				Department = department,
				CurrencyCode = currencyCode,
				TaxConfiguration = taxConfig,
				OsTaxBaseAmount = osTaxBaseAmount,
				LocalTaxBaseAmount = localTaxBaseAmount,
				OsTaxAmount = osTaxAmount,
				LocalTaxAmount = localTaxAmount,
				EffectiveRate = effectiveRate,
				TaxDate = taxDate ?? ZDate.Today
			});
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)));
		TestObjectCreator testObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		GlbCompany TestCompany => TestObjectCreator.NonCurrentCompany;

		GlbBranch TestBranch => TestObjectCreator.NonCurrentBranch;

		GlbDepartment TestDept => TestObjectCreator.NonCurrentDepartment;

		OrgHeader TestOrgHeader => TestObjectCreator.TestOrganisation;

		String TestCurrencyCode => CurrencyCodes.Australia;

		AccChargeCode TestRecoveryLineChargeCode => TestObjectCreator.CC1;
	}
}
