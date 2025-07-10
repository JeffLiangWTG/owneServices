using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LandedCostingPreferencesRegistryDataType))]
	sealed class LandedCostingPreferencesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LandedCostingPreferencesRegistryDataType>
	{
		protected override LandedCostingPreferencesRegistryDataType GetNewDataType()
		{
			return new LandedCostingPreferencesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "LandedCostingPreferencesRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			LandedCostingGroup landedCostingGroup1 = (LandedCostingGroup)lhs;
			LandedCostingGroup landedCostingGroup2 = (LandedCostingGroup)rhs;

			AssertEquals("Rhs.Charges.Count", landedCostingGroup1.Charges.Count, landedCostingGroup2.Charges.Count);

			for (int i = 0; i < landedCostingGroup1.Charges.Count; ++i)
			{
				foreach (ZPropertyInfo propertyInfo in landedCostingGroup1.Charges[i].ZPropertyInfoHash)
				{
					AssertEquals(landedCostingGroup1.Charges[i][propertyInfo.Name], landedCostingGroup2.Charges[i][propertyInfo.Name]);
				}
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();

			LandedCostingGroup landedCostingGroup = collection.AddNew();

			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "Group 1";
			landedCostingGroup.CostDistributionList.AddPair("ABC", "");
			landedCostingGroup.CostDistributionCode = "ABC";

			ChargeGroupAndChargeCode charge = landedCostingGroup.Charges.AddNew();

			charge.ChargeGroupList.AddPair("XYZ", "");
			charge.ChargeGroupCode = "XYZ";
			charge.ChargeCodeCodeForDefaultValue = "ABC";
			charge.IsExcluded = false;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,
				101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,
				114,0,114,0,97,0,121,0,79,0,102,0,76,0,97,0,110,0,100,0,101,0,100,0,67,0,111,0,115,0,116,0,105,0,110,0,103,0,71,0,114,
				0,111,0,117,0,112,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,
				47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,
				99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,
				0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,
				0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,76,0,97,0,
				110,0,100,0,101,0,100,0,67,0,111,0,115,0,116,0,105,0,110,0,103,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,71,0,114,0,
				111,0,117,0,112,0,73,0,68,0,62,0,49,0,60,0,47,0,71,0,114,0,111,0,117,0,112,0,73,0,68,0,62,0,60,0,71,0,114,0,111,0,117,
				0,112,0,78,0,97,0,109,0,101,0,62,0,71,0,114,0,111,0,117,0,112,0,32,0,49,0,60,0,47,0,71,0,114,0,111,0,117,0,112,0,78,0,
				97,0,109,0,101,0,62,0,60,0,67,0,111,0,115,0,116,0,68,0,105,0,115,0,116,0,114,0,105,0,98,0,117,0,116,0,105,0,111,0,110,
				0,67,0,111,0,100,0,101,0,62,0,65,0,66,0,67,0,60,0,47,0,67,0,111,0,115,0,116,0,68,0,105,0,115,0,116,0,114,0,105,0,98,0,
				117,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,62,0,60,0,67,0,
				104,0,97,0,114,0,103,0,101,0,71,0,114,0,111,0,117,0,112,0,65,0,110,0,100,0,67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,
				0,100,0,101,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,71,0,114,0,111,0,117,0,112,0,67,0,111,0,100,0,101,0,62,0,88,
				0,89,0,90,0,60,0,47,0,67,0,104,0,97,0,114,0,103,0,101,0,71,0,114,0,111,0,117,0,112,0,67,0,111,0,100,0,101,0,62,0,60,0,
				67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,45,0,
				48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,
				0,48,0,48,0,48,0,60,0,47,0,67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,60,0,73,0,115,0,69,
				0,120,0,99,0,108,0,117,0,100,0,101,0,100,0,62,0,78,0,60,0,47,0,73,0,115,0,69,0,120,0,99,0,108,0,117,0,100,0,101,0,100,
				0,62,0,60,0,47,0,67,0,104,0,97,0,114,0,103,0,101,0,71,0,114,0,111,0,117,0,112,0,65,0,110,0,100,0,67,0,104,0,97,0,114,
				0,103,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,62,0,60,0,47,0,76,0,97,0,
				110,0,100,0,101,0,100,0,67,0,111,0,115,0,116,0,105,0,110,0,103,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,47,0,65,0,114,
				0,114,0,97,0,121,0,79,0,102,0,76,0,97,0,110,0,100,0,101,0,100,0,67,0,111,0,115,0,116,0,105,0,110,0,103,0,71,0,114,0,
				111,0,117,0,112,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}
	}
}
