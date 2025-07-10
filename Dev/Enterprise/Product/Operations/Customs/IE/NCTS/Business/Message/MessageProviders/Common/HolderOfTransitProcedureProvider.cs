using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class HolderOfTransitProcedureProvider : PartyProvider, IHolder
	{
		public static HolderOfTransitProcedureProvider New(JobDocAddress docAddress, ZString declarationType)
		{
			HolderOfTransitProcedureProvider result = null;
			if (docAddress != null)
			{
				result = docAddress.E2_AddressOverride
					? new HolderOfTransitProcedureProvider(docAddress, docAddress.E2_GovRegNum, new IE.Business.ContactProvider(docAddress.E2_Contact, docAddress.E2_Phone, docAddress.E2_Email), string.Empty)
					: New(docAddress.Address, declarationType);
			}
			return result;
		}

		internal static HolderOfTransitProcedureProvider New(OrgAddress orgAddress, ZString declarationType)
		{
			HolderOfTransitProcedureProvider result = null;
			if (orgAddress != null)
			{
				var header = orgAddress.Header;
				result = new HolderOfTransitProcedureProvider(orgAddress, GetRegNo(orgAddress), IE.Business.ContactProvider.New(GetContact(header)), GetHolderId(header, declarationType));
			}
			return result;
		}

		static string GetHolderId(OrgHeader header, ZString declarationType)
		{
			string result = null;
			if (declarationType.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.TIR))
			{
				var customsCodes = header.CustomsCodes.Find((OrgCusCode x) => x.OK_CodeType == NctsPhase5DeclarationTypeList.Codes.TIR);
				if (customsCodes.Any())
				{
					result = customsCodes.First().OK_CustomsRegNo;
				}
			}
			return result;
		}

		HolderOfTransitProcedureProvider(IDocAddress docAddress, string id, IContact contact, string holderId)
			: base(docAddress, id, contact)
		{
			this.HolderId = holderId;
		}

		public string HolderId { get; private set; }
	}
}
