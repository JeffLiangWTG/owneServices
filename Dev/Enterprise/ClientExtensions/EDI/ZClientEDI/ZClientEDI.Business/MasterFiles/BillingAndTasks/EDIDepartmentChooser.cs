using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIDepartmentChooser : DepartmentChooser
	{
		protected EDIDepartmentChooser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static new DepartmentChooser New(BusinessObjectFactory factory)
		{
			return new EDIDepartmentChooser(factory);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
		}

		static DepartmentChooser OverriddenNew(BusinessObjectFactory factory)
		{
			return New(factory);
		}

		protected override bool UseDefaultLoginDepartment(ZString jobType)
		{
			return base.UseDefaultLoginDepartment(jobType) || IsJobTypeEDIClientSpecific(jobType);
		}

		bool IsJobTypeEDIClientSpecific(ZString jobType)
		{
			return (
				jobType == EDIJobInvoicingConsumerTypes.Incident.Code ||
				jobType == EDIJobInvoicingConsumerTypes.PSQuote.Code);
		}

		internal bool UseDefaultLoginDepartmentForTest(ZString jobType) => UseDefaultLoginDepartment(jobType);
	}
}
