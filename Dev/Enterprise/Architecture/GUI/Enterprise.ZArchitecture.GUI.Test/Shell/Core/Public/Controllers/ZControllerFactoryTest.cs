using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[DoNotAddToTestTree()] // Called by reflection from ZModules project
	sealed class ZControllerFactoryTest : TestCaseWithDummy
	{
		public void TestGetControllerForBizo()
		{
			var company = StaticCurrentFetcher.Instance.CurrentCompany;
			var bizObject = (BusinessObject)company;
			var controller = ZControllerFactory.Instance.GetControllerForBizo(bizObject);
			AssertEquals("Controller returned null after executing 'GetControllerForBizo(Ibusiness bizo)'", true, controller != null);
		}

		[ExpectNoExceptions()]
		public void TestAllControllers()
		{
			var exceptionsThrown = new ArrayList();
			var controllersWithProblems = new ArrayList();

			var company = StaticCurrentFetcher.Instance.CurrentCompany;
			string originalCountryCode = company.Country.RN_Code;
			try
			{
				foreach (var countryCode in IDCountryFactoryTest.TestCountryCodes)
				{
					company.SetCountry(countryCode);

					var fieldControllers = typeof(ControllerIDs).GetFields(BindingFlags.Static | BindingFlags.Public);
					foreach (var fieldController in fieldControllers)
					{
						var controllerID = (ControllerID)fieldController.GetValue(null);

						try
						{
							var controller = ZControllerFactory.Create(controllerID);
						}
						catch (Exception ex)
						{
							controllersWithProblems.Add(String.Format("{0} controller, country {1}", controllerID.ToString(), countryCode));
							exceptionsThrown.Add(String.Format("{0} - {1} exception:\n{2}\n", controllerID.ToString(), countryCode, ex.ToString()));
						}
					}
				}
			}
			finally
			{
				company.SetCountry(originalCountryCode);
			}

			if (controllersWithProblems.Count > 0)
			{
				var errorMessage = new StringBuilder();
				errorMessage.Append("DO YOU OWN ANY OF THESE CONTROLLERS? If so, please fix them! The following controllers could not be created. Please check the assembly names and paths in the ZModules project, ControllerRegistration.cs file.\r\n\r\n");
				controllersWithProblems.Sort();
				foreach (string s in controllersWithProblems)
				{ errorMessage.Append(s + System.Environment.NewLine); }
				errorMessage.Append("\n\nThese are the exceptions that occurred:\n");
				exceptionsThrown.Sort();
				foreach (string s in exceptionsThrown)
				{ errorMessage.Append(s + System.Environment.NewLine); }

				Fail(errorMessage.ToString());
			}
		}

		public void TestGetCorrectControllerAndBusinessObjectCreateFactoryOnlyOnce()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertEquals(0, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(x => x.NameForDebugging == "ZController_GetNewFactory"));

			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(DummyControllerIDs.Dummy, dummyBizO.PK, false);
			AssertEquals(DummyControllerIDs.Dummy, result.Controller.ID);
			AssertEquals(dummyBizO.PK, result.BusinessObject.PK);
			AssertEquals(1, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(x => x.NameForDebugging == "ZController_GetNewFactory"));

			result = ZControllerFactory.GetCorrectControllerAndBusinessObject(DummyControllerIDs.Dummy1, dummyBizO.PK, false);
			AssertEquals(DummyControllerIDs.Dummy1, result.Controller.ID);
			AssertEquals(dummyBizO.PK, result.BusinessObject.PK);
			AssertEquals(2, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(x => x.NameForDebugging == "ZController_GetNewFactory"));
		}

		public void TestGetNewControllerAndExistingBusinessObjectCreateFactory()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Guid = dummyBizO.PK;
			Factory.Save();

			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(DummyControllerIDs.DummyControllerWithPlugInAndNavigationProvider, dummyBizO.PK, false);
			AssertEquals("The new ControllerID should be Dummy", DummyControllerIDs.Dummy, result.Controller.ID);
			AssertEquals(dummyBizO.PK, result.BusinessObject.PK);
		}
	}
}
