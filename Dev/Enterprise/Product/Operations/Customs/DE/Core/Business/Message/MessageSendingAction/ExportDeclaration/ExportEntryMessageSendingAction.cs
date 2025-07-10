using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEntryMessageSendingAction : MessageSendingAction, IObsoleteValidation
	{
		#region Construction

		public ExportEntryMessageSendingAction(CusEntryHeader entry, ExportEntryMessageSendingActionCollection parentCollection)
			: this(entry)
		{
			this.parentCollection = parentCollection;
		}

		readonly ExportEntryMessageSendingActionCollection parentCollection;

		internal ExportEntryMessageSendingAction(CusEntryHeader entry)
			: base(entry, (x) => ((CusEntryHeader)x).MovementReferenceNumber)
		{
			movementReferenceNumber = Details;
			localReferenceNumber = entry.LocalReferenceNumber;
		}

		#endregion

		#region MessagingObject

		public new CusEntryHeader MessagingObject => (CusEntryHeader)base.MessagingObject;

		public ZString ProcedureType => MessagingObject.EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString Variant => MessagingObject.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		public ZString Description => MessagingObject.EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZString EntryStatus => MessagingObject.CH_EntryStatus;

		public ZString ExportCustomsOffice => MessagingObject.Declaration?.JE_CustomsOffice ?? ZString.Empty;

		#endregion

		#region Properties

		public override ZBool ShouldSend
		{
			set
			{
				base.ShouldSend = value;

				if (value && EntryStatus.IsEmpty && EntryType.IsEmpty)
				{
					EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
				}
			}
		}

		#endregion

		#region New Properties

		[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.EntryTypeList))]
		[MaxLength(3)]
		public ZString EntryType
		{
			get { return entryType; }
			set
			{
				SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value);
				if (!IsValidationSuspended)
				{
					ValidateEntryType();
					UpdateReasonOrAnnotationLabels();
				}
				ClearIrrelevantData();
				DefaultExitCustomsOfficeFromDeclaration();
				PresetSecurityType();
				AlternativeEvidences.SetReadOnlyIncludingChildren(ExitTypeReadOnly);
			}
		}
		ZString entryType;

		public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(nameof(EntryType));

		[ReadOnlyMember(nameof(MovementReferenceNumberReadOnly))]
		[MaxLength(18)]
		public ZString MovementReferenceNumber
		{
			get => movementReferenceNumber;
			set
			{
				if (movementReferenceNumber != value)
				{
					SetNonPersistentPropertyValue(MovementReferenceNumberInfo, ref movementReferenceNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateMovementReferenceNumber();
					}
					if (!movementReferenceNumber.IsEmpty)
					{
						SetMrnCusEntryNumber(value);
						MessagingObject.HasChanges = true;
					}
				}
			}
		}
		ZString movementReferenceNumber;

		bool MovementReferenceNumberReadOnly => GetMRNReadOnly();

		bool GetMRNReadOnly()
		{
			if (ExportEntryTypeList.IsSupplementaryExportDeclaration(EntryType))
			{
				return !Details.IsEmpty;
			}
			return ExportEntryTypeList.IsExportDeclaration(EntryType) || !EntryStatus.IsEmpty;
		}

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		[MaxLength(22)]
		public ZString LocalReferenceNumber
		{
			get => localReferenceNumber;
			set
			{
				SetNonPersistentPropertyValue(LocalReferenceNumberInfo, ref localReferenceNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalReferenceNumber();
				}
			}
		}

		ZString localReferenceNumber;

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(nameof(LocalReferenceNumber));

		void UpdateReasonOrAnnotationLabels()
		{
			AnnotationInfo.HumanReadableName = EntryType == ExportEntryTypeList.Codes.CancellationRequest ? Res.GetString("56818818-a293-46a6-8de0-12b32744ec9d", "Reason") : string.Empty;
		}

		void PresetSecurityType()
		{
			var entryStyle = MessagingObject.Declaration?.JE_EntryStyle ?? ZString.Empty;

			if (EntryType == ExportEntryTypeList.Codes.ExportDeclaration)
			{
				if (entryStyle == EntryStyleListExport.Codes.ExportNormal)
				{
					SecurityType = ExportSecurityTypeList.Codes.EXS;
				}
				else
				{
					SecurityType = ExportSecurityTypeList.Codes.NotUsed;
				}
			}
			else if (entryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
			{
				SecurityType = ExportSecurityTypeList.Codes.NotUsed;
			}
			else
			{
				SecurityType = ExportSecurityTypeList.Codes.EXS;
			}
		}

		void ClearIrrelevantData()
		{
			if (AnnotationReadOnly)
			{
				Annotation = ZString.Empty;
			}

			if (ExitTypeReadOnly)
			{
				ExitType = ZString.Empty;
				AlternativeEvidences.RemoveAndDeleteAll();
			}

			if (ExitDateReadOnly)
			{
				ExitDate = ZDateTime.Empty;
			}

			if (ExitCustomsOfficeReadOnly)
			{
				ExitCustomsOffice = ZString.Empty;
			}
		}

		void DefaultExitCustomsOfficeFromDeclaration()
		{
			if (!ExitCustomsOfficeReadOnly)
			{
				var declarationExitOffice = MessagingObject.Declaration?.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit)?.CY_Data ?? ZString.Empty;
				if (!declarationExitOffice.IsEmpty)
				{
					ExitCustomsOffice = declarationExitOffice.Left(ExitCustomsOfficeMaxLength);
				}
			}
		}

		[ReadOnlyMember(nameof(AnnotationReadOnly))]
		[MaxLength(512)]
		public ZString Annotation
		{
			get { return annotation; }
			set
			{
				SetNonPersistentPropertyValue(AnnotationInfo, ref annotation, value);
				if (!IsValidationSuspended)
				{
					ValidateAnnotation();
				}
			}
		}
		ZString annotation;

		bool AnnotationReadOnly => !ExportEntryTypeList.IsCancellationOrExitToExport(EntryType);

		public ResourceStringData AnnotationCaption
		{
			get
			{
				ResourceStringData result;
				if (EntryType == ExportEntryTypeList.Codes.CancellationRequest)
				{
					result = new ResourceStringData("C0376513-383E-4F9A-95C3-BA667DE88A6A", (NoResString)"Reason for Cancellation");// ResourceStringData
				}
				else
				{
					result = new ResourceStringData("7d2929a9-5a3e-4b0d-a010-f3d2fcf1e247", (NoResString)"Annotation");// ResourceStringData
				}

				return result;
			}
		}

		public ZPropertyInfo AnnotationInfo => GetZPropertyInfo(nameof(Annotation));

		[ReadOnlyMember(nameof(ExitTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.ExitTypeList))]
		[MaxLength(3)]
		public ZString ExitType
		{
			get { return exitType; }
			set
			{
				SetNonPersistentPropertyValue(ExitTypeInfo, ref exitType, value);
				if (!IsValidationSuspended)
				{
					ValidateExitType();
				}
			}
		}
		ZString exitType;

		bool ExitTypeReadOnly => !ExportEntryTypeList.IsExitToExport(EntryType);

		public ZPropertyInfo ExitTypeInfo => GetZPropertyInfo(nameof(ExitType));

		[ReadOnlyMember(nameof(ExitDateReadOnly))]
		public ZDateTime ExitDate
		{
			get { return exitDate; }
			set
			{
				SetNonPersistentPropertyValue(ExitDateInfo, ref exitDate, value);
				if (!IsValidationSuspended)
				{
					ValidateExitDate();
				}
			}
		}
		ZDateTime exitDate;

		bool ExitDateReadOnly => !ExportEntryTypeList.IsExitToExport(EntryType);

		public ZPropertyInfo ExitDateInfo => GetZPropertyInfo(nameof(ExitDate));

		[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.ExitCustomsOfficeList))]
		[ReadOnlyMember(nameof(ExitCustomsOfficeReadOnly))]
		[MaxLength(nameof(ExitCustomsOfficeMaxLength))]
		public ZString ExitCustomsOffice
		{
			get { return exitCustomsOffice; }
			set
			{
				SetNonPersistentPropertyValue(ExitCustomsOfficeInfo, ref exitCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					ValidateExitCustomsOffice();
				}
			}
		}
		ZString exitCustomsOffice;

		const int ExitCustomsOfficeMaxLength = 8;

		bool ExitCustomsOfficeReadOnly => !ExportEntryTypeList.IsExitToExport(EntryType);

		public ZPropertyInfo ExitCustomsOfficeInfo => GetZPropertyInfo(nameof(ExitCustomsOffice));

		public ZBool NotSubmitConsignee
		{
			get { return notSubmitConsignee; }
			set
			{
				SetNonPersistentPropertyValue(NotSubmitConsigneeInfo, ref notSubmitConsignee, value);
			}
		}
		ZBool notSubmitConsignee;

		public ZPropertyInfo NotSubmitConsigneeInfo => GetZPropertyInfo(nameof(NotSubmitConsignee));

		public bool SubmitConsignee => !NotSubmitConsignee;

		[List(nameof(Lookups) + "." + nameof(ExportEntryMessageSendingActionLookups.SecurityTypeList))]
		[ReadOnlyMember(nameof(SecurityTypeReadOnly))]
		[MaxLength(1)]
		public ZString SecurityType
		{
			get { return securityType; }
			set
			{
				SetNonPersistentPropertyValue(SecurityTypeInfo, ref securityType, value);
				if (!IsValidationSuspended)
				{
					ValidateSecurityType();
				}
			}
		}
		ZString securityType;

		bool SecurityTypeReadOnly => !ExportEntryTypeList.IsExportDeclaration(EntryType);

		public ZPropertyInfo SecurityTypeInfo => GetZPropertyInfo(nameof(SecurityType));

		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public AlternativeEvidenceCollection AlternativeEvidences
		{
			get
			{
				if (alternativeEvidences == null)
				{
					alternativeEvidences = new AlternativeEvidenceCollection(Factory);
					alternativeEvidences.SetReadOnlyIncludingChildren(ExitTypeReadOnly);
					RegisterEditableChildObject(alternativeEvidences);
				}
				return alternativeEvidences;
			}
		}
		AlternativeEvidenceCollection alternativeEvidences;

		[ChildEditable(true)]
		public ExportEntryLineCollection EntryLines
		{
			get
			{
				if (entryLines == null)
				{
					entryLines = new ExportEntryLineCollection(MessagingObject);
					RegisterEditableChildObject(entryLines);
				}
				return entryLines;
			}
		}
		ExportEntryLineCollection entryLines;

		#endregion

		#region Functionality

		internal void CopyValuesBackToEntry()
		{
			if (ShouldSend)
			{
				var entryInstruction = MessagingObject.EntryInstruction;
				if (ExitDate.IsValid && entryInstruction != null && ExitDate != entryInstruction.ZG_ExitDate)
				{
					entryInstruction.ZG_ExitDate = ExitDate;
				}
				MessagingObject.LocalReferenceNumber = LocalReferenceNumber;
			}
		}

		#endregion

		#region Lookups and Validation

		public ExportEntryMessageSendingActionLookups Lookups => new ExportEntryMessageSendingActionLookups(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEntryType();
			ValidateAnnotation();
			ValidateExitType();
			ValidateExitDate();
			ValidateExitCustomsOffice();
			ValidateSecurityType();
			ValidateLocalReferenceNumber();
		}

		public void ValidateEntryType()
		{
			var targetInfo = EntryTypeInfo;
			targetInfo.ClearAllNotifications();
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (ShouldSend)
			{
				MandatoryValidation.CheckEntered(targetInfo);

				if ((EntryType == ExportEntryTypeList.Codes.CancellationRequest && !EntryStatusValidForCAN.Contains(EntryStatus)) ||
					(EntryType == ExportEntryTypeList.Codes.ExportDeclaration && !EntryStatus.IsEmpty))
				{
					targetInfo.AddMessageError(CannotSendEntryTypeDueToEntryStatusMessageError);
				}
				else if ((EntryType == ExportEntryTypeList.Codes.ExportAmendment && !CanSendAMDMessage) ||
						(EntryType == ExportEntryTypeList.Codes.SupplementaryExportDeclaration && !CanSendENTMessage))
				{
					targetInfo.AddMessageError(CannotSendEntryTypeDueToEntryStatusAndProcedureMessageError);
				}
			}

			if (EntryType == ExportEntryTypeList.Codes.ExitToExport &&
				MessagingObject.MovementReferenceNumberIssueDate.IsValid &&
				MessagingObject.MovementReferenceNumberIssueDate > ZDateTime.Today.AddDays(-70))
			{
				targetInfo.AddMessageError(Res.GetString("43455CF5-3BB5-4C29-BAC8-02A860957D77", "The Follow Up Procedure can be initiated earliest 70 days after Release for Export ({0}).", MessagingObject.MovementReferenceNumberIssueDate.ToString("dd.MM.yyyy")));
			}

			ValidateAnnotation();
			ValidateMovementReferenceNumber();
		}

		bool CanSendAMDMessage => EntryStatusValidForAMD.Contains(EntryStatus) && ExportDeclarationTypeProcedureList.IsPresentationOutsideOfficialPlace(Factory, Variant, ProcedureType);

		bool CanSendENTMessage => EntryStatusValidForENT.Contains(EntryStatus) && ExportDeclarationTypeProcedureList.IsIncompleteDeclaration(ProcedureType);

		ImmutableHashSet<string> EntryStatusValidForCAN => Factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction | EntryStatusValidForCAN", // Cache Key
			() => ImmutableHashSet.Create(
				"110", "130", "131", "132", "141", "142", "500", "501", "502", "541", "542", "570" //Change this when a codedescriptionpairlist is created
			)
		);

		ImmutableHashSet<string> EntryStatusValidForAMD => Factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction | EntryStatusValidForAMD", // Cache Key
			() => ImmutableHashSet.Create(
				"110", "130", "131", "132" //Change this when a codedescriptionpairlist is created
			)
		);

		ImmutableHashSet<string> EntryStatusValidForENT => Factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction | EntryStatusValidForENT", // Cache Key
			() => ImmutableHashSet.Create(
				"131", "141", "501", "541" //Change this when a codedescriptionpairlist is created
			)
		);

		string CannotSendEntryTypeDueToEntryStatusMessageError => Res.GetString("A353DD99-E51F-4767-8547-585C91BB5247", "The current Entry Status does not allow this type of message ({0}) to be accepted.", EntryType);

		string CannotSendEntryTypeDueToEntryStatusAndProcedureMessageError => Res.GetString("199CA222-18B7-4E68-B75C-E61CE0E41CA6", "The current Entry Status and/ or Type(Procedure) does not allow this type of message ({0}) to be accepted.", EntryType);

		public void ValidateAnnotation()
		{
			AnnotationInfo.ClearAllNotifications();
			if (ExportEntryTypeList.IsCancellation(EntryType) || (ExportEntryTypeList.IsExitToExport(EntryType) && ExportExitTypeList.Is2(ExitType)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(AnnotationInfo);
			}
		}

		public void ValidateExitType()
		{
			var targetInfo = ExitTypeInfo;
			targetInfo.ClearAllNotifications();

			if (!ExitTypeReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);

				if (ExportExitTypeList.Is4(ExitType) && !AlternativeEvidences.Any())
				{
					targetInfo.AddMessageError(Res.GetString("DF565293-2FBF-4078-9308-EC9492C4D697", "You have not entered an Alternative Evidence."));
				}
			}
		}

		public void ValidateExitDate()
		{
			ExitDateInfo.ClearAllNotifications();
			if (!ExitDateReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(ExitDateInfo);
			}

			if (ExitDate.IsValid)
			{
				if (ExportExitTypeList.Is2(ExitType) && ExitDate.IsInThePastDatePartOnly)
				{
					ExitDateInfo.AddMessageError(Res.GetString("44B6DFC7-331C-49EB-8896-343354B3199D", "Exit Date must not be in the past for the selected Exit Type."));
				}
				else if (ExportExitTypeList.Is4(ExitType))
				{
					if (ExitDate.IsInTheFutureDatePartOnly)
					{
						ExitDateInfo.AddMessageError(Res.GetString("A51B00C9-511D-4C70-93FC-D04D61A777F2", "Exit Date must not be in the future for the selected Exit Type."));
					}
					if (MessagingObject.MovementReferenceNumberIssueDate.IsValid && ExitDate.Date < MessagingObject.MovementReferenceNumberIssueDate.Date)
					{
						ExitDateInfo.AddMessageError(Res.GetString("{2AC5165D-5F22-4785-92D9-D6BC5DACB406}", "Exit Date must not be earlier than Release Date."));
					}
				}
			}
		}

		public void ValidateExitCustomsOffice()
		{
			ExitCustomsOfficeInfo.ClearAllNotifications();
			if (!ExitCustomsOfficeReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(ExitCustomsOfficeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(ExitCustomsOfficeInfo);
		}

		public void ValidateSecurityType()
		{
			SecurityTypeInfo.ClearAllNotifications();
			ListValidation.MessageErrorIfInvalidCode(SecurityTypeInfo);

			if (!SecurityTypeReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(SecurityTypeInfo);
			}
		}

		public void ValidateMovementReferenceNumber()
		{
			var targetInfo = MovementReferenceNumberInfo;
			targetInfo.ClearAllNotifications();

			if (entryType == ExportEntryTypeList.Codes.SupplementaryExportDeclaration && MovementReferenceNumber.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("9B7E44C1-EC33-47BE-87C7-6618519330AF", "You have not entered the MRN of the incomplete Export Declaration."));
			}
			if (entryType != ExportEntryTypeList.Codes.ExportDeclaration)
			{
				var mrnError = MRNFormatValidator.CheckMRNFormat(MovementReferenceNumber, Factory, ZString.Empty);
				if (!mrnError.IsEmpty)
				{
					targetInfo.AddMessageError(mrnError);
				}
			}
		}

		public void ValidateLocalReferenceNumber()
		{
			var targetInfo = LocalReferenceNumberInfo;
			targetInfo.ClearAllNotifications();

			if (ShouldSend)
			{
				var entry = MessagingObject;
				var declaration = entry.Declaration;
				if (declaration != null)
				{
					var localReferenceNumber = LocalReferenceNumber;
					if (!localReferenceNumber.IsEmpty)
					{
						if (parentCollection != null && parentCollection.Cast<ExportEntryMessageSendingAction>().Count(x => x.ShouldSend && x.LocalReferenceNumber == localReferenceNumber) > 1)
						{
							targetInfo.AddMessageError(Res.GetString("03927112-7685-4304-AF0D-E34F7AAA7D49", "LRN number must be unique."));
						}

						var currentDeclarantAddressPK = declaration.JE_OA_DeclarantAddress;
						var currentRepresentativePK = declaration.JE_OA_Representative;
						var currentEntryInstruction = entry.EntryInstruction;
						var currentConstellation3rdDigitIs0 = currentEntryInstruction.Constellation3rdDigitIs0();
						var currentConstellation3rdDigitIs1 = currentEntryInstruction.Constellation3rdDigitIs1();
						if ((!currentDeclarantAddressPK.IsEmpty && currentConstellation3rdDigitIs0) || (!currentRepresentativePK.IsEmpty && currentConstellation3rdDigitIs1))
						{
							var cusEntryNumbers = CusEntryNumber.Load<CusEntryNumber>(Factory, CusEntryNumberTypes.Standard.LocalReferenceNumber, localReferenceNumber, Core.Constants.CountryCodes.Germany);
							var otherExportEntryHeadersOfDE = cusEntryNumbers.Where(x => x.Parent is CusEntryHeader cusEntryHeader && !GetEntryStatusList().Contains(cusEntryHeader.CH_EntryStatus) && cusEntryHeader.Declaration is JobDeclaration newDeclaration && newDeclaration.PK != declaration.PK && newDeclaration.IsExport)
								.Select(x => x.Parent as CusEntryHeader);

							var otherDeclaration = otherExportEntryHeadersOfDE.FirstOrDefault(x =>
							(currentConstellation3rdDigitIs0 && ((x.Declaration.JE_OA_DeclarantAddress == currentDeclarantAddressPK && x.EntryInstruction.Constellation3rdDigitIs0()) || (x.Declaration.JE_OA_Representative == currentDeclarantAddressPK && x.EntryInstruction.Constellation3rdDigitIs1()))) ||
							(currentConstellation3rdDigitIs1 && ((x.Declaration.JE_OA_DeclarantAddress == currentRepresentativePK && x.EntryInstruction.Constellation3rdDigitIs0()) || (x.Declaration.JE_OA_Representative == currentRepresentativePK && x.EntryInstruction.Constellation3rdDigitIs1())))
							)?.Declaration;
							if (otherDeclaration != null)
							{
								targetInfo.AddMessageError(Res.GetString("30E0DA38-37FD-4F04-A736-7581477E6125", "LRN number must be unique. LRN {0} was already used on Job {1}.", localReferenceNumber, otherDeclaration.JE_DeclarationReference));
							}
						}
					}
				}
			}

			ImmutableHashSet<string> GetEntryStatusList() => Factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction | GetEntryStatusList", // Cache Key
					() => ImmutableHashSet.Create("", "191", "192", "520"));
		}

		public override void ValidateShouldSend()
		{
			base.ValidateShouldSend();

			if (!IsValidationSuspended)
			{
				ValidateLocalReferenceNumber();
			}
		}

		#endregion
	}
}
