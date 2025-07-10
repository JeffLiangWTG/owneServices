using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyDetentionAdviceLine))]
	sealed class DocAgencyDetentionAdviceLine_FromContainerTest : DocAgencyDetentionAdviceLineTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			AssertEquals("", Wrapper.ContainerNo);
			AssertEquals(null, Wrapper.ContainerType);
			AssertEquals("", Wrapper.VesselName);
			AssertEquals("", Wrapper.VoyageNo);
			AssertEquals(null, Wrapper.DetentionPort);
			AssertEquals("", Wrapper.BillNumber);
			AssertEquals(ZDateTime.Empty, Wrapper.ReleaseDate);
			AssertEquals(ZDateTime.Empty, Wrapper.RequiredDate);
			AssertEquals(ZDateTime.Empty, Wrapper.PickupDate);
		}

		public void TestContainerNo()
		{
			Container.JC_ContainerNum = "TEST4300011";
			AssertEquals("TEST4300011", Wrapper.ContainerNo);
		}

		public void TestVesselName()
		{
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Bill.JS_JX = Sailing.PK;
			AssertEquals("MAJAPAHIT", Wrapper.VesselName);
		}

		public void TestVoyageNo()
		{
			Voyage.JV_VoyageFlight = "777";
			Bill.JS_JX = Sailing.PK;
			AssertEquals("777", Wrapper.VoyageNo);
		}

		public void TestBillNumber()
		{
			Bill.JS_HouseBill = "BillNumber";
			AssertEquals("BILLNUMBER", Wrapper.BillNumber);
		}

		public void TestReleaseDate()
		{
			var now = ZDateTime.Now.ToSmallDateTime();
			Sailing.Destination.JB_AvailabilityDate = now.AddDays(-5);
			Bill.JS_JX = Sailing.PK;

			AssertEquals(now.AddDays(-5), Wrapper.ReleaseDate);

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";

			var sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "AUMEL");
			sailing.Destination.JB_AvailabilityDate = now.AddDays(-4);

			var transport = Bill.TransportsIncludingRelated.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			AssertEquals(now.AddDays(-4), Wrapper.ReleaseDate);
		}

		public void TestRequiredDate()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			Container.JC_EmptyReturnedBy = now.AddDays(-4);

			AssertEquals(now.AddDays(-4), Wrapper.RequiredDate);
		}

		public void TestPickupDate()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			Bill.JS_JX = Sailing.PK;
			Stock.R6_ContainerNum = Container.JC_ContainerNum = "TEST4300011";
			Movement.E9_JV = Voyage.PK;
			Movement.E9_MovementDate = now.AddDays(-3);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			Movement.E9_OA_Depot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.With(oA_RL_NKRelatedPortCode: Voyage.Destinations[0].JB_RL_NKPortOfDischarge).PK;

			AssertEquals(now.AddDays(-3), Wrapper.PickupDate);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ContainerType : 20GP - Twenty foot general purpose
DetentionPort : AUSYD - Sydney
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Voyage.JV_VoyageFlight = "777";

			Bill.JS_JX = Sailing.PK;

			Container.JC_ContainerNum = "TEST4300011";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			return new DocAgencyDetentionAdviceLine_FromContainer(Container, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
DocAgencyDetentionAdviceLine_FromContainer
======================================================================
Name                                    Type
----------------------------------------------------------------------
ContainerType                           ContainerType
DetentionPort                           Location
BillNumber                              String
ContainerNo                             String
DetentionType                           String
PickupDate                              DateTime
ReleaseDate                             DateTime
RequiredDate                            DateTime
VesselName                              String
VoyageNo                                String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DocAgencyDetentionAdviceLine_FromContainer(Container, Factory);
		}

		BillOfLading Bill
		{
			get { return bill ?? (bill = Factory.New<BillOfLading>()); }
		}
		BillOfLading bill;

		BillOfLadingContainer Container
		{
			get { return container ?? (container = Bill.RealContainers.AddNew()); }
		}
		BillOfLadingContainer container;

		new DocAgencyDetentionAdviceLine Wrapper
		{
			get { return new DocAgencyDetentionAdviceLine_FromContainer(Container, Factory); }
		}

		#endregion
	}
}
