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
	[TestedType(typeof(PricingPageChargeCodeGroup))]
	sealed class PricingPageChargeCodeGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestChargePKAndDescription()
		{
			var chargeCode = AddAndReturnNewAccChargeCode("A New Description", "", ZGuid.Empty);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertEquals("ChargeCodeDescription", "A New Description", Charge.ChargeCodeDescription);
		}

		public void TestChargeCodeList()
		{
			AssertEquals("Enterprise.MasterFiles.Business.AccChargeCodeCollection", Charge.ChargeCodeList.GetType().FullName);
			AssertNotNull("ChargeCodeList.Factory should not be null.", Charge.ChargeCodeList.Factory);
		}

		#region Validation Tests

		public void TestValidateChargeCodePK_ValueIsInChargeCodeList()
		{
			AssertNoErrors("Precondition: ChargeCodePK should not have errors.", Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.Empty;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Charge.ChargeCodePK = ZGuid.Empty;
			AssertHasError(Charge.ChargeCodePKInfo, "Please enter a Charge Code.");

			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertHasError(Charge.ChargeCodePKInfo, "Enter a valid Charge Code.");

			var chargeCode = AddAndReturnNewAccChargeCode("", "ORG", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);
		}

		public void TestValidateChargeCodePK_DuplicatedChargeCode()
		{
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Configuration.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			Configuration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;

			var chargeCode = AddAndReturnNewAccChargeCode("", "ORG", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			var duplicatedCharge = Configuration.Charges.AddNew();
			duplicatedCharge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			duplicatedCharge.ChargeCodePK = chargeCode.PK;

			AssertHasError(duplicatedCharge.ChargeCodePKInfo, "There must be only one the same charge code in the list.");
		}

		public void TestValidateChargeCodePK_OriginPickupChargesSection()
		{
			Configuration.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Configuration.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			Configuration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;

			var chargeCode = AddAndReturnNewAccChargeCode("", "OBR", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "OBO", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "CDS", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "ORG", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "LOD", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "DST", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertHasError(Charge.ChargeCodePKInfo, "Only Charge Codes under Charge Group of OBR,OBO,CDS,ORG,LOD can be selected for Origin Pickup Charges.");
		}

		public void TestValidateChargeCodePK_DestinationDeliveryChargesSection()
		{
			Configuration.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Configuration.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			Configuration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;

			var chargeCode = AddAndReturnNewAccChargeCode("", "DST", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "UNL", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "BRK", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "BON", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "CDS", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			chargeCode = AddAndReturnNewAccChargeCode("", "ORG", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertHasError(Charge.ChargeCodePKInfo, "Only Charge Codes under Charge Group of DST,UNL,BRK,BON,CDS can be selected for Destination Delivery Charges.");
		}

		public void TestValidateChargeCodePK_SectionChanged()
		{
			Configuration.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Configuration.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			Configuration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;

			var chargeCode = AddAndReturnNewAccChargeCode("", "DST", Env.CurrentCompany.PK);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Configuration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			AssertHasError(Charge.ChargeCodePKInfo, "Only Charge Codes under Charge Group of OBR,OBO,CDS,ORG,LOD can be selected for Origin Pickup Charges.");
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

			ConfigurationCollection = new ChargeCodeForPricingPageSectionsConfigurationCollection();
			Configuration = ConfigurationCollection.AddNew();
			Charge = Configuration.Charges.AddNew();
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
			var result = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));

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

			result[AccChargeCodeSchema.Constants.AC_RateCalculator] = "FLT";

			Factory.Save();

			return result;
		}

		ChargeCodeForPricingPageSectionsConfigurationCollection ConfigurationCollection;
		ChargeCodeForPricingPageSectionsConfiguration Configuration;
		PricingPageChargeCodeGroup Charge;

		#endregion
	}
}
