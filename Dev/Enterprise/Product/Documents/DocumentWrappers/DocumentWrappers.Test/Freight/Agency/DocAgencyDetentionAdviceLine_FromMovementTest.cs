using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyDetentionAdviceLine_FromMovement))]
	sealed class DocAgencyDetentionAdviceLine_FromMovementTest : DocAgencyDetentionAdviceLineTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			DocAgencyDetentionAdviceLine wrapper = new DocAgencyDetentionAdviceLine_FromMovement(Movement, Factory);
			AssertEquals("", wrapper.ContainerNo);
			AssertEquals(null, wrapper.ContainerType);
			AssertEquals("", wrapper.VesselName);
			AssertEquals("", wrapper.VoyageNo);
			AssertEquals(null, wrapper.DetentionPort);
			AssertEquals("", wrapper.BillNumber);
			AssertEquals(ZDateTime.Empty, wrapper.ReleaseDate);
			AssertEquals(ZDateTime.Empty, wrapper.RequiredDate);
			AssertEquals(ZDateTime.Empty, wrapper.PickupDate);
		}

		public void TestContainerNo()
		{
			Stock.R6_ContainerNum = "TEST4300011";
			AssertEquals("TEST4300011", Wrapper.ContainerNo);
		}

		public void TestVesselName()
		{
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Movement.E9_JV = Voyage.PK;
			AssertEquals("MAJAPAHIT", Wrapper.VesselName);
		}

		public void TestVoyageNo()
		{
			Voyage.JV_VoyageFlight = "777";
			Movement.E9_JV = Voyage.PK;
			AssertEquals("777", Wrapper.VoyageNo);
		}

		public void TestReleaseDate()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			Movement.E9_MovementDate = now.AddDays(-5);

			AssertEquals(now.AddDays(-5), Wrapper.ReleaseDate);
		}

		public void TestRequiredDate()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			OrgHeader client = Factory.New<OrgHeader>();

			OrgContainerDetention detention = principal.CarrierContainerPenalties.AddNew();
			detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			detention.PD_OH_Client = client.PK;
			detention.PD_FreeDays = 5;

			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			Movement.E9_MovementDate = now.AddDays(-9);
			Movement.E9_OH_Principal = principal.PK;
			Movement.E9_OH_ResponsibleParty = client.PK;

			AssertEquals(now.AddDays(-4), Wrapper.RequiredDate);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ContainerType : 20GP - Twenty foot general purpose
DetentionPort :  is null
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Voyage.JV_VoyageFlight = "777";

			Stock.R6_ContainerNum = "TEST4300011";
			Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Movement.E9_JV = Voyage.PK;

			return new DocAgencyDetentionAdviceLine_FromMovement(Movement, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
DocAgencyDetentionAdviceLine_FromMovement
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
			return new DocAgencyDetentionAdviceLine_FromMovement(Movement, Factory);
		}

		new DocAgencyDetentionAdviceLine Wrapper
		{
			get { return new DocAgencyDetentionAdviceLine_FromMovement(Movement, Factory); }
		}

		#endregion
	}
}
