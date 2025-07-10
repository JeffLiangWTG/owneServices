using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using TariffFormatter = Enterprise.Customs.EU.Business.TariffFormatter;

namespace Enterprise.Customs.DE.Business.Declaration
{
	[SystemDefinedValues]
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument, ITariffFormatProvider
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
		{
			public const string AuthorizationNumber = nameof(PreviousDocument.AuthorizationNumber);
			public const string Status = nameof(PreviousDocument.Status);
			public const string SimplifiedGrantAuthorizationFlag = nameof(PreviousDocument.SimplifiedGrantAuthorizationFlag);
			public const string UsualProcessingFlag = nameof(PreviousDocument.UsualProcessingFlag);
			public const string FormattedTariff = nameof(PreviousDocument.FormattedTariff);
			public const string CSI_ItemNumberString = nameof(PreviousDocument.CSI_ItemNumberString);

			public const int AuthorizationNumberMaxLength = 35;
		}

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;
			if (IsImport)
			{
				result = new ImportPreviousDocumentValidation(this);
			}
			else if (IsExport)
			{
				if (PreviousDocumentLookups.GetProcedureList(false).ContainsCode(CSI_Procedure))
				{
					result = new ExportPreviousProcedureValidation(this);
				}
				else
				{
					result = new ExportPreviousDocumentValidation(this);
				}
			}
			else
			{
				result = new PreviousDocumentValidation(this);
			}
			return result;
		}

		protected override ZString HumanReadableNameCore => CSI_Procedure.IsEmpty
			? Res.GetString("BDFD421C-917F-48C5-A125-0D63733D9411", "Previous Document")
			: Res.GetString("EF4F300B-6D18-44B2-BAC8-A9E8B9D0E331", "Previous Procedure");

		#region Properties

		public ZString CusEntryInstructionParentStyle => (Parent as CusEntryInstruction)?.CEI_Style ?? ZString.Empty;

		[MaxLength(Schema.AuthorizationNumberMaxLength)]
		public ZString AuthorizationNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AuthorizationNumber);
			set
			{
				var oldValue = AuthorizationNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(AuthorizationNumberInfo, value);
					this.SetSystemDefinedValue(Schema.AuthorizationNumber, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAuthorizationNumber();
					}
					AuthorizationNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(Schema.AuthorizationNumber);

		public ZBool UsualProcessingFlag
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.UsualProcessingFlag);
			set
			{
				var oldValue = UsualProcessingFlag;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.UsualProcessingFlag, value);
					UsualProcessingFlagInfo.RefreshBinding(oldValue);
					ClearQuantityIfReadOnly();
				}
			}
		}

		public ZPropertyInfo UsualProcessingFlagInfo => GetZPropertyInfo(Schema.UsualProcessingFlag);

		[ReadOnlyMember(nameof(CSI_LineNo_ReadOnly))]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		bool CSI_LineNo_ReadOnly => IsImport && IsProcedureATNEU && (CSI_SubType == PreviousDocSubTypeList.Codes.AWB || CSI_SubType == PreviousDocSubTypeList.Codes.ULD);

		void ClearLineNoIfReadOnly()
		{
			if (CSI_LineNo_ReadOnly)
			{
				CSI_LineNo = ZInt.Zero;
			}
		}

		[MaxLength(nameof(CSI_DescriptionMaxLength))]
		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		int CSI_DescriptionMaxLength
		{
			get
			{
				var result = Schema.DescriptionMaxLength;
				if (IsProcedureATZL)
				{
					result = 100;
				}
				else if (IsProcedureATAV)
				{
					result = 300;
				}
				else if (IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
				{
					result = 35;
				}
				return result;
			}
		}

		bool CSI_Description_ReadOnly => IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments && CodeCusCodeList.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement);

		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				base.CSI_SubType = value;
				ClearCSI_ReferenceNumber2IfReadOnly();
				ClearLineNoIfReadOnly();
				SetCustodianEori();
			}
		}

		void SetCustodianEori()
		{
			if (Parent is CusEntryInstruction parent && IsProcedureATNEU && CSI_SubType == PreviousDocSubTypeList.Codes.AWB)
			{
				var declaration = parent.JobDeclaration;
				var depot = declaration.DepotDocAddress;
				var custodianEori = depot.IsEmpty ? ZString.Empty : depot.Organisation.GetEUEoriDetails();
				if (custodianEori.IsEmpty)
				{
					var cto = declaration.ContainerTerminalOperatorDocAddress;
					if (!cto.IsEmpty)
					{
						custodianEori = cto.Organisation.GetEUEoriDetails();
					}
				}

				if (!custodianEori.IsEmpty)
				{
					CSI_ReferenceNumber2 = custodianEori;
				}
			}
		}

		public ZBool SimplifiedGrantAuthorizationFlag
		{
			get => CSI_SubType == SimplifiedGrantAuthorizationList.Codes.J;
			set => CSI_SubType = value ? SimplifiedGrantAuthorizationList.Codes.J : SimplifiedGrantAuthorizationList.Codes.N;
		}

		public ZPropertyInfo SimplifiedGrantAuthorizationFlagInfo => GetWrappedZPropertyInfo(Schema.SimplifiedGrantAuthorizationFlag, x => CSI_SubTypeInfo);

		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set
			{
				var oldValue = CSI_Procedure;
				base.CSI_Procedure = value;
				if (oldValue != CSI_Procedure)
				{
					CSI_Description = CSI_Description.Left(CSI_DescriptionMaxLength);
				}
			}
		}

		[MaxLength(nameof(CSI_ReferenceNumber2MaxLength))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		public int CSI_ReferenceNumber2MaxLength
		{
			get
			{
				if (IsImport)
				{
					if (IsProcedureATNEU)
					{
						return 17;
					}
					else if (IsProcedureATZL)
					{
						return 35;
					}
				}
				else if (IsExport)
				{
					return 26;
				}
				return Schema.CSI_ReferenceNumber2MaxLength;
			}
		}

		public bool CSI_ReferenceNumber2_ReadOnly => IsImport && IsProcedureATNEU && CSI_SubType == PreviousDocSubTypeList.Codes.REG;

		void ClearCSI_ReferenceNumber2IfReadOnly()
		{
			if (CSI_ReferenceNumber2_ReadOnly)
			{
				CSI_ReferenceNumber2 = ZString.Empty;
			}
		}

		public override ZString CSI_Status
		{
			get => base.CSI_Status;
			set
			{
				base.CSI_Status = value;
				if (Status && CSI_ReferenceNumber.Length > 21)
				{
					CSI_ReferenceNumber = ZString.Empty;
				}
				Validation.ValidateCSI_ReferenceNumber();
			}
		}

		public ZBool Status
		{
			get => CSI_Status == YesNoList.Codes.Yes;
			set => CSI_Status = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
		}

		public ZPropertyInfo StatusInfo => GetWrappedZPropertyInfo(Schema.Status, x => CSI_StatusInfo);

		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.DocumentCodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		bool CSI_ReferenceNumber_ReadOnly => !(IsImport || IsInExportInvoiceLinePreviousProcedure) && CodeCusCodeList.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference);

		internal ZZRefCusCodeListCombined CodeCusCodeList
		{
			get
			{
				var parent = Parent;
				if (parent is JobComInvoiceHeader || IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
				{
					var code = CSI_Code;
					return Factory.GetCachedValue(string.Join("_", "PreviousDocument.CodeCusCodeList", code, ZDateTime.Today), () =>
					{
						var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
						query.AddToFilter(Lookups.DocumentCodeList.CompleteFilter);
						return Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
					});
				}
				return null;
			}
		}

		internal ZBool IsInExportPreviousDocuments => Factory.GetValue(ref isInExportPreviousDocuments, () => CSI_Procedure.IsEmpty && IsExport);
		CachedProperty<ZBool> isInExportPreviousDocuments;

		internal ZBool IsInExportInvoiceLinePreviousDocuments => Factory.GetValue(ref isInExportInvoiceLinePreviousDocuments, () => CSI_Procedure.IsEmpty && IsExport && Parent is JobComInvoiceLine);
		CachedProperty<ZBool> isInExportInvoiceLinePreviousDocuments;

		internal ZBool IsInExportInvoiceLinePreviousProcedure => Factory.GetValue(ref isInExportInvoiceLinePreviousProcedure, () => !CSI_Procedure.IsEmpty && IsExport && Parent is JobComInvoiceLine);
		CachedProperty<ZBool> isInExportInvoiceLinePreviousProcedure;

		internal ZBool IsInExportCusClassPartPivotPreviousDocuments => Factory.GetValue(ref isInExportCusClassPartPivotPreviousDocuments, () => CSI_Procedure.IsEmpty && IsExport && Parent is CusClassPartPivot);
		CachedProperty<ZBool> isInExportCusClassPartPivotPreviousDocuments;

		internal ZBool IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments => IsInExportInvoiceLinePreviousDocuments || IsInExportCusClassPartPivotPreviousDocuments;

		int CSI_ReferenceNumberMaxLength
		{
			get
			{
				var maxLength = Schema.ReferenceNumberMaxLength;
				if (this.Requires18Or21CharactersReference())
				{
					maxLength = 21;
				}
				else if (IsProcedureATNEU && (CSI_SubType == PreviousDocSubTypeList.Codes.AWB || CSI_SubType == PreviousDocSubTypeList.Codes.ULD))
				{
					maxLength = 44;
				}
				else if (procedureCodesRequires28CharactersReferenceForImport.Contains(CSI_Procedure))
				{
					maxLength = 28;
				}
				else if (IsExport && !UniversalValidationHelper.IsInAESTransitionPeriod)
				{
					maxLength = 70;
				}
				return maxLength;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.InvoiceLineNumberList))]
		public ZString CSI_ItemNumberString
		{
			get => CSI_ItemNumber.ToString();
			set
			{
				var oldValue = CSI_ItemNumberString;
				ZShort.TryParse(value, out var zshortValue);
				CSI_ItemNumber = zshortValue;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_ItemNumberString();
				}
				CSI_ItemNumberStringInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CSI_ItemNumberStringInfo => GetZPropertyInfo(Schema.CSI_ItemNumberString);

		[ResourceStringData("C31A1F43-F291-429F-82B3-E170FBA181D8", Caption = "Item No.")]
		[ReadOnlyMember(nameof(CSI_ItemNumber_ReadOnly))]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		bool CSI_ItemNumber_ReadOnly => !IsImport && CodeCusCodeList.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber);

		[ReadOnlyMember(nameof(MainQuantity_ReadOnly))]
		[DecimalPlaces(nameof(CSI_QuantityDecimals))]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		internal int CSI_QuantityDecimals => (IsImport && IsProcedureATNEU) ? 0 : (IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments ? 5 : 3);

		void ClearQuantityIfReadOnly()
		{
			if (MainQuantity_ReadOnly)
			{
				CSI_Quantity = ZDecimal.Zero;
				CSI_UnitOfQuantity = ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(MainQuantity_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.UnitOfQuantityList))]
		public override ZString CSI_UnitOfQuantity
		{
			get => base.CSI_UnitOfQuantity;
			set => base.CSI_UnitOfQuantity = value;
		}

		bool MainQuantity_ReadOnly => (IsProcedureATZL && !UsualProcessingFlag) || (IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments && CodeCusCodeList.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit));

		public bool hasUOM => CodeCusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit);

		[BusinessObjectTestExclude]
		public override ZString CSI_Tariff
		{
			get => base.CSI_Tariff;
			set => base.CSI_Tariff = TariffFormatter.Format(value).Left(CSI_TariffInfo.MaxLength);
		}

		[ResourceStringData("15406EB8-A2FE-4E7C-93E4-2C313AD1EDAF", Caption = "Commodity Code")]
		public ZString FormattedTariff
		{
			get => TariffFormatter.DisplayFormat(CSI_Tariff);
			set => CSI_Tariff = value;
		}

		public ZPropertyInfo FormattedTariffInfo => GetWrappedZPropertyInfo(Schema.FormattedTariff, x => CSI_TariffInfo);

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = EU.Business.TariffFormatter.New(Core.Constants.CountryCodes.Germany));
		TariffFormatter tariffFormatter;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsImport)
			{
				ClearUnRelatedProperties();
				this.ReferenceNumberValid();
			}
		}

		void ClearUnRelatedProperties()
		{
			var procedure = CSI_Procedure;
			var previousDocumentExtendList = new PreviousDocumentConfiguration();
			var availableColumns = previousDocumentExtendList.GetAvailableColumnsFromProcedureCode(IsImport, procedure, CusEntryInstructionParentStyle);
			var availableFields = previousDocumentExtendList.GetAvailableFieldsFromProcedureCode(procedure);

			var unRelatedProperties = previousDocumentExtendList.GetUnavailableColumns(availableColumns.Select(x => x.ColumnName).Concat(availableFields));
			foreach (var columnName in unRelatedProperties)
			{
				GetZPropertyInfo(columnName).ClearValue();
			}
		}

		#endregion

		public bool IsImport => ImportExportParent?.IsImport ?? false;

		public bool IsExport => ImportExportParent?.IsExport ?? false;

		public override ZString KeyToDeterimeUniqueness => CSI_SubType + CSI_Code + CSI_ReferenceNumber;

		public bool IsProcedureATZL => Factory.GetValue(ref isProcedureATZLCached, () => CSI_Procedure == PreviousProcedureList.Codes._ATZL);
		CachedProperty<bool> isProcedureATZLCached;

		public bool IsProcedureATAV => Factory.GetValue(ref isProcedureATAVCached, () => CSI_Procedure == PreviousProcedureList.Codes._ATAV);
		CachedProperty<bool> isProcedureATAVCached;

		public bool IsProcedureATNEU => Factory.GetValue(ref isProcedureATNEUCached, () => CSI_Procedure == PreviousProcedureList.Codes._ATNEU);
		CachedProperty<bool> isProcedureATNEUCached;

		readonly ImmutableHashSet<string> procedureCodesRequires28CharactersReferenceForImport = new HashSet<string>
		{
			PreviousProcedureList.Codes._ATA,
			PreviousProcedureList.Codes._ESUMA,
			PreviousProcedureList.Codes._VO,
			PreviousProcedureList.Codes._TIR,
			PreviousProcedureList.Codes._PUEB,
			PreviousProcedureList.Codes._GB,
		}.ToImmutableHashSet();
	}
}
