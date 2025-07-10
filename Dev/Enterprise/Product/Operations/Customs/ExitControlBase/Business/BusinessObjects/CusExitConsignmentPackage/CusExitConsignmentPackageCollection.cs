using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business
{
	public interface ICusExitConsignmentPackageCollection<out T> : IActiveBusinessObjectCollection<T>
	{
		new T this[int index] { get; }
		void MarkAsNeedingValidation();
		event CollectionCountChangedEventHandler CollectionCountChange;
	}

	public class CusExitConsignmentPackageCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitConsignmentPackageCollection<T>
		where T : CusExitConsignmentPackage
	{
		public CusExitConsignmentPackageCollection(CusExitHeader master)
			: base(master.Factory, master, new ZQuery(), CusExitConsignmentPackageSchema.CXP_CXH_Header)
		{
		}
	}
}
