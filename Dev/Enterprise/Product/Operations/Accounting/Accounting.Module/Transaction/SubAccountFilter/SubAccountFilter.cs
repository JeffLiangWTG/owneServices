using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public class SubAccountFilter : ModuleFilter
	{
		public SubAccountFilter(ZString description) : base(description)
		{
		}

		public SubAccountFilter(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		[List("SubAccountTypeList")]
		public ZString SubAccountType
		{
			get
			{
				return subAccountType;
			}
			set
			{
				if (subAccountType != value)
				{
					subAccountType = value;
					SubAccount = ZGuid.Empty;

					Validation.ValidateSubAccountType();
					Validation.ValidateSubAccount();
					InvalidateCachedQuery();
				}

				SubAccountTypeInfo.RefreshBinding();
			}
		}
		ZString subAccountType;

		public CodeDescriptionPairList SubAccountTypeList
		{
			get
			{
				if (subAccountTypes == null)
				{
					subAccountTypes = new AccountingMasterFilesConstants.SubAccountTypeList();
				}

				return subAccountTypes;
			}
		}
		CodeDescriptionPairList subAccountTypes;

		public ZPropertyInfo SubAccountTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SubAccountType)); }
		}

		[List("SubAccountList")]
		public ZGuid SubAccount
		{
			get
			{
				return subAccount;
			}
			set
			{
				if (subAccount != value)
				{
					subAccount = value;

					Validation.ValidateSubAccount();
					InvalidateCachedQuery();
				}

				SubAccountInfo.RefreshBinding();
			}
		}
		ZGuid subAccount;

		public ZPropertyInfo SubAccountInfo
		{
			get { return GetZPropertyInfo(nameof(SubAccount)); }
		}

		public IBusinessObjectCollection SubAccountList
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return SubAccountHelper.GetSubAccountList(factory, SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType));
			}
		}

		BusinessObjectFactory factory;

		protected override void ClearCore()
		{
			SubAccountType = ZString.Empty;
			SubAccount = ZGuid.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			SubAccountFilter filter = (SubAccountFilter)filterToCopyFrom;
			SubAccountType = filter.SubAccountType;
			SubAccount = filter.SubAccount;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Organisations; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new SubAccountFilter(category, parentCollection);
		}

		public new SubAccountModuleFilterValidation Validation
		{
			get { return (SubAccountModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new SubAccountModuleFilterValidation(this);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(SubAccountHelper.GetSubAccountsQuery(SubAccountType, SubAccount));

			return result;
		}

		protected override bool IsEmptyCore => SubAccountType.IsEmpty && SubAccount.IsEmpty;

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return null; }
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString("SubAccountType", SubAccountType.ToString());
			writer.WriteElementString("SubAccount", SubAccount.ToString());
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			if (reader.Name == "SubAccountType")
			{
				SubAccountType = new ZString(reader.ReadElementString("SubAccountType"));
			}
			if (reader.Name == "SubAccount")
			{
				SubAccount = new Guid(reader.ReadElementString("SubAccount"));
			}
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			SubAccountType = RandomString(MaxLength);
			SubAccount = ZGuid.NewZGuid();
		}

#endif
		#endregion
	}
}
