using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocumentWrapperFactoryTest : TestCaseWithFactory
	{
		public void TestCreateWrapperWithNamespace()
		{
			using (((IGlbCompany)Env.CurrentCompany).TemporarilySetCountry(CountryCodes.Latvia))
			{
				var declaration = (BusinessObject)Factory.New<Integration.Customs.EU.IJobDeclaration>();
				var entryHeader = (BusinessObject)Factory.New<Integration.Customs.EU.ICusEntryHeader>();
				entryHeader[CusEntryHeaderSchema.CH_JE] = declaration.PK;

				var wrapper = DocumentWrapperFactory.CreateWrapper(DataContext.SADH, "Enterprise.DocumentWrappers.Customs.EU", entryHeader);
				AssertNotNull("SADH Wrapper is not null", wrapper);
				AssertEquals("wrapper.WrappedObject", entryHeader, wrapper.WrappedObject);
			}
		}

		public void TestGenericWrapperGetter()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			DocumentWrapper[] freightJobWrapperArray = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, declaration);
			AssertNotNull("freightJobWrapperArray should not be null", freightJobWrapperArray);
			AssertEquals("freightJobWrapperArray.Length", 1, freightJobWrapperArray.Length);
			DocumentWrapper freightJobWrapper = freightJobWrapperArray[0];
			AssertNotNull("freightJobWrapper should not be null", freightJobWrapper);
			AssertEquals("freightJobWrapper.WrappedObject", declaration, freightJobWrapper.WrappedObject);
		}

		public void TestGenericWrapperGetterWithCopyInfo()
		{
			DocWrapperCopyInfoForTesting copyInfo = new DocWrapperCopyInfoForTesting();
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			IBODocDataProvider[] freightJobWrapperArray = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, declaration, new DocWrapperCopyInfo[] { copyInfo });
			AssertNotNull("freightJobWrapperArray should not be null", freightJobWrapperArray);
			AssertEquals("freightJobWrapperArray.Length", 2, freightJobWrapperArray.Length);
			AssertEquals("first wrapper is not a copy", false, freightJobWrapperArray[0].AdditionalCopyInfo != null);
			AssertEquals("second wrapper is a copy", true, freightJobWrapperArray[1].AdditionalCopyInfo != null);
			AssertEquals("second wrapper CopyInfo instance same as the one passed in", copyInfo, freightJobWrapperArray[1].AdditionalCopyInfo);
		}

		public void TestGenerateGenericWrappersForParentAndChildType()
		{
			BusinessObject transport = (BusinessObject)Factory.New<Integration.Freight.ITransport>();
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();

			DocumentWrapper[] freightJobWrapperArray = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJobRouting, shipment, transport);
			AssertNotNull("freightJobWrapperArray should not be null", freightJobWrapperArray);
			AssertEquals("freightJobWrapperArray.Length", 1, freightJobWrapperArray.Length);
			DocumentWrapper freightJobWrapper = freightJobWrapper = freightJobWrapperArray[0];
			AssertEquals("freightJobWrapper.WrappedObject", shipment, freightJobWrapper.WrappedObject);
		}

		public void TestCreateWrapper_InvalidType()
		{
			AssertExceptionThrown<ArgumentNullException>(() => DocumentWrapperFactory.CreateWrapper(DataContext.UnitTest, null));
		}

		public void TestCreateWrapper()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(DataContext.ForwardingShipment, shipment);
			AssertNotNull("Shipment Wrapper is not null", wrapper);

			var consol = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingConsol>();
			wrapper = DocumentWrapperFactory.CreateWrapper(DataContext.ForwardingConsol, consol);
			AssertNotNull("Consol Wrapper is not null", wrapper);
			var manifest = Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifest.AMA_ParentId = consol.PK;
			manifest.AMA_ParentTableCode = consol.TablePrefix;
			wrapper = DocumentWrapperFactory.CreateWrapper("Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderDocWrapper, Enterprise.Customs.ASYCUDA.Business", new Object[] { consol, ZString.Empty, ZString.Empty });
			AssertNotNull("Consol Wrapper is not null", wrapper);
		}

		public void TestCreateWrappers()
		{
			DocWrapperCopyInfoForTesting copyInfo = new DocWrapperCopyInfoForTesting();

			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();
			IBODocDataProvider[] wrappers = DocumentWrapperFactory.CreateWrappers(DataContext.Shipment, shipment, new DocWrapperCopyInfo[] { copyInfo });
			AssertEquals("Should create two wrappers - one original, one copy", 2, wrappers.Length);
			AssertEquals("first wrapper is not a copy", false, wrappers[0].AdditionalCopyInfo != null);
			AssertEquals("second wrapper is a copy", true, wrappers[1].AdditionalCopyInfo != null);
			AssertEquals("second wrapper CopyInfo instance same as the one passed in", copyInfo, wrappers[1].AdditionalCopyInfo);
		}

		public void TestCreateCustomsWrapper()
		{
			AssertCustomsWrapperType<Integration.Customs.AU.IJobDeclaration>(CountryCodes.Australia, "Enterprise.DocumentWrappers.Customs.AU.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.IBaseJobDeclaration>(CountryCodes.Fiji, "Enterprise.DocumentWrappers.Customs.General.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.ZA.IJobDeclaration>(CountryCodes.SouthAfrica, "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.SG.IJobDeclaration>(CountryCodes.Singapore, "Enterprise.Customs.SG.V4.Business.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.MY.IJobDeclaration>(CountryCodes.Malaysia, "Enterprise.DocumentWrappers.Customs.General.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.NZ.IJobDeclaration>(CountryCodes.NewZealand, "Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration");
			AssertCustomsWrapperType<Integration.Customs.NZ.IJobComInvoiceHeader>(CountryCodes.NewZealand, "Enterprise.DocumentWrappers.Customs.NZ.DocJobComInvoiceHeader", DataContext.JobComInvoiceHeader);
		}

		public void TestCreateCustomsWrapperZA()
		{
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var wrapper = DocumentWrapperFactory.CreateCustomsWrapperZA(DataContext.ZADA306, shipment);
			AssertEquals("Wrapper for ZA ForwardingShipment", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocZADA306", wrapper?.GetType().FullName);
		}

		public void TestCreateNZConsolWrappers()
		{
			BusinessObject consol = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingConsol>();
			DocumentWrapper wrapper = DocumentWrapperFactory.CreateNZConsolWrapper(consol);
			AssertNotNull("Consol Wrapper for NZ outward report constructed", wrapper);
		}

		public void TestCreateCHConsolWrappers()
		{
			BusinessObject consol = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingConsol>();
			DocumentWrapper wrapper = DocumentWrapperFactory.CreateNZConsolWrapper(consol);
			AssertNotNull("Consol Wrapper for CH outward report constructed", wrapper);
		}

		public void TestCreateOrganisationWrappersWithSupplierBuyer()
		{
			var orgHeader = (BusinessObject)Factory.New<IOrgHeader>();
			var wrapper = DocumentWrapperFactory.CreateOrganisationWrapperWithSupplierBuyer(orgHeader, null, null);
			AssertNotNull("Organisation Wrapper is constructed", wrapper);
		}

		public void TestCreateContainerWrapperWithShipment()
		{
			BusinessObject container = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingContainer>();
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();

			DocumentWrapper wrapper = DocumentWrapperFactory.CreateContainerWrapperWithShipment(container, shipment);
			AssertNotNull("Container Wrapper Created", wrapper);

			ZString wrapperType = new ZString(wrapper.GetType().ToString());
			AssertEquals("Wrapper is of type DocContainer", true, wrapperType.Contains("DocContainer"));
		}

		public void TestCreateIMOShipmentWrapper()
		{
			BusinessObject consol = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingConsol>();
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();

			DocumentWrapper wrapper = DocumentWrapperFactory.CreateIMOShipmentWrapper(shipment, consol);
			AssertNotNull("IMO Shipment Wrapper Created", wrapper);

			ZString wrapperType = new ZString(wrapper.GetType().ToString());
			AssertEquals("Wrapper is of type DocIMOShipment", true, wrapperType.Contains("DocIMOShipment"));
		}

		public void TestCreateServiceWrapperWithParent()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			BusinessObject service = (BusinessObject)Factory.New<IJobService>();
			DocumentWrapper parentWrapper = DocumentWrapperFactory.CreateWrapper(DataContext.Shipment, shipment);

			DocumentWrapper wrapper = DocumentWrapperFactory.CreateServiceWrapperWithParent(service, parentWrapper);
			AssertNotNull("Service Shipment Wrapper Created", wrapper);

			ZString wrapperType = new ZString(wrapper.GetType().ToString());
			AssertEquals("Wrapper is of type DocService", true, wrapperType.Contains("DocService"));
		}

		public void TestCreateCustomsContainerWrapperWithDeclaration()
		{
			BusinessObject cusContainer = (BusinessObject)Factory.New<Integration.Customs.AU.ICusContainer>();
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.AU.IJobDeclaration>();

			DocumentWrapper wrapper = DocumentWrapperFactory.CreateCustomsContainerWrapperWithDeclaration(cusContainer, declaration, "AU");
			AssertNotNull("CusContainer Wrapper created", wrapper);

			ZString wrapperType = new ZString(wrapper.GetType().ToString());
			AssertEquals("Wrapper is of type DocCusContainer", true, wrapperType.Contains("AU.DocCusContainer"));
		}

		public void TestCreateFromClientDLL()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientHook()))
			{
				var factory = new BusinessObjectFactory();

				var bizObj = factory.New<DummyBusinessObject>();
				var wrapper = (TestDocumentBusinessObjectWrapper)DocumentWrapperFactory.CreateWrapper(GetType().FullName + "+TestDocumentBusinessObjectWrapper", bizObj);
				AssertNotNull("Test wrapper should not be null", wrapper);
			}
		}

		public void TestShowInvalidCastExceptionMessage()
		{
			using (((IGlbCompany)Env.CurrentCompany).TemporarilySetCountry(CountryCodes.Latvia))
			{
				var declaration = (BusinessObject)Factory.New<Integration.Customs.EU.IJobDeclaration>();
				var dummyLoader = new DummyGenericWrapperLoader();
				AssertNoExceptionThrown(() => { DocumentWrapperFactory.GetDocumentWrapper(DataContext.GenericFreightJobFromShipment, declaration, dummyLoader); });
				AssertEquals(string.Format("An error happened when generating the document. Data context \"{0}\" could be improper for this document. Please try modifying your customized document configuration and choose an appropriate data context before running again.",
					DataContext.GenericFreightJobFromShipment), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var entryHeader = (BusinessObject)Factory.New<Integration.Customs.EU.ICusEntryHeader>();
				AssertNoExceptionThrown(() => { DocumentWrapperFactory.GetDocumentWrapper(DataContext.GenericFreightJobInvoice, declaration, dummyLoader, entryHeader); });
				AssertEquals(string.Format("An error happened when generating the document. Data context \"{0}\" could be improper for this document. Please try modifying your customized document configuration and choose an appropriate data context before running again.",
					DataContext.GenericFreightJobInvoice), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSGCusContainerDocumentWrapper()
		{
			var cusContainer = (BusinessObject)Factory.New<Integration.Customs.SG.ICusContainer>();
			var declaration = (BusinessObject)Factory.New<Integration.Customs.SG.IJobDeclaration>();
			var wrapper = DocumentWrapperFactory.CreateCustomsContainerWrapperWithDeclaration(cusContainer, declaration, CountryCodes.Singapore);
			AssertNotNull("SGCusContainer Wrapper created", wrapper);

			var wrapperType = wrapper.GetType().FullName;
			AssertEquals("Enterprise.Customs.SG.V4.Business.DocCusContainer", wrapperType);
		}

		void AssertCustomsWrapperType<T>(string countryCode, string type, DataContext dataContext = DataContext.Declaration) where T : class
		{
			var businessObject = Factory.New<T>() as BusinessObject;
			var wrapper = DocumentWrapperFactory.CreateCustomsWrapper(dataContext, businessObject, countryCode);
			AssertEquals($"Wrapper for {countryCode} Customs {dataContext}", type, wrapper?.GetType().FullName);
		}

		public class TestDocumentBusinessObjectWrapper : DocumentWrapper
		{
			public static TestDocumentBusinessObjectWrapper New(BusinessObject bizObj, BusinessObjectFactory factory)
			{
				return new TestDocumentBusinessObjectWrapper();
			}

			public override string ToString()
			{
				return ZString.Empty;
			}
		}

		class TestClientHook : ClientHook
		{
			public override Clients Client
			{
				get { return new Clients(); }
			}

			public override string ClientDisplayName
			{
				get { return null; }
			}
		}

		class DummyGenericWrapperLoader : GenericWrapperLoader
		{
			public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			{
				throw new InvalidCastException();
			}

			public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
			{
				throw new InvalidCastException();
			}

			public override Type GetWrapperType()
			{
				return null;
			}
		}
	}
}
