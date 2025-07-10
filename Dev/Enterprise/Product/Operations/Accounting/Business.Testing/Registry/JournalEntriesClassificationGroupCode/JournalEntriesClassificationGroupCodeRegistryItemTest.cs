using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupCodeRegistryItem))]
	public class JournalEntriesClassificationGroupCodeRegistryItemTest : StronglyTypedRegistryItemTestCase<JournalEntriesClassificationGroupCodeCollection>
	{
		protected override StronglyTypedRegistryItem<JournalEntriesClassificationGroupCodeCollection, JournalEntriesClassificationGroupCodeCollection> GetNewRegistryItem()
		{
			return new JournalEntriesClassificationGroupCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}

	[TestedType(typeof(JournalEntriesClassificationGroupCodeRegistryDataType))]
	public class JournalEntriesClassificationGroupCodeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JournalEntriesClassificationGroupCodeRegistryDataType>
	{
		public void TestValidationException_WhenAllocationOptionIsGen()
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.SetJournalEntriesNumberAllocationOptionToGen(GlbCompany.CurrentCompany);

			var registryDataType = GetNewDataType();
			var validSamples = GetValidSamples();
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode;

			foreach (var validSampleAndBinaryValueInDb in validSamples)
			{
				var validSample = validSampleAndBinaryValueInDb.ValidSample as JournalEntriesClassificationGroupCodeCollection;
				AssertExceptionThrown<RegistryValidationException>(
					message: "Exception occurs when Allocation Option has already been GEN",
					expectedExceptionMessage: "The Journal Entries Classification Group Code cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry.",
					() => registryDataType.ValidateBeforeRegistryFormSave(registryItem, validSample, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}
		}

		#region Implementation

		protected override JournalEntriesClassificationGroupCodeRegistryDataType GetNewDataType() => new JournalEntriesClassificationGroupCodeRegistryDataType();

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		protected override string ExpectedEditorName => "JournalEntriesClassificationGroupCodeRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			var journalEntriesClassificationGroupCodeCollection2 = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode2 = journalEntriesClassificationGroupCodeCollection2.AddNew();
			journalEntriesClassificationGroupCode2.Code = "TS2";
			journalEntriesClassificationGroupCode2.Description = "Description2";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(journalEntriesClassificationGroupCodeCollection, new JournalEntriesClassificationGroupCodeRegistryDataType().Serialise(journalEntriesClassificationGroupCodeCollection)),
				new ValidSampleAndBinaryValueInDB(journalEntriesClassificationGroupCodeCollection2, new JournalEntriesClassificationGroupCodeRegistryDataType().Serialise(journalEntriesClassificationGroupCodeCollection2))
			};
		}

		#endregion
	}
}
