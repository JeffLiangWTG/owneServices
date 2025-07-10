using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DataTransfer.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class EUInputDocumentValueObjectDataAdapter : InputDocumentValueObjectDataAdapter, ICustomsWareEUInputDocumentValueObjectDataAdapter
	{
		#region Export

		protected override void PopulateConsignmentHeader(Customs.Business.BaseJobDeclaration dec, XSD.ConsignmentHeader consHeader, IValueObjectExportContext context)
		{
			base.PopulateConsignmentHeader(dec, consHeader, context);

			var euDec = dec as JobDeclaration;
			if (euDec != null)
			{
				if (euDec.JE_TransportModeInland != "" || euDec.JE_RN_NKTransportNationality != "")
				{
					var transport = consHeader.Transport.AddNew();
					CreateTransport(transport, XSD.TransportTransportType.Inland, new TransportModeTranslator().TranslateToWCOCode(euDec.JE_TransportModeInland, true), "", euDec.JE_RN_NKTransportNationality);
				}
			}
		}

		protected override void CreateParty(XSD.Party party, ZString partyType, OrgHeader org, IDocAddress address)
		{
			base.CreateParty(party, partyType, org, address);

			if (org != null)
			{
				var eori = org.GetEuIdentificationNumber();
				if (eori != "")
				{
					CreateReference(party.Reference.AddNew(), "EORI", eori);
				}

				var vat = org.GetEUVATCodeOfThisOrg();
				if (vat != "")
				{
					CreateReference(party.Reference.AddNew(), "VAT", vat);
				}

				var btw = org.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW);
				if (btw != "")
				{
					CreateReference(party.Reference.AddNew(), OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, btw);
				}
			}
		}

		#endregion

		#region Import

		protected override void ImportEntry(Customs.Business.BaseJobDeclaration dec, Customs.Business.CusEntryHeader entry, XSD.Declaration xsddec, IValueObjectImportContext context)
		{
			base.ImportEntry(dec, entry, xsddec, context);

			var euEntry = entry as CusEntryHeader;

			if (euEntry != null)
			{
				foreach (XSD.Reference xsdreference in xsddec.DeclarationHeader.Reference)
				{
					if (xsdreference.RefCode == "UCR")
					{
						euEntry.LoadOrCreateUCRNumber(xsdreference.RefText);
					}
				}
			}
		}

		protected override void ImportDeclaration(Customs.Business.BaseJobDeclaration dec, XSD.Declaration xsdDec)
		{
			base.ImportDeclaration(dec, xsdDec);
			var declaration = dec as JobDeclaration;
			if (declaration != null)
			{
				var cei = declaration.CustomsEntryInstructions.FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_EntryStyle = xsdDec.DeclarationHeader.DeclarationInfo.DeclarationType.Left(declaration.JE_EntryStyleInfo.MaxLength);
				cei.CEI_SubStyle = xsdDec.DeclarationHeader.DeclarationInfo.DeclarationSubType.Left(1);
			}
		}
		#endregion
	}
}
