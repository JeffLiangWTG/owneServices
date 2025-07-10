using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class CommonJobDeclarationValidation : AutoITJobDeclarationValidation
{
	public CommonJobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	public new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public void ValidateMessageVersion()
	{
		ValidateCalculatedProperty(Parent.MessageVersionInfo);
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateMessageVersion();
	}

	protected void CheckMessageVersion()
	{
		if (Parent.IsMessageVersionApplicable)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MessageVersionInfo);
		}
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();

		if (!Parent.JE_CustomsOffice.IsEmpty && (Parent.JE_CustomsOffice.Length != 8 || !Parent.JE_CustomsOffice.IsLettersAndNumbersOnlyOrEmpty))
		{
			Parent.JE_CustomsOfficeInfo.AddError(ValidationCaptions.JobDeclaration.CustomsOfficeShouldBe8AlphanumericCharacters);
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo, (CodeDescriptionPairList)Parent.Lookups.Locations);
	}

	protected override void CheckJE_ContainerMode()
	{
		base.CheckJE_ContainerMode();
		if (!Parent.HasCusContainers && Parent.ContainersRequired)
		{
			Parent.JE_ContainerModeInfo.AddMessageError(ValidationCaptions.JobDeclaration.AtLeastOneContainerRecordMustBeEntered);
		}
	}

	protected override void AddMessageErrorIfDeclarantTypeNotEntered()
	{
		if (Parent.IsUCC6 && Parent.JE_OA_Representative.IsEmpty)
		{
			return;
		}

		base.AddMessageErrorIfDeclarantTypeNotEntered();
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();

		var declaration = Parent;

		if (declaration.IsUCC6)
		{
			var representative = declaration.JE_OA_Representative;
			var representativeInfo = declaration.JE_OA_RepresentativeInfo;

			if (!representative.IsEmpty && representative == declaration.JE_OA_DeclarantAddress)
			{
				representativeInfo.AddMessageError(ValidationCaptions.JobDeclaration.RepresentativeMustBeDifferentFromDeclarant);
			}

			if (!declaration.JE_DeclarantType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(representativeInfo);
			}
		}
	}

	protected override void CheckJE_DeclarantType()
	{
		base.CheckJE_DeclarantType();
		var relevantOrganizationPK = Parent.IsImport ? Parent.JE_OH_Importer : Parent.JE_OH_Supplier;
		var declarantPK = Parent.DeclarantAddress?.Header?.PK ?? ZGuid.Empty;
		if (Parent.JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._1Self && relevantOrganizationPK != declarantPK)
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(ValidationCaptions.JobDeclaration.NoCoherenceBetweenRepresentativeTypeAndDeclarant);
		}
	}

	protected override void ValidatePowerOfAttorney()
	{
		// Intentionally kept blank: Validation not required
	}

	protected override void CheckJE_CustomsProfile()
	{
		base.CheckJE_CustomsProfile();
		var customsProfileInfo = Parent.JE_CustomsProfileInfo;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(customsProfileInfo);
		CheckHasMauCertificateIfApplicable();
		if (!Parent.IsUCC6)
		{
			PanValidation.Validate(customsProfileInfo);
		}
	}

	protected override void CheckJE_GS_NKCusAgent()
	{
		if (!Parent.IsUCC6)
		{
			base.CheckJE_GS_NKCusAgent();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GS_NKCusAgentInfo);
		}
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();

		if (Parent.MeansOfTransportCrossingBorderIdentity.Length > CustomsFieldMaxLength.MeansOfTransportCrossingBorderIdentity)
		{
			string messageError;
			if (Parent.IsSea)
			{
				messageError = ValidationCaptions.JobDeclaration.VesselPlusVoyageExceedsCustomsMaxLengthExceesCaption(CustomsFieldMaxLength.MeansOfTransportCrossingBorderIdentity);
			}
			else
			{
				messageError = ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(CustomsFieldMaxLength.MeansOfTransportCrossingBorderIdentity);
			}

			Parent.JE_VesselNameInfo.AddMessageError(messageError);
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();

		var parent = Parent;
		var declarantAddress = parent.DeclarantAddress;
		if (declarantAddress != null)
		{
			var declarantAddressInfo = parent.JE_OA_DeclarantAddressInfo;
			new CustomsAddressValidator(declarantAddress, declarantAddressInfo.HumanReadableName, parent)
				.ValidateMaximumLengthCustomsFields(declarantAddressInfo);
		}
	}

	protected void CheckBarrierPort(ZPropertyInfo barrierPortPropertyInfo)
	{
		var portTaxGenerationEnabled = ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.Value;
		if (portTaxGenerationEnabled && Parent.IsSea && Parent.BarrierPortUNLOCO == null)
		{
			barrierPortPropertyInfo.AddNotification(GetPortNotificationType(), ValidationCaptions.JobDeclaration.ProvidePortCodeToDeterminePortTaxRate);
		}
	}

	protected override INotificationType GetPortNotificationType()
	{
		return CargoWise.EntityFramework.NotificationType.Warning;
	}

	protected override void CheckJE_ShipmentIncoTerm()
	{
		base.CheckJE_ShipmentIncoTerm();
		if (!CustomsRulesProvider.IncotermIsValidForIT(Parent.JE_ShipmentIncoTerm))
		{
			Parent.JE_ShipmentIncoTermInfo.AddMessageError(ValidationCaptions.Shared.IncotermInvalid);
		}
	}

	protected override void CheckJE_LocationQualifier()
	{
		base.CheckJE_LocationQualifier();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationQualifierInfo, Parent.Lookups.LocationQualifierList);
	}

	#region Implementation

	void CheckHasMauCertificateIfApplicable()
	{
		var customsProfile = Parent.JE_CustomsProfile;
		if (!customsProfile.IsEmpty && Parent.IsUCC6 && !CustomsCredentialHelper.NodePresentInCompanyAllowedListWithMAUCertificate(customsProfile))
		{
			Parent.JE_CustomsProfileInfo.AddMessageError(ValidationCaptions.Shared.SelectedNodeHasNoMAUCertificate);
		}
	}

	NodeProgressiveAnnualNumberValidation PanValidation => panValidation ?? (panValidation = new NodeProgressiveAnnualNumberValidation(new JobDeclarationFountainProvider(Parent)));
	NodeProgressiveAnnualNumberValidation panValidation;

	#endregion
}
