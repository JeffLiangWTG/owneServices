using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ServiceWrapperCollection : GenericWrapperCollection<ServiceWrapper>
	{
		public ServiceWrapperCollection(GenericWrapper wrapperForReportName, BusinessObjectFactory factory)
			: base(factory)
		{
			this.WrapperForReportName = wrapperForReportName;
		}
		readonly GenericWrapper WrapperForReportName;

		public ServiceWrapperCollection(JobServiceDependentCollection services, BusinessObjectFactory factory)
			: base(factory)
		{
			if (services != null)
			{
				foreach (JobService service in services)
				{
					Add(new ServiceWrapper(service, factory));
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (WrapperForReportName != null)
			{
				((ServiceWrapper)bizOAdded).WrapperForReportName = WrapperForReportName;
			}
		}
	}
}
