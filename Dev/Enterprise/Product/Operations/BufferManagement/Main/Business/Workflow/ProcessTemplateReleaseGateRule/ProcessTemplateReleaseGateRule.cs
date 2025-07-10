using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRule : AutoProcessTemplateReleaseGateRule
	{
		public ProcessTemplateReleaseGateRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}

