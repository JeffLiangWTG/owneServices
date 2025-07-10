using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DocumentOpenCloseTextRegistryItem))]
	public class DocumentOpenCloseTextRegistryItemTest : MultilingualStringRegistryItemTest
	{
		public virtual void TestConstructor()
		{
			ResourceString defaultText2 = ResString.GetMultilingualString("c71cc8e1-1b02-4e22-a77a-d8f01d7010e8", "Default-Text-2");
			ResourceString defaultText4 = ResString.GetMultilingualString("188b8b18-139e-4676-a515-d3dda7363672", "Default-Text-4");

			DocumentOpenCloseTextRegistryItem registryItem1 = new DocumentOpenCloseTextRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1");
			DocumentOpenCloseTextRegistryItem registryItem2 = new DocumentOpenCloseTextRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", defaultText2);
			DocumentOpenCloseTextRegistryItem registryItem3 = new DocumentOpenCloseTextRegistryItem("Name3", (NoResString)"Category3", (NoResString)"Caption3", (NoResString)"Hint3", RegistryStorageFlags.System);
			DocumentOpenCloseTextRegistryItem registryItem4 = new DocumentOpenCloseTextRegistryItem("Name4", (NoResString)"Category4", (NoResString)"Caption4", (NoResString)"Hint4", RegistryStorageFlags.Company, defaultText4);

			AssertRegistryItemProperties(registryItem1, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.All, string.Empty);
			AssertRegistryItemProperties(registryItem2, "Name2", "Category2", "Caption2", "Hint2", RegistryStorageFlags.All, defaultText2);
			AssertRegistryItemProperties(registryItem3, "Name3", "Category3", "Caption3", "Hint3", RegistryStorageFlags.System, string.Empty);
			AssertRegistryItemProperties(registryItem4, "Name4", "Category4", "Caption4", "Hint4", RegistryStorageFlags.Company, defaultText4);
		}

		protected void AssertRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem, string name, string category, string caption, string hint, RegistryStorageFlags storage, string defaultValue)
		{
			AssertEquals("Name", name, registryItem.Name);
			AssertEquals("Category", category, registryItem.Category);
			AssertEquals("Caption", caption, registryItem.Caption);
			AssertEquals("Hint", hint, registryItem.Hint);
			AssertEquals("Storage", storage, registryItem.Storage);
			AssertEquals("DefaultValue", defaultValue, registryItem.DefaultValue);
			AssertEquals("EditorInfo.EditorType", TextEditorType.Memo, ((TextRegistryEditorInfo)registryItem.EditorInfo).EditorType);
			AssertAdditionalRegistryItemProperties(registryItem);
		}

		protected virtual void AssertAdditionalRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem)
		{
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new DocumentOpenCloseTextRegistryItem("", null, null, null);
		}

		protected override string ValidValue
		{
			get { return "This is the text."; }
		}
	}
}
