using CargoWise.Customs.CA.MessageContracts.CAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentSeller : ICADMessageDeclarationGoodsShipmentSeller
{
	public CADDeclarationGoodsShipmentSeller(JobDocAddress docAddress)
	{
		this.docAddress = docAddress;
	}

	readonly JobDocAddress docAddress;

	#region ICADMessageDeclarationGoodsShipmentSeller

	string ICADMessageDeclarationGoodsShipmentSeller.Name => CADMessageHelper.StripOutInvalidCharacters(docAddress.CompanyName).SubstringSafe(0, CADDeclarationGoodsShipmentBuyer.CompanyNameMaxLength).ToUpper();

	ICADMessageDeclarationGoodsShipmentAddress ICADMessageDeclarationGoodsShipmentSeller.SellerAddress
	{
		get
		{
			if (sellerAddress == null)
			{
				sellerAddress = new CADDeclarationGoodsShipmentAddress(docAddress, true);
			}
			return sellerAddress;
		}
	}
	CADDeclarationGoodsShipmentAddress sellerAddress;

	string ICADMessageDeclarationGoodsShipmentSeller.CommunicationID => docAddress.PhoneNumber.FormattedForBinding;

	#endregion
}
