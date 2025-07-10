#if NETFRAMEWORK
using System;
#endif
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
#if NETFRAMEWORK
using NUnit.Framework;
#endif

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class GeneralActionsTest : TestCaseWithFactory
	{
		public void TestAllActionsAreAccessable()
		{
			IDictionary<string, IList<string>> lookup = new SortedDictionary<string, IList<string>>();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.OperationalActions);
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.NotEqual, GetAccessableContexts());

			foreach (OperationalAction action in Factory.Load<OperationalAction>(filter))
			{
				IList<string> list;

				if (!lookup.TryGetValue(action.SU_BusinessContext, out list))
				{
					list = new List<string>();
					lookup.Add(action.SU_BusinessContext, list);
				}

				list.Add(action.SU_MenuName);
			}

			AssertGroupedErrorList("These actions are inaccessable and should be removed.", lookup);
		}

#if NETFRAMEWORK
		[SnailTest]
		public void TestSanityCheck()
		{
			AssertionCount += Helper.TestSanityCheck();
		}
#endif

		#region Implementation

		static List<string> GetAccessableContexts()
		{
			List<string> result = new List<string>();
			ModuleList modules = new ModuleList();

			foreach (ModuleIdentifier id in ModuleIDs.AllExcludingClientModules)
			{
				if (id == ModuleIDs.NotAssigned)
				{
					continue;
				}

				GetBusinessContext(result, id, null);

				foreach (string countryCode in modules.GetCountryOverridesRegistered(id))
				{
					if (string.IsNullOrEmpty(countryCode))
					{
						continue;
					}

					GetBusinessContext(result, id, countryCode);
				}
			}

			return result;
		}

		static void GetBusinessContext(List<string> result, ModuleIdentifier id, string countryCode)
		{
			using (ZModule module = countryCode == null ? ZModuleFactory.Instance.Create(id) : ZModuleFactory.Instance.Create(id, countryCode))
			{
				IOperationalActionSupportable supportable;
				OperationalActionSupporter supporter;
				ZFilterGridModule filterModule;

				if ((supportable = module as IOperationalActionSupportable) != null &&
					(supporter = supportable.OperationalActionSupporter) != null &&
					(filterModule = module as ZFilterGridModule) != null &&
					filterModule.Plugins.GetPlugin(ControllerIDs.OperationalActions) != null)
				{
					result.Add(OperationalAction.GetFullBusinessContext(supporter.BusinessContext));
				}
			}
		}

#if NETFRAMEWORK // Should be fixed in WI00669071 
		protected override void TearDown()
		{
			if (helper != null)
			{
				helper = null;
			}

			if (domain != null)
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				AppDomain.Unload(domain);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
				domain = null;
			}

			base.TearDown();
		}

		GeneralActionsTestHelper Helper
		{
			get
			{
				// Don't forget update AssertionCount when running test in another app domain. See TestSanityCheck() for example.
				if (helper == null)
				{
					if (domain == null)
					{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
						domain = AppDomain.CreateDomain("SuperMagic");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
					}

					helper = GeneralActionsTestHelper.New(domain);
				}

				return helper;
			}
		}
		GeneralActionsTestHelper helper;

		AppDomain domain;
#endif

		#endregion
	}
}
