using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckIECCode()
	{
		base.CheckIECCode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.IECCodeInfo, "IEC for Selected Supplier/Exporter");
	}

	protected override void CheckBranchSerialNumber()
	{
		base.CheckBranchSerialNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BranchSerialNumberInfo, "BSN – Branch Serial Number for Selected Supplier/Exporter");
	}

	protected override void CheckJE_ExporterType()
	{
		base.CheckJE_ExporterType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ExporterTypeInfo);
	}

	protected override void CheckJE_ExaminationDate()
	{
		base.CheckJE_ExaminationDate();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExaminationDateInfo);
		}
	}

	protected override void CheckJE_ExaminingOfficerDesignation()
	{
		base.CheckJE_ExaminingOfficerDesignation();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExaminingOfficerDesignationInfo);
		}
	}

	protected override void CheckJE_ExaminingOfficerName()
	{
		base.CheckJE_ExaminingOfficerName();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExaminingOfficerNameInfo);
		}
	}

	protected override void CheckJE_SealBy()
	{
		var parent = Parent;
		if (parent.JE_SealBy == SealByCodeList.Codes.S &&
			!(parent.JE_ContainerMode == Common.IN.INContainerModeList.Codes.BreakBulk) &&
			!parent.IsContainerised)
		{
			parent.JE_SealByInfo.AddWarning(Res.GetString("D23F8DA6-98D7-4655-AD96-F5E5FC0643CD", "When 'Seal By' is set to S – Self, then expected Container Mode is either 'CNT', 'BBK' or 'CPC'."));
		}
	}

	protected override void CheckJE_StuffingAt()
	{
		base.CheckJE_StuffingAt();
		var parent = Parent;
		if (parent.IsSea && parent.IsContainerised)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_StuffingAtInfo);
		}
	}

	protected override void CheckJE_SampleAccompanied()
	{
		base.CheckJE_SampleAccompanied();
		var parent = Parent;
		if (parent.IsSea && parent.IsContainerised && IsFactoryStuffed)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_SampleAccompaniedInfo);
		}
	}

	protected override void CheckJE_SupervisingOfficerDesignation()
	{
		base.CheckJE_SupervisingOfficerDesignation();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SupervisingOfficerDesignationInfo);
		}
	}

	protected override void CheckJE_SupervisingOfficerName()
	{
		base.CheckJE_SupervisingOfficerName();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SupervisingOfficerNameInfo);
		}
	}

	protected override void CheckJE_RotationNumber()
	{
		base.CheckJE_RotationNumber();
		var parent = Parent;
		if (!parent.JE_RotationNumber.IsNumbersOnlyOrEmpty)
		{
			var rotationNumberinfo = parent.JE_RotationNumberInfo;
			rotationNumberinfo.AddMessageError(Res.GetString("C6E4B619-C6E8-41EE-A22F-7743844FA463", "You have entered an invalid Rotation Number. It must be any number between 0 to 9999999"));
		}
	}

	protected override void CheckJE_CustomsLoadPort()
	{
		base.CheckJE_CustomsLoadPort();
		var parent = Parent;
		if (parent.IsAir || parent.IsSea)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_CustomsLoadPortInfo, Res.GetString("99236972-C945-411F-85C5-82EC5AEBBFE5", "Customs Location [Loading Facility]"));
		}
	}

	protected override void CheckJE_Commissionerate()
	{
		base.CheckJE_Commissionerate();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CommissionerateInfo);
		}
	}

	protected override void CheckJE_Division()
	{
		base.CheckJE_Division();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DivisionInfo);
		}
	}

	protected override void CheckJE_Range()
	{
		base.CheckJE_Range();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RangeInfo);
		}
	}

	protected override void CheckJE_SealNo()
	{
		base.CheckJE_SealNo();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SealNoInfo);
		}
	}

	protected override void CheckJE_Verified()
	{
		base.CheckJE_Verified();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VerifiedInfo);
		}
	}

	protected override void CheckJE_SampleForwarded()
	{
		base.CheckJE_SampleForwarded();
		if (IsFactoryStuffed)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SampleForwardedInfo);
		}
	}

	ZBool IsFactoryStuffed => Parent.IsFactoryStuffed && Parent.IsSeaAndContainerised;
}
