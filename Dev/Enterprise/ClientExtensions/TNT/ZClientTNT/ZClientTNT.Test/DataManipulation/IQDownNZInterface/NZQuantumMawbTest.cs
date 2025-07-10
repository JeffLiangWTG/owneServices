using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;
using UniversalReferenceConstants = Enterprise.Customs.NZ.Business.UniversalReferenceConstants;

namespace Enterprise.Client.TNT.NZ.Testing
{
	[TestedType(typeof(NZQuantumMawb))]
	public class NZQuantumMawbTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipmentNotUpdatedIfSentToCustoms()
		{
			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			NZQuantumMawb mawb = (NZQuantumMawb)GetNewBusinessObject();
			ForwardingConsol consol = CreateConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HWBDUMMY01";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_ActualWeight = 4m;
			shipment.JS_GoodsDescription = "Modems";
			JobDeclaration jobdec = Factory.New<JobDeclaration>();
			jobdec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			jobdec.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			jobdec.JE_JS = shipment.PK;
			Factory.Save();
			shipment.JS_E_ARV = new ZDateTime(2006, 11, 15);
			shipment.JS_E_DEP = new ZDateTime(2006, 11, 14);
			Factory.Save();
			int numberOfShipmentsBefore = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			mawb.LinkedConsol = consol;
			NotificationBuffer notify = new NotificationBuffer();
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(notify, true);
			AssertEquals("Zero Shipment should be created, only one updated", numberOfShipmentsBefore, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("1 shipment should only be attached to consol", 1, consol.Shipments.Count);
			AssertEquals("Estimated arrival date should not be updated", new ZDateTime(2006, 11, 15), shipment.JS_E_ARV);
			AssertEquals("Estimated departure date should not be updated", new ZDateTime(2006, 11, 14), shipment.JS_E_DEP);
		}

		public void TestShipmentsAreNotDetached()
		{
			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			NZQuantumMawb mawb = (NZQuantumMawb)GetNewBusinessObject();
			ForwardingConsol consol = CreateConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "608214785";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_ActualWeight = 4m;
			shipment.JS_GoodsDescription = "Modems";
			Factory.Save();
			int numberOfShipmentsBefore = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			mawb.LinkedConsol = consol;
			NotificationBuffer notify = new NotificationBuffer();
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(notify, true);
			AssertEquals("One Shipment should be created", numberOfShipmentsBefore + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("2 shipments should be attached to consol", 2, consol.Shipments.Count);
		}

		public void TestDeclarationIsCreated()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.ReadyForManifesting, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.WriteOff);
		}

		public void TestDeclarationIsCreated_IsX2()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Export, true);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.ReadyForManifesting, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff);
		}

		public void TestTSWDeclarationGoodsLocatedAtIsNotOverridden()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, true);
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(new NotificationBuffer(), true);
			AssertEquals("One Shipment should be created", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("New Declaration should be Created", 1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			var jobShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery());
			var jobDec = Factory.LoadTop1<JobDeclaration>(new ZQuery());
			AssertEquals("Should be an ECI Write off", JobMessageSubTypeList.Codes.WriteOff, jobDec.JE_MessageSubType);
			AssertEquals("Should be ready for manifesting", LowValueConsignmentStatusList.Codes.ReadyForManifesting, jobDec.JE_EntryStatus);
			AssertEquals("JobDeclaration should be an Import", Customs.Business.JobMessageTypeList.Codes.Import, jobDec.JE_MessageType);
			AssertEquals("Goods Located at 'FW' should not get overridden", GoodsLocatedAtList.Codes.FW, jobDec.JE_GoodsLocatedAt);
			AssertEquals("Bonded Warehouse should default to current branch OrgProxy", GlbBranch.CurrentBranch.OrgProxy.PK, jobDec.WarehouseDocAddress.OrganisationPK);
			AssertEquals("Forwarder should default to current branch OrgProxy", GlbBranch.CurrentBranch.OrgProxy.PK, jobDec.Forwarder.PK);
		}

		public void TestJobDeclarationCreateIsImport()
		{
			var mawb = InitialNZQuantumMawb(null, false);
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(new NotificationBuffer(), true);
			AssertEquals("One Shipment should be created", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("New Declaration should be Created", 1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			ForwardingShipment jobShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery());
			JobDeclaration jobDec = Factory.LoadTop1<JobDeclaration>(new ZQuery());
			AssertEquals("Should be an ECI Write off", JobMessageSubTypeList.Codes.WriteOff, jobDec.JE_MessageSubType);
			AssertEquals("Export date should be the same as DateAtOrigin", jobDec.JE_DateAtOrigin, jobDec.JE_ExportDate);
			AssertEquals("Should be ready for manifesting", LowValueConsignmentStatusList.Codes.ReadyForManifesting, jobDec.JE_EntryStatus);
			AssertEquals("Should be linked to a shipment", jobShipment.PK, jobDec.JE_JS);
			AssertEquals("Branch should be SYD", "SYD", jobDec.Branch.GB_Code);
			AssertEquals("JobDeclaration should be an export", Customs.Business.JobMessageTypeList.Codes.Import, jobDec.JE_MessageType);
			AssertEquals("PackType should be 'PK'", UniversalReferenceConstants.PackageTypeListCodes.Package, jobDec.JE_TotalNoOfPacksPackType);
		}

		public void TestLinkedConsol()
		{
			MissingShippingLineEventWasRaised = false;
			NZQuantumMawb mawb = (NZQuantumMawb)GetNewBusinessObject();
			try
			{
				mawb.MissingShippingLineOnConsolEvent += new EventHandler(Mawb_MissingShippingLineOnConsolEvent);
				OrgHeader airLineOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C00001000";
				mawb.LinkedConsol = consol;
				Assert("MissingShippingLineEvent should be raised if the Consol does not have a Carrier/ShippingLine", MissingShippingLineEventWasRaised);
				AssertNull("LinkedConsol should be null", mawb.LinkedConsol);
				AssertEquals("UniqueConsignRef should be empty", ZString.Empty, mawb.LinkedConsolUniqueConsignRef);
				MissingShippingLineEventWasRaised = false;
				consol.SetDefaultShippingLineAddress(airLineOrg);
				mawb.LinkedConsol = consol;
				Assert("MissingShippingLineEvent should NOT be raised. The Consol has a Carrier/ShippingLine", !MissingShippingLineEventWasRaised);
				AssertEquals("LinkedConsol should be Linked", consol.PK, mawb.LinkedConsol.PK);
				AssertEquals("UniqueConsignRef should C00001000", "C00001000", mawb.LinkedConsolUniqueConsignRef);
				mawb.LinkedConsol = consol;
				Assert("Should be events raised if LinkedCosol and Value are the same", !MissingShippingLineEventWasRaised);
				AssertEquals("LinkedConsol should be Linked", consol.PK, mawb.LinkedConsol.PK);
				AssertEquals("UniqueConsignRef should still be C00001000", "C00001000", mawb.LinkedConsolUniqueConsignRef);
				mawb.LinkedConsol = null;
				Assert("MissingShippingLineEvent should NOT be raised. Should not be checking ShippingLine on a null value", !MissingShippingLineEventWasRaised);
				AssertNull("Linked consol should now be null", mawb.LinkedConsol);
				AssertEquals("UniqueConsignRef should refreshed, it should be empty", ZString.Empty, mawb.LinkedConsolUniqueConsignRef);
			}
			finally
			{
				mawb.MissingShippingLineOnConsolEvent -= new EventHandler(Mawb_MissingShippingLineOnConsolEvent);
			}
		}

		public void TestBlankCurrency()
		{
			NZQuantumMawb mawb1 = (NZQuantumMawb)GetNewBusinessObject();
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobDeclaration jobDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			shipment1.JS_RX_NKGoodsValueCurr = "";
			jobDeclaration1 = mawb1.CreateDefaultInvoiceForDeclaration(shipment1, jobDeclaration1);
			Assert(jobDeclaration1.Invoices[0].JZ_RX_NKInvoice_Currency == "NZD");
			NZQuantumMawb mawb2 = (NZQuantumMawb)GetNewBusinessObject();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobDeclaration jobDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			shipment2.JS_RX_NKGoodsValueCurr = "EUR";
			jobDeclaration2 = mawb2.CreateDefaultInvoiceForDeclaration(shipment2, jobDeclaration2);
			Assert(jobDeclaration2.Invoices[0].JZ_RX_NKInvoice_Currency == "EUR");
		}

		public void TestDeclarationWithHighConsignmentValueNZD()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false, ShipmentRecordStringWithHighConsignmentValueNZD);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.NotSentToCustoms, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Simplified);
		}

		public void TestDeclarationWithHighConsignmentValueAUD()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false, ShipmentRecordStringWithHighConsignmentValueAUD);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.NotSentToCustoms, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Simplified);
		}

		public void TestDeclarationWithLowConsignmentValueNZD()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false, ShipmentRecordStringWithLowConsignmentValueNZD);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.ReadyForManifesting, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.WriteOff);
		}

		public void TestDeclarationWithLowConsignmentValueAUD()
		{
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false, ShipmentRecordStringWithLowConsignmentValueAUD);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.ReadyForManifesting, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.WriteOff);
		}

		public void TestDeclarationWithMissingConversionRate()
		{
			SetMissingCustomsExchangeRateForTesting();
			var mawb = InitialNZQuantumMawb(JobMessageTypeList.Codes.Import, false, ShipmentRecordStringWithLowConsignmentValueAUD);
			AssertDeclarationInfos(mawb, LowValueConsignmentStatusList.Codes.NotSentToCustoms, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Simplified);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpAUDCurrency();
		}

		void SetUpAUDCurrency()
		{
			var audRate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDate.Today, Core.Constants.CurrencyCodes.Australia, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			if (audRate == null)
			{
				audRate = Factory.New<RefExchangeRate>();
				audRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				audRate.RE_StartDate = new ZDateTime(2000, 1, 1);
				audRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			}

			audRate.RE_SellRate = 0.93m;
		}

		NZQuantumMawb InitialNZQuantumMawb(ZString messageType, bool isX2, string shipmentRecordString = ShipmentRecordString)
		{
			if (!isX2)
			{
				SetLowValueThresholdForTesting();
				TNTDataRegistry.Instance.INDFileImportConsignmentValueThreshold = 400.00m;
			}

			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			var mawb = (NZQuantumMawb)GetNewBusinessObject(isX2, shipmentRecordString);
			Factory.GetDatabaseCount(typeof(JobDeclaration));
			var consol = CreateConsol();
			if (messageType == JobMessageTypeList.Codes.Import)
			{
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "NZAKL";
			}
			else if (messageType == JobMessageTypeList.Codes.Export)
			{
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "GBLHR";
			}

			Factory.Save();
			AssertEquals("Precondition: 0 shipments", 0, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("Precondition: 0 declarations", 0, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			mawb.LinkedConsol = consol;
			return mawb;
		}

		void SetMissingCustomsExchangeRateForTesting()
		{
			var audRate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDate.Today, Core.Constants.CurrencyCodes.Australia, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			audRate.Delete();
			Factory.Save();
		}

		void AssertDeclarationInfos(NZQuantumMawb mawb, ZString entryStatus, ZString messageType, ZString messageSubType)
		{
			var notify = new NotificationBuffer();
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(notify, true);
			AssertEquals("One Shipment should be created", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("New Declaration should be Created", 1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			var jobShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery());
			var jobDec = Factory.LoadTop1<JobDeclaration>(new ZQuery());
			AssertEquals("JE_MessageSubType Should be " + messageSubType, messageSubType, jobDec.JE_MessageSubType);
			AssertEquals("Export date should be the same as DateAtOrigin", jobDec.JE_DateAtOrigin, jobDec.JE_ExportDate);
			AssertEquals("JE_EntryStatus Should be" + entryStatus, entryStatus, jobDec.JE_EntryStatus);
			AssertEquals("Should be linked to a shipment", jobShipment.PK, jobDec.JE_JS);
			AssertEquals("Branch should be SYD", "SYD", jobDec.Branch.GB_Code);
			AssertEquals("JE_MessageType should be " + messageType, messageType, jobDec.JE_MessageType);
			AssertEquals("PackType should be 'PK'", UniversalReferenceConstants.PackageTypeListCodes.Package, jobDec.JE_TotalNoOfPacksPackType);
			AssertEquals("Goods Located should be \"FW\"(Bonded Warehouse)", GoodsLocatedAtList.Codes.FW, jobDec.JE_GoodsLocatedAt);
			AssertEquals("Bonded Warehouse should default to current branch OrgProxy", GlbBranch.CurrentBranch.OrgProxy.PK, jobDec.WarehouseDocAddress.OrganisationPK);
			AssertEquals("1 Invoice headers should be created", 1, jobDec.Invoices.Count);
			IList<BaseJobComInvoiceHeader> matchedInvoices = new List<BaseJobComInvoiceHeader>(jobDec.Invoices.Find(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "1")));
			var invoice = (JobComInvoiceHeader)matchedInvoices[0];
			AssertEquals("Supplier should be the the Consignor", jobShipment.ConsignorPK, invoice.JZ_OH_Supplier);
			AssertEquals("Invoice amount should be the goods value", jobShipment.JS_GoodsValue, invoice.JZ_InvoiceAmount);
			AssertEquals("Invoice Currency should be Goods Currency", jobShipment.GoodsValueCurr.RX_Code, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("IncoTerm should be the JS_INCO", jobShipment.JS_INCO, invoice.JZ_IncoTerm);
			AssertEquals("Invoice Weight should be Shipment Weight", jobShipment.JS_ActualWeight, invoice.JZ_Weight);
			AssertEquals("Invoice Unit of weight should be the Shipment Unit of weight", jobShipment.JS_UnitOfWeight, invoice.JZ_WeightUQ);
			AssertEquals("1 Invoice line should be created", 1, invoice.JobComInvoiceLines.Count);
			var line = invoice.JobComInvoiceLines[0];
			AssertEquals("Line Number should be 1", (short)1, line.JI_LineNo);
			AssertEquals("Invoice Quantity should be same as OuterPacks", new ZDecimal(jobShipment.JS_OuterPacks), line.JI_InvoiceQuantity);
			AssertEquals("Line Price should be goods value", jobShipment.JS_GoodsValue, line.JI_LinePrice);
			AssertEquals("Weight should Shipment Actual Weight", jobShipment.JS_ActualWeight, line.JI_Weight);
			AssertEquals("Weight Unit should be Shipments Unit of weight", jobShipment.JS_UnitOfWeight, line.JI_WeightUQ);
			AssertNotEquals("Goods Description should NOT be the same as shipment", jobShipment.JS_GoodsDescription, line.JI_Description);
			AssertEquals("Goods Description should be the same as declaration", jobDec.JE_GoodsDescription, line.JI_Description);
			AssertEquals("Goods Origin should be the same as shipment", jobShipment.JS_RL_NKOrigin.SubstringSafe(0, 2), line.JI_CountryOfOrigin);
			AssertEquals("Goods Origin should be the same as declaration", jobDec.JE_RL_NKOrigin.SubstringSafe(0, 2), line.JI_CountryOfOrigin);
			AssertNotEquals("Package Type should NOT be the same as shipment", jobShipment.JS_F3_NKPackType, line.JI_InvoiceUQ);
			AssertEquals("Package Type should be the same as declaration", jobDec.JE_TotalNoOfPacksPackType, line.JI_InvoiceUQ);
			notify.Clear();
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(notify, true);
			AssertEquals("Should still be only 1 shipment in database", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("Should still be only 1 declaration in database", 1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
		}

		ForwardingConsol CreateConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2006, 6, 22);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2006, 6, 23);
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "QF0151";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08622592112";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.SetDefaultShippingLineAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			return consol;
		}

		bool MissingShippingLineEventWasRaised;
		void Mawb_MissingShippingLineOnConsolEvent(object sender, EventArgs e)
		{
			MissingShippingLineEventWasRaised = true;
		}

		void SetLowValueThresholdForTesting()
		{
			var refDataGrouping = Factory.LoadTop1<RefDataGrouping>(new ZQuery(RefDataGroupingSchema.ZZZ_DataGrouping, Core.Constants.CountryCodes.NewZealand));
			if (refDataGrouping == null)
			{
				refDataGrouping = Factory.New<RefDataGrouping>();
				refDataGrouping.ZZZ_DataGrouping = Core.Constants.CountryCodes.NewZealand;
				refDataGrouping.ZZZ_Description = "Auto-created test country " + Core.Constants.CountryCodes.NewZealand;
				Factory.Save();
			}

			var lowValueThreshold = Factory.New<RefCusTaxOrFee>();
			lowValueThreshold.ZZF_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.NewZealand;
			lowValueThreshold.ZZF_Description = "Low Value Threshold";
			lowValueThreshold.ZZF_Code = RateTypes.Deminimus;
			lowValueThreshold.ZZF_StartDate = new ZDateTime(1900, 1, 1);
			lowValueThreshold.ZZF_EndDate = new ZDateTime(2079, 6, 6);
			lowValueThreshold.ZZF_Value = 200.0;
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(false);
		}

		BusinessObject GetNewBusinessObject(bool isX2, string shipmentRecordString = ShipmentRecordString)
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			QuantumConsolRecord consolRecord = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord shipment = QuantumShipmentRecord.New("SYD", "", shipmentRecordString);
			QuantumShipmentNotesRecord shipmentNote = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumSegment segment = new QuantumSegment(consolRecord, new QuantumShipmentRecord[] { shipment }, new QuantumShipmentNotesRecord[] { shipmentNote });
			return new NZQuantumMawb(Factory, segment, isX2);
		}

		const string ConsolRecordString = "01BA0151SINSYD150705AMAWBDUMMY123M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string ShipmentRecordStringBase = "03HWBDUMMY01ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME ";
		const string ShipmentRecordString = ShipmentRecordStringBase + "NS       234.34NZD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordStringWithHighConsignmentValueNZD = ShipmentRecordStringBase + "NS       999.99NZD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordStringWithHighConsignmentValueAUD = ShipmentRecordStringBase + "NS       399.00AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordStringWithLowConsignmentValueNZD = ShipmentRecordStringBase + "NS       300.00NZD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordStringWithLowConsignmentValueAUD = ShipmentRecordStringBase + "NS       300.00AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordNoteString = "04HWBDUMMY0101123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
	}
}
