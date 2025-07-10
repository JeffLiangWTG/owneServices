using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class NoFiltersReportFRDeclarationsTest : ReportFRDeclarationsTest
	{
		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "BRK";
			declarant.GS_FullName = "Broker Name";

			expectedDeclaration.JE_GS_NKCusAgent = declarant.GS_Code;

			var registeredEventTime = Factory.New<StmALog>();
			using (registeredEventTime.LockForUpdatingKeyFieldsForTesting())
			{
				registeredEventTime.SL_Parent = expectedDeclaration.PK;
				registeredEventTime.SL_SE_NKEvent = Events.CustomsClearedCode;
				registeredEventTime.SL_EventTime = new ZDate(2020, 11, 12);
			}

			var entryHeader = expectedDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "100";

			var entryInstruction1 = expectedDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "10";
			var entryInstruction2 = expectedDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "31P";

			var invoice1 = expectedDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			var invoice2 = expectedDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.InvoiceLines.AddNew();

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPL";
			supplier.OH_FullName = "Supplier Full Name";
			supplier.CustomsCodes.AddNew(EuropeanUnionSharedCodeTypes.Eori, "331597005", Core.Constants.CountryCodes.France);
			supplier.CustomsCodes.AddNew(FranceCodeTypes.EoriBranchSuffix, "00064", Core.Constants.CountryCodes.France);

			expectedDeclaration.JE_OH_Supplier = supplier.PK;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "Importer Full Name";

			expectedDeclaration.JE_OH_Importer = importer.PK;
			expectedDeclaration.JE_ApplicationCode = "DG";
			expectedDeclaration.JE_MessageType = "IMP";
			expectedDeclaration.JE_GoodsOrigin = "CN";
			expectedDeclaration.JE_RL_NKFinalDestination = "FR";
			expectedDeclaration.JE_TotalWeight = 1000;
			expectedDeclaration.JE_CustomsProfile = "Profile";
			expectedDeclaration.JE_DeltaMode = "D";
			expectedDeclaration.JE_TransportMode = "SEA";
			expectedDeclaration.JE_ShipmentIncoTerm = "FOB";
			expectedDeclaration.JE_ShipmentIncoTermPlace = "BEGERAC";
			expectedDeclaration.JE_TransportModeInland = "RAI";

			var cauOffice = expectedDeclaration.CustomsOffices.Cast<EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "CAU") ?? expectedDeclaration.CustomsOffices.AddNew();
			cauOffice.CY_Type = "EUO";
			cauOffice.CY_Code = "CAU";
			cauOffice.CY_Data = "FR123456";

			var entOffice = expectedDeclaration.CustomsOffices.Cast<EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "ENT") ?? expectedDeclaration.CustomsOffices.AddNew();
			entOffice.CY_Type = "EUO";
			entOffice.CY_Code = "ENT";
			entOffice.CY_Data = "FR654321";

			expectedDeclaration.JE_TotalWeightUnit = "KG";
			expectedDeclaration.JE_TotalVolume = 900;
			expectedDeclaration.JE_TotalVolumeUnit = "LT";
			expectedDeclaration.JE_TotalNoOfPacks = 3;
			expectedDeclaration.JE_TotalNoOfPacksPackType = "CT";
			expectedDeclaration.CusContainers.AddNew();
			expectedDeclaration.JE_RL_NKOrigin = "CN";
			expectedDeclaration.JE_DateAtOrigin = new ZDateTime(2020, 11, 01);
			expectedDeclaration.JE_RL_NKPortOfFirstArrival = "FRMRS";
			expectedDeclaration.JE_DateAtFinalDestination = new ZDateTime(2020, 12, 01);
			expectedDeclaration.JE_ExportDate = new ZDateTime(2020, 10, 4);
			expectedDeclaration.JE_DateOfArrival = new ZDateTime(2020, 11, 20);
			expectedDeclaration.JE_DateOfFirstArrival = new ZDateTime(2020, 11, 19);
			expectedDeclaration.JE_HouseBill = "HBill";
			expectedDeclaration.JE_MasterBill = "MBill";
			expectedDeclaration.JE_VesselName = "Black Pearl";
			expectedDeclaration.JE_VoyageFlightNo = "FLYNO1234";
			expectedDeclaration.JE_RL_NKPortOfArrival = "FRMRS";
			expectedDeclaration.JE_ContainerMode = "LSE";
			expectedDeclaration.JE_GoodsDestination = "A";

			var jobLoader = new JobHeader.Loader(expectedDeclaration);
			var jobHeader = jobLoader.TryCreate();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentID = expectedDeclaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_Status = "WRK";

			Factory.Save();
		}

		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string, string)[]
			{
				("JE_PK", expectedDeclaration.PK.ToString()),
				("JE_GB", GlbBranch.CurrentBranch.PK.ToString()),
				("JobNumber", "B0000FR"),
				("CreatedTime", expectedDeclaration.JE_SystemCreateTimeUtc.ToString("s")),
				("BrokerDeclarant", "Broker Name"),
				("RegisteredTime", new ZDate(2020, 11, 12).ToString("s")),
				("EntryStatus", "BAE"),
				("CustomsOfficeCAU", "FR123456"),
				("CustomsOfficeENT", "FR654321"),
				("DeltaMode", "D"),
				("JE_CustomsProfile", "Profile"),
				("TransportMode", "SEA"),
				("DeclarationType", "10/31P"),
				("ContainerMode", "LSE"),
				("TransportModeInternal", "RAI"),
				("BranchCode", GlbBranch.CurrentBranch.GB_Code),
				("BranchName", GlbBranch.CurrentBranch.GB_BranchName),
				("JobType", "B"),
				("InvoicesCount", expectedDeclaration.Invoices.Count.ToString()),
				("InvoiceLinesCount", 3.ToString()),
				("Incoterm", "FOB"),
				("IncotermPlace", "BEGERAC"),
				("SupplierEori","331597005"),
				("SupplierEoriSuffix","00064"),
				("SupplierCode","SUPPL"),
				("SupplierName","Supplier Full Name"),
				("ImporterCode","IMPORTER"),
				("ImporterName", "Importer Full Name"),
				("JE_ApplicationCode", "DG"),
				("MessageType", "IMP"),
				("JE_OH_Supplier", expectedDeclaration.JE_OH_Supplier.ToString()),
				("JE_OH_Importer", expectedDeclaration.JE_OH_Importer.ToString()),
				("CountryOfSupply","CN"),
				("CountryOfDestination", "FR"),
				("TotalWeight","1000.000"),
				("TotalWeightUQ","KG"),
				("TotalVolume", "900.000"),
				("TotalVolumeUQ", "LT"),
				("TotalNoOfPacks", "3"),
				("TotalNoOfPacksPackType", "CT"),
				("ContainerCount", "1"),
				("Origin", "CN"),
				("OriginETD" ,expectedDeclaration.JE_DateAtOrigin.ToString("s")),
				("FirstArrival", "FRMRS"),
				("DestETA", expectedDeclaration.JE_DateAtFinalDestination.ToString("s")),
				("LoadADT", expectedDeclaration.JE_ExportDate.ToString("s")),
				("ArrivalATA" , expectedDeclaration.JE_DateOfArrival.ToString("s")),
				("FirstArrivalATA", expectedDeclaration.JE_DateOfFirstArrival.ToString("s")),
				("HouseBill", "HBill"),
				("MasterBill", "MBill"),
				("Vessel", "Black Pearl"),
				("VoyageFlight", "FLYNO1234"),
				("PortOfDischarge", "FRMRS"),
				("JobStatus", "WRK"),
				("ShipmentPK", null),
				("GoodsDestination", expectedDeclaration.JE_GoodsDestination)
			};
		}

		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();
	}

	#region Filter By Test Classes

	class FilterByCompanyPkReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "NC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "NB";

			Factory.New<JobDeclaration>().JE_GB = newBranch.PK;
		}
	}

	class FilterByBranchPkReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@BranchPk", GlbBranch.CurrentBranch.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var newBranchInCurrentCompany = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().SingleOrDefault(x => x.GB_Code == "TES") ?? GlbCompany.CurrentCompany.Branches.AddNew();
			newBranchInCurrentCompany.GB_Code = "TB1";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "NC1";
			var branchInNewCompany = newCompany.Branches.AddNew();
			branchInNewCompany.GB_Code = "TB2";
			Factory.Save();

			Factory.New<JobDeclaration>().JE_GB = newBranchInCurrentCompany.PK;
			Factory.New<JobDeclaration>().JE_GB = branchInNewCompany.PK;
		}
	}

	class FilterByContainerModeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ContainerMode", "CNT");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_ContainerMode = "CNT";

			Factory.New<JobDeclaration>();
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByMessageTypeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@MessageType", "IMP");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_MessageType = "IMP";

			var exportDeclaration1 = Factory.New<JobDeclaration>();
			exportDeclaration1.JE_MessageType = "EXP";
			var exportDeclaration2 = Factory.New<JobDeclaration>();
			exportDeclaration2.JE_MessageType = "EXP";
		}
	}

	class FilterByTransportModeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@TransportMode", "SEA");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_TransportMode = "SEA";

			var airDeclaration1 = Factory.New<JobDeclaration>();
			airDeclaration1.JE_TransportMode = "AIR";
			var trainDeclaration = Factory.New<JobDeclaration>();
			trainDeclaration.JE_MessageType = "RAI";
		}
	}

	class FilterByCreateDateReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CreateDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@CreateDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_SystemCreateTimeUtc = new ZDateTime(2020, 11, 01);

			var oldDeclaration1 = Factory.New<JobDeclaration>();
			oldDeclaration1.JE_SystemCreateTimeUtc = new ZDateTime(2019, 11, 01);
			var oldDeclaration2 = Factory.New<JobDeclaration>();
			oldDeclaration2.JE_SystemCreateTimeUtc = new ZDateTime(2021, 11, 01);
		}
	}

	class FilterByImporterReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ImporterPk", expectedDeclaration?.JE_OH_Importer.ToString() ?? "NULL");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			expectedDeclaration.JE_OH_Importer = importer.PK;

			var otherImporter = Factory.New<OrgHeader>();
			otherImporter.OH_Code = "OTHER IMP";

			var otherDeclaration1 = Factory.New<JobDeclaration>();
			otherDeclaration1.JE_OH_Importer = otherImporter.PK;
			Factory.New<JobDeclaration>();
		}
	}

	class FilterBySupplierReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@SupplierPk", expectedDeclaration?.JE_OH_Supplier.ToString() ?? "NULL");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			expectedDeclaration.JE_OH_Supplier = supplier.PK;

			var otherImporter = Factory.New<OrgHeader>();
			otherImporter.OH_Code = "OTHER SUPPL";

			var otherDeclaration1 = Factory.New<JobDeclaration>();
			otherDeclaration1.JE_OH_Supplier = otherImporter.PK;
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByDepartureDateReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@DepartureDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@DepartureDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_ExportDate = new ZDateTime(2020, 11, 01);

			var oldDeclaration1 = Factory.New<JobDeclaration>();
			oldDeclaration1.JE_ExportDate = new ZDateTime(2019, 11, 01);
			var oldDeclaration2 = Factory.New<JobDeclaration>();
			oldDeclaration2.JE_ExportDate = new ZDateTime(2021, 11, 01);
		}
	}

	class FilterByArrivalDateReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ArrivalDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@ArrivalDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_DateOfArrival = new ZDateTime(2020, 11, 01);

			var oldDeclaration1 = Factory.New<JobDeclaration>();
			oldDeclaration1.JE_DateOfArrival = new ZDateTime(2019, 11, 01);
			var oldDeclaration2 = Factory.New<JobDeclaration>();
			oldDeclaration2.JE_DateOfArrival = new ZDateTime(2021, 11, 01);
		}
	}

	class FilterByAssessmentDateReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@AssessmentDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@AssessmentDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var entryInstruction1 = expectedDeclaration.CustomsEntryInstructions.Any<CusEntryInstruction>() ? expectedDeclaration.CustomsEntryInstructions[0] : expectedDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2020, 11, 01);

			var otherDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction2 = otherDeclaration.CustomsEntryInstructions.Any<CusEntryInstruction>() ? otherDeclaration.CustomsEntryInstructions[0] : otherDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_DateForDuty = new ZDateTime(2019, 11, 01);
		}
	}

	class FilterByCpcReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CPC", "40");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var entryInstruction1 = expectedDeclaration.CustomsEntryInstructions.Any<CusEntryInstruction>() ? expectedDeclaration.CustomsEntryInstructions[0] : expectedDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "40";
			expectedDeclaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = entryInstruction1.PK;

			var otherDeclaration1 = Factory.New<JobDeclaration>();
			var entryInstruction2 = otherDeclaration1.CustomsEntryInstructions.Any<CusEntryInstruction>() ? otherDeclaration1.CustomsEntryInstructions[0] : otherDeclaration1.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "13P";
			otherDeclaration1.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = entryInstruction2.PK;

			var otherDeclaration2 = Factory.New<JobDeclaration>();
			var entryInstruction3 = otherDeclaration2.CustomsEntryInstructions.Any<CusEntryInstruction>() ? otherDeclaration2.CustomsEntryInstructions[0] : otherDeclaration2.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "20P";
		}
	}

	class FilterByEntryStatusReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@EntryStatus", "BAE");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.CustomsEntryHeaders.AddNew().CH_EntryStatus = "BAE";

			Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().CH_EntryStatus = "ERR";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByMessageStatusReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@MessageStatus", "AWO");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.CustomsEntryHeaders.AddNew().CH_Status = "AWO";

			Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().CH_Status = "ERR";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByCustomsProfileReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CustomsProfile", "Profile1");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_CustomsProfile = "Profile1";

			Factory.New<JobDeclaration>().JE_CustomsProfile = "Profile2";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByDeltaModeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@DeltaMode", "G");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_DeltaMode = "G";

			Factory.New<JobDeclaration>().JE_DeltaMode = "F";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByPortOfDischargeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@PortOfDischarge", "ITMRS");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedDeclaration.JE_RL_NKPortOfArrival = "ITMRS";

			Factory.New<JobDeclaration>().JE_RL_NKPortOfArrival = "ITVCE";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByDeltaReferenceReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@DeltaReference", "DELTA-REF");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var entryHeader = expectedDeclaration.CustomsEntryHeaders.AddNew();
			var deltaNumber = Factory.New<CusEntryNumber>();
			deltaNumber.CE_ParentID = entryHeader.PK;
			deltaNumber.CE_ParentTable = "CH";
			deltaNumber.CE_Category = "CUS";
			deltaNumber.CE_EntryType = expectedDeclaration.JE_MessageType;
			deltaNumber.CE_EntryNum = "DELTA-REF";

			Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByCustomsOfficeReportFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CustomsOffice", "FR654321");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			expectedDeclaration.JE_CustomsOffice = "FR654321";

			Factory.New<JobDeclaration>().JE_CustomsOffice = "IT123456";
			Factory.New<JobDeclaration>();
		}
	}

	class FilterByCustomsPortOfLoadingFRDeclarationsTest : FilterByBaseReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@PortOfLoading", "FRMRS");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			expectedDeclaration.JE_RL_NKPortOfLoading = "FRMRS";

			Factory.New<JobDeclaration>().JE_RL_NKPortOfLoading = "ITVCE";
			Factory.New<JobDeclaration>();
		}
	}

	#endregion

	#region ReportFRDeclarationsTest

	abstract class ReportFRDeclarationsTest : FRReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_FRDeclarations";

		protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
		{
			yield return (typeof(Guid), "JE_PK");
			yield return (typeof(Guid), "JE_GB");
			yield return (typeof(string), "JobNumber");
			yield return (typeof(DateTime), "CreatedTime");
			yield return (typeof(string), "BrokerDeclarant");
			yield return (typeof(DateTime), "RegisteredTime");
			yield return (typeof(string), "EntryStatus");
			yield return (typeof(string), "CustomsOfficeCAU");
			yield return (typeof(string), "CustomsOfficeENT");
			yield return (typeof(string), "DeltaMode");
			yield return (typeof(string), "JE_CustomsProfile");
			yield return (typeof(string), "TransportMode");
			yield return (typeof(string), "DeclarationType");
			yield return (typeof(string), "ContainerMode");
			yield return (typeof(string), "TransportModeInternal");
			yield return (typeof(string), "BranchCode");
			yield return (typeof(string), "BranchName");
			yield return (typeof(string), "JobType");
			yield return (typeof(int), "InvoicesCount");
			yield return (typeof(int), "InvoiceLinesCount");
			yield return (typeof(string), "Incoterm");
			yield return (typeof(string), "IncotermPlace");
			yield return (typeof(string), "SupplierEori");
			yield return (typeof(string), "SupplierEoriSuffix");
			yield return (typeof(string), "SupplierCode");
			yield return (typeof(string), "SupplierName");
			yield return (typeof(string), "ImporterCode");
			yield return (typeof(string), "ImporterName");
			yield return (typeof(string), "JE_ApplicationCode");
			yield return (typeof(string), "MessageType");
			yield return (typeof(Guid), "JE_OH_Supplier");
			yield return (typeof(Guid), "JE_OH_Importer");
			yield return (typeof(string), "CountryOfSupply");
			yield return (typeof(string), "CountryOfDestination");
			yield return (typeof(decimal), "TotalWeight");
			yield return (typeof(string), "TotalWeightUQ");
			yield return (typeof(decimal), "TotalVolume");
			yield return (typeof(string), "TotalVolumeUQ");
			yield return (typeof(int), "TotalNoOfPacks");
			yield return (typeof(string), "TotalNoOfPacksPackType");
			yield return (typeof(int), "ContainerCount");
			yield return (typeof(string), "Origin");
			yield return (typeof(DateTime), "OriginETD");
			yield return (typeof(string), "FirstArrival");
			yield return (typeof(DateTime), "DestETA");
			yield return (typeof(DateTime), "LoadADT");
			yield return (typeof(DateTime), "ArrivalATA");
			yield return (typeof(DateTime), "FirstArrivalATA");
			yield return (typeof(string), "HouseBill");
			yield return (typeof(string), "MasterBill");
			yield return (typeof(string), "Vessel");
			yield return (typeof(string), "VoyageFlight");
			yield return (typeof(string), "PortOfDischarge");
			yield return (typeof(string), "JobStatus");
			yield return (typeof(Guid), "ShipmentPK");
			yield return (typeof(string), "GoodsDestination");
		}

		protected virtual IEnumerable<string> GetParametersValuesList()
		{
			var populatedFilterParameters = GetPopulatedFilterParameters().ToDictionary(x => x.ParameterName, x => x.ParameterValue);

			var parameterNameList = GetParameterNameList();
			foreach (var parameterName in parameterNameList)
			{
				yield return populatedFilterParameters.ContainsKey(parameterName) ? $"'{populatedFilterParameters[parameterName]}'" : "NULL";
			}
		}

		protected override void PrepareTestData()
		{
			expectedDeclaration = Factory.New<JobDeclaration>();

			expectedDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			expectedDeclaration.JE_DeclarationReference = "B0000FR";
		}
		protected JobDeclaration expectedDeclaration;

		protected sealed override IEnumerable<string> GetParameterNameList()
		{
			yield return "@CompanyPk";
			yield return "@BranchPk";
			yield return "@MessageType";
			yield return "@TransportMode";
			yield return "@ContainerMode";
			yield return "@CreateDateFrom";
			yield return "@CreateDateTo";
			yield return "@ImporterPk";
			yield return "@SupplierPk";
			yield return "@DepartureDateFrom";
			yield return "@DepartureDateTo";
			yield return "@ArrivalDateFrom";
			yield return "@ArrivalDateTo";
			yield return "@AssessmentDateFrom";
			yield return "@AssessmentDateTo";
			yield return "@CPC";
			yield return "@EntryStatus";
			yield return "@MessageStatus";
			yield return "@CustomsProfile";
			yield return "@DeltaMode";
			yield return "@PortOfDischarge";
			yield return "@DeltaReference";
			yield return "@CustomsOffice";
			yield return "@PortOfLoading";
		}
	}

	abstract class FilterByBaseReportFRDeclarationsTest : ReportFRDeclarationsTest
	{
		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string ColumnName, string Value)[]
			{
			   ("JE_PK", expectedDeclaration.PK.ToString())
			};
		}
	}

	#endregion
}
