using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public class XmlDataTransferExporter : IXmlDataTransferExporter
	{
		public XmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForPermission)
		{
			this.Adapter = adapter;
			this.CheckForPermission = checkForPermission;
		}

		protected readonly IValueObjectDataAdapter Adapter;
		#region Export

		public void PromptUserAndExport(IList selectedElements)
		{
			PromptUserAndExportCore(selectedElements);
		}

		public void PromptUserAndExport(ZQuery exportQuery)
		{
			PromptUserAndExportCore(GetElementsToExport(exportQuery));
		}

		public string Export(IList selectedElements)
		{
			return ExportCore(selectedElements);
		}

		protected virtual void PromptUserAndExportCore(IList selectedElements)
		{
			if (AreSelectedElementsOkToExport(selectedElements))
			{
				ShowDialogAndExport(selectedElements);
			}
		}

		protected virtual IList GetElementsToExport(ZQuery exportQuery)
		{
			throw new NotImplementedException();
		}

		protected virtual string ExportCore(IList selectedElements)
		{
			string result = "";

			if (AreSelectedElementsOkToExport(selectedElements))
			{
				result = GetTempFileAndExport(selectedElements);
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected virtual void ShowDialogAndExport(IList selectedElements)
		{
			var gui = ObjectFactory.Get<IXmlDataTransferExporterGUI>();
			if (gui.ShowSaveFileDialog(DefaultFileName, InitialDirectory) == ZDialogResult.OK)
			{
				bool exported;
				using (gui.ShowProgressForm(Adapter as IProgressSupporter, selectedElements.Count))
				{
					exported = DoExportToFile(selectedElements, gui);
				}

				if (exported)
				{
					Globals.Message.ShowInformation(Res.GetString("3ad2be28-6ac9-458f-8eae-c6ef0d19c9e7", "File saved at {0}", gui.UnmappedFile));
				}
			}
		}

		protected virtual bool DoExportToFile(IList selectedElements, IXmlDataTransferExporterGUI gui)
		{
			if (typeof(BusinessObject[]).IsAssignableFrom(selectedElements.GetType()))
			{
				selectedElements = ((BusinessObject[])selectedElements).OfType(Adapter.BusinessObjectType);
			}
			else
			{
				if (selectedElements is BusinessObjectListReader listReader)
				{
					if (!Adapter.BusinessObjectType.IsAssignableFrom(listReader.BusinessObjectType))
					{
						selectedElements =
							new BusinessObjectListReader(listReader.FactoryProvider, listReader.ObjectFilter, Adapter.BusinessObjectType)
							{
								BatchSize = listReader.BatchSize,
								SaveBeforeLoadNextEnabled = listReader.SaveBeforeLoadNextEnabled
							};
					}
				}
				else
				{
					if (typeof(BusinessObject).IsAssignableFrom(Adapter.BusinessObjectType))
					{
						selectedElements = new List<BusinessObject>(selectedElements.Cast<BusinessObject>()).ToArray().OfType(Adapter.BusinessObjectType);
					}
				}
			}

			return ExportFile(selectedElements, gui);
		}

		public ZString DefaultFileName
		{
			get { return defaultFileName; }
			set { defaultFileName = MakeFilenameSafe.MakeSafe(value); }
		}
		ZString defaultFileName = "";

		public ZString InitialDirectory
		{
			get { return initialDirectory; }
			set { initialDirectory = value; }
		}
		ZString initialDirectory = "";

		protected virtual string GetTempFileAndExport(IList selectedElements)
		{
			string fileName = ExportFileName;

			bool exported = DoExportToFile(selectedElements, new UnattendedXmlDataTransferExporterGUI(fileName));
			if (!exported && File.Exists(fileName))
			{
				try
				{
					File.Delete(fileName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return exported ? fileName : "";
		}

		protected virtual ZString ExportFileName
		{
			get { return Env.GetTempFileName(); }
		}

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected virtual bool AreSelectedElementsOkToExport(IList selectedElements)
		{
			var result = true;

			if (selectedElements.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("a0735ed0-ad7c-4931-be92-056109b370b5", "Select one or more items to create the XML file for."));
				result = false;
			}
			else if (HasChanges(selectedElements))
			{
				Globals.Message.ShowError(Res.GetString("bf7308f8-1594-433f-a249-04c3fec018cb", "You must save before you can export the data."));
				result = false;
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected bool ExportFile(IList selectedBusinessObjects, IXmlDataTransferExporterGUI gui)
		{
			bool exported = false;
			var notify = new NotificationBuffer();
			using (var tempFile = TempFile.New())
			{
				try
				{
					using (Stream toFile = File.Create(tempFile.Filename))
					using (var transactionManager = BeginTransactionWithManager())
					{
						DoExport(toFile, selectedBusinessObjects, notify);
						transactionManager.CommitTransaction();
					}
					if (!notify.HasErrors)
					{
						if (gui.IsLocalFile)
						{
							tempFile.Filename.MoveOrOverwriteFile(gui.UnmappedFile);
						}
						else
						{
							using (var sourceStream = File.OpenRead(tempFile.Filename))
							using (var targetStream = gui.OpenFile())
							{
								sourceStream.CopyTo(targetStream);
							}
						}
						if (ShoulSaveFactoriesAfterExport)
						{
							SaveFactoriesAfterExport(selectedBusinessObjects);
						}
						exported = true;
					}
				}
				catch (IOException ex)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
				catch (UnauthorizedAccessException ex)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
			}

			if (!string.IsNullOrEmpty(notify.AsString))
			{
				Globals.Message.ShowInformation(notify.AsString);
			}

			return exported;
		}

		protected virtual ITransactionManager BeginTransactionWithManager()
		{
			return new StubTransactionManager();
		}

		protected virtual bool ShoulSaveFactoriesAfterExport
		{
			get { return true; }
		}

		protected virtual void SaveFactoriesAfterExport(IEnumerable selectedBusinessObjects)
		{
			var factories = new List<ITransactionParticipant>();

			foreach (BusinessObject bizO in selectedBusinessObjects)
			{
				if (bizO.Factory != null && !factories.Contains(bizO.Factory))
				{
					factories.Add(bizO.Factory);
				}
			}

			if (factories.Count > 0)
			{
				try
				{
					BusinessObjectFactory.SaveTogether(factories.ToArray());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		protected virtual void DoExport(Stream file, IList selectedBusinessObjects, INotifications notifications)
		{
			var serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
			serialiser.ExportXmlData(file, Adapter, selectedBusinessObjects, new ValueObjectExportContext(notifications), "", "", "");
		}

		bool HasChanges(IEnumerable selectedElements)
		{
			return !(selectedElements is BusinessObjectListReader) && selectedElements.Cast<BusinessObject>().Any(bo => bo.HasChanges);
		}

		#endregion

		#region Licence

		readonly bool CheckForPermission;

		protected bool HasInterfaceConnector
		{
			get { return Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected virtual void ShowPermssionError()
		{
			Globals.Message.ShowError(XmlDataTransferDirector.InterfaceConnectorPermissionError);
		}

		protected bool IsAllowedToImport()
		{
			bool result;

			if (CheckForPermission)
			{
				if (HasInterfaceConnector)
				{
					result = true;
				}
				else
				{
					ShowPermssionError();
					result = false;
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		#endregion

		protected class UnattendedXmlDataTransferExporterGUI : IXmlDataTransferExporterGUI
		{
			public UnattendedXmlDataTransferExporterGUI(string file)
			{
				UnmappedFile = file;
			}

			public string UnmappedFile { get; private set; }

			public bool IsLocalFile => true;

			public Stream OpenFile()
			{
				throw new NotImplementedException();
			}

			public IDisposable ShowProgressForm(IProgressSupporter progressSupporter, int totalCount)
			{
				throw new NotImplementedException();
			}

			public ZDialogResult ShowSaveFileDialog(string defaultFileName, string initialDirectory)
			{
				throw new NotImplementedException();
			}
		}
	}
}
