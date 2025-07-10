using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestsSubclassesOf(typeof(RegistryItemSet))]
	public abstract class RegistryItemSetTestCase<T> : TransactionedTestCase where T : RegistryItemSet
	{
		public void TestAllItemsShouldBeAdded()
		{
			List<string> propertyNames = new List<string>();
			AddMissingItems(GetIRegistryItemProperties(true), propertyNames);

			if (propertyNames.Count > 0)
			{
				StringBuilder message = new StringBuilder();
				message.AppendLine("The following properties of type IRegistryItem were not returned in GetAllItems(). If they are not meant to be shown on the Registry form, please add a RegistryOptions.IsHidden flag.");
				message.AppendLine();
				foreach (string propertyName in propertyNames)
				{
					message.AppendLine(propertyName);
				}
				Fail(message.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestClassIsSealed()
		{
			AssertEquals("Subclasses of RegistryItemSet must be sealed.", true, typeof(T).IsSealed);
		}

		public virtual void TestItemsAreAddedProperly()
		{
			PropertyInfo[] propertyInfos = GetIRegistryItemProperties(true);

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				RegistryItemDictionary.Instance.PurgeAll();
				IRegistryItem item = (IRegistryItem)propertyInfo.GetValue(ItemSet, null);

				if (item.GetType() == typeof(LinkRegistryItem))
				{
					AssertNull("The registry item with the property name \"" + propertyInfo.Name + "\" is a LinkRegistryItem and should not be added to the dictionary.", RegistryItemDictionary.Instance.GetItem(item.Name));
				}
				else
				{
					IRegistryItem itemInDictionary = RegistryItemDictionary.Instance.GetItem(item.Name);
					AssertNotNull("The registry item with the property name \"" + propertyInfo.Name + "\" should be added to the dictionary. Please ensure that the key matches the property name of the registry item.", itemInDictionary);
					AssertEquals("The registry item with the property name \"" + propertyInfo.Name + "\" should be added to the dictionary.", item, itemInDictionary);
					AssertEquals("The property \"" + propertyInfo.Name + "\" should return the same instance when accessed multiple times.", item, propertyInfo.GetValue(ItemSet, null));
					AssertEquals("The registry item should be added with its name as the key.", item, RegistryItemDictionary.Instance.GetItem(item.Name));
				}

				int initialItemCount = DictionaryInternals.Count;
				propertyInfo.GetValue(ItemSet, null);
				AssertEquals("No new registry items should be added when accessing the same property multiple times.", initialItemCount, DictionaryInternals.Count);
			}
		}

		public void TestItemsAreNotStatic()
		{
			PropertyInfo[] propertyInfos = GetIRegistryItemProperties(false);

			if (propertyInfos.Length > 0)
			{
				List<string> propertyNames = new List<string>();

				foreach (PropertyInfo propertynfo in propertyInfos)
				{
					propertyNames.Add(propertynfo.Name);
				}

				StringBuilder message = new StringBuilder();
				message.AppendLine("The following properties of type IRegistryItem are static. IRegistryItem properties must not be static.");
				message.AppendLine();
				foreach (string name in propertyNames)
				{
					message.AppendLine(name);
				}

				Fail(message.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public virtual void TestItemsCanGetAndSetValue()
		{
			object value;
			foreach (IRegistryItem item in AllItems)
			{
				if (item.GetType() != typeof(LinkRegistryItem))
				{
					value = item.DefaultValue;
					int option = (int)(item.Options & RegistryOptions.CannotCallParameterlessValueGetter);
					if (option == 0)
					{
						value = item.Value;
					}
					value = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					using (item.DataType.SuspendValidation())
					{
						RegistryTester.SetValue(item, value);
					}
				}
			}
		}

		public void TestItemNamesCategoriesAndCaptionsAreValid()
		{
			StringBuilder errorMessage = new StringBuilder();

			string[] reservedNames =
			{
				LinkRegistryItem.Name,
				"FilterCriteria",
				"GridLayout",
			};

			foreach (IRegistryItem item in AllItems)
			{
				bool isLinkRegistryItem = item.GetType() == typeof(LinkRegistryItem);

				if ((item.Name == null) || (item.Name.Trim().Length == 0))
				{
					errorMessage.AppendFormat("A registry item of type {0} with a category of \"{1}\" has an empty name. Empty names are not allowed.", item.GetType().Name, item.Category);
					errorMessage.AppendLine();
				}
				else
				{
					if (!isLinkRegistryItem)
					{
						foreach (string reservedName in reservedNames)
						{
							if (item.Name.StartsWith(reservedName, StringComparison.OrdinalIgnoreCase))
							{
								PropertyInfo propertyInfo = GetPropertyInfoFromItem(item);
								if ((propertyInfo == null) || (propertyInfo.GetCustomAttributes(typeof(RegistryItemReservedNameTestExcludeAttribute), false).Length == 0))
								{
									errorMessage.AppendFormat("A registry item of type {0} has a name starting with \"{1}\". This is a reserved name and cannot be used.", item.GetType().Name, reservedName);
									errorMessage.AppendLine();
								}
								break;
							}
						}
					}
				}

				if (isLinkRegistryItem || !item.HasOption(RegistryOptions.IsHidden))
				{
					if ((item.Category == null) || (item.Category.Trim().Length == 0))
					{
						errorMessage.AppendFormat("A registry item of type {0} with a name of \"{1}\" has an empty category. Empty categories are not allowed.", item.GetType().Name, item.Name);
						errorMessage.AppendLine();
					}
					if ((item.Caption == null) || (item.Caption.Trim().Length == 0))
					{
						errorMessage.AppendFormat("A registry item of type {0} with a name of \"{1}\" has an empty caption. Empty captions are not allowed.", item.GetType().Name, item.Name);
						errorMessage.AppendLine();
					}
				}
			}

			if (errorMessage.Length > 0)
			{
				Fail(errorMessage.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestItemNamesAreUnique()
		{
			RegistryTester.AssertItemNamesAreUnique(AllItems);
		}

		public void TestNoItemsAreLoadedOnConstruction()
		{
			RegistryItemDictionary.Instance.PurgeAll();
			GetNewItemSet();
			AssertEquals("No registry items should be created on construction.", 0, DictionaryInternals.Count);
		}

		public void TestNoRegistryItemFields()
		{
			ItemSet.GetAllItems();

			List<FieldInfo> fieldInfos = new List<FieldInfo>();
			fieldInfos.AddRange(typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance));
			fieldInfos.AddRange(typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static));
			fieldInfos.AddRange(typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
			fieldInfos.AddRange(typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Static));

			for (int i = (fieldInfos.Count - 1); i >= 0; i--)
			{
				if (!(typeof(IRegistryItem).IsAssignableFrom(fieldInfos[i].FieldType)))
				{
					fieldInfos.RemoveAt(i);
				}
			}

			if (fieldInfos.Count > 0)
			{
				StringBuilder errorMessage = new StringBuilder();
				errorMessage.AppendLine("Some fields of type IRegistryItem were found. Registry items should not be stored on fields.");
				foreach (FieldInfo info in fieldInfos)
				{
					errorMessage.Append(info.FieldType.Name);
					errorMessage.Append("^");
					errorMessage.Append(info.Name);
					errorMessage.Append("^");
					errorMessage.AppendLine(((IRegistryItem)info.GetValue(ItemSet)).Name);
				}
				Fail(errorMessage.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			RegistryItemDictionary.Instance.PurgeAll();
			resourcesDemanded = new List<string>();
			Res.ResourceDemanded += Res_ResourceDemanded;
			try
			{
				foreach (IRegistryItem item in FilteredItems(AllowDemandResourceStrings))
				{
					try
					{
						object value = item.Value;
					}
					catch { }
				}
				if (resourcesDemanded.Count == 0)
				{
					Assert(true);
				}
				else
				{
					string[] errors = new string[Math.Min(resourcesDemanded.Count, 50)];
					resourcesDemanded.CopyTo(0, errors, 0, errors.Length);
					Fail("Resources should not be demanded when accessing registry values.\r\nUse MultilingualString to separate the definition of resource strings from their resolution at runtime.\r\n" +  // Cryptic, eh?
						"One explanation for this could be that the constructor of your StronglyTypedRegistryItem<TCollection> asks for Caption, Hint, etc as strings and then CASTS them to NoResString, perhaps when calling the constructor of RegistryItemImpl to pass that to the constrcutor of base StronglyTypedRegistryItem. Try fixing it by changing the signature of your StronglyTypedRegistryItem<TCollection> so that it takes MultiLingualStrings and remove the casting.\r\n\r\n" +
						resourcesDemanded.Count + " Resources Demanded\r\n\r\n" + string.Join("\r\n\r\n", errors));
				}
			}
			finally
			{
				Res.ResourceDemanded -= Res_ResourceDemanded;
			}
		}
		List<string> resourcesDemanded;
		void Res_ResourceDemanded(object sender, ResourceStringDemandedEventArgs e)
		{
			string stackTrace = System.Environment.StackTrace;
			int index = stackTrace.IndexOf("get_StackTrace()");
			stackTrace.IndexOf("\n", index);
			stackTrace = "at " + stackTrace.Substring(index);
			index = stackTrace.IndexOf("TestAccessingValuesOnlyDoesNotDemandResourceStrings()");
			stackTrace = stackTrace.Substring(0, index);
			resourcesDemanded.Add("ResourceDemanded " + e.Key + " " + stackTrace);
		}

		public void TestCategoriesAndCaptionsAreLocalizable()
		{
			if (IsCountrySpecificRegistrySet || IsClientSpecificRegistrySet)
			{
				Assert(true);
				return;
			}

			StringBuilder errors = new StringBuilder();
			using (IMockResourceStringCache mockData = Res.UseMockData())
			{
				mockData.SetResourceGetter(GetResourceString);
				foreach (IRegistryItem item in AllItems)
				{
					if (ItemShouldBeLocalizable(item))
					{
						CheckLocalizable(string.Format("Category \"{0}\" for registry item \"{1}\" is not localizable.", item.Category, item.Name), item.Category, errors);
						CheckLocalizable(string.Format("Caption \"{0}\" for registry item \"{1}\" is not localizable.", item.Caption, item.Name), item.Caption, errors);
						CheckLocalizable(string.Format("Hint \"{0}\" for registry item \"{1}\" is not localizable.", item.Hint, item.Name), item.Hint, errors);
					}
					else
					{
						CheckNotLocalizable(string.Format("Caption \"{0}\" for registry item \"{1}\" should not be localizable, it is for CargoWise use only. If this registry item is conditionally visible then override ConditionallyVisibleRegistryItems.", item.Caption, item.Name), item.Caption, errors);
						CheckNotLocalizable(string.Format("Hint \"{0}\" for registry item \"{1}\" should not be localizable, it is for CargoWise use only. If this registry item is conditionally visible then override ConditionallyVisibleRegistryItems.", item.Hint, item.Name), item.Hint, errors);
					}
				}
			}
			Assert(errors.ToString(), errors.Length == 0);
		}

		protected virtual bool IsCountrySpecificRegistrySet
		{
			get { return GetType().Assembly.GetName().Name.StartsWith("Enterprise.Customs."); }
		}

		protected virtual bool IsClientSpecificRegistrySet
		{
			get { return GetType().Assembly.GetName().Name.StartsWith("ZClient"); }
		}

		bool ItemShouldBeLocalizable(IRegistryItem item)
		{
			RegistryOptions options;
			if (item is LinkRegistryItem)
			{
				options = RegistryOptions.Default;
			}
			else
			{
				options = item.Options;
			}

			var localizationIgnoreMask = RegistryOptions.IsHidden | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise;
			return ((options & localizationIgnoreMask) == RegistryOptions.Default) || ConditionallyVisibleRegistryItems.Contains(item.Name);
		}

		protected virtual IEnumerable<string> ConditionallyVisibleRegistryItems => new[]
		{
			"UseAddressValidationWebService",
			"DatabaseRecoveryModel",
			"UserEventTrackingExternal",
			"BorderWiseUmpApiBaseAddress",
			"RunConvertApplicationStatusToCandidateRating",
			"DeniedPartyScreeningMatchingConfiguration",
			"EnableConsolidatedEntries",
			"ConsolidatedEntryNumberCustomisation",
			"EnableComplianceRisk",
			"HeartbeatDuration",
			"EHubTesting",
			"ServiceTaskHeartbeatDuration",
			"ConsignmentPackageLabelsMandatoryOnLoad",
		};

		void CheckLocalizable(string message, string value, StringBuilder errors)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (value.IndexOf(mockResourceStringDataValue) == -1)
				{
					errors.AppendLine(message);
				}
				value = value.Replace(mockResourceStringDataValue, "");
				foreach (char ch in value)
				{
					if (char.IsLetter(ch))
					{
						errors.AppendLine(message);
						return;
					}
				}
			}
		}

		void CheckNotLocalizable(string message, string value, StringBuilder errors)
		{
			if (!string.IsNullOrEmpty(value) && value.IndexOf(mockResourceStringDataValue) > -1)
			{
				errors.AppendLine(message);
			}
		}

		ResourceStringData GetResourceString(string key)
		{
			return new ResourceStringData(key, string.Empty, string.Empty, mockResourceStringDataValue, string.Empty);
		}

		const string mockResourceStringDataValue = "___MOCK_";

		IEnumerable<IRegistryItem> FilteredItems(IEnumerable<string> excludedPropertyNames)
		{
			var result = new List<IRegistryItem>();

			var type = GetType();
			var propertyInfos = new List<PropertyInfo>();
			propertyInfos.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
			propertyInfos.AddRange(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

			foreach (var propertyInfo in propertyInfos.Where(prop => !excludedPropertyNames.Contains(prop.Name)))
			{
				if (typeof(IRegistryItem).IsAssignableFrom(propertyInfo.PropertyType))
				{
					result.Add((IRegistryItem)propertyInfo.GetValue(this, null));
				}
			}

			return result;
		}

		protected ReadOnlyCollection<IRegistryItem> AllItems
		{
			get
			{
				if (allItems == null)
				{
					allItems = new ReadOnlyCollection<IRegistryItem>(ItemSet.GetAllItems());
				}
				return allItems;
			}
		}

		protected virtual IEnumerable<string> AllowDemandResourceStrings { get; } = Enumerable.Empty<string>();

		IRegistryItemDictionaryInternals DictionaryInternals
		{
			get { return RegistryItemDictionary.Instance; }
		}

		protected T ItemSet
		{
			get
			{
				if (itemSet == null)
				{
					itemSet = GetNewItemSet();
				}
				return itemSet;
			}
		}

		protected virtual T GetNewItemSet()
		{
			return (T)Activator.CreateInstance(typeof(T), true);
		}

		void AddMissingItems(PropertyInfo[] propertyInfos, List<string> propertyNames)
		{
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (typeof(IRegistryItem).IsAssignableFrom(propertyInfo.PropertyType))
				{
					IRegistryItem item = (IRegistryItem)propertyInfo.GetValue(ItemSet, null);
					if ((item == null) || !AllItems.Contains(item))
					{
						bool missing = true;
						LinkRegistryItem linkItem = item as LinkRegistryItem;
						if (linkItem != null)
						{
							foreach (IRegistryItem existingItem in AllItems)
							{
								LinkRegistryItem existingLinkItem = existingItem as LinkRegistryItem;
								if (existingLinkItem != null &&
									existingItem.Caption == item.Caption &&
									existingLinkItem.ModuleID == linkItem.ModuleID)
								{
									missing = false;
									break;
								}
							}
						}
						if (missing)
						{
							propertyNames.Add(propertyInfo.Name);
						}
					}
				}
			}
		}

		PropertyInfo[] GetIRegistryItemProperties(bool instance)
		{
			List<PropertyInfo> result = new List<PropertyInfo>();
			BindingFlags instanceFlag = instance ? BindingFlags.Instance : BindingFlags.Static;
			result.AddRange(typeof(T).GetProperties(BindingFlags.Public | instanceFlag));
			result.AddRange(typeof(T).GetProperties(BindingFlags.NonPublic | instanceFlag));

			for (int i = (result.Count - 1); i >= 0; i--)
			{
				PropertyInfo info = result[i];
				if (!(typeof(IRegistryItem).IsAssignableFrom(info.PropertyType)))
				{
					result.Remove(info);
				}
			}

			return result.ToArray();
		}

		PropertyInfo GetPropertyInfoFromItem(IRegistryItem item)
		{
			foreach (PropertyInfo propertyInfo in GetIRegistryItemProperties(true))
			{
				if (propertyInfo.GetValue(ItemSet, null) == item)
				{
					return propertyInfo;
				}
			}
			return null;
		}

		ReadOnlyCollection<IRegistryItem> allItems;
		T itemSet;

		#region Assert Visible

		protected void AssertVisible(string name)
		{
			AssertVisible(name, false);
		}

		protected void AssertVisible(string name, bool developerOnly)
		{
			AssertVisible(name, true, developerOnly);
		}

		protected void AssertVisible(IRegistryItem item)
		{
			AssertVisible(item, false);
		}

		protected void AssertVisible(IRegistryItem item, bool developerOnly)
		{
			AssertVisible(item, true, developerOnly);
		}

		protected void AssertNotVisible(string name)
		{
			AssertVisible(name, false, false);
		}

		protected void AssertNotVisible(IRegistryItem item)
		{
			AssertVisible(item, false, false);
		}

		void AssertIsHiddenAndIsOnlyForDevelopers(IRegistryItem item, bool isHidden, bool isOnlyForDevelopers)
		{
			AssertEquals("\"" + item.Name + "\".HasOption(RegistryOptions.IsHidden)", isHidden, item.HasOption(RegistryOptions.IsHidden));
			AssertEquals("\"" + item.Name + "\".HasOption(RegistryOptions.IsOnlyForDevelopers)", isOnlyForDevelopers, item.HasOption(RegistryOptions.IsOnlyForDevelopers));
		}

		void AssertVisible(string name, bool visible, bool developerOnly)
		{
			IRegistryItem item = ItemSet.FindByName(name);
			AssertNotNull("GetAllItems() should contain a registry item with the name of \"" + name + "\".", item);
			AssertIsHiddenAndIsOnlyForDevelopers(item, !visible, developerOnly);
		}

		void AssertVisible(IRegistryItem item, bool visible, bool developerOnly)
		{
			AssertEquals("GetAllItems() should contain a registry item with the name of \"" + item.Name + "\".", true, AllItems.Contains(item));
			AssertIsHiddenAndIsOnlyForDevelopers(item, !visible, developerOnly);
		}

		#endregion
	}
}
