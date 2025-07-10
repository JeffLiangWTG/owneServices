using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class EComCHEDIMessageCollectionView : BusinessObjectCollectionView<CHEDIMessage>
{
	public EComCHEDIMessageCollectionView(CHEDIMessageCollection completeCollection) : base(completeCollection)
	{
	}

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
	{
		var ediMessage = element as CHEDIMessage;
		return ediMessage.EM_MessageType == MessageTypeCodeList.Codes.ECM;
	}
}
