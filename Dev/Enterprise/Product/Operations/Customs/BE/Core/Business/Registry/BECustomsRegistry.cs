using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

public sealed class BECustomsRegistry : RegistryItemSet
{
	public static BECustomsRegistry Instance => instance ?? (instance = new BECustomsRegistry());

	[ThreadStatic]
	static BECustomsRegistry instance;

	BECustomsRegistry()
	{
	}

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = CountryFilterPKs.Belgium;
	}

	public override bool IsForProductivityWise => false;

	public MessageVersionRegistryItem CustomsMessageVersion => GetItem("BECustomsMessageVersion", () => new MessageVersionRegistryItem(
		"BECustomsMessageVersion",
		CustomsDataRegistry.Categories.Customs_Belgium,
		ResString.GetMultilingualString("7342EA6C-D89C-46CA-A6D7-97184ED9BD60", "Customs Message Recipient IDs"),
		ResString.GetMultilingualString("A0000F26-9264-4187-BAE1-039C640112E7", "Current version of customs messages the company is configured at customs to submit."),
		RegistryStorageFlags.Company,
		new MessageVersionRegistryCollection().DefaultCollection));

	public MessageVersionRegistryItem SenderIDs => GetItem("BECustomsSenderIDs", () => new MessageVersionRegistryItem(
		"BECustomsSenderIDs",
		CustomsDataRegistry.Categories.Customs_Belgium,
		ResString.GetMultilingualString("3F8BF7FD-2275-41EC-B6A1-B45F0D5E7928", "Customs Message Sender IDs"),
		ResString.GetMultilingualString("7F805D2E-6A98-4F31-AD63-12034270745E", "A unique sender ID for the message header of declarations."),
		RegistryStorageFlags.Company,
		new MessageVersionRegistryCollection().DefaultCollection));

	public CustomsRegistryItem CustomsRegistry => GetItem("BECustomsRegistries", () => new CustomsRegistryItem(
		"BECustomsRegistries",
		CustomsDataRegistry.Categories.Customs_Belgium,
		ResString.GetMultilingualString("A40D22DE-CF80-4AFE-96A6-C107564493CA", "Customs Registries"),
		ResString.GetMultilingualString("1FAEA0C2-7FCE-47D6-B976-2208C0B752DA", "Enter the procedures/declaration types with its starting number for the Customs Registry together with the 0 the starting date agreed with customs. One can select the declaration types H1, H2, H3, H4, H5, H6, H7, B1, B2, B3, B4, TD, TA and DA."),
		RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
		new CustomsRegistryCollection().DefaultCollection));

	public BooleanRegistryItem SendUserAndSecretInHeader => GetItem("SendUserAndSecretInHeader", () => new BooleanRegistryItem(
		"SendUserAndSecretInHeader",
		CustomsDataRegistry.Categories.Customs_Belgium,
		ResString.GetMultilingualString("B5D90812-A639-493F-A01C-82855087E3D2", "Send User And Secret In Header"),
		ResString.GetMultilingualString("0B7C38B7-17EA-402B-BE5D-3AB91243D848", "Send the user id & secret in the header of the interchange."),
		RegistryStorageFlags.Company,
		RegistryOptions.IsOnlyForDevelopers,
		false
	));

	public BooleanRegistryItem DetermineTestSystem => GetItem("DetermineTestSystem", () => new BooleanRegistryItem(
		"DetermineTestSystem",
		CustomsDataRegistry.Categories.Customs_Belgium,
		ResString.GetMultilingualString("7D6C01A5-8A7C-45F9-859D-C0BA390FF892", "Determine Test System"),
		ResString.GetMultilingualString("8FDB9B94-2211-4B34-A71F-C4CE3FB62CAD", "If test declarations must be sent to PREPROD (meaning internal test system of customs), tick Yes. If test declarations must be sent to the normal (external) TEST system of customs, then do not override and leave No ticked."),
		RegistryStorageFlags.Company,
		RegistryOptions.IsOnlyForDevelopers,
		false
	));
}
