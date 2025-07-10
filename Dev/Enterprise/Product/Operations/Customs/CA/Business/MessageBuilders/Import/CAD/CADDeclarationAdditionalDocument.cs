using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationAdditionalDocument : ICADMessageDeclarationAdditionalDocument
{
	public CADDeclarationAdditionalDocument(ZString id, ZString typeCode)
	{
		this.id = id;
		this.typeCode = typeCode;
	}

	readonly ZString id;
	readonly ZString typeCode;

	#region ICADMessageDeclarationAdditionalDocument

	string ICADMessageDeclarationAdditionalDocument.ID => id;

	string ICADMessageDeclarationAdditionalDocument.TypeCode => typeCode;

	#endregion
}
