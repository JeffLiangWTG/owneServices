using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI
{
	public partial class DataImporterForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		protected DataImporterForm()
		{
			InitializeComponent();
		}

		public DataImporterForm(string formCaption, BillingInterfaceName interfaceName)
			: this(new DataImporterBusinessObject(new BusinessObjectFactory()), formCaption, interfaceName)
		{
		}

		public DataImporterForm(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
		{
			InitializeComponent();
			SetDataBinding(businessEntity, "");
			Init(formCaption);
			businessEntity.OnResetProgressText += new EventHandler(BusinessEntity_OnResetProgressText);
			businessEntity.OnAppendProgressText += new EventHandler<DataImporterBusinessObject.TextAppendedEventArgs>(BusinessEntity_OnAppendProgressText);
			OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.CheckedChanged += new EventHandler(SetOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxValueToImporter);
			this.InterfaceName = interfaceName;
		}

		protected BillingInterfaceName InterfaceName;

		void BusinessEntity_OnResetProgressText(object sender, EventArgs e)
		{
			ProgressTextBox.ResetText();
		}

		void BusinessEntity_OnAppendProgressText(object sender, DataImporterBusinessObject.TextAppendedEventArgs e)
		{
			ProgressTextBox.AppendText(e.NewText);
		}

		void SetOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxValueToImporter(object sender, EventArgs e)
		{
			Importer.OnlySaveDataWhenNoRecordsHaveErrors = OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Checked;
		}

		public void SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(bool @checked, bool visible)
		{
			OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Checked = @checked;
			OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Visible = visible;
		}

		public static DataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new DataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, interfaceName);
		}

		void Init(string formCaption)
		{
			MinimumSize = Size;
			this.fFormCaption = formCaption;
			this.ProgressTextBox.ReadOnly = true;
			NotifyGuiHelper = (INotificationSubscriberQueryUserDataImport)ObjectFactory.Get<INotificationSubscriberQueryUser>();
		}

		public new DataImporterBusinessObject BusinessEntity
		{
			get { return (DataImporterBusinessObject)base.BusinessEntity; }
		}

		public DataImporter Importer { get; set; }

		#region ZForm Overrides

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return fFormCaption ?? base.FormCaption; }
		}
		string fFormCaption;

		#endregion

		protected virtual bool OnBeforeImport()
		{
			SetAllButtonsEnabled(false);
			BusinessEntity.OnBeforeImport();
			return true;
		}

		protected virtual void OnAfterImport(bool fatalErrorOccurred)
		{
			ProgressTextBox.AppendText("\r\n");
			BusinessEntity.OnAfterImport(fatalErrorOccurred);
			SetAllButtonsEnabled(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		protected virtual string ImportFileFilter
		{
			get
			{
				string filterClause = "Text files (*.txt)|*.txt";

				if (Importer != null && Importer is FlatFileDataImporter)
				{
					FileExtensionFilterBuilder filterBuilder = new FileExtensionFilterBuilder();
					filterBuilder.Add(((FlatFileDataImporter)Importer).FileExtensionType);
					filterClause = filterBuilder.FilterClause;
				}

				return filterClause;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (!CloseButton.Enabled)
			{
				e.Cancel = true;
			}
		}

		#region Import From File

		void ImportFromFile_Click(object sender, System.EventArgs e)
		{
			NotifyGuiHelper.ResetUpdateDuringImportFlags();

			var dialog = new ZOpenFileDialog();
			dialog.Filter = ImportFileFilter;
			dialog.RestoreDirectory = true;
			dialog.CheckFileExists = true;
			DialogResult dR = dialog.ShowDialog(this);

			if (dR == DialogResult.OK)
			{
				ImportFromFile(dialog.ForceLocalFile());
			}
		}

		protected internal virtual void ImportFromFile(ZString fileName)
		{
			bool fatalErrorOccurred = false;
			string message = Res.GetString("6E399DBA-5B83-4000-8A02-271145B3D19B", "Importing data from file [{0}]...", fileName);
			BusinessEntity.ImportOperationDescription = message;
			try
			{
				if (OnBeforeImport())
				{
					if (!fileName.IsEmpty)
					{
						Cursor originalCursor = Cursor;
						try
						{
							Cursor = Cursors.WaitCursor;
							fatalErrorOccurred = !Importer.ImportData(fileName, this, new SourceInfo(BillingDataSource.InterfaceConnector, InterfaceName, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileName));
						}
						finally
						{
							Cursor = originalCursor;
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				fatalErrorOccurred = true;
				HandleException(ex);
			}
			finally
			{
				OnAfterImport(fatalErrorOccurred);
			}
		}

		#endregion

		#region SetAllButtonsEnabled

		void SetAllButtonsEnabled(bool value)
		{
			this.ImportFromFileButton.Enabled = value;
			this.CloseButton.Enabled = value;
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Enabled = value;
		}

		#endregion

		#region INotifications Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification @event)
		{
			BusinessEntity.Notify(@event);
			Application.DoEvents(); // update the gui where possible
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			NotifyGuiHelper.QueryUser(e);
			Application.DoEvents(); // update the gui where possible
		}

		#endregion

		#region Implementation

		protected INotificationSubscriberQueryUserDataImport NotifyGuiHelper
		{
			get;
			private set;
		}

		void HandleException(Exception ex)
		{
			var isNonFatalException =
				ex is IOException ||
				ex is UnauthorizedAccessException ||
				ex is NotSupportedException ||
				ex is XmlException ||
				ex is OperationCanceledException;

			BusinessEntity.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			if (!isNonFatalException)
			{
				Globals.Message.ShowDeveloperException(ex);
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		internal class WhiteTextBox : TextBox
		{
			public override Color BackColor
			{
				get { return SystemColors.Window; }
				set { }
			}
		}

		#endregion

		#region Exposed For Test
#if DEBUG

		public bool IsReadOnlyProgressTextBox
		{
			get { return ProgressTextBox.ReadOnly; }
		}

#endif
		#endregion
	}
}
