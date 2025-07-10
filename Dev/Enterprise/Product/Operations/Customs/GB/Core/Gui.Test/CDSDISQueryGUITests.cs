using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.GUI.CDSDIS;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.CDS.Testing
{
	[TestedType(typeof(CDSDISQueryForm))]
	class CDSDISQueryGUITests : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var msg = Factory.New<CDSDISQueryMessage>();

			return new CDSDISQueryForm(msg);
		}

		public void TestControls()
		{
			var msg = Factory.New<CDSDISQueryMessage>();

			var controlsToCheck = new Dictionary<string, string>
			{
				{ "MessageNumberTextBox", CDSDISQueryMessage.Schema.EM_MessageNum },
				{ "ApplicationReferenceTextBox", CDSDISQueryMessage.Schema.EM_ApplicationReference },
				{ "MessageOwnerTextBox", CDSDISQueryMessage.Schema.EM_MessageOwner },
				{ "MessageIntepretationHTML", "ResponseInterpretation" },
				{ "MessageStatusTextBox", CDSDISQueryMessage.Schema.EM_Status }
			};

			using (var form = new CDSDISQueryForm(msg))
			{
				form.Show();

				CombineAssertions("Checking Controls", () =>
				{
					controlsToCheck.ForEach((kvp) =>
					{
						var ctrl = form.Controls.Find(kvp.Key, true).FirstOrDefault();
						AssertNotNull($"Key: {kvp.Key}", ctrl);
						AssertEquals($"Binding for {kvp.Key}", kvp.Value, ctrl?.GetBindingMember() ?? "The control you have referenced is not available at present");
					});
				});
			}
		}
	}
}
