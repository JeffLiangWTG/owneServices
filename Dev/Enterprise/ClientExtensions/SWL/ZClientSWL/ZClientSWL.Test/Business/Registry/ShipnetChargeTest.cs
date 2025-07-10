using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetCharge))]
	public class ShipnetChargeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateChargePK()
		{
			ShipnetCharge charge = new ShipnetCharge(SetupBusinessObject, Factory);
			charge.ChargePK = ZGuid.Invalid;
			AssertEquals("HasErrors", true, charge.ChargePKInfo.HasErrors());
			charge.ChargePK = ZGuid.NewZGuid();
			AssertEquals("HasErrors", false, charge.ChargePKInfo.HasErrors());
			charge.ChargePK = ZGuid.Empty;
			AssertEquals("HasErrors", true, charge.ChargePKInfo.HasErrors());
		}

		public void TestCheckDuplicateChargeCodes()
		{
			ShipnetChargeCollection charges = new ShipnetChargeCollection(SetupBusinessObject, Factory);
			ShipnetCharge charge = charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			charge.RunPreSaveValidation();
			AssertEquals("Should not have error", false, charge.ChargePKInfo.HasErrors());
			ShipnetCharge charge2 = charges.AddNew();
			charge2.ChargePK = charge.ChargePK;
			charge.RunPreSaveValidation();
			AssertEquals("Should have error", true, charge.ChargePKInfo.HasErrors());
			AssertEquals("Should contain error message", true, charge.ChargePKInfo.GetErrors().Contains("Duplicate charge codes are entered."));
			charge2.ChargePK = ZGuid.NewZGuid();
			charge.RunPreSaveValidation();
			AssertEquals("Should not have error", false, charge.ChargePKInfo.HasErrors());
		}

		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShipnetCharge result = new ShipnetCharge(SetupBusinessObject, Factory);
			result.ChargePK = ZGuid.NewZGuid();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupBusinessObject = new ShipnetSetupBusinessObject(Factory);
			SetupBusinessObject.IsShipnetCarrier = true;
		}

		ShipnetSetupBusinessObject SetupBusinessObject;
		#endregion
	}
}
