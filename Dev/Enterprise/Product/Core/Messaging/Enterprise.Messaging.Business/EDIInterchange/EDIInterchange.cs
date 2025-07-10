using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Edifact.Manual;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Messaging.Business
{
	[CodeProperty(Schema.EI_InterchangeNum), DescriptionProperty(Schema.EI_InterchangeNum)]
	[ShouldDisplayInUtcTimeForEditAndCreateLogFields]
	public class EDIInterchange : AutoEDIInterchange, IEDIInterchange, IResetToQueuedStatusSupporter, IDocManagerSupport
	{
		#region Constants

		//****************************************************
		//Further codes should be added to ApplicationCodeList
		//****************************************************
		public abstract class ApplicationCodes
		{
			#region AU

			public const string CMR = ApplicationCodeList.Codes.AUCMR;
			public const string AirCargo = ApplicationCodeList.Codes.AUAirCargo;
			public const string SeaCargo = ApplicationCodeList.Codes.AUSeaCargo;
			public const string EXDOC = ApplicationCodeList.Codes.AUExDoc;
			public const string NEXDOCS = ApplicationCodeList.Codes.AUCustomsNEXDOC;
			public const string COLS = ApplicationCodeList.Codes.AUCOLS;

			#endregion

			#region BE
			public const string BECustomsAesSystem = ApplicationCodeList.Codes.BECustomsAesSystem;
			#endregion

			#region CA
			public const string CACustoms = ApplicationCodeList.Codes.CACustoms;
			public const string CAACI = ApplicationCodeList.Codes.CAACI;
			public const string CAEXP = ApplicationCodeList.Codes.CAEXP;
			public const string CAIMP = ApplicationCodeList.Codes.CAIMP;
			public const string CACustomsDIF = ApplicationCodeList.Codes.CACustomsDIF;
			#endregion

			#region NO
			public const string NOCustomsEmma = ApplicationCodeList.Codes.NOCustomsEmma;
			#endregion

			#region NZ

			public const string NewZealandCustoms = ApplicationCodeList.Codes.NZCustoms;
			public const string NewZealandMAFeBACCa = ApplicationCodeList.Codes.NZMAFeBACCa;

			#endregion

			public const string AMS = ApplicationCodeList.Codes.USAMS;
			public const string AirCargoAdvanceScreening = ApplicationCodeList.Codes.AirCargoAdvanceScreening;
			public const string USExportManifest = ApplicationCodeList.Codes.USExportManifest;
			public const string CIM = ApplicationCodeList.Codes.CIM;
			public const string SingaporeCMD = ApplicationCodeList.Codes.SGCustomsCMD;
			public const string SingaporeTradenet4 = ApplicationCodeList.Codes.SGCustomsTradenet4;
			public const string SGCustomsTradenetXML = ApplicationCodeList.Codes.SGCustomsTradenetXML;
			public const string SingaporeNationalTradePlatform = ApplicationCodeList.Codes.SGNationalTradePlatform;
			public const string ERouter = ApplicationCodeList.Codes.ERouter;
			public const string ForwarderEdifact = ApplicationCodeList.Codes.ForwarderEdifact;
			public const string MalaysiaK4K5 = ApplicationCodeList.Codes.MYK4K5;
			public const string OneStop = ApplicationCodeList.Codes.OneStop;
			public const string OFX = ApplicationCodeList.Codes.OFX;
			public const string Traxon = ApplicationCodeList.Codes.HKTraxon;
			public const string UnitedArabEmirates = ApplicationCodeList.Codes.UAECustoms;
			public const string USCustomsExport = ApplicationCodeList.Codes.USCustomsExport;
			public const string USCustomsImport = ApplicationCodeList.Codes.USCustomsImport;
			public const string USeBond = ApplicationCodeList.Codes.USeBond;
			public const string USeManifest = ApplicationCodeList.Codes.USeManifest;
			public const string SouthAfricanCustoms = ApplicationCodeList.Codes.ZACustoms;
			public const string SouthAfricanTransactionOrders = ApplicationCodeList.Codes.ZATransactionOrders;
			public const string WarehouseDocket = ApplicationCodeList.Codes.WarehouseDocket;
			public const string PortAuthority = ApplicationCodeList.Codes.PortAuthority;
			public const string EIDO = ApplicationCodeList.Codes.EIDO;
			public const string ContainerManagement = ApplicationCodeList.Codes.ContainerManagement;
			public const string Inttra = ApplicationCodeList.Codes.Inttra;
			public const string ShippingLineEHubMessaging = ApplicationCodeList.Codes.ShippingLineEHubMessaging;
			public const string GbEdifactShared = ApplicationCodeList.Codes.GbEdifactShared;
			public const string GbMcpPortHealth = ApplicationCodeList.Codes.GbMcpPortHealth;
			public const string GbMcpR01AndR11 = ApplicationCodeList.Codes.GbMcpRra01AndRra11;
			public const string GbMcpR12 = ApplicationCodeList.Codes.GbMcpRra12;
			public const string GbMcpEdifactOutboundOnly = ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly;
			public const string GbCnsEdifactOutboundOnly = ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly;
			public const string GbCcsuk = ApplicationCodeList.Codes.GbCcsuk;
			public const string GbNesAllMessageTypes = ApplicationCodeList.Codes.GbNesAllMessageTypes;
			public const string GbCnsCompass = ApplicationCodeList.Codes.GbCnsCompass;
			public const string GbPentant = ApplicationCodeList.Codes.Pentant;
			public const string GbCDSViaCCSUK = ApplicationCodeList.Codes.GbCDSViaCCSUK;
			public const string GbCustomsDeclarationServices = ApplicationCodeList.Codes.GbCustomsDeclarationServices;
			public const string GbCDSDISQuery = ApplicationCodeList.Codes.GbCDSDISQuery;
			public const string GbCustomsGVMSManifest = ApplicationCodeList.Codes.GbCustomsGVMSManifest;
			public const string GbMessageICSGreatBritain = ApplicationCodeList.Codes.GbMessageICSGreatBritain;
			public const string GbMessageICSNorthernIreland = ApplicationCodeList.Codes.GbMessageICSNorthernIreland;
			public const string GbCommonTransitConvention = ApplicationCodeList.Codes.GbCommonTransitConvention;
			public const string GbCustomsEMCS = ApplicationCodeList.Codes.GbCustomsEMCS;
			public const string GbCustomsNCTS = ApplicationCodeList.Codes.GbCustomsNCTS;
			public const string EuNcts = ApplicationCodeList.Codes.EuNcts;
			public const string eNett = ApplicationCodeList.Codes.eNett;
			public const string XMS = ApplicationCodeList.Codes.XMS;
			public const string Unknown = ApplicationCodeList.Codes.Unknown;
			public const string eHub = ApplicationCodeList.Codes.eHub;
			public const string UniversalDataMessaging = ApplicationCodeList.Codes.UniversalDataMessaging;
			public const string NativeDataMessaging = ApplicationCodeList.Codes.NativeDataMessaging;
			public const string StowPlan = ApplicationCodeList.Codes.StowPlan;
			public const string USCustomsDIS = ApplicationCodeList.Codes.USCustomsDIS;
			public const string AsycudaManifest = ApplicationCodeList.Codes.AsycudaManifest;
			public const string INConsolManifest = ApplicationCodeList.Codes.INConsolManifest;
			public const string GlobalElectronicInvoice = ApplicationCodeList.Codes.GlobalElectronicInvoice;
			public const string GenericMessageDelivery = ApplicationCodeList.Codes.GenericMessageDelivery;
			public const string DECustomsAtlasSystem = ApplicationCodeList.Codes.DECustomsAtlasSystem;
			public const string DECustomsAesSystem = ApplicationCodeList.Codes.DECustomsAesSystem;
			public const string DECustomsEmcsSystem = ApplicationCodeList.Codes.DECustomsEmcsSystem;
			public const string FRCustomsMessage = ApplicationCodeList.Codes.FRCustomsMessage;
			public const string FRPortMessage = ApplicationCodeList.Codes.FRPortMessage;
			public const string TaiwanCustoms = ApplicationCodeList.Codes.TWCustoms;
			public const string AUCustomsNEXDOC = ApplicationCodeList.Codes.AUCustomsNEXDOC;
			public const string USAMA = ApplicationCodeList.Codes.USAMA;
			public const string ITCustoms = ApplicationCodeList.Codes.ITCustoms;
			public const string ITCustomsXTrade = ApplicationCodeList.Codes.ITCustomsXTrade;
			public const string TRCustoms = ApplicationCodeList.Codes.TRCustoms;
			public const string JPCustoms = ApplicationCodeList.Codes.JPCustoms;
			public const string UYCustoms = ApplicationCodeList.Codes.UYCustoms;
			public const string KRCustoms = ApplicationCodeList.Codes.KRCustoms;
			public const string ESCustomsMessage = ApplicationCodeList.Codes.ESCustomsMessage;
			public const string MXCustoms = ApplicationCodeList.Codes.MXCustoms;
			public const string CLCustoms = ApplicationCodeList.Codes.CLCustoms;
			public const string BRCustoms = ApplicationCodeList.Codes.BRCustoms;
			public const string NLCustoms = ApplicationCodeList.Codes.NLCustoms;
			public const string ESCustoms = ApplicationCodeList.Codes.ESCustomsMessage;
			public const string CHCustomsEdec = ApplicationCodeList.Codes.CHCustomsEdec;
			public const string CHCustomsPassar = ApplicationCodeList.Codes.CHCustomsPassar;
			public const string CHCustomsCharteraOutput = ApplicationCodeList.Codes.CHCustomsCharteraOutput;
			public const string PLCustoms = ApplicationCodeList.Codes.PLCustoms;
			public const string PLCustomsPUESCEmailSystem = ApplicationCodeList.Codes.PLCustomsPUESCEmailSystem;
			public const string PLCustomsNCTS = ApplicationCodeList.Codes.PLCustomsNCTS;
			public const string PLCustomsExitControl = ApplicationCodeList.Codes.PLCustomsExitControl;
			public const string UsageData = ApplicationCodeList.Codes.UsageData;
			public const string ARCustoms = ApplicationCodeList.Codes.ARCustoms;
			public const string CNCustomsSingleWindow = ApplicationCodeList.Codes.CNCustomsSingleWindow;
			public const string IECustomsAndExcise = ApplicationCodeList.Codes.IECustomsAndExcise;
			public const string IECustomsCommon = ApplicationCodeList.Codes.IECustomsCommon;
			public const string IECustomsEMCS = ApplicationCodeList.Codes.IECustomsEMCS;
			public const string IECustomsExport = ApplicationCodeList.Codes.IECustomsExport;
			public const string IECustomsImport = ApplicationCodeList.Codes.IECustomsImport;
			public const string IECustomsUCC5Import = ApplicationCodeList.Codes.IECustomsUCC5Import;
			public const string IECustomsNCTS = ApplicationCodeList.Codes.IECustomsNCTS;
			public const string IECustomsPBN = ApplicationCodeList.Codes.IECustomsPBN;
			public const string BECustoms = ApplicationCodeList.Codes.BECustoms;
			public const string IC2 = ApplicationCodeList.Codes.EUICS2;
			public const string ElectronicBillOfLadingMessaging = ApplicationCodeList.Codes.ElectronicBillOfLadingMessaging;
			public const string EUH7 = ApplicationCodeList.Codes.EuH7;
			public const string ShipamaxIntegration = ApplicationCodeList.Codes.ShipamaxIntegration;
			public const string DashDocumentDataProcessing = ApplicationCodeList.Codes.DashDocumentDataProcessing;
			public const string ILCustoms = ApplicationCodeList.Codes.ILCustoms;
			public const string ILManifest = ApplicationCodeList.Codes.ILManifest;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule")]
			internal static ZString GetCodeFromInterchangeSender(ZString senderID)
			{
				string result = ZString.Empty;

				switch (senderID.Trim())
				{
					case InterchangePartyIDs.CMRMailbox:
						result = CMR;
						break;
					case InterchangePartyIDs.NZCustomsTestMailbox:
					case InterchangePartyIDs.NZCustomsLiveMailbox:
						result = NewZealandCustoms;
						break;
					case InterchangePartyIDs.NZMAFeBACCaTestMailbox:
					case InterchangePartyIDs.NZMAFeBACCaLiveMailbox:
						result = NewZealandMAFeBACCa;
						break;
					case InterchangePartyIDs.ERouterMailbox:
						result = ERouter;
						break;
					case InterchangePartyIDs.CIMMailbox:
						result = CIM;
						break;
					case InterchangePartyIDs.CSXMailboxCode:
					case InterchangePartyIDs.OneStopMailboxCode:
						result = OneStop;
						break;
					case InterchangePartyIDs.ZACCustomsMailbox:
						result = SouthAfricanCustoms;
						break;
					case InterchangePartyIDs.MYCTestCustomsMailbox:
					case InterchangePartyIDs.MYCProdCustomsMailbox:
						result = MalaysiaK4K5;
						break;
					case InterchangePartyIDs.TradeNetLiveSystem:
					case InterchangePartyIDs.TradeNetTestSystem:
						result = SingaporeTradenet4;
						break;
					case InterchangePartyIDs.EXDOCSendersMailbox:
						result = EXDOC;
						break;
					case InterchangePartyIDs.TradeNetV4LiveSystem:
					case InterchangePartyIDs.TradeNetV4LiveSystem1:
					case InterchangePartyIDs.TradeNetV4LiveSystem2:
					case InterchangePartyIDs.TradeNetV4LiveSystem3:

					case InterchangePartyIDs.TradeNetV4TestSystem:
						result = SingaporeTradenet4;
						break;
					case InterchangePartyIDs.Inttra:
					case InterchangePartyIDs.InttraV2:
						result = Inttra;
						break;
					case InterchangePartyIDs.ShippingLineViaEHub:
						result = ShippingLineEHubMessaging;
						break;
					case InterchangePartyIDs.KRCustomsMailbox:
						result = KRCustoms;
						break;
				}
				return result;
			}

			internal static bool GetTestModeFromInterchangeSender(ZString senderID)
			{
				return (senderID == InterchangePartyIDs.NZCustomsTestMailbox || senderID == InterchangePartyIDs.NZMAFeBACCaTestMailbox);
			}

			internal static ZString GetCodeFromIfcsumAndIftstaInterchangeSender(BusinessObjectFactory factory, ZString senderID, ZString applicationReference)
			{
				string result = ZString.Empty;

				if (applicationReference == AppReference.Ifcsum || applicationReference == AppReference.Iftsta)
				{
					if (IsSenderAForwarder(factory, senderID))
					{
						result = ForwarderEdifact;
					}
				}

				return result;
			}

			static bool IsSenderAForwarder(BusinessObjectFactory factory, ZString senderCode)
			{
				ZQuery forwarderFilter = new ZQuery(OrgHeaderSchema.OH_Code, senderCode);
				forwarderFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
				return factory.ExistsInDatabase(OrgHeaderSchema.Constants.TableName, forwarderFilter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public abstract class InterchangePartyIDs
		{
			public const string CMRMailbox = "AAA336C";
			public const string NZMAFeBACCaTestMailbox = "NZ MAF ECN Test";
			public const string NZMAFeBACCaLiveMailbox = "NZ MAF ECN Live";
			public const string NZCustomsTestMailbox = "CUSSWT";
			public const string NZCustomsLiveMailbox = "CUSMOD";
			public const string ERouterMailbox = "01010000050001  E  D  I  C  T";
			public const string CIMMailbox = "???";
			public const string OneStopMailboxCode = "1STOP";
			public const string CSXMailboxCode = "CSXWTADL";
			public const string OneStopBeureauCode = "EDIAL";
			public const string ZACCustomsMailbox = "SARS";
			public const string MYCTestCustomsMailbox = "EDITST2006033";
			public const string MYCProdCustomsMailbox = "9556448049886";
			public const string AMSMailbox = "AMS";
			public const string KRCustomsMailbox = "KRCustoms";
			public const string EXDOCSendersMailbox = "8";
			public const string EXDOCReceiversMailbox = "7";
			public const string TradeNetTestSystem = "DCST.DCST201";
			public const string TradeNetLiveSystem = "DCS2.DCS2001";

			public const string TradeNetV4TestSystem = "DCST.DCST401";

			public const string TradeNetV4LiveSystem = "DCS4.DCS4001";
			public const string TradeNetV4LiveSystem1 = "DCSP.DCSP222";
			public const string TradeNetV4LiveSystem2 = "AEB1.AEB1001";
			public const string TradeNetV4LiveSystem3 = "RPI1.RPI1001";

			public const string Inttra = "INTTRA";
			public const string InttraV2 = "INTTRANG2";
			public const string ShippingLineViaEHub = "SHIPPINGLINEVIAEHUB";
		}

		public abstract class Direction
		{
			public const string Receive = ReceiveTransmitList.Codes.Receive;
			public const string Transmit = ReceiveTransmitList.Codes.Transmit;
		}

		public abstract class Status : EDIInterchangeStatusList.Codes
		{
		}

		public abstract class TransportType : EDIInterchangeTransportTypeList.Codes
		{
		}

		public static class AppReference
		{
			public const string Ifcsum = "IFCSUM";
			public const string Iftsta = "IFTSTA";
		}

		public new abstract class Schema : AutoEDIInterchange.Schema
		{
			public const string EI_InterchangeText = "EI_InterchangeText";
			public const string EI_InterchangeDateTime = "EI_InterchangeDateTime";
			public const string EI_ShowAllxTEventsLog = "EI_ShowAllxTEventsLog";
			public const string EI_XTInternalMsgStatus = "EI_XTInternalMsgStatus";
			public const string EI_ShowRelatedxTEventsLog = "EI_ShowRelatedxTEventsLog";
		}

		public const string UNOAUNAString = "UNA:+.? '";

		#endregion

		public static EDIInterchange New(BusinessObjectFactory factory)
		{
			return factory?.New<EDIInterchange>();
		}

		public EDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly ImmutableArray<SchemaColumn> AllBlobFields = ImmutableArray.Create<SchemaColumn>(
			EDIInterchangeSchema.EI_HeaderText, EDIInterchangeSchema.EI_HeaderNText,
			EDIInterchangeSchema.EI_BodyText, EDIInterchangeSchema.EI_BodyNText,
			EDIInterchangeSchema.EI_FooterText, EDIInterchangeSchema.EI_FooterNText);

		protected override ZString HumanReadableNameCore => Res.GetString("24e86d64-9f84-495f-9aa8-12462b797c42", "Interchange {0}", EI_InterchangeNum);

		[ReadOnly(true)]
		public override ZString EI_InterchangeNum
		{
			get { return base.EI_InterchangeNum; }
			set { base.EI_InterchangeNum = value; }
		}

		public override ZString EI_Status
		{
			get { return base.EI_Status; }
			set
			{
				var oldValue = EI_Status;
				base.EI_Status = value;
				if (!IsCopying && oldValue != EI_Status)
				{
					SetSendViaEHubIfNeededOrReset();
				}
			}
		}

		readonly List<string> _messageProcessWarnings = new List<string>();

		public IEnumerable<string> MessageProcessWarnings => _messageProcessWarnings;

		public override ZString EI_ReceiveTransmit
		{
			get => base.EI_ReceiveTransmit;
			set
			{
				var oldValue = EI_ReceiveTransmit;
				base.EI_ReceiveTransmit = value;
				if (!IsCopying && oldValue != EI_ReceiveTransmit)
				{
					SetSendViaEHubIfNeededOrReset();
				}
			}
		}

		public bool ShouldShowInterchangeEventsTab => EI_TransportType == EDIInterchange.TransportType.xT && EI_XTInternalMsgID > 0 && Env.CurrentUser.IsSupportUser;

		public ZBool EI_ShowAllxTEventsLog
		{
			get => showAllxTEventsLog;
			set
			{
				var oldValue = showAllxTEventsLog;
				if (value != oldValue)
				{
					SetNonPersistentPropertyValue(EI_ShowAllxTEventsLogInfo, ref showAllxTEventsLog, value);
					ReloadXtMessageEvents(false);
				}
			}
		}
		ZBool showAllxTEventsLog;

		public ZPropertyInfo EI_ShowAllxTEventsLogInfo => GetZPropertyInfo(Schema.EI_ShowAllxTEventsLog);

		public ZBool EI_ShowRelatedxTEventsLog
		{
			get => showRelatedxTEventsLog;
			set
			{
				var oldValue = showRelatedxTEventsLog;
				if (value != oldValue)
				{
					SetNonPersistentPropertyValue(EI_ShowRelatedxTEventsLogInfo, ref showRelatedxTEventsLog, value);
					ReloadXtMessageEvents(false);
				}
			}
		}
		ZBool showRelatedxTEventsLog;

		public ZPropertyInfo EI_ShowRelatedxTEventsLogInfo => GetZPropertyInfo(Schema.EI_ShowRelatedxTEventsLog);

		public ZString EI_XTInternalMsgStatus => XtMessageEventsInfo.MsgState;

		public ZPropertyInfo EI_XTInternalMsgStatusInfo => GetZPropertyInfo(Schema.EI_XTInternalMsgStatus);

		public void ReloadXtMessageEvents(bool shouldRefresh)
		{
			if (shouldRefresh)
			{
				Factory.ClearCachedValue<IReadOnlyList<IXtMessageEventData>>(GetEventsCatchKey());
			}

			XtMessageEventsInfo.ReLoad(shouldRefresh);
			EI_XTInternalMsgStatusInfo.RefreshBinding();
			XtMessageEvents.RefreshBinding();
		}

		public XtMessageEventsCollection XtMessageEvents => Factory.GetCachedValue(GetEventsCatchKey(), () => XtMessageEventsInfo.XtMessageEvents);

		public XtMessageEventsInfo XtMessageEventsInfo => _xtMessageEventsInfo ?? (_xtMessageEventsInfo = new XtMessageEventsInfo(this));
		XtMessageEventsInfo _xtMessageEventsInfo;

		string GetEventsCatchKey() => $"XtMessageEvents_{EI_XTInternalMsgID}_{EI_ShowRelatedxTEventsLog}_{EI_ShowAllxTEventsLog}";

		void SetSendViaEHubIfNeededOrReset()
		{
			if (!eHubQueueStatusCalculationSuspended)
			{
				if (EI_ReceiveTransmit == EDIInterchange.Direction.Transmit)
				{
					if (EI_Status == EDIInterchange.Status.Queued && ShouldSendViaEHub)
					{
						base.EI_Status = EDIInterchange.Status.eHubQueued;
						SetTransportTypeAndSessionGuid();
					}
					else if (EI_Status == EDIInterchange.Status.eHubQueued)
					{
						SetTransportTypeAndSessionGuid();
					}
				}
				else if (EI_ReceiveTransmit == EDIInterchange.Direction.Receive)
				{
					if (EI_Status == EDIInterchange.Status.eHubQueued) //should only ever be eHubQueued if transmitting.
					{
						base.EI_Status = EDIInterchange.Status.Queued;
						// TODO: add your own TransportType here.
						EI_SessionGUID = ZGuid.Empty;
					}
				}
			}
		}

		void SetTransportTypeAndSessionGuid()
		{
			if (EI_TransportType != EDIInterchangeTransportTypeList.Codes.eHub)
			{
				EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			}

			EI_SessionGUID = (EI_SessionGUID == null || EI_SessionGUID.IsEmpty || !EI_SessionGUID.IsValid)
				? Guid.NewGuid()
				: EI_SessionGUID;
		}

		public bool ShouldSendViaEHub => ShouldSendViaEHubCore;

		protected virtual bool ShouldSendViaEHubCore => false;

		byte eHubQueueStatusCalculationSuspenderIndex;
		bool eHubQueueStatusCalculationSuspended => eHubQueueStatusCalculationSuspenderIndex > 0;

		internal IDisposable SuspendeHubQueueStatusCalculation()
		{
			return new eHubQueueStatusCalculationSuspender(this);
		}

		class eHubQueueStatusCalculationSuspender : IDisposable
		{
			public eHubQueueStatusCalculationSuspender(EDIInterchange interchange)
			{
				this.interchange = interchange;
				interchange.eHubQueueStatusCalculationSuspenderIndex++;
			}

			readonly EDIInterchange interchange;

			public void Dispose()
			{
				interchange.eHubQueueStatusCalculationSuspenderIndex--;
				if (!interchange.eHubQueueStatusCalculationSuspended)
				{
					interchange.SetSendViaEHubIfNeededOrReset();
				}
			}
		}

		#region BusinessObjectCollections
		public EDIInterchangeEDIMessageCollection ContainedMessages
		{
			get
			{
				if (fContainedMessages == null)
				{
					fContainedMessages = GetNewContainedMessagesCollection();
					fContainedMessages.Load();
					fContainedMessages.IsManagedForDataRefresh = true;
				}
				return fContainedMessages;
			}
		}

		protected virtual EDIInterchangeEDIMessageCollection GetNewContainedMessagesCollection()
		{
			return new EDIInterchangeEDIMessageCollection(this, Factory);
		}

		public StmALogCollection LogCollection
		{
			get
			{
				if (fLogCollection == null)
				{
					fLogCollection = new StmALogCollection(Factory, new ZQuery(StmALogSchema.SL_Parent, PK));
					fLogCollection.Load();
				}
				return fLogCollection;
			}
		}

		public EDIMessageCollection InterchangeAcknowledgementMessages
		{
			get
			{
				if (fInterchangeAcknowledgementMessages == null)
				{
					fInterchangeAcknowledgementMessages = new EDIMessageCollection(this, Factory);
					fInterchangeAcknowledgementMessages.Load();
					fInterchangeAcknowledgementMessages.IsManagedForDataRefresh = true;
				}
				return fInterchangeAcknowledgementMessages;
			}
		}

		protected BusinessObjectCollection fBranchCollection;
		public BusinessObjectCollection BranchCollection
		{
			get
			{
				if (fBranchCollection == null)
				{
					fBranchCollection = new GlbBranchCollection(Factory);
				}
				return fBranchCollection;
			}
		}
		#endregion

		public static readonly TypeDecider TypeDecider = new EDIInterchangeTypeDecider();

		#region Initialisation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_GB = GlbBranch.CurrentBranch.PK;
			EI_IsActive = true;
		}

		public static IEnumerable<string> SplitMultipleInterchanges(ZString interchangeString)
		{
			UNCharacterSet characterSet = GetCharacterSetFromInterchangeString(interchangeString);
			ZString[] segments = interchangeString.SplitIgnoringEscapedDelimiter(characterSet.SegmentDelimiterChar, characterSet.EscapeCharacterChar, true);
			StringBuilder builder = new StringBuilder();
			foreach (ZString segment in segments)
			{
				builder.Append(segment);
				if (segment.StartsWith("UNZ"))
				{
					yield return builder.ToString();
					builder = new StringBuilder();
				}
			}
		}

		#region Create New Interchange From String

		public static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, bool shouldReportWholeErrorMessage = false)
		{
			return CreateNewInterchangeFromString(factory, interchangeString, "", false, false, shouldReportWholeErrorMessage);
		}

		public static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode)
		{
			return CreateNewInterchangeFromString(factory, interchangeString, applicationCode, false, false);
		}

		public static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, bool shouldReportWholeErrorMessage)
		{
			return CreateNewInterchangeFromString(factory, interchangeString, applicationCode, false, false, shouldReportWholeErrorMessage);
		}

		public static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, UNCharacterSet characterSet)
		{
			return CreateNewInterchangeFromString(factory, interchangeString, applicationCode, false, false, characterSet);
		}

		public static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, bool ignoreFunctionalGroup, bool onlyCreateInterchange, bool shouldReportWholeErrorMessage = false, bool shouldPutUNGsInBody = false)
		{
			var characterSet = GetCharacterSetFromInterchangeString(interchangeString, shouldReportWholeErrorMessage);
			return CreateNewInterchangeFromString(factory, interchangeString, applicationCode, ignoreFunctionalGroup, onlyCreateInterchange, characterSet, shouldReportWholeErrorMessage, shouldPutUNGsInBody);
		}

		static EDIInterchange CreateNewInterchangeFromString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, bool ignoreFunctionalGroup, bool onlyCreateInterchange, UNCharacterSet characterSet, bool shouldReportWholeErrorMessage = false, bool shouldPutUNGsInBody = false)
		{
			var details = GetInterchangeDetailsFromString(factory, interchangeString, applicationCode, ignoreFunctionalGroup, onlyCreateInterchange, characterSet, shouldReportWholeErrorMessage, shouldPutUNGsInBody);
			var result = (EDIInterchange)factory.New(new EDIInterchangeTypeDecider().GetTypeForApplicationCode(details.ApplicationCode, null, factory));
			var shouldDelete = true;
			try
			{
				result.InitialiseValuesForIncomingInterchange(details, onlyCreateInterchange);
				shouldDelete = false;
			}
			finally
			{
				if (shouldDelete)
				{
					result.Delete(); // effectively a memory transaction
				}
			}
			return result;
		}

		#endregion

		#region Interchange Details from String

		public static InterchangeDetails GetInterchangeDetailsFromString(
			BusinessObjectFactory factory,
			ZString interchangeString,
			ZString applicationCode,
			bool ignoreFunctionalGroup,
			bool onlyPopulateInterchange,
			UNCharacterSet characterSet,
			bool shouldReportWholeErrorMessage = false,
			bool shouldPutUNGsInBody = false)
		{
			InterchangeDetails details;
			try
			{
				details = InterchangeStringExtensions.GetInterchangeDetailsFromString(
					interchangeString,
					ignoreFunctionalGroup,
					onlyPopulateInterchange,
					characterSet,
					shouldPutUNGsInBody);
			}
			catch (InvalidOperationException ex)
			{
				throw new MessageProcessingException(ex.Message, interchangeString, true, false, shouldReportWholeErrorMessage);
			}
			details.ApplicationCode = applicationCode.IsEmpty
				? GetApplicationCode(factory, details.From, details.ApplicationReference)
				: applicationCode;
			return details;
		}

		public void PopulateInterchangeFromString(ZString interchangeString, ZString applicationCode, bool ignoreFunctionalGroup, bool onlyPopulateInterchange)
		{
			var characterSet = GetCharacterSetFromInterchangeString(interchangeString);
			var details = GetInterchangeDetailsFromString(Factory, interchangeString, applicationCode, ignoreFunctionalGroup, onlyPopulateInterchange, characterSet);
			InitialiseValuesForIncomingInterchange(details, onlyPopulateInterchange);
		}

		#endregion

		protected static ZString GetApplicationCode(BusinessObjectFactory factory, ZString senderID, ZString applicationReference)
		{
			var result = ApplicationCodes.GetCodeFromInterchangeSender(senderID);
			if (result.IsEmpty)
			{
				result = ApplicationCodes.GetCodeFromIfcsumAndIftstaInterchangeSender(factory, senderID, applicationReference);
			}

			return result;
		}

		void InitialiseValuesForIncomingInterchange(InterchangeDetails details, bool onlyCreateInterchange)
		{
			if (onlyCreateInterchange)
			{
				InitialiseValuesForIncomingInterchangeOnly(details.HeaderText, details.BodyText, details.FooterText, details.From, details.To, details.InterchangeNum, details.ApplicationCode);
			}
			else
			{
				InitialiseValuesForIncomingInterchangeAndCreateMessages(details.HeaderText, details.BodyText, details.FooterText, details.From, details.To, details.InterchangeNum, details.TestFlag, details.ApplicationCode);
			}
		}

		void InitialiseValuesForIncomingInterchangeAndCreateMessages(string eI_HeaderText, string eI_BodyText, string eI_FooterText, string eI_From, string eI_To, string eI_InterchangeNum, bool testFlag, string eI_ApplicationCode)
		{
			InitialiseValuesForIncomingInterchangeOnly(eI_HeaderText, eI_BodyText, eI_FooterText, eI_From, eI_To, eI_InterchangeNum, eI_ApplicationCode);

			EI_InterchangeType = eI_ApplicationCode;
			SpawnMessagesFromInterchangeText(testFlag);
			EI_Status = Status.Received;
		}

		protected virtual void InitialiseValuesForIncomingInterchangeOnly(string eI_HeaderText, string eI_BodyText, string eI_FooterText, string eI_From, string eI_To, string eI_InterchangeNum, string eI_ApplicationCode)
		{
			this.EI_ApplicationCode = eI_ApplicationCode;
			this.EI_HeaderText = eI_HeaderText;
			this.EI_BodyText = eI_BodyText;
			this.EI_FooterText = eI_FooterText;
			this.EI_From = eI_From;
			this.EI_To = eI_To;
			this.EI_InterchangeNum = eI_InterchangeNum;
			this.EI_Status = Status.Queued;
			this.EI_ReceiveTransmit = Direction.Receive;

			EI_Priority = EDIInterchangePriorityList.Codes.High;
		}

		public virtual UNCharacterSet CharacterSet => unCharacterSet ?? (unCharacterSet = GetCharacterSetFromHeader());

		UNCharacterSet GetCharacterSetFromHeader()
		{
			//Different implementation than in InterchangeStringExtensions
			UNCharacterSet charSet;
			string header = EI_HeaderText;
			if (header.IndexOf("UNOA", StringComparison.Ordinal) != -1)
			{
				charSet = new UNOACharacterSet();
			}
			else if (header.IndexOf("UNOB", StringComparison.Ordinal) != -1)
			{
				charSet = new UNOBCharacterSet();
			}
			else if (header.IndexOf("UNOC", StringComparison.Ordinal) != -1)
			{
				charSet = new UNOCCMRCharacterSet();
			}
			else
			{
				throw new InvalidFormatException("Header did not contain reference to UNOA/UNOB/UNOC");
			}

			if (header.StartsWith("UNA", StringComparison.Ordinal))
			{
				//In the UNOB character set, the escape character is undefined because the delimiters
				//are all unprintable characters
				if (charSet.EscapeCharacter != UNCharacterSet.NotDefined)
				{
					charSet.EscapeCharacterChar = header[6];
				}
				charSet.ElementDelimiterChar = header[4];
				charSet.SegmentDelimiterChar = header[8];
				charSet.SubElementDelimiterChar = header[3];
			}
			return charSet;
		}

		public UNCharacterSet GetCharacterSetFromInterchangeString()
		{
			return GetCharacterSetFromInterchangeString(this.EI_HeaderText);
		}

		public static UNCharacterSet GetCharacterSetFromInterchangeString(string interchangeString, bool shouldReportWholeErrorMessage = false)
		{
			try
			{
				return InterchangeStringExtensions.GetCharacterSetFromInterchangeString(interchangeString);
			}
			catch (InvalidOperationException ex)
			{
				throw new MessageProcessingException(ex.Message, interchangeString, true, false, shouldReportWholeErrorMessage);
			}
		}

		#endregion

		#region Events Times
		public ZDateTime ReceivedDateTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				foreach (StmALog log in LogCollection)
				{
					if (log.Event.SE_Code == AutoEvents.Delivered.Code)
					{
						result = log.SL_EventTime;
						break;
					}
				}
				return result;
			}
		}

		public ZDateTime PreparationDateTime
		{
			get
			{
				if (EI_ApplicationCode == "EX1")
				{
					string date;
					string time;
					if (EI_HeaderText.StartsWith("UNB"))
					{
						string element = EI_HeaderText.Split(Encoding.ASCII.GetString(new byte[1] { 29 })[0])[4];
						date = element.Split(Encoding.ASCII.GetString(new byte[1] { 31 })[0])[0];
						time = element.Split(Encoding.ASCII.GetString(new byte[1] { 31 })[0])[1];
					}
					else if (EI_HeaderText.StartsWith("UNA"))
					{
						string uNB = EI_HeaderText.Split(Encoding.ASCII.GetString(new byte[1] { 28 })[0])[1];
						string element = uNB.Split(Encoding.ASCII.GetString(new byte[1] { 29 })[0])[4];
						date = element.Split(Encoding.ASCII.GetString(new byte[1] { 31 })[0])[0];
						time = element.Split(Encoding.ASCII.GetString(new byte[1] { 31 })[0])[1];
					}
					else
					{
						return ZDateTime.Empty;
					}

					int year = 2000 + int.Parse(date.Substring(0, 2));
					int month = int.Parse(date.Substring(2, 2));
					int day = int.Parse(date.Substring(4, 2));

					int hour = int.Parse(time.Substring(0, 2));
					int minute = int.Parse(time.Substring(2, 2));

					DateTime result = new DateTime(year, month, day, hour, minute, 0);

					return result;
				}
				else
				{
					string date;
					string time;
					if (EI_HeaderText.StartsWith("UNB"))
					{
						date = EI_HeaderText.Split('+')[4].Split(':')[0];
						time = EI_HeaderText.Split('+')[4].Split(':')[1];
					}
					else if (EI_HeaderText.StartsWith("UNA"))
					{
						date = EI_HeaderText.Split('\'')[1].Split('+')[4].Split(':')[0];
						time = EI_HeaderText.Split('\'')[1].Split('+')[4].Split(':')[1];
					}
					else
					{
						return ZDateTime.Empty;
					}

					int year = 2000 + int.Parse(date.Substring(0, 2));
					int month = int.Parse(date.Substring(2, 2));
					int day = int.Parse(date.Substring(4, 2));

					int hour = int.Parse(time.Substring(0, 2));
					int minute = int.Parse(time.Substring(2, 2));

					DateTime result = new DateTime(year, month, day, hour, minute, 0);

					return result;
				}
			}
		}

		#endregion

		#region Logging

		public void LogInterchangeInProgressForAllMessages()
		{
			foreach (EDIMessage message in ContainedMessages)
			{
				message.Logs.AddNew(Events.InterchangeInProgress);
			}
		}

		#endregion

		public ZDecimal EI_SizeInKB => (cachedSizeInKB ?? (cachedSizeInKB = GetSizeInKB())).Value; ZDecimal? cachedSizeInKB;

		ZDecimal GetSizeInKB()
		{
			if (BlobFieldsAlreadyLoaded)
			{
				return (ZDecimal)EI_InterchangeText.Length / 1024;
			}
			else
			{
				var query = @"SELECT DATALENGTH(EI_HeaderText) +
						DATALENGTH(EI_BodyText) +
						DATALENGTH(EI_FooterText) +
						DATALENGTH(EI_HeaderNText) / 2 +
						DATALENGTH(EI_BodyNText) / 2 +
						DATALENGTH(EI_FooterNText) / 2 +
						COALESCE(DATALENGTH(EI_BodyData), 0)
						FROM dbo.EDIInterchange WHERE EI_PK = @PK";

				using (var cmd = Db.Connection.Command(query))
				{
					cmd.AddParameter(ZSqlParameter.New("@PK", PK, EDIInterchangeSchema.PK));
					var length = cmd.ExecuteScalar();
					return new ZDecimal(length) / 1024;
				}
			}
		}

		public ZPropertyInfo EI_SizeInKBInfo => GetZPropertyInfo(nameof(EI_SizeInKB));

		public virtual ZString FormattedInterchangeForHumansToReadIt => EI_InterchangeText;

		readonly HashSet<ZString> CodesUseNTextOrMessageData = new HashSet<ZString>
		{
			ApplicationCodeList.Codes.AUCOLS,
			ApplicationCodeList.Codes.XMS,
			ApplicationCodeList.Codes.UniversalDataMessaging,
			ApplicationCodeList.Codes.NativeDataMessaging,
			ApplicationCodeList.Codes.SYS,
			ApplicationCodeList.Codes.CustomsWare,
			ApplicationCodeList.Codes.Telematics,
			ApplicationCodeList.Codes.ChinaInterfaceMapping,
			ApplicationCodeList.Codes.SGCustomsTradenet4,
			ApplicationCodeList.Codes.SGCustomsTradenetXML,
			ApplicationCodeList.Codes.TWCustoms,
			ApplicationCodeList.Codes.GenericMessageDelivery,
			ApplicationCodeList.Codes.TRCustoms,
			ApplicationCodeList.Codes.KRCustoms,
			ApplicationCodeList.Codes.BRCustoms,
			ApplicationCodeList.Codes.EUICS2,
			ApplicationCodeList.Codes.ILCustoms,
			ApplicationCodeList.Codes.JPCustoms,
			ApplicationCodeList.Codes.INCustoms,
			ApplicationCodeList.Codes.NOCustomsEmma
		};

		//Application codes which change from support Text only to support NText and binary (i.e. UTF8)
		readonly HashSet<ZString> CodesUseLegacyText = new HashSet<ZString>
		{
			ApplicationCodeList.Codes.GlobalElectronicInvoice
		};

		internal bool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed => ShouldUseNTextOrMessageData || ShouldUseLegacyText;

		bool ShouldUseNTextOrMessageData => CodesUseNTextOrMessageData.Contains(EI_ApplicationCode);

		bool ShouldUseLegacyText => CodesUseLegacyText.Contains(EI_ApplicationCode);

		bool HasHeaderNText => ShouldUseNTextOrMessageData || (ShouldUseLegacyText && !base.EI_HeaderNText.IsEmpty);

		bool HasFooterNText => ShouldUseNTextOrMessageData || (ShouldUseLegacyText && !base.EI_FooterNText.IsEmpty);

		public void SetEI_BodyTextOrDataSource(Stream source)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				SetEI_BodyDataSource(new StreamSource(source));
			}
			else
			{
				SetEI_BodyTextSource(new TextReaderSource(source));
			}
		}

		#region EI_HeaderText

		/// <summary>
		/// Don't use this one, as the logic is handled in EI_HeaderText
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ZString EI_HeaderNText
		{
			get
			{
				return base.EI_HeaderNText;
			}
			set
			{
				base.EI_HeaderNText = value;
			}
		}

		UNCharacterSet unCharacterSet;

		public override ZString EI_HeaderText
		{
			get
			{
				return HasHeaderNText ? base.EI_HeaderNText : base.EI_HeaderText;
			}
			set
			{
				if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
				{
					base.EI_HeaderNText = value;
				}
				else
				{
					base.EI_HeaderText = value;
				}
				_UNB = null;
				unCharacterSet = null;
			}
		}

		public bool HeaderTextIsUpdated { get; set; }

		/// <summary>
		/// Don't use this one, as the logic is handled in GetEI_BodyTextReader
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override TextReader GetEI_HeaderNTextReader(bool closeReaderBetweenReads = false)
		{
			return base.GetEI_HeaderNTextReader(closeReaderBetweenReads);
		}

		public override TextReader GetEI_HeaderTextReader(bool closeReaderBetweenReads = false)
		{
			return HasHeaderNText ? base.GetEI_HeaderNTextReader(closeReaderBetweenReads) : base.GetEI_HeaderTextReader(closeReaderBetweenReads);
		}

		/// <summary>
		/// Don't use this one, as the logic is handled in SetEI_BodyTextSource
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void SetEI_HeaderNTextSource(ITextReaderSource source)
		{
			base.SetEI_HeaderNTextSource(source);
		}

		public override void SetEI_HeaderTextSource(ITextReaderSource source)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				base.SetEI_HeaderNTextSource(source);
			}
			else
			{
				base.SetEI_HeaderTextSource(source);
			}
		}

		#endregion

		#region EI_BodyText

		/// <summary>
		/// Don't use EI_BodyNText directly, use EI_BodyText as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never), BusinessObjectTestExclude]
		public override ZString EI_BodyNText
		{
			get
			{
				if (!UsingNTextInternally)
				{
					ErrorReporter.ReportOnce("Don't use EI_BodyNText", "Don't use EI_BodyNText directly, use EI_BodyText as it has fallback logic for various locations of the data.");
				}

				return base.EI_BodyNText;
			}
			set
			{
				if (!UsingNTextInternally)
				{
					ErrorReporter.ReportOnce("Don't use EI_BodyNText", "Don't use EI_BodyNText directly, use EI_BodyText as it has fallback logic for various locations of the data.");
				}

				base.EI_BodyNText = value;
			}
		}

		bool UsingNTextInternally;

		IDisposable AllowNTextUsage()
		{
			return new DisposableAction(() => { UsingNTextInternally = true; }, () => { UsingNTextInternally = false; });
		}

#if DEBUG
		public bool ForceDeprecatedNTextUsageForTesting
		{
			get { return UsingNTextInternally; }
			set { UsingNTextInternally = value; }
		}
#endif

		public override ZString EI_BodyText
		{
			get
			{
				using (AllowNTextUsage())
				{
					if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
					{
						if (HasBodyNText)
						{
							return base.EI_BodyNText;
						}
						else if (HasBodyText)
						{
							return base.EI_BodyText;
						}
						else
						{
							return EI_BodyDataAsText;
						}
					}
					else
					{
						return base.EI_BodyText;
					}
				}
			}
			set
			{
				if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
				{
					EI_BodyDataAsText = value;
					if (HasBodyNText)
					{
						using (AllowNTextUsage())
						{
							base.EI_BodyNText = ZString.Empty;
						}
					}
					if (HasBodyText)
					{
						base.EI_BodyText = ZString.Empty;
					}
				}
				else
				{
					base.EI_BodyText = value;
				}
			}
		}

		internal ZString EI_BodyDataAsText
		{
			get { return MessageEncoding.UTF8WithoutBOM.GetString(EI_BodyData); }
			set { EI_BodyData = MessageEncoding.UTF8WithoutBOM.GetBytes(value); }
		}

		bool HasBodyText
		{
			get
			{
				using (var reader = base.GetEI_BodyTextReader(true))
				{
					return reader.GetString(1).Length > 0;
				}
			}
		}

		bool HasBodyNText
		{
			get
			{
				using (var reader = base.GetEI_BodyNTextReader(true))
				{
					return reader.GetString(1).Length > 0;
				}
			}
		}

		/// <summary>
		/// Don't use GetEI_BodyNTextReader directly, use GetEI_BodyTextReader as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override TextReader GetEI_BodyNTextReader(bool closeReaderBetweenReads = false)
		{
			ErrorReporter.ReportOnce("Don't use EI_BodyNText", "Don't use GetEI_BodyNTextReader directly, use GetEI_BodyTextReader as it has fallback logic for various locations of the data.");
			return base.GetEI_BodyNTextReader(closeReaderBetweenReads);
		}

		public override TextReader GetEI_BodyTextReader(bool closeReaderBetweenReads = false)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				if (HasBodyNText)
				{
					return base.GetEI_BodyNTextReader(closeReaderBetweenReads);
				}
				else if (HasBodyText)
				{
					return base.GetEI_BodyTextReader(closeReaderBetweenReads);
				}
				else
				{
					if (closeReaderBetweenReads)
					{
						ErrorReporter.ReportOnce("GetEI_BodyTextReader_NoCloseReaderBetweenReadsOnStreams", "Streamed reading from binary columns does not support 'closeReaderBetweenReads'. Cannot use this for Application Code types now stored in EI_BodyData.");
					}
					return new StreamReader(base.GetEI_BodyDataReader(), MessageEncoding.UTF8WithoutBOM);
				}
			}
			else
			{
				return base.GetEI_BodyTextReader(closeReaderBetweenReads);
			}
		}

		public void AddMessageProcessWarning(string warning)
		{
			_messageProcessWarnings.Add(warning);
		}

		/// <summary>
		/// Don't use EI_BodyNText", "Don't use SetEI_BodyNTextSource directly, use SetEI_BodyTextSource as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void SetEI_BodyNTextSource(ITextReaderSource source)
		{
			ErrorReporter.ReportOnce("Don't use EI_BodyNText", "Don't use SetEI_BodyNTextSource directly, use SetEI_BodyTextSource as it has fallback logic for various locations of the data.");
			base.SetEI_BodyNTextSource(source);
		}

		public override void SetEI_BodyTextSource(ITextReaderSource source)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				if (HasBodyNText)
				{
					base.EI_BodyNText = ZString.Empty;
				}
				if (HasBodyText)
				{
					base.EI_BodyText = ZString.Empty;
				}
				base.SetEI_BodyDataSource(new TextReaderStreamSource(source));
			}
			else
			{
				base.SetEI_BodyTextSource(source);
			}
		}

		public ZString EI_BodyTextShort
		{
			get
			{
				using (var reader = GetEI_BodyTextReader(false))
				{
					return reader.GetString(LargeMessageHelper.ShortTextSizeLimit);
				}
			}
		}

		public ZString EI_BodyTextDetail
		{
			get
			{
				using (var reader = GetEI_BodyTextReader(false))
				{
					return reader.GetString(LargeMessageHelper.DetailTextSizeLimit);
				}
			}
		}

		#endregion

		#region EI_FooterText

		/// <summary>
		/// Don't use this one, as the logic is handled in EI_FooterText
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ZString EI_FooterNText
		{
			get
			{
				return base.EI_FooterNText;
			}
			set
			{
				base.EI_FooterNText = value;
			}
		}

		public override ZString EI_FooterText
		{
			get
			{
				return HasFooterNText ? base.EI_FooterNText : base.EI_FooterText;
			}
			set
			{
				if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
				{
					base.EI_FooterNText = value;
				}
				else
				{
					base.EI_FooterText = value;
				}
			}
		}

		/// <summary>
		/// Don't use this one, as the logic is handled in GetEI_FooterTextReader
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override TextReader GetEI_FooterNTextReader(bool closeReaderBetweenReads = false)
		{
			return base.GetEI_FooterNTextReader(closeReaderBetweenReads);
		}

		public override TextReader GetEI_FooterTextReader(bool closeReaderBetweenReads = false)
		{
			return HasFooterNText ? base.GetEI_FooterNTextReader(closeReaderBetweenReads) : base.GetEI_FooterTextReader(closeReaderBetweenReads);
		}

		/// <summary>
		/// Don't use this one, as the logic is handled in SetEI_FooterTextSource
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void SetEI_FooterNTextSource(ITextReaderSource source)
		{
			base.SetEI_FooterNTextSource(source);
		}

		public override void SetEI_FooterTextSource(ITextReaderSource source)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				base.SetEI_FooterNTextSource(source);
			}
			else
			{
				base.SetEI_FooterTextSource(source);
			}
		}

		#endregion

		#region EI_InterchangeDateTime

		public ZDateTime EI_InterchangeDateTime
		{
			get
			{
				if (eI_InterchangeDateTime.IsEmpty && !EI_SystemCreateTimeUtc.IsEmpty)
				{
					eI_InterchangeDateTime = Env.Time.GetLocalTimeFromUtc(EI_SystemCreateTimeUtc.ToDateTime());
				}
				return eI_InterchangeDateTime;
			}
		}
		ZDateTime eI_InterchangeDateTime;

		// Needed for right-clicking on messages in grids to Export To Excel, otherwise it barfs
		public ZPropertyInfo EI_InterchangeDateTimeInfo => GetZPropertyInfo(Schema.EI_InterchangeDateTime);

		#endregion

		public bool SaveToDisk
		{
			get
			{
				using (var reader = GetEI_BodyTextReader(false))
				{
					return reader.GetString(LargeMessageHelper.DetailTextSizeLimit + 1).Length > LargeMessageHelper.DetailTextSizeLimit;
				}
			}
		}

		public virtual ZString EI_InterchangeText => EI_HeaderText + EI_BodyText + EI_FooterText;

		public ZPropertyInfo EI_InterchangeTextInfo => GetZPropertyInfo(Schema.EI_InterchangeText);

		#region EI_InterchangeText Related Fields

		public ZString EI_InterchangeTextShort
		{
			get
			{
				var stringBuilder = new StringBuilder();

				using (var reader = GetEI_BodyTextReader(false))
				{
					stringBuilder.Append(reader.ReadToEnd());
				}

				using (var stringReader = new StringReader(stringBuilder.ToString()))
				{
					return LargeMessageHelper.GetTruncatedInterchangeText(EI_HeaderText, stringReader, EI_FooterText, LargeMessageHelper.ShortTextSizeLimit, GetTextPadderForEI_InterchangeText());
				}
			}
		}

		protected virtual LargeMessageHelper.TextPadder GetTextPadderForEI_InterchangeText()
		{
			return null;
		}

		public ZString EI_InterchangeTextDetail
		{
			get
			{
				using (var reader = GetEI_BodyTextReader(false))
				{
					return LargeMessageHelper.GetTruncatedInterchangeText(EI_HeaderText, reader, EI_FooterText, LargeMessageHelper.DetailTextSizeLimit, GetTextPadderForEI_InterchangeText());
				}
			}
		}

		public ZString EI_InterchangeTextDetailFormatted
		{
			get
			{
				ZString interchangeHeader;

				if (EI_HeaderText.IsEmpty)
				{
					interchangeHeader = (EI_FooterText.IsEmpty) ? ZString.Empty : (ZString)(Res.GetString("8211da7f-566d-49de-8e67-58cf165a9a6b", "Interchange Body:") + "\r\n");
				}
				else
				{
					interchangeHeader = Res.GetString("8a307fc3-7814-4003-a62d-356429dc8a72", "Interchange Header:\r\n{0}\r\n\r\nInterchange Body:", EI_HeaderText) + "\r\n";
				}

				var interchangeFooter = (EI_FooterText.IsEmpty) ? ZString.Empty : (ZString)("\r\n\r\n" + Res.GetString("3d684d63-2cc5-40af-879d-5ca1d6748dc6", "Interchange Footer:\r\n{0}", EI_FooterText));

				using (var reader = GetEI_BodyTextReader(false))
				{
					return LargeMessageHelper.GetTruncatedInterchangeText(interchangeHeader, reader, interchangeFooter, LargeMessageHelper.DetailTextSizeLimit, GetTextPadderForEI_InterchangeText());
				}
			}
		}

		#endregion

		public GlbCompany Company => Branch?.Company;

		public ZString eHubID => base.EI_SessionGUID.IsEmpty ? "" : base.EI_SessionGUID.ToString();

		public string TransportModeDescription => IsEHubMessage ? EDIInterchangeTransportTypeList.Descriptions.eHub : EDIInterchangeTransportTypeList.Descriptions.eAdaptor;

		public bool IsEHubMessage
		{
			get
			{
				if (HasBeenQueuedForeHub() || EI_TransportType == EDIInterchangeTransportTypeList.Codes.eHub)
				{
					return true;
				}
				if (HasBeenQueuedForeAdaptor() || EI_TransportType == EDIInterchangeTransportTypeList.Codes.eAdaptor)
				{
					return false;
				}

				//This is very expensive, possibly remove if we can ensure EI_TransportType and EI_Status are enough
				if (HasBeenSentViaeHub())
				{
					return true;
				}

				return false;
			}
		}

		#region CalculatedFields

		internal bool BlobFieldsAlreadyLoaded
		{
			get
			{
				DataRow row = ((INeedRow)this).Row;
				return !IsInDatabase || row == null ||
					(!LazyLoading.LoadRequired(row[EDIInterchangeSchema.EI_BodyText.Name])
					&& !LazyLoading.LoadRequired(row[EDIInterchangeSchema.EI_BodyNText.Name])
					&& !LazyLoading.LoadRequired(row[EDIInterchangeSchema.EI_BodyData.Name]));
			}
		}

		public bool IsTransmitInterchange
		{
			get { return EI_ReceiveTransmit == Direction.Transmit; }
			set { EI_ReceiveTransmit = value ? Direction.Transmit : Direction.Receive; }
		}

		public bool HasBeenAcknowledgedByERouter => InterchangeAcknowledgementMessages.Find(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.ERouter)).Length > 0;

		public bool EI_IsDuplicateInterchange => ExistingInterchangeMatchingToFromAndInterchangeNum != null;

		public EDIInterchange ExistingInterchangeMatchingToFromAndInterchangeNum
		{
			get
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(EDIInterchangeSchema.EI_From, EI_From);
				sQLFilter.AddToFilter(EDIInterchangeSchema.EI_To, EI_To);
				sQLFilter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, EI_InterchangeNum);
				sQLFilter.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, PK);
				return (EDIInterchange)Factory.LoadTop1(typeof(EDIInterchange), sQLFilter);
			}
		}

		public bool EI_NeedsAcknowledgement => EI_NeedsAcknowledgementCore;

		protected virtual bool EI_NeedsAcknowledgementCore => false;

		public ZString NewInterchangeStatus(ZString oldInterchanegStatus)
		{
			return NewInterchangeStatusCore(oldInterchanegStatus);
		}

		protected virtual ZString NewInterchangeStatusCore(ZString oldInterchanegStatus)
		{
			return oldInterchanegStatus;
		}

		#endregion

		public void AssignInterchangeNumber()
		{
			if (IsInDatabase)
			{
				return;
			}

			if (EI_InterchangeNum.IsEmpty || interchangeNumberImplicitlyAssigned)
			{
				EI_InterchangeNum = GetInterchangeNumber();
			}

			var interchangeNumberReplacementString = GetInterchangeNumberReplacementString(EI_InterchangeNum);
			EI_HeaderText = EI_HeaderText.Replace(InterchangeNumberPlaceHolder, interchangeNumberReplacementString);
			EI_FooterText = EI_FooterText.Replace(InterchangeNumberPlaceHolder, interchangeNumberReplacementString);
		}

		bool interchangeNumberImplicitlyAssigned;
		#region BusinessObjectOverrides
		public override void OnSaving()
		{
			base.OnSaving();
			OldEI_HeaderText = EI_HeaderText;
			OldEI_FooterText = EI_FooterText;

			AssignInterchangeNumber();
			interchangeNumberImplicitlyAssigned = true;

			if (EI_HeaderText.Contains(FunctionalGroupNumberPlaceHolder))
			{
				ZString groupNumber = GetFunctionalGroupNumber();
				EI_HeaderText = EI_HeaderText.Replace(FunctionalGroupNumberPlaceHolder, groupNumber);
				EI_FooterText = EI_FooterText.Replace(FunctionalGroupNumberPlaceHolder, groupNumber);
			}

			if (ShouldBatchNumberBeByInterchange && EI_ReceiveTransmit == Direction.Transmit && EI_BodyText.Contains(UniqueBatchNumberPlaceHolderOverride))
			{
				OldEI_BodyText = EI_BodyText;
				ReplaceBatchNumber(GetBatchNumber());
			}

			if (EI_TransportTypeInfo.HasChanges)
			{
				UpdateEDIMessageTransportType();
			}

			FireOnSavingEvent();

			isNewRecord = !IsInDatabase;
			if (isNewRecord)
			{
				CheckInterchangeTypeIsSupportedForGMD();
			}
			else
			{
				var eiStatusInfo = EI_StatusInfo;
				if (eiStatusInfo.HasChanges)
				{
					var originalValue = (ZString)eiStatusInfo.OriginalValue;
					var currentValue = EI_Status;
					if (originalValue != currentValue)
					{
						Logs.AddNew(Events.StatusUpdated, GetUpdatedStatusEventParameters(originalValue, currentValue));
					}
				}
			}
		}

		void CheckInterchangeTypeIsSupportedForGMD()
		{
			if (EI_ApplicationCode == ApplicationCodes.GenericMessageDelivery && !IsTransmitInterchange && !EI_InterchangeType.IsEmpty && !Factory.GetCachedValue<GenericMessageDeliveryInterchangeTypeList>().ContainsCode(EI_InterchangeType))
			{
				ErrorReporter.ReportOnce(System.FormattableString.Invariant($"Unknown Interchange Type '{EI_InterchangeType}'; receive {ApplicationCodes.GenericMessageDelivery} Interchange must be in the supported list '{typeof(GenericMessageDeliveryInterchangeTypeList).FullName}'"));
			}
		}

		void FireOnSavingEvent()
		{
			Saving?.Invoke(this);
		}

		bool isNewRecord;
		public event SavingEventHandler<EDIInterchange> Saving;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				EI_HeaderText = OldEI_HeaderText;
				EI_FooterText = OldEI_FooterText;
				if (!OldEI_BodyText.IsEmpty)
				{
					EI_BodyText = OldEI_BodyText;
					OldEI_BodyText = ZString.Empty;
				}
			}
		}

		public override void Delete()
		{
			ContainedMessages.RemoveAndDeleteAll();
			if (this.IsInDatabase && !IsDeleted)
			{
#if !DEBUG //Only reporting for release system
				string reasonWhyInterchangeCannotBeDeleted;
				if (ReportDelete(out reasonWhyInterchangeCannotBeDeleted))
				{
					ErrorReporter.ReportOnce(reasonWhyInterchangeCannotBeDeleted + " " + DiagnosticDetails);
				}
#endif
			}

			base.Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ErrorReporter message")]
		internal bool ReportDelete(out string reason)
		{
			if (HasBeenQueuedForeAdaptor())
			{
				reason = "You are attempting to delete an EDIInterchange that has been queued EAM service task.";
				return true;
			}
			else if (HasBeenSentViaeAdaptor())
			{
				switch (EI_Status)
				{
					case EDIInterchange.Status.Failed:
						reason = "You are attempting to delete an EDIInterchange that may have already been sent to your eAdaptor web service.";
						break;
					case EDIInterchange.Status.Sent:
						reason = "You are attempting to delete an EDIInterchange that has already been sent to your eAdaptor web service.";
						break;
					default:
						reason = "You are attempting to delete eAdaptor EDIInterchange, interchange status is " + EI_Status + ".";
						break;
				}
				return true;
			}

			if (HasBeenQueuedForeHub())
			{
				reason = "You are attempting to delete an EDIInterchange that has been queued for EHO service task.";
				return true;
			}
			else if (HasBeenSentViaeHub())
			{
				switch (EI_Status)
				{
					case EDIInterchange.Status.Failed:
						reason = "You are attempting to delete an EDIInterchange that may have already been sent to eHub.";
						break;
					case EDIInterchange.Status.eHubPending:
						reason = "You are attempting to delete an EDIInterchange that has already been sent to eHub.";
						break;
					case EDIInterchange.Status.Sent:
						reason = "You are attempting to delete an EDIInterchange that has already been sent to the recipient.";
						break;
					default:
						reason = "You are attempting to delete eHub EDIInterchange, interchange status is " + EI_Status + ".";
						break;
				}
				return true;
			}

			if (HasBeenReceivedViaeHubOreAdaptor() && !IsStatusMessage())
			{
				reason = "You are attempting to delete an EDIInterchange that has been received via eHub or eAdaptor. This is not allowed due to tracking requirements.";
				return true;
			}
			reason = null;
			return false;
		}

		public string DiagnosticDetails
		{
			get
			{
				var builder = new ZStringBuilder();
				foreach (SchemaColumn column in EDIInterchangeSchema.All)
				{
					if (column == null)
					{
						continue;
					}

					var property = GetType().GetProperty(column.Name);
					if (property != null && !property.GetCustomAttributes(typeof(BusinessObjectTestExclude), false).Any())
					{
						var value = property.GetValue(this, null);
						if (value != null && !string.IsNullOrEmpty(value.ToString()) && !IsEmptyByteArray(value))
						{
							builder.Append(string.Format("{0}: {1}", ZPropertyInfo.GetFriendlyColumnNameShared(column.Name), value));
						}
					}
				}
				return string.Format("{{{0}}}", builder.ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		bool IsEmptyByteArray(object value)
		{
			var bytes = value as byte[];
			if (bytes == null)
			{
				return false;
			}

			return bytes.Length == 0;
		}

		#endregion

		//public event MessageGeneratedEventHandler MessageGenerated;
		public const string InterchangeNumberPlaceHolder = "<<INTERCHANGENUMBERPLACEHOLDER>>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string InterchangeNumberPlaceHolderHtml = "&lt;&lt;INTERCHANGENUMBERPLACEHOLDER&gt;&gt;";
		public const string FunctionalGroupNumberPlaceHolder = "<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>";

		#region Implementation

		public IMessageNumberStrategy NumberStrategy { get; set; }

		protected virtual ZString GetInterchangeNumber()
		{
			if (NumberStrategy != null)
			{
				return NumberStrategy.GetMessageReferenceNumber();
			}
			else
			{
				return Env.NumberFountains.EDIFACTNumberFountain("I", EI_From, EI_To).GetNextFormatted(Factory).ToUpper();
			}
		}

		protected virtual ZString GetInterchangeNumberReplacementString(ZString interchangeNumber)
		{
			return interchangeNumber;
		}

		protected ZString GetFunctionalGroupNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("G", EI_From, EI_To).GetNextFormatted(Factory).ToUpper();
		}

		protected virtual bool ShouldBatchNumberBeByInterchange => false;

		protected virtual string GetBatchNumber()
		{
			return ContainedMessages.Count > 0 ? ContainedMessages[0].GetBatchNumberForInterchange() : string.Empty;
		}

		protected virtual void ReplaceBatchNumber(string batchNumber)
		{
			foreach (EDIMessage message in ContainedMessages)
			{
				message.EM_MessageText = message.EM_MessageText.Replace(UniqueBatchNumberPlaceHolderOverride, batchNumber);
			}
			EI_BodyText = EI_BodyText.Replace(UniqueBatchNumberPlaceHolderOverride, batchNumber);
		}

		protected virtual void UpdateEDIMessageTransportType()
		{
			foreach (EDIMessage message in ContainedMessages)
			{
				message.EM_TransportType = EI_TransportType;
			}
		}

		protected virtual string UniqueBatchNumberPlaceHolderOverride => EDIMessage.UniqueBatchNumberPlaceHolder;

		protected ZString OldEI_HeaderText;
		protected ZString OldEI_FooterText;
		protected ZString OldEI_BodyText;

		protected EDIInterchangeEDIMessageCollection fContainedMessages;
		protected EDIMessageCollection fInterchangeAcknowledgementMessages;
		protected StmALogCollection fLogCollection;

		public void SpawnMessagesFromInterchageTextAndMarkAsReceived(bool throwExceptionIfInDatabase = true)
		{
			SpawnMessagesFromInterchangeText(IsTestInterchange, throwExceptionIfInDatabase);
			EI_Status = EDIInterchange.Status.Received;
		}

		public UNBSegment UNB
		{
			get
			{
				if (_UNB == null)
				{
					ZString[] segments = EI_HeaderText.SplitIgnoringEscapedDelimiter(CharacterSet.SegmentDelimiterChar, CharacterSet.EscapeCharacterChar, true);
					foreach (ZString segment in segments)
					{
						if (segment.StartsWith("UNB"))
						{
							_UNB = new UNBSegment();
							_UNB.Parse(CharacterSet, segment.Substring(0, segment.Length - 1));
							break;
						}
					}
				}
				return _UNB;
			}
		}
		UNBSegment _UNB;

		public virtual bool IsTestInterchange
		{
			get
			{
				if (!_IsTestInterchangeSet && UNB != null)
				{
					_IsTestInterchange = UNB.TestIndicator == "1";
					_IsTestInterchangeSet = true;
				}
				return _IsTestInterchange;
			}
		}
		bool _IsTestInterchange;
		bool _IsTestInterchangeSet;

		protected void SpawnMessagesFromInterchangeText(bool testFlag, bool throwExceptionIfInDatabase = true)
		{
			if (throwExceptionIfInDatabase && IsInDatabase)
			{
				throw new InvalidOperationException("You can't spawn the messages from the interchange text because the interchange is already in the database and hence its messages probably are as well.");  // I kan spel tu!
			}

			bool isTestInterchange = testFlag || ApplicationCodes.GetTestModeFromInterchangeSender(EI_From);

			var messagesText = InterchangeStringExtensions.SpawnMessagesFromInterchangeText(EI_BodyText, CharacterSet);

			for (int i = 0; i < messagesText.Count; i++)
			{
				GenerateMessage(i + 1, messagesText[i], isTestInterchange);
			}
		}

		protected virtual string GetMessageNum(string messageText, int messageNumberSequece)
		{
			string uNHString = messageText.Substring(0, messageText.IndexOf(CharacterSet.SegmentDelimiterChar));
			try
			{
				UNHSegment segment = new UNHSegment();
				segment.Parse(CharacterSet, uNHString);
				return segment.MessageReferenceNumber;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return "";
			}
		}

		protected virtual Type GetMessageTypeToCreate(ZString messageText)
		{
			return typeof(EDIMessage);
		}

		protected void GenerateMessage(int messageNumber, string message, bool isTestInterchange)
		{
			EDIMessage newEDIMessage = ContainedMessages.AddNew(GetMessageTypeToCreate(message));
			newEDIMessage.EM_MessageText = message;
			newEDIMessage.EM_IsTestMessage = isTestInterchange;
			newEDIMessage.EM_ApplicationReference = messageNumber.ToString().PadLeft(6, '0');
			newEDIMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			ZString messageNum = GetMessageNum(message, messageNumber);

			newEDIMessage.EM_MessageNum = messageNum;

			try
			{
				OnMessageGenerated(newEDIMessage);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Interchange Message Generated Event Handler Exception", exception);
			}
		}

		protected virtual void OnMessageGenerated(EDIMessage message)
		{
		}

		public void ResetToQueuedStatus() => ResetToQueuedStatusCore();

		protected virtual void ResetToQueuedStatusCore()
		{
			var receiveTransmit = EI_ReceiveTransmit;
			if (receiveTransmit.Equals(Direction.Transmit))
			{
				if (HasBeenSentViaeAdaptor())
				{
					EI_TransportType = EDIInterchange.TransportType.eAdaptor;
					EI_Status = EDIInterchange.Status.eAdaptorQueued;
				}
				else if (HasBeenSentViaeHub())
				{
					EI_TransportType = EDIInterchange.TransportType.eHub;
					EI_Status = EDIInterchange.Status.eHubQueued;
				}
				else if (HasQueueStatusLogged())
				{
					EI_Status = EDIInterchange.Status.Queued;
				}

				EI_RetryCount = 0;
			}
			else if (receiveTransmit.Equals(Direction.Receive))
			{
				EI_Status = EDIInterchange.Status.Queued;
				EI_RetryCount = 0;
			}
		}

		bool HasBeenQueuedForeAdaptor()
		{
			return EI_Status == EDIInterchange.Status.eAdaptorQueued;
		}

		bool HasBeenQueuedForeHub()
		{
			return EI_Status == EDIInterchange.Status.eHubQueued;
		}

		internal bool HasBeenSentViaeAdaptor()
		{
			return DoesAnyLogReferenceContain(EDIInterchange.Status.eAdaptorQueued);
		}

		internal bool HasBeenSentViaeHub()
		{
			return DoesAnyLogReferenceContain(EDIInterchange.Status.eHubQueued);
		}

		internal bool HasQueueStatusLogged()
		{
			return DoesAnyLogReferenceContain(EDIInterchange.Status.Queued);
		}

		internal bool HasBeenReceivedViaeHubOreAdaptor()
		{
			return EI_ReceiveTransmit == EDIInterchange.Direction.Receive && !EI_SessionGUID.IsEmpty;
		}

		internal bool IsStatusMessage()
		{
			return EI_InterchangeType == EDIInterchangeTypeList.Codes.MessageStatusAcknowledgment ||
							EI_InterchangeType == EDIInterchangeTypeList.Codes.MSF ||
							EI_InterchangeType == EDIInterchangeTypeList.Codes.MSS;
		}

		bool DoesAnyLogReferenceContain(ZString queuedStatusCode)
		{
			return Logs.GetAllLogs().Cast<StmALog>().Any(log => log.SL_Reference.Contains(queuedStatusCode));
		}

		internal static KeyValuePair<string, string>[] GetUpdatedStatusEventParameters(ZString oldStatus, ZString newStatus)
			=> new[] {
				new KeyValuePair<string, string>(ParameterCodes.New, newStatus),
				new KeyValuePair<string, string>(ParameterCodes.Old, oldStatus),
			};

		#endregion

		#region IDocManagerSupport Members

		public virtual DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.InterchangeAttachments);
					docManagerInfo.UseBusinessEntityFactoryAsInternal = false;
				}

				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region Audit Columns

		[CargoWise.ComponentModel.List("Lookups.GlbStaffs")]
		public override ZString EI_SystemCreateUser { get => base.EI_SystemCreateUser; set => base.EI_SystemCreateUser = value; }

		[CargoWise.ComponentModel.List("Lookups.GlbStaffs")]
		public override ZString EI_SystemLastEditUser { get => base.EI_SystemLastEditUser; set => base.EI_SystemLastEditUser = value; }

		#endregion
	}
}
