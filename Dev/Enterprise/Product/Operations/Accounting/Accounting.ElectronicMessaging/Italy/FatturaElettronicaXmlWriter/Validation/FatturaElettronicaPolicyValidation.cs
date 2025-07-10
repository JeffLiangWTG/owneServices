using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class FatturaElettronicaPolicyValidation
	{
		public FatturaElettronicaPolicyValidation(INotifications errorNotifications, INotifications warningNotifications, IXPathNavigable document, TransactionBatch transactionBatch)
		{
			// Avoid CA1059 warning.
			Document = document as XmlDocument;
			if (Document == null)
			{
				throw new ArgumentException("Only support XmlDocument as 'document'.");
			}

			var transactionCount = transactionBatch?.TransactionCollection?.Count ?? 0;
			if (transactionCount != 1)
			{
				throw new ArgumentException("Transaction batch must have one and only one transaction info.");
			}
			Transaction = transactionBatch.TransactionCollection[0];

			ErrorsNotifications = errorNotifications;
			WarningsNotifications = warningNotifications;

			NameSpaceManager = new XmlNamespaceManager(Document.NameTable);
			NameSpaceManager.AddNamespace((NoResString)"ds", "http://www.w3.org/2000/09/xmldsig#");   // Developer only namespace info.
			NameSpaceManager.AddNamespace((NoResString)"p", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2");   // Developer only namespace info.
			NameSpaceManager.AddNamespace((NoResString)"xsi", "http://www.w3.org/2001/XMLSchema-instance");   // Developer only namespace info.
		}

		public void ValidateXml()
		{
			CheckFormatoTrasmissione();
			CheckCodiceDestinatario();
			CheckCompanyName();
			CheckPurchaseOrder();
			CheckLine();
			CheckNumero();
		}

		void CheckFormatoTrasmissione()
		{
			var formatoTrasmissione = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/DatiTrasmissione/FormatoTrasmissione", NameSpaceManager);
			var versione = Document.SelectSingleNode("/p:FatturaElettronica", NameSpaceManager).Attributes["versione"];

			if (versione?.InnerText != formatoTrasmissione?.InnerText)
			{
				ErrorsNotifications.AddError("FatturaElettronicaHeader/DatiTrasmissione/FormatoTrasmissione: " + Res.GetString("3BA538D6-69E2-4B71-81B9-A15867E7FD06", "<FormatoTrasmissione> must be equal to VERSIONE attribute in XML Main Root <FatturaElettronica>. Please raise an e-Request."));   // XML path.
			}
		}

		void CheckCodiceDestinatario()
		{
			var codiceDestinatario = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/DatiTrasmissione/CodiceDestinatario", NameSpaceManager);
			var pecDestinatario = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/DatiTrasmissione/PECDestinatario", NameSpaceManager);
			var formatoTrasmissione = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/DatiTrasmissione/FormatoTrasmissione", NameSpaceManager);

			if (codiceDestinatario?.InnerText != CodiceDestinatarioZeroCode && pecDestinatario != null)
			{
				ErrorsNotifications.AddError("FatturaElettronicaHeader/DatiTrasmissione: " + Res.GetString("DC55FFEF-E4EB-496D-B432-9E17B5630EF2", "<PECDestinario> PEC Email Address must be omitted when Account has an Italy CUU Registration Code."));   // XML path.
			}

			if (codiceDestinatario?.InnerText.Length != 6 && formatoTrasmissione.InnerText == FPA12Code)
			{
				ErrorsNotifications.AddError("FatturaElettronicaHeader/DatiTrasmissione: " + Res.GetString("B73C50AD-9FA1-433D-8916-C93FE6988685", "<CodiceDestinatario> CUU Registration Code must be 6 characters long when Account is a GOV Organization."));   // XML path.
			}

			if (codiceDestinatario.InnerText.Length != 7 && formatoTrasmissione.InnerText == FPR12Code)
			{
				ErrorsNotifications.AddError("FatturaElettronicaHeader/DatiTrasmissione: " + Res.GetString("268C0300-B112-4364-91AE-27047678D343", "<CodiceDestinatario> CUU Registration Code must be 7 characters long when Account is a non-GOV organization."));   // XML path.
			}
		}

		void CheckCompanyName()
		{
			var anagrafica1 = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica", NameSpaceManager);
			var anagrafica2 = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/CessionarioCommittente/DatiAnagrafici/Anagrafica", NameSpaceManager);
			var factory = new BusinessObjectFactory();
			var isSupplierNAT = Transaction?.BranchAddress?.IsOrganizationCategoryNAT(factory) == true;             // convert bool? to bool.
			var isRecipientNAT = Transaction?.OrganizationAddress?.IsOrganizationCategoryNAT(factory) == true;      // convert bool? to bool.

			ValidateCompanyName((NoResString)"FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica: ",   // XML path.
(NoResString)"FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica: ",   // XML path.
								anagrafica1, isSupplierNAT);

			ValidateCompanyName((NoResString)"FatturaElettronicaHeader/CessionarioCommittente/DatiAnagrafici/Anagrafica: ",   // XML path.
(NoResString)"FatturaElettronicaHeader/CessionarioCommittente/DatiAnagrafici/Anagrafica: ",   // XML path.
									anagrafica2, isRecipientNAT);
		}

		void CheckPurchaseOrder()
		{
			var formatoTrasmissione = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaHeader/DatiTrasmissione/FormatoTrasmissione", NameSpaceManager);

			if (formatoTrasmissione?.InnerText == FPA12Code)
			{
				var orderList = Document.SelectNodes("/p:FatturaElettronica/FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto", NameSpaceManager);
				int index = 1;

				var warningMsg = Res.GetString("F415D367-11E0-46E2-AC0C-83DED4FCB956", "<IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record.");
				var warningFullMsg = (NoResString)"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto " + warningMsg;// XML path and warning message.

				if (orderList.Count == 0)
				{
					WarningsNotifications.AddWarning(warningFullMsg);
				}
				else
				{
					foreach (XmlNode node in orderList)
					{
						var codiceCUP = node.SelectSingleNode("CodiceCUP");
						var codiceCIG = node.SelectSingleNode("CodiceCIG");

						if (codiceCUP == null || codiceCIG == null)
						{
							warningFullMsg = "FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto" + Res.GetString("84B4893B-853C-40EA-9A52-3B81DC979EB2", "[{0}]: ", index) + warningMsg;  // XML path and warning message.
							WarningsNotifications.AddWarning(warningFullMsg);
						}

						index++;
					}
				}
			}
		}

		void CheckLine()
		{
			ValidateLine("/p:FatturaElettronica/FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee", "FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee", false);
			ValidateLine("/p:FatturaElettronica/FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo", "FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo", true);
		}

		void CheckNumero()
		{
			var numero = Document.SelectSingleNode("/p:FatturaElettronica/FatturaElettronicaBody/DatiGenerali/DatiGeneraliDocumento/Numero", NameSpaceManager);

			if (!IsContainsDigit(numero?.InnerText))
			{
				ErrorsNotifications.AddError((NoResString)"FatturaElettronicaBody/DatiGenerali/DatiGeneraliDocumento/Numero: " + Res.GetString("C942DEC4-69AB-4258-AD40-D7C06BDBF4EC", "<Numero> Transaction Number must have at least one number."));   // XML path.
			}
		}

		#region Implementation

		void ValidateCompanyName(string individuleCompanyNameErrorMessage, string orgCompanyNameErrorMessage, XmlNode anagrafica, bool isNAT)
		{
			var nome = anagrafica.SelectSingleNode((NoResString)"Nome", NameSpaceManager);   // XML node name.
			var cognome = anagrafica.SelectSingleNode((NoResString)"Cognome", NameSpaceManager);   // XML node name.
			var denominazione = anagrafica.SelectSingleNode((NoResString)"Denominazione", NameSpaceManager);   // XML node name.

			if (isNAT && (nome == null || cognome == null || denominazione != null))
			{
				ErrorsNotifications.AddError(individuleCompanyNameErrorMessage + Res.GetString("61F5B4B2-F3FA-47DA-A666-D107F51F2079", "<Nome> and <Cognome> Organization Name must contain First Name and Last Name when Account is an Individual Person (NAT)."));
			}
			else if (!isNAT && (nome != null || cognome != null || denominazione == null))
			{
				ErrorsNotifications.AddError(orgCompanyNameErrorMessage + Res.GetString("06ED05CB-07CF-4BD3-AA84-113B6D2D940F", "<Denominazione> Organization Name must contain Business Name. Please check that Organization Category and Organization Name is correct."));
			}
		}

		void ValidateLine(string path, string displayPath, bool isSummary)
		{
			var lineList = Document.SelectNodes(path, NameSpaceManager);
			int index = 1;

			foreach (XmlNode node in lineList)
			{
				var aliquotaIVA = node.SelectSingleNode("AliquotaIVA", NameSpaceManager);   // XML node name.
				var natura = node.SelectSingleNode((NoResString)"Natura", NameSpaceManager);   // XML node name.
				var esigibilitaIVA = node.SelectSingleNode("EsigibilitaIVA", NameSpaceManager);   // XML node name.
				var riferimentoAmministrazione = node.SelectSingleNode("RiferimentoAmministrazione", NameSpaceManager);   // XML node name.

				if (aliquotaIVA?.InnerText == AliquotaIvaZeroValue && natura == null)
				{
					ErrorsNotifications.AddError(displayPath + Res.GetString("E03FBD1C-5E5A-4EC9-B9A2-598F32802322", "[{0}]: <Natura> Tax Message including a Tax Group Code ({1} N1-N7) must be selected when IVA is zero.", index, "Natura"));   // XML path.
				}

				if (isSummary && natura?.InnerText == NaturaN6 && esigibilitaIVA?.InnerText == EsigibilitaIvaS)
				{
					ErrorsNotifications.AddError(displayPath + Res.GetString("1CD888CD-CB15-4974-BD32-4AC826978E5C", "[{0}]: <EsigibilitaIVA> must not be '{1}' (split payment) when <Natura> equals to '{2}'. Please raise an e-Request.", index, EsigibilitaIvaS, NaturaN6));   // XML path.
				}

				if (!isSummary && (
						(natura != null && riferimentoAmministrazione == null) ||
						(natura == null && riferimentoAmministrazione != null)
					))
				{
					ErrorsNotifications.AddError(displayPath + Res.GetString("ECF76526-3ACE-41F3-90CC-7C83D17F1289", "[{0}]: <RiferimentoAmministrazione> Tax Message must have a Tax Group Code ({1} N1-N7).", index, "Natura"));   // XML path.
				}

				index++;
			}
		}

		bool IsContainsDigit(string source)
		{
			var regex = new Regex(@"\d+");
			return source != null && regex.IsMatch(source);
		}

		readonly XmlDocument Document;
		readonly TransactionInfo Transaction;
		readonly XmlNamespaceManager NameSpaceManager;
		readonly INotifications ErrorsNotifications;
		readonly INotifications WarningsNotifications;

		const string CodiceDestinatarioZeroCode = "0000000";
		const string FPA12Code = "FPA12";
		const string FPR12Code = "FPR12";
		const string AliquotaIvaZeroValue = "0.00";
		const string NaturaN6 = "N6";
		const string EsigibilitaIvaS = "S";

		#endregion
	}
}
