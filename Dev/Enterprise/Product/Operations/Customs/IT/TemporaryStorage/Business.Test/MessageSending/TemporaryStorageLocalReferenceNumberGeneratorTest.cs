using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageLocalReferenceNumberGenerator))]
sealed class TemporaryStorageLocalReferenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When header is null",
			() => new TemporaryStorageLocalReferenceNumberGenerator(null, Mock.Of<ILocalReferenceNumberGenerator>()));

		AssertExceptionThrown<ArgumentNullException>(
			"When localReferenceNumberGenerator is null",
			() => new TemporaryStorageLocalReferenceNumberGenerator(Factory.New<TemporaryStorageHeader>(), null));
	}

	public void TestGenerate_ShouldGenerateNewLrnForBillsWithoutLrnAndMrn()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill1 = header.MasterBill;
		var bill2 = header.Bills.AddNew();

		var mockLrnGenerator = new Mock<ILocalReferenceNumberGenerator>();
		mockLrnGenerator.SetupSequence(x => x.Generate())
			.Returns("NEW_LRN_1")
			.Returns("NEW_LRN_2");

		ILocalReferenceNumberGenerator generator = new TemporaryStorageLocalReferenceNumberGenerator(header, mockLrnGenerator.Object);
		var generatedLrn = generator.Generate();

		mockLrnGenerator.Verify(x => x.Generate(), Times.Exactly(2));
		AssertEquals("LRN returned from the method", "NEW_LRN_1", generatedLrn);
		AssertEquals("Bill1 > Lrn", "NEW_LRN_1", bill1.Lrn);
		AssertEquals("Bill2 > Lrn", "NEW_LRN_2", bill2.Lrn);
	}

	public void TestGenerate_ShouldOverwriteExistingLrnForBillsWithoutMrn()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill1 = header.MasterBill;
		var bill2 = header.Bills.AddNew();

		var lrnEntryNumber1 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill1, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber1.CE_EntryNum = "EXISTING_LRN_1";
		var lrnEntryNumber2 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill2, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber2.CE_EntryNum = "EXISTING_LRN_2";

		var mockLrnGenerator = new Mock<ILocalReferenceNumberGenerator>();
		mockLrnGenerator.SetupSequence(x => x.Generate())
			.Returns("NEW_LRN_3")
			.Returns("NEW_LRN_4");

		ILocalReferenceNumberGenerator generator = new TemporaryStorageLocalReferenceNumberGenerator(header, mockLrnGenerator.Object);
		var generatedLrn = generator.Generate();

		mockLrnGenerator.Verify(x => x.Generate(), Times.Exactly(2));
		AssertEquals("LRN returned from the method", "NEW_LRN_3", generatedLrn);
		AssertEquals("Bill1 > Lrn", "NEW_LRN_3", bill1.Lrn);
		AssertEquals("Bill2 > Lrn", "NEW_LRN_4", bill2.Lrn);
	}

	public void TestGenerate_ShouldNotOverwriteExistingLrnWhenBillsHaveMrn()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill1 = header.MasterBill;
		var bill2 = header.Bills.AddNew();

		var lrnEntryNumber1 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill1, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber1.CE_EntryNum = "EXISTING_LRN_1";
		var lrnEntryNumber2 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill2, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber2.CE_EntryNum = "EXISTING_LRN_2";

		var mrnEntryNumber1 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill1, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber1.CE_EntryNum = "MRN_1";
		var mrnEntryNumber2 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill2, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber2.CE_EntryNum = "MRN_2";

		var mockLrnGenerator = new Mock<ILocalReferenceNumberGenerator>();
		mockLrnGenerator.SetupSequence(x => x.Generate());

		ILocalReferenceNumberGenerator generator = new TemporaryStorageLocalReferenceNumberGenerator(header, mockLrnGenerator.Object);
		var generatedLrn = generator.Generate();

		mockLrnGenerator.Verify(x => x.Generate(), Times.Never);
		AssertNull("LRN returned from the method", generatedLrn);
		AssertEquals("Bill1 > Lrn", "EXISTING_LRN_1", bill1.Lrn);
		AssertEquals("Bill2 > Lrn", "EXISTING_LRN_2", bill2.Lrn);
	}
}
