using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BECustomsRegistry))]
class BECustomsRegistryTest : RegistryItemSetTestCaseWithFactory<BECustomsRegistry>
{
	public void TestIsForProductivityWise()
	{
		AssertEquals(false, ItemSet.IsForProductivityWise);
	}

	public void TestAllRegistryItemsHaveBECountryFilter()
	{
		CombineAssertions(() =>
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				AssertEquals(registryItem.Name + ".CountryFilterPK", true, registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Belgium));
			}
		});
	}

	public void TestSenderId()
	{
		TestGenericRegistryItem(ItemSet.SenderIDs,
			"BECustomsSenderIDs",
			RawDataRegistry.Categories.Customs_Belgium,
			"Customs Message Sender IDs",
			"A unique sender ID for the message header of declarations.",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
			AssertContainsExactElementsInAnyOrder(new MessageVersionRegistryCollection().DefaultCollection.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)), ItemSet.SenderIDs.DefaultValue.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)));
	}

	public void TestCustomsMessageRecipientId()
	{
		TestGenericRegistryItem(ItemSet.CustomsMessageVersion,
			"BECustomsMessageVersion",
			RawDataRegistry.Categories.Customs_Belgium,
			"Customs Message Recipient IDs",
			"Current version of customs messages the company is configured at customs to submit.",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
		AssertContainsExactElementsInAnyOrder(new MessageVersionRegistryCollection().DefaultCollection.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)), ItemSet.CustomsMessageVersion.DefaultValue.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)));
	}

	public void TestCustomsRegistry()
	{
		TestGenericRegistryItem(ItemSet.CustomsRegistry,
			"BECustomsRegistries",
			RawDataRegistry.Categories.Customs_Belgium,
			"Customs Registries",
			"Enter the procedures/declaration types with its starting number for the Customs Registry together with the 0 the starting date agreed with customs. One can select the declaration types H1, H2, H3, H4, H5, H6, H7, B1, B2, B3, B4, TD, TA and DA.",
			RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
			RegistryOptions.Default);
		AssertContainsExactElementsInAnyOrder(new CustomsRegistryCollection().DefaultCollection.Cast<CustomsRegistry>().Select(x => (x.Organization, x.StartingDate)), ItemSet.CustomsRegistry.DefaultValue.Cast<CustomsRegistry>().Select(x => (x.Organization, x.StartingDate)));
	}

	public void TestSendUserAndSecretInHeader()
	{
		TestRegistryItem(ItemSet.SendUserAndSecretInHeader,
			"SendUserAndSecretInHeader",
			RawDataRegistry.Categories.Customs_Belgium,
			"Send User And Secret In Header",
			"Send the user id & secret in the header of the interchange.",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}

	public void TestDetermineTestSystem()
	{
		TestRegistryItem(ItemSet.DetermineTestSystem,
			"DetermineTestSystem",
			RawDataRegistry.Categories.Customs_Belgium,
			"Determine Test System",
			"If test declarations must be sent to PREPROD (meaning internal test system of customs), tick Yes. If test declarations must be sent to the normal (external) TEST system of customs, then do not override and leave No ticked.",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}
}
