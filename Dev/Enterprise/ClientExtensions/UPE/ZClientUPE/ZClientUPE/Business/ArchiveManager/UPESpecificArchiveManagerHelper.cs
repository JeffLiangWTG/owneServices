#if DEBUG
using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class UPESpecificArchiveManagerHelper : IClientSpecificArchiveManagerHelper
	{
		public void CreateClientSpecificConfiguration()
		{
			string createClientBISIShipmentHeader = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientBISIShipmentHeader')
			BEGIN
			CREATE TABLE [dbo].[ClientBISIShipmentHeader]([T8_PK][uniqueidentifier] NOT NULL, [T8_CS] [uniqueidentifier] NOT NULL, [T8_UploadBatchNumber] [int] NOT NULL, [T8_ThirdPartyIndicator] [varchar] (1) NOT NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientBISIShipmentHeader);

			string createClientBISIShipmentCharge = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientBISIShipmentCharge')
			BEGIN
			CREATE TABLE [dbo].[ClientBISIShipmentCharge]([T9_PK] [uniqueidentifier] NOT NULL, [T9_T8] [uniqueidentifier] NOT NULL,[T9_ChargeType] [varchar](4) NOT NULL,[T9_GrossAmount] [money] NOT NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientBISIShipmentCharge);

			string createClientPrintBatch = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientPrintBatch')
			BEGIN
            CREATE TABLE [dbo].[ClientPrintBatch]([T7_PK] [uniqueidentifier] NOT NULL,[T7_BatchType] [varchar](3) NOT NULL,[T7_BatchNumber] [int] NOT NULL,[T7_LastPrintedDate] [smalldatetime] NULL,[T7_PrintCount] [int] NOT NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientPrintBatch);

			string createClientPrintBatchItem = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientPrintBatchItem')
			BEGIN
			CREATE TABLE [dbo].[ClientPrintBatchItem]([T6_PK] [uniqueidentifier] NOT NULL,[T6_T7] [uniqueidentifier] NOT NULL,[T6_ParentID] [uniqueidentifier] NOT NULL,[T6_GS_QueuedBy] [uniqueidentifier] NULL,[T6_SU] [uniqueidentifier] NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientPrintBatchItem);

			string createClientOrgRematch = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientOrgRematch')
			BEGIN
			CREATE TABLE[dbo].[ClientOrgRematch]([T5_PK][uniqueidentifier] NOT NULL,[T5_JE] [uniqueidentifier]NOT NULL,[T5_OrganisationType] [varchar] (3) NOT NULL,[T5_GS_RematchedBy] [uniqueidentifier] NULL,[T5_OH_RematchedFromOrg] [uniqueidentifier] NULL,[T5_OH_RematchedToOrg] [uniqueidentifier] NULL,[T5_RematchedFromDate] [smalldatetime] NULL,[T5_RematchedToDate] [smalldatetime] NULL)
            END";
			Db.Connection.ExecuteNonQuery(createClientOrgRematch);

			string createClientRefund = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientRefund')
			BEGIN
			CREATE TABLE [dbo].[ClientRefund]([T10_PK] [uniqueidentifier] NOT NULL,[T10_GS_NKCreatedUser] [varchar](3) NULL,[T10_ControlNumber] [varchar](11) NOT NULL,[T10_DateCreated] [smalldatetime] NULL,[T10_DateProcessed] [smalldatetime] NULL,[T10_JE] [uniqueidentifier] NULL,[T10_IsRefundRejected] [char](1) NULL,[T10_RefundRejectedDetails] [varchar](1000) NULL,[T10_RefundProcessingFee] [money] NULL,[T10_WriteOffAmount] [money] NULL,[T10_RefundAmount] [money] NULL,[T10_AdditionalCharges] [money] NULL,[T10_AmountRefundedToUPS] [money] NULL,[T10_GS_NKAtFaultUser] [varchar](3) NULL,[T10_RefundReason] [varchar](50) NULL,	[T10_CS] [uniqueidentifier] NULL,[T10_EnquiryRaisedBy] [varchar](20) NULL,[T10_EnquiryPhoneNumber] [varchar](100) NULL,[T10_EnquiryDetails] [varchar](1000) NULL,[T10_EnquiryContact] [varchar](256) NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientRefund);

			string createClientXPLDUploadLog = @"IF NOT EXISTS (SELECT null FROM sys.tables WHERE name = 'ClientXPLDUploadLog')
			BEGIN
			CREATE TABLE[dbo].[ClientXPLDUploadLog]([U3_PK][uniqueidentifier] NOT NULL,[U3_ReasonCode] [varchar] (100) NULL,[U3_DateCreated] [datetime] NULL,[U3_BISIData] [varchar] (1000) NULL,[U3_TrackingNumber] [varchar] (100) NOT NULL)
			END";
			Db.Connection.ExecuteNonQuery(createClientXPLDUploadLog);
		}
		public void CreateClientSpecificData(ZGuid shipmentPK)
		{
			var declaration = CreateDeclarationData(shipmentPK);
			var declarationPK = declaration.PK;
			var cusHAWBPk = CreateCusHAWB(declarationPK);
			CreateClientBISIShipmentData(cusHAWBPk);
			CreateClientPrintBatchItemData(declarationPK, cusHAWBPk);
			CreateClientOrgRematchData(declarationPK);
			CreateClientRefundData(declarationPK, cusHAWBPk);
			CreateClientXPLDUploadLogData(declaration);
		}
		public void DropClientSpecificConfiguration()
		{
			string dropclientBISIShipmentCharge = @"Drop Table ClientBISIShipmentCharge";
			Db.Connection.ExecuteNonQuery(dropclientBISIShipmentCharge);

			string dropclientBISIShipmentHeader = @"Drop Table ClientBISIShipmentHeader";
			Db.Connection.ExecuteNonQuery(dropclientBISIShipmentHeader);

			string dropClientPrintBatchItem = @"Drop Table ClientPrintBatchItem";
			Db.Connection.ExecuteNonQuery(dropClientPrintBatchItem);

			string dropClientPrintBatch = @"Drop Table ClientPrintBatch";
			Db.Connection.ExecuteNonQuery(dropClientPrintBatch);

			string dropClientOrgRematch = @"Drop Table ClientOrgRematch";
			Db.Connection.ExecuteNonQuery(dropClientOrgRematch);

			string dropClientRefund = @"Drop Table ClientRefund";
			Db.Connection.ExecuteNonQuery(dropClientRefund);

			string dropClientXPLDUploadLog = @"Drop Table ClientXPLDUploadLog";
			Db.Connection.ExecuteNonQuery(dropClientXPLDUploadLog);
		}
		public void AssertNumberOfRecords(string status, Action<bool> assert)
		{
			var dbSnapshot = DbSnapshot.TakeSnapshot(status, Db.Connection);
			int expectedNumber;
			expectedNumber = 0;
			switch (status)
			{
				case "Beginning":
					expectedNumber = 0;
					break;
				case "TestRecords":
					expectedNumber = 3;
					break;
				case "AfterPurge":
					expectedNumber = 0;
					break;
			}

			assert(expectedNumber == dbSnapshot.TableRecordCount["ClientBISIShipmentHeader"].RowCount);
			assert(expectedNumber == dbSnapshot.TableRecordCount["ClientBISIShipmentCharge"].RowCount);
			assert(expectedNumber * 2 == dbSnapshot.TableRecordCount["ClientPrintBatchItem"].RowCount);
			assert(expectedNumber == dbSnapshot.TableRecordCount["ClientOrgRematch"].RowCount);
			assert(expectedNumber * 2 == dbSnapshot.TableRecordCount["ClientRefund"].RowCount);
			assert(expectedNumber == dbSnapshot.TableRecordCount["ClientXPLDUploadLog"].RowCount);
		}
		BaseJobDeclaration CreateDeclarationData(ZGuid shipmentPK)
		{
			var factory = new BusinessObjectFactory();
			var jobDeclaration = factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_GB = Env.CurrentBranch.PK;
			jobDeclaration.JE_JS = shipmentPK;
			jobDeclaration.JE_HouseBill = "housebill1";
			factory.Save();
			return jobDeclaration;
		}
		ZGuid CreateCusHAWB(ZGuid declarationPK)
		{
			var factory = new BusinessObjectFactory();
			var cusHawb = factory.New<CusHAWB>();
			cusHawb.CS_JE_CustomsFormalEntry = declarationPK;
			factory.Save();
			return cusHawb.PK;
		}
		void CreateClientBISIShipmentData(ZGuid cusHAWBPk)
		{
			var factory = new BusinessObjectFactory();
			var clientBISIShipmentHeader = factory.New<ClientBISIShipmentHeader>();
			clientBISIShipmentHeader.T8_CS = cusHAWBPk;
			clientBISIShipmentHeader.T8_UploadBatchNumber = 1;
			clientBISIShipmentHeader.T8_ThirdPartyIndicator = "1";
			factory.Save();
			var clientBISIShipmentCharge = factory.New<ClientBISIShipmentCharge>();
			clientBISIShipmentCharge.T9_T8 = clientBISIShipmentHeader.PK;
			clientBISIShipmentCharge.T9_ChargeType = "1";
			clientBISIShipmentCharge.T9_GrossAmount = 1;
			factory.Save();
		}
		void CreateClientPrintBatchItemData(ZGuid declarationPK, ZGuid cusHAWBPk)
		{
			var factory = new BusinessObjectFactory();
			var clientPrintBatch = factory.New<UPEPrintBatch>();
			clientPrintBatch.T7_BatchType = "ULA";
			clientPrintBatch.T7_BatchNumber = 1;
			clientPrintBatch.T7_LastPrintedDate = new ZDateTime(2009, 06, 15);
			clientPrintBatch.T7_PrintCount = 1;

			factory.Save();

			var clientPrintBatchItemcusHAWB = factory.New<UPEPrintBatchItem>();
			clientPrintBatchItemcusHAWB.T6_T7 = clientPrintBatch.PK;
			clientPrintBatchItemcusHAWB.T6_ParentID = cusHAWBPk;

			var clientPrintBatchItemDec = factory.New<UPEPrintBatchItem>();
			clientPrintBatchItemDec.T6_T7 = clientPrintBatch.PK;
			clientPrintBatchItemDec.T6_ParentID = declarationPK;

			factory.Save();
		}
		void CreateClientOrgRematchData(ZGuid declarationPK)
		{
			var factory = new BusinessObjectFactory();

			var clientOrgRematch = factory.New<UPEOrgRematch>();
			clientOrgRematch.T5_JE = declarationPK;
			clientOrgRematch.T5_OrganisationType = "123";
			factory.Save();
		}
		void CreateClientRefundData(ZGuid declarationPK, ZGuid cusHAWBPk)
		{
			var factory = new BusinessObjectFactory();

			var clienRefundcusHAWB = factory.New<ClientRefund>();
			clienRefundcusHAWB.T10_ControlNumber = "1";
			clienRefundcusHAWB.T10_CS = cusHAWBPk;

			var clientRefundDec = factory.New<ClientRefund>();
			clientRefundDec.T10_ControlNumber = "1";
			clientRefundDec.T10_JE = declarationPK;

			factory.Save();
		}
		void CreateClientXPLDUploadLogData(BaseJobDeclaration declaration)
		{
			var factory = new BusinessObjectFactory();
			var clientXPLDUploadLog = factory.New<ClientXPLDUploadLog>();
			clientXPLDUploadLog.U3_TrackingNumber = declaration.JE_HouseBill;
			factory.Save();
		}
	}
}
#endif
