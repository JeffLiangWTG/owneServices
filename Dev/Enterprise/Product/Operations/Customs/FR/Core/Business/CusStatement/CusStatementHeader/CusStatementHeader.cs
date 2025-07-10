using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	[CodeProperty(CusStatementHeader.Schema.B2_StatementNumber), DescriptionProperty(nameof(CusStatementHeader.HumanReadableName))]
	public class CusStatementHeader : BaseCusStatementHeader, ICorrelationIDProvider, IFRMessagesOwner, IBillGenerationSupport, IEDocsProvider
	{
		public CusStatementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : BaseCusStatementHeader.Schema
		{
			public const string EntryNumber = "EntryNumber";
			public const string CorrelationID = "CorrelationID";
			public const int CorrelationMaxLength = 10;
		}

		protected override Customs.Business.CusStatementHeaderValidation GetNewValidation() => new CusStatementHeaderValidation(this);

		protected override Customs.Business.CusStatementHeaderLookups GetNewLookups() => new CusStatementHeaderLookups(this);

		public new CusStatementHeaderLookups Lookups => (CusStatementHeaderLookups)base.Lookups;

		[ChildEditable(false)]
		public CusStatementEntryCollection Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new CusStatementEntryCollection(this);
					entries.Load();
					RegisterEditableChildObject(entries);
				}
				return entries;
			}
		}
		CusStatementEntryCollection entries;

		public CusStatementChargesDetail ChargesDetail
		{
			get
			{
				if (chargesDetail == null)
				{
					chargesDetail = CusStatementChargesDetail.LoadOrCreate(this);
					RegisterEditableChildObject(chargesDetail);
				}
				return chargesDetail;
			}
		}
		CusStatementChargesDetail chargesDetail;

		[ChildEditable(true)]
		public FREDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new FREDIMessageCollection(this);
					messages.Load();
					messages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}
		FREDIMessageCollection messages;

		[ReadOnly(true)]
		[ResourceStringData("FR.CusStatementHeader.B2_StatementNumber", Caption = "Statement Number")]
		public override ZString B2_StatementNumber { get => base.B2_StatementNumber; set => base.B2_StatementNumber = value; }

		[ReadOnly(true)]
		[ResourceStringData("FR.CusStatementHeader.EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber
		{
			get => ChargesDetail.B3_EntryNum;
			set => ChargesDetail.B3_EntryNum = value;
		}

		public ZPropertyInfo EntryNumberInfo { get { return GetWrappedZPropertyInfo(Schema.EntryNumber, x => ChargesDetail.B3_EntryNumInfo); } }

		[ReadOnly(true)]
		[ResourceStringData("FR.CusStatementHeader.B2_Status", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.StatusList))]
		public override ZString B2_Status { get => base.B2_Status; set => base.B2_Status = value; }

		[ResourceStringData("FR.CusStatementHeader.StatusDescription", Caption = "Status Description", ShortCaption = "Status Desc.")]
		public ZString StatusDescription => Lookups.StatusList.GetMultilingualDescriptionFromCode(B2_Status);

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_PaymentType", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.MethodOfPaymentList))]
		public override ZString B2_PaymentType { get => base.B2_PaymentType; set => base.B2_PaymentType = value; }

		[ResourceStringData("FR.CusStatementHeader.PaymentTypeDescription", Caption = "Payment Method Description", ShortCaption = "Payment Method Desc.")]
		public ZString PaymentTypeDescription => Lookups.MethodOfPaymentList.GetMultilingualDescriptionFromCode(B2_PaymentType);

		[ResourceStringData("FR.CusStatementHeader.B2_ProcessDate", Caption = "Date")]
		public override ZDateTime B2_ProcessDate { get => base.B2_ProcessDate; set => base.B2_ProcessDate = value; }

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_BranchDesignation", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.DirectionList))]
		public override ZString B2_BranchDesignation { get => base.B2_BranchDesignation; set => base.B2_BranchDesignation = value; }

		[ResourceStringData("FR.CusStatementHeader.BranchDesignationDescription", Caption = "Type Description", ShortCaption = "Type Desc.")]
		public ZString BranchDesignationDescription => Lookups.DirectionList.GetMultilingualDescriptionFromCode(B2_BranchDesignation);

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_StatementType", Caption = "Frequency")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PeriodicityList))]
		public override ZString B2_StatementType { get => base.B2_StatementType; set => base.B2_StatementType = value; }

		[ResourceStringData("FR.CusStatementHeader.StatementTypeDescription", Caption = "Frequency Description", ShortCaption = "Frequency Desc.")]
		public ZString StatementTypeDescription => Lookups.PeriodicityList.GetMultilingualDescriptionFromCode(B2_StatementType);

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_EntryFilerCode", Caption = "Profile")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.Profiles))]
		public override ZString B2_EntryFilerCode { get => base.B2_EntryFilerCode; set => base.B2_EntryFilerCode = value; }

		[ResourceStringData("FR.CusStatementHeader.B2_AccountNo", Caption = "Account")]
		public override ZString B2_AccountNo { get => base.B2_AccountNo; set => base.B2_AccountNo = value; }

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_ImporterCustomsID", Caption = "Representative ID")]
		public override ZString B2_ImporterCustomsID { get => base.B2_ImporterCustomsID; set => base.B2_ImporterCustomsID = value; }

		[ReadOnlyMember(nameof(HasEntryNumber))]
		[ResourceStringData("FR.CusStatementHeader.B2_OH_Importer", Caption = "Representative")]
		public override ZGuid B2_OH_Importer { get => base.B2_OH_Importer; set => base.B2_OH_Importer = value; }

		[ResourceStringData("FR.CusStatementHeader.ImporterFullName", Caption = "Representative")]
		public ZString ImporterFullName => Importer == null ? string.Empty : Importer.OH_Code + " " + Importer.OH_FullName;

		[ReadOnlyMember(nameof(HasEntryNumber))]
		public override ZDate B2_PeriodStartDate { get => base.B2_PeriodStartDate; set => base.B2_PeriodStartDate = value; }

		[ReadOnlyMember(nameof(HasEntryNumber))]
		public override ZDate B2_PeriodEndDate { get => base.B2_PeriodEndDate; set => base.B2_PeriodEndDate = value; }

		[ResourceStringData("FR.CusStatementHeader.B2_DueDate", Caption = "Due Date")]
		public override ZDateTime B2_DueDate { get => base.B2_DueDate; set => base.B2_DueDate = value; }

		[ReadOnlyMember(nameof(HasEntryNumberOrPaymentTypeIsNeitherRNOrM))]
		public override ZString B2_CheckNo { get => base.B2_CheckNo; set => base.B2_CheckNo = value; }

		public ZBool HasEntryNumber => !EntryNumber.IsEmpty;

		public ZBool HasEntryNumberOrPaymentTypeIsNeitherRNOrM => HasEntryNumber || (B2_PaymentType != MethodOfPaymentList.Codes.R && B2_PaymentType != MethodOfPaymentList.Codes.M);

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("FR.CusStatementHeader.HumanReadableName", "Liquidation {0}", B2_StatementNumber);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new CusStatementHeaderValueSetStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			B2_StatementType = ZString.Empty;
			B2_PaymentType = MethodOfPaymentList.Codes.R;
		}

		public ZString DeltaAgreementAccountRepresentativeID => RelatedCusAccount?.CZ_RepresentativeID ?? ZString.Empty;

		internal OrgCusAccount RelatedCusAccount
		{
			get
			{
				OrgCusAccount account = null;
				if (Importer != null)
				{
					if (B2_BranchDesignation == StatementEntryTypeImpExpList.Codes.Import)
					{
						account = Importer.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(x => x.CZ_Code == OrgCusAccountCodeList.Codes.DGI && x.CZ_Type == OrgCusAccountDeltaGTypeList.Codes.G2);
					}
					else if (B2_BranchDesignation == StatementEntryTypeList.Codes.Export)
					{
						account = Importer.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(x => x.CZ_Code == OrgCusAccountCodeList.Codes.DGE && x.CZ_Type == OrgCusAccountDeltaGTypeList.Codes.G2);
					}
				}
				return account;
			}
		}

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		[ResourceStringData("FR.CusStatementHeader.CorrelationID", Caption = "Reference Number")]
		public ZString CorrelationID
		{
			get { return ChargesDetail.B3_BrokerReference; }
			set { ChargesDetail.B3_BrokerReference = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => ChargesDetail.B3_BrokerReferenceInfo); } }

		CorrelationIDGenerator CorrelationIdGenerator => correlationIdGenerator ?? (correlationIdGenerator = new CorrelationIDGenerator(this, this));

		CorrelationIDGenerator correlationIdGenerator;

		public ZString CorrelationIDPrefix => ZString.Empty;

		#region  IBillGenerationSupport

		BusinessObjectFactory IBillGenerationSupport.Factory => Factory;

		OrgHeader IBillGenerationSupport.CarrierPrincipal => null;

		ZString IBillGenerationSupport.TransportMode => ZString.Empty;

		ZString IBillGenerationSupport.ServiceLevel => ZString.Empty;

		RefUNLOCO IBillGenerationSupport.Origin => null;

		RefUNLOCO IBillGenerationSupport.Destination => null;

		RefUNLOCO IBillGenerationSupport.Load => null;

		RefUNLOCO IBillGenerationSupport.Discharge => null;

		ZString IBillGenerationSupport.TranshipmentIndicator => ZString.Empty;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(B2_StatementNumberInfo, GetNewStatementNumber);
			CorrelationIdGenerator.InitCorrelationID(IsUniqueCorrelationID);
		}

		bool IsUniqueCorrelationID(ZString correlationID)
		{
			var query = new ZDBOnlyQuery(typeof(CusStatementChargesDetail));
			query.AddToFilter(CusStatementLineSchema.B3_EntryType, StatementEntryTypeList.Codes.DCG);
			query.AddToFilter(CusStatementLineSchema.B3_BrokerReference, correlationID);
			return !Factory.Exists(typeof(CusStatementChargesDetail), query);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			CorrelationIdGenerator.ClearCorrelationIDOnSaved(saveSucceeded);
			base.OnSaved(saveSucceeded);
		}

		ZString GetNewStatementNumber(BusinessObjectFactory factory)
		{
			var target = new FRStatementNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.FRStatementNumber,
				FountainGetter = Env.NumberFountains.GetFRStatementNumberGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			var result = target.Value.ToUpper();

			return result;
		}

		public static CusStatementHeader LoadOrCreateDCGStatementHeader(BusinessObjectFactory factory, ZGuid companyPK, IDCGResponseDataProvider dataProvider, bool shouldCreateNewOneIfNotLoaded)
		{
			CusStatementHeader result = null;
			var brokerReference = dataProvider.DCGReference;

			return Load() ?? New();

			CusStatementHeader Load()
			{
				var query = new ZDBOnlyQuery(typeof(CusStatementHeader));
				query.AddToFilter(CusStatementHeaderSchema.B2_GC, companyPK);
				var subQuery = new ZDBOnlySubQuery(typeof(CusStatementChargesDetail), CusStatementLineSchema.B3_B2);
				subQuery.AddToFilter(CusStatementLineSchema.B3_EntryType, StatementEntryTypeList.Codes.DCG);
				subQuery.AddToFilter(CusStatementLineSchema.B3_BrokerReference, brokerReference);
				query.AddSubQuery(subQuery, JoinCondition.And);
				result = factory.LoadTop1<CusStatementHeader>(query);
				return result;
			}

			CusStatementHeader New()
			{
				if (shouldCreateNewOneIfNotLoaded)
				{
					result = factory.New<CusStatementHeader>();
					result.B2_GC = companyPK;
					result.ChargesDetail.B3_BrokerReference = brokerReference;
					result.B2_StatementType = dataProvider.Frequency;
					result.B2_PeriodStartDate = dataProvider.PeriodStartDate;
					result.B2_PeriodEndDate = dataProvider.PeriodEndDate;
					result.B2_BranchDesignation = dataProvider.Direction;
				}

				return result;
			}
		}

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#region IDocumentSupportable Members
		DocumentSupporter IDocumentSupportable.DocumentSupporter => new CusStatementHeaderDocumentSupporter(this);
		#endregion

		#region IDocManagerSupport Members
		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CusStatementHeader));
		DocManagerInfo docManagerInfo;
		#endregion

		#endregion
	}
}
