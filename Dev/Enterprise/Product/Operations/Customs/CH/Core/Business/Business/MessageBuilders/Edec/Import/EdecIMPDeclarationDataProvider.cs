using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecIMPDeclarationDataProvider : EdecDeclarationDataProvider<ImportDeclarationMessageSendingObject>
{
	public EdecIMPDeclarationDataProvider(ImportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}
	public override string DispatchCountry => Declaration?.DispatchCountryCode;

	public override bool DispatchCountryConfirmation => Declaration?.JE_DispatchCountryConfirmation ?? false;

	public override string PlaceofUnloading => Declaration?.JE_LocationOfGoods;

	public override string InjunctionType => Declaration != null && Declaration.JE_AdditionalDecisionInfo ? "1" : "0";

	public override string ServiceType => "1";

	protected override IEdecBusiness NewEdecBusiness(CusEntryHeader entryHeader) => EdecIMPBusinessDataProvider.New(entryHeader);

	protected override IEdecGoodsItem NewEdecGoodsItem(CusEntryLine entryLine) => EdecIMPGoodsItemDataProvider.New(entryLine);

	public override IEdecAddress ImporterAddress => importerAddress ?? (importerAddress = EdecAddressDataProvider.New(Declaration?.ImporterDocumentaryAddress));
	IEdecAddress importerAddress;

	public override IEdecAddress ConsigneeAddress => consigneeAddress ?? (consigneeAddress = EdecAddressDataProvider.New(Declaration?.IntermConsignee, true));
	IEdecAddress consigneeAddress;

	public override IEdecAddress AuthorizedConsigneeAddress => authorizedConsigneeAddress ?? (authorizedConsigneeAddress = EdecAddressDataProvider.New(Declaration?.Representative));
	IEdecAddress authorizedConsigneeAddress;
}
