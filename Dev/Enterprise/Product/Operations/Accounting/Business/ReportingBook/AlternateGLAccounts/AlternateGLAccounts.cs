using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AlternateGLAccounts : NonPersistentBusinessObject, IStmALogParent
	{
		public AlternateGLAccounts(BusinessObjectFactory factory) : base(factory)
		{
		}

		public bool? CreateMultipleAlternateGLAccount;

		ZGuid fParentGLAccountPK;
		[List("GLHeaders")]
		public ZGuid ParentGLAccountPK
		{
			get
			{
				return fParentGLAccountPK;
			}
			set
			{
				SetNonPersistentPropertyValue(ParentGLAccountPKInfo, ref fParentGLAccountPK, value);
				AlternateGLAccountsWithAttributeSet.ParentGLAccountPK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateParentGLAccountPK();
				}

				var parentGLAccount = Factory.Load<AccGLHeader>(fParentGLAccountPK);
				if (parentGLAccount != null)
				{
					CashFlowCategory = parentGLAccount.AG_CashFlowType;
					Unit = parentGLAccount.AG_StatisticalUnits;
					AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x =>
					{
						x.ParentGLAccountPK = fParentGLAccountPK;
						x.AlternateGLAccount.AGA_DebitCredit = parentGLAccount.AG_DebitCredit;
						x.AlternateGLAccount.AGA_ReportSection = parentGLAccount.AG_Column;
					});
				}
			}
		}

		public ZPropertyInfo ParentGLAccountPKInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ParentGLAccountPK));
			}
		}

		protected bool ParentGLAccountPK_ReadOnly
		{
			get { return WithoutParentAccountTypeList.ContainsCode(AccountType); }
		}

		public void ResetParentGLAccountPK()
		{
			fParentGLAccountPK = FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase
				? (ZGuid)FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault()?.AAA_AG_GLHeaderInfo.OriginalValue
				: fParentGLAccountPK;
		}

		readonly AccGLHeader parentGLAccount;
		public AccGLHeader ParentGLAccount => parentGLAccount == null || parentGLAccount.PK != fParentGLAccountPK ? Factory.Load<AccGLHeader>(fParentGLAccountPK) : parentGLAccount;

		public ZGuid OriginalAlternateGLAccountPK { get; set; }

		public ZGuid OriginalParentGLAccountPK { get; set; }
		readonly AccGLHeader originalParentGLAccount;
		public AccGLHeader OriginalParentGLAccount => originalParentGLAccount == null || originalParentGLAccount.PK != OriginalParentGLAccountPK ? Factory.Load<AccGLHeader>(OriginalParentGLAccountPK) : originalParentGLAccount;

		public AccGLHeaderCollection GLHeaders => new AccGLHeaderCollection(Factory);

		ZGuid fChartPK;
		[List("Charts")]
		public ZGuid ChartPK
		{
			get
			{
				return fChartPK;
			}
			set
			{
				if (value != fChartPK)
				{
					AlternateGLAccountsWithAttributeSet.ChartPK = value;
					SetNonPersistentPropertyValue(ChartPKInfo, ref fChartPK, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateChartPK();
					}
				}
			}
		}

		public ZPropertyInfo ChartPKInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChartPK));
			}
		}

		protected bool ChartPK_ReadOnly => IsInDatabase;

		public AccAlternateChartCollection Charts => FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.AlternateCharts;

		ZString accountType;
		[List("AccountTypeList")]
		[MaxLength(3)]
		public ZString AccountType
		{
			get
			{
				return accountType;
			}
			set
			{
				SetNonPersistentPropertyValue(AccountTypeInfo, ref accountType, value);
				AlternateGLAccountsWithAttributeSet.AccountType = accountType;
				AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x => x.AccountType = accountType);
				if (WithoutParentAccountTypeList.ContainsCode(accountType))
				{
					ParentGLAccountPK = Guid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAccountType();
				}
			}
		}

		public ZPropertyInfo AccountTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AccountType));
			}
		}

		protected bool AccountType_ReadOnly => IsInDatabase;

		public CodeDescriptionPairList AccountTypeList => AlternateGLAccountsWithAttributeSet.Count > 1
			? FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.BSHPAndLAccountTypeList
			: FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.AccountTypeList;
		public CodeDescriptionPairList WithoutParentAccountTypeList => FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.WithoutParentAccountTypeList;

		ZString cashFlowCategory;
		[List("CashFlowCategoryList")]
		[MaxLength(3)]
		public ZString CashFlowCategory
		{
			get
			{
				return cashFlowCategory;
			}
			set
			{
				SetNonPersistentPropertyValue(CashFlowCategoryInfo, ref cashFlowCategory, value);
				AlternateGLAccountsWithAttributeSet.CashFlowCategory = cashFlowCategory;
				AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x => x.CashFlowCategory = cashFlowCategory);
			}
		}

		public ZPropertyInfo CashFlowCategoryInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CashFlowCategory));
			}
		}

		public CodeDescriptionPairList CashFlowCategoryList => FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.CashFlowTypeList;

		ZString unit;
		[List("UnitList")]
		[MaxLength(3)]
		public ZString Unit
		{
			get
			{
				return unit;
			}
			set
			{
				SetNonPersistentPropertyValue(UnitInfo, ref unit, value);
				AlternateGLAccountsWithAttributeSet.Unit = unit;
				AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x => x.Unit = unit);
			}
		}

		public ZPropertyInfo UnitInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Unit));
			}
		}

		public ReadOnlyCodeDescriptionPairList UnitList => FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Lookups.StatisticalUnitsList;

		AlternateGLAccountWithAttributeSet firstAlternateGLAccountWithAttributeSet;

		[ChildEditable()]
		public AlternateGLAccountWithAttributeSet FirstAlternateGLAccountWithAttributeSet
		{
			get
			{
				if (firstAlternateGLAccountWithAttributeSet == null)
				{
					if (AlternateGLAccountsWithAttributeSet.Any())
					{
						firstAlternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().FirstOrDefault();
					}
					else
					{
						firstAlternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.AddNew();
						firstAlternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
					}

					RegisterEditableChildObject(firstAlternateGLAccountWithAttributeSet);
				}

				return firstAlternateGLAccountWithAttributeSet;
			}
			set
			{
				firstAlternateGLAccountWithAttributeSet = value;
				RegisterEditableChildObject(firstAlternateGLAccountWithAttributeSet);
			}
		}

		AlternateGLAccountWithAttributeSetCollection aternateGLAccountsWithAttributeSet;

		[ChildEditable()]
		public AlternateGLAccountWithAttributeSetCollection AlternateGLAccountsWithAttributeSet
		{
			get
			{
				var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(ParentGLAccountPK, ChartPK, 0, AccountType, CashFlowCategory, Unit);
				aternateGLAccountsWithAttributeSet = aternateGLAccountsWithAttributeSet ?? new AlternateGLAccountWithAttributeSetCollection(alternateGLAccountWithAttributeSetDetails, Factory);
				RegisterEditableChildObject(aternateGLAccountsWithAttributeSet);
				return aternateGLAccountsWithAttributeSet;
			}
		}

		public void ResetAlternateGLAccountsWithAttributeSet(ZGuid chartPK, ZGuid parentGLAccountPK, bool allowNewAlternateGLAccount = true)
		{
			if (chartPK.IsValid)
			{
				var parentGLAccount = Factory.Load<AccGLHeader>(parentGLAccountPK);
				if (parentGLAccount != null && parentGLAccount.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x => x.ADC_AAC_AlternateChart == chartPK && x.ADC_SeparateNumbering))
				{
					UndoChangeInAlternateGLAccountsWithAttributeSet();
					firstAlternateGLAccountWithAttributeSet = null;
					CreateMultipleAlternateGLAccount = true;
					var dissections = parentGLAccount.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Where(x => x.ADC_AAC_AlternateChart == chartPK);
					CreateAlternateGLAccountWithAttributesAccordingToDissections(dissections, chartPK, parentGLAccountPK, allowNewAlternateGLAccount);
				}
				else
				{
					CreateMultipleAlternateGLAccount = false;
					UndoChangeInAlternateGLAccountsWithAttributeSet();
					LoadAttributes(chartPK, parentGLAccountPK);
					if (!AlternateGLAccountsWithAttributeSet.Any())
					{
						firstAlternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.AddNew();
						firstAlternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
					}
					else
					{
						firstAlternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().FirstOrDefault();
					}
				}
			}
		}

		void UndoChangeInAlternateGLAccountsWithAttributeSet()
		{
			var alternateGLAccountsWithAttributeSet = AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>();
			if (alternateGLAccountsWithAttributeSet.Any(x => x.AlternateGLAccount.IsInDatabase))
			{
				alternateGLAccountsWithAttributeSet.ForEach(x =>
				{
					if (x.IsInDatabase)
					{
						x.Attributes.Reload(true);
						x.AlternateGLAccount.Reload();
					}
					else if (x.AlternateGLAccount.IsInDatabase && x.Attributes.Any(y => !y.IsInDatabase))
					{
						var newAttributes = x.Attributes.Where(y => !y.IsInDatabase);
						x.AlternateGLAccount.AlternateGLAccountAttributes.RemoveRange(newAttributes);
						newAttributes.DeleteAll();
						x.AlternateGLAccount.Reload();
					}
					else
					{
						x.AlternateGLAccount.Delete();
					}
				});
				AlternateGLAccountsWithAttributeSet.RemoveAll();
			}
			else
			{
				AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			}
			AlternateGLAccountsWithAttributeSet.CurrentSequence = 0;
		}

		void CreateAlternateGLAccountWithAttributesAccordingToDissections(IEnumerable<AccAlternateGLAccountDissection> dissections, ZGuid chartPK, ZGuid parentGLAccountPK, bool allowNewAlternateGLAccount)
		{
			var attributeValueDictionary = new Dictionary<string, IEnumerable<string>>();

			foreach (var dissection in dissections)
			{
				switch (dissection.ADC_Attribute)
				{
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, GetOrgHeaderCodes(parentGLAccountPK));
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, AccountingMasterFilesConstants.OCGList.GetAllCodes());
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, AccountingMasterFilesConstants.TICList.GetAllCodes());
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOList.GetAllCodes());
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFEList.GetAllCodes());
						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
						attributeValueDictionary.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, AccountingMasterFilesConstants.SPRList.GetAllCodes());
						break;
					default:
						break;
				}
			}

			LoadAttributes(chartPK, parentGLAccountPK);

			if (allowNewAlternateGLAccount)
			{
				CreateAttributes(new Dictionary<string, string>(), attributeValueDictionary);
			}
		}

		void LoadAttributes(ZGuid chartPK, ZGuid parentGLAccountPK)
		{
			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, parentGLAccountPK);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, chartPK);
			var attributesInDB = Factory.Load<AccAlternateGLAccountAttribute>(query);
			var attributesGroupBySequence = attributesInDB.GroupBy(x => x.AAA_Sequence);

			foreach (var attributes in attributesGroupBySequence)
			{
				var alternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.AddNew();
				alternateGLAccountWithAttributeSet.Sequence = attributes.Key;
				var attributeCollection = new AccAlternateGLAccountAttributeCollection(Factory);
				attributeCollection.AddRange(attributes);
				alternateGLAccountWithAttributeSet.Attributes = attributeCollection;
				alternateGLAccountWithAttributeSet.AlternateGLAccountNum = attributes.FirstOrDefault().AlternateGLAccount.AGA_AccountNum;
			}

			if (AlternateGLAccountsWithAttributeSet.Any())
			{
				AlternateGLAccountsWithAttributeSet.CurrentSequence = AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().Max(x => x.Sequence) + 1;
			}
		}

		void CreateAttributes(Dictionary<string, string> attributeValueDictionary, Dictionary<string, IEnumerable<string>> attributeValueListDictionary)
		{
			if (attributeValueListDictionary.Count == 0)
			{
				if (!AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().Any(x =>
					(!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, out var orgValue) && x.OrganizationCode == orgValue))
					&& (!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, out var ocgValue) && x.OCG == ocgValue))
					&& (!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, out var lfeValue) && x.LFE == lfeValue))
					&& (!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, out var lfoValue) && x.LFO == lfoValue))
					&& (!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, out var ticValue) && x.TIC == ticValue))
					&& (!attributeValueDictionary.ContainsKey(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR)
						|| (attributeValueDictionary.TryGetValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, out var sprValue) && x.SPR == sprValue))))
				{
					var alternateGLAccountWithAttributeSet = AlternateGLAccountsWithAttributeSet.AddNew();

					foreach (var attributeValue in attributeValueDictionary)
					{
						switch (attributeValue.Key)
						{
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG:
								var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, attributeValue.Value);
								alternateGLAccountWithAttributeSet.ORG = org.PK;
								break;
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
								alternateGLAccountWithAttributeSet.OCG = attributeValue.Value;
								break;
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
								alternateGLAccountWithAttributeSet.TIC = attributeValue.Value;
								break;
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
								alternateGLAccountWithAttributeSet.LFO = attributeValue.Value;
								break;
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
								alternateGLAccountWithAttributeSet.LFE = attributeValue.Value;
								break;
							case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
								alternateGLAccountWithAttributeSet.SPR = attributeValue.Value;
								break;
							default:
								break;
						}
					}

					alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
				}
			}
			else
			{
				var attributeValueList = attributeValueListDictionary.FirstOrDefault();
				attributeValueListDictionary.Remove(attributeValueList.Key);
				foreach (var value in attributeValueList.Value)
				{
					attributeValueDictionary.Add(attributeValueList.Key, value);
					CreateAttributes(attributeValueDictionary, attributeValueListDictionary);
					attributeValueDictionary.Remove(attributeValueList.Key);
				}
				attributeValueListDictionary.Add(attributeValueList.Key, attributeValueList.Value);
			}
		}

		IEnumerable<string> GetOrgHeaderCodes(ZGuid parentGLAccountPK)
		{
			var result = new List<string>();

			if (parentGLAccountPK.IsValid && (AccountingConfigurationRegistry.Instance.ARControlAccount.Value == parentGLAccountPK || AccountingConfigurationRegistry.Instance.APControlAccount.Value == parentGLAccountPK))
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				var fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				if (AccountingConfigurationRegistry.Instance.ARControlAccount.Value == parentGLAccountPK)
				{
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
				}
				else
				{
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
				}
				fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				subQuery.AddToFilter(fromAccountFilter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				result = Factory.Load<OrgHeader>(query).Select(x => x.OH_Code.ToString()).ToList();
			}

			return result;
		}

		public AlternateGLAccountsValidation Validation
		{
			get { return new AlternateGLAccountsValidation(this, Factory); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateAll();
		}

		public override bool HasChanges
		{
			get
			{
				return ParentGLAccountPK != OriginalParentGLAccountPK
					|| AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().Any(x => (x.AlternateGLAccount?.HasChanges ?? false) || x.Attributes.HasChanges);
			}
			set => base.HasChanges = value;
		}

		public bool IsInDb { get; set; }
		public override bool IsInDatabase => IsInDb || AlternateGLAccountsWithAttributeSet.All(x => x.IsInDatabase);

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				OriginalParentGLAccountPK = ParentGLAccountPK;
				OriginalAlternateGLAccountPK = FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK;
				IsInDb = true;
			}
		}

		public bool AlternateGLAccountsWithAttributeDeleted { get; set; }

		#region IStmALogParent members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		Logs IStmALogProvider.Logs => FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.Logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get
			{
				return this.Factory;
			}
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK; }
		}

		string IStmALogParent.LogsParentTableName => "AccAlternateGLAccount";

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion
	}
}
