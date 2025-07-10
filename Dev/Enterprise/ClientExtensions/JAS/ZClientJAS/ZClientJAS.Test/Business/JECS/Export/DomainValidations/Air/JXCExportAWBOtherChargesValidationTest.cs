using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCExportAWBOtherChargesValidationTest : JXCValidationTestCase
	{
		public void TestDomainValidationShouldSubclassFromAutoValidationType()
		{
			AssertEquals(typeof(Freight.Forwarding.AWB.Business.AutoExportAWBOtherChargesValidation), typeof(JXCExportAWBOtherChargesValidation).BaseType);
		}

		public void TestValidateEO_ChargeCode()
		{
			AssertHasNoJXCWarnings("Pre-condition", AWBOtherCharges.EO_ChargeCodeInfo);
			AWBOtherCharges.EO_ChargeCode = "**";
			AssertHasInvalidCodeJXCWarning(AWBOtherCharges.EO_ChargeCodeInfo);
			AWBOtherCharges.EO_ChargeCode = "";
			AssertHasNotEnteredJXCWarning(AWBOtherCharges.EO_ChargeCodeInfo);
			AWBOtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			AssertHasNoJXCWarnings(AWBOtherCharges.EO_ChargeCodeInfo);
		}

		public void TestValidateEO_EntitlementCode()
		{
			AssertHasNoJXCWarnings("Pre-condition", AWBOtherCharges.EO_EntitlementCodeInfo);
			AWBOtherCharges.EO_EntitlementCode = "|";
			AssertHasInvalidCodeJXCWarning(AWBOtherCharges.EO_EntitlementCodeInfo);
			AWBOtherCharges.EO_EntitlementCode = "";
			AssertHasNotEnteredJXCWarning(AWBOtherCharges.EO_EntitlementCodeInfo);
			AWBOtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertHasNoJXCWarnings(AWBOtherCharges.EO_EntitlementCodeInfo);
		}

		public void TestShouldNotValidateEO_EntitlementCodeForShipmentAWBOtherCharges()
		{
			ShipmentExportAWBOtherCharges shipmentOtherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			shipmentOtherCharges.EO_EntitlementCode = "|";
			AssertHasNoJXCWarnings(shipmentOtherCharges.EO_EntitlementCodeInfo);
			shipmentOtherCharges.EO_EntitlementCode = "";
			AssertHasNoJXCWarnings(shipmentOtherCharges.EO_EntitlementCodeInfo);
		}

		public void TestValidateEO_ChargeDescription()
		{
			AssertHasNoJXCWarnings("Pre-condition", AWBOtherCharges.EO_ChargeDescriptionInfo);
			AWBOtherCharges.EO_ChargeDescription = "asdfkjasdf";
			AssertHasNoJXCWarnings(AWBOtherCharges.EO_ChargeDescriptionInfo);
			AWBOtherCharges.EO_ChargeDescription = "";
			AssertHasNotEnteredJXCWarning(AWBOtherCharges.EO_ChargeDescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Air);
		}

		ExportAWBOtherCharges AWBOtherCharges
		{
			get
			{
				if (fAWBOtherCharges == null)
				{
					fAWBOtherCharges = Factory.New<ExportAWBOtherCharges>();
				}

				return fAWBOtherCharges;
			}
		}

		ExportAWBOtherCharges fAWBOtherCharges;
	}
}
