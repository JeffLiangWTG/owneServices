using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordReverserTest : TestCaseWithFactory
	{
		public void TestDependencies()
		{
			var reverser = new TaxRecordReverser();
			AssertType<TaxRecordLoader>(reverser.TaxRecordLoader_ExposedForTestOnly);
			AssertType<TaxRecordCreator>(reverser.TaxRecordCreator_ExposedForTestOnly);
		}

		public void TestReverseRealisedSPRAPTaxRecords()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);
			var taxParentLoaderMock = new Mock<ITaxFrameworkBOLoader>();

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			ObjectFactory.Substitute(taxParentLoaderMock.Object);

			var transactionHeaderPK = ZGuid.NewZGuid();
			var matchTransactionPK = ZGuid.NewZGuid();
			var reverseTransactionPK = ZGuid.NewZGuid();

			reversedTaxParentMock.Setup(x => x.Factory).Returns(Factory);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(Factory, transactionHeaderPK)).Returns(taxParentMock.Object);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(Factory, reverseTransactionPK)).Returns(reversedTaxParentMock.Object);

			var postDate = new ZDate("2019-03-03");

			var taxRecord1 = CreateNewTaxTransaction();
			taxRecord1.ATT_AH = transactionHeaderPK;
			var taxRecord2 = CreateNewTaxTransaction();
			taxRecord2.ATT_AH = transactionHeaderPK;

			var amount = 100m;
			taxRecord1.ATT_OSTaxBaseAmount = amount++;
			taxRecord1.ATT_LocalTaxBaseAmount = amount++;
			taxRecord1.ATT_OSTaxAmount = amount++;
			taxRecord1.ATT_LocalTaxAmount = amount++;
			taxRecord2.ATT_OSTaxBaseAmount = amount++;
			taxRecord2.ATT_LocalTaxBaseAmount = amount++;
			taxRecord2.ATT_OSTaxAmount = amount++;
			taxRecord2.ATT_LocalTaxAmount = amount++;

			taxRecord1.ATT_IsCancelled = false;
			taxRecord2.ATT_IsCancelled = false;

			taxRecordLoaderMock.Setup(t => t.LoadSPRAPTaxRecordsLinkedToMatchTransaction(Factory, matchTransactionPK)).Returns(new[] { taxRecord1, taxRecord2 });
			((ITaxRecordReverser)reverser).ReverseRealisedSPRAPTaxRecords(Factory, matchTransactionPK, reverseTransactionPK, postDate);
			var reversedTaxRecordsQuery = new ZQuery(AccTaxTransactionSchema.PK, SQLComparisonOperator.NotEqual, taxRecord1.PK);
			reversedTaxRecordsQuery.AddToFilter(AccTaxTransactionSchema.PK, SQLComparisonOperator.NotEqual, taxRecord2.PK);
			var reversedTaxRecords = Factory.Load<AccTaxTransaction>(reversedTaxRecordsQuery).OrderByDescending(x => x.ATT_OSTaxBaseAmount).ToArray();
			AssertEquals(2, reversedTaxRecords.Length);
			var taxRecord1Copy = reversedTaxRecords[0];
			var taxRecord2Copy = reversedTaxRecords[1];

			amount = -100m;
			AssertEquals(amount--, taxRecord1Copy.ATT_OSTaxBaseAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_LocalTaxBaseAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_OSTaxAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_LocalTaxAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_OSTaxBaseAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_LocalTaxBaseAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_OSTaxAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_LocalTaxAmount);

			Assert(taxRecord1.ATT_IsCancelled);
			Assert(taxRecord2.ATT_IsCancelled);
			Assert(taxRecord1Copy.ATT_IsCancelled);
			Assert(taxRecord2Copy.ATT_IsCancelled);

			AssertEquals(transactionHeaderPK, taxRecord1Copy.ATT_AH);
			AssertEquals(transactionHeaderPK, taxRecord2Copy.ATT_AH);
			AssertEquals(reverseTransactionPK, taxRecord1Copy.ATT_AH_MatchTransaction);
			AssertEquals(reverseTransactionPK, taxRecord2Copy.ATT_AH_MatchTransaction);

			AssertEquals(postDate, taxRecord1Copy.ATT_RealisationDate);
			AssertEquals(postDate, taxRecord2Copy.ATT_RealisationDate);
			AssertEquals(postDate, taxRecord1Copy.ATT_PostDate);
			AssertEquals(postDate, taxRecord2Copy.ATT_PostDate);

			taxRecord1 = Factory.New<AccTaxTransaction>();
			taxRecord1.ATT_AH = transactionHeaderPK;
			taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecord2.ATT_AH = transactionHeaderPK;

			taxRecord1.ATT_IsCancelled = false;
			taxRecord2.ATT_IsCancelled = false;

			var taxRecordCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			reverser.SubstituteTaxRecordCreator_ForTestOnly(taxRecordCreatorMock.Object);
			ZDecimal originalParentTaxAmount = 100M;
			reversedTaxParentMock.SetupProperty(x => x.LocalTaxAmount, originalParentTaxAmount);
			reversedTaxParentMock.SetupProperty(x => x.OSTaxAmount, originalParentTaxAmount);
			var inputParamsToTaxRecordCreator = new AccTaxTransaction[2];
			var index = 0;
			taxRecordLoaderMock.Setup(t => t.LoadSPRAPTaxRecordsLinkedToMatchTransaction(Factory, matchTransactionPK)).Returns(new[] { taxRecord1, taxRecord2 });
			taxRecordCreatorMock.Setup(t => t.CreateCopiesOfTaxRecords(null, It.Is<AccTaxTransaction[]>(x => x.Length == 1)))
				.Returns(new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>() { (CreateNewTaxTransaction(), new List<AccTaxRecordTransactionLinePivot>()) }).Callback<IReadOnlyDictionary<ZGuid, ZGuid>, AccTaxTransaction[]>((dic, arr) =>
				{
					inputParamsToTaxRecordCreator[index++] = arr[0];
				});

			((ITaxRecordReverser)reverser).ReverseRealisedSPRAPTaxRecords(Factory, matchTransactionPK, reverseTransactionPK, postDate);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord2 }, inputParamsToTaxRecordCreator);
			AssertEquals(originalParentTaxAmount, reversedTaxParentMock.Object.LocalTaxAmount);
			AssertEquals(originalParentTaxAmount, reversedTaxParentMock.Object.OSTaxAmount);
		}

		public void TestReverseNonSPRAPTaxRecords()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);
			var taxRecordCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			var taxParentLoaderMock = new Mock<ITaxFrameworkBOLoader>();

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			reverser.SubstituteTaxRecordCreator_ForTestOnly(taxRecordCreatorMock.Object);
			ObjectFactory.Substitute(taxParentLoaderMock.Object);

			var matchTransactionPK = ZGuid.NewZGuid();
			var postDate = ZDate.Today.AddDays(-10);

			var invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			var reverseInvoice = invoice.ReverseInvoice;

			reversedTaxParentMock.Setup(x => x.Factory).Returns(reverseInvoice.Factory);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(Factory, invoice.PK)).Returns(taxParentMock.Object);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(reverseInvoice.Factory, reverseInvoice.PK)).Returns(reversedTaxParentMock.Object);

			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object)).Returns(System.Array.Empty<AccTaxTransaction>());
			((ITaxRecordReverser)reverser).ReverseNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, postDate);

			taxRecordCreatorMock.Verify(t => t.CreateCopiesOfTaxRecords(It.IsAny<IReadOnlyDictionary<ZGuid, ZGuid>>(), It.IsAny<AccTaxTransaction[]>()), Times.Never);

			var taxRecord1 = CreateNewTaxTransaction();
			taxRecord1.ATT_AH = invoice.PK;
			var taxRecord2 = CreateNewTaxTransaction();
			taxRecord2.ATT_AH = invoice.PK;
			var taxRecord1Copy = CreateNewTaxTransaction();
			taxRecord1Copy.ATT_AH = reverseInvoice.PK;
			var taxRecord2Copy = CreateNewTaxTransaction();
			taxRecord2Copy.ATT_AH = reverseInvoice.PK;

			var amount = 100m;
			taxRecord1Copy.ATT_OSTaxBaseAmount = amount++;
			taxRecord1Copy.ATT_LocalTaxBaseAmount = amount++;
			taxRecord1Copy.ATT_OSTaxAmount = amount++;
			taxRecord1Copy.ATT_LocalTaxAmount = amount++;
			taxRecord2Copy.ATT_OSTaxBaseAmount = amount++;
			taxRecord2Copy.ATT_LocalTaxBaseAmount = amount++;
			taxRecord2Copy.ATT_OSTaxAmount = amount++;
			taxRecord2Copy.ATT_LocalTaxAmount = amount++;

			taxRecord1.ATT_IsCancelled = false;
			taxRecord2.ATT_IsCancelled = false;
			var postDate1 = ZDate.Today.AddDays(-8);
			taxRecord2.ATT_RealisationDate = postDate1;
			taxRecord1Copy.ATT_IsCancelled = false;
			taxRecord2Copy.ATT_IsCancelled = false;

			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object)).Returns(new[] { taxRecord1, taxRecord2 });
			var inputParamsToTaxRecordCreator = new AccTaxTransaction[2];

			var taxableLine = (ITaxableTransactionLine)TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			var lines = new List<ITaxableTransactionLine>();
			lines.Add(taxableLine);
			ZDecimal originalParentTaxAmount = 100M;
			reversedTaxParentMock.SetupProperty(x => x.LocalTaxAmount, originalParentTaxAmount);
			reversedTaxParentMock.SetupProperty(x => x.OSTaxAmount, originalParentTaxAmount);
			reversedTaxParentMock.Setup(x => x.GetLines()).Returns(lines);
			AssertEquals(line.PK, taxableLine.PK);

			var reversedLine = (InvoicingLineBase)reverseInvoice.Lines.First();
			AssertEquals(line.PK, reversedLine.CopiedFromPK);

			var linesDictionary = new Dictionary<ZGuid, ZGuid>();
			linesDictionary.Add(lines[0].CopiedFromPK, lines[0].PK);

			taxRecordCreatorMock.Setup(t => t.CreateCopiesOfTaxRecords(linesDictionary, It.IsAny<AccTaxTransaction[]>())).Returns(new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>() { (taxRecord1Copy, new List<AccTaxRecordTransactionLinePivot>()), (taxRecord2Copy, new List<AccTaxRecordTransactionLinePivot>()) }).Callback<IReadOnlyDictionary<ZGuid, ZGuid>, AccTaxTransaction[]>((dic, arr) => inputParamsToTaxRecordCreator = arr);

			((ITaxRecordReverser)reverser).ReverseNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, postDate);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord2 }, inputParamsToTaxRecordCreator);

			amount = -100m;
			AssertEquals(amount--, taxRecord1Copy.ATT_OSTaxBaseAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_LocalTaxBaseAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_OSTaxAmount);
			AssertEquals(amount--, taxRecord1Copy.ATT_LocalTaxAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_OSTaxBaseAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_LocalTaxBaseAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_OSTaxAmount);
			AssertEquals(amount--, taxRecord2Copy.ATT_LocalTaxAmount);

			Assert(taxRecord1.ATT_IsCancelled);
			Assert(taxRecord2.ATT_IsCancelled);
			Assert(taxRecord1Copy.ATT_IsCancelled);
			Assert(taxRecord2Copy.ATT_IsCancelled);

			AssertEquals("Realisation date is set only when empty", postDate, taxRecord1.ATT_RealisationDate);
			AssertEquals("Realisation date is set only when empty", postDate1, taxRecord2.ATT_RealisationDate);
			AssertEquals(postDate, taxRecord1Copy.ATT_RealisationDate);
			AssertEquals(postDate, taxRecord2Copy.ATT_RealisationDate);
			AssertEquals(postDate, taxRecord1Copy.ATT_PostDate);
			AssertEquals(postDate, taxRecord2Copy.ATT_PostDate);
			AssertEquals(originalParentTaxAmount, reversedTaxParentMock.Object.LocalTaxAmount);
			AssertEquals(originalParentTaxAmount, reversedTaxParentMock.Object.OSTaxAmount);
		}

		public void TestReverseRealisedSPRAPTaxRecords_DoesNotInvokeRoundingMethod_WhenSettingTaxTransactionAmounts()
		{
			AssertReverseAPTaxRecordsDoesNotInvokeRoundingMethod(
				x => x.LoadSPRAPTaxRecordsLinkedToMatchTransaction(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>()),
				(reverser, originalParent, reversingParent) => reverser.ReverseRealisedSPRAPTaxRecords(Factory, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZDate.Today)
				);
		}

		public void TestReverseNonSPRAPTaxRecords_DoesNotInvokeRoundingMethod_WhenSettingTaxTransactionAmounts()
		{
			AssertReverseAPTaxRecordsDoesNotInvokeRoundingMethod(
				x => x.LoadNonSPRAPTaxRecords(It.IsAny<ITaxRecordParent>()),
				(reverser, originalParent, reversingParent) => reverser.ReverseNonSPRAPTaxRecords(originalParent, reversingParent, ZDate.Today)
				);
		}

		void AssertReverseAPTaxRecordsDoesNotInvokeRoundingMethod(
			Expression<Func<ITaxRecordLoader, AccTaxTransaction[]>> getLoadMethodExpression,
			Action<ITaxRecordReverser, ITaxRecordParent, ITaxRecordParent> action
			)
		{
			var reverser = new TaxRecordReverser();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxFrameworkDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			var roundingMethodApplierMock = new Mock<IRoundingMethodApplier>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParentLoaderMock = new Mock<ITaxFrameworkBOLoader>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxRecordCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);

			ObjectFactory.Substitute(taxParentLoaderMock.Object);
			ObjectFactory.Substitute(taxFrameworkDependencyFactory.Object);
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			reverser.SubstituteTaxRecordCreator_ForTestOnly(taxRecordCreatorMock.Object);
			taxFrameworkDependencyFactory.Setup(x => x.GetRoundingMethodApplier()).Returns(roundingMethodApplierMock.Object);
			roundingMethodApplierMock.Setup(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>())).
				Returns((ZString roundingMethod, ZDecimal amount, ZBool isForiegnCurrency, ZInt currencyDecimals) => amount);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeaderPK = ZGuid.NewZGuid() });
			var reversedTaxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { OsTaxAmount = 100 });

			roundingMethodApplierMock.Invocations.Clear();
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(taxParentMock.Object);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(reversedTaxParentMock.Object);
			taxRecordCreatorMock.Setup(t => t.CreateCopiesOfTaxRecords(It.IsAny<Dictionary<ZGuid, ZGuid>>(), It.IsAny<AccTaxTransaction[]>())).Returns(new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>() { (reversedTaxRecord, new List<AccTaxRecordTransactionLinePivot>()) });
			reversedTaxParentMock.Setup(x => x.Factory).Returns(Factory);
			reversedTaxParentMock.Setup(x => x.GetLines()).Returns(new List<ITaxableTransactionLine>());

			taxRecordLoaderMock.Setup(getLoadMethodExpression).Returns(new[] { taxRecord });

			action(reverser, taxParentMock.Object, reversedTaxParentMock.Object);

			roundingMethodApplierMock.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), Times.Never, "Rounding method is not called when an amount is set during reversal.");
			AssertEquals("Postcondition: An amount property has changed and rounding method hasn't been called", -100m, reversedTaxRecord.ATT_OSTaxAmount);

			reversedTaxRecord.ATT_OSTaxAmount = 5;
			roundingMethodApplierMock.Verify(x => x.ApplyRounding(It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZBool>(), It.IsAny<ZInt>()), "Postcondition: when an amount is set explicitly on reverse tax record.");
		}

		public void TestReverseNonSPRAPTaxRecords_ValidationSuspended()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);
			var taxRecordCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			reverser.SubstituteTaxRecordCreator_ForTestOnly(taxRecordCreatorMock.Object);

			var postDate = ZDate.Today.AddDays(-10);
			reversedTaxParentMock.Setup(x => x.Factory).Returns(Factory);
			reversedTaxParentMock.Setup(x => x.GetLines()).Returns(new List<ITaxableTransactionLine>());

			var taxRecord1 = CreateNewTaxTransaction();
			var taxRecord1Copy = CreateNewTaxTransaction();
			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object)).Returns(new[] { taxRecord1 });
			taxRecordCreatorMock.Setup(t => t.CreateCopiesOfTaxRecords(It.IsAny<Dictionary<ZGuid, ZGuid>>(), It.IsAny<AccTaxTransaction[]>())).Returns(new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>() { (taxRecord1Copy, new List<AccTaxRecordTransactionLinePivot>()) });

			var invalidTaxRateErrorMessage = "Rate must be less than 100%";

			Assert("Precondition", !Factory.IsValidationSuspended);

			taxRecord1Copy.ATT_Rate = 150;
			AssertHasError("Precondition", taxRecord1Copy.ATT_RateInfo, invalidTaxRateErrorMessage);

			taxRecord1Copy.ATT_Rate = 65;
			AssertNoError("Precondition", taxRecord1Copy.ATT_RateInfo, invalidTaxRateErrorMessage);

			taxRecordCreatorMock.Setup(t => t.CreateCopiesOfTaxRecords(It.IsAny<Dictionary<ZGuid, ZGuid>>(), It.IsAny<AccTaxTransaction[]>())).Returns(new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>() { (taxRecord1Copy, new List<AccTaxRecordTransactionLinePivot>()) }).Callback(() => taxRecord1Copy.ATT_Rate = 120);
			((ITaxRecordReverser)reverser).ReverseNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, postDate);

			Assert(taxRecord1Copy.ATT_Rate > 100);
			AssertNoError("No error due to suspended validation", taxRecord1Copy.ATT_RateInfo, invalidTaxRateErrorMessage);
		}

		public void TestUpdatePostDateOnNonSPRTAPTaxRecords()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);
			var taxRecordCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			var taxParentLoaderMock = new Mock<ITaxFrameworkBOLoader>();

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			reverser.SubstituteTaxRecordCreator_ForTestOnly(taxRecordCreatorMock.Object);
			ObjectFactory.Substitute(taxParentLoaderMock.Object);

			var invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			var reverseInvoice = invoice.ReverseInvoice;

			reversedTaxParentMock.Setup(x => x.Factory).Returns(reverseInvoice.Factory);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(Factory, invoice.PK)).Returns(taxParentMock.Object);
			taxParentLoaderMock.Setup(x => x.LoadTaxRecordParent(reverseInvoice.Factory, reverseInvoice.PK)).Returns(reversedTaxParentMock.Object);

			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(reversedTaxParentMock.Object)).Returns(System.Array.Empty<AccTaxTransaction>());
			((ITaxRecordReverser)reverser).UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, ZDate.Today.AddDays(-5));

			taxRecordLoaderMock.Verify(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object), Times.Never);

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			var taxRecord1Copy = Factory.New<AccTaxTransaction>();
			taxRecord1Copy.ATT_AH = reverseInvoice.PK;
			var taxRecord2Copy = Factory.New<AccTaxTransaction>();
			taxRecord2Copy.ATT_AH = reverseInvoice.PK;

			var emptyDate = ZDate.Empty;
			var originalPropertyValue = ZDate.Today.AddDays(-10);

			taxRecord1.ATT_IsCancelled = true;
			taxRecord2.ATT_IsCancelled = true;
			taxRecord1.ATT_RealisationDate = originalPropertyValue;
			taxRecord2.ATT_RealisationDate = emptyDate;

			taxRecord1Copy.ATT_IsCancelled = true;
			taxRecord2Copy.ATT_IsCancelled = true;
			taxRecord1Copy.ATT_PostDate = originalPropertyValue;
			taxRecord2Copy.ATT_PostDate = originalPropertyValue;
			taxRecord1Copy.ATT_RealisationDate = originalPropertyValue;
			taxRecord2Copy.ATT_RealisationDate = originalPropertyValue;

			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(reversedTaxParentMock.Object)).Returns(new[] { taxRecord1Copy, taxRecord2Copy });
			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object)).Returns(new[] { taxRecord1, taxRecord2 });

			((ITaxRecordReverser)reverser).UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, emptyDate);
			taxRecordLoaderMock.Verify(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object), Times.Once);

			AssertEquals("Reversed tax record 1 ATT_RealisationDate", emptyDate, taxRecord1Copy.ATT_RealisationDate);
			AssertEquals("Reversed tax record 1 ATT_PostDate", emptyDate, taxRecord1Copy.ATT_PostDate);
			AssertEquals("Reversed tax record 2 ATT_RealisationDate", emptyDate, taxRecord2Copy.ATT_RealisationDate);
			AssertEquals("Reversed tax record 2 ATT_PostDate", emptyDate, taxRecord2Copy.ATT_PostDate);

			AssertEquals("Original tax record 1 ATT_RealisationDate", originalPropertyValue, taxRecord1.ATT_RealisationDate);
			AssertEquals("Original tax record 2 ATT_RealisationDate", emptyDate, taxRecord2.ATT_RealisationDate);

			taxRecordLoaderMock.Invocations.Clear();

			var postDate1 = ZDate.Today.AddDays(-8);
			((ITaxRecordReverser)reverser).UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, postDate1);
			taxRecordLoaderMock.Verify(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object), Times.Once);

			AssertEquals("Reversed tax record 1 ATT_RealisationDate", postDate1, taxRecord1Copy.ATT_RealisationDate);
			AssertEquals("Reversed tax record 1 ATT_PostDate", postDate1, taxRecord1Copy.ATT_PostDate);
			AssertEquals("Reversed tax record 2 ATT_RealisationDate", postDate1, taxRecord2Copy.ATT_RealisationDate);
			AssertEquals("Reversed tax record 2 ATT_PostDate", postDate1, taxRecord2Copy.ATT_PostDate);

			AssertEquals("Realisation date is set only when empty: taxRecord1.ATT_RealisationDate", originalPropertyValue, taxRecord1.ATT_RealisationDate); // Original tax record 1
			AssertEquals("Realisation date is set only when empty: taxRecord2.ATT_RealisationDate", postDate1, taxRecord2.ATT_RealisationDate); // Original tax record 2
		}

		public void TestReverseNotRealisedSPRAPRecords()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxLoaderMock.Setup(x => x.LoadSPRAPTaxRecords(taxParentMock.Object, true, false)).Returns(new[] { taxRecord1, taxRecord2 });

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxLoaderMock.Object);

			Assert("Precondition: ATT_IsCancelled", !taxRecord1.ATT_IsCancelled);
			Assert("Precondition: ATT_IsCancelled", !taxRecord2.ATT_IsCancelled);
			((ITaxRecordReverser)reverser).ReverseNotRealisedSPRAPRecords(taxParentMock.Object);
			Assert("ATT_IsCancelled", taxRecord1.ATT_IsCancelled);
			Assert("ATT_IsCancelled", taxRecord2.ATT_IsCancelled);
		}

		public void TestReverseRealisedSPRAPTaxRecordsSetsTaxAmountsOnPivotsCorrectly()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var transactionHeaderPK = ZGuid.NewZGuid();
			var matchTransactionPK = ZGuid.NewZGuid();
			var reverseTransactionPK = ZGuid.NewZGuid();

			reversedTaxParentMock.Setup(x => x.Factory).Returns(Factory);

			var postDate = new ZDate("2024-03-03");

			var taxRecord1 = CreateNewTaxTransaction();
			taxRecord1.ATT_AH = transactionHeaderPK;

			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_LocalTaxAmount = 4m;
			pivot1.ATP_ATT = taxRecord1.PK;

			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord1.PK;
			pivot2.ATP_LocalTaxAmount = -8m;

			taxRecordLoaderMock.Setup(t => t.LoadSPRAPTaxRecordsLinkedToMatchTransaction(Factory, matchTransactionPK)).Returns(new[] { taxRecord1 });
			((ITaxRecordReverser)reverser).ReverseRealisedSPRAPTaxRecords(Factory, matchTransactionPK, reverseTransactionPK, postDate);
			var reversedTaxRecordsQuery = new ZQuery(AccTaxTransactionSchema.PK, SQLComparisonOperator.NotEqual, taxRecord1.PK);
			var reversedTaxRecords = Factory.Load<AccTaxTransaction>(reversedTaxRecordsQuery);
			AssertEquals("Postcondition: Reversed Tax Records Count", 1, reversedTaxRecords.Length);
			var reversedTaxRecord1 = reversedTaxRecords[0];
			var reversedPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, reversedTaxRecord1.PK)).OrderBy(x => x.ATP_LocalTaxAmount).ToArray();
			AssertEquals("Postcondition: Reversed Pivots Count", 2, reversedPivots.Length);
			AssertEquals("Pivot 1 amount", -4m, reversedPivots[0].ATP_LocalTaxAmount);
			AssertEquals("Pivot 2 amount", 8m, reversedPivots[1].ATP_LocalTaxAmount);
		}

		public void TestReverseNonSPRAPTaxRecordsSetsTaxAmountsOnPivotsCorrectly()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>(MockBehavior.Strict);

			var reverser = new TaxRecordReverser();
			reverser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var postDate = ZDate.Today.AddDays(-10);

			var invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			var reverseInvoice = invoice.ReverseInvoice;

			reversedTaxParentMock.Setup(x => x.Factory).Returns(reverseInvoice.Factory);

			var taxRecord1 = CreateNewTaxTransaction();
			taxRecord1.ATT_AH = invoice.PK;

			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_LocalTaxAmount = 4m;
			pivot1.ATP_ATT = taxRecord1.PK;

			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord1.PK;
			pivot2.ATP_LocalTaxAmount = -8m;

			taxRecordLoaderMock.Setup(t => t.LoadNonSPRAPTaxRecords(taxParentMock.Object)).Returns(new[] { taxRecord1 });
			var taxableLine = (ITaxableTransactionLine)TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			var lines = new List<ITaxableTransactionLine>();
			lines.Add(taxableLine);
			ZDecimal originalParentTaxAmount = 100M;
			reversedTaxParentMock.SetupProperty(x => x.LocalTaxAmount, originalParentTaxAmount);
			reversedTaxParentMock.SetupProperty(x => x.OSTaxAmount, originalParentTaxAmount);
			reversedTaxParentMock.Setup(x => x.GetLines()).Returns(lines);
			AssertEquals("InvoicingLineBase and ITaxableTransactionLine represent same line", line.PK, taxableLine.PK);

			((ITaxRecordReverser)reverser).ReverseNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, postDate);

			var reversedTaxRecordsQuery = new ZQuery(AccTaxTransactionSchema.PK, SQLComparisonOperator.NotEqual, taxRecord1.PK);
			var reversedTaxRecords = Factory.Load<AccTaxTransaction>(reversedTaxRecordsQuery);
			AssertEquals("Postcondition : Reversed Tax Records Count", 1, reversedTaxRecords.Length);
			var reversedTaxRecord1 = reversedTaxRecords[0];
			var reversedPivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, reversedTaxRecord1.PK)).OrderBy(x => x.ATP_LocalTaxAmount).ToArray();
			AssertEquals("Postcondition : Reversed Pivots Count", 2, reversedPivots.Length);
			AssertEquals("Pivot 1 amount", -4m, reversedPivots[0].ATP_LocalTaxAmount);
			AssertEquals("Pivot 2 amount", 8m, reversedPivots[1].ATP_LocalTaxAmount);
		}

		AccTaxTransaction CreateNewTaxTransaction()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_ETC = TaxConfig.PK;
			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;

			return taxRecord;
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ??= new TaxFrameworkTestObjectCreator(Factory);
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		AccTaxConfiguration TaxConfig => taxConfig ??= TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
		AccTaxConfiguration taxConfig;
	}
}
