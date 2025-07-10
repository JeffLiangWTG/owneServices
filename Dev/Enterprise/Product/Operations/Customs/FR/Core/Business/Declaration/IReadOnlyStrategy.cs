using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IReadOnlyStrategy
	{
		bool IsReadonly(ZPropertyInfo propertyInfo);
	}
}
