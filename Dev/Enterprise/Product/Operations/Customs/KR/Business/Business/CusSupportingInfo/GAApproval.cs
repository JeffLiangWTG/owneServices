using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business
{
	public class GAApproval : CusSupportingInfo, ISupportMultipleResourceStringData
	{
		public GAApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int GAA_ProcedureMaxLength = 2;
			public const int GAA_CodeMaxLength = 1;
			public const int NonGAReasonTypeMaxLength = 5;
		}

		public int GAA_SubTypeMaxLength => Parent.IsExport ? 1 : 2;
		public int GAA_ReferenceNumberMaxLength => Parent.IsExport ? 18 : 20;
		public int GAA_ReferenceNumber2MaxLength => Parent.IsExport ? 20 : 22;
		public int GAA_DescriptionMaxLength => Parent.IsExport ? 300 : 50;
		public int GAA_AdditionalDescriptionMaxLength => Parent.IsExport ? 200 : 60;

		[List(nameof(Lookups) + "." + nameof(GAApprovalLookups.OGARegulationCategoryList))]
		[ResourceStringData("B016EB6E-AC57-466F-9411-12B921307FB2", Caption = "Regulation Category")]
		[MaxLength(Schema.GAA_ProcedureMaxLength)]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set
			{
				var oldValue = CSI_Procedure;
				base.CSI_Procedure = value;

				if (!IsDefaultValueSuspended)
				{
					if (Parent.IsExport)
					{
						if (!string.IsNullOrEmpty(DocumentTypeForProcedure))
						{
							CSI_Code = DocumentTypeForProcedure;
						}
					}

					if (oldValue != value)
					{
						CSI_Status = ZString.Empty;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateNonGAReasonType();
				}
			}
		}

		void ValidateNonGAReasonType()
		{
			if (Parent.IsExport)
			{
				var validation = (EXPGAApprovalValidation)Validation;
				validation.ValidateNonGAReasonType();
			}
		}

		public ZZRefCusCodeListCombined OGARegulationCategory => MessageFunctions.GetRefCusCodeList(Factory, CSI_Procedure, Messaging.Constants.ZZ.NKCodeType.OGARegulationCategory);

		public ZString DocumentTypeForProcedure => OGARegulationCategory?.GetAttribute(Messaging.Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType) ?? ZString.Empty;

		[ResourceStringData("3655A588-790B-4F50-A4E8-3341E21BD58A", Caption = "Requirement Type", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("73AF761E-A302-4AC4-9938-62E6129B33FE", Caption = "Use Code", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[MaxLength(nameof(GAA_SubTypeMaxLength))]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				bool beforeAdditionalDescriptionReadonly = CSI_AdditionalDescription_ReadOnly;

				var oldValue = CSI_SubType;
				base.CSI_SubType = value;

				if (!IsDefaultValueSuspended)
				{
					if (Parent.IsExport)
					{
						if (beforeAdditionalDescriptionReadonly != CSI_AdditionalDescription_ReadOnly)
						{
							CSI_AdditionalDescription = ZString.Empty;
						}

						CSI_ReferenceNumber = CSI_ReferenceNumber_ReadOnly && Parent.IsReferenceNumberRelevant ? YesNoList.Descriptions.No.ToUpper() : string.Empty;
						CSI_DateOfIssue = ZDateTime.Empty;
						CSI_ReferenceNumber2 = ZString.Empty;

						if (oldValue != value)
						{
							CSI_Status = ZString.Empty;
						}
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateNonGAReasonType();
				}
			}
		}

		[ResourceStringData("68611442-D81D-4EFA-A7CC-1BE1E1C2A6DF", Caption = "Document Type")]
		[MaxLength(Schema.GAA_CodeMaxLength)]
		[ReadOnlyMember(nameof(CSI_Code_ReadOnly))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		bool CSI_Code_ReadOnly
		{
			get { return Parent.IsExport && !string.IsNullOrEmpty(DocumentTypeForProcedure); }
		}

		[ResourceStringData("B0AC28B1-756C-47CA-BF66-192BE4E3B2D8", Caption = "Approval Number")]
		[MaxLength(nameof(GAA_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		bool CSI_ReferenceNumber_ReadOnly
		{
			get { return Parent.IsExport && !RequirementTypeCodeList.IsGADeclarationRequired(CSI_SubType); }
		}

		[ResourceStringData("3AA42E1D-1720-4755-8859-34BBA8EBB620", Caption = "Unique Item ID")]
		[MaxLength(nameof(GAA_ReferenceNumber2MaxLength))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		bool CSI_ReferenceNumber2_ReadOnly
		{
			get { return Parent.IsExport && !RequirementTypeCodeList.IsGADeclarationRequired(CSI_SubType); }
		}

		[ResourceStringData("DF0495E9-E789-4937-B14A-DB27A35855B3", Caption = "Document Name")]
		[MaxLength(nameof(GAA_DescriptionMaxLength))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[ResourceStringData("6A1BF7DA-E9AC-45F7-B230-12CED3AC0638", Caption = "Additional Reason For Approval Exemption")]
		[MaxLength(nameof(GAA_AdditionalDescriptionMaxLength))]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		bool CSI_AdditionalDescription_ReadOnly
		{
			get { return Parent.IsExport && RequirementTypeCodeList.IsGADeclarationRequired(CSI_SubType); }
		}

		[ResourceStringData("0857CAF5-B466-441F-A717-975DB4A3A334", Caption = "Issue Date")]
		[ReadOnlyMember(nameof(CSI_DateOfIssue_ReadOnly))]
		public override ZDateTime CSI_DateOfIssue
		{
			get => IsDeclarationDateToBeIssuedDate ? Parent.DeclarationDate : base.CSI_DateOfIssue;
			set => base.CSI_DateOfIssue = value;
		}

		bool CSI_DateOfIssue_ReadOnly => IsDeclarationDateToBeIssuedDate;
		bool IsDeclarationDateToBeIssuedDate => Parent.IsExport && Parent.IsIssueDateRelevant && Factory.GetCachedValue<RequirementTypeCodeList>().ContainsCode(CSI_SubType) && !RequirementTypeCodeList.IsGADeclarationRequired(CSI_SubType);

		[ReadOnly(true)]
		[ResourceStringData("3170A1F0-4EE0-442F-8708-3D08CDB15631", Caption = "Seq #")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		public override ZString CSI_Status
		{
			get => base.CSI_Status;
			set
			{
				base.CSI_Status = value;
				if (!IsValidationSuspended)
				{
					ValidateNonGAReasonType();
				}
			}
		}

		[MaxLength(Schema.NonGAReasonTypeMaxLength)]
		[ResourceStringData("9E08AA9E-27E5-4670-81E3-4E6DBDB11781", Caption = "Non-GA Reason Type")]
		[List(nameof(Lookups) + "." + nameof(GAApprovalLookups.NonGAReasonTypeList))]
		[ReadOnlyMember(nameof(NonGAReasonType_ReadOnly))]
		[BusinessObjectTestExclude]
		public ZString NonGAReasonType
		{
			get
			{
				var result = ZString.Empty;
				if (!(CSI_Procedure.IsEmpty || CSI_SubType.IsEmpty || CSI_Status.IsEmpty))
				{
					result = CSI_Procedure + CSI_SubType + CSI_Status;
				}
				return result;
			}
			set
			{
				CheckMaximumLength(NonGAReasonTypeInfo, value);

				CSI_Status = value.Length == Schema.NonGAReasonTypeMaxLength ? value.Right(2) : ZString.Empty;

				NonGAReasonTypeInfo.RefreshBinding();
			}
		}
		bool NonGAReasonType_ReadOnly => CSI_Procedure.IsEmpty || CSI_SubType.IsEmpty || RequirementTypeCodeList.IsGADeclarationRequired(CSI_SubType);
		public ZPropertyInfo NonGAReasonTypeInfo => GetZPropertyInfo(nameof(NonGAReasonType));

		[ResourceStringData("B1F519B8-9C66-4A9F-AEDA-DD3271270367", Caption = "Non-GA Mandatory Doc.")]
		public ZString ExportNonGAMandatoryDocument => GetNonGAReasonCode(Messaging.Constants.ZZ.NKCodeType.ENGAR)?.GetAttribute(Messaging.Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode) ?? ZString.Empty;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result = null;
			if (Parent?.IsExport ?? ZBool.False)
			{
				result = new EXPGAApprovalValidation(this);
			}
			else if (Parent?.IsImport ?? ZBool.False)
			{
				result = new IMPGAApprovalValidation(this);
			}
			else
			{
				result = new GAApprovalValidation(this);
			}

			return result;
		}

		public new GAApprovalValidation Validation => (GAApprovalValidation)base.Validation;
		public new GAApprovalLookups Lookups => (GAApprovalLookups)base.Lookups;
		public new ILineOrProduct Parent => base.Parent as ILineOrProduct;
		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new GAApprovalLookups(this);
		}

		internal void UpdateGAApprovalData(GAApproval gAApprovalData)
		{
			using (new DefaultValueSuspender(this))
			{
				CSI_SubType = gAApprovalData.CSI_SubType;
				CSI_Code = gAApprovalData.CSI_Code;
				CSI_Description = gAApprovalData.CSI_Description;
				CSI_Status = gAApprovalData.CSI_Status;
				CSI_AdditionalDescription = gAApprovalData.CSI_AdditionalDescription;
			}
		}

		bool IsDefaultValueSuspended => defaultValueSuspenderIndex > 0;

		public IDisposable GetDefaultValueSuspender() => new DefaultValueSuspender(this);

		class DefaultValueSuspender : IDisposable
		{
			public DefaultValueSuspender(GAApproval gaApproval)
			{
				this.gaApproval = gaApproval;
				checked
				{
					this.gaApproval.defaultValueSuspenderIndex++;
				}
			}

			readonly GAApproval gaApproval;

			public void Dispose()
			{
				checked
				{
					gaApproval.defaultValueSuspenderIndex--;
				}
			}
		}
		int defaultValueSuspenderIndex;

		public ZZRefCusCodeListCombined GetNonGAReasonCode(string type)
		{
			return MessageFunctions.GetRefCusCodeList(Factory, NonGAReasonType, type);
		}

		public IReadOnlyList<string> MultipleKeysToUse => new string[] { (Parent as JobComInvoiceLine)?.Declaration.JE_MessageType };
	}
}
