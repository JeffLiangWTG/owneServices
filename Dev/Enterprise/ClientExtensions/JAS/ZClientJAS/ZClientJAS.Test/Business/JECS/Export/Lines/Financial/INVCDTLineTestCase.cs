using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class INVCDTLineTestCase : MessageLineTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", InvoiceWrapper, Line.InvoiceWrapper);
		}

		#region TestLineAsString
		public void TestLineAsString()
		{
			SetupInvoiceWrapperForTestLineAsString();
			ZString[] lineContent = GetExpectedLineContentForTestLineAsString();
			ZString expectedLineAsString = ExpectedLineType + JXCConstants.Version + JXCConstants.Delimiter + ZString.Join(JXCConstants.Delimiter.ToString(), lineContent);
			AssertEquals("LineAsString", expectedLineAsString, Line.LineAsString);
		}

		protected virtual void SetupInvoiceWrapperForTestLineAsString()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			Invoice.AH_TransactionNum = "TRAN101";
			((JASOrgHeader)currentBranch.OrgProxy).NettingCode = "AUCOR";
			Invoice.AH_InvoiceDate = new ZDateTime(2005, 12, 12);
			currentBranch.GB_RL_NKHomePort = "ESMAD";
			Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			Invoice.Job.JH_JobNum = "JOB101";
			Invoice.AH_RX_NKTransactionCurrency = "IDR";
			Invoice.AH_OSTotal = (Invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote) ? -198500 : 198500;
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			receivingForwarder.OH_Code = "RECVFWDORG";
			receivingForwarder.NettingCode = "ITMIL";
			receivingForwarder.OfficeCode = "ITVAL";
			Invoice.AH_OH = receivingForwarder.PK;
			SetShipperDetails("Shipper", "SH ADD1", "SH ADD2");
			currentBranch.Factory.Save();
		}

		protected virtual ZString[] GetExpectedLineContentForTestLineAsString()
		{
			ZString[] result = new ZString[ExpectedFieldCount];
			result[ExpectedFieldPositions.TransactionNumber] = "TRAN101";
			result[ExpectedFieldPositions.SendingNettingCode] = "AUCOR";
			result[ExpectedFieldPositions.TransactionDate] = "12/12/2005";
			result[ExpectedFieldPositions.ISOCountryCode] = "ES";
			result[ExpectedFieldPositions.VATCode] = "IVA";
			result[ExpectedFieldPositions.TransactionPerOperativo] = ExpectedLineType.Left(1);
			result[ExpectedFieldPositions.WeightUnit] = "K";
			result[ExpectedFieldPositions.Reference] = "JOB101";
			result[ExpectedFieldPositions.CurrencyCode] = "IDR";
			result[ExpectedFieldPositions.TotalTransaction] = "198500";
			result[ExpectedFieldPositions.DestinationNettingCode] = "ITMIL";
			result[ExpectedFieldPositions.DestinationOfficeCode] = "ITVAL";
			result[ExpectedFieldPositions.NumberOfPieces] = "0";
			result[ExpectedFieldPositions.GrossWeight] = "0";
			result[ExpectedFieldPositions.Volume] = "0";
			result[ExpectedFieldPositions.ChargeableWeight] = "0";
			result[ExpectedFieldPositions.ShipperName] = "Shipper";
			result[ExpectedFieldPositions.ShipperAddress1] = "SH ADD1";
			result[ExpectedFieldPositions.ShipperAddress2] = "SH ADD2";
			return result;
		}

		#endregion
		#region TestLineAsString_NonJobInvoice
		public void TestLineAsString_NonJobInvoice()
		{
			SetupInvoiceWrapperForTestLineAsString_NonJobInvoice();
			ZString[] lineContent = GetExpectedLineContentForTestLineAsString_NonJobInvoice();
			ZString expectedLineAsString = ExpectedLineType + JXCConstants.Version + JXCConstants.Delimiter + ZString.Join(JXCConstants.Delimiter.ToString(), lineContent);
			AssertEquals("LineAsString", expectedLineAsString, Line.LineAsString);
		}

		void SetupInvoiceWrapperForTestLineAsString_NonJobInvoice()
		{
			Invoice.AH_JH = ZGuid.Empty;
			Invoice.AH_TransactionNum = "TRAN101";
			Invoice.AH_InvoiceDate = new ZDateTime(2006, 1, 1);
		}

		protected virtual ZString[] GetExpectedLineContentForTestLineAsString_NonJobInvoice()
		{
			ZString[] result = new ZString[ExpectedFieldCount];
			result[ExpectedFieldPositions.TransactionNumber] = "TRAN101";
			result[ExpectedFieldPositions.TransactionDate] = "01/01/2006";
			result[ExpectedFieldPositions.CurrencyCode] = "AUD";
			result[ExpectedFieldPositions.ISOCountryCode] = "AU";
			result[ExpectedFieldPositions.VATCode] = "GST";
			result[ExpectedFieldPositions.TransactionPerOperativo] = ExpectedLineType.Left(1);
			result[ExpectedFieldPositions.WeightUnit] = "K";
			result[ExpectedFieldPositions.TotalTransaction] = "0";
			result[ExpectedFieldPositions.NumberOfPieces] = "0";
			result[ExpectedFieldPositions.GrossWeight] = "0";
			result[ExpectedFieldPositions.Volume] = "0";
			result[ExpectedFieldPositions.ChargeableWeight] = "0";
			result[ExpectedFieldPositions.ShipperName] = GlbBranch.CurrentBranch.OrgProxy.OH_FullName.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperNameMaxLength);
			result[ExpectedFieldPositions.ShipperAddress1] = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
			result[ExpectedFieldPositions.ShipperAddress2] = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
			return result;
		}

		#endregion
		#region TestLineAsString_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength
		public void TestLineAsString_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength()
		{
			SetupInvoiceWrapperForTestLineAsString_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength();
			ZString[] lineContent = GetExpectedLineContentAsStringArray_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength();
			ZString expectedLineAsString = ExpectedLineType + JXCConstants.Version + JXCConstants.Delimiter + ZString.Join(JXCConstants.Delimiter.ToString(), lineContent);
			AssertEquals("LineAsString", expectedLineAsString, Line.LineAsString);
		}

		void SetupInvoiceWrapperForTestLineAsString_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength()
		{
			Invoice.AH_TransactionNum = "TRAN101";
			Invoice.AH_InvoiceDate = new ZDateTime(2006, 10, 10);
			SetShipperDetails("1234567890123456789012345678901234567890", "23456789012345678901234567890123456789012345678901", "34567890123456789012345678901234567890123456789012");
		}

		protected virtual ZString[] GetExpectedLineContentAsStringArray_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength()
		{
			ZString[] result = new ZString[ExpectedFieldCount];
			result[ExpectedFieldPositions.TransactionNumber] = "TRAN101";
			result[ExpectedFieldPositions.TransactionDate] = "10/10/2006";
			result[ExpectedFieldPositions.ISOCountryCode] = "AU";
			result[ExpectedFieldPositions.VATCode] = "GST";
			result[ExpectedFieldPositions.TransactionPerOperativo] = ExpectedLineType.Left(1);
			result[ExpectedFieldPositions.WeightUnit] = "K";
			result[ExpectedFieldPositions.CurrencyCode] = "AUD";
			result[ExpectedFieldPositions.TotalTransaction] = "0";
			result[ExpectedFieldPositions.NumberOfPieces] = "0";
			result[ExpectedFieldPositions.GrossWeight] = "0";
			result[ExpectedFieldPositions.Volume] = "0";
			result[ExpectedFieldPositions.ChargeableWeight] = "0";
			result[ExpectedFieldPositions.ShipperName] = "12345678901234567890123456789012345";
			result[ExpectedFieldPositions.ShipperAddress1] = "23456789012345678901234567890123456";
			result[ExpectedFieldPositions.ShipperAddress2] = "34567890123456789012345678901234567";
			return result;
		}

		#endregion
		#region Implementation
		protected override sealed MessageLine GetMessageLine()
		{
			return GetINVCDTLine(InvoiceWrapper);
		}

		protected void SetShipperDetails(ZString shipperName, ZString address1, ZString address2)
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = shipperName;
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1 = address1;
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2 = address2;
		}

		protected new INVCDTLine Line
		{
			get
			{
				return (INVCDTLine)base.Line;
			}
		}

		protected InvoiceWrapper InvoiceWrapper
		{
			get
			{
				if (fInvoiceWrapper == null)
				{
					fInvoiceWrapper = GetNewInvoiceWrapper();
				}

				return fInvoiceWrapper;
			}
		}

		protected virtual InvoiceWrapper GetNewInvoiceWrapper()
		{
			return new InvoiceWrapper(JASInvoicingBase);
		}

		protected InvoicingBase Invoice
		{
			get
			{
				return JASInvoicingBase as InvoicingBase;
			}
		}

		protected IJASInvoicingBase JASInvoicingBase
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = GetNewJASInvoicingBase();
				}

				return fInvoice;
			}
		}

		protected abstract IJASInvoicingBase GetNewJASInvoicingBase();
		protected abstract INVCDTLine GetINVCDTLine(InvoiceWrapper invoiceWrapper);
		protected abstract JXCConstants.INVCDTFieldPositions ExpectedFieldPositions { get; }

		InvoiceWrapper fInvoiceWrapper;
		IJASInvoicingBase fInvoice;
		#endregion
	}
}
