using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NctsDepartureHeaderProvider : NctsHeaderSharedDataProvider
	{
		public NctsDepartureHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageRecipient => string.Format(System.Globalization.CultureInfo.InvariantCulture, "NTA.{0}", nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry);

		public override string CustomsOfficeOfDestination => nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture : nctsHeader.DestinationCustomsOfficeCodeForDeparture;

		public string LRN => nctsHeader.MovementHeader.BM_PaperlessInbondNum;

		public virtual string HolderOfTheTransitProcedureIdentificationNumber
		{
			get
			{
				var identificationNumber = new PartyProvider(nctsHeader.Principal, IsInPhase5TransitionPeriod).IdentificationNumber;
				if (string.IsNullOrEmpty(identificationNumber))
				{
					identificationNumber = null;
				}
				return identificationNumber;
			}
		}

		public string HolderOfTheTransitProcedureTIRNumber => IsTIRInbondEntryType ? holderOfTheTransitProcedureOrgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers).FirstOrDefault()?.OK_CustomsRegNo : null;

		protected ZBool IsTIRInbondEntryType => nctsHeader.MovementHeader.IsTIRDeclaration;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure
		{
			get
			{
				if (holderOfTheTransitProcedure == null && nctsHeader.Principal != null)
				{
					holderOfTheTransitProcedure = new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, true, IsInPhase5TransitionPeriod);

					var holderOfTheTransitProcedureProvider = (HolderOfTheTransitProcedureProvider)holderOfTheTransitProcedure;

					if (holderOfTheTransitProcedureProvider != null && holderOfTheTransitProcedureProvider.IsEmpty)
					{
						holderOfTheTransitProcedure = null;
					}
				}
				return holderOfTheTransitProcedure;
			}
		}

		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		OrgHeader holderOfTheTransitProcedureOrgHeader => _holderOfTheTransitProcedureOrgHeader ?? (_holderOfTheTransitProcedureOrgHeader = nctsHeader.Principal?.Organisation);
		OrgHeader _holderOfTheTransitProcedureOrgHeader;

		public INCTSRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => RepresentativeProvider.New(nctsHeader.MovementHeader.Representative, nctsHeader.IsInPhase5TransitionPeriod));
		CachedValue<INCTSRepresentative> representative;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => nctsHeader.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;
	}
}
