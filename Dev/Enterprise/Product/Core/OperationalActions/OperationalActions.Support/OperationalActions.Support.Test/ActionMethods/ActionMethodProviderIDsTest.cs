using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	internal sealed class ActionMethodProviderIDsTest : TestCase
	{
		public void TestSanity()
		{
			IDictionary<string, IList<string>> brokenIDs = new SortedDictionary<string, IList<string>>();
			List<string> errors = new List<string>();
			foreach (FieldInfo info in typeof(ActionMethodProviderIDs).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				CheckFieldInfo(info, errors);
				if (errors.Count > 0)
				{
					brokenIDs.Add(info.Name, errors);
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("The following ActionMethodIDs have errors that need to be fixed.", brokenIDs);
		}

		static void CheckFieldInfo(FieldInfo info, List<string> errors)
		{
			if (!info.IsPublic)
			{
				errors.Add("Should be public");
			}

			if (!info.IsInitOnly)
			{
				errors.Add("Should be readonly");
			}

			if (info.FieldType != typeof(ActionMethodProviderID))
			{
				errors.Add("Of the wrong type, should be ActionMethodID");
			}
			else
			{
				ActionMethodProviderID id = (ActionMethodProviderID)info.GetValue(null);
				if (id == null)
				{
					errors.Add("Should not be null");
				}
				else
				{
					if (string.IsNullOrEmpty(id.Name))
					{
						errors.Add(string.Format("'{0}' has no Name", id.Guid));
					}

					if (ActionMethodProviderIDs.FindByGuid(id.Guid) != id)
					{
						errors.Add("FindByGuid returned a different id");
					}

					Type actionMethodType;
					if (id.ProviderFullName.Length == 0)
					{
						errors.Add("Provider type full name is empty");
					}
					else if ((actionMethodType = Type.GetType(id.ProviderFullName)) == null)
					{
						errors.Add(string.Format("Unable to find \"{0}\"", id.ProviderFullName));
					}
					else
					{
						try
						{
							Activator.CreateInstance(actionMethodType);
						}
						catch (Exception ex)
						{
							errors.Add(string.Format("Caught exception while trying to instanciate '{0}':\n{1}", actionMethodType.Name, ex.ToString()));
						}
					}
				}
			}
		}

		public void TestNoDuplicates()
		{
			IDictionary<string, IList<string>> lookup = new SortedDictionary<string, IList<string>>();
			foreach (FieldInfo info in typeof(ActionMethodProviderIDs).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				ActionMethodProviderID id = info.GetValue(null) as ActionMethodProviderID;
				if (id != null)
				{
					IList<string> list;
					if (!lookup.TryGetValue(id.ProviderFullName, out list))
					{
						list = new List<string>();
						lookup.Add(id.ProviderFullName, list);
					}

					list.Add(info.Name);
				}
			}

			IDictionary<string, IList<string>> trimmedLookup = new SortedDictionary<string, IList<string>>();
			foreach (KeyValuePair<string, IList<string>> pair in lookup)
			{
				if (pair.Value.Count > 1)
				{
					trimmedLookup.Add(pair);
				}
			}

			AssertGroupedErrorList("The following providers are referenced by multiple ActionMethodID's", trimmedLookup);
		}

		public void TestUniqueNames()
		{
			IDictionary<string, IList<string>> lookup = new SortedDictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (FieldInfo info in typeof(ActionMethodProviderIDs).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				ActionMethodProviderID id = info.GetValue(null) as ActionMethodProviderID;
				if (id != null)
				{
					IList<string> list;
					if (!lookup.TryGetValue(id.Name, out list))
					{
						list = new List<string>();
						lookup.Add(id.Name, list);
					}

					list.Add(info.Name);
				}
			}

			IDictionary<string, IList<string>> trimmedLookup = new SortedDictionary<string, IList<string>>();
			foreach (KeyValuePair<string, IList<string>> pair in lookup)
			{
				if (pair.Value.Count > 1)
				{
					trimmedLookup.Add(pair);
				}
			}

			AssertGroupedErrorList("The following names are used by multiple ActionMethodID's", trimmedLookup);
		}

		public void TestUniqueGuid()
		{
			IDictionary<ZGuid, IList<string>> lookup = new SortedDictionary<ZGuid, IList<string>>();
			foreach (FieldInfo info in typeof(ActionMethodProviderIDs).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				ActionMethodProviderID id = info.GetValue(null) as ActionMethodProviderID;
				if (id != null)
				{
					IList<string> list;
					if (!lookup.TryGetValue(id.Guid, out list))
					{
						list = new List<string>();
						lookup.Add(id.Guid, list);
					}

					list.Add(info.Name);
				}
			}

			IDictionary<string, IList<string>> trimmedLookup = new SortedDictionary<string, IList<string>>();
			foreach (KeyValuePair<ZGuid, IList<string>> pair in lookup)
			{
				if (pair.Value.Count > 1)
				{
					trimmedLookup.Add(pair.Key.ToString(), pair.Value);
				}
			}

			AssertGroupedErrorList("The following guids are used by multiple ActionMethodID's", trimmedLookup);
		}
	}
}
