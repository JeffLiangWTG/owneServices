using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDetentionInvoice))]
	sealed class FreightWrapperFromDetentionInvoiceTest : FreightWrapperTest
	{
		public override void TestContainerLayoutStyle()
		{
			FreightWrapper[] wrappers = FreightWrapper.New(WrappedBO, Factory);

			SetupBusinessObjectForContainerLayoutStyleTest(WrappedBO);

			AssertEquals("NoContainers", wrappers[0].ContainerLayoutStyle);

			CommonContainer container1 = AddContainer(WrappedBO);
			wrappers = FreightWrapper.New(WrappedBO, Factory);

			AssertEquals("SingleContainer", wrappers[0].ContainerLayoutStyle);

			CommonContainer container2 = AddContainer(WrappedBO);
			wrappers = FreightWrapper.New(WrappedBO, Factory);

			AssertEquals("MultipleContainersSingleDetails: 2 Containers.", "MultipleContainersSingleDetails", wrappers[0].ContainerLayoutStyle);
		}

		public void TestAlternativeBranding()
		{
			var detention = Factory.New<ContainerDetentionForTest>();
			var wrapper = new FreightWrapperFromDetentionInvoice(detention, Factory);
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

			detention.AlternativeBrandingForTest = branding;

			AssertNotEquals("Precondition: alternate image size does not equal to default image size.", image.Size, branding.Image.Size);
			AssertNotEquals("Precondition: alternate brand name does not equal to default brand name.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), branding.BrandName);
			AssertNotEquals("Precondition: alternate brand email does not equal to default brand email.", GlbStaff.CurrentUser.GS_EmailAddress, branding.BrandEmailAddress);

			AssertEquals("Alternative company logo.", branding.Image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("Alternative brand name.", branding.BrandName.ToUpper(), wrapper.BrandName);
			AssertEquals("Alternative brand email.", branding.BrandEmailAddress, wrapper.BrandEmailAddress);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var detention = (ContainerDetention)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromDetentionInvoice(detention, Factory);

			AssertEquals("TrackingBusinessObjectPK", detention.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ContainerSummary", "20GP x 0" },
					{ "ContainerLayoutStyle", "SingleContainer" },
					{ "JobNumber", "JobNumber 1" },
					{ "JobNumberBarcodeText", "^ADI=JobNumber 1;;|" },
					{ "JobNumberBarcodeTextForFont", "È^ADI=JobNumber¯1;;|'Ê" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈJobNumber¯1*Ê" },
					{ "JobNumberHeading", "Job Number" },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
InvoicingJob : JobNumber 1
JobHeaderLocalClient : #1
Principal : PRINCIPAL\nAUSTRALIA";
			}
		}

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			ContainerDetention detention = (ContainerDetention)parent;
			string containerNum = string.Format("C{0}", detention.Movements.Count);

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = containerNum;
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;

			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = containerNum;

			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;

			detention.Movements.Add(movement);

			return container;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var detention = (ContainerDetention)GetNewBusinessObjectToWrap();
			detention.NC_JobNumber = "JobNumber 1";
			AddContainer(detention);

			new Job.Loader(detention).TryLoadOrCreate();

			detention.NC_OH_Principal = GetOrgHeader("PRINCIPAL").PK;

			return new FreightWrapperFromDetentionInvoice(detention, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			detention.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;

			return detention;
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Test Classes

		class ContainerDetentionForTest : ContainerDetention, IEDocsProvider
		{
			public ContainerDetentionForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new DocumentSupporter DocumentSupporter
			{
				get { return new ContainerDetentionDocumentSupporterForTest(this); }
			}

			public DocumentBrandingBusinessObject AlternativeBrandingForTest { get; set; }
		}

		class ContainerDetentionDocumentSupporterForTest : DocumentSupporter
		{
			public ContainerDetentionDocumentSupporterForTest(ContainerDetentionForTest bizObj)
				: base(bizObj)
			{
			}

			public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
			{
				return ((ContainerDetentionForTest)BusinessObject).AlternativeBrandingForTest;
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.INVALID; }
			}

			#region Not Implemented

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
