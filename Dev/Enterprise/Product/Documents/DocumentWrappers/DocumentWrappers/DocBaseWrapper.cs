using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappersCore;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocBaseWrapper : DocBaseWrapperBaseWithImageSupport, IDocManagerPlaceholderBarcode, IDocManagerBarcode
	{
		protected DocBaseWrapper(object objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region Menu Filter Fields

		public virtual ZBool PrintStandard
		{
			get
			{
				return ZBool.True;
			}
		}

		public virtual ZBool PrintClientSpecific
		{
			get
			{
				return ZBool.False;
			}
		}

		#endregion

		#region Format Number

		/// <summary>
		/// This takes in a number and if it is zero it will return "0".
		/// If the number ends in ".000" is will strip the zeros off the end.
		/// Otherwise it will return the number to the significant digit.
		/// </summary>
		/// <param name="number">The number you want to format</param>
		public ZString FormatNumber(ZDecimal number)
		{
			ZString result = Utilities.Round(number, 3).ToString();

			if (result.Contains(DecimalSeparator))
			{
				result = result.TrimEnd('0');
				result = result.TrimEnd(DecimalSeparator.ToCharArray());
			}

			return result;
		}

		public static string DecimalSeparator
		{
			get { return Culture.Current.NumberFormat.NumberDecimalSeparator; }
		}

		/// <summary>
		/// This takes a decimal number and makes sure that it has at least the minimum decimals.
		/// If there were less than the minimum decimals, this would add required number of "0".
		/// If the minimum decimals was 0 and the number hasn't got any decimal but '.', it would be removed.
		/// </summary>
		/// <param name="number">The number you want to format</param>
		/// <param name="minDecimals">The minimum decimals you want the formatted number has</param>
		public ZString FormatNumber(ZDecimal number, int minDecimals)
		{
			ZString result = Utilities.Round(number, 3).ToString();

			if (result.Contains(DecimalSeparator))
			{
				result = result.TrimEnd('0');
			}

			if (minDecimals > 0)
			{
				if (!result.Contains(DecimalSeparator))
				{
					result += DecimalSeparator;
				}

				ZString[] split = result.Split(DecimalSeparator.ToCharArray());
				if (split[1].Length < minDecimals)
				{
					result = result.PadRight(result.Length + (minDecimals - split[1].Length), '0');
				}
			}
			else
			{
				result = result.TrimEnd(DecimalSeparator.ToCharArray());
			}

			return result;
		}

		#endregion

		#region Statement

		public ZString GetStatementFooterAddress(BusinessObjectFactory factory)
		{
			ZString result = ZString.Empty;
			if (CurrentBranch != null && CurrentBranch.MailToAddress != null)
			{
				result = CurrentBranch.MailToAddress.Organisation.Name + "\n"
					+ CurrentBranch.MailToAddress.Address1 + ", " + CurrentBranch.MailToAddress.Address2 + "\n"
					+ CurrentBranch.MailToAddress.City + " " + CurrentBranch.MailToAddress.State + " "
					+ CurrentBranch.MailToAddress.PostCode + " " + CurrentBranch.MailToAddress.Country;
			}

			return result;
		}

		#endregion

		#region Wrapper Fields

		#region Current Company

		protected DocCompany fCurrentCompany;
		public DocCompany CurrentCompany
		{
			get
			{
				if (fCurrentCompany == null)
				{
					fCurrentCompany = DocCompany.New(GlbCompany.CurrentCompany, Factory);
				}

				return fCurrentCompany;
			}
		}

		#endregion

		#region Current Branch

		protected DocBranch fCurrentBranch;
		public DocBranch CurrentBranch
		{
			get
			{
				if (fCurrentBranch == null)
				{
					fCurrentBranch = DocBranch.New(GlbBranch.CurrentBranch, Factory);
				}

				return fCurrentBranch;
			}
		}

		protected AccBankAccount fCurrentBranchDefaultBankAccount;
		public AccBankAccount CurrentBranchDefaultBankAccount
		{
			get
			{
				if (fCurrentBranchDefaultBankAccount == null)
				{
					fCurrentBranchDefaultBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
				}

				return fCurrentBranchDefaultBankAccount;
			}
		}

		#endregion

		#region Current Department

		protected DocDepartment fCurrentDepartment;
		public DocDepartment CurrentDepartment
		{
			get
			{
				if (fCurrentDepartment == null)
				{
					fCurrentDepartment = DocDepartment.New(GlbDepartment.CurrentDepartment, Factory);
				}

				return fCurrentDepartment;
			}
		}

		#endregion

		#region Current User

		protected DocStaff fCurrentUser;
		public DocStaff CurrentUser
		{
			get
			{
				if (fCurrentUser == null)
				{
					fCurrentUser = DocStaff.New(GlbStaff.CurrentUser, Factory);
				}

				return fCurrentUser;
			}
		}

		#endregion

		public ZBool IsExportDocument
		{
			get { return IsExportDocumentCore; }
		}

		protected virtual ZBool IsExportDocumentCore
		{
			get { return DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP); }
		}

		public ZBool IsImportDocument
		{
			get { return IsImportDocumentCore; }
		}

		protected virtual ZBool IsImportDocumentCore
		{
			get { return DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV); }
		}

		public ZBool IsAnyDocument
		{
			get { return DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ANY); }
		}

		#endregion

		#region GetNotes Utilities

		#region GetNotes

		protected ZString GetNotes(ZString description, BusinessObject bizO)
		{
			ZString result = ZString.Empty;
			if (bizO != null)
			{
				result = GetNotes(description, bizO.GetNotes());
			}
			return result;
		}

		protected ZString GetNotes(ZString description, Notes bizONotes)
		{
			return GetNotes(description, ZString.Empty, bizONotes);
		}

		protected ZString GetNotes(ZString description, ZString context, Notes bizONotes)
		{
			return GetNotes(description, context, bizONotes, ZString.Empty);
		}

		protected ZString GetNotes(ZString description, ZString context, Notes bizONotes, ZString direction)
		{
			ZString contextFreightMode = GetStmNoteFreightContextCodeByTransportOrContainerMode(context);
			ZString result = "";

			if (bizONotes != null)
			{
				var notes = bizONotes.FindByDescription(description, true);
				foreach (StmNote note in notes)
				{
					if (direction.IsEmpty || note.ST_NoteContextDirection == direction || note.ST_NoteContextDirection == nameof(StmNoteContextDirection.A) || ((direction == "I" || direction == "E") && note.ST_NoteContextDirection == nameof(StmNoteContextDirection.B)))
					{
						string noteContextFreightMode = note.ST_NoteContext.Length < 3 ? string.Empty : note.ST_NoteContext.ToUpper().Substring(2, 1);
						if (contextFreightMode == nameof(StmNoteContextFreightMode.A) || note.ST_NoteContext.IsEmpty || note.ST_NoteContext == StmNoteContextUtils.StmNoteContextsAllToString || noteContextFreightMode == contextFreightMode.ToUpper() || noteContextFreightMode == nameof(StmNoteContextFreightMode.A))
						{
							if ((result.IsEmpty) || (!result.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, note.ST_NoteDataAsText)))
							{
								result += note.ST_NoteDataAsText;
								result += "\n";
							}
						}
					}
				}

				result = result.TrimEnd();
			}

			return result;
		}

		string GetStmNoteFreightContextCodeByTransportOrContainerMode(string context)
		{
			switch (context)
			{
				case Enterprise.Core.Constants.TransportModes.Air:
					return nameof(StmNoteContextFreightMode.I);
				case Enterprise.Core.Constants.TransportModes.SeaAir:
				case Enterprise.Core.Constants.TransportModes.AirSea:
					return nameof(StmNoteContextFreightMode.B);
				case Enterprise.Core.Constants.TransportModes.All:
					return nameof(StmNoteContextFreightMode.A);
				case Enterprise.Core.Constants.TransportModes.Rail:
					return nameof(StmNoteContextFreightMode.W);
				case Enterprise.Core.Constants.TransportModes.Road:
					return nameof(StmNoteContextFreightMode.R);
				case Enterprise.Core.Constants.TransportModes.Sea:
					return nameof(StmNoteContextFreightMode.S);
				case Enterprise.Core.Constants.ContainerModes.LCL:
					return nameof(StmNoteContextFreightMode.L);
				case Enterprise.Core.Constants.ContainerModes.FCL:
					return nameof(StmNoteContextFreightMode.F);
				case "":
					return nameof(StmNoteContextFreightMode.A);
				default:
					return nameof(StmNoteContextFreightMode.Undefined);  // by default if no match is made, return NO notes context, not ALL notes
			}
		}

		#endregion

		#region GetNotesInStringArray
		protected ZString[] GetNotesInStringArray(ZString description, BusinessObject bizo)
		{
			ZString[] result = null;
			if (bizo != null)
			{
				result = GetNotesInStringArray(description, bizo.GetNotes());
			}
			return result;
		}

		protected ZString[] GetNotesInStringArray(ZString description, Notes bizoNotes)
		{
			ZString noteText = GetNotes(description, bizoNotes);
			if (!noteText.IsEmpty)
			{
				ZString wrappedNoteText = WrapTextForAColumn(noteText, 120);
				return wrappedNoteText.Split('\n');
			}
			else
			{
				return System.Array.Empty<ZString>();
			}
		}
		#endregion

		#region GetAllNotes
		protected ZString GetAllNotes(BusinessObject bizo)
		{
			ZString result = ZString.Empty;
			if (bizo != null)
			{
				result = GetAllNotes(bizo.GetNotes());
			}
			return result;
		}

		protected ZString GetAllNotes(Notes bizoNotes)
		{
			ZString result = "";
			if (bizoNotes != null)
			{
				StmNoteCollection allNotes = (StmNoteCollection)bizoNotes.GetAllNotes();
				allNotes.Sort(new NoteSorter());
				foreach (StmNote note in allNotes)
				{
					result += note.ST_Description.ToUpper();
					result += "  (" + note.ST_NoteType_DescriptiveText + ")" + "\n";
					result += note.ST_NoteDataAsText.TrimEnd();
					result += "\n\n";
				}
			}
			result = result.Replace("\r", "");
			return result.Trim();
		}
		#endregion

		#region GetAllNotesInStringArray
		protected ZString[] GetAllNotesInStringArray(BusinessObject bizo)
		{
			ZString[] result = null;
			if (bizo != null)
			{
				result = GetAllNotesInStringArray(bizo.GetNotes());
			}
			return result;
		}

		protected ZString[] GetAllNotesInStringArray(Notes bizoNotes)
		{
			ZString result = GetAllNotes(bizoNotes);
			if (!result.IsEmpty)
			{
				ZString allNotesWrapped = WrapTextForAColumn(result, 120);
				return allNotesWrapped.Split('\n');
			}
			else
			{
				return System.Array.Empty<ZString>();
			}
		}
		#endregion

		#region Cartage Instructions Note

		protected ZString PickupOrDeliveryCartageInstructions()
		{
			return PickupOrDeliveryCartageInstructions(ZString.Empty);
		}

		protected ZString PickupOrDeliveryCartageInstructions(string contextFreightMode)
		{
			return PickupOrDeliveryCartageInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode);
		}

		protected ZString PickupOrDeliveryCartageInstructions(string direction, string contextFreightMode)
		{
			return PickupOrDeliveryCartageInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode, direction);
		}

		protected ZString PickupOrDeliveryCartageInstructions(Notes bizONotes)
		{
			return PickupOrDeliveryCartageInstructions(bizONotes, ZString.Empty);
		}

		protected ZString PickupOrDeliveryCartageInstructions(Notes bizONotes, ZString contextFreightMode)
		{
			return PickupOrDeliveryCartageInstructions(bizONotes, contextFreightMode, ZString.Empty);
		}

		protected ZString PickupOrDeliveryCartageInstructions(Notes bizONotes, ZString contextFreightMode, ZString direction)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (!IsImportDocument)
			{
				ZString noteText = Instructions(PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, bizONotes, contextFreightMode, direction);
				if (!noteText.IsEmpty)
				{
					result.Append(noteText);
				}
			}

			if (!IsExportDocument)
			{
				ZString noteText = Instructions(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, bizONotes, contextFreightMode, direction);
				if (!noteText.IsEmpty)
				{
					result.Append(noteText);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("\n");
		}

		#endregion

		#region Handling Instructions Note

		protected ZString PickupOrDeliveryHandlingInstructions()
		{
			return PickupOrDeliveryHandlingInstructions(ZString.Empty);
		}

		protected ZString PickupOrDeliveryHandlingInstructions(string contextFreightMode)
		{
			return PickupOrDeliveryHandlingInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode);
		}

		protected ZString PickupOrDeliveryHandlingInstructions(string direction, string contextFreightMode)
		{
			return PickupOrDeliveryHandlingInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode, direction);
		}

		protected ZString PickupOrDeliveryHandlingInstructions(Notes bizONotes)
		{
			return PickupOrDeliveryHandlingInstructions(bizONotes, ZString.Empty);
		}

		protected ZString PickupOrDeliveryHandlingInstructions(Notes bizONotes, ZString contextFreightMode)
		{
			return PickupOrDeliveryHandlingInstructions(bizONotes, contextFreightMode, ZString.Empty);
		}

		protected ZString PickupOrDeliveryHandlingInstructions(Notes bizONotes, ZString contextFreightMode, ZString direction)
		{
			return Instructions(PredefinedNoteTypes.Instance.HandlingInstructions.Description, bizONotes, contextFreightMode, direction);
		}

		#endregion

		#region Special Instructions Note

		protected ZString PickupOrDeliverySpecialInstructions()
		{
			return PickupOrDeliverySpecialInstructions(ZString.Empty);
		}

		protected ZString PickupOrDeliverySpecialInstructions(string contextFreightMode)
		{
			return PickupOrDeliverySpecialInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode);
		}

		protected ZString PickupOrDeliverySpecialInstructions(string direction, string contextFreightMode)
		{
			return PickupOrDeliverySpecialInstructions(((BusinessObject)WrappedObject).GetNotes(), contextFreightMode, direction);
		}

		protected ZString PickupOrDeliverySpecialInstructions(Notes bizONotes)
		{
			return PickupOrDeliverySpecialInstructions(bizONotes, ZString.Empty);
		}

		protected ZString PickupOrDeliverySpecialInstructions(Notes bizONotes, ZString contextFreightMode)
		{
			return PickupOrDeliverySpecialInstructions(bizONotes, contextFreightMode, ZString.Empty);
		}

		protected ZString PickupOrDeliverySpecialInstructions(Notes bizONotes, ZString contextFreightMode, ZString direction)
		{
			return Instructions(PredefinedNoteTypes.Instance.SpecialInstructions.Description, bizONotes, contextFreightMode, direction);
		}

		#endregion

		#region Instruction Notes

		protected ZString Instructions(ZString noteTypeDescription, Notes bizONotes, ZString contextFreightMode, ZString direction)
		{
			ZStringBuilder result = new ZStringBuilder();
			ZString noteText = GetNotes(noteTypeDescription, contextFreightMode, bizONotes, direction);
			if (!noteText.IsEmpty)
			{
				result.Append(noteText);
			}

			return result.ToStringWithDelimiterBetweenAppends("\n");
		}

		#endregion

		#endregion

		#region Weight Volume Display for Shipment and Consol Doc

		protected string GetWeightVolumeDisplayOption()
		{
			switch (ReportName.Trim().ToUpper())
			{
				#region Shipment Docs

				case PreAlert:
				case DocumentsAvailableNotice:
					return Env.Registry.PreAlertWeightAndVolumeDisplay;
				case ArrivalNotice:
					return Env.Registry.ArrivalNoticeWeightAndVolumeDisplay;
				case DeliveryOrder:
					return Env.Registry.DeliveryOrderWeightAndVolumeDisplay;
				case ShippingAdvice:
					return Env.Registry.ShippingAdviceWeightAndVolumeDisplay;
				case OutturnReport:
					return Env.Registry.OutturnReportWeightAndVolumeDisplay;
				case AgentsInstruction:
					return Env.Registry.AgentsInstructionNoticeWeightAndVolumeDisplay;
				case ShipperDepartureNotice:
					return Env.Registry.ShipperDepartureNoticeWeightAndVolumeDisplay;
				case BookingConfirmation:
					return Env.Registry.BookingConfirmationWeightAndVolumeDisplay;
				case ColoadMasterManifest:
					return Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay;
				case ShipmentCartageAdvice:
				case ShipmentCartageAdviceWithReceipt:
					if (IsExportDocument)
					{
						return Env.Registry.CartageAdviceExportWeightAndVolumeDisplay;
					}
					else
					{
						return Env.Registry.CartageAdviceImportWeightAndVolumeDisplay;
					}

				#endregion

				#region Consol Docs

				case AgentDepartureNotice:
					return Env.Registry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay;
				case LetterToOverseasAgent:
					return Env.Registry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay;
				case Manifest:
				case ManifestLandscape:
				case ManifestLandscapeDetailed:
				case SummaryManifest:
					if (IsExportDocument)
					{
						return Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay;
					}
					else
					{
						return Env.Registry.ConsolManifestConsolImportWeightAndVolumeDisplay;
					}

				case ForwardingInstruction:
					return Env.Registry.ConsolForwardingInstructionWeightAndVolumeDisplay;
				case CargoLoadList:
					return Env.Registry.ConsolCargoLoadListWeightAndVolumeDisplay;

				#endregion

				default:
					return Core.WeightAndVolumeDisplayTypes.Codes.Actual;
			}
		}

		#endregion

		#region Document Name Constants

		public const string PreAlert = "PRE-ALERT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ArrivalNotice = "ARRIVAL NOTICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string DocumentsAvailableNotice = "DOCUMENTS AVAILABLE NOTICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string DeliveryOrder = "DELIVERY ORDER";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string AgentsInstruction = "AGENTS INSTRUCTION";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ShippingAdvice = "SHIPPING ADVICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string OutturnReport = "OUTTURN REPORT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string BookingConfirmation = "BOOKING CONFIRMATION";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ShipmentCartageAdvice = "SHIPMENT CARTAGE ADVICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ShipmentCartageAdviceWithReceipt = "SHIPMENT CARTAGE ADVICE WITH RECEIPT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ColoadMasterManifest = "CO-LOAD MASTER MANIFEST";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string SummaryManifest = "SUMMARY MANIFEST";

		public const string Manifest = "MANIFEST";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ManifestLandscape = "MANIFEST (LANDSCAPE)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ManifestLandscapeDetailed = "MANIFEST DETAILED (LANDSCAPE)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ForwardingInstruction = "FORWARDING INSTRUCTION";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string CargoLoadList = "CARGO LOAD LIST";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string AgentDepartureNotice = "AGENT DEPARTURE NOTICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ShipperDepartureNotice = "SHIPPER DEPARTURE NOTICE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string LetterToOverseasAgent = "LETTER TO OVERSEAS AGENT";

		public const string TranshipmentLabel = "TRANSHIPMENT";
		public const string OnForwardingLabel = "ONFORWARDING";
		public const string ImportLabel = "IMPORT";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ConsolIMO = "CONSOL IMO";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string ShipmentIMO = "SHIPMENT IMO";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name Constants")]
		public const string CFSIMO = "CFS IMO";

		#endregion

		#region Misc Utility Fields

		public ZString WrapTextForAColumn(ZString value, ZInt columnWidth)
		{
			ZString result = ZString.Empty;
			ZInt count = 0;
			ZString currentWord = ZString.Empty;
			ZString columnText = ZString.Empty;
			ZString stringValue = value.Replace("\n", " \n ");
			stringValue = stringValue.Replace("\t", " \t ");

			foreach (ZString word in stringValue.Split(' '))
			{
				currentWord = word;

				if (currentWord == "\t")
				{
					currentWord = "   ";
				}

				if (currentWord == "\n")
				{
					result += columnText.Trim() + "\n";
					columnText = ZString.Empty;
				}
				else if (columnText.Length + currentWord.Length + 1 > columnWidth)
				{
					result += columnText.Trim() + "\n";
					columnText = currentWord + " ";
				}
				else
				{
					columnText += currentWord + " ";
				}
			}

			if (!columnText.IsEmpty)
			{
				result += columnText;
			}

			result = result.Trim('\n');
			return result.Trim();
		}

		#endregion

		#region IDocManagerPlaceholderBarcode Members

		/// <summary>
		/// 3-letter internal DocManager code. 
		/// </summary>
		protected ZString DocManagerCode
		{
			get
			{
				var support = GetParentBOForNoteStorageEDocsAndDocData() as IDocManagerSupport;
				return support != null ? support.DocManagerInfo.DocManagerCode : ZString.Empty;
			}
		}

		/// <summary>
		/// Returns a property that uniquely identifies this record in this table. e.g. Shipment number S00001001.
		/// Should be composed of only one field.
		/// </summary>
		protected virtual ZString DocManagerUniqueID
		{
			get { return ZString.Empty; }
		}

		TextBarcode DocManagerPlaceholderBarcode
		{
			get
			{
				if (fDocManagerPlaceholderBarcode == null)
				{
					BarcodeGenerator generator = new BarcodeGenerator();
					fDocManagerPlaceholderBarcode = generator.CreateDocumentBarcode(DocManagerCode, DocManagerUniqueID);
				}
				return fDocManagerPlaceholderBarcode;
			}
		}
#if DEBUG
		internal ZString BarcodePrerequisitesForTestingONLY
		{
			get { return "Code-[" + DocManagerCode + "]\r\nUniqueID-[" + DocManagerUniqueID + "]"; }
		}
#endif

		TextBarcode fDocManagerPlaceholderBarcode;

		/// <summary>
		/// Barcode to be used on the COVERSHEET template only. 
		/// This property should be used in conjunction with 
		/// BarcodeTextPlaceholder to produce a barcode 
		/// </summary>
		public ZString BarcodeTextForFontPlaceholder
		{
			get
			{
				TextBarcode barcode = DocManagerPlaceholderBarcode;
				return (barcode != null) ? barcode.TextAs128sFontString : ZString.Empty;
			}
		}

		/// <summary>
		/// Barcode to be used on the COVERSHEET template only. 
		/// This property should be used in conjunction with 
		/// BarcodeTextForFontPlaceholder to produce a barcode 
		/// </summary>
		public ZString BarcodeTextPlaceholder
		{
			get
			{
				TextBarcode barcode = DocManagerPlaceholderBarcode;
				return (barcode != null) ? barcode.TextToEncode : ZString.Empty;
			}
		}

		#endregion

		#region IDocManagerBarcode Members

		/// <summary>
		/// Object that provides the strings for 128s font encoding. 
		/// Override and call the appropriate function in Enterprise.Barcode.Business.BarcodeGenerator
		/// to generate this barcode in any subclasses.
		/// </summary>
		protected virtual TextBarcode DocManagerBarcode
		{
			get { return new TextBarcode(ZString.Empty); }
		}

		public virtual ZString BarcodeTextForFont
		{
			get
			{
				TextBarcode barcode = DocManagerBarcode;
				return (barcode != null) ? barcode.TextAs128sFontString : ZString.Empty;
			}
		}

		public virtual ZString BarcodeText
		{
			get
			{
				TextBarcode barcode = DocManagerBarcode;
				return (barcode != null) ? barcode.TextToEncode : ZString.Empty;
			}
		}

		#endregion

		#region IDocManagerBarcode Members with Unique ID

		protected virtual TextBarcode DocManagerBarCodeWithUniqueID
		{
			get { return new TextBarcode(ZString.Empty); }
		}

		public ZString BarcodeTextForFontWithUniqueID
		{
			get
			{
				var barcode = DocManagerBarCodeWithUniqueID;
				return (barcode != null) ? barcode.TextAs128sFontString : ZString.Empty;
			}
		}

		public ZString BarcodeTextWithUniqueID
		{
			get
			{
				var barcode = DocManagerBarCodeWithUniqueID;
				return (barcode != null) ? barcode.TextToEncode : ZString.Empty;
			}
		}

		#endregion

		#region Customised Client Logo
		public Image CustomisedLogo
		{
			get { return ImageProvider.GetInstance().Logo; }
		}
		#endregion

		#region Air and Sea watermarks

		public Image AirWatermark
		{
			get
			{
				if (fAirWatermarkImage == null || fAirWatermarkImage.IsDisposed())
				{
					Stream imageStream = typeof(DocBaseWrapper).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.AirWatermark.png");
					fAirWatermarkImage = Image.FromStream(imageStream);
				}

				return fAirWatermarkImage;
			}
		}

		public Image SeaWatermark
		{
			get
			{
				if (fSeaWatermarkImage == null || fSeaWatermarkImage.IsDisposed())
				{
					Stream imageStream = typeof(DocBaseWrapper).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.SeaWatermark.png");
					fSeaWatermarkImage = Image.FromStream(imageStream);
				}

				return fSeaWatermarkImage;
			}
		}

		Image fAirWatermarkImage;
		Image fSeaWatermarkImage;

		#endregion

		#region GetDocDataValue Implementation
		protected override ZString GetDocDataValueOnly(ZString docDataIdentifier)
		{
			if (fDocDataManager == null)
			{
				fDocDataManager = new DocDataManager(GetParentBOForNoteStorageEDocsAndDocData());
			}
			return fDocDataManager.GetValue(docDataIdentifier);
		}
		DocDataManager fDocDataManager;

		protected virtual BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return WrappedObject as BusinessObject;
		}

		#endregion

		#region GetServiceDirection

		protected ZString GetServiceDirection(ZString origin, ZString destination)
		{
			ZString result;

			if (origin.IsEmpty || destination.IsEmpty)
			{
				return ZString.Empty;
			}

			if (origin.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode &&
				destination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Domestic;
			}
			else if (origin.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Export;
			}
			else if (destination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Import;
			}
			else
			{
				result = OrgConstants.ServiceDirection.Code.CrossTrade;
			}
			return result;
		}

		#endregion
	}
}
