using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderValidation
{
	public NctsHeaderValidation(NctsHeader parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateRepresentationType();
		ValidateDeclarantAddressPK();
		ValidateSubscriber();
		ValidateAuthorization();
	}

	#region RepresentationType

	public void ValidateRepresentationType()
	{
		ValidateCalculatedProperty(Parent.RepresentationTypeInfo);
	}

	protected virtual void CheckRepresentationType()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.RepresentationTypeInfo);
	}

	#endregion

	#region DeclarantAddressPK

	public void ValidateDeclarantAddressPK()
	{
		ValidateCalculatedProperty(Parent.DeclarantAddressPKInfo);
	}

	protected void CheckDeclarantAddressPK()
	{
		TypeValidation.CheckValidGuid(Parent.DeclarantAddressPKInfo);
		if (Parent.IsDeclarantAddressPKRequired && Parent.DeclarantAddressPK.IsEmpty)
		{
			Parent.DeclarantAddressPKInfo.AddMessageError(ValidationCaptions.NctsHeader.ForTheSelectedRepTypeThisFieldIsMandatory);
		}
	}

	#endregion

	#region BH_CustomsProfile

	protected override void CheckBH_CustomsProfile()
	{
		base.CheckBH_CustomsProfile();
		var customsProfileInfo = Parent.BH_CustomsProfileInfo;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(customsProfileInfo);

		if (!Parent.IsPhase5)
		{
			PanValidation.Validate(customsProfileInfo);
		}
	}

	#endregion

	#region Authorization

	public void ValidateAuthorization()
	{
		ValidateCalculatedProperty(Parent.AuthorizationInfo);
	}

	protected void CheckAuthorization()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.AuthorizationInfo);
	}

	#endregion

	#region Subscriber

	public void ValidateSubscriber()
	{
		ValidateCalculatedProperty(Parent.SubscriberInfo);
	}

	protected virtual void CheckSubscriber()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SubscriberInfo);
	}

	#endregion

	#region BH_RL_NKImportLoadPort

	protected override void CheckBH_RL_NKImportLoadPort()
	{
		base.CheckBH_RL_NKImportLoadPort();

		new ImportLoadPortValidator(Parent).CheckImportLoadPort();
	}

	#endregion

	public void ValidateConsignorMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage()
	{
		ValidateTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(Parent.Consignor, x => x.Consignor);
	}

	public void ValidateConsigneeMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage()
	{
		ValidateTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(Parent.Consignee, x => x.Consignee);
	}

	public new NctsHeader Parent => (NctsHeader)base.Parent;

	#region Implementation

	NodeProgressiveAnnualNumberValidation PanValidation => panValidation ?? (panValidation = new NodeProgressiveAnnualNumberValidation(new NctsHeaderCustomsMessageFountainProvider(Parent)));
	NodeProgressiveAnnualNumberValidation panValidation;

	void ValidateTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(JobDocAddress trader, Func<NctsDepartureCargoDesc, JobDocAddress> getTrader)
	{
		var parent = Parent;
		var movementHeader = Parent.MovementHeader;
		var isGroupage = movementHeader?.IsGroupage ?? false;

		if (isGroupage && !trader.IsEmpty && movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(x => !getTrader(x).IsEmpty))
		{
			trader.OrganisationPKInfo.AddMessageError(ValidationCaptions.NctsHeader.ConsignorAndConsigneeAtHeaderLevelMustBeEmpty);
		}
	}

	#endregion

	#region ImportLoadPortValidator

	class ImportLoadPortValidator
	{
		public ImportLoadPortValidator(NctsHeader header)
		{
			nctsHeader = header;
		}

		readonly NctsHeader nctsHeader;

		public void CheckImportLoadPort()
		{
			var movementHeader = nctsHeader.MovementHeader;
			if (movementHeader != null)
			{
				var messageError = CheckImportLoadPort(movementHeader, nctsHeader.BH_RL_NKImportLoadPort);
				AddMessageErrorIfNotEmpty(messageError);
			}
		}

		ZString CheckImportLoadPort(NctsDepartureMovementHeader movementHeader, ZString importLoadPort)
		{
			if (movementHeader.IsTIRDeclaration)
			{
				return importLoadPort.IsEmpty ? (ZString)ValidationCaptions.NctsHeader.CountryOfDispatchIsequired : ZString.Empty;
			}
			return CheckImportLoadPortForNonTirDeclaration(movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>(), importLoadPort);
		}

		ZString CheckImportLoadPortForNonTirDeclaration(IEnumerable<NctsDepartureCargoDesc> goodItems, ZString importLoadPort)
		{
			if (goodItems.Any(x => x.MoveHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem()))
			{
				return ZString.Empty;
			}
			if (IsCountryOfDispatchEmptyBothHeaderAndGoodItems())
			{
				return ValidationCaptions.NctsHeader.YouHaveNotEnteredADispatchCountry;
			}

			if (!IsCountryOfDispatchEqualBetweenHeaderAndGoodItems())
			{
				if (!IsCountryOfDispatchHeaderOrGoodItemsEmpty())
				{
					return ValidationCaptions.NctsHeader.CountryDeclarationDifferentDispatch;
				}
			}

			return ZString.Empty;

			bool IsCountryOfDispatchEmptyBothHeaderAndGoodItems() => importLoadPort.IsEmpty && goodItems.All(x => x.BY_RN_NKCountryOfDispatch.IsEmpty);
			bool IsCountryOfDispatchHeaderOrGoodItemsEmpty() => importLoadPort.IsEmpty || goodItems.All(x => x.BY_RN_NKCountryOfDispatch.IsEmpty);
			bool IsCountryOfDispatchEqualBetweenHeaderAndGoodItems() => goodItems.Any(x => x.BY_RN_NKCountryOfDispatch == importLoadPort);
		}

		void AddMessageErrorIfNotEmpty(ZString messageError)
		{
			if (!messageError.IsEmpty)
			{
				nctsHeader.BH_RL_NKImportLoadPortInfo.AddMessageError(messageError);
			}
		}
	}

	#endregion
}
