using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class HouseBillOfLadingTermsAndConditionsCollectionRegistryItem : RegistryImageCollectionRegistryItem<HouseBillOfLadingTermsAndConditionsCollection>
	{
		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, false)
		{
		}

		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, false)
		{
		}

		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool emptyDefaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, emptyDefaultValue)
		{
		}

		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool emptyDefaultValue)
			: base(new RegistryItemImplWithDefaultValue(name, category, caption, hint, new HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType(), storage, options, emptyDefaultValue))
		{
		}

		internal override void AddCodeAndDescriptionToList(CodeDescriptionPairList list, RegistryImage element)
		{
			if (((HouseBillOfLadingTermsAndConditions)element).IsDeliveryModeALL)
			{
				list.AddPair(element.Code, element.Description);
			}
		}

		protected override HouseBillOfLadingTermsAndConditionsCollection GetEmptyValue()
		{
			return new HouseBillOfLadingTermsAndConditionsCollection();
		}

		#region class RegistryItemImplWithDefaultValue

		class RegistryItemImplWithDefaultValue : RegistryImageCollectionRegistryItemImpl
		{
			public RegistryItemImplWithDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType dataType, RegistryStorageFlags storage, bool emptyDefaultValue)
				: this(name, category, caption, hint, dataType, storage, RegistryOptions.Default, emptyDefaultValue)
			{
			}

			public RegistryItemImplWithDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, bool emptyDefaultValue)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				this.emptyDefaultValue = emptyDefaultValue;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return (emptyDefaultValue) ? new HouseBillOfLadingTermsAndConditionsCollection() : GetHouseBillOfLadingTermsAndConditionsImagesDefaultValue();
			}

			HouseBillOfLadingTermsAndConditionsCollection GetHouseBillOfLadingTermsAndConditionsImagesDefaultValue()
			{
				HouseBillOfLadingTermsAndConditionsCollection result = new HouseBillOfLadingTermsAndConditionsCollection();

				HouseBillOfLadingTermsAndConditions termsAndConditions1 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions2 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions3 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions4 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions5 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions6 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions7 = result.AddNew();
				HouseBillOfLadingTermsAndConditions termsAndConditions8 = result.AddNew();

				termsAndConditions1.Code = "IAU";
				termsAndConditions2.Code = "FIA";
				termsAndConditions3.Code = "TTC";
				termsAndConditions4.Code = "INZ";
				termsAndConditions5.Code = HouseBillOfLadingTermsAndConditions.FWB;
				termsAndConditions6.Code = HouseBillOfLadingTermsAndConditions.SWB;
				termsAndConditions7.Code = "TUS";
				termsAndConditions8.Code = "CPT";

				termsAndConditions1.Description = ResString.GetMultilingualString("e297e09a-09d9-40ce-9f30-0f68b84f7e1d", "IT Club Bill of Lading Terms and Conditions.");
				termsAndConditions2.Description = ResString.GetMultilingualString("a07136aa-4a0c-440a-ae3f-26404e62b78b", "FIATA Bill of Lading Terms and Conditions.");
				termsAndConditions3.Description = ResString.GetMultilingualString("49dfed84-a27a-4b00-862c-97d3125f851b", "TT Club Bill of Lading Terms and Conditions.");
				termsAndConditions4.Description = ResString.GetMultilingualString("ff316e0a-792e-415a-a960-8c9ed23f3f2b", "IT Club NZ Bill of Lading Terms and Conditions.");
				termsAndConditions5.Description = ResString.GetMultilingualString("57c48f73-21d2-4a35-b60f-351bbc51fc82", "FIATA Waybill Terms and Conditions.");
				termsAndConditions6.Description = ResString.GetMultilingualString("f2aa4a51-f2bb-48d2-bbce-f473dee5ae6e", "Sea Waybill Terms and Conditions.");
				termsAndConditions7.Description = ResString.GetMultilingualString("f48e5f46-5ccf-4a16-a1d2-c355f97d5f31", "TT Club United States Terms and Conditions.");
				termsAndConditions8.Description = ResString.GetMultilingualString("20eb2725-0281-489c-a752-91e2b3912bd6", "Carta De Porte Terms and Conditions.");

				termsAndConditions1.DeliveryMode = "ALL";
				termsAndConditions2.DeliveryMode = "ALL";
				termsAndConditions3.DeliveryMode = "ALL";
				termsAndConditions4.DeliveryMode = "ALL";
				termsAndConditions5.DeliveryMode = "ALL";
				termsAndConditions6.DeliveryMode = "ALL";
				termsAndConditions7.DeliveryMode = "ALL";
				termsAndConditions8.DeliveryMode = "ALL";

				termsAndConditions1.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-ITC_Aus_Backv22.gif";
				termsAndConditions2.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-FIATA_Intl_A4_Back.gif";
				termsAndConditions3.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_Intl_A4_131anz_TC.gif";
				termsAndConditions4.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_ITC_NZ_TCv1.gif";
				termsAndConditions5.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_FWB_Terms.gif";
				termsAndConditions6.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_SWB_Terms.gif";
				termsAndConditions7.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-TTC_US_Ltr_Terms.png";
				termsAndConditions8.DefaultImageResourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL_CPT_Terms.gif";

				return result;
			}

			readonly bool emptyDefaultValue;
		}

		#endregion

		#region HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType

		[RegistryEditor("Enterprise.Registry.GUI.HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
		internal class HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<HouseBillOfLadingTermsAndConditionsCollection>
		{
			protected override bool ValuesAreEqualCore(HouseBillOfLadingTermsAndConditionsCollection a, HouseBillOfLadingTermsAndConditionsCollection b)
			{
				return a.ContainsSameElementsInAnyOrder(b);
			}
		}

		#endregion
	}
}
