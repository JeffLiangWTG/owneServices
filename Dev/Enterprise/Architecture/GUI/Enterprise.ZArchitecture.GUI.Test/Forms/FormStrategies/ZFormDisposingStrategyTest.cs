using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	public class ZFormDisposingStrategyTest : TransactionedTestCase
	{
		public void TestFormDoesNotLeakWhenOnLoadedCalledDirectly()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(ExceptionReportingForm)); // Ensure loading of ZFormStrategy class

			var initialCount = Application.OpenForms.Count;

			var form = new KForm();
			try
			{
				form.Show();
				AssertEquals("Creating form should increase OpenForm count", Application.OpenForms.Count, initialCount + 1);
			}
			finally
			{
				form.Hide();
				form.Dispose();
			}

			AssertEquals("Count should return to initial count", initialCount, Application.OpenForms.Count);
		}

		public void TestExpiredRegistryItemsArePurgedOnDispose()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(ExceptionReportingForm)); // Ensure loading of ZFormStrategy class

			using (new KForm())
			{
				IRegistryItemDictionaryInternals dictionaryInternals = RegistryItemDictionary.Instance;
				RegistryItemDictionary.Instance.PurgeAll();
				for (var i = 0; i < dictionaryInternals.ItemThreshold; i++)
				{
					var item = new StringRegistryItem("DUMMY_ITEM_" + i.ToString(), null, null, null, RegistryStorageFlags.System);
					RegistryItemDictionary.Instance.Add(item);
				}

				AssertNotNull("DUMMY_ITEM_0 should be in the dictionary.", RegistryItemDictionary.Instance.GetItem("DUMMY_ITEM_0"));
				AssertNotNull("DUMMY_ITEM_1 should be in the dictionary.", RegistryItemDictionary.Instance.GetItem("DUMMY_ITEM_2"));
				var newItem1 = new StringRegistryItem("NEW_ITEM_1", null, null, null, RegistryStorageFlags.System);
				var newItem2 = new StringRegistryItem("NEW_ITEM_2", null, null, null, RegistryStorageFlags.System);
				RegistryItemDictionary.Instance.Add(newItem1);
				RegistryItemDictionary.Instance.Add(newItem2);
				dictionaryInternals.GetHolder("NEW_ITEM_1").ElapsedSinceLastUse = new TimeSpan(0, (dictionaryInternals.TimeoutMinutes + 1), 0);
				dictionaryInternals.GetHolder("NEW_ITEM_2").ElapsedSinceLastUse = new TimeSpan(0, (dictionaryInternals.TimeoutMinutes + 1), 0);
			}

			AssertNotNull("DUMMY_ITEM_0 should be in the dictionary.", RegistryItemDictionary.Instance.GetItem("DUMMY_ITEM_0"));
			AssertNotNull("DUMMY_ITEM_1 should be in the dictionary.", RegistryItemDictionary.Instance.GetItem("DUMMY_ITEM_2"));
			AssertNull("NEW_ITEM_1 should not be in the dictionary.", RegistryItemDictionary.Instance.GetItem("NEW_ITEM_1"));
			AssertNull("NEW_ITEM_2 should not be in the dictionary.", RegistryItemDictionary.Instance.GetItem("NEW_ITEM_2"));
		}
	}
}
