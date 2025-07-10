using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditCardFee))]
	public class CreditCardFeeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateChargeCodePK()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			BizObj.RunPreSaveValidation();
			AssertEquals("ChargeCodePK should have errors", true, BizObj.ChargeCodePKInfo.HasErrors());

			BizObj.ChargeCodePK = ZGuid.Empty;
			AssertEquals("ChargeCodePK should have errors", true, BizObj.ChargeCodePKInfo.HasErrors());

			BizObj.ChargeCodePK = chargeCode.PK;
			AssertEquals("ChargeCodePK should not have any errors", false, BizObj.ChargeCodePKInfo.HasErrors());
		}

		public void TestValidatePercentage()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("Percentage should have errors", true, BizObj.PercentageInfo.HasErrors());

			BizObj.Percentage = ZDecimal.Zero;
			AssertEquals("Percentage should have errors", true, BizObj.PercentageInfo.HasErrors());

			BizObj.Percentage = 10.10;
			AssertEquals("Percentage should not have any errors", false, BizObj.PercentageInfo.HasErrors());
		}

		[ExpectException(typeof(ZArchitecture.Environment.RegistryValidationException))]
		public void TestDuplicateRegistryEntry()
		{
			CreditCardFeeCollection collection = new CreditCardFeeCollection();
			ZGuid testGuid = ZGuid.NewZGuid();

			CreditCardFee creditCardFee = collection.AddNew();
			creditCardFee.ChargeCodePK = testGuid;
			creditCardFee.Percentage = 20.20;

			AccountingConfigurationRegistry.Instance.CreditCardFee.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			CreditCardFeeCollection value = AccountingConfigurationRegistry.Instance.CreditCardFee.Value;
			Assert("Validation should pass", !creditCardFee.HasErrors);

			CreditCardFee duplicateRegistry = collection.AddNew();
			duplicateRegistry.ChargeCodePK = testGuid;
			duplicateRegistry.Percentage = 20.20;

			AccountingConfigurationRegistry.Instance.CreditCardFee.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.ChargeCodePKInfo, CreditCardFee.Schema.ChargeCodePK);
			TestZPropertyInfo(BizObj.PercentageInfo, CreditCardFee.Schema.Percentage);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			BizObj.ChargeCodePK = chargeCode.PK;
			BizObj.Percentage = 10.10;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new CreditCardFee BizObj
		{
			get { return (CreditCardFee)base.BizObj; }
		}

		#endregion
	}
}
