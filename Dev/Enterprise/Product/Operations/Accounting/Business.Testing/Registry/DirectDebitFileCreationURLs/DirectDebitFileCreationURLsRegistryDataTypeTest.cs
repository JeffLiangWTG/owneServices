using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DirectDebitFileCreationURLsRegistryDataType))]
	class DirectDebitFileCreationURLsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DirectDebitFileCreationURLsRegistryDataType>
	{
		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs1, NonPersistentBusinessObject rhs1)
		{
			DirectDebitFileCreationURL lhs = lhs1 as DirectDebitFileCreationURL;
			DirectDebitFileCreationURL rhs = rhs1 as DirectDebitFileCreationURL;

			AssertEquals("BankAccountPK", lhs.BankAccountPK, rhs.BankAccountPK);
			AssertEquals("Bank 's website", lhs.BankWebsite, rhs.BankWebsite);
		}

		#region Implementation

		protected override DirectDebitFileCreationURLsRegistryDataType GetNewDataType()
		{
			return new DirectDebitFileCreationURLsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DirectDebitFileCreationURLsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var bankAccount1 = GetBankAccount(new Guid("61083EAF-5ADC-4124-8FD2-2DC00819C0AE"));
			var bankAccount2 = GetBankAccount(new Guid("8BAD59A0-F8BD-4569-B2AA-054F05F41458"));

			Factory.Save();

			var settings = new DirectDebitFileCreationURLCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var setting1 = settings.AddNew();
			setting1.BankWebsite = "http://xxxxx";
			setting1.BankAccountPK = bankAccount1.PK;

			var setting2 = settings.AddNew();
			setting2.BankWebsite = "http://yyyy";
			setting2.BankAccountPK = bankAccount2.PK;

			return new[] { new ValidSampleAndBinaryValueInDB(settings, DataType.Serialise(settings)) };
		}

		#endregion

		BusinessObject GetBankAccount(Guid pk)
		{
			return GetOrNew<AccBankAccount>(Factory, pk, (bankAccount) =>
			{
				bankAccount.AB_GC = Env.CurrentCompany.PK;
				bankAccount.AB_AutoDDRFormat = "NAB";
				bankAccount.AB_AllowAutoDDR = true;
				bankAccount.AB_IsActive = true;
			});
		}

		#region Factory

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get { return (factory) ?? (factory = new BusinessObjectFactory()); }
		}

		#endregion
	}
}
