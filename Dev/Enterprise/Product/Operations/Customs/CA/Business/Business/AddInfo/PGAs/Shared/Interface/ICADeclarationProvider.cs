using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public interface ICADeclarationProvider : IDeclarationProvider
	{
		ZBool IsValidationEnabled { get; }
	}
}
