using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public interface INctsCusStorageDocPivotParent : ILinkable, ICusStorageDocPivotTypeSupporter
	{
		NctsCusStorageDocPivotCollection EDocPivotCollection { get; }
	}
}
