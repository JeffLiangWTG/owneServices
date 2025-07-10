using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocService))]
	sealed class DocServiceTest : DocumentWrapperTestCase
	{
		public void TestBusinesObjectToLogAgainst()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocBaseJobDeclaration declarationWrapper = DocBaseJobDeclaration.New(declaration, Factory);
			JobService declarationService = declaration.DocsAndCartage.Services.AddNew();
			DocService declarationServiceWrapper = DocService.New(declarationService, declarationWrapper, Factory);
			AssertEquals("((IBODocDataProvider)declarationServiceWrapper).BusinessObjectToLogAgainst", declaration, ((IBODocDataProvider)declarationServiceWrapper).BusinessObjectToLogAgainst);

			JobService standaloneService = Factory.New<JobService>();
			DocService standaloneServiceWrapper = DocService.New(standaloneService, Factory);
			AssertEquals("((IBODocDataProvider)standaloneServiceWrapper).BusinessObjectToLogAgainst", standaloneService, ((IBODocDataProvider)standaloneServiceWrapper).BusinessObjectToLogAgainst);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocService.New(AService, ShipmentWrapper, Factory)
				};
		}

		#region ZString Tests

		public void TestDescription()
		{
			AService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertEquals("Service Description is Fumigation", "Fumigation", ServiceWrapper.Description);
		}

		public void TestDetails()
		{
			AService.ES_ServiceNote = "Note123";
			AssertEquals("Service Note is Note123", "Note123", ServiceWrapper.Details);
		}

		public void TestReference()
		{
			AService.ES_References = "Ref23456";
			AssertEquals("Reference is Ref23456", "Ref23456", ServiceWrapper.Reference);
		}

		public void TestReferenceOrOrderRef()
		{
			AService.ES_References = "Ref23456";
			AssertEquals("Reference is Ref23456", "Ref23456", ServiceWrapper.ReferenceOrOrderRef);

			ServiceWrapper.SetReportNameForTesting("Authorization for Service");
			Shipment.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			AssertEquals("Should come from the shipment", "Orders", ServiceWrapper.ReferenceOrOrderRef);
		}

		public void TestReferenceOrOrderRefHeading()
		{
			AssertEquals("REFERENCE", ServiceWrapper.ReferenceOrOrderRefHeading);

			ServiceWrapper.SetReportNameForTesting("Authorization for Service");
			AssertEquals("ORDER NUMBERS / REFERENCE", ServiceWrapper.ReferenceOrOrderRefHeading);
		}

		#endregion

		#region ZDateTime Tests

		public void TestBookedDate()
		{
			AService.ES_Booked = ZDateTime.Today;
			AssertEquals("Booked date should be today", ZDateTime.Today, ServiceWrapper.BookedDate);
		}

		#endregion

		#region Wrapper Tests

		public void TestContractor()
		{
			AssertEquals("Contractor should be null", null, ServiceWrapper.Contractor);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "Org1";
			OrgAddress add1 = org1.MainAddress;
			add1.OA_Address1 = "Address1";

			AService.ES_OA_Location = org1.PK;
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)ServiceWrapper.Contractor.WrappedObject;
			AssertEquals("Contractor should be Location", org1.ToString(), intermediateWrapper.WrappedObject.ToString());
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Org2";
			OrgAddress add2 = org2.MainAddress;
			add2.OA_Address1 = "Address2";
			AService.ES_OH_Contractor = org2.PK;

			intermediateWrapper = (DocBaseWrapper)ServiceWrapper.Contractor.WrappedObject;
			AssertEquals("Contractor should be contractor", org2.ToString(), intermediateWrapper.WrappedObject.ToString());
		}

		public void TestLocation()
		{
			OrgHeader loc = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address2 = loc.Addresses.AddNew();
			AService.ES_OH_Contractor = loc.PK;

			AssertEquals("Location should be Main Address", ServiceWrapper.Location.WrappedObject, loc.MainAddress);

			AService.ES_OA_Location = address2.PK;
			AssertEquals("Location should be Specific Address", ServiceWrapper.Location.WrappedObject, address2);

			AService.ES_OH_Contractor = ZGuid.Empty;
			AService.ES_OA_Location = ZGuid.Empty;
			AssertNull("Location should be null", ServiceWrapper.Location);
		}

		public void TestRequestedBy()
		{
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)ServiceWrapper.RequestedBy.WrappedObject;
			AssertEquals("Should return the org proxy", GlbBranch.CurrentBranch.OrgProxy.ToString(), intermediateWrapper.WrappedObject.ToString());
		}

		#endregion

		#region Parents Test

		public void TestParent()
		{
			AssertEquals("Should return Shipment", ShipmentWrapper.ToString(), ServiceWrapper.Parent.ToString());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			AService = Shipment.DocsAndCartage.Services.AddNew();
			ShipmentWrapper = DocShipment.New(Shipment, Factory);

			ServiceWrapper = DocService.New(AService, ShipmentWrapper, Factory);

			base.SetUp();
		}

		DocService ServiceWrapper;
		JobService AService;
		ForwardingShipment Shipment;
		DocShipment ShipmentWrapper;

		#endregion

		#region DocumentText Test

		public void TestServiceDocumentTitle()
		{
			AService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			ServiceWrapper.SetReportNameForTesting("Request for Service");
			AssertEquals("Text should return Request for Service", "Request for Fumigation Service", ServiceWrapper.ServiceDocumentTitle);

			ServiceWrapper.SetReportNameForTesting("Authorization for Service");
			AssertEquals("Text should return Authorization for Service", "Authorization for Fumigation Service", ServiceWrapper.ServiceDocumentTitle);
		}

		public void TestServiceDocumentOpeningText()
		{
			string request = "Request";
			string authorise = "Authorise";
			DocumentsDataRegistry.Instance.RequestForServiceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, request);
			DocumentsDataRegistry.Instance.AuthorisationForServiceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authorise);
			AssertEquals("Text should be empty", "", ServiceWrapper.ServiceDocumentOpeningText);

			ServiceWrapper.SetReportNameForTesting("Request for Service");
			AssertEquals("Text should not be empty", request, ServiceWrapper.ServiceDocumentOpeningText);

			ServiceWrapper.SetReportNameForTesting("Authorization for Service");
			AssertEquals("Text should not be empty", authorise, ServiceWrapper.ServiceDocumentOpeningText);
		}

		public void TestServiceDocumentClosingText()
		{
			string request = "Request Closing";
			string authorise = "Authorise Closing";
			DocumentsDataRegistry.Instance.RequestForServiceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, request);
			DocumentsDataRegistry.Instance.AuthorisationForServiceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authorise);
			AssertEquals("Text should be empty", "", ServiceWrapper.ServiceDocumentClosingText);

			ServiceWrapper.SetReportNameForTesting("Request for Service");
			AssertEquals("Text should not be empty", request, ServiceWrapper.ServiceDocumentClosingText);

			ServiceWrapper.SetReportNameForTesting("Authorization for Service");
			AssertEquals("Text should not be empty", authorise, ServiceWrapper.ServiceDocumentClosingText);
		}

		#endregion
	}
}
