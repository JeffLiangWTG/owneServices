using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.TNT.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.NZ.Testing
{
	[TestedType(typeof(QuantumNZForm))]
	public class QuantumNZFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			NZImportManager importManager = new NZImportManager(new BusinessObjectFactory(), ResourceRetriever.SaveResourceToFile(TNTTestUtils.ResourcePrefix + "SYD.X2.20040817.180100.ok"));
			using (QuantumNZForm form = new QuantumNZForm(importManager))
			{
				AssertEquals("Form Caption should be 'Quantum Interface Import'", "Quantum Interface Import", form.FormCaption);
			}
		}

		public void TestMissingShippingLineOnConsol()
		{
			NZImportManager importManager = new NZImportManager(new BusinessObjectFactory(), ResourceRetriever.SaveResourceToFile(TNTTestUtils.ResourcePrefix + "SYD.X2.20040817.180100.ok"));
			using (QuantumNZFormTestClass form = new QuantumNZFormTestClass(importManager))
			{
				form.MissingShippingLineOnConsol("C00004004", EventArgs.Empty);
				Assert("Should show a warning", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				string expectedMessage = "Cannot Match to Consol C00004004, the Carrier is not specified." + System.Environment.NewLine;
				expectedMessage += "If a carrier is not specified, then ECI Manifests cannot be created." + System.Environment.NewLine;
				expectedMessage += "Please enter a carrier on Consol C00004004, then re-match Quantum Mawbs to Consol C00004004.";
				AssertMultilineASCIIEquals("Message should be " + expectedMessage, expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var path = TestUtils.CopyResourceToFile("SYD.X2.20040817.180100.ok");
			NZImportManager importManager = new NZImportManager(new BusinessObjectFactory(), path);
			return new QuantumNZForm(importManager);
		}

		EmbeddedResourceRetriever ResourceRetriever;
		TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			ResourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			base.TearDown();
			ResourceRetriever.Dispose();
			TestUtils.Dispose();
		}

		class QuantumNZFormTestClass : QuantumNZForm
		{
			public QuantumNZFormTestClass(NZImportManager businessEntity) : base(businessEntity)
			{
			}

			public new void MissingShippingLineOnConsol(object sender, EventArgs e)
			{
				base.MissingShippingLineOnConsol(sender, e);
			}
		}
	}
}
