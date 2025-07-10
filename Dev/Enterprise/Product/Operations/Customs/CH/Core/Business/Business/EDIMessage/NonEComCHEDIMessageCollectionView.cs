using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class NonEComCHEDIMessageCollectionView : BusinessObjectCollectionView<CHEDIMessage>
{
	public NonEComCHEDIMessageCollectionView(CHEDIMessageCollection completeCollection) : base(completeCollection)
	{
	}

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
	{
		var ediMessage = element as CHEDIMessage;
		return ediMessage.EM_MessageType != MessageTypeCodeList.Codes.ECM;
	}
}
