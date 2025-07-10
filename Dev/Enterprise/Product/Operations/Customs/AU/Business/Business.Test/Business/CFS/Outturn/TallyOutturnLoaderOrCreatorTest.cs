using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class TallyOutturnLoaderOrCreatorTest : TestCaseWithFactory
	{
		public void TestCurrentBranchPremiseID()
		{
			AssertEquals("9495C", creator.CurrentBranchPremiseID);
			var currentBranchOrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			try
			{
				creator.ClearCachedValues();
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertEquals("9495C", creator.CurrentBranchPremiseID);
				creator.ClearCachedValues();
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = currentBranchOrgProxy;
				AssertEquals("9495C", creator.CurrentBranchPremiseID);
				creator.ClearCachedValues();
				GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "12345";
				AssertEquals("12345", creator.CurrentBranchPremiseID);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = currentBranchOrgProxy;
				GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = ZString.Empty;
			}
		}

		#region Finding

		public void TestFindOutturn()
		{
			GetCorrectOutturn(GetIncorrectPremiseIDHeader());
			GetCorrectOutturn(GetIncorrectVesselHeader());
			GetCorrectOutturn(GetIncorrectVoyageHeader());

			var correctHeader = GetCorrectHeader();
			GetIncorrectCargoTypeOutturn(correctHeader);
			GetIncorrectContainerOutturn(correctHeader);
			var correctOutturn = GetCorrectOutturn(correctHeader);

			var foundOutturn = creator.FindOutturn();
			AssertEquals(correctOutturn, foundOutturn);

			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			creator = new TallyOutturnLoaderOrCreator(container);
			foundOutturn = creator.FindOutturn();
			AssertNull(foundOutturn);
			correctHeader.C6_OutturningPremiseID = "9920A";
			foundOutturn = creator.FindOutturn();
			AssertEquals(correctOutturn, foundOutturn);
		}

		public void TestFindHeader()
		{
			TallyOutturnHeader header = GetCorrectHeader();

			TallyOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn.C5_ContainerNumber = "BBBB2222227";
			outturn.C5_HouseBill = "HOUSE9394";

			AssertNull(creator.FindOutturn());
			AssertEquals(header, creator.FindHeader());
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.ContainerHasNoConsolException))]
		public void TestFindHeaderNoConsol()
		{
			TallyOutturn correctOutturn = GetCorrectOutturn(GetCorrectHeader());

			consol.Containers.RemoveAll();
			AssertNull("precondition", container.Consol);

			creator.FindHeader();
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.ContainerHasNoConsolException))]
		public void TestFindOutturnNoConsol()
		{
			TallyOutturn correctOutturn = GetCorrectOutturn(GetCorrectHeader());

			consol.Containers.RemoveAll();
			AssertNull("precondition", container.Consol);

			creator.FindOutturn();
		}

		public void TestFindOutturnIncorrect()
		{
			GetCorrectOutturn(GetIncorrectPremiseIDHeader());
			GetCorrectOutturn(GetIncorrectVesselHeader());
			GetCorrectOutturn(GetIncorrectVoyageHeader());

			TallyOutturnHeader correctHeader = GetCorrectHeader();
			GetIncorrectCargoTypeOutturn(correctHeader);
			GetIncorrectContainerOutturn(correctHeader);

			AssertNull(creator.FindOutturn());
		}

		public void TestFindHeaderIncorrect()
		{
			GetCorrectOutturn(GetIncorrectPremiseIDHeader());
			GetCorrectOutturn(GetIncorrectVesselHeader());
			GetCorrectOutturn(GetIncorrectVoyageHeader());

			AssertNull(creator.FindHeader());
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.FoundTooManyMatchesException))]
		public void TestFindOutturnMoreThanOne()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			GetCorrectOutturn(header);
			GetCorrectOutturn(header);

			creator.FindOutturn();
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.FoundTooManyMatchesException))]
		public void TestFindOutturnMoreThanOneHeader()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			GetCorrectHeader();
			GetCorrectOutturn(header);

			creator.FindOutturn();
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.FoundTooManyMatchesException))]
		public void TestFindHeaderMoreThanOne()
		{
			GetCorrectHeader();
			GetCorrectHeader();

			creator.FindHeader();
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.AlreadyLinkedException))]
		public void TestFindOutturnThatIsLinkedToSomethingElse()
		{
			TallyOutturn outturn = GetCorrectOutturn(GetCorrectHeader());
			TallyContainer otherContainer = Factory.New<TallyContainer>();
			CFSTallyContainerWrapper.Load(otherContainer).Outturns.Add(outturn);
			TallyOutturn foundOutturn = creator.FindOutturn(); // Exception should be thrown here
			CFSTallyContainerWrapper.Load(container).Outturns.Add(outturn);
		}

		#endregion

		#region Creating

		public void TestCreateOutturnAndHeader()
		{
			TallyOutturn outturn = creator.CreateOutturnAndHeader();
			outturn.Notes.ClearAllNotifications();
			var header = outturn.Header;

			AssertEquals("ADMIRALENGRACHT", header.C6_VesselName);
			AssertEquals("54321S", header.C6_VoyageNum);
			AssertEquals("9495C", header.C6_OutturningPremiseID);

			AssertEquals(CMRImportCargoTypes.Codes.FullContainerLoad, outturn.C5_CargoType);
			AssertEquals("BBBB2222227", outturn.C5_ContainerNumber);
			AssertEquals(ZString.Empty, outturn.C5_HouseBill);
			AssertEquals(ZString.Empty, outturn.C5_MasterBill);

			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			Factory.Save();

			header.Validation.ValidateAll();
			AssertEquals(false, header.HasNotifications());

			outturn.Validation.ValidateAll();
			AssertEquals(false, outturn.HasNotifications());

			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			creator = new TallyOutturnLoaderOrCreator(container);
			outturn = creator.CreateOutturnAndHeader();
			header = outturn.Header;
			AssertEquals("ADMIRALENGRACHT", header.C6_VesselName);
			AssertEquals("54321S", header.C6_VoyageNum);
			AssertEquals("9920A", header.C6_OutturningPremiseID);
		}

		public void TestCreateOutturn()
		{
			TallyOutturn outturn = creator.CreateOutturn(GetCorrectHeader());
			outturn.Validation.ValidateAll();
			AssertEquals(false, outturn.HasNotifications());

			AssertEquals(CMRImportCargoTypes.Codes.FullContainerLoad, outturn.C5_CargoType);
			AssertEquals("BBBB2222227", outturn.C5_ContainerNumber);
			AssertEquals(ZString.Empty, outturn.C5_HouseBill);
			AssertEquals(ZString.Empty, outturn.C5_MasterBill);
		}

		[ExpectException(typeof(TallyOutturnLoaderOrCreator.ContainerHasNoConsolException))]
		public void TestCreateOutturnAndHeaderNoConsol()
		{
			consol.Containers.RemoveAll();
			AssertNull("precondition", container.Consol);

			creator.CreateOutturnAndHeader();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<TallyContainer>();
			creator = new TallyOutturnLoaderOrCreator(container);

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9495C";

			consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.MainTransport.JW_Vessel = "ADMIRALENGRACHT";
			consol.MainTransport.JW_RL_NKDiscPort = "AUSYD";
			consol.MainTransport.JW_RL_NKLoadPort = "NZAKL";
			consol.MainTransport.JW_VoyageFlight = "54321S";

			consol.Containers.Add(container);
			container.JC_ContainerNum = "BBBB2222227";
		}

		CFSLoadListConsol consol;
		TallyContainer container;
		TallyOutturnLoaderOrCreator creator;

		#region Test Data Providers

		TallyOutturnHeader GetCorrectHeader()
		{
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			header.C6_VesselName = "ADMIRALENGRACHT";
			header.C6_OutturningPremiseID = "9495C";
			header.C6_VoyageNum = "54321S";
			return header;
		}

		TallyOutturnHeader GetIncorrectVesselHeader()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			header.C6_VesselName = "ANL EXPLORER";
			return header;
		}

		TallyOutturnHeader GetIncorrectVoyageHeader()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			header.C6_VoyageNum = "183848S";
			return header;
		}

		TallyOutturnHeader GetIncorrectPremiseIDHeader()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			header.C6_OutturningPremiseID = "9914N";
			return header;
		}

		TallyOutturn GetCorrectOutturn(TallyOutturnHeader header)
		{
			TallyOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "BBBB2222227";
			return outturn;
		}

		TallyOutturn GetIncorrectContainerOutturn(TallyOutturnHeader header)
		{
			TallyOutturn outturn = GetCorrectOutturn(header);
			outturn.C5_ContainerNumber = "AAAA1111113";
			return outturn;
		}

		TallyOutturn GetIncorrectCargoTypeOutturn(TallyOutturnHeader header)
		{
			TallyOutturn outturn = GetCorrectOutturn(header);
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			return outturn;
		}

		#endregion

		#endregion
	}
}
