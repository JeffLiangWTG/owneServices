using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public static class RegistryTester
	{
		public static void SetValue(IRegistryItem item, object value)
		{
			Guid departmentPK = HasDepartmentStorage(item) ? EnvProxy.Instance.CurrentDepartment.PK : Guid.Empty;
			Guid branchPK = HasBranchStorage(item) ? EnvProxy.Instance.CurrentBranch.PK : Guid.Empty;
			Guid companyPK = (branchPK == Guid.Empty && HasCompanyStorage(item)) ? EnvProxy.Instance.CurrentCompany?.PK ?? Guid.Empty : Guid.Empty;
			item.SetValue(companyPK, branchPK, departmentPK, value);
		}

		public static void AssertItemNamesAreUnique(IEnumerable<IRegistryItem> items)
		{
			List<List<IRegistryItem>> duplicateItems = new List<List<IRegistryItem>>();
			Dictionary<string, List<IRegistryItem>> itemDictionary = new Dictionary<string, List<IRegistryItem>>();

			foreach (IRegistryItem item in items)
			{
				if (item.GetType() != typeof(LinkRegistryItem))
				{
					List<IRegistryItem> itemList;
					string upperCaseName = item.Name.ToUpperInvariant();

					if (itemDictionary.TryGetValue(upperCaseName, out itemList))
					{
						duplicateItems.Add(itemList);
					}
					else
					{
						itemDictionary[upperCaseName] = itemList = new List<IRegistryItem>();
					}

					itemList.Add(item);
				}
			}

			FailIfHasDuplicateItems(duplicateItems, itemDictionary);
		}

		static void FailIfHasDuplicateItems(List<List<IRegistryItem>> duplicateItems, Dictionary<string, List<IRegistryItem>> itemDictionary)
		{
			if (duplicateItems.Count > 0)
			{
				StringBuilder message = new StringBuilder();
				message.Append("The following registry item names have been duplicated:\r\n\r\n");

				foreach (List<IRegistryItem> itemList in duplicateItems)
				{
					string name = itemList[0].Name;
					message.Append(name);
					message.Append(": ");

					for (int i = 0; i < itemList.Count; i++)
					{
						if (i > 0)
						{
							message.Append(", ");
						}
						message.Append(itemList[i].Category);
						message.Append(RegistryItemSet.Delimiter);
						message.Append(itemList[i].Caption);
					}

					message.AppendLine();
				}

				TestCase.Fail(message.ToString());
			}
			else
			{
				TestCase.Assert(true);
			}
		}

		static bool HasBranchStorage(IRegistryItem item)
		{
			return (item.Storage & (RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment)) != 0;
		}

		static bool HasCompanyStorage(IRegistryItem item)
		{
			return (item.Storage & (RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment)) != 0;
		}

		static bool HasDepartmentStorage(IRegistryItem item)
		{
			return (item.Storage & (RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.SystemDepartment)) != 0;
		}
	}
}
