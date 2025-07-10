using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BFHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZString ApplicationReason { get; }
	}
}
