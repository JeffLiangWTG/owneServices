using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingDocManagerInfo))]
	class InvoicingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		public void TestGetEDocsProviders()
		{
			InvoicingDocManagerInfo info = new InvoicingDocManagerInfo(GetEmptyParentBusinessObject(), null);
			AssertEquals("Length", 0, info.GetEDocsProviders().Length);

			DummyBusinessObject dummyBizO = Factory.New<DummyBusinessObject>();
			info = new InvoicingDocManagerInfo(dummyBizO, null);
			AssertEquals("Length", 0, info.GetEDocsProviders().Length);

			ARInvoice invoice = Factory.New<ARInvoice>();
			info = new InvoicingDocManagerInfo(invoice, null);
			AssertEquals("Length", 0, info.GetEDocsProviders().Length);

			Job job = Factory.NewJobForTesting<Job>();
			invoice.AH_JH = job.PK;
			AssertEquals("Length", 0, info.GetEDocsProviders().Length);

			job.Parent = new MockJobHeaderParent(Factory);
			AssertEquals("Length", 0, info.GetEDocsProviders().Length);

			MockEDocsProvider provider = new MockEDocsProvider(Factory);
			job.Parent = provider;
			IEDocsProvider[] providers = info.GetEDocsProviders();
			AssertEquals("Length", 1, providers.Length);
			AssertEquals("[0]", provider, providers[0]);
		}

		public void TestInvoiceDocStorageMain()
		{
			var info = new InvoicingDocManagerInfo(GetEmptyParentBusinessObject(), null);
			AssertNotNull(info.InvoiceDocStorageMain);
			Assert("IStorageMain", info.InvoiceDocStorageMain is IStorageMain);
			Assert("StorageMain", info.InvoiceDocStorageMain is StorageMain);
		}

		class MockEDocsProvider : IJobHeaderParent, IEDocsProvider
		{
			public MockEDocsProvider(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			#region IJobNumber

			string IJobNumber.JobNumber
			{
				get { return ""; }
			}

			#endregion

			#region IJobHeaderParent Members

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { return factory; }
			}
			readonly BusinessObjectFactory factory;

			ZGuid IJobHeaderParentCore.PK
			{
				get { return ZGuid.Empty; }
			}

			string IJobHeaderParentCore.TableName
			{
				get { return ""; }
			}

			bool IJobHeaderParentCore.IsInDatabase
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			bool IJobHeaderParent.IsDeleted
			{
				get { return false; }
			}

			#endregion

			#region IEDocsProvider Members

			EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			#endregion

			#region IDocumentSupportable Members

			DocumentSupporter IDocumentSupportable.DocumentSupporter
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			string IDocumentSupportable.TableName
			{
				get { return string.Empty; }
			}

			#endregion

			#region IDocManagerSupport Members

			DocManagerInfo IDocManagerSupport.DocManagerInfo
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			#endregion
		}

		class MockJobHeaderParent : IJobHeaderParent
		{
			public MockJobHeaderParent(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			#region IJobNumber

			string IJobNumber.JobNumber
			{
				get { return ""; }
			}

			#endregion

			#region IJobHeaderParent Members

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { return factory; }
			}
			readonly BusinessObjectFactory factory;

			ZGuid IJobHeaderParentCore.PK
			{
				get { return ZGuid.Empty; }
			}

			string IJobHeaderParentCore.TableName
			{
				get { return ""; }
			}

			bool IJobHeaderParentCore.IsInDatabase
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			bool IJobHeaderParent.IsDeleted
			{
				get { return false; }
			}

			#endregion
		}
	}
}
