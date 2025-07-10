using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(TNTConsolModuleOverride))]
	public class TNTConsolModuleOverrideTest : ZModuleBasherTest
	{
		public void TestQuantumMenuItem()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertQuantumMenuItem("Import Quantum Exit&2");
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			AssertQuantumMenuItem("Import Quantum File");
		}

		void AssertQuantumMenuItem(string quantumMenuText)
		{
			using (JobConsolModule module = (JobConsolModule)ZModuleFactory.Instance.Create(ModuleIDs.JobConsol))
			{
				MenuAssertion.AssertHasMenu("Should find the Import Menu Item tool bar button", module.FormActionMenu, "&Actions", "D&ata Transfer", quantumMenuText);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobConsol;
		}

		public void TestGetFilePathToImport()
		{
			using (TNTConsolModuleOverrideTestClass module = new TNTConsolModuleOverrideTestClass())
			using (var testUtils = new TNTTestUtils())
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				module.FileName = testUtils.CopyResourceToFile("SYD.X1.00000000.000000.ok");
				AssertFileName(module, false, Core.Constants.CountryCodes.Australia);
				module.FileName = testUtils.CopyResourceToFile("UUU.X2.20040404.140500.ok");
				AssertFileName(module, false, Core.Constants.CountryCodes.Australia);
				module.FileName = testUtils.CopyResourceToFile("AKL.IQDOWNE.00000000.000000.ok");
				AssertFileName(module, true, Core.Constants.CountryCodes.Australia);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				module.FileName = testUtils.CopyResourceToFile("AKL.IND.20060713.150553.OK");
				AssertFileName(module, false, Core.Constants.CountryCodes.NewZealand);
				module.FileName = testUtils.CopyResourceToFile("AKL.X2.20060713.150553.OK");
				AssertFileName(module, false, Core.Constants.CountryCodes.NewZealand);
				module.FileName = testUtils.CopyResourceToFile("AKL.IQDOWNI.20040817.180100.ok");
				AssertFileName(module, true, Core.Constants.CountryCodes.NewZealand);
				module.FileName = testUtils.CopyResourceToFile("akl.iqdowne.20060328.093016.ok");
				AssertFileName(module, true, Core.Constants.CountryCodes.NewZealand);
			}
		}

		void AssertFileName(TNTConsolModuleOverrideTestClass module, bool resultShouldBeEmpty, string initialDirectory)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			if (resultShouldBeEmpty)
			{
				AssertEquals("File Path should be empty", "", module.GetFilePathToImport(initialDirectory));
				AssertEquals("Message to user", "Incorrect File Name Format", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertEquals("File Path should be " + module.FileName, module.FileName, module.GetFilePathToImport(initialDirectory));
				AssertNull("Should not be any message to the user", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals("Initial Directory should be set.", initialDirectory, module.initialDirectory);
		}

		class TNTConsolModuleOverrideTestClass : TNTConsolModuleOverride
		{
			public string FileName;
			public string initialDirectory;
			protected override DialogResult ShowDialog(ZOpenFileDialog dialog)
			{
				initialDirectory = dialog.InitialDirectory;
				dialog.FileName = FileName;
				return DialogResult.OK;
			}

			public new string GetFilePathToImport(string initialDirectory)
			{
				return base.GetFilePathToImport(initialDirectory);
			}
		}
	}
}
