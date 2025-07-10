using System;
using System.Drawing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem))]
	sealed class HouseBillOfLadingTermsAndConditionsCollectionRegistryItemTest
			: RegistryImageCollectionRegistryItemTest<HouseBillOfLadingTermsAndConditionsCollectionRegistryItem, HouseBillOfLadingTermsAndConditionsCollection>
	{
		#region Default Value

		public void TestDefaultValue()
		{
			HouseBillOfLadingTermsAndConditionsCollectionRegistryItem registryItem = new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem("DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem", null, null, null, RegistryStorageFlags.All);
			AssertDefaultValueProperties(registryItem.DefaultValue);
		}

		public void TestValueIsDefaultValueIfRegistryItemHasNoSavedValues()
		{
			HouseBillOfLadingTermsAndConditionsCollectionRegistryItem registryItem = new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem("DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem", null, null, null, RegistryStorageFlags.All);
			AssertDefaultValueProperties(registryItem.Value);
		}

		void AssertDefaultValueProperties(HouseBillOfLadingTermsAndConditionsCollection defaultValue)
		{
			AssertEquals("DefaultValue.Count", 8, defaultValue.Count);

			AssertEquals("DefaultValue[0].Code", "IAU", defaultValue[0].Code);
			AssertEquals("DefaultValue[1].Code", "FIA", defaultValue[1].Code);
			AssertEquals("DefaultValue[2].Code", "TTC", defaultValue[2].Code);
			AssertEquals("DefaultValue[3].Code", "INZ", defaultValue[3].Code);
			AssertEquals("DefaultValue[4].Code", "FWB", defaultValue[4].Code);
			AssertEquals("DefaultValue[5].Code", "SWB", defaultValue[5].Code);
			AssertEquals("DefaultValue[6].Code", "TUS", defaultValue[6].Code);
			AssertEquals("DefaultValue[7].Code", "CPT", defaultValue[7].Code);

			AssertEquals("DefaultValue[0].Description", "IT Club Bill of Lading Terms and Conditions.", defaultValue[0].Description);
			AssertEquals("DefaultValue[1].Description", "FIATA Bill of Lading Terms and Conditions.", defaultValue[1].Description);
			AssertEquals("DefaultValue[2].Description", "TT Club Bill of Lading Terms and Conditions.", defaultValue[2].Description);
			AssertEquals("DefaultValue[3].Description", "IT Club NZ Bill of Lading Terms and Conditions.", defaultValue[3].Description);
			AssertEquals("DefaultValue[4].Description", "FIATA Waybill Terms and Conditions.", defaultValue[4].Description);
			AssertEquals("DefaultValue[5].Description", "Sea Waybill Terms and Conditions.", defaultValue[5].Description);
			AssertEquals("DefaultValue[6].Description", "TT Club United States Terms and Conditions.", defaultValue[6].Description);
			AssertEquals("DefaultValue[7].Description", "Carta De Porte Terms and Conditions.", defaultValue[7].Description);

			AssertEquals("DefaultValue[0].DeliveryMode", "ALL", defaultValue[0].DeliveryMode);
			AssertEquals("DefaultValue[1].DeliveryMode", "ALL", defaultValue[1].DeliveryMode);
			AssertEquals("DefaultValue[2].DeliveryMode", "ALL", defaultValue[2].DeliveryMode);
			AssertEquals("DefaultValue[3].DeliveryMode", "ALL", defaultValue[3].DeliveryMode);
			AssertEquals("DefaultValue[4].DeliveryMode", "ALL", defaultValue[4].DeliveryMode);
			AssertEquals("DefaultValue[5].DeliveryMode", "ALL", defaultValue[5].DeliveryMode);
			AssertEquals("DefaultValue[6].DeliveryMode", "ALL", defaultValue[6].DeliveryMode);
			AssertEquals("DefaultValue[7].DeliveryMode", "ALL", defaultValue[7].DeliveryMode);

			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-ITC_Aus_Backv22.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-FIATA_Intl_A4_Back.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_Intl_A4_131anz_TC.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_ITC_NZ_TCv1.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_FWB_Terms.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_SWB_Terms.gif"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_US_Ltr_Terms.png"));
			AssertNotNull("Resource should not be null.", typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_CPT_Terms.gif"));

			AssertEquals("DefaultValue[0].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-ITC_Aus_Backv22.gif", defaultValue[0].DefaultImageResourceName);
			AssertEquals("DefaultValue[1].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-FIATA_Intl_A4_Back.gif", defaultValue[1].DefaultImageResourceName);
			AssertEquals("DefaultValue[2].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_Intl_A4_131anz_TC.gif", defaultValue[2].DefaultImageResourceName);
			AssertEquals("DefaultValue[3].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_ITC_NZ_TCv1.gif", defaultValue[3].DefaultImageResourceName);
			AssertEquals("DefaultValue[4].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_FWB_Terms.gif", defaultValue[4].DefaultImageResourceName);
			AssertEquals("DefaultValue[5].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_SWB_Terms.gif", defaultValue[5].DefaultImageResourceName);
			AssertEquals("DefaultValue[6].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_US_Ltr_Terms.png", defaultValue[6].DefaultImageResourceName);
			AssertEquals("DefaultValue[7].DefaultImageResourceName", "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_CPT_Terms.gif", defaultValue[7].DefaultImageResourceName);
		}

		#endregion

		public override void TestLatestImagesCodeDescList()
		{
			FallbackLevel fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			HouseBillOfLadingTermsAndConditions image1 = Collection.AddNew();
			HouseBillOfLadingTermsAndConditions image2 = Collection.AddNew();

			image1.Code = "cde";
			image1.Description = (NoResString)"dsc1";
			image1.DeliveryMode = nameof(PrintCopyType.ALL);
			image1.Image = new Bitmap(10, 10);

			image2.Code = "cde";
			image2.Description = (NoResString)"dsc2";
			image2.DeliveryMode = nameof(PrintCopyType.EML);
			image2.Image = new Bitmap(10, 10);

			Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);

			AssertEquals("LatestImagesCodeDescList should be empty if Fallback is null.", 0, Item.LatestImagesCodeDescList(null).Count);

			CodeDescriptionPairList codeList = Item.LatestImagesCodeDescList(fallback);
			AssertEquals("CodeList Count", 1, codeList.Count);
			AssertEquals("Description", "dsc1", codeList.GetDescriptionFromCode("cde"));

			HouseBillOfLadingTermsAndConditions image3 = Collection.AddNew();
			HouseBillOfLadingTermsAndConditions image4 = Collection.AddNew();

			image3.Code = "abc";
			image3.Description = (NoResString)"dsc3";
			image3.DeliveryMode = nameof(PrintCopyType.ALL);
			image3.Image = new Bitmap(20, 20);

			image4.Code = "abc";
			image4.Description = (NoResString)"dsc4";
			image4.DeliveryMode = nameof(PrintCopyType.EML);
			image4.Image = new Bitmap(20, 20);

			((IRegistryItemInternals)Item).SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);
			((IRegistryItemInternals)Item).SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

			codeList = Item.LatestImagesCodeDescList(fallback);
			AssertEquals("CodeList Count", 2, codeList.Count);
			AssertEquals("Description", "dsc1", codeList.GetDescriptionFromCode("cde"));
			AssertEquals("Description", "dsc3", codeList.GetDescriptionFromCode("abc"));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<HouseBillOfLadingTermsAndConditionsCollection, HouseBillOfLadingTermsAndConditionsCollection> GetNewRegistryItem()
		{
			return new DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem("DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem", null, null, null, RegistryStorageFlags.All);
		}

		protected override HouseBillOfLadingTermsAndConditionsCollection GetNewCollection()
		{
			return new HouseBillOfLadingTermsAndConditionsCollection();
		}

		protected override RegistryImage AddNew(string code, MultilingualString description, Image image)
		{
			HouseBillOfLadingTermsAndConditions result = (HouseBillOfLadingTermsAndConditions)base.AddNew(code, description, image);
			result.DeliveryMode = "ALL";
			return result;
		}

		#region class DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem

		class DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem : HouseBillOfLadingTermsAndConditionsCollectionRegistryItem
		{
			public DummyHouseBillOfLadingTermsAndConditionsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, storage, true)
			{
			}
		}

		#endregion

		#endregion
	}
}
