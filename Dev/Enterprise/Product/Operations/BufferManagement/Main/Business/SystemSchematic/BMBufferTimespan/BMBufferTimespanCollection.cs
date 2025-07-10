using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMBufferTimespan)]
	public class BMBufferTimespanCollection : ActiveBusinessObjectCollection<BMBufferTimespan>, IBMBufferTimespanCollection
	{
		public BMBufferTimespanCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			ApplySort(nameof(BMBufferTimespan.BMT_Name), System.ComponentModel.ListSortDirection.Ascending);
		}

		IBMBufferTimespan IBMBufferTimespanCollection.this[int index]
		{
			get { return base[index]; }
		}
	}
}
