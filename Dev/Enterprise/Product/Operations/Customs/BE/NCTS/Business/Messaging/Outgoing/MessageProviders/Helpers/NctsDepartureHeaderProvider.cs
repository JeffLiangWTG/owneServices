using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NctsDepartureHeaderProvider : NctsHeaderSharedDataProvider
	{
		public NctsDepartureHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string CustomsOfficeOfDestination => nctsHeader.MovementHeader.DestinationCustomsOfficeCodeForDeparture;

		public virtual string LRN => nctsHeader.MovementHeader.BM_PaperlessInbondNum;

		public virtual string HolderOfTheTransitProcedureIdentificationNumber => holderOfTheTransitProcedureOrgHeader?.GetIdentificationNumber();

		public string HolderOfTheTransitProcedureTIRNumber => IsTIRInbondEntryType ? holderOfTheTransitProcedureOrgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers).FirstOrDefault()?.OK_CustomsRegNo : null;

		protected ZBool IsTIRInbondEntryType => nctsHeader.MovementHeader.IsTIRDeclaration;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = nctsHeader.Principal != null ? new HolderOfTheTransitProcedureProvider(nctsHeader.Principal) : null);
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		OrgHeader holderOfTheTransitProcedureOrgHeader => _holderOfTheTransitProcedureOrgHeader ?? (_holderOfTheTransitProcedureOrgHeader = nctsHeader.Principal?.Organisation);
		OrgHeader _holderOfTheTransitProcedureOrgHeader;

		public INCTSRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => RepresentativeProvider.New(nctsHeader.MovementHeader.Representative));
		CachedValue<INCTSRepresentative> representative;
	}
}
