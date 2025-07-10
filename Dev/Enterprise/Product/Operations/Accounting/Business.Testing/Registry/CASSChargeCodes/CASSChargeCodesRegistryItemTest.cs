using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSChargeCodesRegistryItem))]
	public class CASSChargeCodesRegistryItemTest : StronglyTypedRegistryItemTestCase<CASSChargeCodeCollection>
	{
		#region Implementation

		public void TestDefaultChargeCode()
		{
			if (!string.IsNullOrEmpty(Env.Registry.FreightChargeCode.ToString()))
			{
				AssertEquals("DefaultChargeCode", Env.Registry.FreightChargeCode, Item.DefaultValue[0].ChargeCodePK);
			}
			else
			{
				AssertEquals("DefaultChargeCode", 0, Item.DefaultValue.Count);
			}
		}

		public void TestDefaultValue()
		{
			Guid chargeCode1 = Guid.NewGuid();
			Env.Instance.Registry.FreightChargeCode = chargeCode1;
			var regValue = Item.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("DefaultValue: CASSType", "ALL", regValue[0].CASSType);
			AssertEquals("DefaultValue: CASSComponentCode", "ALL", regValue[0].CASSComponentCode);
			AssertEquals("DefaultValue: Charge Code", chargeCode1, regValue[0].ChargeCodePK);
		}

		public void TestStorage()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, Item.Storage);
		}

		protected Type ChargeCodeRegistryItemWrapperType
		{
			get { return typeof(CASSChargeCodesRegistryItem); }
		}

		protected override StronglyTypedRegistryItem<CASSChargeCodeCollection, CASSChargeCodeCollection> GetNewRegistryItem()
		{
			return new CASSChargeCodesRegistryItem("", null, null, null);
		}

		protected override FallbackLevel Fallback
		{
			get { return new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		#endregion
	}

	[TestedType(typeof(CASSChargeCodeListRegistryDataType))]
	public class CASSChargeCodeListRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CASSChargeCodeListRegistryDataType>
	{
		#region Implementation

		protected override CASSChargeCodeListRegistryDataType GetNewDataType()
		{
			return new CASSChargeCodeListRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CASSChargeCodesRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var copy = AccountingConfigurationRegistry.Instance.CASSChargeCodes.DefaultValue;
			var copy2 = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy, DataType.Serialise(copy)),
				new ValidSampleAndBinaryValueInDB(copy2, DataType.Serialise(copy2))
			};
		}

		public override void TestGetSetValidValues()
		{
			foreach (ValidSampleAndBinaryValueInDB validSample in GetValidSamples())
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, validSample.ValidSample as CASSChargeCodeCollection);
				object readValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertValuesEqual("New value set.", validSample.ValidSample, readValue);
			}
		}

		#endregion
	}
}
