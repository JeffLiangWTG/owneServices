using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LocationsChargesGroup))]
	sealed class LocationsChargesGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCharges()
		{
			ChargeCodeGroup charge = BizObj.Charges.AddNew();

			AssertEquals("Charges.ParentLocationsChargesGroup", BizObj, BizObj.Charges.ParentLocationsChargesGroup);
			AssertEquals("Charges.Factory", BizObj.Factory, BizObj.Charges.Factory);
			AssertEquals("Charges.CurrentFallbackLevel", BizObj.CurrentFallbackLevel, charge.CurrentFallbackLevel);
		}

		public void TestXmlSerializationChangeCompatability()
		{
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			byte[] data = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><LocationsChargesGroup><Location>USNYC</Location><Charges xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeCodeGroup><ChargeCodePK>22c470f2-5a46-49e6-8f69-c71a2f4971e1</ChargeCodePK></ChargeCodeGroup><ChargeCodeGroup><ChargeCodePK>3393a8c2-5253-4f3c-ae47-853143f976d2</ChargeCodePK></ChargeCodeGroup></Charges></LocationsChargesGroup>");
			LocationsChargesGroup deserialisedBusinessObject = (LocationsChargesGroup)dummyDataType.Deserialise(data);

			BizObj.Location = "USNYC";
			ChargeCodeGroup charge1 = BizObj.Charges.AddNew();
			ChargeCodeGroup charge2 = BizObj.Charges.AddNew();
			charge1.ChargeCodePK = new ZGuid("22c470f2-5a46-49e6-8f69-c71a2f4971e1");
			charge2.ChargeCodePK = new ZGuid("3393a8c2-5253-4f3c-ae47-853143f976d2");

			CheckAllPropertiesAreEqual(BizObj, deserialisedBusinessObject, false);
		}

		#region Validation Tests

		public void TestValidateLocation()
		{
			AssertNoErrors("Precondition: GroupName should not have errors.", BizObj.LocationInfo);

			BizObj.Location = "";
			AssertHasError(BizObj.LocationInfo, "Please enter a value.");

			BizObj.Location = "O0";
			AssertHasError(BizObj.LocationInfo, "Enter a valid selection.");

			BizObj.Location = "^___^";
			AssertHasError(BizObj.LocationInfo, "Enter a valid selection.");

			BizObj.Location = "US";
			AssertNoErrors(BizObj.LocationInfo);

			BizObj.Location = "USNYC";
			AssertNoErrors(BizObj.LocationInfo);

			AssertEquals(RefUNLOCOSchema.RL_Code.MaxLength, BizObj.LocationInfo.MaxLength);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Location = "";
			BizObj.ClearAllNotifications();

			AssertNoErrors("Precondition: BizObj should not have errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated Location.", BizObj.LocationInfo);
		}

		#endregion

		#region Implementation

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			LocationsChargesGroup originalGroup = (LocationsChargesGroup)originalBusinessObject;
			LocationsChargesGroup newGroup = (LocationsChargesGroup)newBusinessObject;

			AssertEquals("NewBusinessObject.Charges.Count", originalGroup.Charges.Count, newGroup.Charges.Count);
			AssertEquals("NewBusinessObject.Charges.ParentLocationsChargesGroup", newGroup, newGroup.Charges.ParentLocationsChargesGroup);

			if (!isClone)
			{
				AssertEquals("NewBusinessObject.CurrentFallbackLevel", originalGroup.CurrentFallbackLevel, newGroup.CurrentFallbackLevel);
			}

			for (int i = 0; i < originalGroup.Charges.Count; ++i)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(originalGroup.Charges[i], newGroup.Charges[i]);
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.Location = "USNYC";
			ChargeCodeGroup charge1 = BizObj.Charges.AddNew();
			ChargeCodeGroup charge2 = BizObj.Charges.AddNew();

			charge1.ChargeCodePK = ZGuid.NewZGuid();
			charge2.ChargeCodePK = ZGuid.NewZGuid();

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new LocationsChargesGroup BizObj
		{
			get { return (LocationsChargesGroup)base.BizObj; }
		}

		#endregion
	}
}
