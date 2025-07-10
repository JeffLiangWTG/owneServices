using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class ExportEntryMessageSendingAction : BEJobDeclarationMessageSendingObject, IObsoleteValidation
{
	public ExportEntryMessageSendingAction(Declaration.CusEntryHeader entry) : base(entry)
	{
	}

	public override ZString TypeOfEntry
	{
		get => typeOfEntry;
		set
		{
			var oldValue = typeOfEntry;
			CheckMaximumLength(TypeOfEntryInfo, value);
			SetNonPersistentPropertyValue(TypeOfEntryInfo, ref typeOfEntry, value);

			if (oldValue == BEExportEntryTypeList.Codes.CancellationRequest && oldValue != value)
			{
				Justification = ZString.Empty;
			}
			else if (value == BEExportEntryTypeList.Codes.CancellationRequest)
			{
				AlternativeEvidences.RemoveAll();
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateTypeOfEntry();
			}
		}
	}
	ZString typeOfEntry;

	public ZString ExportCustomsOffice => Header.Declaration?.JE_CustomsOffice ?? ZString.Empty;

	public ZPropertyInfo ExportCustomsOfficeInfo => GetZPropertyInfo(nameof(ExportCustomsOffice));

	[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.ExitTypeList))]
	[ReadOnlyMember(nameof(ExitTypeReadOnly))]
	public ZString ExitType
	{
		get => exitType;
		set
		{
			CheckMaximumLength(ExitTypeInfo, value);
			SetNonPersistentPropertyValue(ExitTypeInfo, ref exitType, value);
		}
	}

	ZString exitType;

	bool ExitTypeReadOnly => true;

	public ZPropertyInfo ExitTypeInfo => GetZPropertyInfo(nameof(ExitType));

	[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.ExitCustomsOfficeList))]
	[ReadOnlyMember(nameof(ExitCustomsOfficeReadOnly))]
	[MaxLength(10)]
	public ZString ExitCustomsOffice
	{
		get => exitCustomsOffice;
		set
		{
			CheckMaximumLength(ExitCustomsOfficeInfo, value);
			SetNonPersistentPropertyValue(ExitCustomsOfficeInfo, ref exitCustomsOffice, value);
		}
	}
	ZString exitCustomsOffice;

	bool ExitCustomsOfficeReadOnly => false;

	public ZPropertyInfo ExitCustomsOfficeInfo => GetZPropertyInfo(nameof(ExitCustomsOffice));

	[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.SecurityTypeList))]
	[ReadOnlyMember(nameof(SecurityTypeReadOnly))]
	[MaxLength(1)]
	public ZString SecurityType
	{
		get => securityType;
		set
		{
			CheckMaximumLength(SecurityTypeInfo, value);
			SetNonPersistentPropertyValue(SecurityTypeInfo, ref securityType, value);
		}
	}
	ZString securityType;

	bool SecurityTypeReadOnly => false;

	public ZPropertyInfo SecurityTypeInfo => GetZPropertyInfo(nameof(SecurityType));

	[ReadOnlyMember(nameof(ExitDateReadOnly))]
	public ZDateTime ExitDate
	{
		get => exitDate;
		set => SetNonPersistentPropertyValue(ExitDateInfo, ref exitDate, value);
	}
	ZDateTime exitDate;

	bool ExitDateReadOnly => true;

	public ZPropertyInfo ExitDateInfo => GetZPropertyInfo(nameof(ExitDate));

	[ResourceStringData("45C975AB-9668-4642-B8C0-7A9EB2A10BCA", Caption = "Annotation")]
	[ReadOnlyMember(nameof(AnnotationReadOnly))]
	[MaxLength(512)]
	public ZString Annotation
	{
		get => annotation;
		set
		{
			CheckMaximumLength(AnnotationInfo, value);
			SetNonPersistentPropertyValue(AnnotationInfo, ref annotation, value);
		}
	}

	ZString annotation;

	bool AnnotationReadOnly => true;

	public ZPropertyInfo AnnotationInfo => GetZPropertyInfo(nameof(Annotation));

	[ChildEditable(true)]
	public AlternativeEvidenceCollection AlternativeEvidences
	{
		get
		{
			if (alternativeEvidences == null)
			{
				alternativeEvidences = new AlternativeEvidenceCollection(Factory);
				RegisterEditableChildObject(alternativeEvidences);
			}
			return alternativeEvidences;
		}
	}
	AlternativeEvidenceCollection alternativeEvidences;

	protected override bool ShouldSend_ReadOnly => Header.EntryInstruction is CusEntryInstruction instruction && instruction.CEI_SubStyle == EntrySubStyleList.Codes.RetrospectiveLodgementOfAnExportReExportDeclaration;

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		DefaultExitCustomsOfficeFromDeclaration();
	}

	void DefaultExitCustomsOfficeFromDeclaration()
	{
		if (!ExitCustomsOfficeReadOnly)
		{
			var declarationExitOffice = Header.Declaration?.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit)?.CY_Data ?? ZString.Empty;
			if (!declarationExitOffice.IsEmpty)
			{
				ExitCustomsOffice = declarationExitOffice;
			}
		}
	}

	[ReadOnlyMember(nameof(JustificationReadOnly))]
	[ResourceStringData("NPBO:Enterprise.Customs.BE.Business.MessageSending.MessageSendingObject|Justification", Caption = "Justification")]
	[MaxLength(512)]
	public ZString Justification
	{
		get => justification;
		set
		{
			CheckMaximumLength(JustificationInfo, value);
			SetNonPersistentPropertyValue(JustificationInfo, ref justification, value);
		}
	}

	ZString justification;

	public ZPropertyInfo JustificationInfo => GetZPropertyInfo(nameof(Justification));

	bool JustificationReadOnly => false;

	public ExportEntryMessageSendingActionLookups Lookups => fLookups ?? (fLookups = new ExportEntryMessageSendingActionLookups(this));
	ExportEntryMessageSendingActionLookups fLookups;

	public new ExportEntryMessageSendingActionValidation Validation => (ExportEntryMessageSendingActionValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new ExportEntryMessageSendingActionValidation(this);
}
