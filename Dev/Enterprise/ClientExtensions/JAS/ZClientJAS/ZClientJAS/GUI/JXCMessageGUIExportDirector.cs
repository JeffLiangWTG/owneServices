using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public class JXCMessageGUIExportDirector
	{
		public JXCMessageGUIExportDirector(JXCMessageExporter exporter, IJXCExportForm exportForm)
		{
			this.Exporter = exporter;
			this.ExportForm = exportForm;

			CheckForNullArguments();
		}

		#region AdditionalPreExportCheck

		public event CancelEventHandler AdditionalPreExportCheck;

		bool OnAdditionalPreExportCheckMethod()
		{
			CancelEventArgs e = new CancelEventArgs();
			if (AdditionalPreExportCheck != null)
			{
				AdditionalPreExportCheck(this, e);
			}
			return !e.Cancel;
		}

		#endregion

		#region EnsureMessageCanBeExported

		virtual public bool EnsureMessageCanBeExported()
		{
			using (ZWaitCursorChanger waitCursorChanger = new ZWaitCursorChanger())
			{
				return
						EnsureHasNoChangesAndInDatabase() &&
						OnAdditionalPreExportCheckMethod() &&
						EnsureHasNoErrorsAndJXCValidationWarnings();
			}
		}

		bool EnsureHasNoChangesAndInDatabase()
		{
			bool result = ExportForm.BusinessEntity is NonPersistentBusinessObject || (ExportForm.BusinessEntity.IsInDatabase && !ExportForm.BusinessEntity.HasChanges);

			if (!result)
			{
				Globals.Message.ShowError("You must save before you can export data to JXC file");
			}

			return result;
		}

		internal Action<BusinessObjectFactory> ManageJXCValidationForTest;

		bool EnsureHasNoErrorsAndJXCValidationWarnings()
		{
			bool result = false;

			SetWeirdJasValidationContext(ExportForm.BusinessEntity, true);

			if (ManageJXCValidationForTest != null)
			{
				ManageJXCValidationForTest(ExportForm.BusinessEntity.Factory);
			}
			else
			{
				new JXCDomainValidationManager(ExportForm.BusinessEntity.Factory).ManageJXCValidations(Exporter.ExportValidationTypeToUse);
			}

			ExportForm.ValidateAll();

			if (!ExportForm.BusinessEntity.HasErrors)
			{
				JXCWarningInfoCollector warningInfoCollector = new JXCWarningInfoCollector(ExportForm.BusinessEntity);
				if (warningInfoCollector.IsEmpty)
				{
					result = true;
				}
				else
				{
					JXCWarningMessageBox.ShowDialog(warningInfoCollector);
				}
			}
			else
			{
				Globals.Message.ShowError("There are errors that need to be corrected before JXC message can be exported");
			}

			SetWeirdJasValidationContext(ExportForm.BusinessEntity, false);

			return result;
		}

		void SetWeirdJasValidationContext(BusinessObject businessEntity, bool isInContext)
		{
			IJASInvoicingBase invoice = businessEntity as IJASInvoicingBase; // NULL

			if (invoice != null)
			{
				invoice.IsInJasSpecificNeedsCoreValidation = isInContext;
			}
		}

		#endregion

		#region Export

		virtual public void Export()
		{
			ZString exportPath = string.Empty;
			if (!JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport || QueryPathFromUser(out exportPath))
			{
				using (ZWaitCursorChanger waitCursorChanger = new ZWaitCursorChanger())
				{
					if (Exporter.WriteToFile(exportPath))
					{
						NotifyExportSuccessful(Exporter.HeaderData, exportPath);
					}
					else
					{
						NotifyExportFailed(Exporter.HeaderData);
					}
				}
			}
		}

		bool QueryPathFromUser(out ZString selectedPath)
		{
			bool result = false;

			using (ZFolderBrowserDialog dialog = new ZFolderBrowserDialog())
			{
				dialog.Description = "Please select a directory where the file is exported to";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					selectedPath = dialog.UnmappedSelectedPath;
					result = true;
				}
				else
				{
					selectedPath = "";
				}
			}

			return result;
		}

		void NotifyExportSuccessful(IJXCExportHeader headerData, ZString exportPath)
		{
			string dirPath = (exportPath.IsEmpty) ? JASDataRegistry.Instance.JXCOutgoingDirectoryName : exportPath;
			string message = "JXC Message for '" + headerData.HumanReadableName + "' has been successfully exported to \"" + dirPath + "\"";
			Globals.Message.ShowInformation(message);
		}

		void NotifyExportFailed(IJXCExportHeader headerData)
		{
			string errorMessage = "Failed to export JXC Message for '" + headerData.HumanReadableName + "'. Please refer to the JXC Export Log note in the Notes tab";
			Globals.Message.ShowError(errorMessage);
		}

		#endregion

		void CheckForNullArguments()
		{
			if (Exporter == null)
			{
				throw new ArgumentNullException("Exporter");
			}

			if (ExportForm == null)
			{
				throw new ArgumentNullException("ExportForm");
			}

			if (ExportForm.BusinessEntity == null)
			{
				throw new ArgumentNullException("ExportForm.BusinessEntity");
			}
		}

		public readonly JXCMessageExporter Exporter;
		public readonly IJXCExportForm ExportForm;
	}
}
