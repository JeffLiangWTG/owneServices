//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNRCanPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoNRCanPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class NRCanPGAHeaderAddInfoValidation : AutoNRCanPGAHeaderAddInfoValidation
	{
		public NRCanPGAHeaderAddInfoValidation(AutoNRCanPGAHeaderAddInfo parent) : base(parent)
		{
		}

		protected new AutoNRCanPGAHeaderAddInfo Parent
		{
			get { return base.Parent; }
		}

		protected NRCanPGAHeader NRCanHeader
		{
			get { return (NRCanPGAHeader)Parent.Parent; }
		}

		protected override void CheckCA_AuthorizedParty()
		{
			base.CheckCA_AuthorizedParty();

			if (NRCanHeader.CA_EXPProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_AuthorizedPartyInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_AuthorizedPartyInfo, Parent.Lookups.AuthorizedPartyTypeCodes);
		}

		protected override void CheckCA_CaratWeight()
		{
			base.CheckCA_CaratWeight();
			if (NRCanHeader.CA_RDAProgramInd == YesNoList.Codes.Yes && Parent.CA_CaratWeight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_CaratWeightInfo);
			}
		}

		protected override void CheckCA_PackUQ1()
		{
			base.CheckCA_PackUQ1();

			if (NRCanHeader.CA_RDAProgramInd == YesNoList.Codes.Yes && Parent.CA_PackQty1 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_PackUQ1Info);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PackUQ1Info, Parent.Lookups.CustomsUQList);
		}

		protected override void CheckCA_PackUQ2()
		{
			base.CheckCA_PackUQ2();

			if (NRCanHeader.CA_RDAProgramInd == YesNoList.Codes.Yes && Parent.CA_PackQty2 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_PackUQ2Info);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PackUQ2Info, Parent.Lookups.CustomsUQList);
		}

		protected override void CheckCA_PackUQ3()
		{
			base.CheckCA_PackUQ3();

			if (NRCanHeader.CA_RDAProgramInd == YesNoList.Codes.Yes && Parent.CA_PackQty3 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_PackUQ3Info);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PackUQ3Info, Parent.Lookups.CustomsUQList);
		}

		protected override void CheckCA_AuthorizedProductID()
		{
			base.CheckCA_AuthorizedProductID();

			if (Parent.CA_AuthorizedProductID.IsEmpty
				&& NRCanHeader.CA_EXPProgramInd == YesNoList.Codes.Yes
				&& NRCanHeader.InvoiceLine != null
				&& NRCanHeader.InvoiceLine.CA_TradeName.IsEmpty)
			{
				Parent.CA_AuthorizedProductIDInfo.AddMessageError(Res.GetString("847ac6b5-b635-4b5c-aa5d-753f9c500258", "If Trade Name is not provided, Authorized Product ID should not be empty."));
			}
		}

		protected override void CheckCA_IntendedUseCode()
		{
			base.CheckCA_IntendedUseCode();
			if (NRCanHeader.CA_EEFProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
				if (!NRCanHeader.CA_IsNotRegulatedByOEE)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_IntendedUseCodeInfo);
				}

				if (Parent.CA_IntendedUseCode == NRCanIntendedUseCodes.Codes.NR04)
				{
					Parent.CA_IntendedUseCodeInfo.AddMessageError(Res.GetString("280B7641-5CCA-4ED4-9FC1-1EC86F3E15CE", "NR04 is only allowed for Explosives Program."));
				}
			}
		}
	}
}
