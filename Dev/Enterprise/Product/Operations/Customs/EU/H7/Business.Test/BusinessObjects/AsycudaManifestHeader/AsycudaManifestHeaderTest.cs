using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : AsycudaManifestHeaderAbstractTest
	{
		public void TestSetDefaultValues()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			CombineAssertions("default values", () =>
			{
				AssertEquals("Application code", "LVC", header.AMA_ApplicationCode);
				AssertEquals("Manifest type", "EH7", header.AMA_ManifestType);
				AssertEquals("Declarant", header.Branch.OrgProxy.MainAddress.PK, header.AMA_OA_Declarant);
			});
		}

		public void TestCaptions()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			CombineAssertions(() =>
			{
				AssertDataBoundCaptions(header.AMA_RN_NKCountryInfo, "Member State", "Member State", "Member St.", "ISO 3166-1 alpha-2 code of the EU Member State the declaration is submitted to.");
				AssertDataBoundCaptions(header.AMA_GBInfo, "Branch", "Branch", "Br.", "Company branch code associated with the declaration.");
				AssertDataBoundCaptions(header.AMA_TransportModeInfo, "Transport Mode", "Trans. Mode", "Mode", "The transport mode for the shipment.");
				AssertDataBoundCaptions(header.AMA_VoyageInfo, "Flight/Voyage", "Fl./Voy.", "F/V", "The flight or voyage number associated with the shipment.");
				AssertDataBoundCaptions(header.AMA_VesselNameInfo, "Vessel", "Vessel", "Vessel", "Name of vessel.");
				AssertDataBoundCaptions(header.AMA_LloydsNumberInfo, "Vessel IMO Number", "Vessel IMO No.", "IMO No.", string.Empty);
				AssertDataBoundCaptions(header.AMA_RadioCallSignInfo, "Radio Call Sign", "Call Sign", "Call Sign", "Maritime call sign assigned to a vessel used for radio transmissions.");
				AssertDataBoundCaptions(header.AMA_RN_NKConveyanceNationalityInfo, "Conveyance Country/Region", "Convey. Ctry/Rgn.", "Conv. Ctry/Rgn.", "The country/region of conveyance.");
				AssertDataBoundCaptions(header.AMA_VehicleRegistrationInfo, "Vehicle Registration", "Vehicle Reg.", "Vehicle", "The registration number of the vehicle the shipment is transported on.");
				AssertDataBoundCaptions(header.AMA_Trailer1RegNoInfo, "Trailer 1", "Tr. 1", "Tr. 1", "The registration number of the trailer attached to the vehicle the shipment is transported on (if applicable).");
				AssertDataBoundCaptions(header.AMA_RN_NKTrailer1RegCountryInfo, "Trailer 1 Country", "Tr. 1 Country", "Tr. 1 Ctry.", "The issuing country of the registration number of the trailer attached to the vehicle the shipment is transported on (if applicable).");
				AssertDataBoundCaptions(header.AMA_Trailer2RegNoInfo, "Trailer 2", "Tr. 2", "Tr. 2", "The registration number of the second trailer attached to the vehicle the shipment is transported on (if applicable).");
				AssertDataBoundCaptions(header.AMA_RN_NKTrailer2RegCountryInfo, "Trailer 2 Country", "Tr. 2 Country", "Tr. 2 Ctry.", "The issuing country of the registration number of the second trailer attached to the vehicle the shipment is transported on (if applicable).");
				AssertDataBoundCaptions(header.AMA_RL_NKPortOfLoadingInfo, "Load Port", "Load Port", "Load", "The place where shipments are loaded and secured aboard a vessel/aircraft. It may or may not be the same as the Port of Origin.");
				AssertDataBoundCaptions(header.AMA_E_DEPInfo, "Estimated Departure Date", "Est. Departure", "ETD", string.Empty);
				AssertDataBoundCaptions(header.AMA_RL_NKPortOfFirstArrivalInfo, "Port Of First Arrival", "First Arrival", "1st Arr. Port", "First Port of Arrival");
				AssertDataBoundCaptions(header.AMA_RL_NKPortOfDischargeInfo, "Discharge Port", "Discharge", "Disc.", "A place where a ship, vehicle, aircraft, cargo, emergency service, or person discharges or unloads some or all of its shipments.");
				AssertDataBoundCaptions(header.AMA_E_ARVInfo, "Estimated Date of Arrival", "Est. Arrival", "ETA", string.Empty);
				AssertDataBoundCaptions(header.AMA_A_ARVInfo, "Actual Date of Arrival", "Act. Arrival", "ATA", string.Empty);
				AssertDataBoundCaptions(header.AMA_CustomsOfficeInfo, "Customs Office", "Cus. Office", "Cus. Off.", "Code identifying the Customs Office associated with the declaration.");
				AssertDataBoundCaptions(header.PresentationOfficeInfo, "Presentation Office", "Pres. Office", "Pres. Off.", "Code identifying the Customs Office of goods presentation.");
				AssertDataBoundCaptions(header.AMA_OA_DeclarantInfo, "Declarant", "Declarant", "Decl.", "The importer/legal declarant submitting the declaration.");
				AssertDataBoundCaptions(header.AMA_OA_RepresentativeInfo, "Representative", "Representative", "Rep.", "The representative (agent) submitting the declaration on the importer's/legal declarant's behalf.");
				AssertDataBoundCaptions(header.AMA_AgentTypeInfo, "Representative Status", "Rep. Status", "Rep. Status", "The relevant code representing the status of the representative.");
				AssertDataBoundCaptions(header.AMA_PaymentMethodInfo, "Payment Method", "Payment Method", "Payment Method", "The method of payment for any applicable duties/taxes.");
				AssertDataBoundCaptions(header.AMA_ApplicationCodeInfo, "Submit Type", "Submit Type", "Submit Type", "The declaration specification version of the relevant Customs authority used to submit the declaration.");
				AssertDataBoundCaptions(header.AMA_OA_PresenterInfo, "Presenter", "Pres.", "Pres.", "The presenter of the goods.");
			});
		}

		public void TestVoyageFlightNoLabel()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_TransportMode = TransportModes.Sea;
			Assert("Precondition: transport mode is Sea", header.IsSea);
			AssertCaptions(header.VoyageFlightNoLabel, "Voyage", "Voyage", "Voy.", "Unique identifier assigned to a specific journey or trip undertaken by a vessel.");

			header.AMA_TransportMode = TransportModes.Air;
			Assert("Precondition: transport mode is Sea", header.IsAir);
			AssertCaptions(header.VoyageFlightNoLabel, "Flight", "Flight", "Fl.", "The flight number the shipment is transported on.");

			header.AMA_TransportMode = TransportModes.Rail;
			Assert("Precondition: transport mode is Rail", header.IsRail);
			AssertCaptions(header.VoyageFlightNoLabel, "Journey", "Journey", "Jrny.", "The Journey number associated with the shipment.");

			header.AMA_TransportMode = TransportModes.Road;
			Assert("Precondition: transport mode is neither air or sea or rail", !(header.IsAir || header.IsSea || header.IsRail));
			AssertCaptions(header.VoyageFlightNoLabel, "Flight/Voyage/Journey", "Fl./Voy./Jrny.", "F/V/J", "The flight, voyage or journey number associated with the shipment.");
		}

		void AssertDataBoundCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertCaptions(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, expectedFullDescription, propertyInfo.Name);
		}

		static void AssertCaptions(ResourceStringData resourceStringData, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription, string assertionMessage = "")
		{
			AssertEquals($"{assertionMessage} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{assertionMessage} MediumCaption", expectedMediumCaption, resourceStringData.MediumCaption);
			AssertEquals($"{assertionMessage} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			AssertEquals($"{assertionMessage} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalDocument]);
				AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalDocuments()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<AdditionalDocumentCollection<AdditionalDocument>>(header.AdditionalDocuments);
		}

		public void TestPreviousDocuments()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<PreviousDocumentCollection<PreviousDocument>>(header.PreviousDocuments);
		}

		public void TestSupportingDocuments()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(header.SupportingDocuments);
		}

		public void TestPresentationOffice()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.PresentationOffice = "IETHR800";

			var presentationOfficeCode = header.PresentationOfficeCode;

			CombineAssertions(() =>
			{
				AssertEquals("AMA", presentationOfficeCode.CY_ParentTableCode);
				AssertEquals(header.PK, presentationOfficeCode.CY_ParentID);
				AssertEquals("PRE", presentationOfficeCode.CY_Code);
				AssertEquals("IETHR800", presentationOfficeCode.CY_Data);
			});

			header.PresentationOffice = ZString.Empty;
			Factory.Save();

			Assert("CusCodeData deleted as CY_Data is now empty", presentationOfficeCode.IsDeleted);
		}

		[TestDate(2023, 08, 15)]
		public void TestAMA_JobReference()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var header2 = GetNewBusinessObject() as AsycudaManifestHeader;
			Env.NumberFountains.EUH7ManifestJobReference.SetNext(Factory, 6789);
			header.OnSaving();
			AssertEquals("H7D00006789", header.AMA_JobReference);

			var customisation = EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization.Value;

			var yearElement = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit];
			yearElement.Include = true;
			yearElement.Order = 1;
			yearElement.Detail = "4";

			var monthElement = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits];
			monthElement.Include = true;
			monthElement.Order = 2;
			monthElement.Detail = "2";

			var sequenceElement = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber];
			sequenceElement.Include = true;
			sequenceElement.Order = 3;
			sequenceElement.Detail = "5";

			using (EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				header2.OnSaving();
				AssertEquals("H7D20230806790", header2.AMA_JobReference);
			}
		}

		public void TestAMA_PaymentAccountNumber()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_PaymentAccountNumber = "0000001";
			AssertEquals(35, header.AMA_PaymentAccountNumberInfo.MaxLength);
		}

		#region IRelatedJob Member Test

		public void TestIRelatedJobMembers()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_JobReference = "AA1111";
			header.RegistrationDate = new ZDateTime(2024, 1, 9);

			var relatedJob = header as IRelatedJob;

			CombineAssertions("IRelatedJob Members", () =>
			{
				AssertEquals("JobNumber", "AA1111", relatedJob.JobNumber);
				AssertEquals("HumanReadableName", "LV Manifest AA1111", relatedJob.JobDescription);
				AssertEquals("JobStatus", "REG", relatedJob.JobStatus);
			});
		}

		#endregion
		public void TestDataGrouping()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals("Precondition: header.DataGrouping", Constants.CountryCodes.Latvia, header.DataGrouping);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.FrenchGuyana))
			{
				var header2 = GetNewBusinessObject() as AsycudaManifestHeader;
				AssertEquals("header2.DataGrouping", Constants.CountryCodes.France, header2.DataGrouping);
			}
		}

		public void TestPersonsTabNotVisible()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("Persons Tab is hidden", false, header.NeedPersonsTabCore_Exposed);
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Persons Tab is hidden", false, header.NeedPersonsTabCore_Exposed);
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Persons Tab is hidden", false, header.NeedPersonsTabCore_Exposed);
			header.AMA_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("Persons Tab is hidden", false, header.NeedPersonsTabCore_Exposed);
		}

		public void TestRepresentativeStatusCode()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_AgentType = EUH7AgentTypes.Codes.DIR;
			AssertEquals("When Agent Type is Direct", "2", header.RepresentativeStatusCode);

			header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertEquals("When Agent Type is Indirect", "3", header.RepresentativeStatusCode);

			header.AMA_AgentType = "NOT";
			AssertEquals("When not DIR or IND", string.Empty, header.RepresentativeStatusCode);
		}

		public void TestCloneManifestHeader()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Ireland))
			{
				var header = SetupHeaderForClone();
				SetupBillForClone(header);
				Factory.Save();

				var clonedHeader = (AsycudaManifestHeader)header.Clone();

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("IE", clonedHeader.AMA_RN_NKCountry);
					AssertEquals(header.AMA_GB, clonedHeader.AMA_GB);
					AssertEquals("AIR", clonedHeader.AMA_TransportMode);
					AssertEquals(string.Empty, clonedHeader.AMA_Voyage);
					AssertEquals("GB2AB", clonedHeader.AMA_RL_NKPortOfLoading);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_E_DEP);
					AssertEquals("ADALV", clonedHeader.AMA_RL_NKPortOfFirstArrival);
					AssertEquals("IEORK", clonedHeader.AMA_RL_NKPortOfDischarge);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_E_ARV);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_A_ARV);
					AssertEquals(string.Empty, clonedHeader.MasterBill.ABL_BillNumber);
					AssertEquals(1, clonedHeader.Bills.Count);
					AssertEquals(header.AMA_OA_Declarant, clonedHeader.AMA_OA_Declarant);
					AssertEquals(header.AMA_OA_Representative, clonedHeader.AMA_OA_Representative);
					AssertEquals("DIR", clonedHeader.AMA_AgentType);
					AssertEquals("A", clonedHeader.AMA_PaymentMethod);
					AssertEquals("LV1", clonedHeader.AMA_ApplicationCode);
					AssertEquals("IEORK802", clonedHeader.AMA_CustomsOffice);
					AssertEquals("IEORK802", clonedHeader.PresentationOffice);

					var clonedBill = clonedHeader.Bills.Single();
					AssertEquals(string.Empty, clonedBill.ABL_BillNumber);
					AssertEquals("bill des", clonedBill.ABL_GoodsDescription);
				});
			}
		}

		AsycudaManifestHeader SetupHeaderForClone()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brnach = Factory.NewWithValidTestData<GlbBranch>();
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_GB = brnach.PK;
			header.AMA_TransportMode = "AIR";
			header.AMA_Voyage = "QN006";
			header.AMA_RL_NKPortOfLoading = "GB2AB";
			header.AMA_E_DEP = DateTime.Now;
			header.AMA_RL_NKPortOfFirstArrival = "ADALV";
			header.AMA_RL_NKPortOfDischarge = "IEORK";
			header.AMA_E_ARV = DateTime.Now;
			header.AMA_A_ARV = DateTime.Now;
			header.MasterBill.ABL_BillNumber = "bill123";
			header.AMA_OA_Declarant = org.MainAddress.PK;
			header.AMA_OA_Representative = org.MainAddress.PK;
			header.AMA_AgentType = "DIR";
			header.AMA_PaymentMethod = "A";
			header.AMA_ApplicationCode = "LV1";
			header.AMA_CustomsOffice = "IEORK802";
			header.PresentationOffice = "IEORK802";
			return header;
		}

		void SetupBillForClone(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "bill234";
			bill.ABL_GoodsDescription = "bill des";
		}
	}

	#region IWorkflowProvider Tests

	sealed class AsycudaManifestHeaderWorkflowProviderTest : MasterFiles.Business.Testing.WorkflowProviderTest<AsycudaManifestHeader, ProcessTaskCollection<EUH7AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>>
	{
		AsycudaManifestHeader asycudaManifestHeader => BusinessObject;

		public void TestGetTemplateSelectionCriteria()
		{
			AssertNotNull(((IWorkflowProvider)asycudaManifestHeader).GetTemplateSelectionCriteria());
			Assert("is ColumnValueRanker", ((IWorkflowProvider)asycudaManifestHeader).GetTemplateSelectionCriteria() is ColumnValueRanker);
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode;

		public void TestGetTemplateSelectionCriteria_CountryCode()
		{
			asycudaManifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Latvia;

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_SubType1 = Core.Constants.CountryCodes.Latvia;

			asycudaManifestHeader.Factory.Save();

			AssertEquals("Precondition; ProcessTaskTemplate has a Task", 1, ProcessTaskTemplate.WorkflowItems.Tasks.Count);
		}
	}

	#endregion

	sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool NeedPersonsTabCore_Exposed => base.NeedPersonsTabCore;
	}
}
