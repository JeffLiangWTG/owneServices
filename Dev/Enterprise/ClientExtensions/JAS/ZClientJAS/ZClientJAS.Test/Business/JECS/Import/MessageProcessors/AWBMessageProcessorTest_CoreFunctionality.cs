using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class AWBMessageProcessorTest_CoreFunctionality : JXCMessageProcessorBaseTest
	{
		#region TestProcessStandardShipment
		public void TestProcessStandardShipment_FromFactory()
		{
			JASOrgHeader receivingForwarder = new BusinessObjectFactory().New<JASOrgHeader>();
			receivingForwarder.OH_Code = "RCV12342";
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUMEL";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			receivingForwarder.OH_IsDebtor = true;
			receivingForwarder.Factory.Save();
			ZGuid defaultChargeCodeGuid = ZGuid.NewZGuid();
			JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, defaultChargeCodeGuid.ToGuid());
			bool result = Processor.ProcessStandardShipment(Factory, 8, NotificationBuffer);
			Assert("Should be processed successfully", result);
			JASForwardingShipment shipment = Factory.LoadTop1<JASForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "7/0400859"));
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)))
			{
				AssertEquals(172m, shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals(285.5m, shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight);
				AssertEquals(4, shipment.AWBHeader.AWBOtherCharges.Count);
				AssertEquals(4, job.Charges.Count);
				RefCurrency euroCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "EUR");
				ZGuid freightChargeCodeGuid = new ZGuid(Env.Registry.FreightChargeCode);
				AssertJobCharge(job.Charges[0], freightChargeCodeGuid, "International Freight", euroCurrency.RX_Code, 2931m);
				AssertJobCharge(job.Charges[1], defaultChargeCodeGuid, "Other 1", euroCurrency.RX_Code, 1.26m);
				AssertJobCharge(job.Charges[2], defaultChargeCodeGuid, "Other 2", euroCurrency.RX_Code, 2.26m);
				AssertJobCharge(job.Charges[3], defaultChargeCodeGuid, "Other 4", euroCurrency.RX_Code, 4.26m);
				AssertEquals("INV.4010409613+4+5+6", shipment.JS_BookingReference);
				AssertEquals("first SHIPMENT", shipment.JS_MarksAndNumbers);
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_FromConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASOrgHeader receivingForwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			receivingForwarder.OH_Code = "DESFKMA";
			receivingForwarder.OH_IsDebtor = true;
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			receivingForwarder.Factory.Save();
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			ZGuid defaultChargeCodeGuid = ZGuid.NewZGuid();
			JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, defaultChargeCodeGuid.ToGuid());
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Disbursement);
			AccChargeCode accChargeCode = Factory.LoadTop1<AccChargeCode>(query);
			accChargeCode.AC_IATA_ChargeCodeMap = "MA";
			AssertEquals("Pre-condition", 0, consol.Shipments.Count);
			bool result = Processor.ProcessStandardShipment(consol, 20, NotificationBuffer);
			Assert("Should be processed successfully", result);
			JASForwardingShipment shipment = Factory.LoadTop1<JASForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "10/0403155"));
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)))
			{
				AssertEquals(4m, shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals(8, shipment.AWBHeader.AWBOtherCharges.Count);
				AssertEquals(5, job.Charges.Count);
				job.Charges.Sort(JobChargeSchema.Constants.JR_Desc, System.ComponentModel.ListSortDirection.Ascending);
				RefCurrency euroCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "EUR");
				ZGuid freightChargeCodeGuid = new ZGuid(Env.Registry.FreightChargeCode);
				AssertJobCharge(job.Charges[0], defaultChargeCodeGuid, "FS/MY", euroCurrency.RX_Code, 7.2m);
				AssertJobCharge(job.Charges[1], defaultChargeCodeGuid, "I.A.T", euroCurrency.RX_Code, 5.17m);
				AssertJobCharge(job.Charges[2], accChargeCode.PK, "SAF", euroCurrency.RX_Code, 25m);
				AssertJobCharge(job.Charges[3], defaultChargeCodeGuid, "SC", euroCurrency.RX_Code, 3.6m);
				AssertJobCharge(job.Charges[4], accChargeCode.PK, "X RAY", euroCurrency.RX_Code, 7.2m);
				AssertEquals("1333", shipment.JS_BookingReference);
				AssertEquals("second shipment", shipment.JS_MarksAndNumbers);
				Assert("Shipment should be attached to the consol", consol.Shipments.Contains(shipment));
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_FirstRecordNotHAWBRecord()
		{
			bool result = Processor.ProcessStandardShipment(Factory, 1, NotificationBuffer);
			Assert("Not an HAWB record, should return false", !result);
		}

		public void TestProcessStandardShipment_UpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASOrgHeader receivingForwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			receivingForwarder.OH_Code = "DESFKMA";
			receivingForwarder.OH_IsDebtor = true;
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUMEL";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			receivingForwarder.Factory.Save();
			ZGuid defaultChargeCodeGuid = ZGuid.NewZGuid();
			JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, defaultChargeCodeGuid.ToGuid());
			bool result = Processor.ProcessStandardShipment(Factory, 8, NotificationBuffer);
			Assert("Should be processed successfully", result);
			using (Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)))
			{
				AssertEquals(172m, shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals(285.5m, shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight);
				AssertEquals(4, shipment.AWBHeader.AWBOtherCharges.Count);
				AssertEquals(4, job.Charges.Count);
				RefCurrency euroCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "EUR");
				ZGuid freightChargeCodeGuid = new ZGuid(Env.Registry.FreightChargeCode);
				AssertJobCharge(job.Charges[0], freightChargeCodeGuid, "International Freight", euroCurrency.RX_Code, 2931m);
				AssertJobCharge(job.Charges[1], defaultChargeCodeGuid, "Other 1", euroCurrency.RX_Code, 1.26m);
				AssertJobCharge(job.Charges[2], defaultChargeCodeGuid, "Other 2", euroCurrency.RX_Code, 2.26m);
				AssertJobCharge(job.Charges[3], defaultChargeCodeGuid, "Other 4", euroCurrency.RX_Code, 4.26m);
				AssertEquals("INV.4010409613+4+5+6", shipment.JS_BookingReference);
				AssertEquals("first SHIPMENT", shipment.JS_MarksAndNumbers);
				AssertBusinessObjectCreatedOrUpdatedNotification(shipment);
			}
		}

		public void TestProcessStandardShipment_ShouldNotUpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASOrgHeader receivingForwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			receivingForwarder.OH_Code = "DESFKMA";
			receivingForwarder.OH_IsDebtor = true;
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUMEL";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			receivingForwarder.Factory.Save();
			ZGuid defaultChargeCodeGuid = ZGuid.NewZGuid();
			JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, defaultChargeCodeGuid.ToGuid());
			bool result = Processor.ProcessStandardShipment(Factory, 8, NotificationBuffer);
			Assert("Should be processed successfully", result);
			Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull("Should not be imported", job);
			AssertEquals(0m, shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight);
			AssertEquals(0m, shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight);
			AssertEquals(0, shipment.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("", shipment.JS_BookingReference);
			AssertEquals("", shipment.JS_MarksAndNumbers);
			AssertEquals(1, NotificationBuffer.Events.Length);
			AssertEquals(shipment.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Shipment will not be updated.", ((WarningNotification)NotificationBuffer.Events[0]).AdditionalInfo);
		}

		JASForwardingShipment CreateExistingShipment()
		{
			JASForwardingShipment shipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "7/0400859";
			shipment.JS_RL_NKOrigin = "ITNAP";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_E_DEP = new ZDateTime(2004, 9, 23);
			Factory.Save();
			return shipment;
		}

		#endregion
		public void TestProcessFBDNRecords_ShipmentAsAWBParent()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 100m;
			AssertEquals("Pre-condition", 2, shipment.AWBHeader.LineNumberOfFirstEmptyRateLine);
			Processor.ProcessFBDNRecords(shipment, 9);
			AssertEquals("Should be clearing the existing ones rather than adding to a new rate line", 3, shipment.AWBHeader.LineNumberOfFirstEmptyRateLine);
			AssertEquals(172m, shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight);
			AssertEquals("4", shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
			AssertEquals(285.5m, shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight);
			AssertEquals("5", shipment.AWBHeader.AWBRateLines[1].ER_NoOfPiecesOrRCP);
		}

		public void TestProcessFBDNRecords_ConsolAsAWBParent()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.AWBRateLines[0].ER_GrossWeight = 100m;
			AssertEquals("Pre-condition", 2, consol.AWBHeader.LineNumberOfFirstEmptyRateLine);
			Processor.ProcessFBDNRecords(consol, 1);
			AssertEquals("Should be clearing the existing ones rather than adding to a new rate line", 2, consol.AWBHeader.LineNumberOfFirstEmptyRateLine);
			AssertEquals(172m, consol.AWBHeader.AWBRateLines[0].ER_GrossWeight);
			AssertEquals("4", consol.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
		}

		public void TestProcessOTHRRecords_ShipmentAsAWBParent()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.AWBHeader.AWBOtherCharges.AddNew();
			AssertEquals("Pre-condition", 1, shipment.AWBHeader.AWBOtherCharges.Count);
			Processor.ProcessOTHRRecords(shipment, 9);
			AssertEquals("Should be clearing the existing ones", 4, shipment.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Other 1", shipment.AWBHeader.AWBOtherCharges[0].EO_ChargeDescription);
			AssertEquals("Other 2", shipment.AWBHeader.AWBOtherCharges[1].EO_ChargeDescription);
			AssertEquals("Other 3", shipment.AWBHeader.AWBOtherCharges[2].EO_ChargeDescription);
			AssertEquals("Other 4", shipment.AWBHeader.AWBOtherCharges[3].EO_ChargeDescription);
		}

		public void TestProcessOTHRRecords_ConsolAsAWBParent()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.AWBOtherCharges.AddNew();
			AssertEquals("Pre-condition", 1, consol.AWBHeader.AWBOtherCharges.Count);
			Processor.ProcessOTHRRecords(consol, 1);
			AssertEquals("Should be clearing the existing ones", 4, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("FS/MY/C", consol.AWBHeader.AWBOtherCharges[0].EO_ChargeDescription);
			AssertEquals("POST /C", consol.AWBHeader.AWBOtherCharges[1].EO_ChargeDescription);
			AssertEquals("AWB  /A", consol.AWBHeader.AWBOtherCharges[2].EO_ChargeDescription);
			AssertEquals("P.C.S/C", consol.AWBHeader.AWBOtherCharges[3].EO_ChargeDescription);
		}

		AWBMessageProcessorForTest Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new AWBMessageProcessorForTest();
				}

				return fProcessor;
			}
		}

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

		void AssertBusinessObjectCreatedOrUpdatedNotification(JASForwardingShipment shipment)
		{
			BusinessObjectCreatedOrUpdatedNotification[] notifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingShipment));
			AssertEquals(1, notifications.Length);
			AssertEquals(shipment, notifications[0].BusinessEntity);
		}

		void AssertJobCharge(JobCharge jobCharge, ZGuid expectedChargeCodePK, ZString expectedDesc, ZString expectedSellCurrencyCode, ZDecimal expectedSellAmount)
		{
			AssertEquals(expectedChargeCodePK, jobCharge.JR_AC);
			AssertEquals(expectedDesc, jobCharge.JR_Desc);
			AssertEquals(expectedSellCurrencyCode, jobCharge.JR_RX_NKSellCurrency);
			AssertEquals(expectedSellAmount, jobCharge.JR_OSSellAmt);
		}

		AWBMessageProcessorForTest fProcessor;
		NotificationBuffer fNotificationBuffer;
		#region AWBMessageProcessorForTest
		class AWBMessageProcessorForTest : AWBMessageProcessor
		{
			public AWBMessageProcessorForTest() : base(new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUMEL;ITNAP;AUCOR;ITMIL;AUMEL"), new MAWBRecord(JXCConstants.LineTypes.MAWB, "n;0;MIL;160;30336633;J.A.S. JET AIR SERVICE S.P.A.;VIA S. MARIA DEL PIANTO N 84;80100 NAPOLI;;081-5841650-;ITROM;JAS FORWARDING WORLWIDE PTY;C/O SMITH LEWIS & STAFF PTY;UNIT 17, MIAC BLDNG INTL DRIVE;03043 TULLAMARINE,VICTORIA;99/99999;AUMEL;CATHAY PACIFIC AIRWAYS;;;;...;JAS SPA MILAN  ITALY;J.A.S. JET AIR SERVICE S.P.A.;20090 SEGRATE - MILANO ITALY;E1J LIN 7576001;38/4/7576/0013;ITMIL;ROM ROME;DOG.FATTA        VIA BARBERINI 3;NON-CEE TRAFFIC00187 ROMA;P.I. 00862211000;C.F. 07277450156;CCIAA 606898;I.TRIB.ROMA1099/86;;MEL;CX;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;CX1050;23/09/2004;;;;TTL PIECES NO  4 -;TO THIS AWB IS ATTACHED CARGO MANIFEST AND ENVELOPE FOR CNEE;;;;;;;T1;;0004;0172.00;K;000002931.12;000002931.12;000000000.00;000000000.00;000000000.00;;;000000006.20;000000000.00;000000067.48;000000000.00;000003004.80;000000000.00;;;;;;;;;;;;;;;;JAS SPA MILAN ITALY;20/09/2004;MILANO;JAS SPA MIL;;;;;;;;;IT;AU;VIA S. MARIA DEL PIANTO N 84;081-5841650-;;;C/O SMITH LEWIS & STAFF PTY;99/99999;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY/C;00034.40;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;POST /C;00031.82;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "0004;;0172.00;K;Q;ZZ;;;00621.0;00004.72;000002931.12;CONSOLIDATE SHIPMENTAS PER ATTACHED     CARGO MANIFEST      TTL CARTONS NO  4   TTL VOLUME CBM 3,726FREIGHT PREPAID;CONSOLIDATE SHIPMENTAS PER ATTACHED;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;AWB  /A;00006.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;PKC;P.C.S/C;00001.26;;;;;;"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB101"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB102"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;7/0400859;ITNAP;IT;7/0400859;NAP;160;30336633;JET AIR SERVICE  SPA0192191015;20090 SEGRATE/MILANO;;;;AVIO S.P.A.;VIALE IMPERO ANG. VIALE ALFA;;;80038 POMIGLIANO;NA;80038;IT;;N;TE  081/3163111;287844;;ADI LIMITED;FINN STREET 3550;;;BENDIGO VICTOR;;XXXXX;AU;;N;99/99999;255945;;;;;;;;N;99/99999;;;;;38/4/7576/0013;;NAPLES;INV.4010409613+4+5+6;;;;;;;MEL;;CX;;;;;EUR;;c;;NVD;EUR;000050812.00;USD;MELBOURNE;CX1050;23/09/2004;;;;ATTACHED COMM.INVOICE AND P.LIST;;;;;;;;;;;4;0172.00;K;000000000.00;000000000.00;;;;;;;;;;;;;20/09/2004;NAP;;;;;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV.4010409613+4+5+6;S"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;001;Other 1;00001.26;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "4;;0172.00;K;Q;ZZ;;;00821.0;ASAGREED;000000821.00;PARTS FOR ENGINE    AIRCRAFT            S/TERMS  DDP        TTL CARTONS NO  4   MARKED P/L 376673+  376674+5+6          FREIGHT PREPAID;PARTS FOR ENGINE    AIRCRAFT;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;002;Other 2;00002.26;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;003;Other 3;00003.26;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "5;;0285.50;L;Q;ZZ;;;02110.0;ASAGREED;000002110.00;Freight Breakdown 2;Some description here;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;004;Other 4;00004.26;;;;;;"), new SHMKRecord(JXCConstants.LineTypes.SHMK, "first;SHIPMENT;forty"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB103"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB104"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB105"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;10/0403155;ITVCE;IT;10/0403155;VCE;176;72809063;JET AIR SERVICE  SPA0192191015;00054 FIUMICINO ROMA;;;;MAG.PETERSANT PERENZIN & C SRL;VIALE XXIV MAGGIO, 56;;;31015 CONEGLIANO;TV;31015;IT;;N;TE  0438/410450;275866;;HUGO BOSS AUSTRALIA;8-14 ALBERT STREET;;;03072     PRESTON;;03072;AU;;N;99/99999;147829;;;;;;;;N;99/99999;;;;;38/4/7576/0061;;VENICE;1333;;;;;;;MEL;;EK;;;;;EUR;;P;;NVD;EUR;000000010.00;EUR;MELBOURNE;EK4040;09/10/2004;;;;ATTACHED INVOICE;;;;;;;;;;;4;0004.00;K;000000054.00;;000000054.00;;;;;;000000121.20;;000000015.97;;000000191.17;;05/10/2004;VCE;;;;000000191.17;;NoFhl;N;;;;;;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MAA;F.O.B;000000030.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "c;MAA;SAF;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "c;MAA;X RAY;000000007.20;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "1333;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "4;;0004.00;K;Q;ZZ;;;00024.0;00002.25;000000054.00;KNITWEAR            39X30X30/4;KNITWEAR            39X30X30/4;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MYC;FS/MY;000000007.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "c;MCC;SC;000000003.60;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;I.A.T;000000005.17;;;;;;"), new SHMKRecord(JXCConstants.LineTypes.SHMK, "second;shipment;forty"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "p;MAA;INLAN;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MAA;CUST.;000000034.00;;;;;;"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") })
			{
			}

			public new bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber)
			{
				return base.ProcessStandardShipment(factoryProvider, bodyRecordIndex, notificationSubscriber);
			}

			public new void ProcessFBDNRecords(IAWBParent aWBParent, int startingIndex)
			{
				base.ProcessFBDNRecords(aWBParent, startingIndex);
			}

			public new void ProcessOTHRRecords(IAWBParent aWBParent, int startingIndex)
			{
				base.ProcessOTHRRecords(aWBParent, startingIndex);
			}

			protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
			{
				return false;
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
	}
}
