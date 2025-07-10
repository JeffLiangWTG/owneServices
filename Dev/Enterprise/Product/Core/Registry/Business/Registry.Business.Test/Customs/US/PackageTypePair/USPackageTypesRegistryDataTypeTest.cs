using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(USPackageTypesRegistryDataType))]
	sealed class USPackageTypesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<USPackageTypesRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "USPackageTypePairsRegistryItemEditor"; }
		}

		protected override USPackageTypesRegistryDataType GetNewDataType()
		{
			return new USPackageTypesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			USPackageTypePairCollection collection = new USPackageTypePairCollection();
			var packageTypePair = collection.AddNew();
			var type = ObjectFactory.GetType<IRefPackTypeCollection>();
			var packTypeCollection = (IRefPackTypeCollection)Activator.CreateInstance(type, RegistryFactory.Instance);
			var codeDescriptionPair = (CodeDescriptionPair)packTypeCollection.GetAsCodeDescriptionPair()[0];
			packageTypePair.FreightPackageType = codeDescriptionPair.Code;

			packageTypePair.CustomsPackageType = packageTypePair.CustomsPackageTypesList[0].Code;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new USPackageTypesRegistryDataType().Serialise(collection))
			};
		}
	}
}
