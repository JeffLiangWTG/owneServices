using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSN : CargoImpBase, ICimParser, IFsn
	{
		public CIMFSN(List<string> list, EDIMessage inboundMessage) : base(new ErrorCollector())
		{
			linesOfCargoImp = list;
			linesOfCargoImpWithoutId = new List<string>();
			for (int i = 1; i < list.Count; i++)
			{
				linesOfCargoImpWithoutId.Add(list[i]);
			}
			this.inboundMessage = inboundMessage;
			this.factory = inboundMessage.Factory;
		}

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		public const string Code = "FSN";

		protected override string[] CargoImpLinesWithoutType
		{
			get { return linesOfCargoImpWithoutId.ToArray(); }
		}

		public override ZString MessageInterpretation
		{
			get
			{
				var htmlTable = new HtmlTableCreator();
				htmlTable.WriteRow("Customs Action Code", CAC);
				htmlTable.WriteRow("Customs Action Text", CAT);
				htmlTable.WriteRow("Customs Action Date", Date);
				htmlTable.WriteRow("Agent Reference", AgentReference);
				htmlTable.WriteRow("Pieces", Pieces);
				if (Awb != null)
				{
					if (!Awb.ChiefDeclarationUCR.IsEmpty)
					{
						htmlTable.WriteRow("Linked Entry", Awb.ChiefDeclarationUCR);
					}
					var hawb = Awb as CusHAWB;
					if (hawb != null && hawb.Shipment != null)
					{
						htmlTable.WriteRow("Linked Shipment", hawb.Shipment.JS_UniqueConsignRef);
					}
					else
					{
						var mawb = Awb as CusMAWB;
						if (mawb != null && mawb.Consol != null)
						{
							htmlTable.WriteRow("Linked Consol", mawb.Consol.JK_UniqueConsignRef);
						}
					}
				}

				return string.Format(
				  @" {0}
<h3>{1} - {2}</h3>
<p>Freight Status Notification</p> 
 {3}
",
					MessagePrettierCss.CSS,
					Awb == null ? new ZString(AwbSerialNumber) : Awb.ReferenceNumber,
					CAT,
					htmlTable.ToHtml()
					);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public bool DoAllProcessingBeforePrinting()
		{
			try
			{
				ParseInboundFsn();

				if (CAC.StartsWith("X")) // ... maybe it would be better to look at the sending PIMA of the interchange to see if it's "CUK...NES"? 
				{
					return UpdateForwardingConsolForExportFsn(); // FSN for export consol - will be no Cus*AWB object, instead update the ForwardingConsol
				}

				Awb = FindAwb(AwbSerialNumber, SplitNumber, CtoAirport + CtoShed, factory);
				if (Awb == null && CAC == CustomsStatusCodes.Codes.ThroughAirWaybillReleased)
				{
					Awb = CreateBlankAwbFromFsnCuForTAWBProcessing(AwbSerialNumber, SplitNumber, factory);
				}

				if (Awb != null)
				{
					var updatedOk = Awb.UpdateStatusToCacIfAllowed(CAC, ParseDateHeedingYear(Date), AgentReference, CAT);
					var recipientPima = inboundMessage.Interchange != null ? inboundMessage.Interchange.EI_To.Right(6) : ZString.Empty;
					var recipientText = recipientPima.StartsWith("000") ? (" - " + recipientPima.Right(3)) : "";
					var subject = string.Format("{4}CCSUK FSN - {0} {1} {2} {3}", Awb.ReferenceNumberWithShed, Awb.CustomsActionCode, Awb.LatestCustomsActionText, recipientText, updatedOk ? "" : "FAILED ");
					new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)Awb).SendEmail(subject, MessageInterpretation,
						NotificationItem, CAC, Awb.Branch.Company.PK.ToGuid(), Awb.Branch.PK.ToGuid(), Guid.Empty);
					return updatedOk;
				}
				else
				{
					var subject = $"Could not find CCSUK job {CtoAirport}{CtoShed}-{AwbSerialNumber} using FSN data";
					var body = $"{BrandingFactory.Instance.ProductName} received an FSN status update but could not find the job it references.  " +
						"No job was created because the CAC of the FSN was not CU. No update has been performed.  " +
						$"Consignment & split number={AwbSerialNumber}, message number={inboundMessage.EM_MessageNum}  " +
						"This message will be reprocessed a total of five times, five minutes apart, before being finally marked as error.";
					new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, null).SendEmail(subject, body,
						NotificationItem, CAC, Guid.Empty, Guid.Empty, Guid.Empty);
					return false;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new FormatException("Could not process inbound FSN, format was unexpected.", ex);
			}
		}

		bool UpdateForwardingConsolForExportFsn()
		{
			bool foundExportConsol = false;
			var query = new ZQuery(JobConsolSchema.JK_RL_NKLoadPort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(JobConsolSchema.JK_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddYears(-1));
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, AwbSerialNumber.KeepAlphanumericCharacters().Left(11));
			var consol = inboundMessage.Factory.LoadTop1<ForwardingConsol>(query);
			if (consol != null)
			{
				var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
				if (wrapper != null)
				{
					var helper = wrapper.MawbExportHelper;
					if (helper != null)
					{
						helper.ME_ChiefCustomsActionCodeFromFsn = CAC;
						helper.ME_ChiefCustomsActionTextFromFsn = CAT;
						helper.ME_ChiefCustomsActionDateFromFsn = ParseDateHeedingYear(Date);
						if (CAC == CustomsStatusCodes.Codes.MASTEROPEN)
						{
							helper.ME_ChiefConsolIsClosed = false;
						}
						helper.Messages.Add(inboundMessage);
						new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, null).SendEmail(string.Format("Export FSN update for {0} - {1} {2}", AwbSerialNumber, CAC, CAT), MessageInterpretation,
							NotificationItem, CAC, Guid.Empty, Guid.Empty, Guid.Empty);
						foundExportConsol = true;
					}
				}
			}
			return foundExportConsol;
		}

		IRegistryItem NotificationItem => GBCustomsDataRegistry.Instance.GetNotificationItem(CAC, "", GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactFsnStatusUpdates);

		ICcsukCusAwb CreateBlankAwbFromFsnCuForTAWBProcessing(ZString awbSerialNumber, ZString splitNumber, BusinessObjectFactory factory)
		{
			// TAWB gives immediate auto-FSN with CAC=CU, the FSN sometimes arrives before the FRI so we need to be able to process it without crying
			ICcsukCusAwb result = null;
			var parts = Regex.Split(awbSerialNumber, "-");
			var mawbNumber = NthElementOrEmpty(0, parts) + NthElementOrEmpty(1, parts);
			var hawbNumber = NthElementOrEmpty(2, parts);
			var mawbOrBasic = new CusMAWB.Loader(factory).FindFromMawbNumber(mawbNumber, CtoAirport + CtoShed);
			if (mawbOrBasic == null)
			{
				mawbOrBasic = factory.New<CusMAWB>();
				mawbOrBasic.CM_MAWB = mawbNumber;
				mawbOrBasic.Profile = inboundMessage.Interchange.EI_To.Replace("/", "");
				mawbOrBasic.CargoTerminalOperatorAirport = CtoAirport;
				mawbOrBasic.CargoTerminalOperator = CtoShed;
			}
			ICcsukCusAwb parentForSplit = mawbOrBasic;
			if (!String.IsNullOrEmpty(hawbNumber))
			{
				var hawb = new CusHAWB.Loader(factory).FindHawb(hawbNumber, "", mawbNumber);
				if (hawb == null)
				{
					hawb = mawbOrBasic.ChildBills.AddNew();
					hawb.CS_HAWB = hawbNumber;
					hawb.CargoTerminalOperatorAirport = CtoAirport;
					hawb.CargoTerminalOperator = CtoShed;
				}
				parentForSplit = hawb;
			}

			if (!splitNumber.IsEmpty)
			{
				var split = parentForSplit.Splits[splitNumber];
				if (split == null)
				{
					split = parentForSplit.Splits.AddNew();
					split.SplitReference = splitNumber;
					split.NumberOfPiecesExpected = ZShort.Parse(Pieces);
				}
				result = split;
			}
			else
			{
				parentForSplit.NumberOfPiecesExpected = ZShort.Parse(Pieces);
				result = parentForSplit;
			}
			return result;
		}

		static ZDateTime ParseDateHeedingYear(string dateAsStringddMMMHHmm)
		{
			ZDateTime dateTime;
			ZDateTime.TryParseExact(dateAsStringddMMMHHmm, out dateTime, "ddMMMHHmm");
			if (ZDateTime.Now.AddMinutes(10) < dateTime)
			{
				dateTime = dateTime.AddYears(-1);  // if today is 1st Jan and we see a message dated 31st Dec, TryParseExact guesses that it's 31st Dec this year.  Change it to be last year. 10 minutes' grace, same as CHIEF
			}
			return dateTime;
		}

		public void ParseInboundFsn()
		{
			AwbSerialNumber = NthElementOrEmpty(2, linesOfCargoImp.ToArray());
			ZString airportAndShed = NthElementOrEmpty(1, linesOfCargoImp.ToArray());
			CtoAirport = airportAndShed.SubstringSafe(0, 3);
			CtoShed = airportAndShed.SubstringSafe(3, 3);
			// Status line looks like	CSN/CA/2/12SEP1123/14729/ROUTE 3
			//or with a split,			CSN/CA-03/2/12SEP1123/14729/ROUTE 3'
			var entireCsnStatusLineElements = linesOfCargoImp.Count > 3 ? Regex.Split(linesOfCargoImp[3], "/") : Array.Empty<string>();
			var statusCodeAndSplitNumber = NthElementOrEmpty(1, entireCsnStatusLineElements);
			var statusCodeAndSplitElements = Regex.Split(statusCodeAndSplitNumber, "-");
			var statusCode = NthElementOrEmpty(0, statusCodeAndSplitElements);
			SplitNumber = NthElementOrEmpty(1, statusCodeAndSplitElements);
			Pieces = NthElementOrEmpty(2, entireCsnStatusLineElements);
			Date = NthElementOrEmpty(3, entireCsnStatusLineElements);
			AgentReference = NthElementOrEmpty(4, entireCsnStatusLineElements);
			CAT = NthElementOrEmpty(5, entireCsnStatusLineElements);
			CAC = statusCode;
		}

		public void DoPrinting()
		{
			new PrintFromFsnProvider(this, inboundMessage, ServiceLogger).DoPrinting();
		}

		public ZString SplitNumber
		{
			get; private set;
		}

		public ZString Pieces
		{
			get;
			private set;
		}

		public ZString Date
		{
			get;
			private set;
		}

		public ZString AgentReference
		{
			get;
			private set;
		}

		public ZString CAC
		{
			get;
			private set;
		}

		public ZString CAT
		{
			get;
			private set;
		}

		public ZString AwbSerialNumber
		{
			get;
			private set;
		}

		ZString CtoAirport { get; set; }
		ZString CtoShed { get; set; }

		public ICcsukCusAwb Awb { get; private set; }
		readonly List<string> linesOfCargoImp;
		readonly List<string> linesOfCargoImpWithoutId;
		readonly BusinessObjectFactory factory;
		readonly EDIMessage inboundMessage;
	}
}
