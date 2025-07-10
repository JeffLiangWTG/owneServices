using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module
{
	public partial class UCC6TemporaryStorageFilterControl : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterControl
	{
		public UCC6TemporaryStorageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
