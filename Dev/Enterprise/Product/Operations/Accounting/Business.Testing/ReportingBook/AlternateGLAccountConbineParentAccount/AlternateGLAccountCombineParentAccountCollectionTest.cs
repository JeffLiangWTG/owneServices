using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccountCombineParentAccountCollection))]
	public class AlternateGLAccountCombineParentAccountCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternateGLAccountCombineParentAccountCollection>
	{
		#region TestLoadingFromFilterString

		public void TestLoadingFromFilterString()
		{
			PrepareData();

			var filterHelper = new AlternateGLAccountFilterHelper(Factory);
			AlternateGLAccountCombineParentAccounts.SetFilterHelper(filterHelper);
			AlternateGLAccountCombineParentAccounts.Load();

			AssertEquals("There should be 3 records in the collection", 3, AlternateGLAccountCombineParentAccounts.Count);

			filterHelper.SetOuterQuery(new ZQuery(AccAlternateGLAccountSchema.AGA_AccountNum, "10.00.1000"));
			AlternateGLAccountCombineParentAccounts.Load();
			AssertEquals("There should be 2 records in the collection", 2, AlternateGLAccountCombineParentAccounts.Count);

			filterHelper.SetOuterQuery(new ZQuery(AccAlternateGLAccountSchema.AGA_AccountNum, "10.00.1010"));
			AlternateGLAccountCombineParentAccounts.Load();
			AssertEquals("There should be 1 records in the collection", 1,
				AlternateGLAccountCombineParentAccounts.Count);

			CombineAssertions("Chart Info", () =>
			{
				AssertEquals("MGT", AlternateGLAccountCombineParentAccounts[0].ChartCode);
				AssertEquals("Management Reporting", AlternateGLAccountCombineParentAccounts[0].ChartName);
			});

			CombineAssertions("Attributes", () =>
			{
				AssertEquals("TPY - Third Party", AlternateGLAccountCombineParentAccounts[0].OCGAttribute);
				AssertEquals("AALSHI", AlternateGLAccountCombineParentAccounts[0].ORGAttribute);
				AssertEquals("WEU - Within EU", AlternateGLAccountCombineParentAccounts[0].LFEAttribute);
				AssertEquals("LOC - Local", AlternateGLAccountCombineParentAccounts[0].LFOAttribute);
				AssertEquals("STI - Standard Tax IDs", AlternateGLAccountCombineParentAccounts[0].TICAttribute);
				AssertEquals("SPS - Sales/Purchases", AlternateGLAccountCombineParentAccounts[0].SPRAttribute);
			});
		}

		void PrepareData()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT", "Management Reporting");
			creator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			AccAlternateGLAccount1 = creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var account2 = creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", "P&L", "CR", 1, "AS", 2);

			var glHeader1 = creator.CreateGLHeader("Test.aa");
			var glHeader2 = creator.CreateGLHeader("Test.bb");

			creator.CreateAccAlternateGlAccountAttribute(AccAlternateGLAccount1, glHeader1.PK, 1, "OCG", "OCG");
			creator.CreateAccAlternateGlAccountAttribute(AccAlternateGLAccount1, glHeader2.PK, 2, "OCG", "TPY");
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "OCG", "TPY");
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "ORG", attributeValuePK: creator.AALSHI.PK.ToGuid());
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "LFE", "WEU");
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "LFO", "LOC");
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "SPR", "SPS");
			creator.CreateAccAlternateGlAccountAttribute(account2, glHeader1.PK, 2, "TIC", "STI");
			Factory.Save();
		}

		#endregion

		public void TestFindBoxListProvider()
		{
			PrepareData();
			var collection = GetCollectionToTest();
			var findBoxListProvider = new AlternateGLAccountCombineParentAccountCollectionFindBoxListProvider(collection);

			AssertEquals(string.Empty, findBoxListProvider.DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertEquals(AccAlternateGLAccount1.AGA_Description, findBoxListProvider.DescriptionFromPrimaryKey(AccAlternateGLAccount1.PK));

			AssertEquals(string.Empty, findBoxListProvider.CodeFromPrimaryKey(ZGuid.NewZGuid()));
			AssertEquals(AccAlternateGLAccount1.AGA_AccountNum, findBoxListProvider.CodeFromPrimaryKey(AccAlternateGLAccount1.PK));

			AssertEquals(Guid.Empty, findBoxListProvider.PrimaryKeyFromCode(string.Empty));
			AssertEquals(AccAlternateGLAccount1.PK, findBoxListProvider.PrimaryKeyFromCode(AccAlternateGLAccount1.AGA_AccountNum));
		}

		#region Implementation

		protected override AlternateGLAccountCombineParentAccountCollection GetCollectionToTest()
		{
			return new AlternateGLAccountCombineParentAccountCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AlternateGLAccountCombineParentAccount(Factory);
		}

		AlternateGLAccountCombineParentAccountCollection fAccountCollection;
		protected AlternateGLAccountCombineParentAccountCollection AlternateGLAccountCombineParentAccounts => fAccountCollection ?? (fAccountCollection = new AlternateGLAccountCombineParentAccountCollection(Factory));

		#endregion

		#region TestAfterLoaded Event

		public void TestAfterLoadedEvent()
		{
			var aRFilterHelper = new AlternateGLAccountFilterHelper(Factory);
			AlternateGLAccountCombineParentAccounts.SetFilterHelper(aRFilterHelper);

			AssertEquals("Pre-condition", false, eventFired);
			AlternateGLAccountCombineParentAccounts.AfterLoaded += AfterLoadedHandler;
			AlternateGLAccountCombineParentAccounts.Load();
			AssertEquals("EvenHandler should be called", true, eventFired);
			AlternateGLAccountCombineParentAccounts.AfterLoaded -= AfterLoadedHandler;
		}

		void AfterLoadedHandler(object source, EventArgs args)
		{
			eventFired = true;
		}
		bool eventFired;

		#endregion

		AccAlternateGLAccount AccAlternateGLAccount1;
	}
}
