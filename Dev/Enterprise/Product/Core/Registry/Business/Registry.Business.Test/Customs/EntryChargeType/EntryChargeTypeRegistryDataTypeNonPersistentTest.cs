using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.Customs.EntryChargeTypeSettingCollectionRegistryItem;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeRegistryDataType))]
	sealed class EntryChargeTypeRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EntryChargeTypeRegistryDataType>
	{
		protected override string ExpectedEditorName => "EntryChargeTypeRegistryItemEditor";

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			var lhsParent = (EntryChargeTypeSetting)lhs;
			var rhsParent = (EntryChargeTypeSetting)rhs;

			AssertEquals(lhsParent.ChargeCode_List.Count, rhsParent.ChargeCode_List.Count);
			AssertEquals(lhsParent.ChargeType, rhsParent.ChargeType);
			AssertEquals(lhsParent.AC_ChargeCode, rhsParent.AC_ChargeCode);
		}

		protected override EntryChargeTypeRegistryDataType GetNewDataType()
		{
			return new EntryChargeTypeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new EntryChargeTypeSettingCollection();
			EntryChargeTypeSetting chargeType1 = collection.AddNew();
			chargeType1.ChargeType = "CVD";
			chargeType1.AC_ChargeCode = AccChargeCode.PK;
			AssertEquals(collection, chargeType1.ParentCollection);

			var collection2 = new EntryChargeTypeSettingCollection();
			EntryChargeTypeSetting chargeType2 = collection2.AddNew();
			chargeType2.ChargeType = "ENF";
			chargeType2.AC_ChargeCode = AccChargeCode.PK;
			collection2.Add(chargeType2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new EntryChargeTypeRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new EntryChargeTypeRegistryDataType().Serialise(collection2))
			};
		}

		#region AccChargeCode
		BusinessObject AccChargeCode
		{
			get
			{
				if (fAccChargeCode == null)
				{
					var factory = new BusinessObjectFactory();
					System.Reflection.Assembly masterFilesAssembly = System.Reflection.Assembly.Load("Enterprise.MasterFiles.Business");
					Type accChargeCodeType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.AccChargeCode");
					fAccChargeCode = factory.New(accChargeCodeType);
				}
				return fAccChargeCode;
			}
		}
		BusinessObject fAccChargeCode;
		#endregion
	}
}
