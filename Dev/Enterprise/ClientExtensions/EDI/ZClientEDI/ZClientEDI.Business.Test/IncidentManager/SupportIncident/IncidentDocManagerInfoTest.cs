using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentDocManagerInfo))]
	public class IncidentDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<SupportIncident>().Request;
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<SupportIncident>().Request;
		}
	}
}
