using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5PartyNameProviderWithAddressWrapper : PartyNameWrapper, INCTSPartyNameProviderWithAddress
	{
		public static NCTS5PartyNameProviderWithAddressWrapper New(JobDocAddress jobDocAddress, bool shouldNotTrimPostCode = false, bool isInPhase5TransitionPeriod = false) => jobDocAddress?.Address?.Header == null ? null : new NCTS5PartyNameProviderWithAddressWrapper(jobDocAddress, shouldNotTrimPostCode, isInPhase5TransitionPeriod);

		NCTS5PartyNameProviderWithAddressWrapper(JobDocAddress docAd, ZBool notTrimPostCode, bool isInPhase5TransitionPeriod) : base(docAd?.Address?.Header)
		{
			docAddress = docAd;
			addressOverride = docAd.E2_AddressOverride;
			shouldNotTrimPostCode = notTrimPostCode;
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}
		readonly JobDocAddress docAddress;
		readonly ZBool addressOverride;
		readonly ZBool shouldNotTrimPostCode;
		readonly bool isInPhase5TransitionPeriod;

		protected override ZString NameCore => addressOverride ? docAddress.E2_CompanyName : Id.IsEmpty ? base.NameCore : ZString.Empty;

		public INCTSCommonAddressInfo Address => address ?? (address = Id.IsEmpty
																		|| (orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual
																			&& Id == OrgHeaderExtension.GetPASCode(orgHeader))
																		? NCTS5CommonAddressInfoWrapper.New(docAddress, shouldNotTrimPostCode, isInPhase5TransitionPeriod)
																		: null);
		NCTS5CommonAddressInfoWrapper address;

		protected override ZString IdCore => addressOverride ? ZString.Empty : base.IdCore;
	}
}
