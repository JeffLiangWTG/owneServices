using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class ExportJobDeclarationValidation : CommonJobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();

		ValidateGoodsLocationDescription();
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		var locationOfGoodsInfo = Parent.JE_LocationOfGoodsInfo;
		var entryInstructions = Parent.CustomsEntryInstructions;
		if (entryInstructions.AnyEntryInstructionIsAtPlace())
		{
			MandatoryValidation.MessageErrorIfNotEntered(locationOfGoodsInfo);
		}
		else if (entryInstructions.AnyEntryInstructionIsAtCustoms())
		{
			MandatoryValidation.MessageErrorIfIsEntered(locationOfGoodsInfo);
		}
	}

	protected override void CheckJE_RL_NKPortOfLoading()
	{
		base.CheckJE_RL_NKPortOfLoading();
		CheckBarrierPort(Parent.JE_RL_NKPortOfLoadingInfo);
	}

	protected override void CheckJE_RL_NKOrigin()
	{
		base.CheckJE_RL_NKOrigin();

		if (!Parent.AreDeclarationAndSupplierCountryCodesCompatible())
		{
			Parent.JE_RL_NKOriginInfo.AddWarning(ValidationCaptions.JobDeclaration.CannotHaveDifferentSuppliers);
		}
	}

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();
		CheckJE_TransportModeInlandWithExitAndCustomsOffice(Parent);
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		ApplyMandatoryValidationToJE_RN_NKTransportNationalityIfApplicable();
	}

	protected override void CheckJE_LocationQualifier()
	{
		base.CheckJE_LocationQualifier();
		if (!Parent.ZG_AuthorisationNumber.IsEmpty && !Parent.JE_LocationQualifier.IsEmpty)
		{
			Parent.JE_LocationQualifierInfo.AddMessageError(ValidationCaptions.JobDeclaration.LocationQualifierMustBeEmpty);
		}
	}

	protected virtual void ApplyMandatoryValidationToJE_RN_NKTransportNationalityIfApplicable()
	{
		var transportMode = Parent.JE_TransportMode;
		if (transportMode != TransportTypeList.Codes.Rail && transportMode != TransportTypeList.Codes.Mail && transportMode != TransportTypeList.Codes.FixedTransportInstallations)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}

	#region Implementation

	protected bool HasOfficesOfType(string officeType)
	{
		return Parent.CustomsOffices
			.Cast<OfficeCode>()
			.Any(x => x.CY_Code == officeType && !x.CY_Data.IsEmpty);
	}

	void CheckJE_TransportModeInlandWithExitAndCustomsOffice(JobDeclaration parent)
	{
		if (!parent.IsUCC6 && parent.OfficeOfExit != parent.JE_CustomsOffice)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportModeInlandInfo);
		}
	}

	#endregion
}
