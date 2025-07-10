using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(CustomsIncoTermOverrideCollectionRegistryItem))]
	public class CustomsIncoTermOverrideCollectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CustomsIncoTermOverrideCollection>
	{
		public void TestGetInternationalCode()
		{
			CustomsIncoTermOverrideCollectionRegistryItem registryItem = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride;
			CustomsIncoTermOverrideCollection collection = registryItem.Value;
			CustomsIncoTermOverride incoTerm = collection.AddNew();
			incoTerm.CustomsCode = "FBB";
			incoTerm.InternationalCode = Core.Constants.IncoTerms.FreeOnBoard;
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals(ZString.Empty, registryItem.GetInternationalCode(Core.Constants.IncoTerms.FreeOnBoard));
			AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, registryItem.GetInternationalCode("FBB"));
			AssertEquals(ZString.Empty, registryItem.GetInternationalCode(ZString.Empty));
		}

		protected override StronglyTypedRegistryItem<CustomsIncoTermOverrideCollection, CustomsIncoTermOverrideCollection> GetNewRegistryItem()
		{
			return new CustomsIncoTermOverrideCollectionRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
