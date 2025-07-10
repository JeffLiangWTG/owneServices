using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationAdditionalInformation : ICADMessageDeclarationAdditionalInformation
{
	public CADDeclarationAdditionalInformation(string statementCode = "", string statementTypeCode = "", string statementDescription = "", string limitDateTime = "")
	{
		this.statementCode = statementCode;
		this.statementTypeCode = statementTypeCode;
		this.statementDescription = statementDescription;
		this.limitDateTime = limitDateTime;
	}

	readonly ZString statementCode;
	readonly ZString statementTypeCode;
	readonly ZString statementDescription;
	readonly ZString limitDateTime;

	#region ICADMessageDeclarationAdditionalInformation

	string ICADMessageDeclarationAdditionalInformation.StatementCode => statementCode;

	string ICADMessageDeclarationAdditionalInformation.StatementDescription => statementDescription;

	string ICADMessageDeclarationAdditionalInformation.LimitDateTime => limitDateTime;

	string ICADMessageDeclarationAdditionalInformation.StatementTypeCode => statementTypeCode;

	#endregion
}
