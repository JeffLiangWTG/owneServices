using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusStorageDocPivotParent : ILinkable, ICusStorageDocPivotTypeSupporter
	{
		CusStorageDocPivotCollection EDocPivotCollection { get; }
	}
}
