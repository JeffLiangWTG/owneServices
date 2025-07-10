using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class OceanMessageExporterTest : AirOceanMessageExporterTestCase
	{
		public void TestExportValidationTypeToUse()
		{
			OceanMessageExporter exporter = new OceanMessageExporter(Consol, NotificationBuffer);
			AssertEquals("Should use JXC ocean validation", JXCExportValidationType.Ocean, exporter.ExportValidationTypeToUse);
			exporter = new OceanMessageExporter(PreShipmentWrapper, NotificationBuffer);
			AssertEquals("Should use JXC ocean validation", JXCExportValidationType.Ocean, exporter.ExportValidationTypeToUse);
		}

		#region PreShipment
		protected override void PreparePreShipment()
		{
			PreShipmentWrapper.SendingForwarderPK = SendingForwarder.PK;
			PreShipmentWrapper.ReceivingForwarderPK = ReceivingForwarder.PK;
			PreShipmentWrapper.SendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			PreShipmentWrapper.Shipment.JS_RL_NKDestination = "IDJKT";
			PreShipmentWrapper.Shipment.JS_HouseBill = "HB 10293";
			PreShipmentWrapper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			PreShipmentWrapper.Shipment.JS_BookingReference = "1235";
			PreShipmentWrapper.Shipment.JS_MarksAndNumbers = "HAHAHAHA";
			JASForwardingPackLine packLine1 = null;
			PreShipmentWrapper.Shipment.JS_JX = Factory.New(typeof(JobSailing)).PK;
			JASForwardingShipment otherShipment1 = CreateNewShipmentWithSailing();
			JASForwardingShipment otherShipment2 = CreateNewShipmentWithSailing();
			AddOuterPackLineAndContainer(null, PreShipmentWrapper.Shipment, "CONT123", "this is the pack", ref packLine1);
			AddOuterPackLineAndContainer(null, otherShipment1, "CONT133", "", ref packLine1);
			AddOuterPackLineAndContainer(null, otherShipment2, "CONT134", "", ref packLine1);
			AddOuterPackLineAndContainer(null, PreShipmentWrapper.Shipment, "CONT234", "another packline");
			JASForwardingPackLine packLine3 = null;
			JASForwardingShipment otherShipment3 = CreateNewShipmentWithSailing();
			AddOuterPackLineAndContainer(null, PreShipmentWrapper.Shipment, "CONT345", "last packline", ref packLine3);
			AddOuterPackLineAndContainer(null, otherShipment3, "CONT355", "", ref packLine3);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 1", 48m);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 2", 64m);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 3", 72m);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 4", 111.11m);
			// These belong to a different branch, should not be included in the export.
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 5", 100m, false, true);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 6", 110m, false, true);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 7", 120m, false, true);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 8", 130m, false, true);
			// These are not collect charges, should not be included in the export
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 9", 140m, true, false);
			AddJobChargeToShipment(PreShipmentWrapper, PreShipmentWrapper.Shipment, "Charge 10", 150m, true, false);
		}

		protected override void PrepareDummyExporterForTestExportedMessage_PreShipment()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "IDJKT";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines = new MessageLine[11];
			lines[0] = new OHBLLine(HouseLevelRecordType.PreShipment, PreShipmentWrapper, new ExportHouseBillOfLading(PreShipmentWrapper, PreShipmentWrapper.Shipment));
			lines[1] = new REFRLine("1235", REFRLine.ReferenceFrom.Shipper);
			lines[2] = new REFRLine("HB 10293", REFRLine.ReferenceFrom.Consignee);
			JASForwardingPackLine firstPackLine = (JASForwardingPackLine)PreShipmentWrapper.Shipment.OuterPackLines[0];
			lines[3] = new CONTLine((ForwardingContainer)firstPackLine.GetContainer(PreShipmentWrapper.Shipment.Sailing), firstPackLine);
			JASForwardingPackLine secondPackLine = (JASForwardingPackLine)PreShipmentWrapper.Shipment.OuterPackLines[1];
			lines[4] = new CONTLine((ForwardingContainer)secondPackLine.GetContainer(PreShipmentWrapper.Shipment.Sailing), secondPackLine);
			JASForwardingPackLine thirdPackLine = (JASForwardingPackLine)PreShipmentWrapper.Shipment.OuterPackLines[2];
			lines[5] = new CONTLine((ForwardingContainer)thirdPackLine.GetContainer(PreShipmentWrapper.Shipment.Sailing), thirdPackLine);
			JobCharge[] jobCharges = GetJobChargesFromShipment(PreShipmentWrapper.Shipment);
			lines[6] = new CHGSLine(jobCharges[0]);
			lines[7] = new CHGSLine(jobCharges[1]);
			lines[8] = new CHGSLine(jobCharges[2]);
			lines[9] = new CHGSLine(jobCharges[3]);
			lines[10] = new SHMKLine(PreShipmentWrapper.Shipment);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("HB 10293", lines) };
			JASDataRegistry.Instance.IncludeProfitShareCharges = true;
		}

		#endregion
		#region Standard Consol
		protected override void PrepareStandardConsol()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_MasterBillNum = "08112345678";
			Consol.JK_RL_NKDischargePort = "USATL";
			Consol.SetDefaultReceivingForwarderAddress(ReceivingForwarder);
			Consol.SetDefaultSendingForwarderAddress(SendingForwarder);
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_HouseBill = "HB11";
			shipment1.JS_BookingReference = "BHB11";
			AddOuterPackLineAndContainer(Consol, shipment1, "CONT123", "this is the pack");
			AddOuterPackLineAndContainer(Consol, shipment1, "CONT234", "pack 2");
			AddOuterPackLineAndContainer(Consol, shipment1, "CONT345", "pack 3");
			// These belong to a different branch, should not be included in the export
			AddJobChargeToShipment(Consol, shipment1, "Charge1", 10m, false, true);
			AddJobChargeToShipment(Consol, shipment1, "Charge2", 30m, false, true);
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			shipment2.JS_HouseBill = "HB12";
			shipment2.JS_MarksAndNumbers = "MN12";
			AddJobChargeToShipment(Consol, shipment2, "Charge 3", 111.34m);
			// This is not a collect charge, should not be included in the export
			AddJobChargeToShipment(Consol, shipment2, "Charge4", 40m, true, false);
		}

		protected override void PrepareDummyExporterForTestExportedMessage_StandardConsol()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "USATL";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines = new MessageLine[11];
			lines[0] = new OMANLine(Consol);
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments[0];
			lines[1] = new OHBLLine(HouseLevelRecordType.Standard, Consol, new ExportHouseBillOfLading(Consol, shipment1));
			lines[2] = new REFRLine(shipment1.JS_BookingReference, REFRLine.ReferenceFrom.Shipper);
			lines[3] = new REFRLine(shipment1.JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			JASForwardingPackLine firstPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[0];
			lines[4] = new CONTLine((ForwardingContainer)firstPackLine.GetContainer(Consol), firstPackLine);
			JASForwardingPackLine secondPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[1];
			lines[5] = new CONTLine((ForwardingContainer)secondPackLine.GetContainer(Consol), secondPackLine);
			JASForwardingPackLine thirdPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[2];
			lines[6] = new CONTLine((ForwardingContainer)thirdPackLine.GetContainer(Consol), thirdPackLine);
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments[1];
			lines[7] = new OHBLLine(HouseLevelRecordType.Standard, Consol, new ExportHouseBillOfLading(Consol, shipment2));
			lines[8] = new REFRLine(shipment2.JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			JobCharge[] jobCharges = GetJobChargesFromShipment(shipment2);
			lines[9] = new CHGSLine(jobCharges[0]);
			lines[10] = new SHMKLine(shipment2);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("08112345678", lines) };
		}

		#endregion
		#region Co-Load Consol
		protected override void PrepareDummyExporterForTestExportedMessage_CoLoadConsol()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "USATL";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines1 = new MessageLine[6];
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments[0];
			lines1[0] = new OHBLLine(HouseLevelRecordType.CoLoad, Consol, new ExportHouseBillOfLading(Consol, shipment1));
			lines1[1] = new REFRLine(shipment1.JS_BookingReference, REFRLine.ReferenceFrom.Shipper);
			lines1[2] = new REFRLine(shipment1.JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			JASForwardingPackLine firstPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[0];
			lines1[3] = new CONTLine((ForwardingContainer)firstPackLine.GetContainer(Consol), firstPackLine);
			JASForwardingPackLine secondPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[1];
			lines1[4] = new CONTLine((ForwardingContainer)secondPackLine.GetContainer(Consol), secondPackLine);
			JASForwardingPackLine thirdPackLine = (JASForwardingPackLine)shipment1.OuterPackLines[2];
			lines1[5] = new CONTLine((ForwardingContainer)thirdPackLine.GetContainer(Consol), thirdPackLine);
			MessageLine[] lines2 = new MessageLine[4];
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments[1];
			lines2[0] = new OHBLLine(HouseLevelRecordType.CoLoad, Consol, new ExportHouseBillOfLading(Consol, shipment2));
			lines2[1] = new REFRLine(shipment2.JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			JobCharge[] jobCharges = GetJobChargesFromShipment(shipment2);
			lines2[2] = new CHGSLine(jobCharges[0]);
			lines2[3] = new SHMKLine(shipment2);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("08112345678_1", lines1), new JXCMessageExporter.MessageFileNameAndContents("08112345678_2", lines2) };
		}

		#endregion
		public void TestExportFileNameWhenOceanBillOfLadingNumberNotSpecified()
		{
			OceanMessageExporterForTest exporter = new OceanMessageExporterForTest(Consol, NotificationBuffer);
			Consol.JK_MasterBillNum = "";
			Consol.JK_UniqueConsignRef = "C001";
			AssertEquals("TST", exporter.GetMessageFileNameFromConsolMasterBill(Consol, "TST"));
			AssertEquals("C001", exporter.GetMessageFileNameFromConsolMasterBill(Consol, ""));
		}

		protected override AirOceanMessageExporter GetNewAirOceanMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			return new OceanMessageExporter(consol, notificationSubscriber);
		}

		protected override AirOceanMessageExporter GetNewAirOceanMessageExporter(PreShipmentWrapper preShipment, INotifications notificationSubscriber)
		{
			return new OceanMessageExporter(preShipment, notificationSubscriber);
		}

		void AddOuterPackLineAndContainer(JASForwardingConsol consol, JASForwardingShipment shipment, ZString containerNum, ZString packageDesc)
		{
			JASForwardingPackLine packLine = null;
			AddOuterPackLineAndContainer(consol, shipment, containerNum, packageDesc, ref packLine);
		}

		void AddOuterPackLineAndContainer(JASForwardingConsol consol, JASForwardingShipment shipment, ZString containerNum, ZString packageDesc, ref JASForwardingPackLine packLine)
		{
			if (packLine == null)
			{
				packLine = (JASForwardingPackLine)shipment.OuterPackLines.AddNew();
				packLine.JL_Description = packageDesc;
			}

			ForwardingContainer container = packLine.Containers.AddNew();
			container.JC_ContainerNum = containerNum;
			container.JC_JX = shipment.SailingPK;
			if (consol != null)
			{
				container.JC_JK = consol.PK;
			}
		}

		void AddJobChargeToShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ZString desc, ZDecimal amount)
		{
			AddJobChargeToShipment(headerData, shipment, desc, amount, true, true);
		}

		void AddJobChargeToShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ZString desc, ZDecimal amount, bool useCurrentCompany, bool isCollectCharge)
		{
			ZGuid companyPK = (useCurrentCompany) ? GlbCompany.CurrentCompany.PK : OtherCompany.PK;
			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, companyPK);
			JobHeader jobHeader = Factory.LoadTop1<JobHeader>(filter);
			if (jobHeader == null)
			{
				jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_ParentID = shipment.PK;
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeader.JH_GC = companyPK;
			}

			JobCharge newCharge = Factory.NewWithValidTestData<JobCharge>();
			newCharge.JR_Desc = desc;
			newCharge.JR_OSSellAmt = amount;
			newCharge.JR_JH = jobHeader.PK;
			newCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			newCharge.JR_OH_SellAccount = (isCollectCharge) ? headerData.ReceivingForwarder.PK : ZGuid.Empty;
			jobHeader.LoadCharges_ForTestOnly();
		}

		JobCharge[] GetJobChargesFromShipment(JASForwardingShipment shipment)
		{
			JobCharge[] result = null;
			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			JobHeader jobHeader = Factory.LoadTop1<JobHeader>(filter);
			if (jobHeader != null)
			{
				result = (JobCharge[])Factory.Load(typeof(JobCharge), new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
			}

			return result;
		}

		JASForwardingShipment CreateNewShipmentWithSailing()
		{
			JASForwardingShipment result = Factory.New<JASForwardingShipment>();
			result.JS_JX = Factory.New(typeof(JobSailing)).PK;
			return result;
		}

		GlbCompany OtherCompany
		{
			get
			{
				if (fOtherCompany == null)
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
					fOtherCompany = Factory.LoadTop1<GlbCompany>(filter);
				}

				return fOtherCompany;
			}
		}

		GlbCompany fOtherCompany;
		#region class OceanMessageExporterForTest
		class OceanMessageExporterForTest : OceanMessageExporter
		{
			public OceanMessageExporterForTest(JASForwardingConsol consol, INotifications notification) : base(consol, notification)
			{
			}

			public new ZString GetMessageFileNameFromConsolMasterBill(JASForwardingConsol consol, ZString suffix)
			{
				return base.GetMessageFileNameFromConsolMasterBill(consol, suffix);
			}
		}
		#endregion
	}
}
