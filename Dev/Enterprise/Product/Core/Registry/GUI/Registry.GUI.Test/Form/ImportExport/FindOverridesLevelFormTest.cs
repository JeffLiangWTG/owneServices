using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FindOverridesLevelForm))]
	sealed class FindOverridesLevelFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new FindOverridesLevelForm();
		}

		#endregion

		public void TestValidation()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Bananas In Pyjamas";
			company.GC_Code = "BIP";

			var b1 = company.Branches.AddNew();
			b1.GB_BranchName = "B1";
			b1.GB_Code = "B1";

			Factory.Save();

			//Both valid, no error
			ValidationHelper(baseLevel => SelectNode(baseLevel, "Default"), overrideLevel => SelectNode(overrideLevel, "System"), Enumerable.Empty<string>());
			ValidationHelper(baseLevel => SelectNode(baseLevel, "Companies/Bananas In Pyjamas"), overrideLevel => SelectNode(overrideLevel, "System"), Enumerable.Empty<string>());

			ValidationHelper(baseLevel => SelectNode(baseLevel, "System"), overrideLevel => SelectNode(overrideLevel, "System"), new[] { "The base level is the same as the override level. Please select a different base or override level." });
			ValidationHelper(baseLevel => baseLevel.SelectedNode = null, overrideLevel => SelectNode(overrideLevel, "System"), new[] { "Please select a base level" });
			ValidationHelper(baseLevel => SelectNode(baseLevel, "Companies"), overrideLevel => SelectNode(overrideLevel, "System"), new[] { "The base level you have selected is not valid. Please select a valid base level." });
			ValidationHelper(baseLevel => SelectNode(baseLevel, "Default"), overrideLevel => overrideLevel.SelectedNode = null, new[] { "Please select a override level" });
		}

		public void TestSelectedValues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Bananas In Pyjamas";
			company.GC_Code = "BIP";

			var b1 = company.Branches.AddNew();
			b1.GB_BranchName = "B1";
			b1.GB_Code = "B1";

			Factory.Save();

			using (var form = new FindOverridesLevelForm())
			{
				SelectNode(form.BaseLevelTreeView, "Companies/Bananas In Pyjamas");
				SelectNode(form.OverrideLevelTreeView, "Default");

				AssertEquals("Bananas In Pyjamas", form.SelectedValues.Item1.Description);
				AssertEquals("Default", form.SelectedValues.Item2.Description);
			}
		}

		void ValidationHelper(Action<ZTreeView> selectBaseLevelNode, Action<ZTreeView> selectOverrideLevelNode, IEnumerable<string> expectedErrors)
		{
			using (var form = new FindOverridesLevelForm())
			{
				selectBaseLevelNode(form.BaseLevelTreeView);
				selectOverrideLevelNode(form.OverrideLevelTreeView);

				var notifications = new NotificationCollection();
				form.Validate_Exposed(notifications);

				var expected = string.Join("\r\n", expectedErrors.OrderBy(o => o));
				var actual = string.Join("\r\n", notifications.GetErrors().Select(o => o.Message).OrderBy(o => o));

				AssertEquals(expected, actual);
			}
		}

		public static void SelectNode(ZTreeView tree, string nodePath)
		{
			TreeViewAssertion.AssertHasPath("root", tree.Nodes, nodePath);

			var path = nodePath.Split('/');
			var parentCollection = path.Take(path.Length - 1).Aggregate(tree.Nodes, (node, name) => node[name].Nodes);
			tree.PerformSelect(parentCollection[path.Last()]);
		}
	}
}
