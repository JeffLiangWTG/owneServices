using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestsSubclassesOf(typeof(EntryChargeTypeList), typeof(ExcludeEntryChargeTypeListFromTestAttribute), new Type[] { typeof(EmptyEntryChargeTypeList) }, ExcludePrivate = true)]
	public abstract class EntryChargeTypeListTestCase : TestCase
	{
		public void TestRegistryItemDoesntReturnNull()
		{
			AssertNotNull(ChargeTypeList.RegistryItem);
		}

		public void TestCountrySpecificListIsMappedInEnterpriseApplicationConfiguration()
		{
			Hashtable objectHashtables = (Hashtable)ObjectFactory.Get("CustomsEntryChargeTypeList");
			Dictionary<string, EntryChargeTypeList> allList = new Dictionary<string, EntryChargeTypeList>(objectHashtables.Count);

			foreach (DictionaryEntry entry in objectHashtables)
			{
				ObjectHandle handle = (ObjectHandle)entry.Value;
				allList.Add((string)entry.Key, (EntryChargeTypeList)handle.GetObject());
			}

			AssertNotNull("Accounting uses this method to tell Customs charges from any other charges. If you have a list for a new country, the list should be mapped in CustomsEntryChargeTypeList in EnterpriseApplicationConfiguration.xml", allList[CountryCode]);
		}

		#region Implementation

		protected void AssertListElementIsCorrect(EntryChargeType element, ZString code, ZString description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
		{
			AssertEquals("Code", code, element.Code);
			AssertEquals("Description", description, element.Description);
			AssertEquals("IsPaidWhenMessageClears", isPaidWhenMessageClears, element.IsPaidWhenMessageClears);
			AssertEquals("ParentCodeForGSTOnARInvoice", parentCodeForGSTOnARInvoice, element.ParentCodeForGSTOnARInvoice);
		}

		protected EntryChargeTypeList ChargeTypeList
		{
			get
			{
				if (fChargeTypeList == null)
				{
					fChargeTypeList = GetNewEntryChargeTypeList();
				}
				return fChargeTypeList;
			}
		}
		EntryChargeTypeList fChargeTypeList;

		protected abstract EntryChargeTypeList GetNewEntryChargeTypeList();
		protected abstract ZString CountryCode { get; }

		#endregion
	}
}
