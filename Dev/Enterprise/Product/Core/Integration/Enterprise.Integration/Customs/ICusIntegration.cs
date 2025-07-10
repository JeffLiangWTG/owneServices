using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICusIntegration
	{
		ZString Execute(Customs.IBaseJobDeclaration declaration);
	}
}