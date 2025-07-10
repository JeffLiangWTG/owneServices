using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class DeclarationTestHelper : MasterFilesTestHelper
	{
		public DeclarationTestHelper()
		{
		}

		public DeclarationTestHelper(bool isImportData)
		{
			IsImportData = isImportData;
		}

		public DeclarationTestHelper(BusinessObjectFactory factory, bool isImportData)
			: base(factory)
		{
			IsImportData = isImportData;
		}

		public bool IsImportData;

		public override RefCurrency AUD
		{
			get
			{
				var result = base.AUD;
				result.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.490196m);
				return result;
			}
		}

		public EDIMessage GetEDIReleaseResponseMessage(string ccn, string interchangeNum)
		{
			return GetEDIReleaseResponseMessage(ccn, interchangeNum, ZDateTime.Now.AddDays(1), "", false);
		}

		public EDIMessage GetEDIReleaseResponseMessage(string ccn, string interchangeNum, string originalMessageNumber, bool saveRequired)
		{
			return GetEDIReleaseResponseMessage(ccn, interchangeNum, ZDateTime.Now.AddDays(1), originalMessageNumber, saveRequired);
		}

		public EDIMessage GetEDIReleaseResponseMessage(string ccn, string interchangeNum, ZDateTime systemCreateTime, string originalMessageNumber = "", bool saveRequired = false)
		{
			return GetEDIReleaseResponseMessage(ccn, new ZDateTime(2010, 11, 25, 08, 20, 0), interchangeNum, systemCreateTime, originalMessageNumber, saveRequired);
		}

		public EDIMessage GetEDIReleaseResponseMessage(string ccn, ZDateTime processingDate, string interchangeNum, ZDateTime systemCreateTime, string originalMessageNumber = "", bool saveRequired = false)
		{
			var messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:{0}:203'
GIS+4'
FTX+AAG+++DELIVERY INSTRUCTIONS LINE 1:LINE 2'
EQD+CN+CONTAINER1'
EQD+CN+CONTAINER2'
EQD+CN+CONTAINER3'
ERP+2:{1}'
RFF+XC:{2}'
UNT+11+1'".Replace("\r\n", "");

			originalMessageNumber = ((ZString)originalMessageNumber).IsEmpty ? "0001" : originalMessageNumber;

			return GetEDIReleaseResponseMessage(string.Format(messageText, processingDate.ToString("yyyyMMddHHmm"), originalMessageNumber, ccn), systemCreateTime, interchangeNum, saveRequired);
		}

		public EDIMessage GetIIDResponseMessage(ZDateTime processingDate, string interchangeNum, ZDateTime systemCreateTime, bool saveRequired = false)
		{
			var messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::911+10207000017327+11'
DTM+9:{0}:203'
GIS+9'
UNT+5+1'".Replace("\r\n", "");
			return GetEDIReleaseResponseMessage(string.Format(messageText, processingDate.ToString("yyyyMMddHHmm")), systemCreateTime, interchangeNum, saveRequired);
		}

		public EDIMessage GetEDIReleaseResponseMessage(string messageText, ZDateTime systemCreateTime, string interchangeNum = "1", bool saveRequired = false)
		{
			return GetEDIReleaseMessage(messageText, systemCreateTime, EDIInterchange.Direction.Receive, interchangeNum, saveRequired);
		}

		internal EDIMessage GetEDIReleaseMessage(string messageText, ZDateTime systemCreateTime, string direction = EDIInterchange.Direction.Transmit, string interchangeNum = "1", bool saveRequired = false)
		{
			var interchange = Factory.New<CAEDIInterchange>();
			interchange.EI_InterchangeNum = interchangeNum;
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_EI = interchange.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message.EM_MessageText = messageText.Replace("\r\n", "");
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = message.IsTransmitMessage ? EDIInterchange.Status.Sent : EDIInterchange.Status.Received;
			message.EM_SystemCreateTimeUtc = systemCreateTime;
			var tn = GetDataFromMessageBody(messageText, @"BGM\+:::([^\']+?)\+([^\']+?)\+11'", 2);
			if (tn.Length == TransactionNumber.Schema.FormattedTransactionNumberMaxLength)
			{
				message.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, tn);
			}
			message.SetSystemDefinedValue(EDIReleaseMessage.Schema.CargoControlNumber, GetDataFromMessageBody(messageText, @"RFF\+XC:([^\']*)\'", 1));
			message.EM_ApplicationReference = GetDataFromMessageBody(messageText, @"RFF\+XC:([^\']*)\'", 1);
			if (saveRequired)
			{
				Factory.Save();
			}
			return message;
		}

		ZString GetDataFromMessageBody(ZString messageText, ZString regexstring, int index)
		{
			var match = Regex.Match(messageText, regexstring);
			if (match.Success && match.Groups.Count >= index)
			{
				return match.Groups[index].Value;
			}
			return ZString.Empty;
		}

		internal RNSRequestMessage GetStatusQueryMessage(string ccn)
		{
			return GetStatusQueryMessage(ccn, ZDateTime.Now);
		}

		internal RNSRequestMessage GetStatusQueryMessage(string ccn, ZDateTime systemCreateTime)
		{
			return GetRNSRequest(ccn, "", RNSMessageTypes.Codes.StatusQuery, systemCreateTime);
		}

		internal RNSRequestMessage GetRNSRequest(string ccn, string transactionNo, string subType, ZDateTime systemCreateTime)
		{
			const string messageText = "UNH+{0}+CUSREP:D:96A:UN'BGM+998'DTM+132:201009200644:203'RFF+ABT:{1}'RFF+TN:{2}'UNT+6+145'";
			var request = Factory.New<RNSRequestMessage>();
			request.EM_MessageSubType = subType;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_Status = EDIMessage.Status.Sent;
			request.EM_SystemCreateTimeUtc = systemCreateTime;
			request.EM_MessageText = string.Format(messageText, EDIMessage.MessageNumberPlaceHolder, ccn, transactionNo);
			request.EM_ApplicationReference = ccn;
			request.EM_MessageOwner = transactionNo;
			return request;
		}

		internal Transport CreateTransportLeg(TransportCollection transports, ZString loadPort, ZString discPort, ZDateTime etd, ZDateTime eta)
		{
			var leg = transports.AddNew();
			leg.JW_RL_NKLoadPort = loadPort;
			leg.JW_RL_NKDiscPort = discPort;
			leg.JW_ETD = etd;
			leg.JW_ETA = eta;
			return leg;
		}

		public JobComInvoiceHeader CreateInvoice(JobDeclaration declaration, ZString invoiceNo, ZDecimal invoiceAmount, ZString invoiceCurrency)
		{
			var invoice = (declaration != null) ? declaration.Invoices.AddNew() : Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = invoiceNo;
			invoice.JZ_InvoiceAmount = invoiceAmount;
			invoice.JZ_RX_NKInvoice_Currency = invoiceCurrency;
			return invoice;
		}

		public JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, ZString tariff, ZDecimal quantity, ZDecimal linePrice, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			var line = (invoice != null) ? invoice.JobComInvoiceLines.AddNew() : Factory.New<JobComInvoiceLine>();
			line.JI_Tariff = tariff;
			line.JI_InvoiceQuantity = quantity;
			line.JI_CustomsQuantity = quantity;
			line.JI_LinePrice = linePrice;
			line.JI_Weight = weight;
			line.JI_WeightUQ = weightUQ;
			line.JI_Volume = volume;
			line.JI_VolumeUQ = volumeUQ;
			return line;
		}

		public void LinkInvoiceWithContainer(JobComInvoiceHeader invoice, CusContainer container)
		{
			foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
			{
				foreach (NonPersistentCusContainer lineContainer in line.ContainersForInvoiceLinesForBindingOnly)
				{
					var baseContainer = lineContainer.Container;
					if (baseContainer != null && baseContainer.PK == container.PK)
					{
						lineContainer.IsForInvoiceLine = true;
						break;
					}
				}
			}
		}

		public CusContainer CreateCusContainer(JobDeclaration dec, ZString containerNo, ZString sealNo, RefContainer container, ZString mode)
		{
			var cusContainer = (dec != null) ? dec.CusContainers.AddNew() : Factory.New<CusContainer>();
			cusContainer.CO_ContainerNumber = containerNo;
			cusContainer.CO_Seal = sealNo;
			cusContainer.CO_RC = container.PK;
			cusContainer.CO_FCL_LCL_AIR = mode;
			return cusContainer;
		}

		public override OrgHeader CreateConsignor()
		{
			var org = base.CreateConsignor();
			org.OH_RL_NKClosestPort = IsImportData ? AUSYD.Code : CAVAR.Code;
			org.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;
			org.MainAddress.OA_PostCode = "43422";
			return org;
		}

		public override OrgHeader CreateConsignee()
		{
			var org = base.CreateConsignee();
			org.OH_RL_NKClosestPort = IsImportData ? CAVAR.Code : AUSYD.Code;
			return org;
		}

		public override OrgHeader CreateExportForwarder()
		{
			var org = base.CreateExportForwarder();
			org.OH_RL_NKClosestPort = CATOR.Code;
			return org;
		}

		public override OrgHeader CreateImportForwarder()
		{
			var org = base.CreateImportForwarder();
			org.OH_RL_NKClosestPort = AUMEL.Code;
			return org;
		}

		public OrgHeader CreateOrganisation(ZString threeLetterCode, ZString fullName, ZString closestPort, ZString address1, ZString city, ZString phone)
		{
			var org = CreateOrganisation(fullName, closestPort, address1, city, phone);
			org.OH_Code = threeLetterCode + new Random().Next(1000000).ToString();
			return org;
		}

		public OrgHeader CreateOrganisation(ZString threeLetterCode, ZString fullName, ZString closestPort, ZString address1, ZString city, ZString state, ZString postCode, ZString phone)
		{
			var org = CreateOrganisation(threeLetterCode, fullName, closestPort, address1, city, phone);
			org.MainAddress.OA_PostCode = postCode;
			org.MainAddress.OA_State = state;
			return org;
		}

		public const string ValidBusinessNumber1 = "231231346RM0001";
		public const string ValidBusinessNumber2 = "231231346RM0002";
		public const string ValidAuthorizationID1 = "AD0011";
		public const string ValidAuthorizationID2 = "AD0012";

		#region Export Tariff
		#region ExportTariffHelper
		public CACExportTariffTestCase ExportTariffHelper
		{
			get { return exportTariffHelper ?? (exportTariffHelper = new CACExportTariffTestCase(Factory)); }
		}
		CACExportTariffTestCase exportTariffHelper;
		#endregion

		public TariffView ExportTariff64059000
		{
			get { return ExportTariffHelper.Tariff64059000; }
		}

		public TariffView ExportTariff84289020
		{
			get { return ExportTariffHelper.Tariff84289020; }
		}

		public TariffView ExportTariff85164000
		{
			get { return ExportTariffHelper.Tariff85164000; }
		}

		public TariffView ExportTariff87032330
		{
			get { return ExportTariffHelper.Tariff87032330; }
		}
		#endregion
	}
}
