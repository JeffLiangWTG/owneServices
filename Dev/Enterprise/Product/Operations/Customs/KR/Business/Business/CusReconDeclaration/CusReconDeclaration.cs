using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconDeclaration : AutoKRCusReconDeclaration,
		Integration.Customs.KR.ICusReconDeclaration,
		IControllerIDProvider,
		IEDIMessageCollectionProviderWithID,
		ISequenceNumberHeader
	{
		public CusReconDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new class Schema : AutoCusReconDeclaration.Schema
		{
			public new const int CRD_DeclarationTypeMaxLength = 1;
			public const int KR_RefundCauseMaxLength = 2;
			public const int KR_RefundReasonMaxLength = 2;
			public const int CustomsOfficeMaxLength = 3;
			public const int KR_CustomsDivisionMaxLength = 2;
			public const int KR_TaxOfficeMaxLength = 3;
		}
		public bool IsRefundTypeContractRevocation => CRD_DeclarationType == RefundTypeList.Codes.B;

		[ChildEditable(true)]
		public CusReconEntryLineCollection CusReconEntryLines
		{
			get
			{
				if (cusReconEntryLines == null)
				{
					cusReconEntryLines = new CusReconEntryLineCollection(this);
					cusReconEntryLines.Load();
					RegisterEditableChildObject(cusReconEntryLines);
				}
				return cusReconEntryLines;
			}
		}
		CusReconEntryLineCollection cusReconEntryLines;

		ShortSequenceNumberGenerator shortSequenceNumberGenerator;
		ShortSequenceNumberGenerator ShortSequenceNumberGenerator => shortSequenceNumberGenerator ??= new ShortSequenceNumberGenerator(this);

		public void CalculateEntrySequenceNumberOnAdded(CusReconEntry entry)
		{
			if (CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CRD_MessageStatus))
			{
				ShortSequenceNumberGenerator.RecalculateWhenAdded(entry);
			}
		}

		public void RecalculateEntrySequenceNumberOnDeleted(CusReconEntry entry)
		{
			if (CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CRD_MessageStatus))
			{
				ShortSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(entry);
			}
		}

		protected override Customs.Business.CusReconEntryCollection CreateNewCusReconEntryCollection()
		{
			return new CusReconEntryCollection(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				if (CRD_JobReferenceNumber.IsEmpty)
				{
					var seed = Core.Constants.CountryCodes.KoreaSouth + CusReconDeclarationSchema.Constants.Prefix;
					var fountainNumber = Env.NumberFountains.KRNumberFountain(seed, NumberFountainMaxValues._8digit).GetNextFormatted(Factory);
					CRD_JobReferenceNumber = CusReconDeclarationSchema.Constants.Prefix + ZInt.ParseSafe(fountainNumber, 0).ToString(Constants.NumberFormatDigit.D8);
				}
				if (CusEntryNumber5UL == null)
				{
					Create5ULEntryNumber();
				}
				if (RefundDeclarationNumber.IsEmpty)
				{
					CusEntryNumber5UL.Generate5ULEntryNumber(Branch.GB_GC);
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				CRD_JobReferenceNumber = ZString.Empty;
				CusEntryNumber5UL.CE_EntryNum = ZString.Empty;
			}
		}

		CusEntryNumber CusEntryNumber5UL => cusEntryNumber5UL ??= GetEntryNumber(ElectronicDocumentTypeList.Codes._5UL);
		CusEntryNumber cusEntryNumber5UL;

		CusEntryNumber CusEntryNumber5UO => cusEntryNumber5UO ??= GetEntryNumber(ElectronicDocumentTypeList.Codes._5UO);
		CusEntryNumber cusEntryNumber5UO;

		CusEntryNumber CusEntryNumber5UN => cusEntryNumber5UN ??= GetEntryNumber(ElectronicDocumentTypeList.Codes._5UN);
		CusEntryNumber cusEntryNumber5UN;

		[ResourceStringData("51094174-A7EA-430F-849A-CFBAF234EB8D", Caption = "Entry Number")]
		public ZString RefundDeclarationNumber => CusEntryNumber5UL?.CE_EntryNum ?? ZString.Empty;

		[ResourceStringData("B39A8DEF-488C-4B8F-B39E-2871B8332678", Caption = "Entry Number")]
		public ZString FormattedRefundDeclarationNumber => MessageFunctions.DeclarationNumberFormat(RefundDeclarationNumber);

		[DecimalPlaces(DecimalPlacesConstants.TotalRefundAmount)]
		[ResourceStringData("F10F1131-128E-4CC4-A351-4FFD4E18DF45", Caption = "Total Refund Amount")]
		public ZDecimal TotalRefundAmount => CusReconEntryLines.Sum(x => x.TotalRefundAmount);

		[ResourceStringData("324E5E44-2052-43B7-B550-4E8F9BDE4B39", ShortCaption = "Msg. Status Desc.", Caption = "Message Status Description")]
		public override ZString MessageStatusDescription => base.MessageStatusDescription;

		[ResourceStringData("0F120F52-CD15-4174-83D4-BA720EFD7BC7", ShortCaption = "Entry Status Desc.", Caption = "Entry Status Description")]
		public ZString EntryStatusDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(CRD_CustomsStatus);

		[ResourceStringData("46CCF8B7-F676-4F80-924C-B79C8244030D", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate => CusEntryNumber5UL?.CE_IssueDate ?? ZDateTime.Empty;

		[ResourceStringData("1C72699A-E8EB-4E8C-B8D5-A084E9BCD213", Caption = "Refund Approval Date")]
		public ZDateTime RefundApprovalDate => CusEntryNumber5UO?.CE_IssueDate ?? ZDateTime.Empty;

		[ResourceStringData("89DC6DC4-AC5D-4525-BDE1-99C62183ABE4", MediumCaption = "Refund Approval No.", Caption = "Refund Approval Number")]
		public ZString RefundApprovalNumber => CusEntryNumber5UO?.CE_EntryNum ?? ZString.Empty;

		[ResourceStringData("5187BF41-82DB-494C-A1B7-903530D792C5", Caption = "Refund Bill Count")]
		public ZInt RefundBillCount => CusReconEntryLines.Count;

		[ResourceStringData("B4E5D346-3D4F-47F1-975C-5B84F018895D", Caption = "Payer Company Name")]
		public ZString PayerCompanyName => DeclarantAddress?.CompanyName ?? ZString.Empty;

		[ResourceStringData("9002A608-D4B2-4B06-9318-89A09BDF7A5A", Caption = "Branch Code")]
		public override ZGuid CRD_GB_Branch { get => base.CRD_GB_Branch; set => base.CRD_GB_Branch = value; }

		[MaxLength(Schema.CustomsOfficeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.CustomsOfficeList))]
		public override ZString CRD_CustomsOffice { get => base.CRD_CustomsOffice; set => base.CRD_CustomsOffice = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.EntryStatusList))]
		[ResourceStringData("8B878DD5-5CA9-420F-8C93-3DCBC7BAA25E", Caption = "Entry Status")]
		public override ZString CRD_CustomsStatus { get => base.CRD_CustomsStatus; set => base.CRD_CustomsStatus = value; }

		[ResourceStringData("4176F041-B318-4BFD-929C-16B5E04481B8", Caption = "Payer")]
		public override ZGuid CRD_OA_DeclarantAddress { get => base.CRD_OA_DeclarantAddress; set => base.CRD_OA_DeclarantAddress = value; }

		[ResourceStringData("F44ADE7E-CA3E-42C1-978F-5574E6C42C96", Caption = "Customs Office Name")]
		public new ZString OfficeDescription => base.OfficeDescription;

		[ResourceStringData("7BCA4417-BEF8-4B1F-9F35-6D310CC68A73", Caption = "Provision Date")]
		public ZDateTime ProvisionDate => CusEntryNumber5UN?.CE_IssueDate ?? ZDateTime.Empty;

		[ResourceStringData("4A4DE5D7-0E8A-40FE-AAE4-D235B59726EB", MediumCaption = "Provision No.", Caption = "Provision Number")]
		public ZString ProvisionNumber => CusEntryNumber5UN?.CE_EntryNum ?? ZString.Empty;

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.MessageStatusList))]
		public override ZString CRD_MessageStatus { get => base.CRD_MessageStatus; set => base.CRD_MessageStatus = value; }

		[MaxLength(Schema.CRD_DeclarationTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.RefundTypeList))]
		[ResourceStringData("807E8187-639D-49E3-AC3F-8483680DA83D", Caption = "Refund Type")]
		public override ZString CRD_DeclarationType
		{
			get => base.CRD_DeclarationType;
			set
			{
				var oldValue = base.CRD_DeclarationType;
				base.CRD_DeclarationType = value;
				if (oldValue != CRD_DeclarationType)
				{
					if (CRD_DeclarationType == RefundTypeList.Codes.B)
					{
						foreach (var cusReconEntryLine in CusReconEntryLines)
						{
							if (cusReconEntryLine.ContractRevocation == null)
							{
								cusReconEntryLine.ContractRevocations.AddNew();
							}
						}
					}
					else
					{
						foreach (var cusReconEntryLine in CusReconEntryLines)
						{
							cusReconEntryLine.ContractRevocations.RemoveAndDeleteAll();
						}
					}
				}
				CRD_DeclarationTypeInfo.RefreshBinding();
				CusReconEntryLines.RefreshBinding();
			}
		}

		[MaxLength(Schema.KR_RefundCauseMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.RefundCauseCodeList))]
		[ResourceStringData("F7475127-E3AF-4F24-9FA0-2A6E52B4CBBF", Caption = "Refund Cause")]
		public override ZString CRD_RefundCauseCode
		{
			get => base.CRD_RefundCauseCode;
			set
			{
				base.CRD_RefundCauseCode = value;
				if (!RefundCauseCodeList.IsRefundReasonCodeMandatory(value))
				{
					CRD_RefundReasonCode = ZString.Empty;
				}
			}
		}

		[MaxLength(Schema.KR_RefundReasonMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.RefundReasonCodeList))]
		[ResourceStringData("8E1C3C48-516F-4C47-8051-626B6CB32608", Caption = "Refund Reason")]
		public override ZString CRD_RefundReasonCode { get => base.CRD_RefundReasonCode; set => base.CRD_RefundReasonCode = value; }

		[MaxLength(Schema.KR_CustomsDivisionMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.CustomsDivisionList))]
		[ResourceStringData("C0991C20-FB7E-4AC6-8637-A54B4CBC7469", Caption = "Department")]
		public override ZString CRD_CustomsDivision { get => base.CRD_CustomsDivision; set => base.CRD_CustomsDivision = value; }

		[MaxLength(Schema.KR_TaxOfficeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.TaxOfficeList))]
		[ResourceStringData("30B7C1A1-1C2E-4821-8A0D-F52ADC381B50", Caption = "Tax Office")]
		public override ZString CRD_TaxOffice { get => base.CRD_TaxOffice; set => base.CRD_TaxOffice = value; }

		[MaxLength(Schema.CRD_GS_NKCustomsAgentMaxLength)]
		[ResourceStringData("96208E72-F163-42EA-84EA-A0C17EB8041B", Caption = "Broker")]
		public override ZString CRD_GS_NKCustomsAgent { get => base.CRD_GS_NKCustomsAgent; set => base.CRD_GS_NKCustomsAgent = value; }

		public OrgHeaderWrapper PayerWrapper => OrgHeaderWrapper.New(Payer);
		public OrgHeader Payer => DeclarantAddress?.Header;

		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.BankTypeList))]
		[ResourceStringData("0E25C9CA-D59E-49AA-A536-856EAEE41F68", Caption = "Payer Bank")]
		public ZString PayerBank => PayerWrapper?.ZO_BankCode ?? ZString.Empty;

		[ResourceStringData("50DB7D47-2A30-480F-9ACC-80C56664FEA1", MediumCaption = "Bank Account No.", Caption = "Bank Account Number")]
		public ZString BankAccountNumber => PayerWrapper?.ZO_BankAccNo ?? ZString.Empty;

		[ResourceStringData("F985B07A-1ED5-4AF6-A3C0-92B0F83DE6B8", MediumCaption = "Registration No.1", Caption = "Registration Number 1")]
		public ZString RegistrationNumberOne
		{
			get
			{
				var result = ZString.Empty;
				if (Payer != null)
				{
					if (Payer.GetIsIndividual())
					{
						result = Payer.CustomsCodes.GetCustomsRegNo(IdentificationType.KoreanRegNoForResident, Core.Constants.CountryCodes.KoreaSouth);
					}
					else
					{
						result = Payer.CustomsCodes.GetCustomsRegNo(IdentificationType.BusinessRegNo, Core.Constants.CountryCodes.KoreaSouth);
					}
				}
				return result;
			}
		}

		[ResourceStringData("EEFA5F67-81FE-4960-BD77-AE1494611FA1", MediumCaption = "Registration No.2", Caption = "Registration Number 2")]
		public ZString KoreanRegistrationNumberOfCEO
		{
			get
			{
				var result = ZString.Empty;
				if (Payer != null && !Payer.GetIsIndividual())
				{
					result = Payer.CustomsCodes.GetCustomsRegNo(IdentificationType.KoreanRegNoForResident, Core.Constants.CountryCodes.KoreaSouth);
				}
				return result;
			}
		}

		[ChildEditable(true)]
		public CusReconEntryNumCollection EntryNumbers
		{
			get
			{
				if (entryNumbers == null)
				{
					entryNumbers = new CusReconEntryNumCollection(this);
					entryNumbers.Load();
					RegisterEditableChildObject(entryNumbers);
				}

				return entryNumbers;
			}
		}
		CusReconEntryNumCollection entryNumbers;

		public bool IsRefundReasonCodeMandatory => RefundCauseCodeList.IsRefundReasonCodeMandatory(CRD_RefundCauseCode);

		CusEntryNumber GetEntryNumber(string entryType)
		{
			return EntryNumbers.GetCusEntryNumWithMaxVersionNumber(entryType);
		}

		CusEntryNumber Create5ULEntryNumber()
		{
			var cusEntryNumber = EntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			return cusEntryNumber;
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		public new CusReconDeclarationLookups Lookups => (CusReconDeclarationLookups)base.Lookups;

		protected override Customs.Business.CusReconBase.CusReconDeclarationLookups GetNewLookups() => new CusReconDeclarationLookups(this);
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusReconDeclarationFetchStrategy(this);
		protected override Customs.Business.CusReconBase.CusReconDeclarationValidation GetNewValidation() => new CusReconDeclarationValidation(this);
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
		}

		ZString IEDIMessageCollectionProviderWithID.IDNumber => RefundDeclarationNumber;
		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.KR.CusReconDeclaration;
		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();
		Enterprise.Messaging.Business.EDIMessageCollection Enterprise.Messaging.Business.IEDIMessageCollectionProvider.Messages => Messages;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(CusReconEntries);

		void IEDIMessageCollectionProviderWithID.MarkAsFailed()
		{
			var failedStatus = CustomsMessageStatusTypeList.GetErrorStatus(CRD_MessageStatus);
			if (!string.IsNullOrEmpty(failedStatus))
			{
				CRD_MessageStatus = failedStatus;
			}
		}

		public void OnCustomsBillNumbersSelected(KREntryCustomsBillsView[] entryCustomsBillsViews)
		{
			foreach (KREntryCustomsBillsView entryCustomsBillsView in entryCustomsBillsViews)
			{
				CusReconEntryLines.AddNewLine(entryCustomsBillsView);
			}
		}
	}
}
