using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SGEntry
	{
		ZString ImportDeclarationNumber { get; }
		ZDate ExtensionDate { get; }
		ZString ApplicationReason { get; }
	}
}
