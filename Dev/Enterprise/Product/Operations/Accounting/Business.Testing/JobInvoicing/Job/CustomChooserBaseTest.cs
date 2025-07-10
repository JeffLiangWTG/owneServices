using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CustomChooserBaseTest : TestCaseWithFactory
	{
		public TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}
		TestObjectCreator creator;

		public FallbackLevel FallbackLevel
		{
			get
			{
				if (fallbackLevel == null)
				{
					fallbackLevel = new FallbackLevel(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				}
				return fallbackLevel;
			}
		}
		FallbackLevel fallbackLevel;

		public class DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper : IJobInvoicingPlugIn, IBusinesssObjectProviderForDocumentWrapper
		{
			public DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper(BusinessObject businessObjectForDocumentWrapper)
			{
				this.businessObjectForDocumentWrapper = businessObjectForDocumentWrapper;
			}
			readonly BusinessObject businessObjectForDocumentWrapper;

			BusinessObject IBusinesssObjectProviderForDocumentWrapper.BusinessObjectForDocumentWrapper => businessObjectForDocumentWrapper;
			public IJobInvoicingSupporter InvoicingSupporter => throw new NotImplementedException();

			public bool AllowInvoiceDeletion => throw new NotImplementedException();

			public bool IsDeleted => throw new NotImplementedException();

			public ZGuid PK => throw new NotImplementedException();

			public string TableName => throw new NotImplementedException();

			public bool IsInDatabase => throw new NotImplementedException();

			public BusinessObjectFactory Factory => throw new NotImplementedException();

			public string JobNumber => throw new NotImplementedException();

			public void OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleted(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleting(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}
		}

		public class DummyJobInvoicingPluginIsNotBusinessObject : IJobInvoicingPlugIn
		{
			public IJobInvoicingSupporter InvoicingSupporter => throw new NotImplementedException();

			public bool AllowInvoiceDeletion => throw new NotImplementedException();

			public bool IsDeleted => throw new NotImplementedException();

			public ZGuid PK => throw new NotImplementedException();

			public string TableName => throw new NotImplementedException();

			public bool IsInDatabase => throw new NotImplementedException();

			public BusinessObjectFactory Factory => throw new NotImplementedException();

			public string JobNumber => throw new NotImplementedException();

			public void OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleted(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleting(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}
		}
	}
}
