using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromContainerReleaseInstance))]
	sealed class FreightWrapperFromContainerReleaseInstanceTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var shipment = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(shipment, false);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);
			var wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("TrackingBusinessObjectPK", shipment.PK, wrapper.TrackingBusinessObjectPK);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFull()
		{
			#region Setup

			var principal = Factory.NewWithValidTestData<OrgHeader>();

			var exportAgent = GetOrgHeader("McLaren");
			var exportPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			exportPort.O5_PortOrCountry = "UAIEV";
			exportPort.O5_OA_AgentOfficeAddress = exportAgent.MainAddress.PK;

			var importAgent = GetOrgHeader("Manchester United");
			var importPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			importPort.O5_PortOrCountry = "AUSYD";
			importPort.O5_OA_AgentOfficeAddress = importAgent.MainAddress.PK;

			var receivingForwarderDocAddress = Factory.New<JobDocAddress>();
			receivingForwarderDocAddress.E2_CompanyName = "McLaren";
			receivingForwarderDocAddress.DocAddressType = DocAddressType.ReceivingForwarderAddress;

			var sendingForwarderDocAddress = Factory.New<JobDocAddress>();
			sendingForwarderDocAddress.E2_CompanyName = "Manchester United";
			sendingForwarderDocAddress.DocAddressType = DocAddressType.SendingForwarderAddress;

			var agencyBooking = Factory.New<AgencyBooking>();
			agencyBooking.DocAddresses.Add(receivingForwarderDocAddress);
			agencyBooking.DocAddresses.Add(sendingForwarderDocAddress);
			agencyBooking.JS_OH_DeliveryAgent = principal.PK;
			agencyBooking.JS_RL_NKOrigin = "UAIEV";
			agencyBooking.JS_RL_NKDestination = "AUSYD";

			var releaseHeader = new ReleaseHeader(agencyBooking, false);
			var releaseInstance = new ReleaseInstance(releaseHeader);

			#endregion

			var fullWrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Manchester United", fullWrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "McLaren", fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", "McLaren", fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", "Manchester United", fullWrapper.SendingForwarder.CompanyName);

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		public override void TestJobHeaderLocalClient()
		{
			AssertEquals("JobHeaderLocalClient", ZString.Empty, Wrapper.JobHeaderLocalClient.ToString());
		}

		public void TestLogAgainstShipment()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(shipment, false);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);
			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("BusinessObjectToLogAgainst", shipment, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);
			AssertEquals("ParentBusinessObject", shipment, ((IBODocDataProvider)wrapper).ParentBusinessObject);
		}

		public void TestJobNumber()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_CFSReference = "TEST123456";
			AgencyShipmentContainer container = booking.BookedContainers.AddNew();
			OrgAddress org = Factory.New<OrgAddress>();
			container.JC_OA_DepartureContainerYardAddress = org.PK;
			ReleaseDetail detail = new ReleaseDetail(container);
			detail.ReleaseCount = 1;
			ReleaseHeader header = new ReleaseHeader(booking, false);
			header.Details.Add(detail);

			header.DoRelease(new NotificationsHandler());
			AssertEquals("Release header instances", 1, header.Instances.Count);

			ReleaseInstance releaseInstance = header.Instances[0];
			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);
			AssertEquals("JobNumber", "TEST123456-1", wrapper.JobNumber);
		}

		public void TestJobNumberHeading()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(booking, true);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);
			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("JobNumberHeading", "Release Number", wrapper.JobNumberHeading);
		}

		public void TestSecondaryNumber()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00001000";
			ReleaseHeader header = new ReleaseHeader(booking, true);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);
			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("SecondaryNumber", "V00001000", wrapper.SecondaryNumber);
		}

		public void TestSecondaryHeading()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(booking, true);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);
			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("SecondaryHeading", "Shipment Number", wrapper.SecondaryHeading);
		}

		public void TestConsignee()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "AWESOMSYD";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			ReleaseHeader header = new ReleaseHeader(shipment, true);
			ReleaseInstance releaseInstance = new ReleaseInstance(header);

			FreightWrapperFromContainerReleaseInstance wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);

			AssertEquals("Consignee", "AWESOMSYD", wrapper.Consignee.CompanyCode);
		}

		public override void TestWrapperNotes()
		{
			AssertEquals("Notes", 1, Wrapper.Notes.Count);
		}

		public void TestAlternativeBranding()
		{
			var shipment = Factory.New<AgencyBooking>();
			var header = new ReleaseHeader(shipment, false);
			var releaseInstance = new ReleaseInstanceForTest(header);
			var wrapper = new FreightWrapperFromContainerReleaseInstance(releaseInstance, Factory);
			var image = new Bitmap(1, 1);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, image);
			AssertEquals("Default company logo without branding.", image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("Default brand name without branding.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), wrapper.BrandName);
			AssertEquals("Default brand email without branding.", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);

			var branding = new PrincipalBranding();
			branding.Code = "PB1";
			branding.BrandName = "Brand 1";
			branding.BrandEmailAddress = "generic@brand1.com";
			branding.Image = new Bitmap(2, 2);

			releaseInstance.AlternativeBrandingForTest = branding;

			AssertNotEquals("Precondition: alternate image size does not equal to default image size.", image.Size, branding.Image.Size);
			AssertNotEquals("Precondition: alternate brand name does not equal to default brand name.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), branding.BrandName);
			AssertNotEquals("Precondition: alternate brand email does not equal to default brand email.", GlbStaff.CurrentUser.GS_EmailAddress, branding.BrandEmailAddress);

			AssertEquals("Alternative company logo.", branding.Image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("Alternative brand name.", branding.BrandName.ToUpper(), wrapper.BrandName);
			AssertEquals("Alternative brand email.", branding.BrandEmailAddress, wrapper.BrandEmailAddress);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Release Number" },
					{ "SecondaryHeading", "Shipment Number" },
				};
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromContainerReleaseInstance(instance, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			ReleaseHeader header = new ReleaseHeader(shipment, true);
			header.Init();
			return header.Instances.AddNew();
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			instance = GetNewBusinessObjectToWrap() as ReleaseInstance;
		}

		ReleaseInstance instance;

		#endregion

		#region Test Classes

		class ReleaseInstanceForTest : ReleaseInstance, IDocumentSupportable
		{
			public ReleaseInstanceForTest(ReleaseHeader header)
				: base(header)
			{
			}

			public new DocumentSupporter DocumentSupporter
			{
				get { return new ReleaseInstanceDocumentSupporterForTest(this); }
			}

			public DocumentBrandingBusinessObject AlternativeBrandingForTest { get; set; }
		}

		class ReleaseInstanceDocumentSupporterForTest : DocumentSupporter
		{
			public ReleaseInstanceDocumentSupporterForTest(ReleaseInstanceForTest bizObj)
				: base(bizObj)
			{
			}

			public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
			{
				return ((ReleaseInstanceForTest)BusinessObject).AlternativeBrandingForTest;
			}

			#region Not Implemented

			public override BusinessContext BusinessContext
			{
				get { throw new NotImplementedException(); }
			}

			public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				throw new NotImplementedException();
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}

			#endregion //Not Implemented
		}

		#endregion
	}
}
