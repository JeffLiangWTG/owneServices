using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationControlBag))]
	sealed class ConsolidatedDeclarationControlBagTest : ControlBagAbstractTest
	{
		public void TestControlTypes()
		{
			var controls = new Dictionary<ControlReference, Control>();
			CombineAssertions(() =>
			{
				using (var panel = new ZPanel())
				{
					var bag = GetControlBagForTesting();
					bag.CreateControls(panel, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.PaymentStatusTextBox), bag, controls);
					AssertControlType<ZDropEdit>(nameof(ConsolidatedDeclarationControlBag.EntryStyleDropEdit), bag, controls);
					AssertControlType<ZCodeFindBox>(nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox), bag, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox), bag, controls);
					AssertControlType<ConsolidatedDeclarationDetailsUserControl>(nameof(ConsolidatedDeclarationControlBag.ConsolidatedDeclarationDetailsUserControl), bag, controls);
				}
			});
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ConsolidatedDeclarationControlBag.PaymentStatusTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.EntryStyleDropEdit);
				yield return nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox);
				yield return nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.ConsolidatedDeclarationDetailsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ConsolidatedDeclarationControlBag.Instance;

		void AssertControlType<T>(string controlName, ControlBag bag, IDictionary<ControlReference, Control> controls)
		{
			AssertType<T>(controlName, controls[new ControlReference(bag, controlName)]);
		}
	}
}
