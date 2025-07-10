using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeInvoiceTaxMessageOverride))]
	class ChargeCodeInvoiceTaxMessageOverrideTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateChargeCode()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.ChargeCodeInfo);

			BizObj.ChargeCode = ZGuid.Empty;
			AssertHasErrors(BizObj.ChargeCodeInfo);

			BizObj.ChargeCode = chargeCode.PK;
			AssertNoErrors(BizObj.ChargeCodeInfo);
		}

		public void TestValidateEnglishOverrideMessage()
		{
			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.EnglishOverrideMessageInfo);

			BizObj.EnglishOverrideMessage = "Normal length message";
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.EnglishOverrideMessageInfo);
		}

		public void TestEnglishOverrideMessageMaxLength()
		{
			AssertEquals(AccInvMsgSchema.A9_EnglishMsg.MaxLength, BizObj.EnglishOverrideMessageInfo.MaxLength);
		}

		public void TestLocalOverrideMessageMaxLength()
		{
			AssertEquals(AccInvMsgSchema.A9_LocalMsg.MaxLength, BizObj.LocalOverrideMessageInfo.MaxLength);
		}

		public void TestValidateLocalOverrideMessage()
		{
			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.LocalOverrideMessageInfo);

			BizObj.LocalOverrideMessage = "Normal length message";
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.LocalOverrideMessageInfo);
		}

		public void TestValidateTaxMessage()
		{
			AccInvMsg invMsg = Factory.NewWithValidTestData<AccInvMsg>();
			invMsg.A9_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			Factory.Save();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.TaxMessageInfo);

			BizObj.TaxMessage = ZGuid.Empty;
			AssertHasErrors(BizObj.TaxMessageInfo);

			BizObj.TaxMessage = invMsg.PK;
			AssertNoErrors(BizObj.TaxMessageInfo);
		}

		public void TestUniquenessValidation()
		{
			ChargeCodeInvoiceTaxMessageOverrideCollection collection = new ChargeCodeInvoiceTaxMessageOverrideCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			ChargeCodeInvoiceTaxMessageOverride override1 = collection.AddNew();
			ChargeCodeInvoiceTaxMessageOverride override2 = collection.AddNew();

			BusinessObject chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			BusinessObject chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			BusinessObject invMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			BusinessObject invMsg2 = Factory.NewWithValidTestData<AccInvMsg>();

			override1.ChargeCode = override2.ChargeCode = chargeCode1.PK;
			override1.TaxMessage = override2.TaxMessage = invMsg1.PK;
			AssertHasRowError(override1, ChargeCodeInvoiceTaxMessageOverride.DuplicateMessage);
			AssertHasRowError(override2, ChargeCodeInvoiceTaxMessageOverride.DuplicateMessage);

			override1.ChargeCode = chargeCode2.PK;
			AssertNoRowErrors(override1);
			AssertNoRowErrors(override2);

			override1.ChargeCode = chargeCode1.PK;
			override1.TaxMessage = invMsg2.PK;
			AssertNoRowErrors(override1);
			AssertNoRowErrors(override2);

			override1.TaxMessage = ZGuid.Empty;
			AssertNoRowErrors(override1);
			AssertNoRowErrors(override2);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new ChargeCodeInvoiceTaxMessageOverride(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ChargeCodeInvoiceTaxMessageOverride BizObj
		{
			get { return (ChargeCodeInvoiceTaxMessageOverride)base.BizObj; }
		}

		#endregion
	}
}
