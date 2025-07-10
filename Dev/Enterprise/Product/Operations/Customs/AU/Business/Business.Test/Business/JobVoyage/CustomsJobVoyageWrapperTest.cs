using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsJobVoyageWrapper))]
	sealed class CustomsJobVoyageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIDocManagerSupportMembers()
		{
			var docManagerSupporter = Wrapper as IDocManagerSupport;
			AssertType(typeof(DocManagerInfo), docManagerSupporter.DocManagerInfo);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var destination = Voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUXXX";
			destination.JB_E_ARV = new ZDateTime(2012, 02, 13);
			var origin = Voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USXXX";
			origin.JA_A_DEP = new ZDateTime(2012, 02, 10);
			Voyage.JV_VoyageFlight = "12345";
			var fileNameProvider = Wrapper as IDataExportCSVFileNameProvider;
			AssertEquals("File name suffix", "12345_USXXX_120210_AUXXX_120213", fileNameProvider.FileNameSuffix);
		}

		public void TestIMessageManageableBizObj()
		{
			IMessageManageableBizObj bizObj = Wrapper;
			AssertEquals("MessageManager", typeof(JobVoyageMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestFactory()
		{
			AssertEquals("Factory", Voyage.Factory, Wrapper.Factory);
		}

		public void TestImpendingArrivalStatus()
		{
			AssertEquals("Default Code", CMRBaseStatuses.Codes.NotSent, Wrapper.ImpendingArrivalStatus.Code);
		}

		public void TestLoad()
		{
			Wrapper.Factory.Save();
			AssertNotNull(CustomsJobVoyageWrapper.Load(new BusinessObjectFactory(), Voyage.PK));
		}

		public void TestMessages()
		{
			AssertEquals("Messages", Voyage.Messages, Wrapper.Messages);
		}

		public void TestFlightNo()
		{
			Voyage.JV_VoyageFlight = "QF123";
			AssertEquals("FlightNo", "QF123", Wrapper.FlightNo);
		}

		public void TestPortOfFirstArrival()
		{
			SetupTwoOriginsAndTwoDestinations();
			AssertEquals("PortOfFirstArrival", "AUSYD", Wrapper.PortOfFirstArrival);
		}

		public void TestDateTimeOfDepartureUTC()
		{
			SetupTwoOriginsAndTwoDestinations();
			AssertEquals("PortOfFirstArrival", new ZDateTime(2005, 6, 5, 23, 28, 0), Wrapper.DateTimeOfDepartureUTC);
			Voyage.Origins[0].JA_RL_NKPortOfLoading = ZString.Empty;
			Voyage.Origins[1].JA_RL_NKPortOfLoading = ZString.Empty;
			AssertEquals("PortOfFirstArrival", ZDateTime.Empty, Wrapper.DateTimeOfDepartureUTC);
		}

		public void TestLastOverseasPortOfDeparture()
		{
			SetupTwoOriginsAndTwoDestinations();
			AssertEquals("LastOverseasPortOfDeparture", "USLAX", Wrapper.LastOverseasPortOfDeparture);
		}

		public void TestLines()
		{
			SetupTwoOriginsAndTwoDestinations();
			AssertEquals("Lines.Length", 2, Wrapper.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			SetupTwoOriginsAndTwoDestinations();
			AssertEquals("Lines.Length", 0, Wrapper.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("Lines.Length", 2, Wrapper.DatabaseLines.Length);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, ((IStatusNeedsRecalculationProvider)Wrapper).StatusNeedsRecalculation);
			Wrapper.Messages.AddNew().EM_MessageText = "12#";
			AssertEquals("StatusNeedsRecalculation", true, ((IStatusNeedsRecalculationProvider)Wrapper).StatusNeedsRecalculation);
		}

		public void TestDetails()
		{
			Voyage.JV_VoyageFlight = "QF123";
			AssertEquals("Details", "Flight: QF123\r\n", ((ICMRMessageRespondee)Wrapper).Details);
		}

		public void TestShortDescription()
		{
			Voyage.JV_VoyageFlight = "QF123";
			AssertEquals("ShortDescription", "Flight: QF123", ((ICMRMessageRespondee)Wrapper).ShortDescription);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CustomsJobVoyageWrapperValidation), Wrapper.Validation.GetType());
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";
		public void TestABNSpacesStripped()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABNNumberWithSpaces;
			AssertEquals("Wrapper Responsible Party ID", ABNNumberWithOutSpaces, Wrapper.ResponsiblePartyID);
		}

		#region Implementation

		void SetupTwoOriginsAndTwoDestinations()
		{
			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";
			destination2.JB_E_ARV = new ZDateTime(2005, 7, 1, 8, 20, 0);
			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_E_ARV = new ZDateTime(2005, 7, 1, 5, 20, 0);
			destination1.JB_A_ARV = new ZDateTime(2005, 7, 1, 5, 40, 0);

			VoyageOrigin origin2 = Voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			origin2.JA_A_DEP = new ZDateTime(2005, 6, 5, 16, 28, 0);
			VoyageOrigin origin1 = Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "USNYC";
			origin1.JA_A_DEP = new ZDateTime(2005, 6, 5, 12, 28, 0);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Wrapper;
		}

		CustomsJobVoyageWrapper wrapper;
		CustomsJobVoyageWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = new CustomsJobVoyageWrapper(Voyage);
				}
				return wrapper;
			}
		}

		JobVoyage voyage;
		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
				}
				return voyage;
			}
		}

		#endregion
	}
}
