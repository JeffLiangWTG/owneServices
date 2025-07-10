using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentBuyer : ICADMessageDeclarationGoodsShipmentBuyer
{
	public CADDeclarationGoodsShipmentBuyer(JobDocAddress docAddress)
	{
		this.docAddress = docAddress;
	}

	readonly JobDocAddress docAddress;
	public const int CompanyNameMaxLength = 40;

	#region ICADDeclarationGoodsShipmentBuyer

	public string Name => CADMessageHelper.StripOutInvalidCharacters(docAddress.CompanyName).SubstringSafe(0, CompanyNameMaxLength).ToUpper();

	public ICADMessageDeclarationGoodsShipmentAddress BuyerAddress
	{
		get
		{
			if (buyerAddress == null)
			{
				buyerAddress = new CADDeclarationGoodsShipmentAddress(docAddress);
			}
			return buyerAddress;
		}
	}
	CADDeclarationGoodsShipmentAddress buyerAddress;

	public string CommunicationID => docAddress.PhoneNumber.FormattedForBinding;

	public bool IsEmpty => ((ZString)Name).IsEmpty && ((ZString)CommunicationID).IsEmpty && BuyerAddress.IsEmpty;

	#endregion
}
