using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public interface ICusStorageDocPivotParent : ILinkable, ICusStorageDocPivotTypeSupporter
	{
		CusStorageDocPivotCollection EDocPivotCollection { get; }
	}
}
