using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class IDCountryFactoryTest : TestCase
	{
		public void TestMemoryLeak()
		{
			var list = MemoryLeakHelper();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			for (var i = 0; i < 10; ++i)
			{
				object dummy;
				Assert(!list[i].TryGetTarget(out dummy));
			}
		}

		List<WeakReference<object>> MemoryLeakHelper()
		{
			var result = new List<WeakReference<object>>();
			for (var i = 0; i < 10; ++i)
			{
				result.Add(new WeakReference<object>(new MockIDCountryFactory()));
			}
			return result;
		}

		public void TestModuleIdIsNull()
		{
			AssertExceptionThrown<ModuleIDIsNullException>(() => MockIDCountryFactory.Instance.CreateNew(null));
		}

		public void TestNotAssigned()
		{
			AssertExceptionThrown<ModuleNotAssignedIDException>(() => MockIDCountryFactory.Instance.CreateNew(ModuleIDs.NotAssigned));
		}

		public void TestCreateNew()
		{
			AssertNotNull(MockIDCountryFactory.Instance.CreateNew(DummyRegistrationIDs.DummyShipment));
			AssertEquals(new DummyShipment().GetType(), MockIDCountryFactory.Instance.CreateNew(DummyRegistrationIDs.DummyShipment).GetType());
		}

		public void TestCreateNewWithCountry()
		{
			AssertNotNull(MockIDCountryFactory.Instance.CreateNewWithCountry(DummyRegistrationIDs.DummyShipment, "AU"));
			AssertEquals(new DummyShipment().GetType(), MockIDCountryFactory.Instance.CreateNewWithCountry(DummyRegistrationIDs.DummyShipment, "AU").GetType());
		}

		public void TestGetCountryOverridesRegistered()
		{
			var countryCodes = ZModuleFactory.Instance.GetCountryOverridesRegisteredForModule(ModuleIDs.Customs.JobDeclaration);
			AssertEquals("Country Codes for customs", true, countryCodes.Length > 0);
		}

		public void TestGetRegisteredIdentifierByName()
		{
			var controllerID = ZControllerFactory.Instance.GetRegisteredIdentifierByName(DummyControllerIDs.Dummy.Name);
			AssertEquals(DummyControllerIDs.Dummy, controllerID);
		}

		public void TestRepairOnMissingType()
		{
			try
			{
				MockIDCountryFactory.Instance.CreateNew(DummyRegistrationIDs.DummyNoSuchType);
				Assert("Exception should have been thrown", false);
			}
			catch (Exception ex)
			{
#if !WINZOR
				ExceptionReporter.Instance.HandleUnhandledException(ex);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains(@$"Would you like to attempt to repair the {Enterprise.Core.Constants.ProductName} installation?"));
#else
				Assert(ex.GetType().ToString() == "System.IO.FileNotFoundException");
#endif
			}
		}

		public void TestCountryCodeFromNonOwnerThread_ShouldNotThrowException()
		{
			var company = StaticCurrentFetcher.Instance.CurrentCompany;
			var factory = ((BusinessObject)company).Factory;
			factory.ThreadSentry.RelinquishThreadOwnership();
			var code = MockIDCountryFactory.Instance.CountryCode_Exposed;
			factory.ThreadSentry.TakeThreadOwnership();

			AssertEquals(company.GC_RN_NKCountryCode, code);
		}

		//TODO: Work out all countries in all Modules + add a few more for good measure
		public static string[] TestCountryCodes = { "AU", "SG", "NZ", "ER", "US", "JP", "ZA" };

		class MockIDCountryFactory : IDCountryFactory<RegistrationIdentifier, RegistrationInfo, DummyRegistrationList>
		{
			public static readonly MockIDCountryFactory Instance = new MockIDCountryFactory();

			public string CountryCode_Exposed
			{
				get { return CountryCode; }
			}
		}
	}
}
