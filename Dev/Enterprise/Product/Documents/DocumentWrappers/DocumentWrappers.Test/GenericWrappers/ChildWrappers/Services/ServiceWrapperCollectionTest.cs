using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ServiceWrapperCollection))]
	sealed class ServiceWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<ServiceWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ServiceWrapper(null, Factory);
		}

		protected override ServiceWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ServiceWrapperCollection((JobServiceDependentCollection)null, Factory);
		}

		public void TestContainerServiceWrapperCollectionWithServices()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";
			JobService service = container.Services.AddNew();
			service.ES_ServiceCode = "CLN";

			ServiceWrapperCollection wrapperColelction = new ServiceWrapperCollection(container.Services, Factory);
			AssertEquals("wrapperColelction.Count", 1, wrapperColelction.Count);
			AssertEquals("wrapperColelction[0].Type.Code", "CLN", wrapperColelction[0].Type.Code);
		}

		public void TestWrapperForReportName()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";
			JobService service = container.Services.AddNew();
			service.ES_ServiceCode = "CLN";

			FreightWrapperFromConsol parentWrapper = new FreightWrapperFromConsol(consol, Factory);
			parentWrapper.SetReportNameForTesting("Request For Service");

			ServiceWrapperCollection wrapperCollection = new ServiceWrapperCollection(parentWrapper, Factory);
			wrapperCollection.Add(ServiceWrapper.New(container.Services[0], Factory));
			AssertEquals("Request For Service", wrapperCollection[0].WrapperForReportName.ReportName);
		}
	}
}
