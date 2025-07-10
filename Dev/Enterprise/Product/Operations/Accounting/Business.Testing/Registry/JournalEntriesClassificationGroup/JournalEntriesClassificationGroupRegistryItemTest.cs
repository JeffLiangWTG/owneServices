using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupRegistryItem))]

	public class JournalEntriesClassificationGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<JournalEntriesClassificationGroupCollection>
	{
		protected override StronglyTypedRegistryItem<JournalEntriesClassificationGroupCollection, JournalEntriesClassificationGroupCollection> GetNewRegistryItem()
		{
			return new JournalEntriesClassificationGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}

	[TestedType(typeof(JournalEntriesClassificationGroupRegistryDataType))]
	public class JournalEntriesClassificationGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JournalEntriesClassificationGroupRegistryDataType>
	{
		public void TestValidationException_WhenAllocationOptionIsGen()
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.SetJournalEntriesNumberAllocationOptionToGen(GlbCompany.CurrentCompany);

			var registryDataType = GetNewDataType();
			var validSamples = GetValidSamples();
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup;

			foreach (var validSampleAndBinaryValueInDb in validSamples)
			{
				var validSample = validSampleAndBinaryValueInDb.ValidSample as JournalEntriesClassificationGroupCollection;
				AssertExceptionThrown<RegistryValidationException>(
					message: "Exception occurs when Allocation Option has already been GEN",
					expectedExceptionMessage: "The Journal Entries Classification Group cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry.",
					() => registryDataType.ValidateBeforeRegistryFormSave(registryItem, validSample, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}
		}

		#region Implementation

		protected override JournalEntriesClassificationGroupRegistryDataType GetNewDataType() => new JournalEntriesClassificationGroupRegistryDataType();

		protected override string ExpectedEditorName => "JournalEntriesClassificationGroupRegistryItemEditor";

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			var journalEntriesClassificationGroupCode2 = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode2.Code = "TS2";
			journalEntriesClassificationGroupCode2.Description = "Description2";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.Ledger = LedgerTypes.AccountsReceivable;
			journalEntriesClassificationGroup.TransactionType = TransactionTypes.Invoice;
			journalEntriesClassificationGroup.GroupCode = "TST";

			var journalEntriesClassificationGroupCollection2 = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup2 = journalEntriesClassificationGroupCollection2.AddNew();
			journalEntriesClassificationGroup2.Ledger = LedgerTypes.AccountsPayable;
			journalEntriesClassificationGroup2.TransactionType = TransactionTypes.CreditNote;
			journalEntriesClassificationGroup2.GroupCode = "TS2";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(journalEntriesClassificationGroupCollection, new JournalEntriesClassificationGroupRegistryDataType().Serialise(journalEntriesClassificationGroupCollection)),
				new ValidSampleAndBinaryValueInDB(journalEntriesClassificationGroupCollection2, new JournalEntriesClassificationGroupRegistryDataType().Serialise(journalEntriesClassificationGroupCollection2))
			};
		}

		#endregion
	}
}
