using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.Generator
{
	/// <summary>
	/// Tool for generate Native XML Schema and test Native XML request
	/// </summary>
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class NativeSchemaGenerationForm : Form
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Development only tool, and not deployed to clients., Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		public NativeSchemaGenerationForm()
		{
			InitializeComponent();
			Initialisation.Initialiser.InitialiseWinForms();

			Text = Core.Constants.ProductName + " Native Schema Tool";

			if (!WTG.TestHelpers.TestingState.IsTest)
			{
				this.TemporaryContext = Env.SetTemporaryUserContext(User.SupportUserName, new Guid("54226AD9-9E8A-4E29-A7F7-E73A7D18DDF1"), new Guid("6B53E737-E70C-41BA-BC1E-EB2867C10E7F"));
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		public IDefinitionLoader Locator { get; set; }
		IDisposable TemporaryContext { get; set; }

		#region Entity Set Schema Panel

		void GenerateSchemaButton_Click(object sender, EventArgs e)
		{
			if (schemaTextBox.Text.IsEmpty())
			{
				PreviewButton_Click(sender, e);
			}
			SaveXsdFile(schemaTextBox.Text);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		void PreviewButton_Click(object sender, EventArgs e)
		{
			try
			{
				var xsdGenerator = new NativeXsdGenerator();
				var entitySetDefinition = GetEntitySetDefinition(entitySetNameTextBox.Text);
				schemaTextBox.Text = xsdGenerator.Generate(entitySetDefinition).ToString();
			}
			catch (ApplicationException ex)
			{
				MessageBox.Show("Please specify a valid entity set name - " + ex.Message);
			}
		}

		EntitySetDefinition GetEntitySetDefinition(String entitySetName)
		{
			var definitionData = Locator.Load(entitySetName);
			return new EntitySetDefinitionBuilder(definitionData).GetEntitySetDefinition();
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Development only tool, do not want to reference ZArchitecture, and not deployed to clients.")]
		void SaveXsdFile(string content)
		{
			if (content.IsEmpty())
			{
				MessageBox.Show("There is nothing to generate");
				return;
			}
			var saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "XSD Files|*.xsd";
			saveFileDialog1.Title = "Save an XSD File";
			saveFileDialog1.RestoreDirectory = true;
			saveFileDialog1.ShowDialog();

			if (string.IsNullOrEmpty(saveFileDialog1.FileName))
			{
				return;
			}

			File.WriteAllText(saveFileDialog1.FileName, content);
			try
			{
				Process.Start(saveFileDialog1.FileName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				MessageBox.Show(ex.Message);
			}
		}

		#endregion

		#region Import/Export Panel

		void importButton_Click(object sender, EventArgs e)
		{
			var importService = ObjectFactory.Get<IImportService>("NativeXmlImportService");
			importService.Import();
		}

		#endregion
	}
}
