using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReport))]
	sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			(var report, _, var header) = GetNewBusinessObject(Factory);
			AssertEquals(header.PK, report.Header.PK);
		}

		public void TestClearanceDate()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				var clearanceDate = ZDateTime.Now;
				AssertEquals("ClearanceDate is empty when no CLR EntryNum added", ZDateTime.Empty, report.ClearanceDate);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(report, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_IssueDate = clearanceDate;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				AssertEquals("ClearanceDate is filled when CLR EntryNum added/updated", clearanceDate, report.ClearanceDate);
			});
		}

		public void TestCircuit()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Circuit is empty when no CLR EntryNum added", ZString.Empty, report.Circuit);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(report, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				newEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.GREEN;
				var message = MessageFunctionCodeList.Codes.GreenCircuitText + " - " + CircuitCodeList.Descriptions.GREEN;
				AssertEquals("Circuit is filled when CLR EntryNum added/updated (Green)", message, report.Circuit);
				newEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.RED;
				message = MessageFunctionCodeList.Codes.RedCircuitText + " - " + CircuitCodeList.Descriptions.RED;
				AssertEquals("Circuit is filled when CLR EntryNum added/updated (Red)", message, report.Circuit);
				newEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.ORANGE;
				message = MessageFunctionCodeList.Codes.OrangeCircuitText + " - " + CircuitCodeList.Descriptions.ORANGE;
				AssertEquals("Circuit is filled when CLR EntryNum added/updated (Orange)", message, report.Circuit);
			});
		}

		public void TestArrivalDate()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			report.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
			AssertEquals("ArrivalDate is filled with the datetime in CER_DateTime", new ZDateTime(2022, 02, 15, 16, 43, 27), report.ArrivalDate);
		}

		public void TestAdditonalInfos()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoCollection>(report.AdditionalInfos);
		}

		public void TestCusExitReportItems()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(report.CusExitReportItems);
		}

		public void TestCusExitReportItemPackages()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(report.CusExitReportItemPackages);
		}

		public void TestCusSupportingInfoTypes()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)report).GetCusSupportingInfoTypes();
			AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestLookups()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportLookups>(report.Lookups);
		}

		public void TestDefaultTransportTypeOnModeOfTransport()
		{
			CombineAssertions(() =>
			{
				var (report, _,  _) = GetNewBusinessObject(Factory);
				report.CER_TransportType = ZString.Empty;
				report.CER_TransportMode = "2";
				AssertEquals("No change", ZString.Empty, report.CER_TransportType);
				foreach (var motValue in GetTestData())
				{
					report.CER_TransportMode = motValue;
					AssertEquals($"For CER_TransportMode: {motValue}", ZString.Empty, report.CER_TransportType);
				}
			});

			static IEnumerable<string> GetTestData()
			{
				yield return TransportTypeList.Codes.Air;
				yield return TransportTypeList.Codes.FixedTransportInstallations;
				yield return TransportTypeList.Codes.InlandWaterwayTransport;
				yield return TransportTypeList.Codes.OwnPropulsion;
				yield return TransportTypeList.Codes.Mail;
				yield return TransportTypeList.Codes.Rail;
				yield return TransportTypeList.Codes.Road;
				yield return TransportTypeList.Codes.Sea;
			}
		}

		public void TestCER_TransportTypeReadOnly()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var exitReport = exitHeader.CusExitReports.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("When discrepancies is not ticked, the TransportType is readOnly", exitReport.CER_TransportTypeInfo.ReadOnly, true);

				exitReport.CER_Calc_Discrepancies = true;
				AssertEquals("When discrepancies is ticked, the TransportType is not readOnly", exitReport.CER_TransportTypeInfo.ReadOnly, false);
			});
		}

		public void TestCER_TransportIDReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("When discrepancies is not ticked, the TransportID is readOnly", report.CER_TransportIDInfo.ReadOnly, true);

				report.CER_Calc_Discrepancies = true;
				AssertEquals("When discrepancies is ticked, the TransportID is not readOnly", report.CER_TransportIDInfo.ReadOnly, false);
			});
		}

		public void TestCER_RN_NKTransportNationalityReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("When discrepancies is not ticked, the TransportNationality is readOnly", report.CER_RN_NKTransportNationalityInfo.ReadOnly, true);

				report.CER_Calc_Discrepancies = true;
				AssertEquals("When discrepancies is ticked, the TransportNationality is not readOnly", report.CER_RN_NKTransportNationalityInfo.ReadOnly, false);
			});
		}

		public void TestCER_DateTimeReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("CER_DateTime is readOnly", report.CER_DateTimeInfo.ReadOnly, true);
			});
		}

		public void TestSetReadOnlyTab()
		{
			var report1 = Factory.New<CusExitReport>();
			var exitReportItem1 = report1.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = ZGuid.NewZGuid();

			var report2 = Factory.New<CusExitReport>();
			var exitReportItem2 = report2.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = ZGuid.NewZGuid();

			var additionalInfo1 = exitReportItem1.AdditionalInfos.AddNew();
			var additionalInfo2 = exitReportItem2.AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				report1.CER_Calc_Discrepancies = false;
				report2.CER_Calc_Discrepancies = false;
				AssertEquals("when CER_Calc_Discrepancies is false in the first line, additionalInfo1 is readonly", true, additionalInfo1.CusExitReportItem.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is false in the second line, additionalInfo2 is readonly", true, additionalInfo2.CusExitReportItem.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is false in the first line, exitReportItem1 is readonly", true, exitReportItem1.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is false in the second line, exitReportItem3 is readonly", true, exitReportItem2.ReadOnly);

				report1.CER_Calc_Discrepancies = true;
				AssertEquals("when CER_Calc_Discrepancies is true in the first line, additionalInfo1 is not readonly", false, additionalInfo1.CusExitReportItem.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is false in the second line, additionalInfo2 is readonly", true, additionalInfo2.CusExitReportItem.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is true in the first line, exitReportItem1 is not readonly", false, exitReportItem1.ReadOnly);
				AssertEquals("when CER_Calc_Discrepancies is false in the second line, exitReportItem3 is readonly", true, exitReportItem2.ReadOnly);
			});
		}

		public void TestValidation()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportValidation>(report.Validation);
		}

		public void TestIsAccepted()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_Status = ZString.Empty;
				AssertEquals("With status empty report is not accepted", false, report.IsAccepted);

				report.CER_Status = "EXR";
				AssertEquals("With status EXR report is accepted", true, report.IsAccepted);

				report.CER_Status = "CLP";
				AssertEquals("With status CLP report is not accepted", false, report.IsAccepted);

				report.CER_Status = "COX";
				AssertEquals("With status COX report is accepted", true, report.IsAccepted);

				report.CER_Status = "ERR";
				AssertEquals("With status ERR report is not accepted", false, report.IsAccepted);

				report.CER_Status = "REF";
				AssertEquals("With status REF report is accepted", true, report.IsAccepted);

				report.CER_Status = "AAA";
				AssertEquals("With status AAA report is not accepted", false, report.IsAccepted);

				report.CER_Status = "RCE";
				AssertEquals("With status RCE report is not accepted", false, report.IsAccepted);
			});
		}

		public void TestIsSentOrAccepted()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_MessageStatus = "ACC";
				AssertEquals("With message status ACC report is not sent", false, report.IsSentOrAccepted);

				report.CER_MessageStatus = "SNT";
				AssertEquals("With message status SNT report is sent", true, report.IsSentOrAccepted);

				report.CER_MessageStatus = ZString.Empty;
				report.CER_Status = ZString.Empty;
				AssertEquals("With status and message status empty report is not sent or accepted", false, report.IsSentOrAccepted);

				report.CER_Status = "EXR";
				AssertEquals("With status EXR report is accepted", true, report.IsSentOrAccepted);

				report.CER_Status = "CLP";
				AssertEquals("With status CLP report is not sent or accepted", false, report.IsSentOrAccepted);

				report.CER_Status = "COX";
				AssertEquals("With status COX report is accepted", true, report.IsSentOrAccepted);

				report.CER_Status = "ERR";
				AssertEquals("With status ERR report is not sent or accepted", false, report.IsSentOrAccepted);

				report.CER_Status = "REF";
				AssertEquals("With status REF report is accepted", true, report.IsSentOrAccepted);

				report.CER_Status = "AAA";
				AssertEquals("With status AAA report is not sent or accepted", false, report.IsSentOrAccepted);

				report.CER_Status = "RCE";
				AssertEquals("With status RCE report is not sent or accepted", false, report.IsSentOrAccepted);
			});
		}

		public void TestIsSentAndNotEmpty()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_MessageStatus = "ACC";
				AssertEquals("With message status ACC report and customs status Empty", false, report.IsSentOrNotEmpty);

				report.CER_MessageStatus = "SNT";
				AssertEquals("With message status SNT report and customs status Empty", true, report.IsSentOrNotEmpty);

				report.CER_MessageStatus = ZString.Empty;
				AssertEquals("With message status empty report and customs status Empty", false, report.IsSentOrNotEmpty);

				report.CER_Status = "AAA";
				AssertEquals("With message status empty report and customs status not Empty", true, report.IsSentOrNotEmpty);

				report.CER_Status = ZString.Empty;
				AssertEquals("With message status empty report and customs status is Empty", false, report.IsSentOrNotEmpty);
			});
		}

		public void TestClearanceEntryNumber()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				AssertNotNull("ClearanceEntryNumber Not Null", report.ClearanceEntryNumber);
				AssertEquals("ClearanceEntryNumber.CE_RN_NKCountryCode", report.CountryCode, report.ClearanceEntryNumber.CE_RN_NKCountryCode);
				AssertEquals("ClearanceEntryNumber.CE_EntryType", CusEntryNumberTypes.Spain.ClearanceCSV, report.ClearanceEntryNumber.CE_EntryType);
			});
		}

		public void TestClearanceReferenceNumber()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("ClearanceReferenceNumber is empty when no CLR EntryNum added", ZString.Empty, report.ClearanceReferenceNumber);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(report, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = "number";
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				AssertEquals("ClearanceReferenceNumber is filled when CLR EntryNum added/updated", "number", report.ClearanceReferenceNumber);
			});
		}

		public void TestIESMessageInfoProvider_Broker()
		{
			CombineAssertions(() =>
			{
				(var report, _, var header) = GetNewBusinessObject(Factory);
				var esMessageInfoProvider = report as IESMessageInfoProvider;
				header.CXH_GS_NKCustomsAgent = ZString.Empty;
				AssertNull("Broker is null", esMessageInfoProvider.Broker);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AZM";
				header.CXH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
			});
		}

		public void TestIESMessageInfoProvider_MRN()
		{
			(var report, var consignment, _) = GetNewBusinessObject(Factory);
			var esMessageInfoProvider = report as IESMessageInfoProvider;
			CombineAssertions(() =>
			{
				AssertEquals("MRN has the correct value (empty when report has no mrn)", ZString.Empty, esMessageInfoProvider.MRN);

				consignment.CXC_MovementReference = "20ES00999830001277";
				AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageInfoProvider.MRN);
			});
		}

		public void TestIESResponseBusinessObject_EntryReference()
		{
			(var report, var consignment, _) = GetNewBusinessObject(Factory);
			var esMessageBusinessObject = report as IESMessageBusinessObject;
			CombineAssertions(() =>
			{
				AssertEquals("MRN has the correct value (empty when report has no mrn)", ZString.Empty, esMessageBusinessObject.EntryReference);

				consignment.CXC_MovementReference = "20ES00999830001277";
				AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageBusinessObject.EntryReference);
			});
		}

		public void TestIESResponseBusinessObject_BranchPK()
		{
			var report = Factory.New<CusExitReport>();
			var esResponseBusinessObject = report as IESResponseBusinessObject;

			CombineAssertions(() =>
			{
				AssertEquals("BranchPK has the correct value, empty when report not associated to parent", ZGuid.Empty, esResponseBusinessObject.BranchPK);

				var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
				header.CusExitReports.Add(report);
				AssertEquals("BranchPK has the correct value when report associated to parent", header.Branch.PK, esResponseBusinessObject.BranchPK);
			});
		}

		public void TestIESResponseBusinessObject_MessageCollection()
		{
			var report = Factory.New<CusExitReport>();
			var esResponseBusinessObject = report as IESResponseBusinessObject;
			report.Messages.AddNew();
			AssertEquals("MessageCollection has the correct value", report.Messages, esResponseBusinessObject.MessageCollection);
		}

		public void TestIPollingTransactionParent_CertificateName()
		{
			CombineAssertions(() =>
			{
				(var report, _, var header) = GetNewBusinessObject(Factory);
				var pollingTransactionParent = report as IPollingTransactionParent;
				header.CXH_CustomsProfile = ZString.Empty;
				AssertEquals("CertificateName is empty", ZString.Empty, pollingTransactionParent.CertificateName);

				header.CXH_CustomsProfile = "cert";
				AssertEquals("CertificateName is not empty", "cert", pollingTransactionParent.CertificateName);
			});
		}

		public void TestIPollingTransactionParent_IsTest()
		{
			(var report, _, var header) = GetNewBusinessObject(Factory);
			var pollingTransactionParent = report as IPollingTransactionParent;

			CombineAssertions(() =>
			{
				var registrationMock = new Mock<IProductRegistration>();
				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("IsTest is false when PRD and external environment", false, pollingTransactionParent.IsTest);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("IsTest is true when TST and external environment", true, pollingTransactionParent.IsTest);
				}

				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("IsTest is false when PRD and internal environment", false, pollingTransactionParent.IsTest);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					header.TrainingEntry = true;
					AssertEquals("IsTest is true when TST and internal environment when flag is checked", true, pollingTransactionParent.IsTest);

					header.TrainingEntry = false;
					AssertEquals("IsTest is false when TST and internal environment when flag is not checked", false, pollingTransactionParent.IsTest);
				}
			});
		}

		public void TestIPollingTransactionParent_Broker()
		{
			CombineAssertions(() =>
			{
				(var report, _, var header) = GetNewBusinessObject(Factory);
				var pollingTransactionParent = report as IPollingTransactionParent;
				header.CXH_GS_NKCustomsAgent = ZString.Empty;
				AssertNull("Broker is null", pollingTransactionParent.Broker);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AZM";
				header.CXH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("Broker is not null", staff, pollingTransactionParent.Broker);
			});
		}

		public void TestIPollingTransactionParent_Declarant()
		{
			CombineAssertions(() =>
			{
				(var report, _, var header) = GetNewBusinessObject(Factory);
				var pollingTransactionParent = report as IPollingTransactionParent;
				header.CXH_OA_Carrier = ZGuid.Empty;
				AssertNull("Declarant is null", pollingTransactionParent.Declarant);

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				header.CXH_OA_Carrier = declarant.MainAddress.PK;
				AssertEquals("Declarant is not null", declarant, pollingTransactionParent.Declarant);
			});
		}

		public void TestStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var esCode = CountryCodes.Spain;
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingDataGrouping(esCode, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Customs Status CSTEX");
			helper.CreateNewOrGetExistingCusCodeList(esCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "130", "Customs Status 130", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(esCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "PEN", "Customs Status PEN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(eunCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "200", "Customs Status 200", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = Factory.New<CusExitReport>();

				report.CER_Status = "130";
				AssertEquals("Valid status for ES in CSTEX (130)", "Customs Status 130", report.StatusDescription);

				report.CER_Status = "200";
				AssertEquals("Invalid status for ES in CSTEX, code is in EUN (200)", ZString.Empty, report.StatusDescription);

				report.CER_Status = "PEN";
				AssertEquals("Valid status in CodeDescriptionPairList & CSTEX, only gets CodeDescriptionPairList description (PEN)", "Pending Control", report.StatusDescription);

				report.CER_Status = "AAA";
				AssertEquals("AAA status is not in CodeDescriptionPairList nor CSTEX so empty description", ZString.Empty, report.StatusDescription);

				report.CER_Status = "PDA";
				AssertEquals("Invalid status in CodeDescriptionPairList, code is in EUN (PDA)", ZString.Empty, report.StatusDescription);

				report.CER_Status = "EXR";
				AssertEquals("Valid status in CodeDescriptionPairList (EXR)", "Released for Exit", report.StatusDescription);

				report.CER_Status = ZString.Empty;
				AssertEquals("Empty status", ZString.Empty, report.StatusDescription);
			});
		}

		public void TestAdditionalInfosItemNumberDictionary()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.AdditionalInfos.AddNew();
			report.AdditionalInfos.AddNew();
			var additionalInfo1 = report.AdditionalInfos.AddNew();
			additionalInfo1.CSI_ItemNumber = 1;
			Factory.Save();

			var dictionary = new Dictionary<ZInt, ZInt>();
			dictionary.Add(1, 2);
			dictionary.Add(2, 1);
			AssertEquals("Dictionary", true, report.AdditionalInfosItemNumberDictionary.ContainsSameElementsInAnyOrder(dictionary));
		}

		public void TestCanLaunchExitReportUrl()
		{
			CombineAssertions(() =>
			{
				var exitReport = Factory.New<CusExitReport>();
				AssertEquals("Can't launch, Message Status != ACC", false, exitReport.CanLaunchExitReportUrl());

				exitReport.CER_MessageStatus = "ACC";
				AssertEquals("Can launch, Message Status = ACC", true, exitReport.CanLaunchExitReportUrl());
			});
		}

		public void TestGetUrlToLaunch()
		{
			CombineAssertions(() =>
			{
				var expectedMRN = "AH3RRRRRRNNNNNNNN";
				var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalleDecLlegada?wMrn=" + expectedMRN;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

				var consignment = exitHeader.CusExitConsignments.AddNew();
				consignment.CXC_MovementReference = expectedMRN;
				var report = exitHeader.CusExitReports.AddNew();
				report.CER_CXC_Consignment = consignment.PK;

				AssertEquals("Empty because Message Status != ACC", ZString.Empty, report.GetUrlToLaunch());

				report.CER_MessageStatus = "ACC";
				AssertEquals("Correct url because Message Status == ACC", expectedUrl, report.GetUrlToLaunch());

				report.Delete();
				AssertEquals("Empty because Deleted", ZString.Empty, report.GetUrlToLaunch());
			});
		}

		public void TestReadOnlyWhenStatusChanges()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_Status = ZString.Empty;
				AssertEquals("With status empty report is not readonly", false, report.ReadOnly);

				report.CER_Status = "EXR";
				AssertEquals("With status EXR report is readonly", true, report.ReadOnly);

				report.CER_Status = "CLP";
				AssertEquals("With status CLP report is not readonly", false, report.ReadOnly);

				report.CER_Status = "COX";
				AssertEquals("With status COX report is readonly", true, report.ReadOnly);

				report.CER_Status = "ERR";
				AssertEquals("With status ERR report is not readonly", false, report.ReadOnly);

				report.CER_Status = "REF";
				AssertEquals("With status REF report is readonly", true, report.ReadOnly);

				report.CER_Status = "AAA";
				AssertEquals("With status AAA report is not readonly", false, report.ReadOnly);

				report.CER_Status = "RCE";
				AssertEquals("With status RCE report is not readonly", false, report.ReadOnly);
			});
		}

		public void TestReadOnlyWhenMessageStatusChanges()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_MessageStatus = "ACC";
				AssertEquals("With message status ACC report is not readonly", false, report.ReadOnly);

				report.CER_MessageStatus = "SNT";
				AssertEquals("With message status SNT report is readonly", true, report.ReadOnly);

				report.CER_MessageStatus = ZString.Empty;
				AssertEquals("With message status empty report is not readonly", false, report.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).report;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).report;

		public static (CusExitReport report, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Now;
			report.CER_OfficeOfExit = "ES001";
			return (report, consignment, header);
		}
	}
}
