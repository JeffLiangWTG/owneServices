using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class NoFiltersReportFRInvoiceLinesTest : Report_FRInvoiceLinesTest
	{
		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string, string)[]
			{
				("LinePK", invoiceLine.PK.ToString()),
				("JobNumber", "B00001"),
				("JobCreatedDate", new ZDateTime(2020, 11, 23).ToString("s")),
				("BrokerDeclarant", "Broker Name"),
				("DeltaReference", "XXYYZZ"),
				("JobRegisteredDate", new ZDate(2020, 11, 24).ToString("s")),
				("BAEDateTime",  new ZDate(2020, 11, 25).ToString("s")),
				("EntryStatus", "BAE"),
				("CustomsOfficeCAU", "FR123456"),
				("CustomsOfficeENT", "FR654321"),
				("DeltaMode", "D"),
				("CustomsProfile", "Profile"),
				("TransportMode", "SEA"),
				("JobType", "IMP"),
				("Cpc", "10"),
				("ContainerMode", "CNT"),
				("TransportModeInland", "RAI"),
				("BranchCode", GlbBranch.CurrentBranch.GB_Code),
				("Standalone", "Y"),
				("InvoiceNo", "NO.123"),
				("InvoiceLineAmount", "2000.0000"),
				("InvoiceLineCurrency", "EUR"),
				("CurrencyRate", "1.000000000"),
				("IncoTerm", "FOB"),
				("IncoTermPlace", "SYDNEY"),
				("SupplierPK", supplier.PK.ToString()),
				("SupplierEori", "331597005"),
				("SupplierEoriSuffix", "00064"),
				("SupplierCode", "NATCARSYD"),
				("SupplierName", "NATURE CARE MANUFACTUREP/L"),
				("ImporterPK", importer.PK.ToString()),
				("ImporterCode", "SJCORP"),
				("ImporterName", "CORP SPJ"),
				("CountryOfSupply", "AU"),
				("CountryOfDestination", "FR"),
				("OriginCountry", "CN"),
				("PreferenceCode", "100"),
				("TotalWeight", "1000.000"),
				("TotalWeightUQ", "KG"),
				("Volume", "0.100"),
				("VolumeUQ", "M3"),
				("Tariff", "4820900000"),
				("JobStatus", "WRK"),
				("SummaryDeclaration", "XXX Z TEST60"),
				("ContainerID", "CNT1/CNT2"),
				("Origin", "AUSYD"),
				("OriginETD", new ZDateTime(2020, 10, 04).ToString("s")),
				("DestETA", new ZDateTime(2020, 12, 01).ToString("s")),
				("LoadADT",  new ZDateTime(2020, 10, 4).ToString("s")),
				("ArrivalATA", new ZDateTime(2020, 11, 20).ToString("s")),
				("FirstArrivalATA", new ZDateTime(2020, 11, 19).ToString("s")),
				("HouseBill", "HBill"),
				("MasterBill", "MBill"),
				("Vessel", "Black Pearl"),
				("VoyageFlight", "FLYNO1234"),
				("BranchPK", GlbBranch.CurrentBranch.PK.ToString()),
			};
		}

		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();

		protected override void PrepareTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = "IMP";
			declaration.JE_DeclarationReference = "B00001";
			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2020, 11, 23);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "BRK";
			declarant.GS_FullName = "Broker Name";

			declaration.JE_GS_NKCusAgent = declarant.GS_Code;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "100";
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryIsSystemGenerated = true;
			entryNumber.CE_EntryType = declaration.JE_MessageType;
			entryNumber.CE_EntryNum = "XXYYZZ";
			entryNumber.CE_IssueDate = new ZDate(2020, 11, 25);

			var registeredEventTime = Factory.New<StmALog>();
			using (registeredEventTime.LockForUpdatingKeyFieldsForTesting())
			{
				registeredEventTime.SL_Parent = declaration.PK;
				registeredEventTime.SL_SE_NKEvent = Events.CustomsClearedCode;
				registeredEventTime.SL_EventTime = new ZDate(2020, 11, 24);
			}

			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "NATCARSYD";
			supplier.OH_FullName = "NATURE CARE MANUFACTUREP/L";

			supplier.CustomsCodes.AddNew(EuropeanUnionSharedCodeTypes.Eori, "331597005", Core.Constants.CountryCodes.France);
			supplier.CustomsCodes.AddNew(FranceCodeTypes.EoriBranchSuffix, "00064", Core.Constants.CountryCodes.France);

			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OA_SupplierAddress = supplier.MainAddress.PK;

			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SJCORP";
			importer.OH_FullName = "CORP SPJ";

			declaration.JE_OH_Importer = importer.PK;

			invoiceLine.ZG_CountryOfSupply = "AU";
			declaration.JE_GoodsDestination = "FR";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_PrimaryPreference = "100";
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Volume = 0.1m;
			invoiceLine.JI_VolumeUQ = "M3";
			invoiceLine.JI_Tariff = "4820.90.00 00";

			var jobLoader = new JobHeader.Loader(declaration);
			var jobHeader = jobLoader.TryCreate();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_Status = "WRK";

			invoiceHeader.JZ_InvoiceNumber = "No.123";
			invoiceLine.JI_LinePrice = 2000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceCurrExRate = 1m;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = "SYDNEY";

			declaration.JE_CustomsProfile = "Profile";
			declaration.JE_DeltaMode = "D";
			declaration.JE_TransportMode = "SEA";

			invoiceLine.JI_Procedure = "10";
			declaration.JE_ContainerMode = "LSE";
			declaration.JE_TransportModeInland = "RAI";

			var cauOffice = declaration.CustomsOffices.Cast<EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "CAU") ?? declaration.CustomsOffices.AddNew();
			cauOffice.CY_Type = "EUO";
			cauOffice.CY_Code = "CAU";
			cauOffice.CY_Data = "FR123456";

			var entOffice = declaration.CustomsOffices.Cast<EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "ENT") ?? declaration.CustomsOffices.AddNew();
			entOffice.CY_Type = "EUO";
			entOffice.CY_Code = "ENT";
			entOffice.CY_Data = "FR654321";

			var container1 = declaration.CusContainers.AddNew();
			var container1InvoiceLine1 = invoiceLine.ContainersPivot.AddNew();
			container1.CO_ContainerNumber = "CNT1";
			container1InvoiceLine1.C2_CO = container1.PK;
			var container2 = declaration.CusContainers.AddNew();
			var container2InvoiceLine1 = invoiceLine.ContainersPivot.AddNew();
			container2.CO_ContainerNumber = "CNT2";
			container2InvoiceLine1.C2_CO = container2.PK;

			var xxxPreviousDocument = invoiceLine.PreviousDocuments.AddNew();
			xxxPreviousDocument.CSI_Code = "XXX";
			xxxPreviousDocument.CSI_SubType = "Z";
			xxxPreviousDocument.CSI_ReferenceNumber = "TEST60";

			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_DateAtOrigin = new ZDateTime(2020, 11, 01);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2020, 12, 01);
			declaration.JE_ExportDate = new ZDateTime(2020, 10, 4);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 11, 20);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2020, 11, 19);
			declaration.JE_HouseBill = "HBill";
			declaration.JE_MasterBill = "MBill";
			declaration.JE_VesselName = "Black Pearl";
			declaration.JE_VoyageFlightNo = "FLYNO1234";

			Factory.Save();
		}
		JobComInvoiceLine invoiceLine;
		OrgHeader supplier;
		OrgHeader importer;
	}

	#region Filter By Test Classes

	class FilterByCompanyPkReport_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
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
			var declarationFromAnotherBranch = Factory.New<JobDeclaration>();
			declarationFromAnotherBranch.JE_GB = newBranch.PK;

			declarationFromAnotherBranch.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}

	class FilterByCreateDate_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CreateDateFrom", new DateTime(2020, 11, 01).ToString("s"));
			yield return ("@CreateDateTo", new DateTime(2020, 11, 30).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			declaration.JE_SystemCreateTimeUtc = new ZDate(2020, 11, 25);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_SystemCreateTimeUtc = new ZDateTime(2020, 10, 20);
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_SystemCreateTimeUtc = new ZDateTime(2020, 10, 20);
			declaration3.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}

	class FilterByProductCode_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ProductCode", "80033432");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedInvoiceLine.JI_PartNo = "80033432";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "702231";

			invoiceHeader.InvoiceLines.AddNew();
		}
	}

	class FilterByProductDescription_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ProductDescription", "Wine");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			expectedInvoiceLine.JI_Description = "Wine and Spirits";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "Shoes";

			invoiceHeader.InvoiceLines.AddNew();
		}
	}

	class FilterByShipmnetType_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ShipmentType", "IMP");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			declaration.JE_MessageType = "IMP";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "EXP";
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = "COM";
			declaration3.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}

	class FilterByInvoiceNumber_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@InvoiceNumber", "0001");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			invoiceHeader.JZ_InvoiceNumber = "0001";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "0002";

			var declaration2 = Factory.New<JobDeclaration>();
			var invoice3 = declaration2.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "0003";
		}
	}

	class FilterByDeltaReference_FRInvoiceLinesTest : FilterByBaseReportFRInvoiceLinesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@DeltaReference", "0001DELTA");
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			void CreateCusEntryNum(CusEntryHeader cusEntryHeader, string entryNum)
			{
				var cusEntryNum = Factory.New<CusEntryNumber>();
				cusEntryNum.CE_ParentID = cusEntryHeader.PK;
				cusEntryNum.CE_ParentTable = cusEntryHeader.TablePrefix;
				cusEntryNum.CE_EntryNum = entryNum;
				cusEntryNum.CE_EntryType = cusEntryHeader.Declaration.JE_MessageType;
				cusEntryNum.CE_RN_NKCountryCode = "FR";
			}

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			expectedInvoiceLine.JI_CL = entryLine1.PK;
			CreateCusEntryNum(entryHeader1, "0001DELTA");

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			CreateCusEntryNum(entryHeader2, "0002DELTA");

			Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		}
	}

	abstract class FilterByBaseReportFRInvoiceLinesTest : Report_FRInvoiceLinesTest
	{
		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string, string)[]
			{
				("LinePK", expectedInvoiceLine.PK.ToString()),
			};
		}

		protected override void PrepareTestData()
		{
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			expectedInvoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}
		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine expectedInvoiceLine;
	}

	#endregion

	#region Report_FRInvoiceLinesTest

	abstract class Report_FRInvoiceLinesTest : FRReportFunctionalTestCase
	{
		protected sealed override ZString ObjectName => "Report_FRInvoiceLines";

		protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
		{
			yield return (typeof(Guid), "LinePK");
			yield return (typeof(string), "JobNumber");
			yield return (typeof(DateTime), "JobCreatedDate");
			yield return (typeof(string), "BrokerDeclarant");
			yield return (typeof(string), "DeltaReference");
			yield return (typeof(DateTime), "JobRegisteredDate");
			yield return (typeof(DateTime), "BAEDateTime");
			yield return (typeof(string), "EntryStatus");
			yield return (typeof(string), "CustomsOfficeCAU");
			yield return (typeof(string), "CustomsOfficeENT");
			yield return (typeof(string), "DeltaMode");
			yield return (typeof(string), "CustomsProfile");
			yield return (typeof(string), "TransportMode");
			yield return (typeof(string), "JobType");
			yield return (typeof(string), "Cpc");
			yield return (typeof(string), "ContainerMode");
			yield return (typeof(string), "TransportModeInland");
			yield return (typeof(string), "BranchCode");
			yield return (typeof(string), "Standalone");
			yield return (typeof(string), "InvoiceNo");
			yield return (typeof(decimal), "InvoiceLineAmount");
			yield return (typeof(string), "InvoiceLineCurrency");
			yield return (typeof(decimal), "CurrencyRate");
			yield return (typeof(string), "IncoTerm");
			yield return (typeof(string), "IncoTermPlace");
			yield return (typeof(Guid), "SupplierPK");
			yield return (typeof(string), "SupplierEori");
			yield return (typeof(string), "SupplierEoriSuffix");
			yield return (typeof(string), "SupplierCode");
			yield return (typeof(string), "SupplierName");
			yield return (typeof(Guid), "ImporterPK");
			yield return (typeof(string), "ImporterCode");
			yield return (typeof(string), "ImporterName");
			yield return (typeof(string), "CountryOfSupply");
			yield return (typeof(string), "CountryOfDestination");
			yield return (typeof(string), "OriginCountry");
			yield return (typeof(string), "PreferenceCode");
			yield return (typeof(decimal), "TotalWeight");
			yield return (typeof(string), "TotalWeightUQ");
			yield return (typeof(decimal), "Volume");
			yield return (typeof(string), "VolumeUQ");
			yield return (typeof(string), "Tariff");
			yield return (typeof(string), "JobStatus");
			yield return (typeof(string), "SummaryDeclaration");
			yield return (typeof(string), "ContainerID");
			yield return (typeof(string), "Origin");
			yield return (typeof(DateTime), "OriginETD");
			yield return (typeof(DateTime), "DestETA");
			yield return (typeof(DateTime), "LoadADT");
			yield return (typeof(DateTime), "ArrivalATA");
			yield return (typeof(DateTime), "FirstArrivalATA");
			yield return (typeof(string), "HouseBill");
			yield return (typeof(string), "MasterBill");
			yield return (typeof(string), "Vessel");
			yield return (typeof(string), "VoyageFlight");
			yield return (typeof(Guid), "BranchPK");
		}

		protected sealed override IEnumerable<string> GetParameterNameList()
		{
			yield return "@CompanyPk";
			yield return "@CreateDateFrom";
			yield return "@CreateDateTo";
			yield return "@ProductCode";
			yield return "@ProductDescription";
			yield return "@ShipmentType";
			yield return "@InvoiceNumber";
			yield return "@DeltaReference";
		}
	}

	#endregion
}
