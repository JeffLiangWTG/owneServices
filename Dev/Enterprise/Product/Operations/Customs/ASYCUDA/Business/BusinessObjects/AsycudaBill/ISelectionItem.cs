
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface ISelectionItem
	{
		string SelectionDescription(bool showStatus);
		ZGuid PK { get; }
	}
}
