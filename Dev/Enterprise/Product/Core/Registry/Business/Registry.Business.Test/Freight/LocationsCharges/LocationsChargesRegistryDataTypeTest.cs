using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LocationsChargesRegistryDataType))]
	sealed class LocationsChargesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LocationsChargesRegistryDataType>
	{
		protected override LocationsChargesRegistryDataType GetNewDataType()
		{
			return new LocationsChargesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "LocationsChargesRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			LocationsChargesGroup locationsChargesGroup1 = (LocationsChargesGroup)lhs;
			LocationsChargesGroup locationsChargesGroup2 = (LocationsChargesGroup)rhs;

			AssertEquals("Rhs.Charges.Count", locationsChargesGroup1.Charges.Count, locationsChargesGroup2.Charges.Count);

			for (int i = 0; i < locationsChargesGroup1.Charges.Count; ++i)
			{
				foreach (ZPropertyInfo propertyInfo in locationsChargesGroup1.Charges[i].ZPropertyInfoHash)
				{
					AssertEquals(locationsChargesGroup1.Charges[i][propertyInfo.Name], locationsChargesGroup2.Charges[i][propertyInfo.Name]);
				}
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			LocationsChargesCollection collection = new LocationsChargesCollection();
			LocationsChargesGroup locationsChargesGroup = collection.AddNew();

			locationsChargesGroup.Location = "USNYC";
			ChargeCodeGroup charge = locationsChargesGroup.Charges.AddNew();
			charge.ChargeCodePK = new ZGuid(new Guid("b774dbf3-a524-446b-b82b-393733fe5ba9"));

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,115,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,71,0,114,
				0,111,0,117,0,112,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,
				0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,
				0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,
				0,109,0,97,0,34,0,62,0,60,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,115,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,76,0,111,0,99,0,97,0,116,
				0,105,0,111,0,110,0,62,0,85,0,83,0,78,0,89,0,67,0,60,0,47,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,62,0,60,0,67,0,104,0,97,
				0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,98,0,55,0,55,0,52,0,100,0,98,
				0,102,0,51,0,45,0,97,0,53,0,50,0,52,0,45,0,52,0,52,0,54,0,98,0,45,0,98,0,56,0,50,0,98,0,45,0,51,0,57,0,51,0,55,0,51,0,51,0,102,0,101,0,53,0,98,0,97,0,57,0,60,0,47,0,67,
				0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,60,0,47,0,67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,47,0,67,
				0,104,0,97,0,114,0,103,0,101,0,115,0,62,0,60,0,47,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,115,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,47,
				0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,115,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,71,0,114,0,111,0,117,0,112,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}
	}
}
