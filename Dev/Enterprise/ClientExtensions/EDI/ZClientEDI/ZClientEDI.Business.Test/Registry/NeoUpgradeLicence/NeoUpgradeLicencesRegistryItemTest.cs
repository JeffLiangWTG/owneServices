using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(NeoUpgradeLicencesRegistryItem))]
	class NeoUpgradeLicencesRegistryItemTest : StronglyTypedRegistryItemTestCase<NeoUpgradeLicenceCollection, NeoUpgradeLicenceCollection>
	{
		public void TestDefaultValue()
		{
			var collection = new NeoUpgradeLicenceCollection();
			collection.AddNew();
			collection.AddNew();

			var item = new NeoUpgradeLicencesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, collection);
			AssertEquals(2, item.DefaultValue.Count);
		}

		protected override StronglyTypedRegistryItem<NeoUpgradeLicenceCollection, NeoUpgradeLicenceCollection> GetNewRegistryItem() => new NeoUpgradeLicencesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
	}

	[TestedType(typeof(NeoUpgradeLicencesRegistryDataType))]
	class NeoUpgradeLicencesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NeoUpgradeLicencesRegistryDataType>
	{
		#region Implementation

		protected override NeoUpgradeLicencesRegistryDataType GetNewDataType() => new NeoUpgradeLicencesRegistryDataType();

		protected override string ExpectedEditorName => "NeoUpgradeLicencesRegistryEditor";

		protected override bool HasEditor => true;

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			var lhsLicence = (NeoUpgradeLicence)lhs;
			var rhsLicence = (NeoUpgradeLicence)rhs;

			AssertEquals(lhsLicence.LicencePK, rhsLicence.LicencePK);
			AssertEquals(lhsLicence.EnterpriseCode, rhsLicence.EnterpriseCode);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			var licence1 = GetOrNew<LicenceEnterprise>(factory, new Guid("18F0B663-0F42-49B7-AC0E-DAC48079947B"), (l) => l.LE_EnterpriseCode = "LI1");
			var licence2 = GetOrNew<LicenceEnterprise>(factory, new Guid("D69F88F5-09BC-4D18-8977-83DEA35573F8"), (l) => l.LE_EnterpriseCode = "LI2");
			var licence3 = GetOrNew<LicenceEnterprise>(factory, new Guid("1EC5581E-E817-409F-8E7A-DB70D399E82C"), (l) => l.LE_EnterpriseCode = "LI3");
			var licence4 = GetOrNew<LicenceEnterprise>(factory, new Guid("4DA0949B-29B5-49FF-9AB7-700D0E10C136"), (l) => l.LE_EnterpriseCode = "LI4");

			var upgrade1 = new NeoUpgradeLicenceCollection();
			var upgrade1Licence1 = upgrade1.AddNew();
			upgrade1Licence1.LicencePK = licence1.PK;
			var upgrade1Licence2 = upgrade1.AddNew();
			upgrade1Licence2.LicencePK = licence2.PK;

			var upgrade2 = new NeoUpgradeLicenceCollection();
			var upgrade2Licence1 = upgrade2.AddNew();
			upgrade2Licence1.LicencePK = licence3.PK;
			var upgrade2Licence2 = upgrade2.AddNew();
			upgrade2Licence2.LicencePK = licence4.PK;

			var xmlUpgrade1 = $@"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfNeoUpgradeLicence xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><NeoUpgradeLicence><LicencePK>{licence1.PK}</LicencePK><EnterpriseCode>{licence1.LE_EnterpriseCode}</EnterpriseCode></NeoUpgradeLicence><NeoUpgradeLicence><LicencePK>{licence2.PK}</LicencePK><EnterpriseCode>{licence2.LE_EnterpriseCode}</EnterpriseCode></NeoUpgradeLicence></ArrayOfNeoUpgradeLicence>";
			var xmlUpgrade2 = $@"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfNeoUpgradeLicence xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><NeoUpgradeLicence><LicencePK>{licence3.PK}</LicencePK><EnterpriseCode>{licence3.LE_EnterpriseCode}</EnterpriseCode></NeoUpgradeLicence><NeoUpgradeLicence><LicencePK>{licence4.PK}</LicencePK><EnterpriseCode>{licence4.LE_EnterpriseCode}</EnterpriseCode></NeoUpgradeLicence></ArrayOfNeoUpgradeLicence>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(upgrade1, xmlUpgrade1),
				new ValidSampleAndBinaryValueInDB(upgrade2, xmlUpgrade2)
			};
		}

		#endregion
	}
}
