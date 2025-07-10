using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyClusterKeyChildBizoWithAddInfoChildSupporter : DummyClusterKeyChildBizo, IAddInfoChildSupporter
	{
		public DummyClusterKeyChildBizoWithAddInfoChildSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter AddInfoChild => this.LoadOrCreateAddInfoChild(ref addInfoChild);
		DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter addInfoChild;

		BusinessObject IAddInfoChildSupporter.AddInfoChild => AddInfoChild;
		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => DummyBizoSchema.Z0_Guid;
		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
	}
}
