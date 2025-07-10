using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Utility.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordRealiserTest : TestCaseWithFactory
	{
		public void TestDependencies()
		{
			var taxRecordRealiser = new TaxRecordRealiser();
			AssertType(typeof(TaxRecordLoader), taxRecordRealiser.TaxRecordLoader_ExposedForTestOnly);
		}

		[TestDate(2020, 01, 15)]
		public void TestTaxRecordRealiser_RealisesCorrectTaxRecord()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParent = taxParentMock.Object;
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();

			var taxRealiser = new TaxRecordRealiser();
			var itaxRealiser = (ITaxRecordRealiser)taxRealiser;
			taxRealiser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();

			taxRecordLoaderMock.Setup(x => x.LoadMatchingBasisTaxRecords(taxParent)).Returns(new[] { taxRecord1, taxRecord2 });

			var realisationDate = ZDate.Today;
			itaxRealiser.RealiseTaxRecord(taxParent, realisationDate);

			AssertRealisationProperties(taxRecord1, realisationDate);
			AssertRealisationProperties(taxRecord2, realisationDate);
		}

		public void TestRealisePaymentRetentionTaxRecords_ReturnsErrorWhenTaxRecordPKsMismatch()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParent = taxParentMock.Object;
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();

			var taxRealiser = new TaxRecordRealiser();
			var itaxRealiser = (ITaxRecordRealiser)taxRealiser;
			taxRealiser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var taxRecord1 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });
			var taxRecord2 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });
			var taxRecord3 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });

			taxRecordLoaderMock.Setup(x => x.LoadSPRAPTaxRecords(taxParent, true, true)).Returns(new[] { taxRecord1, taxRecord2, taxRecord3 });

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord1.PK, taxRecord2.PK });
			var matchTranasctionDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { ZGuid.NewZGuid() });
			var matchTranasctionDetails3 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord3.PK });

			var expectedErrorMessage = "Some of the Tax Records have been already realized by another user. Please cancel this operation and try again.";

			var errorMessage = itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1, matchTranasctionDetails2 });
			AssertEquals("3 tax records linked to tax parent but passed matchTransactionDetails has 1 tax record guid which is not in the 3 tax records, so no tax record will be processed.", expectedErrorMessage, errorMessage);
			taxRecordLoaderMock.Verify(x => x.LoadSPRAPTaxRecords(taxParent, true, true), Times.Once);
			taxRecordLoaderMock.Invocations.Clear();

			errorMessage = itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1 });
			AssertEquals("3 tax records linked to tax parent and passed matchTransactionDetails contains 2 of these tax records, so 2 tax records should be processed", string.Empty, errorMessage);
			taxRecordLoaderMock.Verify(x => x.LoadSPRAPTaxRecords(taxParent, true, true), Times.Once);
			taxRecordLoaderMock.Invocations.Clear();

			errorMessage = itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1, matchTranasctionDetails2, matchTranasctionDetails3 });
			AssertEquals("3 tax records linked to tax parent, passed matchTransactionDetails has one additional guid, so no tax record will be processed", expectedErrorMessage, errorMessage);
			taxRecordLoaderMock.Verify(x => x.LoadSPRAPTaxRecords(taxParent, true, true), Times.Once);
			taxRecordLoaderMock.Invocations.Clear();

			errorMessage = itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1, matchTranasctionDetails3 });
			AssertEquals("Tax records pks linked to tax parent from cache is same as tax parents passed through MatchTransactionDeails collection", string.Empty, errorMessage);
			taxRecordLoaderMock.Verify(x => x.LoadSPRAPTaxRecords(taxParent, true, true), Times.Once);
			taxRecordLoaderMock.Invocations.Clear();
		}

		[TestDate(2020, 05, 15)]
		public void TestRealisePaymentRetentionTaxRecords_ThrowsExceptionIfTaxBasisIsWrong()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParent = taxParentMock.Object;
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();

			var taxRealiser = new TaxRecordRealiser();
			var itaxRealiser = (ITaxRecordRealiser)taxRealiser;
			taxRealiser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var taxRecord1 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.Posting.Code });
			var taxRecord2 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });

			taxRecordLoaderMock.Setup(x => x.LoadSPRAPTaxRecords(taxParent, true, true)).Returns(new[] { taxRecord1, taxRecord2 });

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord1.PK, taxRecord2.PK });
			matchTranasctionDetails1.PK = ZGuid.NewZGuid();
			matchTranasctionDetails1.RealisationDate = ZDate.Today;

			var expectedError = "SPR tax record has invalid tax basis 'PST'. Only 'PTM' tax basis is allowed for SPR tax";
			AssertExceptionThrown("One of the tax records have incorrect tax basis", typeof(TaxFrameworkUnknownConfigurationValueException),
				expectedError,
				() => itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1 }));
			taxRecordLoaderMock.Verify(x => x.LoadSPRAPTaxRecords(taxParent, true, true), Times.Once);

			CombineAssertions("None of the tax records are processed since one of the records have incorrect tax basis", () =>
			{
				AssertRealisationProperties(taxRecord1, ZDate.Empty, ZGuid.Empty);
				AssertRealisationProperties(taxRecord2, ZDate.Empty, ZGuid.Empty);
			});

			ErrorReporter.Clear();
		}

		[TestDate(2020, 01, 15)]
		public void TestRealisePaymentRetentionTaxRecords_RealisesValidTaxRecordsCorrectly()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParent = taxParentMock.Object;
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();

			var taxRealiser = new TaxRecordRealiser();
			var itaxRealiser = (ITaxRecordRealiser)taxRealiser;
			taxRealiser.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);

			var taxRecord1 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });
			var taxRecord2 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });
			var taxRecord3 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters() { TaxBasis = TaxBasisList.PostingOnMatching.Code });

			taxRecordLoaderMock.Setup(x => x.LoadSPRAPTaxRecords(taxParent, true, true)).Returns(new[] { taxRecord1, taxRecord2, taxRecord3 });

			var postDateJournal1 = ZDate.Today.AddDays(-5);
			var apJournal1 = TestObjectCreator.CreateJournal<APJournal>(10M, postDateJournal1, TestObjectCreator.ABIGAS.PK);
			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord1.PK, taxRecord2.PK });
			matchTranasctionDetails1.PK = apJournal1.PK;
			matchTranasctionDetails1.RealisationDate = apJournal1.AH_PostDate.Date;

			var postDateJournal2 = ZDate.Today;
			var apJournal2 = TestObjectCreator.CreateJournal<APJournal>(10M, postDateJournal2, TestObjectCreator.ABIGAS.PK);
			var matchTranasctionDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord3.PK });
			matchTranasctionDetails2.PK = apJournal2.PK;
			matchTranasctionDetails2.RealisationDate = apJournal2.AH_PostDate.Date;

			CombineAssertions(() =>
			{
				AssertRealisationProperties(taxRecord1, ZDate.Empty, ZGuid.Empty);
				AssertRealisationProperties(taxRecord2, ZDate.Empty, ZGuid.Empty);
				AssertRealisationProperties(taxRecord3, ZDate.Empty, ZGuid.Empty);
			});

			itaxRealiser.RealisePaymentRetentionTaxRecords(taxParent, new List<IMatchTransactionDetails>() { matchTranasctionDetails1, matchTranasctionDetails2 });

			CombineAssertions(() =>
			{
				AssertRealisationProperties(taxRecord1, postDateJournal1, apJournal1.PK);
				AssertRealisationProperties(taxRecord2, postDateJournal1, apJournal1.PK);
				AssertRealisationProperties(taxRecord3, postDateJournal2, apJournal2.PK);
			});
		}

		void AssertRealisationProperties(AccTaxTransaction taxRecord, ZDate expectedRealisationDate, ZGuid expectedMatchTransactionPK = default)
		{
			AssertEquals(expectedRealisationDate, taxRecord.ATT_RealisationDate);
			AssertEquals(expectedMatchTransactionPK, taxRecord.ATT_AH_MatchTransaction);
		}

		TaxFrameworkTestObjectCreator TaxFrameworkObjectCreator => taxFrameworkObjectCreator ?? (taxFrameworkObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkObjectCreator;
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
