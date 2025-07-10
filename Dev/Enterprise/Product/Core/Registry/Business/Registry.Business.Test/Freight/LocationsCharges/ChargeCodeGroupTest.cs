using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeGroup))]
	sealed class ChargeCodeGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestChargePKAndDescription()
		{
			BusinessObject chargeCode = AddAndReturnNewAccChargeCode("A New Description", "", ZGuid.Empty);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertEquals("ChargeCodeDescription", "A New Description", Charge.ChargeCodeDescription);
		}

		public void TestChargeCodeList()
		{
			AssertEquals("Enterprise.MasterFiles.Business.AccChargeCodeCollection", Charge.ChargeCodeList.GetType().FullName);
			AssertNotNull("ChargeCodeList.Factory should not be null.", Charge.ChargeCodeList.Factory);
		}

		#region Validation Tests

		public void TestValidateChargeCodePK_ValueIsInList()
		{
			AssertNoErrors("Precondition: ChargeCodePK should not have errors.", Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertHasError(Charge.ChargeCodePKInfo, "Enter a valid selection.");

			BusinessObject chargeCode = AddAndReturnNewAccChargeCode("", "", ZGuid.Empty);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.Empty;
			AssertHasError(Charge.ChargeCodePKInfo, "Please enter a value.");
		}

		public void TestRunPreSaveValidation()
		{
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Charge.ChargeCodePK = ZGuid.Invalid;

			Charge.ClearAllNotifications();

			AssertNoErrors("Precondition: Charge should not have errors.", Charge);

			Charge.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated ChargeCodePK.", Charge.ChargeCodePKInfo);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ParentLocationsChargesCollection = new LocationsChargesCollection();
			LocationsChargesGroup locationsChargesGroup = ParentLocationsChargesCollection.AddNew();
			Charge = locationsChargesGroup.Charges.AddNew();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			Charge.ChargeCodePK = ZGuid.NewZGuid();
			return Charge;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		BusinessObject AddAndReturnNewAccChargeCode(string aC_Desc, string aC_ChargeGroup, ZGuid aC_GC)
		{
			BusinessObject result = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));

			if (!string.IsNullOrEmpty(aC_Desc))
			{
				result[AccChargeCodeSchema.Constants.AC_Desc] = new ZString(aC_Desc);
			}

			if (!string.IsNullOrEmpty(aC_ChargeGroup))
			{
				result[AccChargeCodeSchema.Constants.AC_ChargeGroup] = new ZString(aC_ChargeGroup);
			}

			if (aC_GC != ZGuid.Empty)
			{
				result[AccChargeCodeSchema.Constants.AC_GC] = aC_GC;
			}

			Factory.Save();

			return result;
		}

		LocationsChargesCollection ParentLocationsChargesCollection;
		ChargeCodeGroup Charge;

		#endregion
	}
}
