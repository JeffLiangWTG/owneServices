using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(LinkRegistryItem))]
	sealed class LinkRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new StringRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}

		public void TestPropertiesAndHasOptionNoButtonCaptionSpecified()
		{
			LinkRegistryItem item = new LinkRegistryItem((NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", ModuleIDs.AccChargeCode);
			AssertEquals("ButtonCaption", "Edit Caption", item.ButtonCaption);
			CheckLinkRegistryItemExceptForButtonCaption(item);
		}

		public void TestPropertiesAndHasOptionWithButtonCaptionSpecified()
		{
			LinkRegistryItem item = new LinkRegistryItem((NoResString)"Category", (NoResString)"Caption", (NoResString)"Button Caption", (NoResString)"Hint", ModuleIDs.AccChargeCode);
			AssertEquals("ButtonCaption", "Button Caption", item.ButtonCaption);
			CheckLinkRegistryItemExceptForButtonCaption(item);
		}

		void CheckLinkRegistryItemExceptForButtonCaption(LinkRegistryItem item)
		{
			AssertEquals("ModuleName", "Caption", item.ModuleName);
			AssertEquals("ModuleID", ModuleIDs.AccChargeCode, item.ModuleID);

			IRegistryItem registryitem = item;

			AssertEquals("Caption", "Caption", registryitem.Caption);
			AssertEquals("Category", "Category", registryitem.Category);
			AssertEquals("Categories.Length", 1, registryitem.Categories.Length);
			AssertEquals("Categories[0]", "Category", registryitem.Categories[0]);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), registryitem.CountryFilterPKs);
			AssertEquals("Hint", "Hint", registryitem.Hint);
			AssertEquals("Name", "LINK_REGISTRY_ITEM", registryitem.Name);
			AssertEquals("Storage", RegistryStorageFlags.System, registryitem.Storage);

			AssertEquals("HasOption()", false, registryitem.HasOption(RegistryOptions.IsReadOnly));
		}
	}
}
