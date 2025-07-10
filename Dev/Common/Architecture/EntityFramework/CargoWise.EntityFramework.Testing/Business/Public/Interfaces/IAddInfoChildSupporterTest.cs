using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBizObjWithAddInfoChildSupporter : DummyBusinessObject, IAddInfoChildSupporter
	{
		public DummyBizObjWithAddInfoChildSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter AddInfoChild => this.LoadOrCreateAddInfoChild(ref addInfoChild);
		DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter addInfoChild;

		public SchemaGuidColumn ChildForeignKeyColumnForTesting
		{
			get => childForeignKeyColumnForTesting;
			set
			{
				if (addInfoChild != null)
				{
					addInfoChild.Delete();
					addInfoChild = null;
				}
				childForeignKeyColumnForTesting = value;
			}
		}
		SchemaGuidColumn childForeignKeyColumnForTesting = DummyBizoSchema.Z0_Guid;

		BusinessObject IAddInfoChildSupporter.AddInfoChild => AddInfoChild;
		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => ChildForeignKeyColumnForTesting;
		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
	}
}
