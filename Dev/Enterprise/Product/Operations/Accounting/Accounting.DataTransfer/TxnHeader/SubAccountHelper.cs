using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public static class SubAccountHelper
	{
		public static ZGuid GetSubAccountPKFromCode(BusinessObjectFactory factory, string subAccountType, string subAccountCode)
		{
			if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code)
			{
				var org = BusinessObjectRetriever.GetOrgHeaderByCode(factory, subAccountCode);
				if (org != null)
				{
					return org.PK;
				}
			}
			else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code)
			{
				var salesGroup = BusinessObjectRetriever.GetAccGroupsByCode(factory, subAccountCode);
				if (salesGroup != null)
				{
					return salesGroup.PK;
				}
			}
			else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code)
			{
				var staff = BusinessObjectRetriever.GetGlbStaffByCode(factory, subAccountCode);
				if (staff != null)
				{
					return staff.PK;
				}
			}
			else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code)
			{
				var staffGroup = BusinessObjectRetriever.GetGlbGroupByCode(factory, subAccountCode);
				if (staffGroup != null)
				{
					return staffGroup.PK;
				}
			}

			return ZGuid.Empty;
		}

		public static Xsd.SubAccountCollection GetSubAccountsXmlFromSubAccounts(BusinessObjectFactory factory, TransactionLineSubAccountCollection subAccounts)
		{
			Xsd.SubAccountCollection collecton = new Xsd.SubAccountCollection();
			foreach (AccTransactionLineSubAccount subAccount in subAccounts)
			{
				var subAccountXml = GetSubAccountXmlFromSubAccountId(factory, SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(subAccount.AL1_SubClassParentTableCode), subAccount.AL1_SubClassParentId);
				if (subAccountXml != null)
				{
					collecton.Add(subAccountXml);
				}
			}
			return collecton;
		}

		public static Xsd.SubAccount GetSubAccountXmlFromSubAccountId(BusinessObjectFactory factory, ZString subAccountType, ZGuid subAccount)
		{
			Xsd.SubAccount subAccountXml = null;
			if (!subAccountType.IsEmpty && subAccount.IsValid)
			{
				subAccountXml = new Xsd.SubAccount();
				subAccountXml.IsSpecified = true;

				if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code)
				{
					var orgHeader = factory.Load<OrgHeader>(subAccount);
					if (orgHeader != null)
					{
						subAccountXml.Code = orgHeader.OH_Code;
						subAccountXml.Type.Code = Core.Constants.SubAccountType.Organization;
						subAccountXml.Type.Description = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Description;
					}
				}
				else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code)
				{
					var salesGroup = factory.Load<AccGroups>(subAccount);
					if (salesGroup != null)
					{
						subAccountXml.Code = salesGroup.AR_Code;
						subAccountXml.Type.Code = Core.Constants.SubAccountType.SalesGroup;
						subAccountXml.Type.Description = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Description;
					}
				}
				else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code)
				{
					var staffandresource = factory.Load<GlbStaff>(subAccount);
					if (staffandresource != null)
					{
						subAccountXml.Code = staffandresource.GS_Code;
						subAccountXml.Type.Code = Core.Constants.SubAccountType.StaffAndResources;
						subAccountXml.Type.Description = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Description;
					}
				}
				else if (subAccountType == AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code)
				{
					var staffGroup = factory.Load<GlbGroup>(subAccount);
					if (staffGroup != null)
					{
						subAccountXml.Code = staffGroup.GG_Code;
						subAccountXml.Type.Code = Core.Constants.SubAccountType.StaffGroup;
						subAccountXml.Type.Description = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Description;
					}
				}
			}
			return subAccountXml;
		}

		public static void CreateSubAccountFromXml(DependentTransactionLine line, Xsd.SubAccountCollection xmlSubAccountCollection, Action<TransactionLineSubAccount, ZGuid> evaluateAction = null)
		{
			if (line.GLHeader != null)
			{
				foreach (TransactionLineSubAccount subAccount in line.SubAccounts)
				{
					var xmlSubAccount = xmlSubAccountCollection.Cast<Xsd.SubAccount>().FirstOrDefault(x => SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(x.Type.Code) == subAccount.AL1_SubClassParentTableCode);

					if (xmlSubAccount != null)
					{
						var value = GetSubAccountPKFromCode(line.Factory, xmlSubAccount.Type.Code, xmlSubAccount.Code);

						if (evaluateAction != null)
						{
							evaluateAction(subAccount, value);
						}
						else
						{
							subAccount.AL1_SubClassParentId = value;
						}
					}
				}
			}
		}
	}
}
