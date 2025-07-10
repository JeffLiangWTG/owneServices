using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IValuePostProcessingStrategy
	{
		void ValuePostProcess(ZPropertyInfo valueThatHasChanged, IZType oldValue);
	}
}
