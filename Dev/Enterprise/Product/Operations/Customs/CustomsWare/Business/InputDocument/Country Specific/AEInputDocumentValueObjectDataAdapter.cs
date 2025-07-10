using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class AEInputDocumentValueObjectDataAdapter : InputDocumentValueObjectDataAdapter, ICustomsWareAEInputDocumentValueObjectDataAdapter
	{
		protected override ZString GetDeclarationType(XSD.DeclarationHeader xsdDeclarationHeader)
		{
			if (xsdDeclarationHeader.DeclarationType == "AEMIRSAL2")
			{
				switch (xsdDeclarationHeader.DeclarationInfo.DeclarationRegime)
				{
					case 1:
					case 4:
						return "IMP";
					case 2:
						return "EXP";
					case 3:
					case 5:
						return "TRA";
				}
			}

			return "";
		}

		protected override void CreateParty(XSD.Party party, ZString partyType, OrgHeader org, IDocAddress address)
		{
			base.CreateParty(party, partyType, org, address);

			if (org != null)
			{
				var ccd = org.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode && x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedArabEmirates);
				if (ccd != null)
				{
					CreateReference(party.Reference.AddNew(), OrgCusCode.CodeTypes.CustomsClientCode, ccd.OK_CustomsRegNo);
				}
			}
		}
	}
}
