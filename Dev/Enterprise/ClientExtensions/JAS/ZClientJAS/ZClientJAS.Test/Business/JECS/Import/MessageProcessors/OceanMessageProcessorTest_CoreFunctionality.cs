using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class OceanMessageProcessorTest_CoreFunctionality : JXCMessageProcessorBaseTest
	{
		#region TestProcessStandardShipment
		public void TestProcessStandardShipment_FirstRecordNotOHBLRecord()
		{
			bool result = Processor.ProcessStandardShipment(Factory, 2, NotificationBuffer);
			Assert("Not an OHBLRecord, should return false", !result);
		}

		public void TestProcessStandardShipment_FromFactory()
		{
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUSYD";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			bool result = Processor.ProcessStandardShipment(Factory, 1, NotificationBuffer);
			Assert("Should be processed successfully", result);
			JASForwardingShipment shipment = Factory.LoadTop1<JASForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "406005003508"));
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)))
			{
				AssertEquals(4, job.Charges.Count);
				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertEquals("SM6180 / PO #9716", shipment.JS_BookingReference);
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_FromConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			consol.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			bool result = Processor.ProcessStandardShipment(consol, 12, NotificationBuffer);
			Assert("Should be processed successfully", result);
			JASForwardingShipment shipment = Factory.LoadTop1<JASForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "NV05050449"));
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)))
			{
				AssertEquals(1, job.Charges.Count);
				AssertEquals(3, shipment.OuterPackLines.Count);
				AssertEquals("65050178", shipment.JS_BookingReference);
				AssertEquals("TESTING!@# BLAHBLAH", shipment.JS_MarksAndNumbers);
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_UpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUSYD";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			bool result = Processor.ProcessStandardShipment(Factory, 1, NotificationBuffer);
			Assert("Should be processed successfully", result);
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)))
			{
				AssertEquals(4, job.Charges.Count);
				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertEquals("SM6180 / PO #9716", shipment.JS_BookingReference);
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_ShouldNotUpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUSYD";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			bool result = Processor.ProcessStandardShipment(Factory, 1, NotificationBuffer);
			Assert("Should be processed successfully", result);
			Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull("Should not be imported", job);
			AssertEquals("Should not be updated", 0, shipment.OuterPackLines.Count);
			AssertEquals("Should not be updated", "", shipment.JS_BookingReference);
			AssertEquals(1, NotificationBuffer.Events.Length);
			AssertEquals(shipment.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Shipment will not be updated.", ((WarningNotification)NotificationBuffer.Events[0]).AdditionalInfo);
		}

		JASForwardingShipment CreateExistingShipment()
		{
			JASForwardingShipment shipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "406005003508";
			shipment.JS_RL_NKOrigin = "USYVR";
			shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();
			return shipment;
		}

		#endregion
		public void TestProcessCONTRecords_WithoutConsol()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.OuterPackLines.RemoveAll();
			shipment.OuterPackLines.AddNew();
			AssertEquals("Pre-condition", 1, shipment.OuterPackLines.Count);
			Processor.ProcessCONTRecords(null, shipment, 2, NotificationBuffer);
			AssertEquals("Two packlines should be imported from CONT records", 2, shipment.OuterPackLines.Count);
			AssertEquals("BLUEBERRIES, CULTIVATED, FROZE", shipment.OuterPackLines[0].JL_Description);
			AssertEquals("RASPBERRIES", shipment.OuterPackLines[1].JL_Description);
			Processor.ProcessCONTRecords(null, shipment, 13, NotificationBuffer);
			AssertEquals("Three packlines should be imported from CONT records", 3, shipment.OuterPackLines.Count);
			AssertEquals("BELONG TO ANOTHER ONE", shipment.OuterPackLines[0].JL_Description);
			AssertEquals("bElong to another one", shipment.OuterPackLines[1].JL_Description);
			AssertEquals("bElong to another ONE", shipment.OuterPackLines[2].JL_Description);
		}

		public void TestProcessCONTRecords()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAll();
			shipment.OuterPackLines.AddNew();
			AssertEquals("Pre-condition", 0, consol.Containers.Count);
			AssertEquals("Pre-condition", 1, shipment.OuterPackLines.Count);
			Processor.ProcessCONTRecords(consol, shipment, 2, NotificationBuffer);
			AssertEquals(1, consol.Containers.Count);
			AssertEquals("First CONT record should create the Container, second CONT record should match the first Container", "TRIU8461608", consol.Containers[0].JC_ContainerNum);
			AssertEquals("Two packlines should be imported from CONT records", 2, shipment.OuterPackLines.Count);
			AssertEquals("BLUEBERRIES, CULTIVATED, FROZE", shipment.OuterPackLines[0].JL_Description);
			AssertEquals("RASPBERRIES", shipment.OuterPackLines[1].JL_Description);
			Processor.ProcessCONTRecords(consol, shipment, 13, NotificationBuffer);
			AssertEquals(4, consol.Containers.Count);
			consol.Containers.Sort(JobContainerSchema.Constants.JC_ContainerNum, ListSortDirection.Ascending);
			AssertEquals("TRIU8461602", consol.Containers[0].JC_ContainerNum);
			AssertEquals("TRIU8461603", consol.Containers[1].JC_ContainerNum);
			AssertEquals("TRIU8461604", consol.Containers[2].JC_ContainerNum);
			AssertEquals("TRIU8461608", consol.Containers[3].JC_ContainerNum);
			AssertEquals("Three packlines should be imported from CONT records", 3, shipment.OuterPackLines.Count);
			AssertEquals("BELONG TO ANOTHER ONE", shipment.OuterPackLines[0].JL_Description);
			AssertEquals("bElong to another one", shipment.OuterPackLines[1].JL_Description);
			AssertEquals("bElong to another ONE", shipment.OuterPackLines[2].JL_Description);
		}

		public void TestProcessCHGSRecords()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			consol.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			AssertNull("Pre-condition", shipment.ShipmentJobHeader);
			Processor.ProcessCHGSRecords(shipment, 2, NotificationBuffer);
			AssertNotNull("Should be created", shipment.ShipmentJobHeader);
			using (Job job = new Job.Loader(shipment).Load())
			{
				AssertEquals(4, job.Charges.Count);
				job.Charges.Sort(JobChargeSchema.Constants.JR_Desc, ListSortDirection.Ascending);
				AssertEquals("NVOCC BILL OF LADING FEE", job.Charges[0].JR_Desc);
				AssertEquals("NVOCC CARRIER B/L FEE", job.Charges[1].JR_Desc);
				AssertEquals("NVOCC DRAYAGE", job.Charges[2].JR_Desc);
				AssertEquals("SAFe (Security Admin. Fee", job.Charges[3].JR_Desc);
			}
		}

		public void TestProcessCHGSRecors_PrepaidChargeShouldNotBeIncluded()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			consol.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			AssertNull("Pre-condition", shipment.ShipmentJobHeader);
			Processor.ProcessCHGSRecords(shipment, 14, NotificationBuffer);
			AssertNotNull("Should be created", shipment.ShipmentJobHeader);
			using (Job job = new Job.Loader(shipment).Load())
			{
				AssertEquals(1, job.Charges.Count);
				AssertEquals("Charges3", job.Charges[0].JR_Desc);
			}
		}

		public void TestProcessCHGSRecords_NoCHGSRecords()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			consol.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			AssertNull("Pre-condition", shipment.ShipmentJobHeader);
			Processor.ProcessCHGSRecords(shipment, 19, NotificationBuffer);
			AssertNull("Job Header should not be created if there are no CHGS records", shipment.ShipmentJobHeader);
		}

		[ExpectNoExceptions]
		public void TestProcessCHGSRecords_ShouldNotThrowExceptionIfJobCannotBeCreated()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			consol.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			using (new Job.Loader(shipment).TryCreate())
			{
				Processor.ProcessCHGSRecords(shipment, 2, NotificationBuffer);
			}
		}

		#region Implementation
		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		OceanMessageProcessorForTest Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new OceanMessageProcessorForTest();
				}

				return fProcessor;
			}
		}

		void AssertBusinessObjectCreatedOrUpdatedNotification(JASForwardingShipment shipment)
		{
			BusinessObjectCreatedOrUpdatedNotification[] notifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingShipment));
			AssertEquals(1, notifications.Length);
			AssertEquals(shipment, notifications[0].BusinessEntity);
		}

		NotificationBuffer fNotificationBuffer;
		OceanMessageProcessorForTest fProcessor;
		#region OceanMessageProcessorForTest
		class OceanMessageProcessorForTest : OceanMessageProcessor
		{
			public OceanMessageProcessorForTest() : base(new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;USSEA;AUCOR;USCOR;AUSYD"), new OMANRecord(JXCConstants.LineTypes.OMAN, "n;01406005003509;KAPITAN MASLOV;615SB;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;23/05/2005;13/06/2005"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;01406005003508;USSEA;406005003508;30;USYVR;FANE;FESCO AUSTRALIA NORTH AMERICA LINE;1087 DOWNTOWER BLVD;SUITE 100;MOBILE, AL 36609;;NA0059288;CONTINENTAL FOOD SALES INC;600 WINSLOW WAY E;SUITE 231;BAINBRIDGE ISLAND, WA 98110;Not Known;wa;;CA;;Y;2068427440;COFO00;;ATYS AUSTRALIA;200 GEORGE DOWNES DR.;CENTRAL MANGROVE;NSW AUSTRALIA 2250;SYDNEY;00;00000;AU;;Y;;COFO00ATYS AUS;;JAS FORWARDING (USA), INC. SEA;11521 EAST MARGINAL WAY, SUITE 120;SEATTLE, WA 98168;;;2122R/055731;VANCOUVER, BC, CANAD;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUSYD;USD;0;P;SYDNEY, AUS.;KAPITAN MASLOV;Voy:615SB;CA;;23/05/2005;;;;1;PCS;51200.000;L;0;4362.00;40FT REEFER CONTAINER SAID TO CONTA;1600-30# CASES FROZEN IQF;BLUEBERRIES;(VENTS CLOSED-TEMP.@-18 DEGREES C);CAED#02A200BC080820050500023;EXPRESS RELEASE;;;08;Optional Field;;23/05/2005;SEATTLE;UTC;JAS FORWARDING WORLDWIDE PTY LTD;UNIT 12, BLDG C, 2-12 BEAUCHAMP RD;BANKSMEADOW NSW 2019;AUSTRALIA;Tel:01161283362888;Fax:01161283362800;NOT KNOWN;LADEN ON BOARD/05.23.05/;ORIGIN;;;One;;1;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "SM6180 / PO #9716;S"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461608;NOT KNOWN;;4;51200.000;1.00;1;BLUEBERRIES, CULTIVATED, FROZE;;;0;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "690;SAFe (Security Admin. Fee;1.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "621;NVOCC HBL FREIGHT PREPAID;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "639;NVOCC BILL OF LADING FEE;1.00;C;USD"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461608;NOT KNOWN;;4;51200.000;1.00;1;RASPBERRIES;;;0;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "649;NVOCC ISPS PORT FEE;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "625;NVOCC DRAYAGE;1.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "637;NVOCC HANDLING FEES;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "649;NVOCC CARRIER B/L FEE;1.00;C;USD"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;NV05050449;HKHKG;NV05050449;30;HKHKG;OOCL;OOCL;;;;;;POLK AUDIO;C/O ORIENTAL LOGISTICS CO., LTD;1-11 KA TING ROAD,;KWAI CHUNG, N.T.,;HKHKG;N/A;;HK;;n;;93-18117;;ASSOCIATED MARKETING GROUP;88 ENTERPRISE AVENUE;BERWICK (MELBOURNE), 3806;AUSTRALIA;MEL;N/A;;AU;;n;;10985;(10985);JAS FORWARDING (HK) LIMITED;UNIT B, 5/F., MTL WARHOUSE BULIDING;PHASE 1 BERTH ONE, KWAI CHUNG;CONTAINER TERMINALS, KWAI CHUNG NT;;;HONG KONG;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUMEL;USD;184.80;C;MELBOURNE;CSCL KELANG;068S;HK;;16/05/2005;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;2;CTN;702.770;K;4.620;;SAID TO CONTAIN;;;PLTS(55 CTNS);HI-FI LOUDSPEAKER   ;   ;  ; -FREIGHT COLLECT- ;ZZ;TWO (2) PALLETS ONLY;;16/05/2005;HONG KONG;CCT;JAS FORWARDING WORLDWIDE PTY LTD;GROUND FLOOR, THE MILLS;200 ARDEN STREET, NORTH MELBOURNE,;VICTORIA 3051, AUSTRALIA;MELBOURNE;;HKHKG;;;;;3;MELBOURNE;0;;N"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461602;NOT KNOWN;;4;51200.000;1.00;1;BELONG TO ANOTHER ONE;;;0;USD"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461603;NOT KNOWN;;4;51200.000;1.00;1;bElong to another one;;;0;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "123;Charges3;1.00;c;USD"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461604;NOT KNOWN;;4;51200.000;1.00;1;bElong to another ONE;;;0;USD"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050178;S"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "456;Charges1;2.00;P;AUD"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050178;C"), new SHMKRecord(JXCConstants.LineTypes.SHMK, "TESTING!@#;BLAHBLAH;forty"), new DummyHouseRecord(JXCConstants.LineTypes.DOHB, "N;TRAFFICNO#123;AUCOR;HB103"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") })
			{
			}

			public new bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber)
			{
				return base.ProcessStandardShipment(factoryProvider, bodyRecordIndex, notificationSubscriber);
			}

			public new void ProcessCONTRecords(JASForwardingConsol consol, JASForwardingShipment shipment, int startingIndex, INotifications notificationSubscriber)
			{
				base.ProcessCONTRecords(consol, shipment, startingIndex, notificationSubscriber);
			}

			public new void ProcessCHGSRecords(JASForwardingShipment shipment, int startingIndex, INotifications notificationSubscriber)
			{
				base.ProcessCHGSRecords(shipment, startingIndex, notificationSubscriber);
			}

			protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
			{
				return true;
			}

			protected override Type FirstLineType
			{
				get
				{
					return typeof(DummyRecord);
				}
			}
		}
		#endregion
		#endregion
	}
}
