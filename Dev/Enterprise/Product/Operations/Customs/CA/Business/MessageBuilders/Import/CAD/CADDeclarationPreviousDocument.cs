using CargoWise.Customs.CA.MessageContracts.CAD;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationPreviousDocument : ICADMessageDeclarationPreviousDocument
{
	public CADDeclarationPreviousDocument(string id = "", string typeCode = "")
	{
		this.id = id;
		this.typeCode = typeCode;
	}

	readonly string id;
	readonly string typeCode;

	#region ICADMessageDeclarationPreviousDocument

	string ICADMessageDeclarationPreviousDocument.ID => id;

	string ICADMessageDeclarationPreviousDocument.TypeCode => typeCode;

	#endregion
}
