using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public struct CUSCARLineKey
	{
		public ZString container { get; set; }
		public ZString masterBill { get; set; }
		public ZString houseBill { get; set; }
		public ZString LineNumber { get; set; }
		public ZString EDN { get; set; }

		public override string ToString()
		{
			return (container + "/" + masterBill + "/" + houseBill).ToUpper();
		}
	}

	public class CMRMessage : EDIMessage
	{
		public CMRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider
		public static readonly new TypeDecider TypeDecider = new CMRTypeDecider();
		#endregion

		public const string MessagingHelpUpdateNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseUpdateNote20090701h.pdf";

		public static string MessagingHelpMenuCaption
		{
			get
			{
				return Res.GetString("d1b5a523-c053-4349-abc5-ecbbb24317af", "Messaging Problems? Click for HELP.");
			}
		}

		public Edifact.Auto.SegmentGroup AutoEdifactMessage
		{
			get
			{
				if (autoEdifactMessage == null)
				{
					autoEdifactMessage = GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet());
				}
				return autoEdifactMessage;
			}
		}
		Edifact.Auto.SegmentGroup autoEdifactMessage;

		public ZString BGMMessageType
		{
			get
			{
				ZString result = ZString.Empty;
				if (AutoEdifactMessage is Edifact.D99B.Messages.CUSRES.CUSRESMessage)
				{
					result = ((Edifact.D99B.Messages.CUSRES.CUSRESMessage)AutoEdifactMessage).BGM[0].DocumentMessageName.DocumentName;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					result = ((Edifact.D99B.Messages.CUSDEC.CUSDECMessage)AutoEdifactMessage).BGM[0].DocumentMessageName.DocumentName;
				}
				else if (AutoEdifactMessage is CUSCARMessage)
				{
					result = ((CUSCARMessage)AutoEdifactMessage).BGM[0].DocumentMessageName.DocumentName;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSPED.CUSPEDMessage)
				{
					result = ((Edifact.D99B.Messages.CUSPED.CUSPEDMessage)AutoEdifactMessage).BGM[0].DocumentMessageName.DocumentName;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSREP.CUSREPMessage)
				{
					result = ((Edifact.D99B.Messages.CUSREP.CUSREPMessage)AutoEdifactMessage).BGM[0].DocumentMessageName.DocumentName;
				}
				return result;
			}
		}

		public ZString BGMReference
		{
			get
			{
				ZString result = ZString.Empty;
				if (AutoEdifactMessage is Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					result = ((Edifact.D99B.Messages.CUSDEC.CUSDECMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
				}
				else if (AutoEdifactMessage is CUSCARMessage)
				{
					result = ((CUSCARMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSPED.CUSPEDMessage)
				{
					result = ((Edifact.D99B.Messages.CUSPED.CUSPEDMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSREP.CUSREPMessage)
				{
					result = ((Edifact.D99B.Messages.CUSREP.CUSREPMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
				}
				return result;
			}
		}

		public ZInt BGMReferenceVersion
		{
			get
			{
				ZString stringResult = ZString.Empty;
				if (AutoEdifactMessage is Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					stringResult = ((Edifact.D99B.Messages.CUSDEC.CUSDECMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.Version;
				}
				else if (AutoEdifactMessage is CUSCARMessage)
				{
					stringResult = ((CUSCARMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.Version;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSPED.CUSPEDMessage)
				{
					stringResult = ((Edifact.D99B.Messages.CUSPED.CUSPEDMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.Version;
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSREP.CUSREPMessage)
				{
					stringResult = ((Edifact.D99B.Messages.CUSREP.CUSREPMessage)AutoEdifactMessage).BGM[0].DocumentMessageIdentification.Version;
				}
				ZInt result = ZInt.Zero;
				ZInt.TryParse(stringResult, out result);
				return result;
			}
		}

		public ZString BGMMessageFunctionCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (AutoEdifactMessage is Edifact.D99B.Messages.CUSRES.CUSRESMessage)
				{
					result = ((Edifact.D99B.Messages.CUSRES.CUSRESMessage)AutoEdifactMessage).BGM[0].MessageFunctionCode.ToString();
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					result = ((Edifact.D99B.Messages.CUSDEC.CUSDECMessage)AutoEdifactMessage).BGM[0].MessageFunctionCode.ToString();
				}
				else if (AutoEdifactMessage is CUSCARMessage)
				{
					result = ((CUSCARMessage)AutoEdifactMessage).BGM[0].MessageFunctionCode.ToString();
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSPED.CUSPEDMessage)
				{
					result = ((Edifact.D99B.Messages.CUSPED.CUSPEDMessage)AutoEdifactMessage).BGM[0].MessageFunctionCode.ToString();
				}
				else if (AutoEdifactMessage is Edifact.D99B.Messages.CUSREP.CUSREPMessage)
				{
					result = ((Edifact.D99B.Messages.CUSREP.CUSREPMessage)AutoEdifactMessage).BGM[0].MessageFunctionCode.ToString();
				}
				return result;
			}
		}

		CUSCARMessage cUSCAR;
		protected CUSCARMessage CUSCAR
		{
			get
			{
				if (cUSCAR == null && AutoEdifactMessage is CUSCARMessage)
				{
					cUSCAR = (CUSCARMessage)GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet());
				}
				return cUSCAR;
			}
		}

		internal void ReleaseEdifactObject()
		{
			cUSCAR = null;
			autoEdifactMessage = null;
		}

		public IEnumerable<CUSCARLineKey> LineKeys()
		{
			if (CUSCAR != null)
			{
				foreach (SegmentGroup7 group7 in CUSCAR.Group7)
				{
					if (group7.CNI[0].DocumentMessageDetails.LanguageNameCode != LineAction.Delete)
					{
						CUSCARLineKey key = new CUSCARLineKey();
						key.LineNumber = group7.CNI[0].ConsolidationItemNumber;
						foreach (SegmentGroup8 group8 in group7.Group8)
						{
							if (group8.RFF.Count > 0)
							{
								if (group8.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
								{
									key.container = group8.RFF[0].Reference.ReferenceIdentifier;
								}

								if (group8.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber ||
									group8.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber)
								{
									key.masterBill = group8.RFF[0].Reference.ReferenceIdentifier;
								}

								foreach (RFFSegment rFF in group8.RFF)
								{
									if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber ||
										rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.HouseWaybillNumber)
									{
										key.houseBill = rFF.Reference.ReferenceIdentifier;
									}
									else if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber)
									{
										key.EDN = rFF.Reference.ReferenceIdentifier;
									}
								}
							}
						}

						yield return key;
					}
				}
			}
		}

		#region Overrides

		public override string CollationKey
		{
			get { return base.CollationKey + EM_MessageType; }
		}

		protected override void PopulateMessageNumber()
		{
			// Message Numbers allocated by Batch Processor per interchange
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override string GetSendersReference()
		{
			switch (EM_LinkedObject)
			{
				case JobDeclaration declaration:
					{
						declaration.PopulateJE_DeclarationReferenceIfNeeded();
						return declaration.JE_DeclarationReference.ToUpper();
					}
				case ConsolidatedDeclaration consolidatedDeclaration:
					return consolidatedDeclaration.CRD_JobReferenceNumber.ToUpper();
				case ForwardingConsol consol:
					{
						consol.PopulateJK_UniqueConsignRefIfNeeded();
						return consol.JK_UniqueConsignRef.ToUpper();
					}
				case ExportCustomsManifestHeader header:
					{
						header.PopulateED_BGMReferenceIfNeeded();
						return header.ED_BGMReference;
					}
				case CusPartShip partShip:
					{
						partShip.SetMessageReference();
						((ISendersMessageReferenceProvider)partShip.HouseBill).PopulateSendersReferenceIfNeeded();
						return "P" + ((ISendersMessageReferenceProvider)partShip.HouseBill).SendersReference + "/" + partShip.CG_MessageReference;
					}
				case CusHAWB hAWB:
					{
						hAWB.SetMessageReference();
						return hAWB.CS_MessageReference;
					}
				case CusMAWB mAWB:
					return mAWB.CM_MAWB;
				case CusSCAHouse house:
					{
						house.PopulateCA_BGMReferenceIfNeeded();
						return house.CA_BGMReference;
					}
				case CusSCAOceanBill oceanBill:
					return oceanBill.CB_OceanBill;
				case CusEntryHeader entryHeader:
					{
						entryHeader.PopulateCH_BGMReferenceIfNeeded();
						return entryHeader.CH_BGMReference;
					}
				case CusSCAPivot pivot:
					return pivot.PK.ToString().Substring(0, 30);
				case CusUnderbond underbond:
					{
						underbond.PopulateC4_SendersMessageReferenceIfNeeded();
						return underbond.C4_SendersMessageReference;
					}
				case ISendersMessageReferenceProvider sendersMessageReferenceProvider:
					{
						sendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded();
						return sendersMessageReferenceProvider.SendersReference;
					}
			}

			throw new OdysseyException("Don't know how to get the senders reference for this message.");
		}

		protected override string GetAgentReference()
		{
			string result = string.Empty;

			if (EntryHeader != null)
			{
				if (EM_LinkedObject is ConsolidatedDeclaration consolidatedDeclaration)
				{
					EntryHeader.CH_BGMReference = consolidatedDeclaration.CRD_JobReferenceNumber;
				}
				else
				{
					EntryHeader.PopulateCH_BGMReferenceIfNeeded();
				}

				result = FilterForValidEDIFACTCharacters(EntryHeader.AgentReference);
			}
			else if (Declaration != null)
			{
				result = FilterForValidEDIFACTCharacters(Declaration.JE_AgentsReference).Trim();
				if (result.Length > 20)
				{
					result = result.Substring(0, result.Substring(19, 1) == "?" ? 21 : 20);
				}
			}
			return result;
		}

		protected override string GetOwnerReference()
		{
			string result = string.Empty;
			if (Declaration != null)
			{
				result = FilterForValidEDIFACTCharacters(Declaration.JE_OwnerRef);
			}
			if (result.Length > 20)
			{
				result = result.Substring(0, result.Substring(19, 1) == "?" ? 21 : 20);
			}

			return result;
		}

		public override void OnSaving()
		{
			if (Declaration != null)
			{
				Declaration.PopulateJE_OwnerRefIfNeeded();
			}
			base.OnSaving();
		}

		protected JobDeclaration Declaration
		{
			get
			{
				if (_declaration == null)
				{
					if (EM_LinkedObject is JobDeclaration declaration)
					{
						_declaration = declaration;
					}
					else if (EM_LinkedObject is ConsolidatedDeclaration consolidatedDeclaration)
					{
						_declaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
					}
					else if (EntryHeader != null)
					{
						_declaration = EntryHeader.Declaration;
					}
				}
				return _declaration;
			}
		}
		JobDeclaration _declaration;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (_entryHeader == null)
				{
					if (EM_LinkedObject is CusEntryHeader entryHeader)
					{
						_entryHeader = entryHeader;
					}
					else if (EM_LinkedObject is ConsolidatedDeclaration consolidatedDeclaration)
					{
						_entryHeader = (CusEntryHeader)consolidatedDeclaration.LeadDeclaration.ActiveEntryHeaders.FirstOrDefault();
					}
				}
				return _entryHeader;
			}
		}
		CusEntryHeader _entryHeader;

#if DEBUG
		internal void ResetCachedValuesForTest()
		{
			_entryHeader = null;
			_declaration = null;
		}
#endif

#if DEBUG
		public
#endif
 static string FilterForValidEDIFACTCharacters(ZString textField)
		{
			ZString result = textField.ToUpper().KeepChars(IMDJobDeclarationValidation.ValidEDIFACTCharacters);
			return result.Replace("'", "?'").Replace(":", "?:").Replace("+", "?+");
		}

		#endregion

		#region IsRejected
		public bool IsRejected
		{
			get { return EM_MessageSubType == EDIMessage.Status.Rejected; }
		}
		#endregion

		#region Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_IsTestMessage = Env.Registry.CMRTestMode;
			EM_ApplicationCode = ApplicationCodes.CMR;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_Status = EDIMessage.Status.Queued;
		}

		#region Constants

		public static class CMRMessageStatusDescription
		{
			public const string ACCEPTED = "ACCEPTED";
			public const string REJECTED = "REJECTED";
			public const string WITHDRAWN = "WITHDRAWN";
			public const string CLEAR = "CLEAR";
			public const string ERROR = "ERROR";
			public const string AMENDMENTDETECTED = "AMENDMENT DETECTED";
		}

		public static class CMRMessageTypes
		{
			public const string CONTRL = "CTL";
			public const string ERM = "ERM";
			public const string EXD = "EXD";
			public const string EXDR = "EXR";
			public const string EXREL = "EXL";
			public const string ESM = "ESM";
			public const string EMM = "EMM";
			public const string DEPART = "DEP";
			public const string STREQ = "STR";
			public const string DEPREC = "DER";
			public const string DEPREL = "DEL";
			public const string WARREL = "WAL";
			public const string WARRET = "WAT";
			public const string CTOREC = "CTC";
			public const string CTOREM = "CTM";
			public const string RACEAN = "RAC";
			public const string EXPED1 = "PD1";
			public const string EXPED2 = "PD2";
			public const string CARMOV = "CMV";

			public const string UBMREQ = "UBM";

			//Air
			public const string AIRCR = "ACR";
			public const string SEACR = "SCR";

			public const string AIRAAR = "AAR";
			public const string AIRIAR = "AIR";
			public const string AIRINT = "ANT";
			public const string AIROUT = "AUT";
			public const string CARLST = "CST";
			public const string PRODIS = "PIS";
			public const string RCR = "RCR";
			public const string SEAAAR = "SAR";
			public const string SEAIAR = "SIR";
			public const string SEAINT = "SNT";
			public const string SEAOUT = "SUT";
			public const string UBMREQE = "URE";
			public const string UBMREQR = "URR";
			public const string SEQ = "SEQ";
			public const string SEI = "SEI";

			public const string ATD = "ATD";

			public const string CARREP = "CRP";
			public const string CARST = "CRS";
			public const string CONREM = "CRM";
			public const string DEPARR = "DRR";

			public const string DOCS = "DOC";
			public const string DRWBCK = "DRW";
			public const string DSA = "DSA";
			public const string EXAM = "EXM";
			public const string IDL = "IDL";
			public const string IMD = "IMD";
			public const string IMPED1 = "IM1";
			public const string IMPED2 = "IM2";

			public const string MOVAPP = "MAP";

			public const string PAYEXC = "PAX";
			public const string PAYDEF = "PAD";
			public const string PAYINV = "PAI";
			public const string PAYOUT = "PAO";
			public const string PAYREC = "PAR";
			public const string PAYSTD = "PAS";

			public const string REFACC = "RCC";

			public const string REFREJ = "RRJ";

			public const string SAC = "SAC";
			public const string SAM = "SAM";

			public const string XRAYADV = "XRY";

			public const string CLREG = "CRG";
			public const string CLNTDUP = "CLD";
		}

		public static class ManifestResponseSubTypes
		{
			public const string Clear = "CLR";
			public const string Error = "ERR";
			public const string Rejected = "REJ";
			public const string Revoked = "REV";
			public const string Withdrawn = "WDW";
		}

		public static class MovementStatusResponseSubTypes
		{
			public const string Load = "LOD";
			public const string DoNotLoad = "DNL";
			public const string HoldForCustoms = "HLD";
			public const string Unknown = "UNK";
		}

		public static class MessageSubTypes
		{
			public const string Original = "ORG";
			public const string Amendment = "AMD";
			public const string Withdraw = "WDW";
			public const string Change = "CHG";
			public const string ReplaceHeader = "REH";
			public const string Request = "REQ";
		}

		#endregion

		#endregion
	}
}
