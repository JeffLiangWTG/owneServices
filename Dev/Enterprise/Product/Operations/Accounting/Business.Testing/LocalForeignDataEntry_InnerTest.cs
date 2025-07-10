using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
	public class LocalForeignDataEntry_InnerTest : TestCaseWithFactory
	{
		public void TestAmountsTogether()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				TestObject.Z0_Foreign = 0;
				TestObject.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObject.Z0_AnotherDecimal = 1;

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObject.Z0_Code);
				AssertEquals(0m, TestObject.Z0_Local);
				AssertEquals(0m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 1.5;
				AssertEquals(1.5m, TestObject.Z0_Local);
				AssertEquals(1.5m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 2;
				AssertEquals(2m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Local = 4;
				AssertEquals(4m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals(0.5m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_AnotherDecimal = 3;
				AssertEquals(0.67m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals(3m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Local = 0;
				TestObject.Z0_AnotherDecimal = 0;
				TestObject.Z0_Foreign = 0;

				TestObject.Z0_AnotherDecimal = 2;
				TestObject.Z0_Foreign = 10;
				AssertEquals(5m, TestObject.Z0_Local);
				AssertEquals(10m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 5;
				AssertEquals(2.5m, TestObject.Z0_Local);
				AssertEquals(5m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestAmountsTogetherWithoutUpdatingFromLocal()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				TestObject.ReSetDataEntry(true);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);

				TestObject.Z0_Foreign = 0;
				TestObject.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObject.Z0_AnotherDecimal = 1;

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObject.Z0_Code);
				AssertEquals(0m, TestObject.Z0_Local);
				AssertEquals(0m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 1.5;
				AssertEquals(1.5m, TestObject.Z0_Local);
				AssertEquals(1.5m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 2;
				AssertEquals(2m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals(1m, TestObject.Z0_AnotherDecimal);

				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				TestObject.Z0_Local = 4; // Should not happend in GUI because it is ReadOnly
				AssertEquals(4m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals("Should not update ExchangeRate", 1m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_AnotherDecimal = 3;
				AssertEquals(0.67m, TestObject.Z0_Local);
				AssertEquals(2m, TestObject.Z0_Foreign);
				AssertEquals(3m, TestObject.Z0_AnotherDecimal);

				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				TestObject.Z0_Local = 0;
				TestObject.Z0_AnotherDecimal = 0;
				TestObject.Z0_Foreign = 0;

				TestObject.Z0_AnotherDecimal = 2;
				TestObject.Z0_Foreign = 10;
				AssertEquals(5m, TestObject.Z0_Local);
				AssertEquals(10m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 5;
				AssertEquals(2.5m, TestObject.Z0_Local);
				AssertEquals(5m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestLocalAmountChanged()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				TestObject.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObject.Z0_Foreign = 100;
				TestObject.Z0_AnotherDecimal = 1;
				TestObject.Z0_Local = 200;

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObject.Z0_Code);
				AssertEquals(200m, TestObject.Z0_Local);
				AssertEquals(100m, TestObject.Z0_Foreign);
				AssertEquals(0.5m, TestObject.Z0_AnotherDecimal);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				TestObject.Z0_Code = "USD";
				TestObject.Z0_Foreign = 300;
				TestObject.Z0_Local = 600;
				AssertEquals("USD", TestObject.Z0_Code);
				AssertEquals(600m, TestObject.Z0_Local);
				AssertEquals(300m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
				Assert(!TestObject.Z0_LocalInfo.ReadOnly);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestLocalAmountChangedWithoutUpdatingFromLocal()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				TestObject.ReSetDataEntry(true);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);

				TestObject.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObject.Z0_Foreign = 100;
				TestObject.Z0_AnotherDecimal = 1;
				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				TestObject.Z0_Local = 200;

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObject.Z0_Code);
				AssertEquals(200m, TestObject.Z0_Local);
				AssertEquals(100m, TestObject.Z0_Foreign);
				AssertEquals("Should not update ExchangeRate", 1m, TestObject.Z0_AnotherDecimal);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				TestObject.Z0_Code = "USD";
				TestObject.Z0_AnotherDecimal = 2m;
				TestObject.Z0_Foreign = 300;
				Assert(TestObject.Z0_LocalInfo.ReadOnly);
				AssertEquals(600m, TestObject.Z0_Local);
				TestObject.Z0_Local = 900;
				AssertEquals("USD", TestObject.Z0_Code);
				AssertEquals(900m, TestObject.Z0_Local);
				AssertEquals(300m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestForeignAmountChanged()
		{
			AssertForegnAmountChanged(false);
		}

		public void TestForeignAmountChangedWithoutUpdatingFromLocal()
		{
			AssertForegnAmountChanged(true);
		}

		void AssertForegnAmountChanged(bool doNotUpdateFromLocal)
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				if (doNotUpdateFromLocal)
				{
					TestObject.ReSetDataEntry(true);
					Assert(TestObject.Z0_LocalInfo.ReadOnly);
				}

				TestObject.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObject.Z0_AnotherDecimal = 2;
				TestObject.Z0_Foreign = 100;
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObject.Z0_Code);
				AssertEquals(50m, TestObject.Z0_Local);
				AssertEquals(100m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);

				CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				TestObject.Z0_Code = "USD";
				TestObject.Z0_AnotherDecimal = 0.5m;
				TestObject.Z0_Foreign = 300;
				AssertEquals("USD", TestObject.Z0_Code);
				AssertEquals(150m, TestObject.Z0_Local);
				AssertEquals(300m, TestObject.Z0_Foreign);
				AssertEquals(0.5m, TestObject.Z0_AnotherDecimal);
				AssertEquals("Should always be readonly when do not update from Local", doNotUpdateFromLocal, TestObject.Z0_LocalInfo.ReadOnly);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestExchangeRateChanged()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				TestObject.Z0_Code = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				TestObject.Z0_Foreign = 100;
				TestObject.Z0_AnotherDecimal = 1;
				TestObject.Z0_AnotherDecimal = 2;
				AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.RX_Code, TestObject.Z0_Code);
				AssertEquals(50m, TestObject.Z0_Local);
				AssertEquals(100m, TestObject.Z0_Foreign);
				AssertEquals(2m, TestObject.Z0_AnotherDecimal);
				Assert(TestObject.Z0_LocalInfo.ReadOnly);

				CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				TestObject.Z0_Code = "USD";
				TestObject.Z0_Foreign = 300;
				TestObject.Z0_AnotherDecimal = 0.5m;
				AssertEquals("USD", TestObject.Z0_Code);
				AssertEquals(150m, TestObject.Z0_Local);
				AssertEquals(300m, TestObject.Z0_Foreign);
				AssertEquals(0.5m, TestObject.Z0_AnotherDecimal);
				AssertEquals(false, TestObject.Z0_ForeignInfo.ReadOnly);
				Assert(!TestObject.Z0_LocalInfo.ReadOnly);

				TestObject.Z0_Foreign = 0;
				TestObject.Z0_Local = 0;
				TestObject.Z0_AnotherDecimal = 0;
				TestObject.Z0_Foreign = 1;
				AssertEquals(0m, TestObject.Z0_Local);
				AssertEquals(1m, TestObject.Z0_Foreign);
				AssertEquals(0m, TestObject.Z0_AnotherDecimal);

				TestObject.Z0_Foreign = 0;
				TestObject.Z0_Local = 0;
				TestObject.Z0_AnotherDecimal = 0;
				TestObject.Z0_Local = 1;
				AssertEquals(1m, TestObject.Z0_Local);
				AssertEquals(0m, TestObject.Z0_Foreign);
				AssertEquals(0m, TestObject.Z0_AnotherDecimal);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}
		[ExpectNoExceptions()]
		public void TestInvalidData()
		{
			TestObject.Z0_Code = "XXX";
			TestObject.Z0_Local = 100;

			TestObject.Z0_Code = "";
			TestObject.Z0_Local = 100;
		}

		[ExpectNoExceptions()]
		public void TestUnhookEventTwice()
		{
			TestObject.UnhookEvent();
			TestObject.UnhookEvent();
		}

		public void TestExchangeRateLoading()
		{
			bool previousFallbackValue = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;

			try
			{
				SetExchangeRateFallbackRegistryValue(true);
				RefCurrency foreignCurrency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate exRate = Factory.New<RefExchangeRate>();
				exRate.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_SellRate = 0.498m;
				exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
				Factory.Save();

				AssertEquals("Precondition: Exchange Rate is 0", 0m, TestObject.Z0_AnotherDecimal);
				Assert("Precondition: Exchange Rate should not have warnings", !TestObject.Z0_AnotherDecimalInfo.HasWarnings());
				TestObject.ExchangeRate.Currency = foreignCurrency.RX_Code;
				AssertEquals("ExchangeRate is 0.498", 0.498m, TestObject.Z0_AnotherDecimal);
				Assert("Exchange Rate should have a warning", TestObject.Z0_AnotherDecimalInfo.HasWarnings());
			}
			finally
			{
				SetExchangeRateFallbackRegistryValue(previousFallbackValue);
			}
		}

		#region Implementation

		protected MyDummyBusinessObject TestObject;

		protected override void SetUp()
		{
			base.SetUp();
			TestObject = Factory.New<MyDummyBusinessObject>();
			CurrentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		}
		GlbCompany CurrentCompany;

		void SetExchangeRateFallbackRegistryValue(bool value)
		{
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				value);
		}

		protected class MyDummyBusinessObject : DummyBusinessObject
		{
			public MyDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				fDataEntry = new LocalForeignDataEntry(Z0_CodeInfo, Z0_AnotherDecimalInfo, Z0_LocalInfo, Z0_ForeignInfo, ExchangeRate);
			}

			#region Z0_Local

			public const string Z0_LocalName = "Z0_Local";
			protected ZDecimal fZ0_Local;

			public ZDecimal Z0_Local
			{
				get { return fZ0_Local; }
				set
				{
					fZ0_Local = value;
					Z0_LocalInfo.RefreshBinding();
				}
			}

			protected bool Z0_Local_ReadOnly => DoNotUpdateFromLocal || Z0_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			protected bool DoNotUpdateFromLocal;

			public void UnhookEvent()
			{
				fDataEntry.UnhookEvents();
			}

			public ZPropertyInfo Z0_LocalInfo
			{
				get { return GetZPropertyInfo(Z0_LocalName); }
			}

			#endregion

			#region Z0_Foreign

			public const string Z0_ForeignName = "Z0_Foreign";
			protected ZDecimal fZ0_Foreign;

			public ZDecimal Z0_Foreign
			{
				get { return fZ0_Foreign; }
				set
				{
					fZ0_Foreign = value;
					Z0_ForeignInfo.RefreshBinding();
				}
			}

			public ZPropertyInfo Z0_ForeignInfo
			{
				get { return GetZPropertyInfo(Z0_ForeignName); }
			}

			#endregion

			#region Exchange Rate

			public ZAccExchangeRate ExchangeRate
			{
				get
				{
					if (fExchangeRate == null)
					{
						fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Sell, Z0_AnotherDecimalInfo, (ZPropertyInfoString)Z0_CodeInfo, null);
					}

					return fExchangeRate;
				}
			}

			protected ZAccExchangeRate fExchangeRate;

			#endregion

			public void ReSetDataEntry(bool doNotUpdateFromLocal)
			{
				if (fDataEntry != null)
				{
					fDataEntry.UnhookEvents();
				}
				this.DoNotUpdateFromLocal = doNotUpdateFromLocal;
				fDataEntry = new LocalForeignDataEntry(Z0_CodeInfo, Z0_AnotherDecimalInfo, Z0_LocalInfo, Z0_ForeignInfo, ExchangeRate, DoNotUpdateFromLocal);
			}

			protected LocalForeignDataEntry fDataEntry;
		}

		#endregion
	}
}
