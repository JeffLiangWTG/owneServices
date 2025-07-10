using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocBaseWrapperWithJobHeaderTestClass : TestCaseWithFactory
	{
		#region Job Details

		public void TestGenericInvoicingJob()
		{
			AssertEquals(null, Wrapper.GenericInvoicingJobForTest);

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			Job header = orgFactory.NewJobWithValidTestDataForTesting<Job>();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			header.LocalChargesPK = client.PK;
			Wrapper.SetJobHeader(header);

			AssertNotEquals(null, Wrapper.GenericInvoicingJobForTest);
		}

		public void TestMode()
		{
			AssertEquals(ZString.Empty, Wrapper.ModeForTest);

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment = orgFactory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "S0001000";

			Job header = new Job.Loader(shipment).TryCreate();

			orgFactory.Save();

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			header.PlugInData = shipment;
			header.LocalChargesPK = client.PK;
			orgFactory.Save();
			Wrapper.SetJobHeader(header);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(Core.Constants.TransportModes.Air, Wrapper.ModeForTest);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(Core.Constants.ContainerModes.Liquid, Wrapper.ModeForTest);
		}

		public void TestServiceDirection()
		{
			AssertEquals(ZString.Empty, Wrapper.ServiceDirectionForTest);

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment = orgFactory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "S0001000";

			Job header = new Job.Loader(shipment).TryCreate();

			orgFactory.Save();

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			header.PlugInData = shipment;
			header.LocalChargesPK = client.PK;
			orgFactory.Save();
			Wrapper.SetJobHeader(header);

			ZString anotherCountryCode;
			if (GlbBranch.CurrentBranch.HomePort.Code.Left(2) == "GB")
			{
				anotherCountryCode = "AUSYD";
			}
			else
			{
				anotherCountryCode = "GBLON";
			}

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(OrgConstants.ServiceDirection.Code.Domestic, Wrapper.ServiceDirectionForTest);

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			shipment.JS_RL_NKDestination = anotherCountryCode;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(OrgConstants.ServiceDirection.Code.Export, Wrapper.ServiceDirectionForTest);

			shipment.JS_RL_NKOrigin = anotherCountryCode;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(OrgConstants.ServiceDirection.Code.Import, Wrapper.ServiceDirectionForTest);

			shipment.JS_RL_NKOrigin = anotherCountryCode;
			shipment.JS_RL_NKDestination = anotherCountryCode;
			orgFactory.Save();

			RecreateWrapper(header);
			AssertEquals(OrgConstants.ServiceDirection.Code.CrossTrade, Wrapper.ServiceDirectionForTest);
		}

		public void TestJobHeader()
		{
			AssertNull("No JobHeader", Wrapper.JobHeader);

			var header = Factory.NewJobForTesting<JobHeader>();
			Wrapper.SetJobHeader(header);
			AssertNotNull("With JobHeader", Wrapper.JobHeader);
		}

		#endregion

		#region Branding

		public void TestCompanyLogoWithUsingLoginBranchLogoForFreight()
		{
			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			Wrapper.SetJobHeader(header);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, header.JH_GB.ToGuid(), Guid.Empty, new Bitmap(2, 2));
			Assert("The default value of the registry item (Use Logo Based On Login Branch For Freight) is false.", !SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.Value);
			AssertEquals("The wrapper company logo should be Controlling Branch Logo.", new Size(2, 2), Wrapper.CompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(3, 3));
			AssertEquals("The wrapper company logo  is still the old one because of the value of the registry item (Use Logo Based On Login Branch For Freight) is false.", new Size(2, 2), Wrapper.CompanyLogo.Size);

			SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("The wrapper company logo  should be Login Branch Logo.", new Size(3, 3), Wrapper.CompanyLogo.Size);
		}

		public void TestBranchLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertNull("No branch logo", Wrapper.BranchLogoExposed);

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_GB = Env.CurrentBranch.PK;
			Wrapper.SetJobHeader(header);
			AssertNull("No branch logo", Wrapper.BranchLogoExposed);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(2, 2));
			AssertEquals("Branch logo", new Size(2, 2), Wrapper.BranchLogoExposed.Size);
		}

		public void TestGetHouseBillBrandImage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Default is null", null, Wrapper.GetHouseBillBrandImage(null, null, DocumentsDataRegistry.Instance.HBLAgentBrandingImage));

			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);

			ForwardingConsol consol = shipment.Consols.AddNew();
			OrgHeader receivingForwarder = OrgHeader.New(Factory);
			receivingForwarder.MiscServ.OM_FWAgentCategory = "STD";

			OrgHeader cnee = OrgHeader.New(Factory);
			cnee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);

			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			shipment.ConsigneePK = cnee.PK;
			DocOrganisation fWDWrapper = DocOrganisation.New(receivingForwarder, Factory);
			DocOrganisation cneeWrapper = DocOrganisation.New(cnee, Factory);
			BrandingTestHelperClass.SetHybridBrandRegistryImage(DocumentsDataRegistry.Instance.HBLAgentBrandingImage);

			Wrapper = new DocWrapperForTest(shipment, Factory);
			Image result = Wrapper.GetHouseBillBrandImage(fWDWrapper, cneeWrapper, DocumentsDataRegistry.Instance.HBLAgentBrandingImage);
			AssertEquals("Brand image from registry", new Size(5, 5), result.Size);

			DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			Wrapper = new DocWrapperForTest(shipment, Factory);
			result = Wrapper.GetHouseBillBrandImage(fWDWrapper, cneeWrapper, DocumentsDataRegistry.Instance.HBLAgentBrandingImage);
			AssertEquals("Should return brand image at cnee's tariff level", new Size(9, 9), result.Size);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			CreateNewWrapper();
			base.SetUp();
		}

		DocWrapperForTest Wrapper;

		void CreateNewWrapper()
		{
			Wrapper = new DocWrapperForTest(Factory.New<ForwardingShipment>(), Factory);
		}

		void RecreateWrapper(Job job)
		{
			Wrapper = RecreateTestingDocWrapper((ForwardingShipment)job.PlugInData, (bizo, factory) => new DocWrapperForTest(bizo, factory));
			Wrapper.SetJobHeader(job);
		}

		TResult RecreateTestingDocWrapper<T, TResult>(T bizo, Func<T, BusinessObjectFactory, TResult> createWrapper)
			where T : BusinessObject
			where TResult : DocumentWrapper
		{
			Factory.Save();
			bizo.Factory.Save();
			ReleaseFactory();
			return createWrapper(Factory.Load<T>(bizo.PK), Factory);
		}

		public class DocWrapperForTest : DocBaseWrapperWithJobHeader
		{
			public DocWrapperForTest(object objectToWrap, BusinessObjectFactory factory)
				: base(objectToWrap, factory)
			{
			}

			public override DocJobHeader JobHeader
			{
				get { return fJobHeader; }
			}

			public void SetJobHeader(JobHeader jobHeader)
			{
				fJobHeader = DocJobHeader.New(jobHeader, Factory);
			}

			public Image BranchLogoExposed
			{
				get { return base.JobHeaderBranchLogo; }
			}

			protected DocJobHeader fJobHeader;

			public DocJobInvoicingJob GenericInvoicingJobForTest
			{
				get { return base.GenericInvoicingJob; }
			}

			public ZString ModeForTest
			{
				get { return base.Mode; }
			}

			public ZString ServiceDirectionForTest
			{
				get
				{
					ZString origin = GenericInvoicingJob == null ? ZString.Empty : GenericInvoicingJob.Origin;
					ZString destination = GenericInvoicingJob == null ? ZString.Empty : GenericInvoicingJob.Destination;
					return base.GetServiceDirection(origin, destination);
				}
			}
		}

		#endregion
	}
}
