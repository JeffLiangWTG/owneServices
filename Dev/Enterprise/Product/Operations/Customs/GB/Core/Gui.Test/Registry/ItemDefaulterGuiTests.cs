using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(ItemDefaulterRegistryItemEditor))]
	public class ItemDefaulterRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ItemDefaulterRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ItemDefaulterControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ItemDefaulterControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ItemDefaulterSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "C601", "C601 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new ItemDefaulterSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var item1 = CreateNewItemDefaulterSetting_CpcToC601(Factory);
			var item2 = CreateNewItemDefaulterSetting_C601ToIpr(orgHeader);
			var item3 = CreateNewItemDefaulterSetting_CpcToSpoff(Factory);
			collection.Add(item1);
			collection.Add(item2);
			collection.Add(item3);
			return new object[] { collection };
		}

		// Copy-pasted from GB/Core/Registry.Test/Business/ItemDefaulterBusinessTests.cs to remove reference to Registry.Test
		static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToC601(BusinessObjectFactory factory)
		{
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = "C601";
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_C601ToIpr(OrgHeader iprClient)
		{
			iprClient.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber, "IP/1234/567/00");
			var item = new ItemDefaulterSetting(iprClient.Factory);
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			item.SourceValue = "C601";
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromImporter;
			item.TargetCode = OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber;
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToSpoff(BusinessObjectFactory factory)
		{
			var hmrcAddress = factory.Load<OrgAddress>(new ZGuid("3A6474C2-B99E-4338-9C9F-ACB95C715B06"));
			if (hmrcAddress == null)
			{
				Assert("Pre-Req failed - OrgAddress with PK C3F842EF-3BE5-448C-BED3-0017B232C624 was not found in test database.  There's nothing special about this address, it was picked at random, but it still needs to exist.", false);
			}
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupervisingOfficeBox44;
			item.TargetCode = "";
			item.TargetOrgAddress = hmrcAddress.PK;
			return item;
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			ItemDefaulterSettingCollection collection1 = (ItemDefaulterSettingCollection)setValue;
			ItemDefaulterSettingCollection collection2 = (ItemDefaulterSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(collection1[i].SourceType, collection2[i].SourceType);
				AssertEquals(collection1[i].SourceValue, collection2[i].SourceValue);
				AssertEquals(collection1[i].TargetCode, collection2[i].TargetCode);
				AssertEquals(collection1[i].TargetOrgAddress, collection2[i].TargetOrgAddress);
				AssertEquals(collection1[i].TargetType, collection2[i].TargetType);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}

	[TestedType(typeof(ItemDefaulterControl))]
	public class ItemDefaulterRegistryZUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ItemDefaulterSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ItemDefaulterControl)control).TaxCodesGrid.ReadOnly;
		}
	}

	[TestedType(typeof(ItemDefaulterSettingCollectionRegistryItem))]
	public class ItemDefaulterSettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<ItemDefaulterSettingCollection>
	{
		protected override StronglyTypedRegistryItem<ItemDefaulterSettingCollection, ItemDefaulterSettingCollection> GetNewRegistryItem()
		{
			return new ItemDefaulterSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}
}
