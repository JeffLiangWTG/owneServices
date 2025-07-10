using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonConsignorWrapper : PartyNameWrapper, INCTSCommonConsignor
	{
		public new static NCTS5CommonConsignorWrapper New(JobDocAddress jobDocAddress) => New(jobDocAddress, isInPhase5TransitionPeriod: false);

		public static NCTS5CommonConsignorWrapper New(JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod) => jobDocAddress?.Address?.Header == null ? null : new NCTS5CommonConsignorWrapper(jobDocAddress, isInPhase5TransitionPeriod);

		NCTS5CommonConsignorWrapper(JobDocAddress docAd, bool isInPhase5TransitionPeriod) : base(docAd?.Address?.Header)
		{
			docAddress = docAd;
			addressOverride = docAd.E2_AddressOverride;
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}
		readonly JobDocAddress docAddress;
		readonly ZBool addressOverride;
		readonly bool isInPhase5TransitionPeriod;

		protected override ZString NameCore => addressOverride ? docAddress.E2_CompanyName : Id.IsEmpty ? base.NameCore : ZString.Empty;

		public INCTSCommonAddressInfo Address => address ?? (address = Id.IsEmpty
																		|| (orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual
																			&& Id == OrgHeaderExtension.GetPASCode(orgHeader))
																		? NCTS5CommonAddressInfoWrapper.New(docAddress, isInPhase5TransitionPeriod: isInPhase5TransitionPeriod)
																		: null);
		NCTS5CommonAddressInfoWrapper address;

		protected override ZString IdCore => addressOverride ? ZString.Empty : base.IdCore;
		public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = PartyContactWrapper.New(docAddress, addressOverride));
		PartyContactWrapper contactPerson;
	}
}
