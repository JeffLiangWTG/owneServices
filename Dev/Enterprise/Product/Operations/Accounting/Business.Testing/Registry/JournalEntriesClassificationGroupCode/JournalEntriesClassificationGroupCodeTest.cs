using System;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupCode))]
	public class JournalEntriesClassificationGroupCodeTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Validation

		public void TestValidateCode()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.RunPreSaveValidation();

			AssertHasErrors("Please enter a Code.", journalEntriesClassificationGroupCode.CodeInfo);

			journalEntriesClassificationGroupCode.Code = "TS";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertHasErrors("Group Code length should be 3.", journalEntriesClassificationGroupCode.CodeInfo);

			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroupCode.CodeInfo.HasErrors());

			var journalEntriesClassificationGroupCode2 = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode2.Code = "TST";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertHasErrors("Group Code should be unique.", journalEntriesClassificationGroupCode.CodeInfo);
		}

		public void TestValidateDescription()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.RunPreSaveValidation();

			AssertHasErrors("Please enter a Description.", journalEntriesClassificationGroupCode.DescriptionInfo);

			journalEntriesClassificationGroupCode.Description = "TST";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroupCode.DescriptionInfo.HasErrors());
		}

		public void TestPrefixValidation()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var demoCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			demoCode.Code = "TST";
			demoCode.Description = "TST Description";
			demoCode.Prefix = "UUU";

			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Description = "UUU Description";
			journalEntriesClassificationGroupCode.Code = "UUU";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();

			AssertEquals(false, journalEntriesClassificationGroupCode.PrefixInfo.HasErrors());

			journalEntriesClassificationGroupCode.Prefix = "TST";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroupCode.PrefixInfo.HasErrors());

			journalEntriesClassificationGroupCode.Prefix = "T";
			journalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroupCode.PrefixInfo.HasErrors());

			journalEntriesClassificationGroupCode.Prefix = "TUUY";
			AssertHasErrors("Prefix length should be less than 3.", journalEntriesClassificationGroupCode.PrefixInfo);

			journalEntriesClassificationGroupCode.Prefix = "UUU";
			AssertHasErrors("Prefix should be unique.", journalEntriesClassificationGroupCode.PrefixInfo);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new JournalEntriesClassificationGroupCode(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
