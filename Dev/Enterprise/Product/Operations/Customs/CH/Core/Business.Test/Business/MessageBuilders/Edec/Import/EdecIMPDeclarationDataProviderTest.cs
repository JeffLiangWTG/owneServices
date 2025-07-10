using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecIMPDeclarationDataProviderTest : EdecDeclarationDataProviderTest<ImportDeclarationMessageSendingObject>
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	protected override DeclarationMessageSendingObject GetMessageSendingObject(CusEntryHeader header) => new ImportDeclarationMessageSendingObject(header);

	protected override EdecDeclarationDataProvider<ImportDeclarationMessageSendingObject> GetEdecDeclarationDataProvider(DeclarationMessageSendingObject sendingObject) => new EdecIMPDeclarationDataProvider(sendingObject as ImportDeclarationMessageSendingObject);

	protected override string ExpectedServiceType => "1";

	public void TestGoodsItems()
	{
		AssertNotNull("GoodsItems should be defined", dataProvider.GoodsItems);
		AssertEquals(true, dataProvider.GoodsItems.FirstOrDefault() is IEdecGoodsItem);
	}

	public override void TestProvider()
	{
		declaration.JE_RL_NKPortOfLoading = "DCXXX";
		declaration.JE_DispatchCountryConfirmation = true;
		declaration.JE_LocationOfGoods = "PU345678901234567890123456789012345";
		declaration.JE_AdditionalDecisionInfo = true;
		declaration.JE_ClearanceLocation = UniversalReferenceConstants.ClearanceLocation.CustomsOffice;

		base.TestProvider();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.DispatchCountry), "DC", dataProvider.DispatchCountry);
			AssertEquals(nameof(dataProvider.DispatchCountryConfirmation), true, dataProvider.DispatchCountryConfirmation);
			AssertEquals(nameof(dataProvider.PlaceofUnloading), "PU345678901234567890123456789012345", dataProvider.PlaceofUnloading);
			AssertEquals(nameof(dataProvider.InjunctionType), "1", dataProvider.InjunctionType);
			AssertEquals(nameof(dataProvider.CustomsOfficeNumber), "CO345678", dataProvider.CustomsOfficeNumber);
			AssertEquals(nameof(dataProvider.ClearanceLocation), UniversalReferenceConstants.ClearanceLocation.CustomsOffice, dataProvider.ClearanceLocation);
		});
	}

	public override void TestAddresses()
	{
		declaration.JE_OH_Importer = newOrgHeader("IMPORTER").PK;

		base.TestAddresses();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.ImporterAddress), "IMPORTER", dataProvider.ImporterAddress?.Name);
			AssertEquals(nameof(dataProvider.AuthorizedConsigneeAddress), "REPRESENTATIVE", dataProvider.AuthorizedConsigneeAddress?.Name);
			AssertEquals(nameof(dataProvider.ConsigneeAddress), "CONSIGNEE", dataProvider.ConsigneeAddress?.Name);
			AssertSame($"{nameof(dataProvider.ImporterAddress)} cached", dataProvider.ImporterAddress, dataProvider.ImporterAddress);
			AssertSame($"{nameof(dataProvider.AuthorizedConsigneeAddress)} cached", dataProvider.AuthorizedConsigneeAddress, dataProvider.AuthorizedConsigneeAddress);
			AssertSame($"{nameof(dataProvider.ConsigneeAddress)} cached", dataProvider.ConsigneeAddress, dataProvider.ConsigneeAddress);
		});
	}

	public override void TestAddressesMissing()
	{
		base.TestAddressesMissing();
		CombineAssertions(() =>
		{
			AssertNull(nameof(dataProvider.ImporterAddress), dataProvider.ImporterAddress);
			AssertNull(nameof(dataProvider.AuthorizedConsigneeAddress), dataProvider.AuthorizedConsigneeAddress);
		});
	}
}
