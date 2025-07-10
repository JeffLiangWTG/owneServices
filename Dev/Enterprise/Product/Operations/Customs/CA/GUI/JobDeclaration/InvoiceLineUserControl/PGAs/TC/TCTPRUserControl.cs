using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class TCTPRUserControl : ZUserControl
	{
		public TCTPRUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();

			if (!DesignMode)
			{
				InitializeLazyCreate(isOnInvoiceLine);
			}
		}

		TCPGAHeader PGAHeader => (TCPGAHeader)DataSource;

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				CountCalcDropEdit.Dispose();
			}
		}

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ImporterDeclarationCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ImporterDeclarationStateDescLabel.DataBindings.RemoveBinding(IsVisibleForBindingString);

			USImporterDeclarationCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			USImporterDeclarationStateDescLabel.DataBindings.RemoveBinding(IsVisibleForBindingString);

			USImporterDeclarationCheckBox.IsVisibleForBindingChanged -= ImporterDeclarationCheckBoxes_IsVisibleForBindingChanged;
			ImporterDeclarationCheckBox.IsVisibleForBindingChanged -= ImporterDeclarationCheckBoxes_IsVisibleForBindingChanged;

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				ImporterDeclarationCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "ZZImporterDeclarationVisibility", false, System.Windows.Forms.DataSourceUpdateMode.Never));
				ImporterDeclarationStateDescLabel.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "ZZImporterDeclarationVisibility", false, System.Windows.Forms.DataSourceUpdateMode.Never));

				USImporterDeclarationCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "USImporterDeclarationVisibility", false, System.Windows.Forms.DataSourceUpdateMode.Never));
				USImporterDeclarationStateDescLabel.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "USImporterDeclarationVisibility", false, System.Windows.Forms.DataSourceUpdateMode.Never));

				USImporterDeclarationCheckBox.IsVisibleForBindingChanged += ImporterDeclarationCheckBoxes_IsVisibleForBindingChanged;
				ImporterDeclarationCheckBox.IsVisibleForBindingChanged += ImporterDeclarationCheckBoxes_IsVisibleForBindingChanged;

				UpdateDeclarationCheckBoxesCaptions();
			}
		}

		void ImporterDeclarationCheckBoxes_IsVisibleForBindingChanged(object sender, EventArgs e)
		{
			UpdateDeclarationCheckBoxesCaptions();
		}

		public void UpdateDeclarationCheckBoxesCaptions()
		{
			if (!PGAHeader.IsNull && !PGAHeader.IsDeleted)
			{
				var zzImporterDeclaraionCode = PGAHeader.IsNewOrRetreadedTiresNeedDeclaraion
				? TCComplicanceStatements.Codes.TC01
				: TCComplicanceStatements.Codes.TC07;
				var usImporterDeclaraionCode = PGAHeader.IsNewOrRetreadedTiresNeedDeclaraion
					? TCComplicanceStatements.Codes.TC02
					: TCComplicanceStatements.Codes.TC08;

				ImporterDeclarationCheckBox.GetExtension<ILabelCaptionRenderer>().Caption =
					zzImporterDeclaraionCode + " - " + Res.GetString("2a14e8bd-1b7f-402a-aa10-02a6d2f12a43", "All Countries");
				USImporterDeclarationCheckBox.GetExtension<ILabelCaptionRenderer>().Caption =
					usImporterDeclaraionCode + " - " + Res.GetString("f1bea2c4-9453-42f2-b3c3-3011a57002c9", "USA");
			}
		}
	}
}
