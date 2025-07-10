using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Module;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Testing
{
	[TestedType(typeof(UPEClientDbSchemaUpgradeInfo))]
	class UPEClientDbSchemaUpgradeInfoTest : ConstraintForClientSpecificSchema
	{
		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var scripts = new List<DatabaseObjectCreateScript>();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					try
					{
						ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}
				AssertEquals("The create script should create the table", true, ExistsDbObject(UPEClientTables.ClientBISIShipmentHeader.TableName));
				AssertEquals("The create script should create the table", true, ExistsDbObject(UPEClientTables.ClientBISIShipmentCharge.TableName));
				AssertEquals("The create script should create the table", true, ExistsDbObject(ClientPWSHeaderSchema.Constants.TableName));
				AssertEquals("The create script should create the table", true, ExistsDbObject(ClientPWSChargeSchema.Constants.TableName));

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}
				AssertEquals("The drop script should drop the table", false, ExistsDbObject(UPEClientTables.ClientBISIShipmentHeader.TableName));
				AssertEquals("The drop script should drop the table", false, ExistsDbObject(UPEClientTables.ClientBISIShipmentCharge.TableName));
				AssertEquals("The create script should create the table", false, ExistsDbObject(ClientPWSHeaderSchema.Constants.TableName));
				AssertEquals("The create script should create the table", false, ExistsDbObject(ClientPWSChargeSchema.Constants.TableName));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public override void TestAllIndexesMustSetAllowPageLocksToOffForClientSpecificSchema()
		{
			Assert("Will remove this method next PR", true);
		}

		#region Views

		public void TestClientBISIUploadWarningReport()
		{
			#region setup data

			var uploadDate = ZDateTime.Now.AddSeconds(-ZDateTime.Now.Second);
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;
			var hawb = Factory.NewWithValidTestData<UPECusHAWB>();
			hawb.CS_HAWB = "hawb";
			hawb.CS_IsResponsePending = ZBool.True; // ACAStatus = 'WAIT' or else = CS_CustomsStatus
			hawb.CS_CustomsStatus = "blah";
			hawb.CurrentQueue.P4_CustomFlag1 = ZBool.False; // exclude from upload warning report
			hawb.CurrentQueue.P4_CustomDate1 = uploadDate.AddMinutes(-61); // upload date
			hawb.CurrentQueue.P4_CustomAttrib3 = BillingTermsCodeDescriptionPairList.Codes.FreightCollect; // won't return if Prepaid
			hawb.CS_CM = mawb.PK;

			var relatedWayBill = Factory.NewWithValidTestData<JobRelatedWayBill>();
			relatedWayBill.EB_WaybillType = "PAR";
			relatedWayBill.EB_WaybillShortNumber = hawb.CS_HAWB;
			relatedWayBill.EB_ParentID = hawb.PK;

			var bisiHeader = Factory.NewWithValidTestData<ClientBISIShipmentHeader>();
			bisiHeader.T8_CS = hawb.PK;
			bisiHeader.T8_UploadBatchNumber = 10;
			var bisiCharge = bisiHeader.Charges.AddNew();
			bisiCharge.T9_ChargeType = "blah";
			bisiCharge.T9_GrossAmount = 11.1m;

			var pwsHeader = Factory.NewWithValidTestData<ClientPWSHeader>();
			pwsHeader.U1_WayBillNumber = "bob";
			pwsHeader.U1_WayBillShortNumber = "short bob";

			Factory.Save();

			#endregion

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var sql = "SELECT * FROM vw_Report_ClientBISIUploadWarningReport";
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("Should not produce any records", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("Should produce one record", 1, table.Rows.Count);

			var row = table.Rows[0];
			Assert((Guid)row["CusHAWBPK"] == hawb.PK.ToGuid());
			Assert((string)row["ShortHAWB"] == hawb.CS_HAWB);
			Assert((string)row["ACAStatus"] == "WAIT");
			Assert((string)row["FreightTerms"] == "Freight Collect");
			Assert((string)row["BillingTermsDescription"] == "Freight Collect");
			Assert((string)row["BillingTerms"] == BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			Assert((int)row["BatchNumber"] == 10);
			Assert((string)row["ChargeType"] == "blah");
			Assert((decimal)row["GrossAmount"] == 11.1m);
			Assert(((DateTime)row["BISIUploadDate"]).ToString() == uploadDate.AddMinutes(-61).ToDateTime().ToString());

			pwsHeader.U1_WayBillNumber = hawb.CS_HAWB;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			Assert(table.Rows.Count == 0);
		}
		#endregion

		#region Functions

		public void TestClientCMRMissingSupplierImporterCodesReport()
		{
			#region setup data for report

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_IsCancelled = ZBool.False;
			dec.JE_DeclarationReference = "dec ref";
			dec.JE_HouseBill = "housebill";
			dec.JE_MasterBill = "masterbill";
			dec.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
			dec.JE_RS_NKServiceLevel = "slv";
			dec.JE_TotalNoOfPacks = 110;
			dec.JE_GB = Env.CurrentBranchPK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "importer";
			importer.OH_FullName = "importer fullname";
			importer.MainAddress.OA_Address1 = "importer address 1";
			importer.MainAddress.OA_Address2 = "importer address 2";
			importer.MainAddress.OA_City = "importer city";
			importer.MainAddress.OA_Phone = "importer phone";
			dec.JE_OH_Importer = importer.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "supplier";
			supplier.OH_FullName = "supplier fullname";
			supplier.MainAddress.OA_Address1 = "supplier address 1";
			supplier.MainAddress.OA_Address2 = "supplier address 2";
			supplier.MainAddress.OA_City = "supplier city";
			supplier.MainAddress.OA_Phone = "supplier phone";
			dec.JE_OH_Supplier = supplier.PK;

			Factory.Save();

			#endregion

			UPEDataRegistry.Instance.EnableUPECustomisations = false;

			dec.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Compiling;
			var sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.True.ToString());
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			DataRow row = table.Rows[0];
			Assert((Guid)row["DeclarationPK"] == dec.PK);
			Assert((string)row["DeclarationReference"] == "dec ref");
			Assert((string)row["HAWB"] == "housebill");
			Assert((DateTime)row["Arrival"] == new ZDateTime(2000, 1, 1));
			Assert((string)row["ServiceLevel"] == "slv");
			Assert((int)row["TotalNoOfPacks"] == 110);
			Assert((string)row["Code"] == "importer");
			Assert((string)row["FullName"] == "importer fullname");
			Assert((string)row["Address1"] == "importer address 1");
			Assert((string)row["Address2"] == "importer address 2");
			Assert((string)row["City"] == "importer city");
			Assert((string)row["Phone"] == "importer phone");
			Assert((string)row["MAWB"] == "masterbill");

			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			row = table.Rows[0];
			Assert((Guid)row["DeclarationPK"] == dec.PK);
			Assert((string)row["DeclarationReference"] == "dec ref");
			Assert((string)row["HAWB"] == "housebill");
			Assert((DateTime)row["Arrival"] == new ZDateTime(2000, 1, 1));
			Assert((string)row["ServiceLevel"] == "slv");
			Assert((int)row["TotalNoOfPacks"] == 110);
			Assert((string)row["Code"] == "supplier");
			Assert((string)row["FullName"] == "supplier fullname");
			Assert((string)row["Address1"] == "supplier address 1");
			Assert((string)row["Address2"] == "supplier address 2");
			Assert((string)row["City"] == "supplier city");
			Assert((string)row["Phone"] == "supplier phone");
			Assert((string)row["MAWB"] == "masterbill");

			dec.CurrentQueue.P4_CustomsQueue = "XXX";
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			dec.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			OrgCusCode cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "111111111";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.True.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.True.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.True.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = "XXX";
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.True.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "111111111";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 0, table.Rows.Count);

			cusCode.OK_CodeType = "XXX";
			Factory.Save();
			sql = string.Format("SELECT * FROM ClientCMRMissingSupplierImporterCodesReport('{0}')", ZBool.False.ToString());
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);
		}

		public void TestClientGetPaymentMethodDescription()
		{
			foreach (UPECargoPaymentMethod method in Enum.GetValues(typeof(UPECargoPaymentMethod)))
			{
				string actualDescription = (string)ExecuteScalar("select dbo.ClientGetPaymentMethodDescription('" + (int)method + "')");
				AssertEquals("Correct payment method description", method.ToString(), actualDescription.Replace(" ", ""));
			}
		}

		public void TestClientGetBillingTermsDescription()
		{
			foreach (CodeDescriptionPair pair in new BillingTermsCodeDescriptionPairList())
			{
				string actualDescription = (string)ExecuteScalar("select dbo.ClientGetBillingTermsDescription('" + pair.Code + "')");
				AssertEquals("Correct billing terms description", pair.Description, actualDescription);
			}
		}

		public void TestClientGetReasonCodeDescription()
		{
			foreach (CodeDescriptionPair pair in new ReasonCodeDescriptionPairList())
			{
				string actualDescription = (string)ExecuteScalar("select dbo.ClientGetReasonCodeDescription('" + pair.Code.Substring(pair.Code.Length - 2) + "')");
				AssertEquals("Correct reason code description", pair.Description, actualDescription);
			}
		}

		public void TestClientGetCusHAWBConsigneeName()
		{
			CusHAWB hAWB = Factory.NewWithValidTestData<CusHAWB>();

			hAWB.CS_ConsigneeName = "CusHAWBConsigneeName";
			Factory.Save();
			string consigneeNameFromCusHAWB = (string)ExecuteScalar("select dbo.ClientGetCusHAWBConsigneeName('" + hAWB.PK + "')");
			AssertEquals("Correct consignee name from dbo.CusHAWB", "CusHAWBConsigneeName", consigneeNameFromCusHAWB);

			hAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			hAWB.Consignee.OH_FullName = "OrgHeaderConsigneeName";
			Factory.Save();
			string consigneeNameFromOrgHeader = (string)ExecuteScalar("select dbo.ClientGetCusHAWBConsigneeName('" + hAWB.PK + "')");
			AssertEquals("Correct consignee name from dbo.OrgHeader", "OrgHeaderConsigneeName", consigneeNameFromOrgHeader);
		}

		[TestDate(2009, 12, 31)]
		public void TestClientFinanceReleaseReport_DontIncludeSubsequentSplitShipments()
		{
			var mAWB = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);

			var splitShipment = Factory.NewWithValidTestData<Callout>();
			splitShipment.CS_HAWB = "SplitHAWB";
			splitShipment.InvoiceNumber = "1";
			splitShipment.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			splitShipment.CS_CM = mAWB.PK;

			var subsequentSplitShipment = Factory.NewWithValidTestData<Callout>();
			subsequentSplitShipment.CS_HAWB = "SplitHAWB";
			subsequentSplitShipment.InvoiceNumber = "2";
			subsequentSplitShipment.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			subsequentSplitShipment.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			subsequentSplitShipment.CS_CM = mAWB.PK;

			var command = Db.Connection.Command("select * from ClientFinanceReleaseReport('2000-01-01', '2010-01-01', 'ALL')");

			Factory.Save();
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			using (var reader = command.ExecuteReader())
			{
				ZString readInvoiceNumber = "";

				int count = 0;
				while (reader.Read())
				{
					readInvoiceNumber = (string)reader["InvoiceNumber"];
					count++;
				}
				AssertEquals("Should produce no records", 0, count);
			}

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			using (var reader = command.ExecuteReader())
			{
				ZString readInvoiceNumber = "";

				int count = 0;
				while (reader.Read())
				{
					readInvoiceNumber = (string)reader["InvoiceNumber"];
					count++;
				}
				AssertEquals("The first part of the split shipment should be returned and not the subsequent", 1, count);
				AssertEquals("The first part of the split shipment should be returned and not the subsequent", "1", readInvoiceNumber);
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestClientFinanceReleaseReport_ReleaseDate()
		{
			foreach (ZString completedQueueName in CommercialQueueCodeDescriptionPairList.CompletedQueueNames)
			{
				TestClientFinanceReleaseReport_ReleaseDate(completedQueueName);
			}
		}

		public void TestClientFinanceReleaseReport_ReleaseDate(ZString completedQueueName)
		{
			var mAWB = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			var callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_HAWB = "SplitHAWB";
			callout.CS_CM = mAWB.PK;

			TestDateAttribute.Date = new DateTime(2006, 1, 1);
			callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 2, 2);
			callout.CurrentQueue.P4_QueueName = completedQueueName;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 3, 3);
			callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var command = Db.Connection.Command("select * from ClientFinanceReleaseReport('2000-01-01', '2010-01-01', 'ALL')");
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals("Should produce no records", false, ((SqlDataReader)reader).HasRows);
			}

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				var releaseDate = new ZDateTime(reader["ReleaseDate"]);
				AssertEquals("ReleaseDate should be when finance first moves into a completed queue (" + completedQueueName + ")", new ZDateTime(2006, 2, 2), releaseDate);
			}
		}

		[TestDate(2005, 1, 1)]
		public void TestClientADPScoringReport_RefundPoints()
		{
			ClientRefund refund = Factory.NewWithValidTestData<UPEJobDeclaration>().RefundManager.CreateClientRefund();
			refund.PostRefund = true;
			PopulateClientRefundWithData(refund);
			Factory.Save();

			refund.ProcessRefund = true;
			TestDateAttribute.Date = new DateTime(2005, 1, 15);
			Factory.Save();
			AssertEquals("T10_DateCreated initially", new ZDateTime(2005, 1, 1), refund.T10_DateCreated);
			AssertEquals("T10_DateProcessed initially", new ZDateTime(2005, 1, 15), refund.T10_DateProcessed);

			AssertRefundPoints("Refund not quite 2 weeks outstanding", -10, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 14));
			AssertRefundPoints("Refund 2 weeks outstanding", -20, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 15));
			AssertRefundPoints("Refund filter range < 1 week", 0, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 7));
			AssertRefundPoints("Refund filter range 1 week", -10, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 8));
			AssertRefundPoints("Refund filter range 1 week", -10, new ZDateTime(2005, 1, 7), new ZDateTime(2005, 1, 14));
			AssertRefundPoints("Refund filter range < 1 week", 0, new ZDateTime(2005, 1, 8), new ZDateTime(2005, 1, 14));

			refund.T10_DateCreated = new ZDateTime(2005, 1, 7);
			refund.T10_DateProcessed = ZDateTime.Empty;
			Factory.Save();
			AssertRefundPoints("When refund not yet closed, refund should be from open (7th) to end filter date (30th)", -30, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 30));
		}

		void PopulateClientRefundWithData(ClientRefund refund)
		{
			refund.T10_EnquiryContact = "Contact";
			refund.T10_EnquiryPhoneNumber = "123";
			refund.T10_EnquiryDetails = "T10_EnquiryDetails";
			refund.T10_EnquiryRaisedBy = refund.RaisedByList[0].Code;
		}

		public void TestClientOrgJobNumbers()
		{
			var declaration1 = Factory.New<UPEJobDeclaration>();
			var declaration2 = Factory.New<UPEJobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			declaration1.JE_OH_Importer = importer.PK;
			declaration2.JE_OH_Importer = importer.PK;
			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Rebill;

			Factory.Save();
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var table = Utilities.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM ClientOrgJobNumbers ('{0}')", importer.PK.ToString()));
			AssertEquals("should not return any declarations", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			table = Utilities.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM ClientOrgJobNumbers ('{0}')", importer.PK.ToString()));
			AssertEquals("should return 2 declarations", 2, table.Rows.Count);

			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM ClientOrgJobNumbers ('{0}')", importer.PK.ToString()));
			AssertEquals("should return 1 declaration", 1, table.Rows.Count);
		}

		#region TestClientUPEBPWReport

		[TestDate(2006, 8, 11)]
		public void TestClientUPEBPWReport()
		{
			CreateBusinessObjectsForClientUPEBPWReport();
			var sql = string.Format("SELECT * FROM ClientUPEBPWReport('{0}', '{0}')", ZDateTime.Today.SqlFormat);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();

			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("AirCargoLettersCreatedCount", 1, row["AirCargoLettersCreatedCount"]);
			AssertEquals("AirCargoDocsCreatedCount", 1, row["AirCargoDocsCreatedCount"]);
			AssertEquals("AirCargoNonDocsCreatedCount", 1, row["AirCargoNonDocsCreatedCount"]);
			AssertEquals("FRMDeclarationCreatedCount", 1, row["FRMDeclarationCreatedCount"]);
			AssertEquals("SACDeclarationCreatedCount", 1, row["SACDeclarationCreatedCount"]);
			AssertEquals("SWLDeclarationCreatedCount", 1, row["SWLDeclarationCreatedCount"]);
			AssertEquals("MatchingQueueAddedCount", 1, row["MatchingQueueAddedCount"]);
			AssertEquals("MatchingQueueWorkedCount", 1, row["MatchingQueueWorkedCount"]);
			AssertEquals("AirCargoInterventionQueuesHitCount", 1, row["AirCargoInterventionQueuesHitCount"]);
			AssertEquals("AirCargoInterventionQueuesWorkedCount", 1, row["AirCargoInterventionQueuesWorkedCount"]);
			AssertEquals("AirCargoEIRQueuesHitCount", 1, row["AirCargoEIRQueuesHitCount"]);
			AssertEquals("AirCargoEIRQueuesWorkedCount", 1, row["AirCargoEIRQueuesWorkedCount"]);
			AssertEquals("AirCargoHoldQueuesHitCount", 1, row["AirCargoHoldQueuesHitCount"]);
			AssertEquals("AirCargoHoldQueuesWorkedCount", 1, row["AirCargoHoldQueuesWorkedCount"]);
			AssertEquals("DeclarationEIRQueuesHitCount", 1, row["DeclarationEIRQueuesHitCount"]);
			AssertEquals("DeclarationEIRQueuesWorkedCount", 1, row["DeclarationEIRQueuesWorkedCount"]);
			AssertEquals("DeclarationHold_B5_RU_RJ_QueuesHitCount", 2, row["DeclarationHold_B5_RU_RJ_QueuesHitCount"]);
			AssertEquals("DeclarationHold_B5_RU_RJ_QueuesWorkedCount", 1, row["DeclarationHold_B5_RU_RJ_QueuesWorkedCount"]);
			AssertEquals("ImporterCreatedOrEditedCount", 1, row["ImporterCreatedOrEditedCount"]);
			AssertEquals("SupplierCreatedOrEditedCount", 1, row["SupplierCreatedOrEditedCount"]);
		}

		[TestDate(2006, 8, 11)]
		public void TestClientUPEBPWReport_UPECustomisationsDisabled()
		{
			CreateBusinessObjectsForClientUPEBPWReport();
			var sql = string.Format("SELECT * FROM ClientUPEBPWReport('{0}', '{0}')", ZDateTime.Today.SqlFormat);

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Factory.Save();

			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("AirCargoLettersCreatedCount", 0, row["AirCargoLettersCreatedCount"]);
			AssertEquals("AirCargoDocsCreatedCount", 0, row["AirCargoDocsCreatedCount"]);
			AssertEquals("AirCargoNonDocsCreatedCount", 0, row["AirCargoNonDocsCreatedCount"]);
			AssertEquals("FRMDeclarationCreatedCount", 0, row["FRMDeclarationCreatedCount"]);
			AssertEquals("SACDeclarationCreatedCount", 0, row["SACDeclarationCreatedCount"]);
			AssertEquals("SWLDeclarationCreatedCount", 0, row["SWLDeclarationCreatedCount"]);
			AssertEquals("MatchingQueueAddedCount", 0, row["MatchingQueueAddedCount"]);
			AssertEquals("MatchingQueueWorkedCount", 0, row["MatchingQueueWorkedCount"]);
			AssertEquals("AirCargoInterventionQueuesHitCount", 0, row["AirCargoInterventionQueuesHitCount"]);
			AssertEquals("AirCargoInterventionQueuesWorkedCount", 0, row["AirCargoInterventionQueuesWorkedCount"]);
			AssertEquals("AirCargoEIRQueuesHitCount", 0, row["AirCargoEIRQueuesHitCount"]);
			AssertEquals("AirCargoEIRQueuesWorkedCount", 0, row["AirCargoEIRQueuesWorkedCount"]);
			AssertEquals("AirCargoHoldQueuesHitCount", 0, row["AirCargoHoldQueuesHitCount"]);
			AssertEquals("AirCargoHoldQueuesWorkedCount", 0, row["AirCargoHoldQueuesWorkedCount"]);
			AssertEquals("DeclarationEIRQueuesHitCount", 0, row["DeclarationEIRQueuesHitCount"]);
			AssertEquals("DeclarationEIRQueuesWorkedCount", 0, row["DeclarationEIRQueuesWorkedCount"]);
			AssertEquals("DeclarationHold_B5_RU_RJ_QueuesHitCount", 0, row["DeclarationHold_B5_RU_RJ_QueuesHitCount"]);
			AssertEquals("DeclarationHold_B5_RU_RJ_QueuesWorkedCount", 0, row["DeclarationHold_B5_RU_RJ_QueuesWorkedCount"]);
			AssertEquals("ImporterCreatedOrEditedCount", 0, row["ImporterCreatedOrEditedCount"]);
			AssertEquals("SupplierCreatedOrEditedCount", 0, row["SupplierCreatedOrEditedCount"]);
		}

		#region CreateBusinessObjectsForClientUPEBPWReport

		void CreateBusinessObjectsForClientUPEBPWReport()
		{
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);

			var airCargoLetter = (UPECusHAWB)mawb.ChildBills.AddNew();
			airCargoLetter.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoLetter.CurrentQueue.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			FillWithValidDataToCreateDeclaration(airCargoLetter);
			airCargoLetter.CreateFormalDecAndMatchIfRequired();
			airCargoLetter.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoLetter.Declaration.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var patternMatchAddress = Factory.NewWithValidTestData<OrgPatternMatchAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			patternMatchAddress.P3_ParentID = airCargoLetter.PK;
			var importerMatchApproval = Factory.New<UPECusHAWBImporterMatchApproval>();
			importerMatchApproval.P2_ParentID = patternMatchAddress.PK;
			importerMatchApproval.P2_RelatedDateForPatternMatch = ZDateTime.Now;
			airCargoLetter.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.EIR;
			airCargoLetter.Declaration.CurrentQueue.P4_CustomsReason = AutoReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder;
			airCargoLetter.CurrentQueue.P4_GC = Env.CurrentCompanyPK;
			airCargoLetter.Declaration.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var airCargoDocs = (UPECusHAWB)mawb.ChildBills.AddNew();
			airCargoDocs.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			FillWithValidDataToCreateDeclaration(airCargoDocs);
			airCargoDocs.CreateFormalDecAndMatch();
			airCargoDocs.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			airCargoDocs.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			airCargoDocs.CurrentQueue.P4_CustomsReason = AutoReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			airCargoDocs.CurrentQueue.P4_CustomsReason = "_";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoDocs.CurrentQueue.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoDocs.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Hold;
			airCargoDocs.Declaration.CurrentQueue.P4_CustomsStatus = AutoReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
			airCargoDocs.CurrentQueue.P4_GC = Env.CurrentCompanyPK;
			airCargoDocs.Declaration.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var airCargoNonDocs = (UPECusHAWB)mawb.ChildBills.AddNew();
			airCargoNonDocs.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			FillWithValidDataToCreateDeclaration(airCargoNonDocs);
			airCargoNonDocs.CreateFormalDecAndMatch();
			airCargoNonDocs.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			airCargoNonDocs.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			airCargoNonDocs.CurrentQueue.P4_CustomsReason = AutoReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			airCargoNonDocs.CurrentQueue.P4_CustomsReason = "_";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoNonDocs.CurrentQueue.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoNonDocs.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Hold;
			airCargoNonDocs.Declaration.CurrentQueue.P4_CustomsStatus = AutoReasonCodeDescriptionPairList.Codes.RU_AlternateBroker;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoNonDocs.Declaration.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			airCargoNonDocs.CurrentQueue.P4_GC = Env.CurrentCompanyPK;
			airCargoNonDocs.Declaration.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave).PK;
			declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave).PK;
			declaration.JE_GB = Env.CurrentBranchPK;
			declaration.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var cusCode = declaration.Importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusCode.OK_CustomsRegNo = "987654321";
			cusCode = declaration.Supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusCode.OK_CustomsRegNo = "987654321";

			Factory.Save();
		}

		#endregion

		#endregion

		#region TestClientUPE_VW_CusHAWB_JobDec

		public void TestClientUPE_VW_CusHAWB_JobDec()
		{
			var sql = "SELECT * FROM ClientUPE_VW_CusHAWB_JobDec";
			var mAWB = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			var hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew();
			var jobDec = Factory.NewWithValidTestData<UPEJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("2 rows should be returned", 2, table.Rows.Count);
			AssertEquals("UPECusHAWB.PK", hAWB.PK, table.Rows[0]["PK"]);
			AssertEquals("Declaration.PK", jobDec.PK, table.Rows[1]["PK"]);

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("0 rows should be returned", 0, table.Rows.Count);
		}

		#endregion

		#region TestProductivityReport

		public void TestClientUPEProductivityReport()
		{
			ZDateTime today = ZDateTime.UtcNow.Date;
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_FullName = "Developer";
			CreateBusinessObjectsForClientUPEProductivityReport();
			var sql = string.Format("SELECT * FROM ClientUPEProductivityReport('{0}', '{0}', '', '') Order By Location", today.SqlFormat);

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("3 rows should be returned", 3, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("Location should be 'Cargo Report'", "Cargo Report", row["Location"]);
			AssertEquals("QueueWorked should be 'INV'", "INV", row["QueueWorked"]);
			AssertEquals("User should be 'Developer'", "Developer", row["User"]);
			AssertEquals("NumberOfShipmentsWorked should be 1", 1, row["NumberOfShipmentsWorked"]);

			row = table.Rows[1];
			AssertEquals("Location should be 'Declaration'", "Declaration", row["Location"]);
			AssertEquals("QueueWorked should be 'COM'", "COM", row["QueueWorked"]);
			AssertEquals("User should be 'Developer'", "Developer", row["User"]);
			AssertEquals("NumberOfShipmentsWorked should be 1", 1, row["NumberOfShipmentsWorked"]);

			row = table.Rows[2];
			AssertEquals("Location should be 'Finance'", "Finance", row["Location"]);
			AssertEquals("QueueWorked should be 'INV'", "INV", row["QueueWorked"]);
			AssertEquals("User should be 'Developer'", "Developer", row["User"]);
			AssertEquals("NumberOfShipmentsWorked should be 1", 1, row["NumberOfShipmentsWorked"]);
		}

		[TestDate(2006, 8, 28)]
		void CreateBusinessObjectsForClientUPEProductivityReport()
		{
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			var hawb = (UPECusHAWB)mawb.ChildBills.AddNew();
			FillWithValidDataToCreateDeclaration(hawb);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			declaration.MoveToQueue(AutoDeclarationQueueCodeDescriptionPairList.Codes.Compiling, ZString.Empty, ZString.Empty, ZString.Empty);
			declaration.JE_AgentsReference = "agent";
			Factory.Save();

			var finance = Factory.NewWithValidTestData<Callout>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			finance.CS_GoodsDescription = "description";
			finance.CurrentQueue.P4_QueueName = AutoCommercialQueueCodeDescriptionPairList.Codes.Finance;
			finance.CS_CM = mawb.PK;
			Factory.Save();
		}

		#endregion

		#region TestClientDetailedUPEEIRReport

		[TestDate(2006, 9, 5)]
		public void TestClientDetailedUPEEIRReport()
		{
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			var hawb = (UPECusHAWB)mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "1Z63611Y6640996404";
			hawb.CurrentQueue.P4_CustomDate4 = new ZDateTime(2006, 9, 6, 11, 23, 43);
			hawb.CurrentQueue.Logs.AddNew(Events.QueueChanged, "CUS\"EIR\",\"XN\",\"BT\",\"blah\"");
			hawb.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var jobDec = Factory.NewWithValidTestData<UPEJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDec.JE_HouseBill = "1Z1370A36694668212";
			jobDec.CurrentQueue.P4_CustomDate4 = new ZDateTime(2006, 9, 6, 11, 43, 28);
			jobDec.CurrentQueue.Logs.AddNew(Events.QueueChanged, "CUS\"EIR\",\"BP\",\"TF\",\"EIR Sent\"");
			jobDec.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var sql = string.Format("SELECT * FROM ClientDetailedUPEEIRReport('{0}', '{0}') Order By Date, Time, Source", new ZDateTime(2006, 9, 6).SqlFormat);
			Factory.Save();
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("2 rows should be returned", 2, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("Date should be '2006/09/06'", "2006/09/06", row["Date"]);
			AssertEquals("Time should be '11:00-12:00'", "11:00-12:00", row["Time"]);
			AssertEquals("Source should be 'Cargo Report'", "Cargo Report", row["Source"]);
			AssertEquals("Housebill should be '1Z63611Y6640996404'", "1Z63611Y6640996404", row["Housebill"]);
			AssertEquals("Reason should be 'XN'", "XN", row["Reason"]);
			AssertEquals("Status should be 'BT'", "BT", row["Status"]);
			AssertEquals("ColumnForCount should be '1'", 1, row["ColumnForCount"]);

			row = table.Rows[1];
			AssertEquals("Date should be '2006/09/06'", "2006/09/06", row["Date"]);
			AssertEquals("Time should be '11:00-12:00'", "11:00-12:00", row["Time"]);
			AssertEquals("Source should be 'Declaration'", "Declaration", row["Source"]);
			AssertEquals("Housebill should be '1Z1370A36694668212'", "1Z1370A36694668212", row["Housebill"]);
			AssertEquals("Reason should be 'BP'", "BP", row["Reason"]);
			AssertEquals("Status should be 'TF'", "TF", row["Status"]);
			AssertEquals("ColumnForCount should be '1'", 1, row["ColumnForCount"]);
		}

		#endregion

		#region TestClientUPEInterventionReport

		[TestDate(2006, 9, 6, 15, 40, 59)]
		public void TestClientUPEInterventionReport()
		{
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			mawb.CM_MAWB = "08166387075";
			var hawb = (UPECusHAWB)mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "M1831398910";
			hawb.CS_OtherSystemConsignorCode = "75X2E4";
			hawb.CS_RL_NKOrigin = "GBLON";
			hawb.CurrentQueue.P4_GC = Env.CurrentCompanyPK;

			var consignor = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True));
			var cusCode = consignor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			cusCode.OK_CustomsRegNo = "122037";

			hawb.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
			var sql = string.Format("SELECT * FROM ClientUPEInterventionReport('{0}', '{0}', '08166387075')", ZDateTime.Today.SqlFormat);

			Factory.Save();
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			DataRow row = table.Rows[0];
			AssertEquals("Masterbill should be '08166387075'", "08166387075", row["Masterbill"]);
			AssertEquals("User should be ''", System.DBNull.Value, row["User"]);
			AssertEquals("QueueMovedTo should be 'PND'", System.DBNull.Value, row["QueueMovedTo"]);
			AssertEquals("RemarksOnQueueMovedTo should be ''", System.DBNull.Value, row["RemarksOnQueueMovedTo"]);
			AssertEquals("Housebill should be 'M1831398910'", "M1831398910", row["Housebill"]);
			AssertEquals("CountryOfOrigin should be 'GB'", "GB", row["CountryOfOrigin"]);
			AssertEquals("ConsignorName should be '" + consignor.OH_FullName + "'", consignor.OH_FullName, row["ConsignorName"]);
			AssertEquals("ConsignorAccountNumber should be '122037'", "122037", row["ConsignorAccountNumber"]);

			consignor.CustomsCodes.RemoveAndDelete(cusCode);
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertEquals("ConsignorAccountNumber should be '75X2E4'", "75X2E4", row["ConsignorAccountNumber"]);

			hawb.CS_OA_ConsignorAddress = ZGuid.Empty;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertEquals("ConsignorAccountNumber should be '75X2E4'", "75X2E4", row["ConsignorAccountNumber"]);
		}

		#endregion

		#region TestClientUPEREVReport

		[TestDate(2006, 9, 8)]
		public void TestClientUPEREVReport()
		{
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			mawb.CM_MAWB = "08186465165";
			var hawb1 = (UPECusHAWB)mawb.ChildBills.AddNew();
			FillWithValidDataToCreateDeclaration(hawb1);
			hawb1.CreateFormalDecAndMatchIfRequired();
			hawb1.Declaration.MoveToQueue(AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification, ZString.Empty, ZString.Empty, ZString.Empty);
			hawb1.Declaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "REV";

			var hawb2 = (UPECusHAWB)mawb.ChildBills.AddNew();
			FillWithValidDataToCreateDeclaration(hawb2);
			hawb2.CreateFormalDecAndMatchIfRequired();
			hawb2.Declaration.MoveToQueue(AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification, ZString.Empty, ZString.Empty, ZString.Empty);
			hawb2.Declaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "";

			var sql = string.Format("SELECT * FROM ClientUPEREVReport('{0}', '{0}')", ZDateTime.Today.SqlFormat);
			Factory.Save();
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("CreatedDate should be today " + ZDateTime.Today.ToDateTime(), ZDateTime.Today.ToDateTime(), row["CreatedDate"]);
			AssertEquals("Masterbill should be '08186465165'", "08186465165", row["Masterbill"]);
			AssertEquals("TotalREV should be 1", 1, row["TotalREV"]);
			AssertEquals("TotalOther should be 1", 1, row["TotalOther"]);
		}

		#endregion

		#region TestClient_UPE_GetRegistryItem

		public void TestClient_UPE_GetRegistryItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, org.PK.ToGuid()))
			{
				var sql = string.Format("select * from Client_UPE_GetRegistryItem(null, 'CODManifestReportZoneRelatedPartyItem')");
				var table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("1 row should be returned", 1, table.Rows.Count);

				var row = table.Rows[0];
				AssertEquals("should be the same org", org.PK.ToGuid().ToString(), row["Value"]);
			}
		}

		#endregion

		#region TestClient_UPE_CheckPostcodeIsInZoneName
		public void TestClient_UPE_CheckPostcodeIsInZoneName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;

			var zone1 = provider.Zones.AddNew();
			zone1.TZ_ZoneName = "zone 1";
			var item1 = zone1.Items.AddNew();
			var postCode11 = Factory.New<RefPostCode>();
			postCode11.RK_CityTownPostCode = "1000";
			item1.TQ_FromPostCode = postCode11.RK_CityTownPostCode;
			var postCode12 = Factory.New<RefPostCode>();
			postCode12.RK_CityTownPostCode = "2000";
			item1.TQ_ToPostCode = postCode12.RK_CityTownPostCode;

			Factory.Save();

			var sql = string.Format("select dbo.Client_UPE_CheckPostcodeIsInZoneName('{0}', '{1}') as value", "1001", "zone 1");
			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("1 row should be returned", 1, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("sql postcode check", false, row["value"]);
			Factory.Save();

			using (UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, org.PK.ToGuid()))
			{
				sql = string.Format("select dbo.Client_UPE_CheckPostcodeIsInZoneName('{0}', '{1}') as value", "1001", "zone 1");
				table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("1 row should be returned", 1, table.Rows.Count);

				row = table.Rows[0];
				AssertEquals("sql postcode check", true, row["value"]);

				var zone2 = provider.Zones.AddNew();
				zone2.TZ_ZoneName = "zone 2";
				var item2 = zone2.Items.AddNew();
				var postCode21 = Factory.New<RefPostCode>();
				postCode21.RK_CityTownPostCode = "3000";
				item2.TQ_FromPostCode = postCode21.RK_CityTownPostCode;
				var postCode22 = Factory.New<RefPostCode>();
				postCode22.RK_CityTownPostCode = "4000";
				item2.TQ_ToPostCode = postCode22.RK_CityTownPostCode;
				Factory.Save();

				sql = string.Format("select dbo.Client_UPE_CheckPostcodeIsInZoneName({0}, '{1}') as value", "3333", "zone 1");
				table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("1 row should be returned", 1, table.Rows.Count);

				row = table.Rows[0];
				AssertEquals("sql postcode check", false, row["value"]);

				sql = string.Format("select dbo.Client_UPE_CheckPostcodeIsInZoneName({0}, '{1}') as value", "3333", UPETransportZonesCodeDescriptionPairProvider.AllMetro);
				table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("1 row should be returned", 1, table.Rows.Count);

				row = table.Rows[0];
				AssertEquals("sql postcode check", true, row["value"]);
			}
		}
		#endregion

		#region TestClient_UPE_CODManifestReport

		public void TestClient_UPE_CODManifestReport()
		{
			#region zones setup
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = relatedOrg.PK;

			var zone1 = provider.Zones.AddNew();
			zone1.TZ_ZoneName = "zone 1";
			var item1 = zone1.Items.AddNew();
			var postCode11 = Factory.New<RefPostCode>();
			postCode11.RK_CityTownPostCode = "1000";
			item1.TQ_FromPostCode = postCode11.RK_CityTownPostCode;
			var postCode12 = Factory.New<RefPostCode>();
			postCode12.RK_CityTownPostCode = "2000";
			item1.TQ_ToPostCode = postCode12.RK_CityTownPostCode;

			var zone2 = provider.Zones.AddNew();
			zone2.TZ_ZoneName = "zone 3";
			var item2 = zone2.Items.AddNew();
			var postCode21 = Factory.New<RefPostCode>();
			postCode21.RK_CityTownPostCode = "3000";
			item2.TQ_FromPostCode = postCode21.RK_CityTownPostCode;
			var postCode22 = Factory.New<RefPostCode>();
			postCode22.RK_CityTownPostCode = "4000";
			item2.TQ_ToPostCode = postCode22.RK_CityTownPostCode;
			#endregion

			#region Bill To setup
			string uANNumber = "XXX999";
			var billTo = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CustomsRegNo = uANNumber;
			cusCode.OK_OH = billTo.PK;

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "10";
			billTo.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			#endregion

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_FullName = "consignee 1";
			consignee1.Addresses.MainAddress.OA_PostCode = "1111";
			consignee1.Addresses.MainAddress.OA_Address1 = "address 1";

			var ship1 = Factory.NewWithValidTestData<CusHAWB>();
			ship1.CS_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			ship1.CS_HAWB = "ship1";

			var queue1 = Factory.NewWithValidTestData<ProcessQueue>();
			queue1.P4_ParentID = ship1.PK;
			var log1 = queue1.Logs.AddNew();
			queue1.P4_CustomDate4 = ZDateTime.Now; // release date
			queue1.P4_CustomDecimal4 = 11m; // invoice total
			queue1.P4_CustomAttrib1 = "Inv 1"; // invoice number
			queue1.P4_GC = Env.CurrentCompanyPK;

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_FullName = "consignee 2";
			consignee2.Addresses.MainAddress.OA_Address1 = "address 2";
			consignee2.Addresses.MainAddress.OA_PostCode = "2222";

			var ship2 = Factory.NewWithValidTestData<CusHAWB>();
			ship2.CS_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			ship2.CS_HAWB = "ship2";

			var queue2 = Factory.NewWithValidTestData<ProcessQueue>();
			queue2.P4_ParentID = ship2.PK;
			var log2 = queue2.Logs.AddNew();
			queue2.P4_CustomDate4 = ZDateTime.Now; // release date
			queue2.P4_CustomDecimal4 = 22m; // invoice total
			queue2.P4_CustomAttrib1 = "Inv 2"; // invoice number
			queue2.P4_GC = Env.CurrentCompanyPK;

			var ship3 = Factory.NewWithValidTestData<CusHAWB>();
			ship3.CS_HAWB = "ship3";
			ship3.CS_ConsigneeName = "consignee 3";
			ship3.CS_ConsigneePostcode = "3333";

			var queue3 = Factory.NewWithValidTestData<ProcessQueue>();
			queue3.P4_ParentID = ship3.PK;
			var log3 = queue3.Logs.AddNew();
			queue3.P4_CustomDate4 = ZDateTime.Now.AddDays(-1); // release date
			queue3.P4_CustomDecimal4 = 33m; // invoice total
			queue3.P4_CustomAttrib1 = "Inv 3"; // invoice number
			queue3.P4_GC = Env.CurrentCompanyPK;

			Factory.Save();

			var fromDate = ZDateTime.Now.AddDays(-2);
			var toDate = ZDateTime.Now.AddDays(2);
			var zoneName = UPETransportZonesCodeDescriptionPairProvider.AllMetro;
			var releaseMethod = "ALL";
			var sql = string.Format("select * from Client_UPE_CODManifestReport('{0}', '{1}', '{2}', '{3}') order by ConsigneeName", fromDate.SqlFormat, toDate.SqlFormat, zoneName, releaseMethod);

			var table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			using (log1.LockForUpdatingKeyFieldsForTesting())
			using (log2.LockForUpdatingKeyFieldsForTesting())
			using (log3.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_Reference = "TT\",\"" + ResolutionCodeDescriptionPairList.Codes.DA_Released + "\"TT";
				log2.SL_Reference = "TT\",\"" + ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms + "\"TT";
				log3.SL_Reference = "TT\",\"" + ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker + "\"TT";
			}

			Factory.Save();

			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			queue1.P4_CustomDecimal2 = (int)UPECargoPaymentMethod.Cheque;
			queue2.P4_CustomDecimal2 = (int)UPECargoPaymentMethod.Cheque;
			queue3.P4_CustomDecimal2 = (int)UPECargoPaymentMethod.Cheque;

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("No rows should be returned", 0, table.Rows.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			AssertEquals("3 rows should be returned", 3, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("ship1", "ship1", row["ShipmentNumber"]);
			AssertEquals("ship1", "Inv 1", row["InvoiceNumber"]);
			AssertEquals("ship1", "consignee 1", row["ConsigneeName"]);
			AssertEquals("ship1", 1111, row["PostCode"]);
			AssertEquals("ship1", 11m, row["InvoiceTotal"]);
			AssertEquals("ship1", "Manual", row["ReleasedMethod"]);

			using (UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, relatedOrg.PK.ToGuid()))
			using (UPEDataRegistry.Instance.CODAutoReleaseAndChaseThresholdItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20m))
			{
				queue1.P4_CustomAttrib2 = uANNumber;
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("3 rows should be returned", 3, table.Rows.Count);
				row = table.Rows[0];
				AssertEquals("ship1", "Automatic", row["ReleasedMethod"]);

				using (log1.LockForUpdatingKeyFieldsForTesting())
				using (log2.LockForUpdatingKeyFieldsForTesting())
				using (log3.LockForUpdatingKeyFieldsForTesting())
				{
					log1.SL_Reference = "TT\",\"" + ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned + "\"TT";
					log2.SL_Reference = "TT\",\"aaaa\"TT";
					log3.SL_Reference = "TT\",\"bbbb\"TT";
				}

				Factory.Save();
				table = Utilities.GetDataTableFromQuery(sql);
				AssertEquals("1 row should be returned", 1, table.Rows.Count);
			}
		}

		#endregion

		#region TestIsBranchUPECustomised

		public void TestIsBranchUPECustomised()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Factory.Save();
			var sql = string.Format("select Enabled from [dbo].[ClientIsBranchUPECustomised]('{0}')", Env.CurrentBranchPK);
			var table = Utilities.GetDataTableFromQuery(sql);
			var row = table.Rows[0];
			AssertEquals("1 row should be returned", 1, table.Rows.Count);
			AssertEquals("Should be false", false, row[0]);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(sql);
			row = table.Rows[0];
			AssertEquals("1 row should be returned", 1, table.Rows.Count);
			AssertEquals("should be true", true, row[0]);
		}

		#endregion

		#region Refund Report

		[TemplateName("UPE Refund Report")]
		public class RefudReportTestCase : ClientSpecificTemplateTestCase
		{
			protected override Clients ClientCode
			{
				get
				{
					return Clients.UPE;
				}
			}
		}

		public class UPERefudReportTest : ClientSpecificReportTestCase
		{
			public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase()
			{
				return new RefudReportTestCase();
			}

			public override ModuleIdentifier ModuleIdToTest
			{
				get
				{
					return ClientModuleRegistration.Reports;
				}
			}

			public override string MenuName
			{
				get
				{
					return "UPE Refund Report";
				}
			}

			public override string Hint
			{
				get
				{
					return ZString.Empty;
				}
			}
		}

		public class TestUPERefundReport : TestCaseWithFactory
		{
			public void TestRefudReport()
			{
				ZString message;

				#region Date TO/FROM

				message = "Date TO/FROM";

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						DateFrom = ZDateTime.Now
					},
					r =>
					{
						r.T10_DateProcessed = ZDateTime.Now;
					},
					r =>
					{
						r.T10_DateProcessed = ZDateTime.Now.AddDays(-1);
					});

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						DateFrom = ZDateTime.Now
					},
					null,
					r =>
					{
						r.T10_DateProcessed = ZDateTime.Now.AddDays(-1);
					});

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						DateTo = ZDateTime.Now.Date
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now.AddDays(1).Date;
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now.AddDays(2).Date;
					});

				#endregion

				#region OPENED Refund Enquiries

				message = "OPENED Refund Enquiries";

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						Opened = true
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now.Date;
					},
					r =>
					{
						r.T10_DateProcessed = ZDateTime.Now.Date;
						r.T10_DateProcessed = ZDateTime.Now.AddDays(5).Date;
					});

				#endregion

				#region CLOSED Refund Enquiries

				message = "CLOSED Refund Enquiries";
				AssertRefundEnquiryResult(message,
					new Filter()
					{
						Closed = true
					},
					r =>
					{
						r.T10_DateProcessed = ZDateTime.Now;
					},
					null);

				#endregion

				#region Write offs

				message = "Write offs";

				AssertRefundEnquiryResult(message,
							new Filter()
							{
								WriteOffs = true
							},
							r =>
							{
								r.T10_WriteOffAmount = 111.11m;
							},
							null);

				#endregion

				#region Refunded

				message = "Refunded";

				AssertRefundEnquiryResult(message,
							new Filter()
							{
								Refunded = true
							},
							r =>
							{
								r.T10_IsRefundRejected = false;
							},
							r =>
							{
								r.T10_IsRefundRejected = true;
							});

				#endregion

				#region Additional Charges

				message = "Additional Charges";

				AssertRefundEnquiryResult(message,
							new Filter()
							{
								HaveAdditionalCharges = true
							},
							r =>
							{
								r.T10_AdditionalCharges = 111.11m;
							},
							null);

				#endregion

				#region Refund to UPS

				message = "Refund to UPS";

				AssertRefundEnquiryResult(message,
							new Filter()
							{
								RefundedToUPS = true
							},
							r =>
							{
								r.T10_AmountRefundedToUPS = 111.11m;
							},
							null);

				#endregion

				#region Outstanding days

				message = "Outstanding days";

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						OutstadingDays = 5
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now;
						r.T10_DateProcessed = ZDateTime.Now.AddDays(5);
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now;
						r.T10_DateProcessed = ZDateTime.Now.AddDays(3);
					});

				AssertRefundEnquiryResult(message,
					new Filter()
					{
						OutstadingDays = 5
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now.AddDays(-5);
					},
					r =>
					{
						r.T10_DateCreated = ZDateTime.Now.AddDays(-3);
					});

				#endregion

				#region At Fault

				message = "At Fault";

				AssertRefundEnquiryResult(message,
							new Filter()
							{
								AtFaultUser = GlbStaff.CurrentUser.GS_Code
							},
							r =>
							{
								r.T10_GS_NKAtFaultUser = GlbStaff.CurrentUser.GS_Code;
							},
							null);

				#endregion

				#region User

				message = "User";

				AssertRefundEnquiryResult(message,
									new Filter()
									{
										User = "ZZZ"
									},
									r =>
									{
										r.T10_GS_NKCreatedUser = "ZZZ";
									},
									null);

				#endregion

				#region Reason

				message = "Reason";

				AssertRefundEnquiryResult(message,
									new Filter()
									{
										Reason = "Reason"
									},
									r =>
									{
										r.T10_RefundReason = "Reason";
									},
									null);

				#endregion
			}

			void AssertRefundEnquiryResult(ZString message, Filter filter, Action<ClientRefund> validAction, Action<ClientRefund> invalidAction)
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var declaration1 = Factory.NewWithValidTestData<UPEJobDeclaration>();
				var declaration2 = Factory.NewWithValidTestData<UPEJobDeclaration>();

				declaration1.JE_OH_Importer = org.PK;
				declaration2.JE_OH_Importer = org.PK;

				var validClientRefund = declaration1.RefundManager.CreateClientRefund();
				var invalidClientRefund = declaration2.RefundManager.CreateClientRefund();

				validClientRefund.T10_ControlNumber = "111";
				invalidClientRefund.T10_ControlNumber = "222";

				validAction?.Invoke(validClientRefund);
				invalidAction?.Invoke(invalidClientRefund);
				Factory.Save();
				UPEDataRegistry.Instance.EnableUPECustomisations = false;
				var table = GetDataTable(filter);
				AssertEquals("Should not produce any rows", 0, table.Rows.Count);

				UPEDataRegistry.Instance.EnableUPECustomisations = true;
				validAction?.Invoke(validClientRefund);
				invalidAction?.Invoke(invalidClientRefund);
				Factory.Save();
				table = GetDataTable(filter);
				AssertEquals(message + ": ClientRefunds in Database", 2, Factory.GetDatabaseCount(typeof(ClientRefund)));
				AssertEquals(message + ": table.Rows.Count", 1, table.Rows.Count);
				AssertRefundEnquiry(message, table.Rows[0], validClientRefund);

				TestCaseHelper.ClearTable(ClientRefund.Schema.TableName);
			}

			void AssertRefundEnquiry(ZString message, DataRow row, ClientRefund refund)
			{
				message += ": ";
				AssertEquals(message + "T10_ControlNumber", refund.T10_ControlNumber, row["T10_ControlNumber"]);
				AssertEquals(message + "T10_GS_NKCreatedUser", refund.T10_GS_NKCreatedUser, row["T10_GS_NKCreatedUser"]);
				AssertEquals(message + "T10_GS_NKAtFaultUser", refund.T10_GS_NKAtFaultUser, row["T10_GS_NKAtFaultUser"]);
				AssertEquals(message + "T10_RefundReason", refund.T10_RefundReason, row["T10_RefundReason"]);

				ZDateTime t10_DateCreated;
				if (!(row["T10_DateCreated"] is DBNull) && !refund.T10_DateCreated.IsEmpty)
				{
					ZDateTime.TryParseISO8601Date(row["T10_DateCreated"].ToString(), out t10_DateCreated);
					AssertEquals(message + "T10_DateCreated", refund.T10_DateCreated.Date, t10_DateCreated.Date);
				}
				ZDateTime t10_DateProcessed;
				if (!(row["T10_DateProcessed"] is DBNull) && !refund.T10_DateProcessed.IsEmpty)
				{
					ZDateTime.TryParseISO8601Date(row["T10_DateProcessed"].ToString(), out t10_DateProcessed);
					AssertEquals(message + "T10_DateProcessed", refund.T10_DateProcessed.Date, t10_DateProcessed.Date);
				}
				AssertEquals(message + "T10_EnquiryRaisedBy", refund.T10_EnquiryRaisedBy, row["T10_EnquiryRaisedBy"]);

				ZDecimal t10_RefundAmount;
				if (ZDecimal.TryParse(row["T10_RefundAmount"].ToString(), out t10_RefundAmount))
				{
					AssertEquals(message + "T10_RefundAmount", refund.T10_RefundAmount, t10_RefundAmount);
				}

				ZDecimal t10_AdditionalCharges;
				if (ZDecimal.TryParse(row["T10_AdditionalCharges"].ToString(), out t10_AdditionalCharges))
				{
					AssertEquals(message + "T10_AdditionalCharges", refund.T10_AdditionalCharges, t10_AdditionalCharges);
				}

				ZDecimal t10_AmountRefundedToUPS;
				if (ZDecimal.TryParse(row["T10_AmountRefundedToUPS"].ToString(), out t10_AmountRefundedToUPS))
				{
					AssertEquals(message + "T10_AmountRefundedToUPS", refund.T10_AmountRefundedToUPS, t10_AmountRefundedToUPS);
				}

				ZDecimal t10_WriteOffAmount;
				if (ZDecimal.TryParse(row["T10_WriteOffAmount"].ToString(), out t10_WriteOffAmount))
				{
					AssertEquals(message + "T10_WriteOffAmount", refund.T10_WriteOffAmount, t10_WriteOffAmount);
				}
			}

			#region Refund Report Implementation

			protected override void SetUp()
			{
				UPEDataRegistry.Instance.EnableUPECustomisations = true;

				base.SetUp();
			}

			DataTable GetDataTable(Filter filter)
			{
				string sql = BuildSql(filter);
				return Utilities.GetDataTableFromQuery(Db.Connection, sql);
			}

			string BuildSql(Filter filter)
			{
				StringBuilder builder = new StringBuilder();
				string yes = "'Y', ";
				string empty = "'', ";
				builder.Append(string.Format("'{0}', ", filter.DateFrom.IsEmpty ? ZDateTime.Now.ToShortDateString() : filter.DateFrom.ToShortDateString()));
				builder.Append(string.Format("'{0}', ", filter.DateTo.IsEmpty ? string.Empty : filter.DateTo.ToShortDateString()));
				builder.Append(filter.Opened ? yes : empty);
				builder.Append(filter.Closed ? yes : empty);
				builder.Append(filter.WriteOffs ? yes : empty);
				builder.Append(filter.Refunded ? yes : empty);
				builder.Append(filter.HaveAdditionalCharges ? yes : empty);
				builder.Append(filter.RefundedToUPS ? yes : empty);
				builder.Append(string.Format("{0}, ", filter.OutstadingDays));
				builder.Append(string.Format("'{0}', ", filter.AtFaultUser));
				builder.Append(string.Format("'{0}', ", filter.User));
				builder.Append(string.Format("'{0}'", filter.Reason));
				return string.Format("EXEC ClientRefundReport {0}", builder.ToString());
			}

			class Filter
			{
				public ZDateTime DateFrom
				{
					get;
					set;
				}
				public ZDateTime DateTo
				{
					get;
					set;
				}
				public ZBool Opened
				{
					get;
					set;
				}
				public ZBool Closed
				{
					get;
					set;
				}
				public ZBool WriteOffs
				{
					get;
					set;
				}
				public ZBool Refunded
				{
					get;
					set;
				}
				public ZBool HaveAdditionalCharges
				{
					get;
					set;
				}
				public ZBool RefundedToUPS
				{
					get;
					set;
				}
				public ZInt OutstadingDays
				{
					get;
					set;
				}
				public ZString AtFaultUser
				{
					get;
					set;
				}
				public ZString User
				{
					get;
					set;
				}
				public ZString Reason
				{
					get;
					set;
				}
				public ZString RaisedBy
				{
					get;
					set;
				}
			}

			#endregion
		}

		#endregion

		#region Implementation

		void FillWithValidDataToCreateDeclaration(UPECusHAWB airCargo)
		{
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			var orgForMatching = Factory.NewWithValidTestData<OrgHeader>();
			orgForMatching.OH_FullName = "AUSTRALIAN FILM & PIPE MANUFACTURERS";
			orgForMatching.OH_RL_NKClosestPort = "AUSYD";
			orgForMatching.MainAddress.OA_City = "SYDNEY";
			orgForMatching.MainAddress.OA_Phone = "+61297255045";
			orgForMatching.MainAddress.OA_PostCode = "2164";
			orgForMatching.MainAddress.OA_State = "NSW";
			orgForMatching.MainAddress.OA_Address1 = "150 WOODPARK RD";
			orgForMatching.MainAddress.OA_Address2 = "SMITHFIELD";
			orgForMatching.CreatePatternMatchingAddressFromMainAddress(Factory);
			orgForMatching.CreatePatternMatchingName(Factory);
			Factory.Save();

			airCargo.CS_GoodsValue = 500m;
			airCargo.CS_GoodsDescription = "test air cargo goods";

			airCargo.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsigneeName = orgForMatching.OH_FullName;
			airCargo.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;

			airCargo.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsignorName = orgForMatching.OH_FullName;
			airCargo.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
		}

		void AssertRefundPoints(string message, int expectedPoints, ZDateTime fromDate, ZDateTime toDate)
		{
			DbCommand command = Db.Connection.Command(string.Format("EXEC ClientADPScoringReport '{0}', '{1}'", fromDate.SqlFormat, toDate.SqlFormat));
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals(message, expectedPoints, reader["RefundPoints"]);
			}
		}

		#endregion

		#endregion

		#region Indexes

		public void TestIndex_ProcessQueue_P4_CustomAttrib2()
		{
			AssertContainsIndex(ProcessQueueSchema.Constants.TableName, UPECusHAWB.BillToAccountNumberProcessQueueColumn.Name);
		}

		public void TestIndex_ProcessQueue_P4_CustomsStatus()
		{
			AssertContainsIndex(ProcessQueueSchema.Constants.TableName, ProcessQueueSchema.Constants.P4_CustomsStatus);
		}

		public void TestIndex_ProcessQueue_P4_CustomDecimal4()
		{
			AssertContainsIndex(ProcessQueueSchema.Constants.TableName, Callout.TotalAmountDueColumn.Name);
		}

		public void TestIndex_ProcessQueue_P4_CustomDate4()
		{
			AssertContainsIndex(ProcessQueueSchema.Constants.TableName, UPEProcessQueue.CommercialReleasedDateColumn.Name);
		}

		void AssertContainsIndex(string tableName, params string[] indexColumns)
		{
			DbIndexReader indexReader = new DbIndexReader(tableName);
			AssertEquals("Expected to find the index", true, indexReader.IsIndexed(indexColumns));
		}

		#endregion

		#region Implementation

		object ExecuteScalar(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			return cmd.ExecuteScalar();
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		bool ExistsDbObject(string objectName)
		{
			DbCommand cmd = Db.Connection.Command("select name FROM sys.objects where name='" + objectName + "'");
			object result = cmd.ExecuteScalar();
			return (result != null);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}

		#endregion
	}
}


