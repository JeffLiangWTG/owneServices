using System.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoChildSupporter
	{
		bool IsDeleted { get; }
		bool IsInDatabase { get; }
		BusinessObject AddInfoChild { get; }
		ZGuid PK { get; }
		BusinessObjectFactory Factory { get; }
		SchemaGuidColumn ChildForeignKeyColumn { get; }
		void RegisterEditableChildObject(IBusiness child);
		void UnRegisterEditableChildObject(IBusiness child);
		void RegisterListChangedCalledRefreshBinding(IBindingList element);
		void UnRegisterListChangedCalledRefreshBinding(IBindingList element);
	}
}
