using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyProcessHandlingInfoProviderBizo : DummyEnterpriseBusinessObject, IProcessHandlingInfoProvider
	{
		public DummyProcessHandlingInfoProviderBizo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			processHandlingInfo = new DummyProcessHandlingInfo(this);
		}

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return processHandlingInfo; }
		}

		readonly ProcessHandlingInfo processHandlingInfo;
	}
}
