using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsConfiguration))]
	sealed class ChargeCodeForPricingPageSectionsConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCharges()
		{
			var charge = BizObj.Charges.AddNew();

			AssertEquals("Charges.ParentChargeCodeForPricingPageSectionsConfiguration", BizObj, BizObj.Charges.ParentConfiguration);
			AssertEquals("Charges.Factory", BizObj.Factory, BizObj.Charges.Factory);
			AssertEquals("Charges.CurrentFallbackLevel", BizObj.CurrentFallbackLevel, charge.CurrentFallbackLevel);
		}

		public void TestPricingPageList()
		{
			(string code, string description)[] values =
			{
				("FCO", "Forwarding Concise"),
			};

			var configuration = (ChargeCodeForPricingPageSectionsConfiguration)GetNewBusinessObject();

			AssertEquals(values.Length, configuration.PricingPageList.Count);

			for (var i = 0; i < values.Length; ++i)
			{
				AssertEquals(values[i].code, configuration.PricingPageList[i].Code);
				AssertEquals(values[i].description, configuration.PricingPageList[i].Description);
			}
		}

		public void TestSectionList()
		{
			(string code, string description)[] values =
			{
				("OPC", "Origin Pickup Charges"),
				("DDC", "Destination Delivery Charges"),
			};

			var configuration = (ChargeCodeForPricingPageSectionsConfiguration)GetNewBusinessObject();

			AssertEquals(values.Length, configuration.SectionList.Count);

			for (var i = 0; i < values.Length; ++i)
			{
				AssertEquals(values[i].code, configuration.SectionList[i].Code);
				AssertEquals(values[i].description, configuration.SectionList[i].Description);
			}
		}

		public void TestXmlSerializationChangeCompatability()
		{
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			byte[] data = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ChargeCodeForPricingPageSectionsConfiguration><PricingPage>FCO</PricingPage><Section>OPC</Section><PricingPageChargeCodes><PricingPageChargeCodeGroup><ChargeCodePK>22c470f2-5a46-49e6-8f69-c71a2f4971e1</ChargeCodePK></PricingPageChargeCodeGroup><PricingPageChargeCodeGroup><ChargeCodePK>3393a8c2-5253-4f3c-ae47-853143f976d2</ChargeCodePK></PricingPageChargeCodeGroup></PricingPageChargeCodes></ChargeCodeForPricingPageSectionsConfiguration>");
			var deserialisedBusinessObject = (ChargeCodeForPricingPageSectionsConfiguration)dummyDataType.Deserialise(data);

			BizObj.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			BizObj.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			var charge1 = BizObj.Charges.AddNew();
			var charge2 = BizObj.Charges.AddNew();
			charge1.ChargeCodePK = new ZGuid("22c470f2-5a46-49e6-8f69-c71a2f4971e1");
			charge2.ChargeCodePK = new ZGuid("3393a8c2-5253-4f3c-ae47-853143f976d2");

			CheckAllPropertiesAreEqual(BizObj, deserialisedBusinessObject, false);

			data = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ChargeCodeForPricingPageSectionsConfiguration><PricingPage>FCO</PricingPage><Section>DDC</Section><PricingPageChargeCodes><PricingPageChargeCodeGroup><ChargeCodePK>22c470f2-5a46-49e6-8f69-c71a2f4971e1</ChargeCodePK></PricingPageChargeCodeGroup><PricingPageChargeCodeGroup><ChargeCodePK>3393a8c2-5253-4f3c-ae47-853143f976d2</ChargeCodePK></PricingPageChargeCodeGroup></PricingPageChargeCodes></ChargeCodeForPricingPageSectionsConfiguration>");
			deserialisedBusinessObject = (ChargeCodeForPricingPageSectionsConfiguration)dummyDataType.Deserialise(data);

			BizObj.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			BizObj.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;

			CheckAllPropertiesAreEqual(BizObj, deserialisedBusinessObject, false);
		}

		#region Validation Tests

		public void TestValidateLocation()
		{
			AssertNoErrors("Precondition: PricingPage should not have errors.", BizObj.PricingPageInfo);

			BizObj.PricingPage = "";
			AssertHasError(BizObj.PricingPageInfo, "Please enter a Pricing Page.");

			BizObj.PricingPage = "O0";
			AssertHasError(BizObj.PricingPageInfo, "Enter a valid Pricing Page.");

			BizObj.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			AssertNoErrors(BizObj.PricingPageInfo);

			AssertEquals(3, BizObj.PricingPageInfo.MaxLength);
		}

		public void TestValidateSection()
		{
			BizObj.Section = "";
			AssertHasError(BizObj.SectionInfo, "Please enter a Section.");

			BizObj.Section = "O0";
			AssertHasError(BizObj.SectionInfo, "Enter a valid Section.");

			BizObj.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			AssertNoErrors(BizObj.SectionInfo);

			BizObj.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;
			AssertNoErrors(BizObj.SectionInfo);

			AssertEquals(3, BizObj.SectionInfo.MaxLength);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: BizObj should not have errors.", BizObj);

			BizObj.PricingPage = "";
			BizObj.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated PricingPage.", BizObj.PricingPageInfo);

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: BizObj should not have errors.", BizObj);

			BizObj.Section = "";
			BizObj.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated Section.", BizObj.PricingPageInfo);
		}

		#endregion

		#region Implementation

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			var originalGroup = (ChargeCodeForPricingPageSectionsConfiguration)originalBusinessObject;
			var newGroup = (ChargeCodeForPricingPageSectionsConfiguration)newBusinessObject;

			AssertEquals("NewBusinessObject.Charges.Count", originalGroup.Charges.Count, newGroup.Charges.Count);
			AssertEquals("NewBusinessObject.Charges.ParentConfiguration", newGroup, newGroup.Charges.ParentConfiguration);

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
			BizObj.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			BizObj.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			var charge1 = BizObj.Charges.AddNew();
			var charge2 = BizObj.Charges.AddNew();

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

		new ChargeCodeForPricingPageSectionsConfiguration BizObj
		{
			get { return (ChargeCodeForPricingPageSectionsConfiguration)base.BizObj; }
		}

		#endregion
	}
}
