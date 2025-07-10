using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class SubAccountHelper
	{
		public enum SubAccountValueType
		{
			Code,
			Description
		}

		public static CodeDescriptionPairList GetSubAccountTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Accounting_83ea04e3-4dcb-4d45-b8c7-4596d8e74a17", () => new AccountingMasterFilesConstants.SubAccountTypeList());
		}

		public static IBusinessObjectCollection GetSubAccountList(BusinessObjectFactory factory, ZString type)
		{
			if (type == OrgHeaderSchema.Constants.Prefix)
			{
				var query = new ZQuery(OrgHeaderSchema.OH_IsActive, true);
				return new OrgHeaderCollection(factory, query);
			}
			else if (type == AccGroupsSchema.Constants.Prefix)
			{
				return new AccGroupsCollection(factory);
			}
			else if (type == GlbStaffSchema.Constants.Prefix)
			{
				var query = new ZQuery(GlbStaffSchema.GS_IsActive, true);
				return new GlbStaffAndResourceCollection(factory, query);
			}
			else if (type == GlbGroupSchema.Constants.Prefix)
			{
				var query = new ZQuery(GlbGroupSchema.GG_IsActive, true);
				return new GlbGroupCollection(factory, query);
			}
			else
			{
				return new AccGroupsCollection(factory, new ZQuery() { IsNoResultQuery = true }); //instead of returning null, we decided to return a collection with NoResult and we decided to go with relatively light weight AccGroupsCollection
			}
		}

		public static bool IsSubAccountReadOnly(ISupportSubAccount bizO)
		{
			return bizO == null || (bizO.SubAccountParent != null && bizO.SubAccountParent.IsJobRelated);
		}

		public static void SetSubClassParentTableCode(ISupportMultiSubAccounts bizO)
		{
			BusinessObject bizObj;
			if ((bizObj = bizO as BusinessObject) != null &&
				!bizObj.ReadOnly &&
				bizO.IsMultiSubAccountsSupported)
			{
				SetMultiSubClassParentTableCode(bizO);
			}
		}

		public static void SetSubClassParentTableCode(TransactionHeaderWithLines header, ODisplayMode displayMode)
		{
			if (displayMode != ODisplayMode.ReadOnly)
			{
				header.Lines.Cast<DependentTransactionLine>().ForEach(x =>
				{
					if (!x.ReadOnly && x.IsInDatabase)
					{
						SetSubClassParentTableCode(x);
					}
				});
			}
		}

		public static void DeleteSubAccounts(ISupportMultiSubAccounts supportSubAccountsObj)
		{
			if (supportSubAccountsObj != null && supportSubAccountsObj.IsMultiSubAccountsSupported && supportSubAccountsObj.SubAccounts.SubAccountElements.Any())
			{
				supportSubAccountsObj.SubAccounts.RemoveAndDeleteAll();
			}
		}

		static void SetMultiSubClassParentTableCode(ISupportMultiSubAccounts supportSubAccountsObj)
		{
			if (!supportSubAccountsObj.IsJobRelated && supportSubAccountsObj.GLHeader != null)
			{
				var bizO = supportSubAccountsObj as BusinessObject;
				var subAccountTypes = supportSubAccountsObj.GLHeader.SubAccountTypes.Cast<AccGLHeaderSubAccount>().Select(x => x.ASA_SubClass);
				IEnumerable<ZString> addSubAccountTypes;
				var isInDatabase = bizO?.IsInDatabase ?? false;
				if (isInDatabase)
				{
					addSubAccountTypes = subAccountTypes.Except(supportSubAccountsObj.SubAccounts.SubAccountElements.Select(x => x.SubAccountTypeParentTableCode));

					var removingTransactionLineSubAccounts = new List<TransactionLineSubAccount>();

					if (supportSubAccountsObj.SubAccounts is TransactionLineSubAccountCollection subAccountCollection)
					{
						foreach (var subAccount in subAccountCollection.Cast<TransactionLineSubAccount>())
						{
							if (!subAccountTypes.Contains(subAccount.SubAccountTypeParentTableCode))
							{
								removingTransactionLineSubAccounts.Add(subAccount);
							}
						}

						foreach (var removingTransactionLineSubAccount in removingTransactionLineSubAccounts)
						{
							subAccountCollection.RemoveAndDelete(removingTransactionLineSubAccount);
						}
					}
				}
				else
				{
					addSubAccountTypes = subAccountTypes;
					supportSubAccountsObj.SubAccounts.RemoveAndDeleteAll();
				}

				addSubAccountTypes.ForEach(subClass =>
				{
					var subAccount = supportSubAccountsObj.SubAccounts.AddNew();
					Action subAccountAction = () => { subAccount.SubAccountTypeParentTableCode = subClass; };
					if (isInDatabase)
					{
						BusinessObject subAccountBusinessObject = subAccount as BusinessObject;
						if (subAccountBusinessObject != null)
						{
							using (subAccountBusinessObject.SuspendSettingHasChanges())
							{
								subAccountAction.Invoke();
							}
						}
					}
					else
					{
						subAccountAction.Invoke();
					}
				});
				supportSubAccountsObj.SubAccounts.Sort(supportSubAccountsObj.SubAccounts.SortPropertyName);
			}
			else
			{
				supportSubAccountsObj.SubAccounts.RemoveAndDeleteAll();
			}
		}

		public static ZString GetSubAccountCodeFromSubAccountId(BusinessObjectFactory factory, ZString subAccountType, ZGuid subAccount)
		{
			return GetSubAccountValueFromSubAccountId(factory, subAccountType, subAccount, SubAccountValueType.Code);
		}

		public static ZString GetSubAccountValueFromSubAccountId(BusinessObjectFactory factory, ZString subAccountType, ZGuid subAccountId, SubAccountValueType subAccountValueType)
		{
			string subAccountValue = string.Empty;

			if (!subAccountType.IsEmpty && !subAccountId.IsEmpty)
			{
				switch (subAccountType)
				{
					case Constants.SubAccountType.Organization:
						{
							var orgHeader = factory.Load<OrgHeader>(subAccountId);
							if (orgHeader != null)
							{
								switch (subAccountValueType)
								{
									case SubAccountValueType.Code: subAccountValue = orgHeader.OH_Code; break;
									case SubAccountValueType.Description: subAccountValue = orgHeader.OH_FullName; break;
								}
							}
							break;
						}
					case Constants.SubAccountType.SalesGroup:
						{
							var salesGroup = factory.Load<AccGroups>(subAccountId);
							if (salesGroup != null)
							{
								switch (subAccountValueType)
								{
									case SubAccountValueType.Code: subAccountValue = salesGroup.AR_Code; break;
									case SubAccountValueType.Description: subAccountValue = salesGroup.AR_DescMultilingual; break;
								}
							}
							break;
						}
					case Constants.SubAccountType.StaffAndResources:
						{
							var staffandresource = factory.Load<GlbStaff>(subAccountId);
							if (staffandresource != null)
							{
								switch (subAccountValueType)
								{
									case SubAccountValueType.Code: subAccountValue = staffandresource.GS_Code; break;
									case SubAccountValueType.Description: subAccountValue = staffandresource.GS_FullName; break;
								}
							}
							break;
						}
					case Constants.SubAccountType.StaffGroup:
						{
							var staffGroup = factory.Load<GlbGroup>(subAccountId);
							if (staffGroup != null)
							{
								switch (subAccountValueType)
								{
									case SubAccountValueType.Code: subAccountValue = staffGroup.GG_Code; break;
									case SubAccountValueType.Description: subAccountValue = staffGroup.GG_Desc; break;
								}
							}
							break;
						}
				}
			}

			return subAccountValue;
		}

		public static void ValidateSubClassParentId(ZPropertyInfo propertyInfo, ZString subAccountType, ISupportMultiSubAccounts subAccountsParent)
		{
			if (subAccountsParent != null)
			{
				if (propertyInfo.Value.IsEmpty && (subAccountsParent.GLHeader?.GetIsSubClassValidationRuleMandatory(subAccountType) ?? false) && !((subAccountsParent as GLJournalLine)?.HasContext(BusinessContext.AutoEliminationJournal) ?? false))
				{
					MandatoryValidation.CheckEntered(propertyInfo);
				}

				if (!propertyInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(propertyInfo);
				}
			}
		}

		public static ZQuery GetSubAccountsQuery(ZString subAccountType, ZGuid subAccountID, bool withoutValue = false)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GenericTransaction.GenericTransaction));

			var subAccountDBParentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType);

			if (!subAccountType.IsEmpty && !subAccountID.IsEmpty)
			{
				ZDBOnlySubQuery subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL);
				subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);
				subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentId, subAccountID);

				ZDBOnlySubQuery subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH);
				subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);
				subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId, subAccountID);

				query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForLine, JoinCondition.Or);
				query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForHeader, JoinCondition.Or);
			}
			else if (!subAccountType.IsEmpty)
			{
				if (withoutValue)
				{
					ZDBOnlySubQuery subAccountQueryForGL = new ZDBOnlySubQuery(typeof(AccGLHeaderSubAccount), AccGLHeaderSubAccountSchema.ASA_AG);
					subAccountQueryForGL.AddToFilter(AccGLHeaderSubAccountSchema.ASA_SubClass, subAccountDBParentTableCode);

					ZDBOnlySubQuery subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL, true);
					subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);

					ZDBOnlySubQuery subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH, true);
					subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);

					query.AddSubQuery(GenericTransactionSchema.VT_AGForSubAccount, subAccountQueryForGL, JoinCondition.And);
					query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForLine, JoinCondition.And);
					query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForHeader, JoinCondition.And);
				}
				else
				{
					ZDBOnlySubQuery subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL);
					subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode, subAccountDBParentTableCode);

					ZDBOnlySubQuery subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH);
					subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode, subAccountDBParentTableCode);

					ZDBOnlySubQuery subAccountQueryForGL = new ZDBOnlySubQuery(typeof(AccGLHeaderSubAccount), AccGLHeaderSubAccountSchema.ASA_AG);
					subAccountQueryForGL.AddToFilter(AccGLHeaderSubAccountSchema.ASA_SubClass, subAccountDBParentTableCode);

					query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForLine, JoinCondition.Or);
					query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForHeader, JoinCondition.Or);
					query.AddSubQuery(GenericTransactionSchema.VT_AGForSubAccount, subAccountQueryForGL, JoinCondition.Or);
				}
			}
			else if (!subAccountID.IsEmpty)
			{
				ZDBOnlySubQuery subAccountQueryForLine = new ZDBOnlySubQuery(typeof(AccTransactionLineSubAccount), AccTransactionLineSubAccountSchema.AL1_AL);
				subAccountQueryForLine.AddToFilter(AccTransactionLineSubAccountSchema.AL1_SubClassParentId, subAccountID);

				ZDBOnlySubQuery subAccountQueryForHeader = new ZDBOnlySubQuery(typeof(AccTransactionHeaderSubAccount), AccTransactionHeaderSubAccountSchema.AHS_AH);
				subAccountQueryForHeader.AddToFilter(AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId, subAccountID);

				query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForLine, JoinCondition.Or);
				query.AddSubQuery(GenericTransactionSchema.VT_SubAccountParentPK, subAccountQueryForHeader, JoinCondition.Or);
			}

			return query;
		}

		public static void CopySubAccounts(ISupportMultiSubAccounts copyOfBizObj, ISupportMultiSubAccounts bizObj, bool isRefresh = false)
		{
			if (copyOfBizObj != null && bizObj != null && copyOfBizObj.IsMultiSubAccountsSupported && copyOfBizObj.SubAccounts != null)
			{
				ISupportSubAccount copyOfBizObjSubAccount;

				if (isRefresh)
				{
					copyOfBizObj.SubAccounts.RemoveAndDeleteAll();
				}

				bizObj.SubAccounts?.SubAccountElements.Where(x => !x.SubAccountParentId.IsEmpty).ForEach(subAccount =>
				{
					if (isRefresh)
					{
						copyOfBizObjSubAccount = copyOfBizObj.SubAccounts.AddNew();
						copyOfBizObjSubAccount.SubAccountTypeParentTableCode = subAccount.SubAccountTypeParentTableCode;
					}
					else
					{
						copyOfBizObjSubAccount = copyOfBizObj.SubAccounts.SubAccountElements.FirstOrDefault(x => x.SubAccountTypeParentTableCode == subAccount.SubAccountTypeParentTableCode);
					}

					if (copyOfBizObjSubAccount != null)
					{
						copyOfBizObjSubAccount.SubAccountParentId = subAccount.SubAccountParentId;
					}
				});

				copyOfBizObj.SubAccounts.Sort(copyOfBizObj.SubAccounts.SortPropertyName);
			}
		}

		public static ZString GetMultiSubAccountTypeCode(ISupportMultiSubAccounts supportSubAccountsObj, BusinessObjectFactory factory)
		{
			var result = string.Empty;
			if (supportSubAccountsObj?.SubAccounts != null && supportSubAccountsObj.IsMultiSubAccountsSupported && factory != null)
			{
				supportSubAccountsObj.SubAccounts.Sort(supportSubAccountsObj.SubAccounts.SortPropertyName);
				var subAccounts = supportSubAccountsObj.SubAccounts.SubAccountElements.Where(x => !x.SubAccountParentId.IsEmpty);

				result = string.Join(", ", subAccounts.Select(x => x.SubAccountTypeDisplayCode + ": " + GetSubAccountCodeFromSubAccountId(factory, x.SubAccountTypeDisplayCode, x.SubAccountParentId)));
			}

			return result;
		}
	}
}
