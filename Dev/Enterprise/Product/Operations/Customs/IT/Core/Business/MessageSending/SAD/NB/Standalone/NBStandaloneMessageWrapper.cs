using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBStandaloneMessageWrapper : NBMessageWrapperBase
{
	public NBStandaloneMessageWrapper(INBWrappableBusinessObject nbObject, int indexOfMessageInDeclaration)
		: base(nbObject)
	{
		this.indexOfMessageInDeclaration = indexOfMessageInDeclaration;
	}
	readonly int indexOfMessageInDeclaration;

	public override ZString AnnualProgressiveNumber => ITEDIMessage.MessageNumberPlaceHolderForNB(indexOfMessageInDeclaration);

	public override INBHeader Header => new NBStandaloneHeaderWrapper(nbObject);
}
