using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing;

public abstract class EMCSDocumentCollectionAbstractTest<TEMCSDocument> : Customs.Business.Testing.CusSupportingInfoCollectionTest<TEMCSDocument> where TEMCSDocument : EMCSDocument
{
	public void TestAllowNewCore()
	{
		var declaration = Factory.New<EMCSJobDeclaration>();
		var collection = new EMCSDocumentCollection<EMCSDocument>(declaration);

		declaration.JE_MessageStatus = string.Empty;
		Factory.InvalidateCachedProperties();

		Assert("Should default to true.", collection.AllowNew);

		declaration.JE_MessageStatus = EDIMessage.Status.Sent;
		Factory.InvalidateCachedProperties();

		Assert("Should not allow new when the message status of parent is SNT.", !collection.AllowNew);

		declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
		Factory.InvalidateCachedProperties();

		Assert("Should not allow new when the message status of parent is ACK.", !collection.AllowNew);
	}
}
