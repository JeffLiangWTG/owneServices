using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(VoyageManifestFilterBusinessObject))]
	sealed class VoyageManifestFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOceanBillNumberFilter()
		{
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			header1.BO_OceanBill = "XAAAX";
			header2.BO_OceanBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter oceanBillNumberFilter = (ModuleNumberFilter)filterBO[VoyageManifestFilterConstants.NumberFilterTypes.OceanBill];
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			oceanBillNumberFilter.Property = "XAA";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			oceanBillNumberFilter.Property = "YAAAY";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "AAA";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "ZZZ";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestContainerNumberFilter()
		{
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			detail1.BD_BO = header1.PK;
			detail2.BD_BO = header2.PK;
			detail1.BD_ContainerNumber = "XAAAX";
			detail2.BD_ContainerNumber = "YAAAY";
			Factory.Save();
			ModuleNumberFilter containerNumberFilter = (ModuleNumberFilter)filterBO[VoyageManifestFilterConstants.NumberFilterTypes.Container];
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			containerNumberFilter.Property = "XAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			containerNumberFilter.Property = "YAAAY";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "AAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "ZZZ";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestLocationsProperty()
		{
			AssertNotNull(filterBO.Locations);
		}

		public void TestLoadDischargeFilter()
		{
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			header1.BO_RL_NKLoadPort = "KRSEL";
			header2.BO_RL_NKLoadPort = "AUBNE";
			header1.BO_RL_NKDischargePort = "AUSYD";
			header2.BO_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			ModuleLocationFilter loadDischargeFilter = (ModuleLocationFilter)filterBO[ZString.Format("{0} / {1}", VoyageManifestFilterConstants.PortFilterTypes.Load, VoyageManifestFilterConstants.PortFilterTypes.Discharge)];
			loadDischargeFilter.Property1 = "KR";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			loadDischargeFilter.Property1 = "AUBNE";
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestOriginDestinationFilter()
		{
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			header1.BO_RL_NKLoadPort = "KRSEL";
			header2.BO_RL_NKLoadPort = "AUBNE";
			header1.BO_RL_NKDischargePort = "AUSYD";
			header2.BO_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			ModuleLocationFilter originDestinationFilter = (ModuleLocationFilter)filterBO[ZString.Format("{0} / {1}", VoyageManifestFilterConstants.PortFilterTypes.Origin, VoyageManifestFilterConstants.PortFilterTypes.Destination)];
			originDestinationFilter.Property1 = "KR";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			originDestinationFilter.Property1 = "AUBNE";
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestConsigneesAndConsignorsProperties()
		{
			AssertNotNull(filterBO.Consignees);
			AssertNotNull(filterBO.Consignors);
		}

		public void TestConsigneeFilter()
		{
			organisation1.OH_IsConsignee = true;
			organisation2.OH_IsConsignee = true;
			organisation3.OH_IsConsignee = true;
			header1.BO_OH_Consignee = organisation1.PK;
			header2.BO_OH_Consignee = organisation2.PK;
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			Factory.Save();
			ModuleGuidsFilter consigneeFilter = (ModuleGuidsFilter)filterBO[String.Format("{0} / {1}", VoyageManifestFilterConstants.PartyFilterTypes.Consignor, VoyageManifestFilterConstants.PartyFilterTypes.Consignee)];
			consigneeFilter.Property2 = organisation1.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			consigneeFilter.Property2 = organisation3.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestConsignorFilter()
		{
			organisation1.OH_IsConsignor = true;
			organisation2.OH_IsConsignor = true;
			organisation3.OH_IsConsignor = true;
			header1.BO_OH_Consignor = organisation1.PK;
			header2.BO_OH_Consignor = organisation2.PK;
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			Factory.Save();
			ModuleGuidsFilter consignorFilter = (ModuleGuidsFilter)filterBO[String.Format("{0} / {1}", VoyageManifestFilterConstants.PartyFilterTypes.Consignor, VoyageManifestFilterConstants.PartyFilterTypes.Consignee)];
			consignorFilter.Property1 = organisation1.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			consignorFilter.Property1 = organisation3.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestStatusListProperty()
		{
			AssertNotNull(filterBO.StatusList);
			AssertEquals(10, filterBO.StatusList.Count);
		}

		public void TestImpendingArrivalStatusFilter()
		{
			entryNumber1.CE_ParentID = voyage1.PK;
			entryNumber2.CE_ParentID = voyage2.PK;
			entryNumber3.CE_ParentID = voyage3.PK;
			entryNumber1.CE_ParentTable = CusSeaManTranHeadSchema.Constants.TableName;
			entryNumber2.CE_ParentTable = CusSeaManTranHeadSchema.Constants.TableName;
			entryNumber3.CE_ParentTable = CusSeaManTranHeadSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.ImpendingArrivalResponseStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.ImpendingArrivalResponseStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.ActualArrivalResponseStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[VoyageManifestFilterConstants.StatusFilterTypes.ImpendingArrival];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			Assert("Expect collection not to contain Voyage3", !filterCollection.Contains(voyage3));
		}

		public void TestActualArrivalStatusFilter()
		{
			arrivalPort1.BA_BT = voyage1.PK;
			arrivalPort2.BA_BT = voyage2.PK;
			arrivalPort3.BA_BT = voyage3.PK;
			entryNumber1.CE_ParentID = arrivalPort1.PK;
			entryNumber2.CE_ParentID = arrivalPort2.PK;
			entryNumber3.CE_ParentID = arrivalPort3.PK;
			entryNumber1.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber2.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber3.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.ActualArrivalResponseStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.ActualArrivalResponseStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.ImpendingArrivalResponseStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[VoyageManifestFilterConstants.StatusFilterTypes.ActualArrival];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			Assert("Expect collection not to contain Voyage3", !filterCollection.Contains(voyage3));
		}

		public void TestCargoListStatusFilter()
		{
			arrivalPort1.BA_BT = voyage1.PK;
			arrivalPort2.BA_BT = voyage2.PK;
			arrivalPort3.BA_BT = voyage3.PK;
			entryNumber1.CE_ParentID = arrivalPort1.PK;
			entryNumber2.CE_ParentID = arrivalPort2.PK;
			entryNumber3.CE_ParentID = arrivalPort3.PK;
			entryNumber1.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber2.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber3.CE_ParentTable = CusSeaManArrivalPortSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.CargoListStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.CargoListStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.CargoReportStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[VoyageManifestFilterConstants.StatusFilterTypes.CargoList];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			Assert("Expect collection not to contain Voyage3", !filterCollection.Contains(voyage3));
		}

		public void TestCargoReportStatusFilter()
		{
			header1.BO_BT = voyage1.PK;
			header2.BO_BT = voyage2.PK;
			header3.BO_BT = voyage3.PK;
			entryNumber1.CE_ParentID = header1.PK;
			entryNumber2.CE_ParentID = header2.PK;
			entryNumber3.CE_ParentID = header3.PK;
			entryNumber1.CE_ParentTable = CusSeaManOBLHeaderSchema.Constants.TableName;
			entryNumber2.CE_ParentTable = CusSeaManOBLHeaderSchema.Constants.TableName;
			entryNumber3.CE_ParentTable = CusSeaManOBLHeaderSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.CargoReportStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.CargoReportStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.CargoListStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[VoyageManifestFilterConstants.StatusFilterTypes.CargoReport];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			Assert("Expect collection not to contain Voyage3", !filterCollection.Contains(voyage3));
		}

		public void TestActualArrivalDateFilter()
		{
			arrivalPort1.BA_BT = voyage1.PK;
			arrivalPort2.BA_BT = voyage2.PK;
			arrivalPort1.BA_ArrivalPortATA = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			arrivalPort2.BA_ArrivalPortATA = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO[VoyageManifestFilterConstants.DateFilterTypes.ActualArrivalDate];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
		}

		public void TestEstimatedArrivalDateFilter()
		{
			arrivalPort1.BA_BT = voyage1.PK;
			arrivalPort2.BA_BT = voyage2.PK;
			arrivalPort1.BA_ArrivalPortETA = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			arrivalPort2.BA_ArrivalPortETA = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO[VoyageManifestFilterConstants.DateFilterTypes.EstimatedArrivalDate];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
		}

		public void TestDepartureDateFilter()
		{
			voyage1.BT_PortOfLastForeignPortATD = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			voyage2.BT_PortOfLastForeignPortATD = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter departureDateFilter = (ModuleDateFilter)filterBO[VoyageManifestFilterConstants.DateFilterTypes.DepartureDate];
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			departureDateFilter.Property2 = ZDateTime.Empty;
			departureDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.Property1 = ZDateTime.Empty;
			departureDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			departureDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			departureDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			departureDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2.", filterCollection.Contains(voyage2));
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			departureDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			departureDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1.", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			departureDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			departureDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1.", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2.", !filterCollection.Contains(voyage2));
		}

		public void TestVesselsProperty()
		{
			AssertNotNull(filterBO.Vessels);
		}

		public void TestVoyageFilter()
		{
			voyage1.BT_VoyageNum = "XAAAX";
			voyage2.BT_VoyageNum = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter voyageFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			voyageFilter.Property = "XAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			voyageFilter.Property = "YAAAY";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "AAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection to contain Voyage2", filterCollection.Contains(voyage2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "ZZZ";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		public void TestVesselFilter()
		{
			voyage1.BT_VesselName = "XAAAX";
			voyage2.BT_VesselName = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter vesselFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			vesselFilter.NkProperty = "XAAAX";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Voyage1", filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
			vesselFilter.NkProperty = "YAA";
			vesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", filterCollection.Contains(voyage2));
			vesselFilter.NkProperty = "ZZZ";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Voyage1", !filterCollection.Contains(voyage1));
			Assert("Expect collection not to contain Voyage2", !filterCollection.Contains(voyage2));
		}

		[ExpectNoExceptions]
		public void TestVoyageFilterMaxSize()
		{
			var voyageFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			voyageFilter.Property = "HOEGH AMERICA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new VoyageManifestFilterBusinessObject();

		CusSeaManTranHead voyage1;
		CusSeaManTranHead voyage2;
		CusSeaManTranHead voyage3;
		CusSeaManOBLHeader header1;
		CusSeaManOBLHeader header2;
		CusSeaManOBLHeader header3;
		CusSeaManOBLDetail detail1;
		CusSeaManOBLDetail detail2;
		OrgHeader organisation1;
		OrgHeader organisation2;
		OrgHeader organisation3;
		CusEntryNumber entryNumber1;
		CusEntryNumber entryNumber2;
		CusEntryNumber entryNumber3;
		CusSeaManArrivalPort arrivalPort1;
		CusSeaManArrivalPort arrivalPort2;
		CusSeaManArrivalPort arrivalPort3;
		CusSeaManTranHeadCollection filterCollection;
		VoyageManifestFilterBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			voyage1 = Factory.NewWithValidTestData<CusSeaManTranHead>();
			voyage2 = Factory.NewWithValidTestData<CusSeaManTranHead>();
			voyage3 = Factory.NewWithValidTestData<CusSeaManTranHead>();
			header1 = Factory.NewWithValidTestData<CusSeaManOBLHeader>();
			header2 = Factory.NewWithValidTestData<CusSeaManOBLHeader>();
			header3 = Factory.NewWithValidTestData<CusSeaManOBLHeader>();
			detail1 = Factory.NewWithValidTestData<CusSeaManOBLDetail>();
			detail2 = Factory.NewWithValidTestData<CusSeaManOBLDetail>();
			organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			entryNumber1 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber1.CE_ParentTable = voyage1.TableName;
			entryNumber2 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber2.CE_ParentTable = voyage2.TableName;
			entryNumber3 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber3.CE_ParentTable = voyage3.TableName;
			arrivalPort1 = Factory.NewWithValidTestData<CusSeaManArrivalPort>();
			arrivalPort2 = Factory.NewWithValidTestData<CusSeaManArrivalPort>();
			arrivalPort3 = Factory.NewWithValidTestData<CusSeaManArrivalPort>();
			filterBO = (VoyageManifestFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new CusSeaManTranHeadCollection(Factory);
		}
	}
}
