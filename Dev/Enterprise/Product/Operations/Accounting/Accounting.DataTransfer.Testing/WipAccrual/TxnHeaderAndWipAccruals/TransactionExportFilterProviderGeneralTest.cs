using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.DataTransfer.TransactionExportFilterProvider;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class TransactionExportFilterProviderGeneralTest : TestCaseWithFactory
	{
		public void TestAllExportTransactionTypesAreMapped()
		{
			IEnumerable exportTransactionTypes = typeof(ExportTransactionTypes).GetFields().Select(f => (string)f.GetValue(null));
			IEnumerable mappedTransactionTypes = AccountingTransactionTypesRegistryMapper.Map.Keys;

			AssertContainsExactElementsInAnyOrder(exportTransactionTypes, mappedTransactionTypes);
		}

		public void TestAllExportTransactionTypesMapToAProperty()
		{
			foreach (var propertyInfo in AccountingTransactionTypesRegistryMapper.Map.Values)
			{
				AssertNotNull("PropertyInfo is not null", propertyInfo);
			}
		}

		public void TestCopyRegistryValuesToFilter()
		{
			SetAllRegistryItems(ZBool.False);

			foreach (CodeDescriptionBool item in SystemDataRegistry.Instance.AccountingTransactionTypes.Value)
			{
				SetRegistryItem(item.Code, ZBool.True);

				AccountingTransactionTypesRegistryMapper.CopyRegistryValuesToFilter(TransactionExportFilterProvider);
				AssertOnlyTransactionTypeFilterEnabled(item.Code);

				SetRegistryItem(item.Code, ZBool.False);
			}
		}

		public void TestFilterMatchesRegistryValues()
		{
			SetAllRegistryItems(ZBool.False);

			foreach (var propertyInfo in AccountingTransactionTypesRegistryMapper.Map.Values)
			{
				propertyInfo.SetValue(TransactionExportFilterProvider, ZBool.True, null);
				AssertEquals("FilterMatchesRegistryValues", false, AccountingTransactionTypesRegistryMapper.FilterMatchesRegistryValues(TransactionExportFilterProvider));
				propertyInfo.SetValue(TransactionExportFilterProvider, ZBool.False, null);
			}

			AssertEquals("FilterMatchesRegistryValues", true, AccountingTransactionTypesRegistryMapper.FilterMatchesRegistryValues(TransactionExportFilterProvider));
		}

		void SetRegistryItem(string exportTransactionType, ZBool value)
		{
			var registryCollection = SystemDataRegistry.Instance.AccountingTransactionTypes.Value;
			((CodeDescriptionBool)registryCollection.FindByCode(exportTransactionType)).Bool = value;
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
		}

		void SetAllRegistryItems(ZBool value)
		{
			var registryCollection = SystemDataRegistry.Instance.AccountingTransactionTypes.Value;
			foreach (CodeDescriptionBool item in registryCollection)
			{
				item.Bool = value;
			}
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
		}

		void AssertOnlyTransactionTypeFilterEnabled(string exportTransactionType)
		{
			foreach (var item in AccountingTransactionTypesRegistryMapper.Map)
			{
				if (item.Key == exportTransactionType)
				{
					AssertEquals(string.Format("Filter '{0}' should be enabled", item.Value.Name), ZBool.True, item.Value.GetValue(TransactionExportFilterProvider, null));
				}
				else
				{
					AssertEquals(string.Format("Filter '{0}' should not be enabled", item.Value.Name), ZBool.False, item.Value.GetValue(TransactionExportFilterProvider, null));
				}
			}
		}

		TransactionExportFilterProvider TransactionExportFilterProvider
		{
			get
			{
				if (fTransactionExportFilterProvider == null)
				{
					fTransactionExportFilterProvider = new TransactionExportFilterProvider(Factory);
				}
				return fTransactionExportFilterProvider;
			}
		}
		TransactionExportFilterProvider fTransactionExportFilterProvider;
	}
}
