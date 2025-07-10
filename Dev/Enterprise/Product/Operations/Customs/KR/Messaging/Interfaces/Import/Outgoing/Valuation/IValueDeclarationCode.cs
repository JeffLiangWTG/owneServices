using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IValueDeclarationCode
	{
		ZString Code { get; }
		ZString CodeOtherDescription { get; }
	}
}
