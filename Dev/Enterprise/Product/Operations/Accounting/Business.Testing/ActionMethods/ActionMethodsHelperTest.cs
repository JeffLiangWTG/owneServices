using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	using System.Data;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Integration;
	using Moq;

	public class ActionMethodsHelperTest : TestCaseWithFactory
	{
		public void TestTargetReference()
		{
			AssertExceptionThrown("Should be exception", typeof(ArgumentNullException), delegate
			{
				ActionMethodsHelper.TargetReference(null);
			}

			);
			DummyBusinessObjectWithInvoicingPlugIn obj = Factory.NewWithValidTestData<DummyBusinessObjectWithInvoicingPlugIn>();
			Factory.Save();
			AssertEquals("Should be HumanReadableName", "Test Description", ActionMethodsHelper.TargetReference(obj));
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			Factory.Save();
			AssertEquals("Should be correct reference", new LogControllerLink("Shipment S001", ControllerIDs.JobShipment, shipment.PK), ActionMethodsHelper.TargetReference(shipment));
		}

		public void TestExRateProviderReference()
		{
			AssertExceptionThrown("Should throw an exception", typeof(ArgumentNullException),
				delegate { ActionMethodsHelper.ExRateProviderReference(null); });

			LogControllerLink link = new LogControllerLink("Link Text", DummyControllerIDs.Dummy, ZGuid.NewZGuid());
			var sourceMock = new Mock<IExchangeRateSource>();
			sourceMock.Setup(m => m.SourceController).Returns(link.Controller);
			sourceMock.Setup(m => m.SourcePK).Returns(link.PK);
			sourceMock.Setup(m => m.Description).Returns(link.Text);
			AssertEquals(link, ActionMethodsHelper.ExRateProviderReference(sourceMock.Object));
			sourceMock.VerifyAll();
			sourceMock.Setup(m => m.SourceController).Returns(DummyControllerIDs.Dummy);
			sourceMock.Setup(m => m.SourcePK).Returns(ZGuid.Empty);
			sourceMock.Setup(m => m.Description).Returns("Text");
			AssertEquals("Text", ActionMethodsHelper.ExRateProviderReference(sourceMock.Object));
			sourceMock.VerifyAll();
			sourceMock.Setup(m => m.SourceController).Returns((ControllerID)null);
			sourceMock.Setup(m => m.Description).Returns("Text");
			AssertEquals("Text", ActionMethodsHelper.ExRateProviderReference(sourceMock.Object));
		}

		class DummyBusinessObjectWithInvoicingPlugIn : DummyBusinessObject, IJobInvoicingPlugIn
		{
			public DummyBusinessObjectWithInvoicingPlugIn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return "Test Description";
				}
			}

			JobInvoicingSupporter fInvoicingSupporter;
			public IJobInvoicingSupporter InvoicingSupporter
			{
				get
				{
					return fInvoicingSupporter ?? (fInvoicingSupporter = new JobInvoicingSupporter(this));
				}
			}

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get
				{
					return Factory;
				}
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			ZGuid IJobHeaderParentCore.PK
			{
				get
				{
					return PK;
				}
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}

			string IJobHeaderParentCore.TableName
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			string IJobNumber.JobNumber
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
