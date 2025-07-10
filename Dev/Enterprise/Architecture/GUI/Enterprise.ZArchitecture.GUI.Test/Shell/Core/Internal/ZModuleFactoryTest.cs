using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[DoNotAddToTestTree()] // Called by reflection from ZModules project
	public class ZModuleFactoryTest : TestCase
	{
		#region TestAllModules

		[ExpectNoExceptions()]
		public void TestAllModules()
		{
			var exceptionsThrown = new ArrayList();
			var modulesWithProblems = new ArrayList();
			var originalCountryCode = CurrentCompanyCountryCode;
			try
			{
				foreach (var countryCode in IDCountryFactoryTest.TestCountryCodes)
				{
					CurrentCompanyCountryCode = countryCode;
					//					IDCountryFactory.SetCountryCodeForTestingOnly(CountryCode);

					foreach (var iD in GetModuleIDs())
					{
						if (iD != NAModuleID)
						{
							try
							{
								using (var module = ZModuleFactory.Instance.Create(iD))
								{
								}
							}
							catch (Exception ex)
							{
								modulesWithProblems.Add(String.Format("{0} module, country {1}", iD.ToString(), countryCode));
								exceptionsThrown.Add(String.Format("{0} - {1} exception:\n{2}\n", iD.ToString(), countryCode, ex.ToString()));
							}
						}
					}
				}
			}
			finally
			{
				CurrentCompanyCountryCode = originalCountryCode;
			}

			if (modulesWithProblems.Count > 0)
			{
				var errorMessage = new StringBuilder();
				errorMessage.Append("The following modules could not be created. Please check the assembly names and paths in the ZModules project, ModuleRegistration.cs file.\r\n\r\n");

				modulesWithProblems.Sort();

				foreach (string s in modulesWithProblems)
				{
					errorMessage.Append(s + System.Environment.NewLine);
				}

				errorMessage.Append("\n\nThese are the exceptions that occurred:\n");

				exceptionsThrown.Sort();

				foreach (string s in exceptionsThrown)
				{
					errorMessage.Append(s + System.Environment.NewLine);
				}

				Fail(errorMessage.ToString());
			}
		}

		#endregion

		string CurrentCompanyCountryCode
		{
			get { return StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code; }
			set { StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(value); }
		}

		protected virtual IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return ModuleIDs.AllExcludingClientModules;
		}

		protected virtual ModuleIdentifier NAModuleID
		{
			get { return ModuleIDs.NotAssigned; }
		}
	}
}
