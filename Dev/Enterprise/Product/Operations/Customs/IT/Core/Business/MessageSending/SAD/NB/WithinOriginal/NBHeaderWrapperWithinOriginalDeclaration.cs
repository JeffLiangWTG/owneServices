using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBHeaderWrapperWithinOriginalDeclaration : INBHeader
{
	public NBHeaderWrapperWithinOriginalDeclaration(INBWrappableBusinessObject nbObject)
	{
		this.nbObject = Argument.NotNull(nbObject, nameof(nbObject));
	}

	readonly INBWrappableBusinessObject nbObject;

	public ZString MessageCodeEntry => GetMessageCodeEntry();

	public ZString ReferenceNumber => Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder;

	public ZString DeclarationCIN => ZString.Empty;

	public ZDate DeclarationDate => ZDate.Empty;

	public ZInt ItemNumber => nbObject.LineNumber;

	ZString GetMessageCodeEntry()
	{
		ZString messageCodeEntry;
		if (nbObject.IsImport == ZBool.True)
		{
			messageCodeEntry = SADConstants.MessageSubTypes.IM;
		}
		else if (nbObject.IsExport == ZBool.True)
		{
			messageCodeEntry = SADConstants.MessageSubTypes.ET;
		}
		else
		{
			messageCodeEntry = ZString.Empty;
		}
		return messageCodeEntry;
	}
}
