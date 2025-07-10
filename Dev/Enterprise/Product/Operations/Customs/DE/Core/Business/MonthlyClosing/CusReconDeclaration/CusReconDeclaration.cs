using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclaration : Customs.Business.CusReconDeclaration
		, Integration.Customs.DE.ICusReconDeclaration
		, IDocManagerSupport
		, IRelatedJob
	{
		public CusReconDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusReconDeclaration.Schema
		{
			public const string RegistrationNumber = nameof(CusReconDeclaration.RegistrationNumber);
			public const string IsDeclarantImporter = nameof(CusReconDeclaration.IsDeclarantImporter);
		}

		[ReadOnly(true)]
		public override ZString CRD_ApplicationCode
		{
			get => base.CRD_ApplicationCode;
			set => base.CRD_ApplicationCode = value;
		}

		[ReadOnly(true)]
		public override ZString CRD_CustomsStatus
		{
			get => base.CRD_CustomsStatus;
			set => base.CRD_CustomsStatus = value;
		}

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZDate CRD_PeriodFrom
		{
			get => base.CRD_PeriodFrom;
			set => base.CRD_PeriodFrom = value;
		}

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZDate CRD_PeriodTo
		{
			get => base.CRD_PeriodTo;
			set => base.CRD_PeriodTo = value;
		}

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZString CRD_DeclarationType
		{
			get => base.CRD_DeclarationType;
			set
			{
				bool hasChanged = CRD_DeclarationType != value;
				base.CRD_DeclarationType = value;
				if (hasChanged)
				{
					PopulateReconClearanceAuthorisationIfNeeded();
				}
			}
		}

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		[ResourceStringData("15F02D88-719B-4BE3-9C7B-9788176BD647", Caption = "Representation Type", MediumCaption = "Rep. Type")]
		public override ZString CRD_DeclarantType
		{
			get => base.CRD_DeclarantType;
			set
			{
				bool hasChanged = CRD_DeclarantType != value;
				base.CRD_DeclarantType = value;
				if (hasChanged)
				{
					PopulateReconClearanceAuthorisationIfNeeded();
				}
			}
		}

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZGuid CRD_OA_DeclarantAddress
		{
			get => base.CRD_OA_DeclarantAddress;
			set
			{
				bool hasChanged = CRD_OA_DeclarantAddress != value;
				base.CRD_OA_DeclarantAddress = value;
				if (!IsDeclarantImporter)
				{
					CRD_OA_ImporterAddress = ZGuid.Empty;
				}
				if (hasChanged)
				{
					PopulateReconClearanceAuthorisationIfNeeded();
				}
			}
		}

		[ResourceStringData("11228884-0460-469B-A40B-08B2680778FB", Caption = "Declarant Code", MediumCaption = "Declarant")]
		public ZString DeclarantCode => Factory.GetCached(ref declarantCodeCached, () => GetOrganizationCode(DeclarantAddress));
		CachedProperty<ZString> declarantCodeCached;

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZGuid CRD_OA_RepresentativeAddress
		{
			get => base.CRD_OA_RepresentativeAddress;
			set
			{
				bool hasChanged = CRD_OA_RepresentativeAddress != value;
				base.CRD_OA_RepresentativeAddress = value;
				if (hasChanged)
				{
					PopulateReconClearanceAuthorisationIfNeeded();
				}
			}
		}

		[ResourceStringData("CF1616B4-194B-4980-9BB8-39288433F5B7", Caption = "Representative Code", MediumCaption = "Representative")]
		public ZString RepresentativeCode => Factory.GetCached(ref representativeCodeCached, () => GetOrganizationCode(RepresentativeAddress));
		CachedProperty<ZString> representativeCodeCached;

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		[ResourceStringData("D013FDFF-CCFA-4F65-88F6-8AEC394ED969", Caption = "Represented Party", MediumCaption = "Represented Party", ShortCaption = "Represented")]
		public override ZGuid CRD_OA_BuyingAgentAddress
		{
			get => base.CRD_OA_BuyingAgentAddress;
			set
			{
				bool hasChanged = CRD_OA_BuyingAgentAddress != value;
				base.CRD_OA_BuyingAgentAddress = value;
				if (hasChanged)
				{
					PopulateReconClearanceAuthorisationIfNeeded();
				}
			}
		}

		[ResourceStringData("23A67864-7E1E-4296-A2C6-38FBEA167C06", Caption = "Represented Party Code", MediumCaption = "Represented Party", ShortCaption = "Represented")]
		public ZString BuyingAgentCode => Factory.GetCached(ref buyingAgentCodeCached, () => GetOrganizationCode(BuyingAgentAddress));
		CachedProperty<ZString> buyingAgentCodeCached;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsDeclarantImporterReadOnly))]
		[ResourceStringData("2725019D-8190-4C5B-8DEA-46C1B39E249F", Caption = "Declarant is Importer?")]
		public ZBool IsDeclarantImporter
		{
			get => CRD_OA_DeclarantAddress == CRD_OA_ImporterAddress && CRD_OA_DeclarantAddress.IsValid;
			set
			{
				CRD_OA_ImporterAddress = value ? CRD_OA_DeclarantAddress : ZGuid.Empty;
				IsDeclarantImporterInfo.RefreshBinding();
			}
		}

		bool IsDeclarantImporterReadOnly => DeclarationIsLinkedToSimplifiedDeclaration || !CRD_OA_DeclarantAddress.IsValid;

		public ZPropertyInfo IsDeclarantImporterInfo => GetZPropertyInfo(Schema.IsDeclarantImporter);

		[ReadOnlyMember(nameof(DeclarationIsLinkedToSimplifiedDeclaration))]
		public override ZGuid CRD_CPH_ReconClearanceAuthorisation
		{
			get => base.CRD_CPH_ReconClearanceAuthorisation;
			set
			{
				bool hasChanged = CRD_CPH_ReconClearanceAuthorisation != value;
				base.CRD_CPH_ReconClearanceAuthorisation = value;
				if (hasChanged)
				{
					AutoPopulate_CRD_CustomsOffice();
				}
			}
		}

		void AutoPopulate_CRD_CustomsOffice()
		{
			var customsOffice = ZString.Empty;
			if (CRD_CPH_ReconClearanceAuthorisation != ZGuid.Empty)
			{
				var permit = Factory.Load<CusAuthorisationHeader>(CRD_CPH_ReconClearanceAuthorisation);
				if (permit != null)
				{
					customsOffice = $"DE00{permit.CPH_Number.SubstringSafe(5, 4)}"; // Fixed prefix
				}
			}
			CRD_CustomsOffice = customsOffice;
		}

		[ReadOnly(true)]
		public override ZString CRD_CustomsOffice
		{
			get => base.CRD_CustomsOffice;
			set => base.CRD_CustomsOffice = value;
		}

		[ReadOnly(true)]
		public override ZString CRD_MessageStatus
		{
			get => base.CRD_MessageStatus;
			set => base.CRD_MessageStatus = value;
		}

		[ResourceStringData("751EEEA6-220D-4269-8A9A-111799AAAA22", Caption = "Procedure")]
		public ZString Procedure => ZString.Empty;

		[ResourceStringData("697225DA-9C75-4879-ABC1-C36D438C6B28", Caption = "Registration Number", MediumCaption = "Reg. Number", ShortCaption = "Reg. No.")]
		public ZString RegistrationNumber => GetCusEntryNumbers().FirstOrDefault()?.CE_EntryNum ?? ZString.Empty;

		public ZBool CanSendMessage => CRD_MessageStatus != MessageStatusList.Codes.Sent;

		CusEntryNumber[] GetCusEntryNumbers()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, Schema.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
			return (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), query);
		}

		[ResourceStringData("A6A2DAD4-9216-4270-86A0-48EBB245D53A", Caption = "Is Finalized?")]
		public ZBool IsFinalized => this.GetFinalizationFlagNote() == MonthlyClosingHelper.DeclarationIsFinalizedFlag;

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		[ResourceStringData("3DB45AFC-362B-490E-8717-0A1514D8E34B", Caption = "Line Status")]
		public ZString LineStatusSummary
		{
			get
			{
				const string txKey = "TX*";
				var openKey = Res.GetString("F4F482EF-FD7F-4508-B850-77B0F2E1EEF0", "Open");

				var totalEntryLinesNumber = 0;
				var allCustomStatuses = CusReconEntries.SelectMany(e => e.CusReconEntryLines)
					.GroupBy(l =>
					{
						var customsStatus = l.CRL_CustomsStatus.ToString();
						switch (l.CRL_CustomsStatus)
						{
							case string v when string.IsNullOrEmpty(v):
								return openKey;
							case string v when v.StartsWith("TX"):
								return txKey;
							default:
								return customsStatus;
						}
					})
					.OrderBy(g =>
					{
						switch (g.Key)
						{
							case string key when key == openKey:
								return 0;
							case UniversalReferenceConstants.EntryStatus.REJ:
								return 1;
							case UniversalReferenceConstants.EntryStatus.RC2:
								return 2;
							case UniversalReferenceConstants.EntryStatus.ERR:
								return 3;
							case txKey:
								return 4;
							default:
								return int.MaxValue;
						}
					})
					.Select(g =>
					{
						var count = g.Count();
						totalEntryLinesNumber += count;
						return $"{g.Key} = {count}";
					});

				var lineStatusDetails = string.Join(", ", allCustomStatuses);
				var lineStatusSummary = Res.GetString("953FA90A-9C21-4007-940B-E44CABBC6B96", "Lines = {0}", totalEntryLinesNumber);
				if (!string.IsNullOrEmpty(lineStatusDetails))
				{
					lineStatusSummary += $" ({lineStatusDetails})";
				}

				return lineStatusSummary;
			}
		}

		public new CusReconEntryCollection CusReconEntries => (CusReconEntryCollection)base.CusReconEntries;

		public new CusReconDeclarationValidation Validation => (CusReconDeclarationValidation)base.Validation;

		public new CusReconDeclarationLookups Lookups => (CusReconDeclarationLookups)base.Lookups;

		public string UnlinkedDeclarationsNumberMessage
		{
			get
			{
				var activeBusinessObjectCollection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
				LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(activeBusinessObjectCollection, this);

				var filterStripBusinessObject = new SimplifiedDeclarationFilterStripBusinessObject();
				filterStripBusinessObject.SetExternalDefaults(activeBusinessObjectCollection);
				foreach (FilterBusinessObjectDefault filterBusinessObjectDefault in activeBusinessObjectCollection.FilterBusinessObjectDefaults)
				{
					if (filterStripBusinessObject.ModuleFilters[filterBusinessObjectDefault.FilterName] is { } filterToActivate)
					{
						filterToActivate.IsActive = true;
					}
				}
				var defaultFilter = filterStripBusinessObject.Filter;

				var additionalFilter = new ZQuery(CusReconEntrySchema.CRE_CRD, null);
				additionalFilter.AddToFilter(CusReconEntrySchema.CRE_GB_Branch, CRD_GB_Branch);
				defaultFilter.AddToFilter(additionalFilter);

				activeBusinessObjectCollection.AdditionalFilter = defaultFilter;
				return Res.GetString("A05FAEAF-277D-4598-B237-107E7C35A8AE", "{0} unlinked Simplified Declarations found", activeBusinessObjectCollection.Count);
			}
		}

		protected override Customs.Business.CusReconEntryCollection CreateNewCusReconEntryCollection() => new CusReconEntryCollection(this);

		protected override Customs.Business.CusReconBase.CusReconDeclarationValidation GetNewValidation() => new CusReconDeclarationValidation(this);

		protected override Customs.Business.CusReconBase.CusReconDeclarationLookups GetNewLookups() => new CusReconDeclarationLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;
			var today = ZDateTime.Today;
			CRD_PeriodFrom = new ZDate(today.Year, today.Month, 1);
			CRD_PeriodTo = new ZDate(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(CRD_JobReferenceNumberInfo, GetNewJobReferenceNumber);
		}

		#region IDocManagerSupportMembers

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GermanyMonthlyClosing)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => CRD_JobReferenceNumber;

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => ZString.Empty;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.DE.MonthlyClosing;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		ZString GetNewJobReferenceNumber(BusinessObjectFactory factory)
		{
			var target = new CusReconDeclarationJobNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.MonthlyClosingJobReference,
				FountainGetter = Env.NumberFountains.GetMonthlyClosingJobReferenceGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		ZBool DeclarationIsLinkedToSimplifiedDeclaration => Factory.GetValue(ref declarationIsLinkedToSimplifiedDeclarationCached, () => CusReconEntries.Any());
		CachedProperty<ZBool> declarationIsLinkedToSimplifiedDeclarationCached;

		void PopulateReconClearanceAuthorisationIfNeeded()
		{
			CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
			var authorizationList = Lookups.AuthorizationList;
			if (authorizationList.Count == 1)
			{
				base.CRD_CPH_ReconClearanceAuthorisation = authorizationList[0].PK;
				AutoPopulate_CRD_CustomsOffice();
			}
		}

		ZString GetOrganizationCode(OrgAddress address)
		{
			if (address != null)
			{
				var organization = Factory.Load<OrgHeader>(address.OA_OH);
				return organization.OH_Code;
			}

			return ZString.Empty;
		}
	}
}
