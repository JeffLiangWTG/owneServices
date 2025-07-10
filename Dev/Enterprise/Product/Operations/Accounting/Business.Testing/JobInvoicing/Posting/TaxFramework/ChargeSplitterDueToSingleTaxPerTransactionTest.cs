using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeSplitterDueToSingleTaxPerTransactionTest : TestCaseWithFactory
	{
		public void TestDependency()
		{
			var chargeSplitterDueToSingleTaxPerTransaction = new ChargeSplitterDueToSingleTaxPerTransaction();
			AssertType<TaxRecordCollectionValidator>(chargeSplitterDueToSingleTaxPerTransaction.TaxRecordCollectionValidator_ExposedForTestOnly);
		}

		public void TestGetSplitCharges_TaxFrameworkEnabledForNonBrazilISS()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Australia, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				Factory.Save();

				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);

				var result = chargeSplitter.GetSplitCharges(postingCharges);

				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Never);

				AssertEquals("The same collection is returned since ISS taxes configured for non-Brazil country", postingCharges, result);
			}
		}

		public void TestGetSplitCharges_TaxFrameworkEnabledForBrazilISS_ButLoggedIntoNonBrazilCompany()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var job = Factory.NewJobForTesting<Job>();
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);

				var result = chargeSplitter.GetSplitCharges(postingCharges);

				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Never);

				AssertEquals("The same collection is returned since the login country is not Brazil", postingCharges, result);
			}
		}

		public void TestGetSplitCharges_TaxFrameworkEnabledForBrazil_ButNoISSTaxesSetup()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(nonISSTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				Factory.Save();

				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);

				var result = chargeSplitter.GetSplitCharges(postingCharges);

				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Never);

				AssertEquals("The same collection is returned since there is no ISS taxes configured", postingCharges, result);
			}
		}

		public void TestGetSplitCharges_TaxFrameworkEnabledForBrazil_ButDifferentChargeCodesUsedForSplitting()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(nonISSTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, debtor: org);

				AssertCollectionNotContains("Precondition: charge1.ChargeCode not configured for Tax", charge1.ChargeCode, chargeCodes);
				AssertCollectionNotContains("Precondition: charge2.ChargeCode not configured for Tax", charge2.ChargeCode, chargeCodes);

				Factory.Save();

				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);

				var result = chargeSplitter.GetSplitCharges(postingCharges);

				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Never);

				AssertEquals("The same collection is returned since there is no ISS taxes configured", postingCharges, result);
			}
		}

		public void TestGetSplitCharges_GetEstimatedTaxRecordsByTaxSystemCodes_ReturnsErrorMessage()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				Factory.Save();
				var key1 = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges1 = new IReceivablesPostingChargeCollection();
				charges1.Add(charge1);
				charges1.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key1, charges1);

				var expectedError = "Some Error";
				var taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
				var taxProcessorMock = new Mock<ITaxProcessor>();

				List<ITaxRecordParentBase> taxParentsFromReceivableCharges = new List<ITaxRecordParentBase>();
				taxProcessorMock.Setup(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction))
					.Returns((taxRecordsWithLinePivots, expectedError))
					.Callback<ITaxRecordParentBase, IEnumerable<ZString>>((taxParent, taxSystemCodes) => taxParentsFromReceivableCharges.Add(taxParent));

				ObjectFactory.Substitute(taxProcessorMock.Object);

				AssertExceptionThrown<InterruptPostingException>("error message", expectedError, () => chargeSplitter.GetSplitCharges(postingCharges));
				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Once);

				AssertEquals(1, taxParentsFromReceivableCharges.Count);
				var taxParentFromReceivableCharge = taxParentsFromReceivableCharges[0];
				AssertEquals("TaxSystemSplitKey", 0, ((ReceivablesTaxParentFromPostingCharge)taxParentFromReceivableCharge).Key.TaxSystemSplitKey);
				AssertEquals("Factory is readonly", typeof(ReadOnlyBusinessObjectFactory), taxParentFromReceivableCharge.Factory.GetType());
			}
		}

		public void TestGetSplitCharges_GetEstimatedTaxRecordsByTaxSystemCodes_ReturnsNoErrorMessage()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				Factory.Save();
				var key1 = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges1 = new IReceivablesPostingChargeCollection();
				charges1.Add(charge1);
				charges1.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key1, charges1);

				var taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
				var taxProcessorMock = new Mock<ITaxProcessor>();

				List<ITaxRecordParentBase> taxParentsFromReceivableCharges = new List<ITaxRecordParentBase>();
				taxProcessorMock.Setup(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction))
					.Returns((taxRecordsWithLinePivots, string.Empty))
					.Callback<ITaxRecordParentBase, IEnumerable<ZString>>((taxParent, taxSystemCodes) => taxParentsFromReceivableCharges.Add(taxParent));

				ObjectFactory.Substitute(taxProcessorMock.Object);

				AssertNoExceptionThrown("error message", () => chargeSplitter.GetSplitCharges(postingCharges));
				taxProcessorMock.Verify(x => x.GetEstimatedTaxRecordsByTaxSystemCodes(It.IsAny<ReceivablesTaxParentFromPostingCharge>(), taxSystemCodesWithLimitRestriction), Times.Exactly(1));

				AssertEquals(1, taxParentsFromReceivableCharges.Count);

				var taxParentFromReceivableCharge = taxParentsFromReceivableCharges[0];
				AssertEquals("TaxSystemSplitKey", 0, ((ReceivablesTaxParentFromPostingCharge)taxParentFromReceivableCharge).Key.TaxSystemSplitKey);
				AssertEquals("Factory is readonly", typeof(ReadOnlyBusinessObjectFactory), taxParentFromReceivableCharge.Factory.GetType());
			}
		}

		public void TestGetSplitCharges_MoreThanOneGroupsPassed_OneGroupRequiringSplit_OtherGroupNotRequiringSplit()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: org);
				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, debtor: org);

				Factory.Save();
				var key1 = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges1 = new IReceivablesPostingChargeCollection();
				charges1.Add(charge1);
				charges1.Add(charge2);

				var key2 = new PostingChargeKey(org.PK, "XYZ", ZGuid.Empty, ZGuid.Empty, 0);
				var charges2 = new IReceivablesPostingChargeCollection();
				charges2.Add(charge3);
				charges2.Add(charge4);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key1, charges1);
				postingCharges.SetCharges(key2, charges2);

				AssertEquals("Precondition: 4 charges share two keys before calling GetSplitCharges()", 2, postingCharges.Keys.Count);
				var result = chargeSplitter.GetSplitCharges(postingCharges);
				AssertEquals(3, result.Keys.Count);

				var key1_1 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 0 && x.InvoiceType == key1.InvoiceType);
				AssertNotNull(key1_1);
				var chargeCollectionWithKey1_1 = result.GetCharges(key1_1);
				AssertEquals("1 charges with key1_1", 1, chargeCollectionWithKey1_1.Count);
				Assert("charge1 has key1", chargeCollectionWithKey1_1.Contains(charge1));

				var key1_2 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 1 && x.InvoiceType == key1.InvoiceType);
				AssertNotNull(key1_2);
				var chargeCollectionWithKey1_2 = result.GetCharges(key1_2);
				AssertEquals("1 charge with key1_2", 1, chargeCollectionWithKey1_2.Count);
				Assert("charge2 has key2", chargeCollectionWithKey1_2.Contains(charge2));

				var key2_1 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 0 && x.InvoiceType == key2.InvoiceType);
				AssertNotNull(key2_1);
				AssertEquals("key2 is actually unchanged", key2_1, key2);
				var chargeCollectionWithKey2 = result.GetCharges(key2_1);
				AssertEquals("2 charges with key2", 2, chargeCollectionWithKey2.Count);
				Assert("charge3 has key2", chargeCollectionWithKey2.Contains(charge3));
				Assert("charge4 has key2", chargeCollectionWithKey2.Contains(charge4));
			}
		}

		public void TestGetSplitCharges_SingleISSTaxFromMultipleChargeCodes()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupSingleTaxForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org);
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org);

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: both chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);
				var result = chargeSplitter.GetSplitCharges(postingCharges);
				AssertEquals("Charges are not split since a single ISS tax is generated", 1, result.Keys.Count);

				AssertEquals("The key remains the same", key, result.Keys.Cast<PostingChargeKey>().First());
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromSingleChargeCode_Case1()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCode = SetupMultipleTaxesForSingleChargeCode(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				AssertNotNull("Precondition", chargeCode);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCode, debtor: org);

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: both chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);

				var expectedErrorMessage = @"Posting is prevented because Tax defaulting rules for Charge Code 'ZZCC1' would create more than one Tax record for the 'ISS' Tax System.
Please review the Tax Override rules configured for this charge code.";

				AssertExceptionThrown<InterruptPostingException>("error message", expectedErrorMessage, () => chargeSplitter.GetSplitCharges(postingCharges));
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromSingleChargeCode_Case2()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCode = SetupMultipleTaxesForSingleChargeCode(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org, "TA3", "TA4");

				AssertNotNull("Precondition", chargeCode);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCode, debtor: org); //will create multiple TaxTransactions
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org); //responsible for creating TaxTransaction1
				var charge3 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org); //responsible for creating TaxTransaction2

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: both chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);

				var expectedErrorMessage = @"Posting is prevented because Tax defaulting rules for Charge Code 'ZZCC1' would create more than one Tax record for the 'ISS' Tax System.
Please review the Tax Override rules configured for this charge code.";

				AssertExceptionThrown<InterruptPostingException>("error message", expectedErrorMessage, () => chargeSplitter.GetSplitCharges(postingCharges));
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromSingleChargeCode_Case3()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCode1 = SetupMultipleTaxesForSingleChargeCode(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);
				var chargeCode2 = SetupMultipleTaxesForSingleChargeCode(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org, "TA3", "TA4", TestObjectCreator.CC2);

				AssertNotNull("Precondition", chargeCode1);
				AssertNotNull("Precondition", chargeCode2);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCode1, debtor: org); //will create multiple TaxTransactions
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCode2, debtor: org); //will create multiple TaxTransactions

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: both chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);

				var expectedErrorMessage = @"Posting is prevented because Tax defaulting rules for Charge Code 'ZZCC1' would create more than one Tax record for the 'ISS' Tax System.
Please review the Tax Override rules configured for this charge code.";

				AssertExceptionThrown<InterruptPostingException>("error message", expectedErrorMessage, () => chargeSplitter.GetSplitCharges(postingCharges));
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromMultipleChargeCodes_Case1()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org); //responsible fore createing TaxTransaction1
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org); //responsible fore createing TaxTransaction2
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: org); //Does not create any TaxTransaction

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: both chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);
				var result = chargeSplitter.GetSplitCharges(postingCharges);
				AssertEquals(2, result.Keys.Count);

				var key1 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 0);
				AssertNotNull(key1);
				var chargeCollectionWithKey1 = result.GetCharges(key1);
				AssertEquals("2 charges with key1", 2, chargeCollectionWithKey1.Count);
				Assert("charge1 has key1", chargeCollectionWithKey1.Contains(charge1));
				Assert("charge3 has key3", chargeCollectionWithKey1.Contains(charge3));

				var key2 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 1);
				AssertNotNull(key2);
				var chargeCollectionWithKey2 = result.GetCharges(key2);
				AssertEquals("1 charge with key2", 1, chargeCollectionWithKey2.Count);
				Assert("charge2 has key2", chargeCollectionWithKey2.Contains(charge2));
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromMultipleChargeCodes_Case2()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org); //responsible fore createing TaxTransaction1
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org); //responsible fore createing TaxTransaction2
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: org); //Does not create any TaxTransaction
				var charge4 = TestObjectCreator.CreateCharge(job, chargeCodes[2], debtor: org); //responsible fore createing TaxTransaction1
				var charge5 = TestObjectCreator.CreateCharge(job, chargeCodes[3], debtor: org); //responsible fore createing TaxTransaction2

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);
				charges.Add(charge4);
				charges.Add(charge5);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: all chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);
				var result = chargeSplitter.GetSplitCharges(postingCharges);
				AssertEquals(2, result.Keys.Count);

				var key1 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 0);
				AssertNotNull(key1);
				var chargeCollectionWithKey1 = result.GetCharges(key1);
				AssertEquals("3 charges with key1", 3, chargeCollectionWithKey1.Count);
				Assert("charge1 has key1", chargeCollectionWithKey1.Contains(charge1));
				Assert("charge3 has key1", chargeCollectionWithKey1.Contains(charge3));
				Assert("charge4 has key1", chargeCollectionWithKey1.Contains(charge4));

				var key2 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 1);
				AssertNotNull(key2);
				var chargeCollectionWithKey2 = result.GetCharges(key2);
				AssertEquals("2 charges with key2", 2, chargeCollectionWithKey2.Count);
				Assert("charge2 has key2", chargeCollectionWithKey2.Contains(charge2));
				Assert("charge5 has key2", chargeCollectionWithKey2.Contains(charge5));
			}
		}

		public void TestGetSplitCharges_MultipleISSTaxesFromMultipleChargeCodes_Case3()
		{
			var org = TestObjectCreator.Debtor;

			IChargeSplitterDueToSingleTaxPerTransaction chargeSplitter = new ChargeSplitterDueToSingleTaxPerTransaction();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var chargeCodes = SetupMultipleTaxesForMultipleChargeCodes(issTaxSystemCode, Core.Constants.CountryCodes.Brazil, org);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCodes[0], debtor: org); //responsible fore createing TaxTransaction1
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCodes[1], debtor: org); //responsible fore createing TaxTransaction2
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: org); //Does not create any TaxTransaction
				var charge4 = TestObjectCreator.CreateCharge(job, chargeCodes[2], debtor: org); //responsible fore createing TaxTransaction1
				var charge5 = TestObjectCreator.CreateCharge(job, chargeCodes[3], debtor: org); //responsible fore createing TaxTransaction2
				var charge6 = TestObjectCreator.CreateCharge(job, chargeCodes[4], debtor: org); //responsible fore createing TaxTransaction3

				Factory.Save();
				var key = new PostingChargeKey(org.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);
				charges.Add(charge4);
				charges.Add(charge5);
				charges.Add(charge6);

				var postingCharges = new PostingChargeCollection();
				postingCharges.SetCharges(key, charges);

				AssertEquals("Precondition: all chanrges share one key before calling GetSplitCharges()", 1, postingCharges.Keys.Count);
				var result = chargeSplitter.GetSplitCharges(postingCharges);
				AssertEquals(3, result.Keys.Count);

				var key1 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 0);
				AssertNotNull(key1);
				var chargeCollectionWithKey1 = result.GetCharges(key1);
				AssertEquals("3 charges with key1", 3, chargeCollectionWithKey1.Count);
				Assert("charge1 has key1", chargeCollectionWithKey1.Contains(charge1));
				Assert("charge3 has key1", chargeCollectionWithKey1.Contains(charge3));
				Assert("charge4 has key1", chargeCollectionWithKey1.Contains(charge4));

				var key2 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 1);
				AssertNotNull(key2);
				var chargeCollectionWithKey2 = result.GetCharges(key2);
				AssertEquals("2 charges with key2", 2, chargeCollectionWithKey2.Count);
				Assert("charge2 has key2", chargeCollectionWithKey2.Contains(charge2));
				Assert("charge5 has key2", chargeCollectionWithKey2.Contains(charge5));

				var key3 = result.Keys.Cast<PostingChargeKey>().First(x => x.TaxSystemSplitKey == 2);
				AssertNotNull(key3);
				var chargeCollectionWithKey3 = result.GetCharges(key3);
				AssertEquals("1 charge with key3", 1, chargeCollectionWithKey3.Count);
				Assert("charge6 has key3", chargeCollectionWithKey3.Contains(charge6));
			}
		}

		List<AccChargeCode> SetupMultipleTaxesForMultipleChargeCodes(ZString taxCode, ZString countryCode, OrgHeader org, string taxAuthorityCode1 = "TA1", string taxAuthorityCode2 = "TA2")
		{
			var taxAuthority1 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode1);
			var taxAuthority2 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode2);
			var taxSystem = TaxTestObjectCreator.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
			taxSystem.Code = taxCode;
			taxSystem.Country = countryCode;

			var taxConfiguration1 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfiguration2 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var organization = org;
			var company = GlbCompany.CurrentCompany;
			var companyData = organization.GetCompanyDataForGlbCompany(company);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData);

			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;
			var chargeCode10 = TestObjectCreator.CC10;
			var chargeCode11 = TestObjectCreator.CC11;
			var chargeCode12 = TestObjectCreator.CC9;

			var taxFrameTaxOverrideGroup1 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration1);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode10);

			var taxFrameTaxOverrideGroup2 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration2);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode11);

			var taxFrameTaxOverrideGroup3 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration1);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup3, chargeCode12);

			var taxID1 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX1");
			taxID1.SetRate_ForTestOnly(10, 1);

			var taxID2 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX2");
			taxID2.SetRate_ForTestOnly(5, 1);

			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);
			var taxMessage = TestObjectCreator.CreateTaxMsg("ISSP", "ISS tax for shipping lines", string.Empty, string.Empty);
			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup3, taxID1.PK, taxMessage.PK);

			Factory.Save();

			return new List<AccChargeCode>() { chargeCode1, chargeCode2, chargeCode10, chargeCode11, chargeCode12 };
		}

		AccChargeCode SetupMultipleTaxesForSingleChargeCode(ZString taxCode, ZString countryCode, OrgHeader org, string taxAuthorityCode1 = "TA1", string taxAuthorityCode2 = "TA2", AccChargeCode chargeCodeToSetup = null)
		{
			var taxAuthority1 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode1);
			var taxAuthority2 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode2);
			var taxSystem = TaxTestObjectCreator.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
			taxSystem.Code = taxCode;
			taxSystem.Country = countryCode;

			var taxConfiguration1 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfiguration2 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var organization = org;
			var company = GlbCompany.CurrentCompany;
			var companyData = organization.GetCompanyDataForGlbCompany(company);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData);

			var chargeCode = chargeCodeToSetup ?? TestObjectCreator.CC1;
			var taxFrameTaxOverrideGroup1 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration1);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

			var taxFrameTaxOverrideGroup2 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration2);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode);

			var taxID1 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX1");
			taxID1.SetRate_ForTestOnly(10, 1);

			var taxID2 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX2");
			taxID2.SetRate_ForTestOnly(5, 1);

			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);

			Factory.Save();

			return chargeCode;
		}

		List<AccChargeCode> SetupSingleTaxForMultipleChargeCodes(ZString taxCode, ZString countryCode, OrgHeader org)
		{
			var taxAuthority = TaxTestObjectCreator.CreateTaxAuthority("TA1");

			var taxSystem = TaxTestObjectCreator.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
			taxSystem.Code = taxCode;
			taxSystem.Country = countryCode;

			var taxConfiguration = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var organization = org;
			var company = GlbCompany.CurrentCompany;
			var companyData = organization.GetCompanyDataForGlbCompany(company);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData);

			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;
			var taxFrameTaxOverrideGroup1 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);

			var taxFrameTaxOverrideGroup2 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);

			var taxID1 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX1");
			taxID1.SetRate_ForTestOnly(10, 1);

			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID1.PK);

			Factory.Save();

			return new List<AccChargeCode>() { chargeCode1, chargeCode2 };
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		AccountingTestObjectCreator TaxTestObjectCreator => taxTestObjectCreator ?? (taxTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator taxTestObjectCreator;

		const string issTaxSystemCode = "ISS";
		const string nonISSTaxSystemCode = "PER";

		readonly List<ZString> taxSystemCodesWithLimitRestriction = new List<ZString> { issTaxSystemCode };
	}
}
