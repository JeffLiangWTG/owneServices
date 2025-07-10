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
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[CodeProperty(CusStatementHeader.Schema.B2_StatementNumber), DescriptionProperty(CusStatementHeader.Schema.B2_StatementNumber)]
	public class CusStatementHeader : BaseCusStatementHeader, IControllerIDProvider, IEDIMessageCollectionProvider, IDocumentSupportable, ISupportMultipleResourceStringData
	{
		public CusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusStatementHeader.Schema
		{
			public const string Remarks = "Remarks";
			public const int RemarksMaxLength = 60;
			public const string PayerFromCustoms = "PayerFromCustoms";
			public const int PayerFromCustomsMaxLength = 100;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == Schema.B2_OH_Importer
				|| (property.Name == Schema.B2_ProcessPort && B2_StatementType == StatementHeaderTypeList.Codes.CustomsDisbursementBill))
			{
				return false;
			}
			return true;
		}

		#endregion

		[ChildEditable(true)]
		public CusStatementLineCollection StatementLines
		{
			get
			{
				if (statementLines == null)
				{
					statementLines = new CusStatementLineCollection(this);
					RegisterEditableChildObject(statementLines);
				}
				return statementLines;
			}
		}
		CusStatementLineCollection statementLines;
		public CusStatementLine FirstLine => StatementLines.Count > 0 ? StatementLines.OrderBy(x => x.B3_SequenceNumber).FirstOrDefault() : null;

		protected override ZString HumanReadableNameCore => Res.GetString("BCF7B9D9-7FBC-46F0-89AD-6D15E441BCB6", "Statement {0}", B2_StatementNumber.TrimEnd());
		protected override Customs.Business.CusStatementHeaderLookups GetNewLookups() => new CusStatementHeaderLookups(this);
		public new CusStatementHeaderLookups Lookups => (CusStatementHeaderLookups)base.Lookups;

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.ImportersList))]
		[ResourceStringData("0E0697BE-20C7-40B4-A3D8-370D8FDD059C", Caption = "Payer")]
		public override ZGuid B2_OH_Importer
		{
			get { return base.B2_OH_Importer; }
			set { base.B2_OH_Importer = value; }
		}

		[ResourceStringData("FE5385BF-2F95-44F2-9D28-F8026D9B1BE0", Caption = "Period From")]
		public ZString PeriodFrom
		{
			get
			{
				var result = ZString.Empty;
				if (B2_PeriodStartDate.IsValid)
				{
					result = B2_PeriodStartDate.ToString(Constants.DateFormatType.MonthYear);
				}
				return result;
			}
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusStatementHeader Load(ZString statementNumber, ZGuid companyPK, string statementType)
			{
				var statementHeader = Factory.LoadTop1<CusStatementHeader>(GetQuery(statementNumber, companyPK, statementType));
				return statementHeader;
			}

			public static ZQuery GetQuery(ZString statementNumber, ZGuid companyPK, string statementType)
			{
				ZQuery result = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
				result.AddToFilter(CusStatementHeaderSchema.B2_GC, companyPK);
				result.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementHeader);
			}
		}
		#endregion

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.CustomsStatement;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => Factory;

		[MaxLength(Schema.RemarksMaxLength)]
		public ZString Remarks
		{
			get => RemarksNote.Text;
			set => RemarksNote.SetNoteText(this, RemarksInfo, value);
		}

		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(Schema.Remarks);

		HiddenTextNote RemarksNote => remarks ?? (remarks = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description));
		HiddenTextNote remarks;

		[ResourceStringData("BC65D056-8F87-461F-A4E6-53A34BAF927F", Caption = "")]
		[MaxLength(Schema.PayerFromCustomsMaxLength)]
		public ZString PayerFromCustoms
		{
			get => PayerFromCustomsNote.Text;
			set => PayerFromCustomsNote.SetNoteText(this, PayerFromCustomsNoteInfo, value);
		}

		public ZPropertyInfo PayerFromCustomsNoteInfo => GetZPropertyInfo(Schema.PayerFromCustoms);

		HiddenTextNote PayerFromCustomsNote => payerFromCustomsNote ?? (payerFromCustomsNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description));
		HiddenTextNote payerFromCustomsNote;

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new CusStatementHeaderDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		public ZString UNIPASSDeclarantID
		{
			get
			{
				if (Company != null)
				{
					return KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithFallbackDefault(Company.PK.ToGuid(), Guid.Empty, Guid.Empty).ToString();
				}
				return ZString.Empty;
			}
		}
		public ZString FormattedTaxInvoiceNumber => B2_StatementType == StatementHeaderTypeList.Codes.IndividualCollectionReceipt || B2_StatementType == StatementHeaderTypeList.Codes.MonthlyReceipt ? B2_StatementNumber.SubstringSafe(5) : ZString.Empty;
		public ZString FormattedCustomsOfficeBusinessNumber => MessageFunctions.GetFormattedNumber(CustomsOffice?.GetAttribute(Constants.ZZ.CodeListAttributeNames.BusinessNumber) ?? ZString.Empty, new int[] { 0, 3, 5 });

		[ResourceStringData("A62E039B-3210-4D31-9ECC-DBE91DC35553", Caption = "Payer Business No.")]
		public ZString FormattedImporterID
		{
			get
			{
				ZString result = B2_ImporterCustomsID;
				if (B2_ImporterCustomsID.Length == 10)
				{
					result = MessageFunctions.GetFormattedNumber(B2_ImporterCustomsID, new int[] { 0, 3, 5 });
				}
				else if (B2_ImporterCustomsID.Length == 13)
				{
					result = MessageFunctions.GetFormattedNumber(B2_ImporterCustomsID, new int[] { 0, 6 });
				}
				return result;
			}
		}
		public ZString FormattedPaymentNumber => B2_StatementType == StatementHeaderTypeList.Codes.Invoice ? MessageFunctions.GetFormattedNumber(B2_StatementNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 }) : ZString.Empty;

		[ResourceStringData("3B672E87-E168-40D5-B91D-A83A8A28D8D2", Caption = "Customs Disbursement Bill #")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.Statements))]
		public override ZString B2_StatementNumber { get => base.B2_StatementNumber; set => base.B2_StatementNumber = value; }

		[ResourceStringData("76B3D890-7F16-4352-AE4B-E34200F88FF7", Caption = "Customs Disbursement Bill #")]
		public ZString FormattedStatementNumber => MessageFunctions.GetFormattedNumberByStatementType(B2_StatementType, B2_StatementNumber);

		[ResourceStringData("c46dcff3-ef63-418e-8a8d-ee20730891e3", Caption = "Related Statement No.")]
		public ZString FormattedAccountNumber
		{
			get
			{
				var relatedStatementTypes = GetRelatedStatementTypes();
				var result = B2_AccountNo;
				if (relatedStatementTypes.Length > 0)
				{
					result = MessageFunctions.GetFormattedNumberByStatementType(relatedStatementTypes[0], B2_AccountNo);
				}
				return result;
			}
		}

		public CusStatementHeader RelatedStatement
		{
			get
			{
				if (relatedStatement == null)
				{
					var statementTypes = GetRelatedStatementTypes();

					if (statementTypes.Length > 0)
					{
						relatedStatement = GetRelatedStatement(statementTypes);
					}
				}
				return relatedStatement;
			}
		}
		CusStatementHeader relatedStatement;

		string[] GetRelatedStatementTypes()
		{
			string[] result = Array.Empty<string>();
			switch (B2_StatementType)
			{
				case StatementHeaderTypeList.Codes.Normal:
					result = new string[] { StatementHeaderTypeList.Codes.NormalReport };
					break;

				case StatementHeaderTypeList.Codes.NormalReport:
					result = new string[] { StatementHeaderTypeList.Codes.Normal };
					break;

				case StatementHeaderTypeList.Codes.Invoice:
					result = new string[] { StatementHeaderTypeList.Codes.MonthlyReceipt, StatementHeaderTypeList.Codes.IndividualCollectionReceipt };
					break;

				case StatementHeaderTypeList.Codes.MonthlyReceipt:
				case StatementHeaderTypeList.Codes.IndividualCollectionReceipt:
					result = new string[] { StatementHeaderTypeList.Codes.Invoice };
					break;
			}
			return result;
		}
		CusStatementHeader GetRelatedStatement(string[] statementType)
		{
			CusStatementHeader result = null;
			if (!B2_AccountNo.IsEmpty && statementType.Length > 0)
			{
				ZQuery query = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, B2_AccountNo);
				query.AddToFilter(CusStatementHeaderSchema.B2_GC, B2_GC);
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);

				result = Factory.LoadTop1<CusStatementHeader>(query);
			}
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.Statements))]
		[ResourceStringData("A72E1CA4-0BF6-4DDA-81BE-63D8593E88A7", Caption = "Bill Statement No.")]
		[ResourceStringData("BD7030DD-26A4-4416-9958-5CD2BACC70CB", Caption = "Invoice Statement No.", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		[ResourceStringData("58466237-4198-44E9-99D7-8E218F6C5334", Caption = "Invoice Statement No.", MultipleKey = StatementHeaderTypeList.Codes.Normal)]
		public override ZString B2_AccountNo
		{
			get => base.B2_AccountNo;
			set => base.B2_AccountNo = value;
		}

		[ResourceStringData("DB5ECB59-CF29-4FC5-82FF-B35C0C42033D", Caption = "Bill Process Date", FullDescription = "Invoice Document Process Date")]
		[ResourceStringData("A1C2A976-9569-4882-8465-B64D777F7A83", Caption = "Invoice Process Date", FullDescription = "Invoice Document Receive Date", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		[ResourceStringData("F9C5324C-1E23-40DB-B141-E8A839E63414", Caption = "Invoice Process Date", FullDescription = "Invoice Document Receive Date", MultipleKey = StatementHeaderTypeList.Codes.Normal)]
		public ZDateTime RelatedProcessDate => RelatedStatement?.B2_ProcessDate ?? ZDateTime.Empty;

		#region VAT
		internal const int VATMaxLength = 11;
		internal const int BaseAmountMaxLength = 13;

		[DecimalPlaces(0)]
		[ResourceStringData("265BD153-AC51-4EB8-A197-8CB9A53657F8", Caption = "Total Customs Value")]
		public ZDecimal TotalVATBaseAmountForVATReport
		{
			get
			{
				if (B2_StatementType == StatementHeaderTypeList.Codes.IndividualCollectionReceipt || B2_StatementType == StatementHeaderTypeList.Codes.MonthlyReceipt)
				{
					if (totalVATBaseAmount == ZDecimal.Zero)
					{
						totalVATBaseAmount = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.ValueForVAT);
					}
					return totalVATBaseAmount;
				}
				return 0;
			}
		}
		ZDecimal totalVATBaseAmount;

		ZString TotalVATBaseAmountString => TotalVATBaseAmountForVATReport.ToString(0);
		public ZInt TotalVATBaseAmountBlankCount => BaseAmountMaxLength - (TotalVATBaseAmountForVATReport.IsEmpty ? 0 : TotalVATBaseAmountString.Length);
		public ZString[] TotalVATBaseAmountDigits => totalVATBaseAmountDigits ?? (totalVATBaseAmountDigits = MessageFunctions.GetIndividualDigits(TotalVATBaseAmountString, TotalVATBaseAmountBlankCount - 1));
		ZString[] totalVATBaseAmountDigits;

		public ZDecimal TotalVAT
		{
			get
			{
				if (!totalVAT.HasValue)
				{
					if (B2_StatementType == StatementHeaderTypeList.Codes.IndividualCollectionReceipt || B2_StatementType == StatementHeaderTypeList.Codes.MonthlyReceipt)
					{
						totalVAT = StatementLines.GetTotalCustomsFeesTotal();
					}
					else
					{
						totalVAT = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.VAT);
					}
				}
				return totalVAT.Value;
			}
		}
		ZDecimal? totalVAT;

		ZString TotalVATString => TotalVAT.ToString(0);
		public ZString[] TotalVATDigits => totalVATDigits ?? (totalVATDigits = MessageFunctions.GetIndividualDigits(TotalVATString, VATMaxLength - (TotalVAT.IsEmpty ? 0 : TotalVATString.Length)));
		ZString[] totalVATDigits;
		#endregion

		public ZDecimal TotalDutyAmount => totalDutyAmount ?? (totalDutyAmount = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.Duty)).Value;
		ZDecimal? totalDutyAmount;

		public ZDecimal TotalLiquorTax => totalLiquorTax ?? (totalLiquorTax = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.LiquorTax)).Value;
		ZDecimal? totalLiquorTax;

		public ZDecimal TotalAgricultureTax => totalAgricultureTax ?? (totalAgricultureTax = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.AgricultureTax)).Value;
		ZDecimal? totalAgricultureTax;

		public ZDecimal TotalTransportationTax => totalTransportationTax ?? (totalTransportationTax = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.TransportationTax)).Value;
		ZDecimal? totalTransportationTax;

		public ZDecimal TotalEducationTax => totalEducationTax ?? (totalEducationTax = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.EducationTax)).Value;
		ZDecimal? totalEducationTax;

		public ZDecimal TotalPenaltyAndInterest => totalPenaltyAndInterest ?? (totalPenaltyAndInterest = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.PenaltyAndInterest)).Value;
		ZDecimal? totalPenaltyAndInterest;

		public ZDecimal TotalSpecialConsumptionTax => totalSpecialConsumptionTax ?? (totalSpecialConsumptionTax = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.SpecialConsumptionTax)).Value;
		ZDecimal? totalSpecialConsumptionTax;

		public ZDecimal TotalPenaltyForLatePayment => totalPenaltyForLatePayment ?? (totalPenaltyForLatePayment = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration)).Value;
		ZDecimal? totalPenaltyForLatePayment;

		public ZDecimal TemporaryOpeningFee => temporaryOpeningFee ?? (temporaryOpeningFee = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.DIF)).Value;
		ZDecimal? temporaryOpeningFee;

		public ZDecimal InspectionFee => inspectionFee ?? (inspectionFee = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.PAF)).Value;
		ZDecimal? inspectionFee;

		public ZDecimal PermissionApplicationFee => permissionApplicationFee ?? (permissionApplicationFee = StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.TOF)).Value;
		ZDecimal? permissionApplicationFee;

		[ResourceStringData("631518BB-F0D8-46A0-A915-258102C09A95", Caption = "Payer Company Name")]
		public ZString PayerCompanyName => Importer?.OH_FullName ?? ZString.Empty;

		#region Payer / Customs

		public OrgHeaderWrapper Payer => payer ??= OrgHeaderWrapper.New(Importer);
		OrgHeaderWrapper payer;
		public OrgAddress PayerAddress
		{
			get
			{
				if (Importer != null && payerAddress == null)
				{
					payerAddress = Importer.CustomsAddress ?? Importer.MainAddress;
				}
				return payerAddress;
			}
		}
		OrgAddress payerAddress;
		public ZString PayerRepresentativeName => Importer?.GetRepresentativeName() ?? ZString.Empty;

		public ZString PayerAddressDetails
		{
			get
			{
				var result = ZString.Empty;
				if (PayerAddress != null)
				{
					result = PayerAddress.Address1 + " " + PayerAddress.Address2;
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombined CustomsOffice => customs ?? (customs = MessageFunctions.GetRefCusCodeList(Factory, B2_ProcessPort, Constants.ZZ.NKCodeType.CustomsOffice));
		ZZRefCusCodeListCombined customs;

		[ResourceStringData("784EC15E-A2A8-4315-89DC-F41DA9CBB9EE", Caption = "Customs Office Name")]
		public ZString CustomsOfficeName => CustomsOffice?.ZZD_Description ?? ZString.Empty;
		public ZString CustomsAddress => CustomsOffice?.GetAttribute(Constants.ZZ.CodeListAttributeNames.Address) ?? ZString.Empty;

		[ResourceStringData("A111CB1A-A286-4C29-9C52-1FE7AE689A76", Caption = "Customs Account ID")]
		public ZString BankAccountID => CustomsOffice?.GetAttribute(Constants.ZZ.CodeListAttributeNames.BankAccountID) ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.CustomsOfficeList))]
		[ResourceStringData("FA91A30E-9406-4C4F-9C8F-8293D37E090C", Caption = "Customs Office")]
		public override ZString B2_ProcessPort
		{
			get { return base.B2_ProcessPort; }
			set { base.B2_ProcessPort = value; }
		}

		#endregion

		public ZString KoreanCustomsID => B2_StatementType == StatementHeaderTypeList.Codes.Invoice ? B2_StatementNumber.Substring(0, 4) : ZString.Empty;
		public ZString FormattedPaymentReferenceNumber
		{
			get
			{
				ZString result = B2_StatementNumber;
				if (B2_StatementType == StatementHeaderTypeList.Codes.Invoice)
				{
					result = MessageFunctions.GetFormattedNumber(B2_StatementNumber, new int[] { 4, 7, 9, 11, 12, 18 });
				}
				return result;
			}
		}

		[DecimalPlaces(0)]
		[ResourceStringData("1C2B5EFB-86BF-4F4C-9C61-7840B766BA11", Caption = "Total Amount After Due Date")]
		public ZDecimal OverdueAmount => overdueAmount ?? (overdueAmount = GetOverdueAmount()).Value;
		ZDecimal? overdueAmount;

		ZDecimal GetOverdueAmount()
		{
			var result = 0m;
			if (B2_DueDate.IsValid)
			{
				var assessmentDate = B2_DueDate.AddDays(1);

				foreach (var line in StatementLines)
				{
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.DutyAmount);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.LiquorTax);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.AgricultureTax);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.TransportationTax);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.EducationTax);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.SpecialConsumptionTax);
					result += Constants.PenaltyCalculationConstants.CalculateLatePaymentInterestAmountAndTruncate(line.VAT);
					result += Constants.PenaltyCalculationConstants.TruncateAmount(line.PenaltyAndInterest);
					result += Constants.PenaltyCalculationConstants.TruncateAmount(line.PenaltyForLatePayment);

					if (B2_StatementAmount >= Constants.PenaltyCalculationConstants.ThresholdAmountExemptForOverduePenalty)
					{
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.DutyAmount, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.LiquorTax, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.AgricultureTax, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.TransportationTax, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.EducationTax, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.SpecialConsumptionTax, assessmentDate);
						result += this.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, line.VAT, assessmentDate);
					}
				}
			}
			return result;
		}

		public ZString BrokerDeclarantID
		{
			get
			{
				ZString result = ZString.Empty;
				if (FirstLine != null)
				{
					if (B2_StatementType == StatementHeaderTypeList.Codes.Invoice || B2_StatementType == StatementHeaderTypeList.Codes.MonthlyReceipt || B2_StatementType == StatementHeaderTypeList.Codes.IndividualCollectionReceipt)
					{
						result = FirstLine.B3_EntryNum.SubstringSafe(0, 5);
					}
				}
				return result;
			}
		}

		public ZString ImportCustomsOfficeName => importCustomsOfficeName ?? (importCustomsOfficeName = MessageFunctions.GetCustomsOffice(Factory, B2_ProcessPort)).Value;
		ZString? importCustomsOfficeName;

		public EDIMessageWrapperCollection MessageWrapper => messagesWrapper ?? (messagesWrapper = new EDIMessageWrapperCollection(Messages.Cast<EDIMessage>(), Factory));
		EDIMessageWrapperCollection messagesWrapper;

		[ResourceStringData("FE061745-1FC7-4315-9944-1F2DB24D1D77", Caption = "Process Date", FullDescription = "Document Receive Date")]
		[ResourceStringData("0A500320-815D-4754-826F-1857911155B8", Caption = "Process Date", FullDescription = "Document Process Date", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		[ResourceStringData("9232C167-A9FA-443F-8DB5-6A9098BC69D0", Caption = "Process Date", FullDescription = "Document Process Date", MultipleKey = StatementHeaderTypeList.Codes.Normal)]
		public override ZDateTime B2_ProcessDate
		{
			get { return base.B2_ProcessDate; }
			set { base.B2_ProcessDate = value; }
		}

		[ResourceStringData("C822070B-6B2A-4813-A345-B84745816613", Caption = "Due Date")]
		public override ZDateTime B2_DueDate
		{
			get { return base.B2_DueDate; }
			set { base.B2_DueDate = value; }
		}

		[ResourceStringData("63E9E7BE-7A34-402D-8381-2045158EA0BB", Caption = "Payment Date")]
		public override ZDateTime B2_PaymentAuthorizationDate
		{
			get { return base.B2_PaymentAuthorizationDate; }
			set { base.B2_PaymentAuthorizationDate = value; }
		}

		[ResourceStringData("B8B68C03-6D64-4BE2-90E7-FDD663903D63", Caption = "Total Amount")]
		[DecimalPlaces(0)]
		public override ZDecimal B2_StatementAmount
		{
			get { return base.B2_StatementAmount; }
			set { base.B2_StatementAmount = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentStatusList))]
		[ResourceStringData("3A2CA62B-749F-429F-B614-F5D69982DD97", Caption = "Status")]
		[ResourceStringData("5AC93387-9BC4-4BB6-82E6-69EF05174434", Caption = "Payment Status", MultipleKey = StatementHeaderTypeList.Codes.CustomsDisbursementBill)]
		public override ZString B2_PaymentStatus
		{
			get { return base.B2_PaymentStatus; }
			set { base.B2_PaymentStatus = value; }
		}
		[ResourceStringData("D2237D75-E174-4C78-B440-718518023BC7", Caption = "Payment Status Name")]
		public ZString B2_PaymentStatusName => Factory.GetCachedValue<StatementHeaderPaymentStatusList>().GetDescriptionFromCode(B2_PaymentStatus);

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.StatementTypeList))]
		[ResourceStringData("D9F9173D-CDBD-4000-AA5B-C5621C824966", Caption = "Statement Type")]
		public override ZString B2_StatementType
		{
			get { return base.B2_StatementType; }
			set { base.B2_StatementType = value; }
		}
		[ResourceStringData("9DCD6E71-D0A6-40D2-A120-1317840C3205", Caption = "Statement Type Name")]
		public ZString B2_StatementTypeName => Factory.GetCachedValue<StatementHeaderTypeList>().GetDescriptionFromCode(B2_StatementType);

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentPartyList))]
		[ResourceStringData("0650DE26-074D-45DC-BF79-5E4CBD292AC7", Caption = "Payment Party")]
		public override ZString B2_PaymentParty
		{
			get { return base.B2_PaymentParty; }
			set { base.B2_PaymentParty = value; }
		}
		[ResourceStringData("5A1A3E20-7031-41D0-895C-A8CE3C933E3F", Caption = "Payment Party Name")]
		public ZString B2_PaymentPartyName => Factory.GetCachedValue<PaymentPartyList>().GetDescriptionFromCode(B2_PaymentParty);

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentTypeList))]
		[ResourceStringData("A20BBC12-6FCD-45C6-B1E3-72D8257FAB35", Caption = "Payment Type")]
		public override ZString B2_PaymentType
		{
			get { return base.B2_PaymentType; }
			set { base.B2_PaymentType = value; }
		}
		[ResourceStringData("E5D89AFB-5AD2-4084-8A81-24A437EDF30B", Caption = "Payment Type Name")]
		public ZString B2_PaymentTypeName => Factory.GetCachedValue<StatementTypeList>().GetDescriptionFromCode(B2_PaymentType);

		[ResourceStringData("8674EA67-D3DE-4ED4-95C8-33B4B636D883", Caption = "Period (From)")]
		public override ZDate B2_PeriodStartDate
		{
			get { return base.B2_PeriodStartDate; }
			set { base.B2_PeriodStartDate = value; }
		}

		[ResourceStringData("DEB75D76-0795-46E9-9F34-26776A177066", Caption = "Period (To)")]
		public override ZDate B2_PeriodEndDate
		{
			get { return base.B2_PeriodEndDate; }
			set { base.B2_PeriodEndDate = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.BillTypeList))]
		[ResourceStringData("A6ED706B-CEEB-48D7-B851-9345B7867ACA", Caption = "Bill Type")]
		public override ZString B2_Status { get => base.B2_Status; set => base.B2_Status = value; }

		[ResourceStringData("A155BE25-A3A2-41A2-9739-33B8B96C7FC0", Caption = "Bill Type Name")]
		public ZString B2_StatusName => Factory.GetCachedValue<StatementHeaderStatusList>().GetDescriptionFromCode(B2_Status);

		[ResourceStringData("F8BFA32A-1CD1-4A25-BD2A-7F3F6962F21D", Caption = "Issue Date")]
		public override ZDateTime B2_PrintDate { get => base.B2_PrintDate; set => base.B2_PrintDate = value; }

		[ResourceStringData("E0AF53AA-F3DC-41E4-B753-A2C0D796AB62", Caption = "Created Time (Local)")]
		public ZDateTime CreateDateTimeInLocalTimeZone
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (B2_SystemCreateTimeUtc.IsValid)
				{
					result = Env.Time.GetLocalTimeFromUtc(B2_SystemCreateTimeUtc.ToDateTime());
				}
				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusStatementHeaderFetchStrategy(this);

		[ResourceStringData("C9A25FEB-3FF6-4910-841C-E868A17AE942", Caption = "Total Amount")]
		[ResourceStringData("B7DCC773-B6E7-4DA1-937C-F129E712259A", Caption = "Total Amount (VAT)", MultipleKey = StatementHeaderTypeList.Codes.MonthlyReceipt)]
		[ResourceStringData("DC6C1009-30D3-447E-BF73-0B4C35A3758D", Caption = "Total Amount (VAT)", MultipleKey = StatementHeaderTypeList.Codes.IndividualCollectionReceipt)]
		[ResourceStringData("4B3C68B2-FFE7-46BC-A76F-EC4AED3988E1", Caption = "Total Due Amount", MultipleKey = StatementHeaderTypeList.Codes.CustomsDisbursementBill)]
		[DecimalPlaces(0)]
		public ZDecimal TotalAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (B2_StatementType == StatementHeaderTypeList.Codes.MonthlyReceipt || B2_StatementType == StatementHeaderTypeList.Codes.IndividualCollectionReceipt)
				{
					result = TotalVAT;
				}
				else
				{
					result = B2_StatementAmount;
				}

				return result;
			}
		}

		public ZString B2_IncomingMessageNo
		{
			get { return base.B2_CheckNo; }
			set { base.B2_CheckNo = value; }
		}

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { B2_StatementType };
	}
}
