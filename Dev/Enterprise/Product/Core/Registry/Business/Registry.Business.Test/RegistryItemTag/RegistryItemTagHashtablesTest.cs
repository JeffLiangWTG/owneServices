using System;
using System.Collections;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryItemTagHashtablesTest : TestCase
	{
		RegistryItemTagHashtablesForTest ItemHash;

		protected override void SetUp()
		{
			base.SetUp();
			ItemHash = new RegistryItemTagHashtablesForTest();
		}

		public void TestGetAndSetIsChanged()
		{
			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.Empty;
			Guid departmentPK = Guid.NewGuid();

			AssertEquals("ItemHash.GetHasValue()", false, ItemHash.GetIsChanged(companyPK, branchPK, departmentPK));

			ItemHash.SetIsChanged(companyPK, branchPK, departmentPK, true);
			AssertEquals("ItemHash.GetHasValue()", true, ItemHash.GetIsChanged(companyPK, branchPK, departmentPK));
			ItemHash.SetIsChanged(companyPK, branchPK, departmentPK, false);
			AssertEquals("ItemHash.GetHasValue()", false, ItemHash.GetIsChanged(companyPK, branchPK, departmentPK));

			companyPK = Guid.Empty;
			branchPK = Guid.NewGuid();
			ItemHash.SetIsChanged(companyPK, branchPK, departmentPK, true);
			AssertEquals("ItemHash.GetHasValue()", true, ItemHash.GetIsChanged(companyPK, branchPK, departmentPK));
		}

		public void TestGetIsChangedKeys()
		{
			Guid branchPK = Guid.NewGuid();
			ItemHash.SetIsChanged(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemHash.SetIsChanged(Guid.Empty, branchPK, Guid.Empty, false);

			bool foundFirst = false;
			bool foundSecond = false;
			foreach (string key in ItemHash.GetIsChangedKeys())
			{
				if (key == ItemHash.GetKeyForTest(Guid.Empty, Guid.Empty, Guid.Empty))
				{
					foundFirst = true;
				}
				if (key == ItemHash.GetKeyForTest(Guid.Empty, branchPK, Guid.Empty))
				{
					foundSecond = true;
				}
			}

			Assert("First key was not found", foundFirst);
			Assert("Second key was not found", foundSecond);
		}

		public void TestClearIsChangedHashtable()
		{
			ItemHash.SetIsChanged(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemHash.SetIsChanged(Guid.Empty, Guid.NewGuid(), Guid.Empty, false);
			ItemHash.ClearIsChangedHashtable();
			AssertEquals("IsChangedHashtable should have nothing in it", 0, ItemHash.GetIsChangedHashtable().Count);
		}

		public void TestGetKey()
		{
			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			AssertEquals("GetKey()", companyPK.ToString() + "+" + branchPK.ToString() + "+" + departmentPK.ToString(),
				ItemHash.GetKeyForTest(companyPK, branchPK, departmentPK));
		}

		#region RegistryItemTagHashtablesForTest

		class RegistryItemTagHashtablesForTest : RegistryItemTagHashtables
		{
			public Hashtable GetIsChangedHashtable()
			{
				return IsChangedHashtable;
			}

			public string GetKeyForTest(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetKey(companyPK, branchPK, departmentPK);
			}
		}

		#endregion
	}
}
