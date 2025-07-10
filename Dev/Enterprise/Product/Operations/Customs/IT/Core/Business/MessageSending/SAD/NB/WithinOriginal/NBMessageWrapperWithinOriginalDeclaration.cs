using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBMessageWrapperWithinOriginalDeclaration : NBMessageWrapperBase
{
	public NBMessageWrapperWithinOriginalDeclaration(INBWrappableBusinessObject nbObject)
		: base(nbObject)
	{
	}

	public override INBHeader Header => new NBHeaderWrapperWithinOriginalDeclaration(nbObject);

	public override ZString AnnualProgressiveNumber => Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder;
}
