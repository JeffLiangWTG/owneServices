using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderCollection : IBusinessObjectCollection
	{
		void DeleteAll();
		void Delete(IBusiness bizo);
		IDictionary<IProcessHeader, IProcessHeader> CloneWorkflowsAndLinksForTemplates(IProcessJobHeader targetJobHeader, IProcessHeaderCollection targetCollection);

		new IProcessHeader AddNew();
		new IProcessHeader this[int index] { get; }
	}
}
