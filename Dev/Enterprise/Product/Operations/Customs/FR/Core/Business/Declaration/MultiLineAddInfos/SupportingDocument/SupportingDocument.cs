using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
		{
			public const string CSI_IsDTP = "CSI_IsDTP";
			public const string IsD48 = "IsD48";
		}

		#region Properties

		protected override bool IsLineCore => true;

		protected override bool IsLineOnlyCore => !(Declaration is JobDeclaration declaration && declaration.IsUCC6);

		public override ZInt QuantityDecimalPlaces => 4;

		public ZInt Quantity2DecimalPlaces => 4;

		public ZString CSI_UnitOfQuantityForMessageSending
		{
			get
			{
				if (CurrentCSI_CodeContainsUQMapTypeAttribute)
				{
					var mappedUQ = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.France, CurrentUQMapType, CSI_UnitOfQuantity, ZDateTime.Today);
					return mappedUQ.IsEmpty ? CSI_UnitOfQuantity : mappedUQ;
				}
				return CSI_UnitOfQuantity;
			}
		}

		public override ZString UnitOfQuantityFieldType => CurrentCSI_CodeContainsUQMapTypeAttribute ? new ZString(nameof(FieldType.TextDropEdit)) : base.UnitOfQuantityFieldType;

		internal bool CurrentCSI_CodeContainsUQMapTypeAttribute => !CurrentUQMapType.IsEmpty;

		internal ZString CurrentUQMapType => RefCusCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.UnitsOfQuantityMapType) ?? string.Empty;

		[MaxLength(70)]
		[ResourceStringData("FRSupportingDocument|CSI_ReferenceNumber2", Caption = "Issuing Authority")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[DecimalPlaces("Quantity2DecimalPlaces")]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[ResourceStringData("FRSupportingDocument|CSI_DateOfIssue", Caption = "Date")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ResourceStringData("FRSupportingDocument|CSI_Value", Caption = "Value or Caution")]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[ResourceStringData("FRSupportingDocument|CSI_IsDTP", Caption = "DTP")]

		public ZBool CSI_IsDTP
		{
			get => CSI_Status.Left(1).EqualsIgnoringCase(YesNoList.Codes.Yes);
			set
			{
				PersistToCSI_Status(value);
				CSI_IsDTPInfo.RefreshBinding();
			}
		}

		void PersistToCSI_Status(ZBool isDTP) => CSI_Status = (isDTP ? YesNoList.Codes.Yes : YesNoList.Codes.No);

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				InvalidateCachedRefCusCode();
				base.CSI_Code = value;
				Declaration?.MarkAsNeedingValidation();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Quantity3();
				}
				CSI_CodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CSI_IsDTPInfo => GetZPropertyInfo(Schema.CSI_IsDTP);

		public bool IsDTP
		{
			get
			{
				var isDTP = false;
				if (Declaration is JobDeclaration declaration)
				{
					var codeType = GetCodeType(declaration.JE_MessageType);

					if (!codeType.IsEmpty && Parent is JobComInvoiceLine invoiceLine)
					{
						var fallbackDate = GetFallbackDate(declaration);
						isDTP = HasIsDTPAttribute(codeType, invoiceLine.EntryInstruction?.CEI_DateForDuty ?? fallbackDate, fallbackDate);
					}
				}
				else if (Parent is CusClassPartPivot pivot)
				{
					var codeType = GetCodeType(pivot.CI_ChildType);
					if (!codeType.IsEmpty)
					{
						isDTP = HasIsDTPAttribute(codeType, ZDateTime.Today, ZDateTime.Today);
					}
				}

				return isDTP;
			}
		}

		public bool IsODS
		{
			get
			{
				bool result = false;
				if (Declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Import ||
					Declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Export)
				{
					var codeType = GetCodeType(Declaration.JE_MessageType);
					var dateForDuty = ZDateTime.Today;
					var fallbackDate = GetFallbackDate(Declaration);

					if (Parent is JobComInvoiceLine invoiceLine)
					{
						dateForDuty = invoiceLine.EntryInstruction?.CEI_DateForDuty ?? fallbackDate;
					}

					result = HasIsODSAttribute(codeType, dateForDuty, fallbackDate);
				}

				return result;
			}
		}

		bool HasIsDTPAttribute(ZString codeType, ZDateTime dateForDuty, ZDateTime fallbackDate) => Lookups.GetIsDTPCodeList(codeType, dateForDuty.IsValid ? dateForDuty : fallbackDate).Any(x => x == CSI_Code);

		bool HasIsODSAttribute(ZString codeType, ZDateTime dateForDuty, ZDateTime fallbackDate) => Lookups.GetIsODSCodeList(codeType, dateForDuty.IsValid ? dateForDuty : fallbackDate).Any(x => x == CSI_Code);

		public bool IsD48
		{
			get
			{
				var isD48 = false;
				if (Declaration is JobDeclaration declaration)
				{
					var codeType = GetCodeType(declaration.JE_MessageType);

					if (!codeType.IsEmpty)
					{
						var fallbackDate = GetFallbackDate(declaration);

						if (Parent is JobComInvoiceLine invoiceLine)
						{
							isD48 = HasIsD48Attribute(codeType, invoiceLine.EntryInstruction?.CEI_DateForDuty ?? fallbackDate, fallbackDate);
						}
						else if (Parent is JobComInvoiceHeader invoiceheader)
						{
							var instructions = invoiceheader.CusEntryInstructions;
							isD48 = (!instructions.Any() && HasIsD48Attribute(codeType, fallbackDate, fallbackDate)) || instructions.Any(x => HasIsD48Attribute(codeType, x.CEI_DateForDuty, fallbackDate));
						}
						else if (Parent is JobDeclaration)
						{
							var instructions = declaration.CustomsEntryInstructions;
							instructions.Load();
							isD48 = (instructions.Count == 0 && HasIsD48Attribute(codeType, fallbackDate, fallbackDate)) || instructions.Cast<CusEntryInstruction>().Any(x => HasIsD48Attribute(codeType, x.CEI_DateForDuty, fallbackDate));
						}
					}
				}

				return isD48;
			}
		}

		ZString GetCodeType(ZString messageType)
		{
			var codeType = ZString.Empty;
			if (messageType == SharedJobMessageTypeList.Codes.Import)
			{
				codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			}
			else if (messageType == SharedJobMessageTypeList.Codes.Export)
			{
				codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			}

			return codeType;
		}

		ZDateTime GetFallbackDate(JobDeclaration declaration)
		{
			var fallbackDate = declaration.IsImport ? declaration.JE_DateOfArrival : declaration.JE_ExportDate;
			if (!fallbackDate.IsValid)
			{
				fallbackDate = ZDateTime.Now;
			}

			return fallbackDate;
		}

		public bool IsD48AndNotClosed => IsD48 && CSI_Value > 0 && CSI_Quantity3 > 0;

		bool HasIsD48Attribute(ZString codeType, ZDateTime dateForDuty, ZDateTime fallbackDate) => Lookups.GetIsD48CodeList(codeType, dateForDuty.IsValid ? dateForDuty : fallbackDate).Any(x => x == CSI_Code);

		[DecimalPlaces(0)]
		[ResourceStringData("FRSupportingDocument|CSI_Quantity3", Caption = "D48 Duration in Months")]
		public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

		[ResourceStringData("FRSupportingDocument|CSI_LineNo", ShortCaption = "Line No", Caption = "License Line No")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set
			{
				var oldValue = CSI_LineNo;

				if (oldValue != value)
				{
					base.CSI_LineNo = value;
				}
			}
		}

		[ResourceStringData("FRSupportingDocument|CSI_AdditionalDescription", ShortCaption = "Product No", Caption = "Product Reference No")]
		[MaxLength(Schema.CSI_AdditionalDescriptionMaxLength)]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		#endregion

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			if (Declaration?.IsUCC6 ?? false)
			{
				return new DeltaIESupportingDocumentValidation(this);
			}
			else
			{
				return new SupportingDocumentValidation(this);
			}
		}

		public override ZBool ShowCodeFindBoxForReferenceNumber => false;

		public ZBool ShowCalcEditItemNumber => Declaration?.IsUCC6AndIsImport ?? false;

		public ZBool ShowDropEditForUnitOfQuantity => CurrentCSI_CodeContainsUQMapTypeAttribute;

		public ZBool ShowTextBoxForUnitOfQuantiy => !ShowDropEditForUnitOfQuantity;

		protected override IValueSetStrategy GetValueSetStrategy() => new SupportingDocumentValueSetStrategy(this);

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
