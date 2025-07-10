using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing.DataAccess;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class MainFormBasher : TransactionedTestCase
	{
		[ExpectNoExceptions]
		[SnailTest]
		[StressTest]
		[RequiresSTA]
		public void TestMainForm()
		{
			// Initialise the ZEnvironment and Registry so that static factories do not cause a false alarm
			BusinessObjectFactory registryFactory = Registry.Business.RegistryFactory.Instance;

			using (MainForm = new MainFormTestCase.TestMainForm())
			{
				MainForm.Show();

				var tree = MainForm.NavigationBar.navigationViewModel;
				var modules = tree
					.Categories
					.SelectMany(t => t.Buttons.OfType<CargoWise.Main.Navigation.MenuSection>())
					.SelectMany(t => t.Subsections.OfType<CargoWise.Main.Navigation.MenuSection>())
					.SelectMany(t => t.Items);

				var moduleTree = ModuleTree.Tree;
				var first = true;

				foreach (var module in modules)
				{
					var moduleInTree = moduleTree.FindByID(module.Key);
					if (moduleInTree != null && !moduleInTree.IsPopup)
					{
						if (first)
						{
							// without this, a leak is always reported on the first hit
							module.LinkAction.Execute(null);
							Application.DoEvents();
							MainForm.DisposePreviousEmbeddedModule();
							first = false;
						}
						else
						{
							BashModule(module);
							Application.DoEvents();
						}
					}
				}
			}

			if (FailureMessages.Count > 0)
			{
				StringBuilder builder = new StringBuilder(FailureMessages.Count * 100);
				foreach (string failureMessage in FailureMessages)
				{
					builder.Append(failureMessage).Append("<BR>");
				}

				HtmlFail(builder.ToString());
			}
		}

		#region Implementation

		MainFormTestCase.TestMainForm MainForm;
		readonly ArrayList FailureMessages = new ArrayList();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1056:Do Not Use GC.Collect()", Justification = "Unit Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)", Justification = "Unit Testing")]
		void BashModule(CargoWise.Main.Navigation.MenuItem module)
		{
			try
			{
				GC.Collect();
				long bytesBefore = GC.GetTotalMemory(true);
				GlbCompany.CurrentCompany.Factory.GetNull<OrgHeader>(); // create null factory up-front to prevent this showing up as a false positive

				Dictionary<long, StackTrace> factoryInstancesBefore = RowFactoryTestUtils.GetActiveRowFactoryStacktraces();
				RowFactoryTestUtils.CreateStackTraceOnConstruction = true;

				module.LinkAction.Execute(null);
				MainForm.DisposePreviousEmbeddedModule();

				Application.DoEvents();
				//clear cache here (so it doesn't complain about leaking a module/fsbo in the global cache)
				ModuleFilter.ClearSelectedFiltersCache();
				GC.Collect();
				long bytesAfter = GC.GetTotalMemory(true);

				Dictionary<long, StackTrace> factoryInstancesAfter = RowFactoryTestUtils.GetActiveRowFactoryStacktraces();
				RowFactoryTestUtils.CreateStackTraceOnConstruction = false;

				if (factoryInstancesAfter.Count > factoryInstancesBefore.Count)
				{
					FailureMessages.Add("<B>" + module.Name + " (Memory Leak)</B>");
					FailureMessages.Add("Extra RowFactories were present at the end of the test.");
					FailureMessages.Add("RowFactories before/after: " + factoryInstancesBefore.Count + "/" + factoryInstancesAfter.Count);
					FailureMessages.Add("Memory usage before/after: " + (bytesBefore / 1024) + "K/" + (bytesAfter / 1024) + "K");
					FailureMessages.Add("");
					FailureMessages.Add("Construction Stack-traces of the extra factories:");

					foreach (long instanceAfter in factoryInstancesAfter.Keys)
					{
						if (!factoryInstancesBefore.ContainsKey(instanceAfter))
						{
							FailureMessages.Add("Factory Instance " + instanceAfter.ToString());
							FailureMessages.Add("");
							FailureMessages.Add(factoryInstancesAfter[instanceAfter].ToString().Replace("\r\n", "<BR>"));
							FailureMessages.Add("");
						}
					}

					FailureMessages.Add("");
				}
			}
			catch (Exception ex)
			{
				FailureMessages.Add("<B>" + module.Name + " (Exception)</B>");
				FailureMessages.Add(ex.ToString());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}

		#endregion
	}
}
