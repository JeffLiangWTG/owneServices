using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBDataObjectReaderTest : DataTransfer.Universal.AirManifest.Testing.AirManifestDataObjectReaderTestHelper
	{
		[TestDate(2014, 11, 01, 01, 01, 01)]
		public void TestUpdateFlightDetailFromCAST()
		{
			var anotherFactory = new BusinessObjectFactory();
			var gbMawb = anotherFactory.New<CusMAWB>();
			gbMawb.CM_MAWB = "61898391193";
			gbMawb.CM_ApplicationCode = "CUK";
			var gbHawb = gbMawb.ChildBills.AddNew();
			gbHawb.CS_HAWB = "8888388";
			gbHawb.CS_ApplicationCode = "CUK";
			anotherFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var communicationMode = GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "ACR";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "61898391193";
				consol.Transports[0].JW_ATD = new ZDateTime(2014, 10, 29);

				var hvlShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				hvlShipment.JS_TransportMode = TransportModes.Air;
				hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				hvlShipment.JS_HouseBill = "8888388";
				consol.Shipments.Add(hvlShipment);

				var consignment = Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
				consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = hvlShipment.HVLVConsignmentHeader.PK;
				((IHVLVConsignment)consignment).HVC_WaybillNumber = "KKQF0001";

				var item = Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVItem>());
				item[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment.PK;

				var forwarderMAWB = CusMAWB.CreateNew(consol);
				forwarderMAWB.FillWithValidTestData();
				forwarderMAWB.SynchroniseData();

				var trigger = hvlShipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.HVLVReadyCode;
				trigger.P9_Description = "Send to Air Cargo";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent;

				hvlShipment.GetLogs().AddNew(AutoEvents.HVLVReady, "HLR Clearance", ZDateTimeOffset.UtcNow);

				Factory.SaveForTesting();

				var message = Factory.New<CMRCARSTMessage>();

				var triggerLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
				triggerLogQuery.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLog = Factory.LoadTop1<StmALog>(triggerLogQuery);

				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
				var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
				using (Factory.BOFactory.AddDisposableService())
				{
					var processor = workflowDescriptor.GetWorkflowTriggerAction(notification, queuedLog);

					var logger = new NotificationsForTesting();

					var universalXMLProcessor = processor as UniversalXmlWorkflowProcessor;
					message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20141030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:61898391193'
RFF+HWB:8888388'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
					message.SetEM_LinkedObject();
					Factory.SaveForTesting();

					processor.Process(logger);

					Factory.SaveForTesting();
				}
				var depotMAWBQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "61898391193");
				depotMAWBQuery.AddToFilter(CusMAWBSchema.CM_JK, DBNull.Value);
				depotMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, forwarderMAWB.CM_ApplicationCode);
				var depotMAWB = new BusinessObjectFactory().LoadTop1<CusMAWB>(depotMAWBQuery);
				AssertNotNull(depotMAWB);
				AssertEquals(new ZDateTime(2014, 10, 30), depotMAWB.CM_ArrivalDate);
				AssertEquals("5X6901", depotMAWB.CM_FlightNo);
				AssertEquals("8888388", depotMAWB.CM_MasterHouseBill);
				AssertEquals(1, depotMAWB.ChildBills.Count);
				AssertEquals("KKQF0001", depotMAWB.ChildBills[0].CS_HAWB);
			}
		}

		public void TestImportArrivalDateFromTransportLeg()
		{
			var oldHomePort = CurrentCompanySecondBranch.GB_RL_NKHomePort;
			try
			{
				CurrentCompanySecondBranch.GB_RL_NKHomePort = "AUSYD";
				var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
				mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);
				mawbDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransportMode = TransportMode.Air,
					LegOrder = 1,
					PortOfLoading = new UNLOCO() { Code = "NZAKL" },
					PortOfDischarge = new UNLOCO() { Code = "USLAX" },
					ActualArrival = ZDate.Today.AddDays(-10),
				};
				var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransportMode = TransportMode.Air,
					LegOrder = 2,
					PortOfLoading = new UNLOCO() { Code = "USLAX" },
					PortOfDischarge = new UNLOCO() { Code = "AUSYD" },
					ActualArrival = ZDate.Today.AddDays(-5),
				};
				mawbDataObject.TransportLegCollection.Add(leg1);
				mawbDataObject.TransportLegCollection.Add(leg2);

				var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
				var mawbBO = reader.ReadIntoBusinessObject();
				AssertEquals(ZDateTime.Today.AddDays(-5), mawbBO.CM_ArrivalDate);
			}
			finally
			{
				CurrentCompanySecondBranch.GB_RL_NKHomePort = oldHomePort;
			}
		}

		public void TestImportingColoadData()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals("CL343", mawbBO.CM_MasterHouseBill);
			Factory.SaveForTesting();

			mawbDataObject.SetAdditionalBillCollection(() => null);
			mawbDataObject.ShipmentType = new CodeDescriptionPair() { Code = AgentType.CoLoad };
			mawbDataObject.BookingConfirmationReference = "CL343";
			reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
			var mawbBO2 = reader.ReadIntoBusinessObject();
			AssertEquals("CL343", mawbBO2.CM_MasterHouseBill);
			AssertEquals(mawbBO, mawbBO2);
		}

		public void TestInvalidMAWBRecyclePeriodWillMatchAll()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Eritrea);
			var newFactory = new BusinessObjectFactory();
			var existingMAWB = newFactory.New<CusMAWB>();
			existingMAWB.CM_MAWB = "MB2343";
			existingMAWB.CM_MasterHouseBill = ZString.Empty;
			var createTime = ZDateTime.Now.AddMonths(-1);
			existingMAWB.CM_SystemCreateTimeUtc = createTime;
			newFactory.Save();
			AssertEquals(createTime, existingMAWB.CM_SystemCreateTimeUtc);

			var mawbDataObject = SetupAirCargoMaster("MB-23 43", "");

			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
			var mawbBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals(existingMAWB.PK, mawbBO.PK);
		}

		public void TestMatchingToExistingCusMAWB()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Eritrea);
			var newFactory = new BusinessObjectFactory();
			var existingMAWB1 = newFactory.New<CusMAWB>();
			existingMAWB1.CM_MAWB = "MB2343";
			existingMAWB1.CM_MasterHouseBill = ZString.Empty;
			var existingMAWB1HAWB1 = existingMAWB1.ChildBills.AddNew();
			existingMAWB1HAWB1.CS_HAWB = "HB2343";
			existingMAWB1HAWB1.CS_IsMasterHouse = ZBool.True;
			var existingMAWB1HAWB2 = existingMAWB1.ChildBills.AddNew();
			existingMAWB1HAWB2.CS_HAWB = "SB8965";
			existingMAWB1HAWB2.CS_MasterHouseBill = "HB2343";
			existingMAWB1HAWB2.CS_IsMasterHouse = ZBool.False;
			existingMAWB1HAWB2.CS_CS_MasterHouseBill = existingMAWB1HAWB1.PK;
			var existingMAWB1HAWB3 = existingMAWB1.ChildBills.AddNew();
			existingMAWB1HAWB3.CS_HAWB = "HB1TOBEDELETED";

			var existingMAWB2 = newFactory.New<CusMAWB>();
			existingMAWB2.CM_MAWB = "MB2343";
			existingMAWB2.CM_MasterHouseBill = "CL343";
			var existingMAWB2HAWB1 = existingMAWB2.ChildBills.AddNew();
			existingMAWB2HAWB1.CS_HAWB = "HB2343";
			existingMAWB2HAWB1.CS_IsMasterHouse = ZBool.True;
			var existingMAWB2HAWB2 = existingMAWB2.ChildBills.AddNew();
			existingMAWB2HAWB2.CS_HAWB = "SB8965";
			existingMAWB2HAWB2.CS_MasterHouseBill = "HB2343";
			existingMAWB2HAWB2.CS_IsMasterHouse = ZBool.False;
			existingMAWB2HAWB2.CS_CS_MasterHouseBill = existingMAWB2HAWB1.PK;
			var existingMAWB2HAWB3 = existingMAWB2.ChildBills.AddNew();
			existingMAWB2HAWB3.CS_HAWB = "HB1TOBEDELETED";

			var existingMAWB3 = newFactory.New<CusMAWB>();
			existingMAWB3.CM_MAWB = "MB8965";
			existingMAWB3.CM_MasterHouseBill = ZString.Empty;
			var existingMAWB3HAWB1 = existingMAWB3.ChildBills.AddNew();
			existingMAWB3HAWB1.CS_HAWB = "HB2343";
			existingMAWB3HAWB1.CS_IsMasterHouse = ZBool.True;
			var existingMAWB3HAWB2 = existingMAWB3.ChildBills.AddNew();
			existingMAWB3HAWB2.CS_HAWB = "SB8965";
			existingMAWB3HAWB2.CS_MasterHouseBill = "HB2343";
			existingMAWB3HAWB2.CS_IsMasterHouse = ZBool.False;
			existingMAWB3HAWB2.CS_CS_MasterHouseBill = existingMAWB3HAWB1.PK;
			var existingMAWB3HAWB3 = existingMAWB3.ChildBills.AddNew();
			existingMAWB3HAWB3.CS_HAWB = "HB1TOBEDELETED";
			newFactory.Save();

			foreach (var log in existingMAWB1HAWB1.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			foreach (var log in existingMAWB1HAWB2.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			foreach (var log in existingMAWB2HAWB1.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			foreach (var log in existingMAWB2HAWB2.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			newFactory.Save();

			var mawbDataObject = SetupAirCargoMaster("MB-23 43", "CL343");
			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);
			var hawbDataObject2 = SetupAirCargoHouse("HB6935", ZBool.False, null);
			var subHawbDataObject = SetupAirCargoHouse("SB8965", ZBool.False, "HB2343");

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject);

			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject2);

			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
			var mawbBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNotNull(mawbBO);
			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertEquals("Should have been matched to existingMAWB2", existingMAWB2.PK, mawbBO.PK);
				AssertContents(mawbBO, "MB2343", "CL343");
				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 3, mawbBO.ChildBills.Count);
				existingMAWB2HAWB1 = Factory.Load<CusHAWB>(existingMAWB2HAWB1.PK);
				AssertNotNull("existingMAWB2HAWB1.IsDeleted", existingMAWB2HAWB1);
				existingMAWB2HAWB2 = Factory.Load<CusHAWB>(existingMAWB2HAWB2.PK);
				AssertNotNull("existingMAWB2HAWB2.IsDeleted", existingMAWB2HAWB2);
				AssertNull("existingMAWB2HAWB3.IsDeleted", Factory.Load<CusHAWB>(existingMAWB2HAWB3.PK));
				var hawbBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB2343");
				var hawbBO2 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB6935");
				var subhawbBO = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "SB8965");
				AssertEquals("hawbBO1 should have been matched to existingMAWB2HAWB1", existingMAWB2HAWB1.PK, hawbBO1.PK);
				AssertCusHAWBContents(hawbBO1, "HB2343", ZBool.True, null);
				AssertEquals("subhawbBOs should have been matched to existingMAWB2HAWB2", existingMAWB2HAWB2.PK, subhawbBO.PK);
				AssertCusHAWBContents(subhawbBO, "SB8965", ZBool.False, "HB2343");
				AssertCusHAWBContents(hawbBO2, "HB6935", ZBool.False, null);
			});

			#endregion

			mawbDataObject.SetAdditionalBillCollection(() => null);
			reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory);
			mawbBO = reader.ReadIntoBusinessObject();
			AssertNotNull(mawbBO);
			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertEquals("Should have been matched to existingMAWB1", existingMAWB1.PK, mawbBO.PK);
				AssertContents(mawbBO, "MB2343", ZString.Empty);
				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 3, mawbBO.ChildBills.Count);
				existingMAWB1HAWB1 = Factory.Load<CusHAWB>(existingMAWB1HAWB1.PK);
				AssertNotNull("existingMAWB1HAWB1.IsDeleted", existingMAWB1HAWB1);
				existingMAWB1HAWB2 = Factory.Load<CusHAWB>(existingMAWB1HAWB2.PK);
				AssertNotNull("existingMAWB1HAWB2.IsDeleted", existingMAWB1HAWB2);
				existingMAWB1HAWB3 = Factory.Load<CusHAWB>(existingMAWB1HAWB3.PK);
				AssertNull("existingMAWB1HAWB3.IsDeleted", existingMAWB1HAWB3);
				var hawbBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB2343");
				var hawbBO2 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB6935");
				var subhawbBO = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "SB8965");
				AssertEquals("hawbBO1 should have been matched to existingMAWB1HAWB1", existingMAWB1HAWB1.PK, hawbBO1.PK);
				AssertCusHAWBContents(hawbBO1, "HB2343", ZBool.True, null);
				AssertEquals("subhawbBOs should have been matched to existingMAWB1HAWB2", existingMAWB1HAWB2.PK, subhawbBO.PK);
				AssertCusHAWBContents(subhawbBO, "SB8965", ZBool.False, "HB2343");
				AssertCusHAWBContents(hawbBO2, "HB6935", ZBool.False, null);
			});

			#endregion
		}

		public void TestImportingCusMAWBData()
		{
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var responsiblePartyRego1 = responsibleParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientID, "CID45686", CountryCodes.Australia);
			var responsiblePartyRego2 = responsibleParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN698536466", CountryCodes.Australia);
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);

			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2010, 1, 4)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 3)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.True, new ZDateTime(2010, 1, 6)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2010, 1, 5)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2010, 1, 7)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 8)));

			mawbDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			mawbDataObject.NoteCollection.Add(SetupNote());
			mawbDataObject.NoteCollection.Add(SetupNote2());

			mawbDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			mawbDataObject.AdditionalReferenceCollection.Add(SetupAdditionalReference());
			mawbDataObject.AdditionalReferenceCollection.Add(SetupAdditionalReference(null, null, "RIP3423", new UniversalDataBuss.DataObjects.Universal.EntryType() { Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID }));

			mawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			mawbDataObject.OrganizationAddressCollection.Add(SetupOrganizationAddress(AddressTypes.Forwarder));
			var responsiblePartyData = SetupOrganizationAddress(AddressTypes.ResponsibleParty, null, null, responsibleParty.OH_FullName, responsibleParty.MainAddress.OA_Address1, responsibleParty.MainAddress.OA_Address2,
				responsibleParty.MainAddress.OA_City, responsibleParty.MainAddress.OA_State, responsibleParty.MainAddress.OA_PostCode, UNLOCO.New(responsibleParty.MainAddress.EffectiveRelatedPortCode), Country.New(responsibleParty.MainAddress.RelatedCountry), responsibleParty.Contacts[0].OC_ContactName, responsibleParty.MainAddress.OA_Phone);
			mawbDataObject.OrganizationAddressCollection.Add(responsiblePartyData);

			mawbDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				CustomizedField.New("CustomField1", (ZString)"CustomField1"),
				CustomizedField.New("CustomField2PART1", (ZString)"CustomField2PART1"),
				CustomizedField.New("CustomField2PART2", (ZString)"CustomField2PART2")
			});

			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);
			var subHawbDataObject = SetupAirCargoHouse("SB8965", ZBool.False, "HB2343");

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject);

			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertContents(mawbBO, "MB2343", "CL343");
				AssertEquals("mawbBO.CM_ApplicationCode", AUCustoms.ImportMessagingMode.ForceCMRMessages, mawbBO.CM_ApplicationCode);
				AssertEquals("mawbBO.CM_IsCTOMAWB", ZBool.False, mawbBO.CM_IsCTOMAWB);
				AssertEquals("mawbBO.CM_DepartureDate", new ZDateTime(2010, 1, 3), mawbBO.CM_DepartureDate);
				AssertEquals("mawbBO.CM_DateOfFirstArrival", new ZDateTime(2010, 1, 5), mawbBO.CM_DateOfFirstArrival);
				AssertEquals("mawbBO.CM_ArrivalDate", new ZDateTime(2010, 1, 8), mawbBO.CM_ArrivalDate);
				AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
				AssertEquals("mawbBO.CM_OH_ResponsibleParty", responsibleParty.PK, mawbBO.CM_OH_ResponsibleParty);

				AssertEquals("mawbBO.CustomsFields1", "CustomField1", mawbBO.GetUserDefinedValue<ZString>("CustomField1"));
				AssertEquals("mawbBO.CustomsFields2Part1", "CustomField2PART1", mawbBO.GetUserDefinedValue<ZString>("CustomField2PART1"));
				AssertEquals("mawbBO.CustomsFields2Part2", "CustomField2PART2", mawbBO.GetUserDefinedValue<ZString>("CustomField2PART2"));

				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
				var hawbBO = mawbBO.ChildBills[0];
				var subhawbBO = mawbBO.ChildBills[1];
				if (subhawbBO.CS_CS_MasterHouseBill.IsEmpty)
				{
					hawbBO = mawbBO.ChildBills[1];
					subhawbBO = mawbBO.ChildBills[0];
				}
				AssertEquals("hawbBO.CS_HAWB", "HB2343", hawbBO.CS_HAWB);
				AssertEquals("hawbBO.CS_CS_MasterHouseBill", ZGuid.Empty, hawbBO.CS_CS_MasterHouseBill);
				AssertEquals("subhawbBO.CS_HAWB", "SB8965", subhawbBO.CS_HAWB);
				AssertEquals("subhawbBO.CS_CS_MasterHouseBill", hawbBO.PK, subhawbBO.CS_CS_MasterHouseBill);

				AssertMultilineASCIIEquals("logger.Logs", @"
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
Matching 'ResponsibleParty':- Matched to 'WUFSHIJNB' address 'Level 2, Building G' with a score of 250.
No matching StmNote found, creating new StmNote.
Populating StmNote...
No matching StmNote found, creating new StmNote.
Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Added Air Cargo House (HAWB: SB8965 MHB: HB2343) from UniversalShipment.
Added Air Cargo House (HAWB: HB2343) from UniversalShipment.
Added AirCargo Report (MAWB: MB2-343 MHB: CL343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343 MHB: CL343) with 2 x StmNote, 2 x CusHAWB.
".Trim(), message.GetLogNoteText());
			});

			#endregion

			mawbDataObject.SetAdditionalReferenceCollection(() => null);
			message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			manager.Process(message);
			mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			mawbBO.Reload();
			AssertEquals("CM_ResponsiblePartyID", responsiblePartyRego2.OK_CustomsRegNo, mawbBO.CM_ResponsiblePartyID);

			responsiblePartyRego2.Delete();
			Factory.SaveForTesting();
			mawbDataObject.SetAdditionalReferenceCollection(() => null);
			message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			manager.Process(message);
			mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			mawbBO.Reload();
			AssertEquals("CM_ResponsiblePartyID", responsiblePartyRego1.OK_CustomsRegNo, mawbBO.CM_ResponsiblePartyID);

			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestDeleteHouseBillsNotInUniversalXmlMessage()
		{
			var mawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			mawbData.WayBillNumber = "08111111111";
			mawbData.Folio = "AB";

			var hawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData.WayBillNumber = "HB1";
			hawbData.GoodsDescription = "YO";
			_ = SubShipmentCollectionAddSafe(mawbData, hawbData);

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			Factory.SaveForTesting();

			var reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory);
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			AssertEquals("cusMAWB.CM_Folio", "AB", cusMAWB.CM_Folio);
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 1, cusMAWB.ChildBills.Count);
			var cusHAWB = cusMAWB.ChildBills[0];
			AssertEquals("cusHAWB.CS_HAWB", "HB1", cusHAWB.CS_HAWB);
			AssertEquals("cusHAWB.CS_GoodsDescription", "YO", cusHAWB.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Information - Populating CusMAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added Air Cargo House (HAWB: HB1) from UniversalShipment.
Information - Updated AirCargo Report (MAWB: 081-11111111) from UniversalShipment.
".Trim(), logger.Logs);

			cusMAWB.ChildBills.RemoveAndDeleteAll();
			var hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2";
			cusMAWB.CM_Folio = ZString.Empty;
			Factory.SaveForTesting();
			logger.ClearLogs();
			reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory);
			mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			AssertEquals("cusMAWB.CM_Folio", "AB", cusMAWB.CM_Folio);
			AssertEquals("hawb.IsDeleted because it is not in the universal xml message", expected: true, hawb.IsDeleted);
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 1, cusMAWB.ChildBills.Count);
			cusHAWB = cusMAWB.ChildBills[0];
			AssertEquals("cusHAWB.CS_HAWB", "HB1", cusHAWB.CS_HAWB);
			AssertEquals("cusHAWB.CS_GoodsDescription", "YO", cusHAWB.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Information - Populating CusMAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added Air Cargo House (HAWB: HB1) from UniversalShipment.
Information - Deleted Air Cargo House (HAWB: HB2) from UniversalShipment.
Information - Updated AirCargo Report (MAWB: 081-11111111) from UniversalShipment.
".Trim(), logger.Logs);

			cusMAWB.ChildBills.RemoveAndDeleteAll();
			hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			cusMAWB.CM_Folio = ZString.Empty;
			Factory.SaveForTesting();
			logger.ClearLogs();
			reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory, true);
			mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			AssertEquals("cusMAWB.CM_Folio. None of MAWB fields are updated as there are HAWBs with active messaging", "", cusMAWB.CM_Folio);
			AssertEquals("hawb is not deleted. Even though it is not in universal xml, it should not be deleted as messaging is active", expected: false, hawb.IsDeleted);
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 2, cusMAWB.ChildBills.Count);
			AssertNotNull("HB1", cusMAWB.ChildBills.Find(x => x.CS_HAWB == "HB1"));
			AssertNotNull("HB2n", cusMAWB.ChildBills.Find(x => x.CS_HAWB == "HB2"));
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Information - Populating CusMAWB...
Warning - AirCargo Report (MAWB: 081-11111111) data will not be updated as there is at least one House Bill with an active messaging.
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added Air Cargo House (HAWB: HB1) from UniversalShipment.
Information - Updated AirCargo Report (MAWB: 081-11111111) from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestDoesNotAllowUpdateArrivalDate()
		{
			var oldHomePort = CurrentCompanySecondBranch.GB_RL_NKHomePort;
			try
			{
				CurrentCompanySecondBranch.GB_RL_NKHomePort = "AUSYD";
				var mawbDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				mawbDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
				mawbDataObject.WayBillNumber = "MB2343";
				mawbDataObject.VoyageFlightNo = "QF345";
				mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);
				mawbDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				var leg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransportMode = TransportMode.Air,
					LegOrder = 1,
					PortOfLoading = new UNLOCO() { Code = "USLAX" },
					PortOfDischarge = new UNLOCO() { Code = "AUSYD" },
					ActualArrival = ZDate.Today.AddDays(-5).AddHours(4),
				};
				mawbDataObject.TransportLegCollection.Add(leg);

				var cusMAWB = Factory.New<CusMAWB>();
				cusMAWB.CM_MAWB = "MB2343";
				cusMAWB.CM_FlightNo = "QF345";
				cusMAWB.CM_ArrivalDate = ZDate.Today.AddDays(-5);

				var hawb = cusMAWB.ChildBills.AddNew();
				hawb.CS_HAWB = "HB2";
				hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
				Factory.SaveForTesting();
				logger.ClearLogs();

				var mawbBO = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory).ReadIntoBusinessObject();
				AssertNotContains("Arrival Date can be updated as long as the date does not change", "There is an attempt to update".Trim(), logger.Logs);
				AssertEquals(cusMAWB, mawbBO);

				cusMAWB.CM_ArrivalDate = ZDate.Today.AddDays(-3);
				Factory.SaveForTesting();
				logger.ClearLogs();

				mawbBO = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory).ReadIntoBusinessObject();
				AssertEquals(cusMAWB, mawbBO);
				AssertContains("Stop importin as the date part is changed", "There is an attempt to update".Trim(), logger.Logs);
			}
			finally
			{
				CurrentCompanySecondBranch.GB_RL_NKHomePort = oldHomePort;
			}
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSource()
		{
			var mawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			mawbData.WayBillNumber = "08111111111";
			mawbData.Folio = "AB";
			mawbData.VoyageFlightNo = "QF345";

			var hawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData.WayBillNumber = "HB1";
			hawbData.GoodsDescription = "YO";
			_ = SubShipmentCollectionAddSafe(mawbData, hawbData);

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			cusMAWB.CM_FlightNo = "QF345";
			Factory.SaveForTesting();

			var hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			cusMAWB.CM_Folio = ZString.Empty;
			Factory.SaveForTesting();
			logger.ClearLogs();
			var reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory);
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			AssertEquals("hawb.IsDeleted", expected: false, hawb.IsDeleted);
			AssertNotContains("Folio is not a significant field and should not be a reason to stop importing", "There is an attempt to update".Trim(), logger.Logs);

			cusMAWB.ChildBills.Load();
			cusMAWB.ChildBills.RemoveAndDeleteAll();
			hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			cusMAWB.CM_FlightNo = "QF123";
			Factory.SaveForTesting();
			logger.ClearLogs();
			reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory);
			mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 1, cusMAWB.ChildBills.Count);
			AssertContains("Should stop importing as flight has changed", "There is an attempt to update", logger.Logs);
		}

		public void TestDoNotUpdateHAWBWithActiveMessaging()
		{
			var mawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			mawbData.WayBillNumber = "08111111111";
			mawbData.Folio = "AB";

			var hawbData = new Shipment();
			hawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData.WayBillNumber = "HB1";
			hawbData.GoodsDescription = "YO";
			_ = SubShipmentCollectionAddSafe(mawbData, hawbData);

			var hawbData2 = new Shipment();
			hawbData2.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData2.WayBillNumber = "SB1";
			hawbData2.GoodsDescription = "XO";
			_ = SubShipmentCollectionAddSafe(mawbData, hawbData2);

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			cusMAWB.CM_Folio = "AB";

			var hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			hawb.CS_GoodsDescription = "OY";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			var hawb2 = cusMAWB.ChildBills.AddNew();
			hawb2.CS_HAWB = "SB1";
			hawb2.CS_GoodsDescription = "IH";

			Factory.SaveForTesting();
			foreach (var log in hawb2.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			logger.ClearLogs();

			var reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory, false);
			var mawbBO = reader.ReadIntoBusinessObject();
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 2, cusMAWB.ChildBills.Count);
			AssertEquals("House bill with active messaging should not be updated", "OY", cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB1").CS_GoodsDescription);
			AssertEquals("House bill without active messaging should be updated", "XO", cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "SB1").CS_GoodsDescription);

			AssertContains("logger.Logs", @"Error - Cannot populate CusHAWB because:
Messaging is active for Air Cargo House (HAWB: HB1)
".Trim(), logger.Logs);
		}

		public void TestImportSubshipments()
		{
			var mawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			mawbData.WayBillNumber = "08111111111";
			mawbData.Folio = "AB";

			var hawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData.WayBillNumber = "HB1";
			hawbData.GoodsDescription = "YO";
			_ = SubShipmentCollectionAddSafe(mawbData, hawbData);

			var subHawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subHawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.SubHouse };
			subHawbData.WayBillNumber = "SB1";
			subHawbData.GoodsDescription = "HI";
			_ = SubShipmentCollectionAddSafe(hawbData, subHawbData);

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";

			var hawb = cusMAWB.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			cusMAWB.CM_Folio = ZString.Empty;
			Factory.SaveForTesting();
			foreach (var log in hawb.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = ZArchitecture.Environment.User.ServiceUserCode;
			}
			logger.ClearLogs();
			var reader = new CusMAWBDataObjectReader(mawbData, null, logger, Factory, true);
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, cusMAWB);
			AssertEquals("cusMAWB.CM_Folio", "", cusMAWB.CM_Folio);
			AssertEquals("hawb.IsDeleted", expected: false, hawb.IsDeleted);
			cusMAWB.ChildBills.Load();
			AssertEquals("cusMAWB.ChildBills.Count", 2, cusMAWB.ChildBills.Count);

			var cusHAWB = cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB1");
			var cusHAWB2 = cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "SB1");
			AssertEquals("cusHAWB2.CS_HAWB", "SB1", cusHAWB2.CS_HAWB);
			AssertEquals("cusHAWB2.CS_GoodsDescription", "HI", cusHAWB2.CS_GoodsDescription);
			AssertEquals("cusHAWB2.CS_CS_MasterHouseBill", cusHAWB.PK, cusHAWB2.CS_CS_MasterHouseBill);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Information - Populating CusMAWB...
Warning - AirCargo Report (MAWB: 081-11111111) data will not be updated as there is at least one House Bill with an active messaging.
Information - Successfully loaded matching CusHAWB.
Information - Populating CusHAWB...
Warning - Air Cargo House (HAWB: HB1) data will not be updated as it has active messaging.
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added Air Cargo House (HAWB: SB1 MHB: HB1) from UniversalShipment.
Information - Updated Air Cargo House (HAWB: HB1) from UniversalShipment.
Information - Updated AirCargo Report (MAWB: 081-11111111) from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestGetExistingBusinessObject_WhenInHVLVProcessAndCoLoadBillEmpty_UsesTRFLogWithRefNumberToFindMAWB()
		{
			AssertGetExistingBusinessObject_WhenInHVLVProcessAndCoLoadBillEmpty_UsesTRFLogToFindMAWB(EventReferenceParameters.Codes.ReferenceNumber);
		}

		public void TestGetExistingBusinessObject_WhenInHVLVProcessAndCoLoadBillEmpty_UsesTRFLogWithJobNumberToFindMAWB()
		{
			AssertGetExistingBusinessObject_WhenInHVLVProcessAndCoLoadBillEmpty_UsesTRFLogToFindMAWB(EventReferenceParameters.Codes.JobNumber);
		}

		void AssertGetExistingBusinessObject_WhenInHVLVProcessAndCoLoadBillEmpty_UsesTRFLogToFindMAWB(string jobNumberEventReferenceCode)
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");

			var mawbWithTRFLog = Factory.New<CusMAWB>();
			mawbWithTRFLog.CM_MAWB = mawbDataObject.GetMasterBill();
			mawbWithTRFLog.CM_MasterHouseBill = ZString.Empty;

			var mawbWithoutTRFLog = Factory.New<CusMAWB>();
			mawbWithoutTRFLog.CM_MAWB = mawbDataObject.GetMasterBill();
			mawbWithoutTRFLog.CM_MasterHouseBill = ZString.Empty;

			var hvlvForwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			hvlvForwardingShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var hvlvUniversalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvUniversalShipment.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValueLegacy };
			hvlvUniversalShipment.DataContext = DataContextFactory.New();
			hvlvUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, hvlvForwardingShipment.JobNumber);

			Factory.SaveForTesting();

			var reader = new CusMAWBDataObjectReaderForTesting(mawbDataObject, hvlvUniversalShipment, logger, Factory);

			AssertNull("Could not find MAWB when no TRF log in MAWB", reader.GetExistingBusinessObject());

			mawbWithTRFLog.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue),
				new KeyValuePair<string, string>(jobNumberEventReferenceCode, hvlvForwardingShipment.JobNumber)
			});

			Factory.SaveForTesting();

			AssertEquals("Find MAWB by TRF Log", mawbWithTRFLog, reader.GetExistingBusinessObject());
		}

		public void TestImportASNShipment()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", null);
			mawbDataObject.DataContext.AddDataSource(DataContextType.TransitReceiveASN, "TRT00000001");
			mawbDataObject.Folio = "AB";

			var hawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			hawbData.WayBillNumber = "HB1";
			hawbData.GoodsDescription = "YO";
			_ = SubShipmentCollectionAddSafe(mawbDataObject, hawbData);

			var subHawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subHawbData.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.SubHouse };
			subHawbData.WayBillNumber = "SB1";
			subHawbData.GoodsDescription = "HI";
			_ = SubShipmentCollectionAddSafe(hawbData, subHawbData);

			Factory.SaveForTesting();

			logger.ClearLogs();
			logger.TopLevelDataObject = mawbDataObject;
			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory, true);
			var mawbBO = reader.ReadIntoBusinessObject();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusMAWB found, creating new CusMAWB.
Information - Populating CusMAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added Air Cargo House (HAWB: SB1 MHB: HB1) from UniversalShipment.
Information - Added Air Cargo House (HAWB: HB1) from UniversalShipment.
Information - Added AirCargo Report (MAWB: MB2-343) from UniversalShipment.
".Trim(), logger.Logs);
		}

		bool SubShipmentCollectionAddSafe(Shipment mawbData, Shipment hawbData)
		{
			var subShipments = mawbData.SubShipmentCollection ?? new DataObjectList<Shipment>();
			subShipments.Add(hawbData);
			return mawbData.SetSubShipmentCollection(() => subShipments);
		}

		sealed class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			readonly List<string> notifications = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}

		sealed class CusMAWBDataObjectReaderForTesting : CusMAWBDataObjectReader
		{
			public CusMAWBDataObjectReaderForTesting(Shipment mawbDataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck = false)
				: base(mawbDataObject, hVLVShipperConsolidation, logger, factory, singleHAWBCheck)
			{
			}

			public new CusMAWB GetExistingBusinessObject() => base.GetExistingBusinessObject();
		}
	}
}
