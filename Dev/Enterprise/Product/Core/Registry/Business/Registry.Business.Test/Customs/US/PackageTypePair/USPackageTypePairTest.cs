using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(USPackageTypePair))]
	sealed class USPackageTypePairTest : RegistryBusinessObjectTemplateTestCase<USPackageTypePair>
	{
		[ExpectNoExceptions]
		public void TestCustomsPackageTypesList()
		{
			USPackageTypePair packageType = new USPackageTypePair();
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IShippingOrPackingingUnitList>(), packageType.CustomsPackageTypesList.GetType());
		}

		protected override USPackageTypePair GetBusinessObjectToClone()
		{
			USPackageTypePair packageType = new USPackageTypePair();
			packageType.CustomsPackageType = packageType.CustomsPackageTypesList[0].Code;

			var type = ObjectFactory.GetType<IRefPackTypeCollection>();
			var packTypeCollection = (IRefPackTypeCollection)Activator.CreateInstance(type, RegistryFactory.Instance);
			var codeDescriptionPair = (CodeDescriptionPair)packTypeCollection.GetAsCodeDescriptionPair()[0];
			packageType.FreightPackageType = codeDescriptionPair.Code;

			packageType.CustomsPackageType = packageType.CustomsPackageTypesList[0].Code;
			return packageType;
		}

		protected override USPackageTypePair GetBusinessObjectToSerialise()
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
