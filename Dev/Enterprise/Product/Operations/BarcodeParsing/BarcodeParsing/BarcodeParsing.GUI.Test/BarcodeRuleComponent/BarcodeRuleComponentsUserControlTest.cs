using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.GUI.Testing
{
	class BarcodeRuleComponentsUserControlTest : BarcodeParsingTestCase
	{
		#region TestColorDecider

		[RequiresSTA]
		public void TestColorDeciding()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			var rule = Helper.CreateRule(ruleSet);
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);
			var component3 = Helper.CreateRuleComponent(rule);
			component2.HasDelimiter = true;
			component2.IsDelimiterMultiComponent = true;

			using (var form = new ZForm(ruleSet))
			{
				var control = new TestBarcodeRuleComponentsUserControl();
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "Rules");
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.RuleComponentsGrid.AllowSorting);
				AssertEquals(Color.Empty, control.CallColourDecidingForRulesComponentsGrid(component1));
				AssertEquals(Color.LightBlue, control.CallColourDecidingForRulesComponentsGrid(component2));
				AssertEquals(Color.LightBlue, control.CallColourDecidingForRulesComponentsGrid(component3));
			}
		}

		#endregion

		#region Implementation

		class TestBarcodeRuleComponentsUserControl : BarcodeRuleComponentsUserControl
		{
			public new ZGrid RuleComponentsGrid
			{
				get { return base.RuleComponentsGrid; }
			}

			public Color CallColourDecidingForRulesComponentsGrid(BarcodeRuleComponent component)
			{
				var eventName = "ColourDeciding";
				var eventDelegate = (MulticastDelegate)this.RuleComponentsGrid.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this.RuleComponentsGrid);
				if (eventDelegate != null)
				{
					var handler = eventDelegate.GetInvocationList().Single();
					var colourDecidingEventArgs = new ColourDecidingEventArgs(component);
					handler.Method.Invoke(handler.Target, new object[] { this.RuleComponentsGrid, colourDecidingEventArgs });
					return colourDecidingEventArgs.Colour;
				}
				throw new InvalidOperationException("ColourDeciding delegate does not exist in Rule Components ZGrid.");
			}
		}

		#endregion
	}
}
