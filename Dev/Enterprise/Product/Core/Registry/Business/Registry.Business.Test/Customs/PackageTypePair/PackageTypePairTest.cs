using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(PackageTypePairForTest))]
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed public class PackageTypePairTest : RegistryBusinessObjectTemplateTestCase<PackageTypePairForTest>
	{
		public void TestCustomsPackageTypeFieldType()
		{
			var packageType = new PackageTypePairForTest();
			AssertEquals("CustomsPackageTypeFieldType", nameof(FieldType.TextDropEdit), packageType.CustomsPackageTypeFieldType);
		}

		public void TestFreightPackageTypesList()
		{
			var packageType = new PackageTypePairForTest();
			AssertEquals(true, packageType.FreightPackageTypesList.ContainsCode("CNT"));
		}

		public void TestValidateFreightPackageType()
		{
			PackageTypePairCollectionForTest collection = new PackageTypePairCollectionForTest();
			PackageTypePairForTest packageType = collection.AddNew();
			packageType.CustomsPackageType = PackageTypePairForTest.SampleCode;
			packageType.FreightPackageType = packageType.FreightPackageTypesList[0].Code;
			collection.Add(packageType);

			packageType = collection.AddNew();
			packageType.CustomsPackageType = PackageTypePairForTest.SampleCode;
			packageType.FreightPackageType = ZString.Empty;
			AssertHasErrorContaining(packageType.FreightPackageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packageType.FreightPackageTypeInfo, PackageTypePair.FreightPackageTypeShouldBeInList);

			packageType.FreightPackageType = "~";
			AssertNoErrorContaining(packageType.FreightPackageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packageType.FreightPackageTypeInfo, PackageTypePair.FreightPackageTypeShouldBeInList);

			packageType.FreightPackageType = packageType.FreightPackageTypesList[0].Code;
			AssertNoErrorContaining(packageType.FreightPackageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packageType.FreightPackageTypeInfo, PackageTypePair.FreightPackageTypeShouldBeInList);
			AssertHasErrorContaining(packageType.FreightPackageTypeInfo, string.Format(PackageTypePair.PackageTypeShouldBeUnique, packageType.FreightPackageType));
		}

		public void TestValidateCustomsPackageType()
		{
			PackageTypePairCollectionForTest collection = new PackageTypePairCollectionForTest();
			PackageTypePairForTest packageType = collection.AddNew();
			packageType.CustomsPackageType = PackageTypePairForTest.SampleCode;
			packageType.FreightPackageType = packageType.FreightPackageTypesList[0].Code;
			collection.Add(packageType);

			packageType = collection.AddNew();
			packageType.FreightPackageType = packageType.FreightPackageTypesList[0].Code;
			packageType.CustomsPackageType = ZString.Empty;
			AssertHasErrorContaining(packageType.CustomsPackageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packageType.CustomsPackageTypeInfo, packageType.CustomsPackageTypeShouldBeInList);

			packageType.CustomsPackageType = "~";
			AssertNoErrorContaining(packageType.CustomsPackageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packageType.CustomsPackageTypeInfo, packageType.CustomsPackageTypeShouldBeInList);
		}

		protected override PackageTypePairForTest GetBusinessObjectToClone()
		{
			PackageTypePairForTest packageType = new PackageTypePairForTest();
			packageType.CustomsPackageType = PackageTypePairForTest.SampleCode;
			packageType.FreightPackageType = packageType.FreightPackageTypesList[0].Code;
			return packageType;
		}

		protected override PackageTypePairForTest GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
