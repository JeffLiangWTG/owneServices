using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ARInvoiceNumberLengthConfigurationRegistryDataType))]
	class ARInvoiceNumberLengthConfigurationRegistryDataTypeTest : RegistryDataTypeTestCase<ARInvoiceNumberLengthConfigurationRegistryDataType>
	{
		protected override ARInvoiceNumberLengthConfigurationRegistryDataType GetNewDataType()
		{
			return new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(5, Encoding.Unicode.GetBytes("5")),
				new ValidSampleAndBinaryValueInDB(6, Encoding.Unicode.GetBytes("6")),
				new ValidSampleAndBinaryValueInDB(7, Encoding.Unicode.GetBytes("7")),
				new ValidSampleAndBinaryValueInDB(8, Encoding.Unicode.GetBytes("8"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { 4, 9 };
		}

		public void TestValidation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ARInvoice arInvoice = factory.NewWithValidTestData<ARInvoice>();
			APInvoice apInvoice = factory.NewWithValidTestData<APInvoice>();
			arInvoice.AH_TransactionNum = "0123456";
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			apInvoice.AH_TransactionNum = "00123456";

			factory.Save();

			ARInvoiceNumberLengthConfigurationRegistryDataType dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			ARInvoiceNumberLengthConfigurationRegistryItem registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 8, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestValidateBeforeRegistryFormSave()
		{
			var mockRegistryChangesNotifier = new Mock<IRegistryChangesNotifier>();

			var dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			var registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);
			using (ObjectFactory.Substitute(mockRegistryChangesNotifier.Object))
			{
				AssertNoExceptionThrown(() => dataType.ValidateBeforeRegistryFormSave(registryItem, 8, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				mockRegistryChangesNotifier.Verify(x => x.Notify("The changes will make effect after system restart"), Times.Once);
			}
		}

		public void TestDuplicationWhenTransactionNumberSequenceIsUsedAndThenUnticked()
		{
			TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation customElement = collection.AddNew();
			customElement.ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1;
			customElement.Order = 1;
			customElement.Include = true;
			customElement.Code = "1";

			TransactionNumberSequenceCustomisation sequence = collection.AddNew();
			sequence.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			sequence.Order = 2;
			sequence.Include = true;
			sequence.Length = 5;

			TransactionsNumberSequenceCustomisationRegistryItem numbersequenceRegistryItem = new TransactionsNumberSequenceCustomisationRegistryItem("TransactionNumberSequenceCustomisation", null, null, null, RegistryStorageFlags.Company);
			TransactionsNumberSequenceCustomisationRegistryDataType numbersequenceDataType = new TransactionsNumberSequenceCustomisationRegistryDataType();

			numbersequenceDataType.Validate(numbersequenceRegistryItem, collection, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			numbersequenceRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var testObjectfactory = new BusinessObjectFactory();

			ARInvoice arInvoice = testObjectfactory.New<ARInvoice>();
			arInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			testObjectfactory.Save();
			ARInvoice arInvoice2 = testObjectfactory.New<ARInvoice>();
			arInvoice2.AH_GC = GlbCompany.CurrentCompany.PK;
			testObjectfactory.Save();

			//Setting the next value for ARInvoice no as 100000
			Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain().SetValues(TestConnection, minValue: 1000, nextValue: 100000, maxValue: 99999999);

			ARInvoiceNumberLengthConfigurationRegistryDataType dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			ARInvoiceNumberLengthConfigurationRegistryItem registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("ARInvoiceNumberLengthConfiguration", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var sqlText2 = @"UPDATE dbo.StmData SET SD_BinaryValue =  NULL
				WHERE SD_Name = 'TransactionNumberSequenceCustomisation'
				AND SD_Owner = @Company";

			using (var command = Db.Connection.Command(sqlText2))  // Avoid use of factory to improve memory usage AND this is a very efficient query.
			{
				command.AddParameterBasedOnDbColumn("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), StmDataSchema.SD_Owner);
				command.ExecuteNonQuery();
			}//Equivalent of Unticking Number Sequence Customisation

			AssertExceptionThrown("Duplication", typeof(RegistryValidationException),
			"You have changed the AR invoice length to 6. However, this will auto-generate a duplicate transaction number for this company that conflicts with the existing AR invoices in the system. You can either change the 'AR Invoice Number Length' or use the 'Number Sequence Customization' registry setting.",
								() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestValidationWithChangingARInvoiceLengths()
		{
			var dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			var registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("ARInvoiceNumberLengthConfiguration", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);

			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var testObjectfactory = new BusinessObjectFactory();
			var arInvoice1 = testObjectfactory.New<ARInvoice>();
			arInvoice1.AH_TransactionNum = "001000";
			arInvoice1.IsManuallySetTransactionNumber_ForTestOnly = true;

			testObjectfactory.Save();

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 7, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 8, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException),
				"You have transaction numbers in the database that exceed this length. The minimum number of digits allowed for this company is 6.",
				() => dataType.Validate(registryItem, 5, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestForAnyExceptionsWithLargeTransactionNumberIsInDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ARInvoice arInvoice = factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_TransactionNum = "11000011000";
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;

			factory.Save();

			ARInvoiceNumberLengthConfigurationRegistryDataType dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			ARInvoiceNumberLengthConfigurationRegistryItem registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("ARInvoiceNumberLengthConfiguration", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 5, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 7, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 8, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestTransactionWithNonDigitsTransactionNumber()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ARInvoice arInvoice = factory.New<ARInvoice>();
			arInvoice.AH_TransactionNum = "T0123456";
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARInvoice arInvoice2 = factory.New<ARInvoice>();
			arInvoice2.AH_TransactionNum = "FIT0123456";
			arInvoice2.IsManuallySetTransactionNumber_ForTestOnly = true;

			factory.Save();

			ARInvoiceNumberLengthConfigurationRegistryDataType dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			ARInvoiceNumberLengthConfigurationRegistryItem registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("ARInvoiceNumberLengthConfiguration", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, 6, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestARInvoiceLengthProposedValueOutsideBounds()
		{
			ARInvoiceNumberLengthConfigurationRegistryDataType dataType = new ARInvoiceNumberLengthConfigurationRegistryDataType(5, 8);
			ARInvoiceNumberLengthConfigurationRegistryItem registryItem = new ARInvoiceNumberLengthConfigurationRegistryItem("ARInvoiceNumberLengthConfiguration", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);

			AssertExceptionThrown(typeof(RegistryValidationException),
			"Value must be greater than or equal to the minimum (5)",
			() => dataType.Validate(registryItem, 4, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException),
				"Value must be less than or equal to the maximum (8)",
				() => dataType.Validate(registryItem, 9, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}
	}
}
