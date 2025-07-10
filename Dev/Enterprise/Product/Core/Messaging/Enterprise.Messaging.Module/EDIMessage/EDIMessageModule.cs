using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Messaging.Module
{
	public class EDIMessageModule : ZFilterGridModule
	{
		public EDIMessageModule()
		{
			VersionHeader = string.Format((NoResString)@"<!-- CW1 Version : {0} Release : {1}-->", new EnterpriseInformationRetriever().VersionNumber, new EnterpriseInformationRetriever().Release);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Messaging.EDIMessage; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		static List<string> HTTPXMLApplicationCodes
		{
			get { return HTTPXMLApplicationCodeList.GetCodes().ToList(); }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result;
			if (selectedBusinessObject != null && ((EDIMessage)selectedBusinessObject).EM_ApplicationCode == ApplicationCodeList.Codes.eNett)
			{
				result = ZControllerFactory.Create(ControllerIDs.LinkedeNettEDIMessage);
			}
			else
			{
				result = ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessage);
			}

			return result;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIMessageFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NonDependentEDIMessageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIMessageFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EDIMessage; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (ShowMessageInterpreter)
			{
				result.Add(new ZMenuItem(MessageInterpreterMenuName, new EventHandler(MessageInterpreter_Click)));
			}
			if (ShowRequeuingMenu)
			{
				result.Add(new ZMenuItem(ResetStatusToQueuedMenuName, new EventHandler(ResetToQueued_Click)));
			}

			result.Add(new ZMenuItem(UniversalXmlMessageMenuName, new EventHandler(UniversalXmlMessageImport_Click)));

			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				result.Add(new ZMenuItem(EAdaptorImportMessageMenuName, OnEAdaptorMessageImport_Click));
			}

			result.Add(new ZMenuItem(SimulateMessageImportNoCommitMenuName, OnSimulateMessageImportWithNoCommit_Click));

			result.Add(new ZMenuItem(SaveUniversalXmlSchemasMenuName, new EventHandler(UniversalXmlSchema_Click)));

			var generateKeyMenu = new ZMenuItem(GenerateKeyMenuName);
			generateKeyMenu.MenuItems.Add(new ZMenuItem(FromEDIMessageMenuName, new EventHandler(GenerateKeyFromEDIMessage_Click)));
			generateKeyMenu.MenuItems.Add(new ZMenuItem(FromXmlMenuName, new EventHandler(GenerateKeyFromXml_Click)));
			result.Add(generateKeyMenu);

			return result.ToArray();
		}

		public static MultilingualString ResetStatusToQueuedMenuName
		{
			get { return ResString.GetMultilingualString("cf07da82-c5fc-446f-af48-588da755b849", "Reset Status to Queued"); }
		}
		public static MultilingualString UniversalXmlMessageMenuName
		{
			get { return ResString.GetMultilingualString("1a28de6b-eda9-4d08-acb9-93d878c8d7e8", "Add Inbound Universal XML Message"); }
		}
		public static MultilingualString EAdaptorImportMessageMenuName => ResString.GetMultilingualString("f4fefc72-cb9a-4240-8c98-e7d49d7e7302", "Import message using eAdaptor - test / support user only");
		public static MultilingualString SimulateMessageImportNoCommitMenuName => ResString.GetMultilingualString("9a7fd5e4-de9d-4cfe-bf11-03f6d6191618", "Simulate import message using eAdaptor - with no commit");
		public static MultilingualString SaveUniversalXmlSchemasMenuName
		{
			get { return ResString.GetMultilingualString("c9be5862-4108-4bc5-ad81-d9b898baf6fe", "Save Universal + Native XML Schemas"); }
		}
		public static MultilingualString GenerateKeyMenuName => ResString.GetMultilingualString("05c79f2b-bfda-4d10-9ad7-f0c36b1b4e39", "Generate Key for Universal XML");
		public static MultilingualString FromEDIMessageMenuName => ResString.GetMultilingualString("04da5348-1696-4a25-8bb7-e841411bdf39", "From Selected EDI Message ");
		public static MultilingualString FromXmlMenuName => ResString.GetMultilingualString("40c9d88d-f09c-4eb4-bd1a-d6cc6966fc81", "From XML File");

		protected virtual bool ShowRequeuingMenu
		{
			get { return true; }
		}

		protected void ResetToQueued_Click(object sender, EventArgs e)
		{
			ResetToQueuedClickHandler(Grid, Factory);
		}

		internal static void ResetToQueuedClickHandler(ZDisplayGrid grid, BusinessObjectFactory factory)
		{
			BusinessObject[] selectedBusinessObjects = grid.SelectedElements;

			if (selectedBusinessObjects.Length == 0)
			{
				Globals.Message.ShowWarning(SelectAtLeastOneMessage);
			}
			else
			{
				if (IsHTTPXMLApplicationCode(selectedBusinessObjects))
				{
					var matchingApplicationCodes = selectedBusinessObjects.Where(bo => HTTPXMLApplicationCodes.Contains(((EDIMessage)bo).EM_ApplicationCode)).Select(bo => ((EDIMessage)bo).EM_ApplicationCode).ToList();
					if (selectedBusinessObjects.Length == 1)
					{
						Globals.Message.ShowWarning($"Messages of application code {matchingApplicationCodes.FirstOrDefault()} cannot be requeued.");
					}
					else
					{
						Globals.Message.ShowWarning($"Messages cannot be requeued because one or more messages have application code {string.Join(",", matchingApplicationCodes)}.");
					}
				}
				else if (Globals.Message.ShowConfirmation(Res.GetString("1c57afd0-3766-439d-bc6e-3136cddb6a77", "Re-queuing the selected record(s) may result in re-sending or re-processing of customs messages. Are you sure you want to proceed?"), ResetStatusToQueuedMenuName, Res.GetString("2b728cf5-07cf-4c96-9b3b-2b9d796c90ac", "yes"), MessageBoxIcon.Warning) == DialogResult.OK)
				{
					foreach (BusinessObject selectedObject in selectedBusinessObjects)
					{
						var resetSupporter = selectedObject as IResetToQueuedStatusSupporter;
						if (resetSupporter != null)
						{
							resetSupporter.ResetToQueuedStatus();
						}
					}
					try
					{
						factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex, new NotificationHandlerWithMessageOverride());
					}
				}
			}
		}

		protected void GenerateKeyFromEDIMessage_Click(object sender, EventArgs e)
		{
			var selectedBusinessObjects = Grid.SelectedElements;
			if (selectedBusinessObjects.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneMessage);
			}
			else
			{
				GenerateKey(selectedBusinessObjects);
			}
		}

		protected void GenerateKeyFromXml_Click(object sender, EventArgs e)
		{
			using (var stream = GetFileStream())
			{
				if (stream != null)
				{
					try
					{
						var message = CreateEDIMessage(stream);
						GenerateKey([message]);
					}
					catch (XmlException exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		void GenerateKey(BusinessObject[] selectedBusinessObjects)
		{
			var directory = GetDirectory();
			if (directory != null)
			{
				var resultMessage = new List<string>();
				using (var tempDir = new TempDirectory())
				{
					foreach (var selectedObject in selectedBusinessObjects)
					{
						var selectedMessage = selectedObject as EDIMessage;
						if (selectedMessage != null)
						{
							GrEngineKeyGenHelper.GenerateKey(selectedMessage, tempDir);
							resultMessage.Add(string.IsNullOrEmpty(selectedMessage.EM_MessageNum) ? (NoResString)"Uploaded Message" : selectedMessage.EM_MessageNum);
						}
					}
					var targetZipFileName = Path.Combine(directory, $"EDIMessageKeysInfo_{ZDateTime.Now:yyyyMMdd_HHmmss}.zip");
					var success = RemoteZipCompression.Zip(tempDir, targetZipFileName);
					if (success)
					{
						Globals.Message.Show(string.Format(Res.GetString("1fa77368-9fb3-433c-a1ac-63bb4234a8b6", "Key generated for message [{0}]."), string.Join(", ", resultMessage)));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("2460cf69-0598-4e4e-aab7-c924184427d0", "Fail creating zip file"));
					}
				}
			}
		}

		internal static bool IsHTTPXMLApplicationCode(BusinessObject[] selectedBusinessObjects)
		{
			foreach (var bo in selectedBusinessObjects)
			{
				if (bo is EDIMessage message)
				{
					if (HTTPXMLApplicationCodes.Contains(message.EM_ApplicationCode))
					{
						return true;
					}
				}
			}
			return false;
		}

		class NotificationHandlerWithMessageOverride : INotificationHandlerWithMessageOverride
		{
			#region INotificationHandler Members

			void INotificationHandler.ReportInformation(string message, string caption)
			{
				Globals.Message.ShowInformation(message, caption);
			}

			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				Globals.Message.ShowError(message, caption);
			}

			#endregion

			#region INotificationHandlerWithMessageOverride Members
			string INotificationHandlerWithMessageOverride.MergeWarningMessage => ConcurrencyMessage;
			string INotificationHandlerWithMessageOverride.CriticalWarningMessage => ConcurrencyMessage;
			string INotificationHandlerWithMessageOverride.CannotDeleteMessage => ConcurrencyMessage;
			string INotificationHandlerWithMessageOverride.DeletedObjectsHeader => null;
			string INotificationHandlerWithMessageOverride.MergedObjectsHeader => null;
			string INotificationHandlerWithMessageOverride.CriticalObjectsHeader => null;
			string INotificationHandlerWithMessageOverride.CannotDeleteObjectsHeader => null;
			#endregion

			string ConcurrencyMessage
			{
				get { return Res.GetString("{DB7237BE-9FEA-459B-AF11-ACB93B53D979}", "Another user has made changes; system cannot reset the data to queue.\r\n\r\nPlease use the 'Find' button to reload the data and try again."); }
			}
		}

		bool ShowMessageInterpreter
		{
			get { return Env.Registry.EnableEDIMessageInterpreter && GlbStaff.CurrentUser.IsSupportUser; }
		}

		public static MultilingualString MessageInterpreterMenuName
		{
			get { return ResString.GetMultilingualString("{3BD1A010-39E0-4F94-86B6-A9E64488AE6E}", "Message &Interpreter"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		void MessageInterpreter_Click(object sender, EventArgs e)
		{
			ZController.ShowModelessFormCore(new GUI.EDIMessageInterpreterForm(new EDIMessageInterpreter()));
		}

		void UniversalXmlSchema_Click(object sender, EventArgs e)
		{
			var directory = GetDirectory();

			try
			{
				if (directory != null)
				{
					var xsdGenerator = ObjectFactory.Get<IUniversalXsdGenerator>();
					var universalFilesWritten = 0;
					var nativeFilesWritten = 0;
					var universalDataObjectsAssembly = typeof(Event).Assembly;
					bool success = true;
					using (var tempDir = new TempDirectory())
					{
						foreach (Type type in universalDataObjectsAssembly.GetExportedTypes())
						{
							if (typeof(TopLevelDataObject).IsAssignableFrom(type) && type != typeof(TopLevelDataObject))
							{
								var schemaInfo = type.GetCustomAttribute<XsdSchemaAttribute>();
								var fileName = schemaInfo.SchemaName;
								var universalXsd = xsdGenerator.GetXsdOutput(type);
								xsdGenerator.SaveXsdFile(universalXsd, Path.Combine(tempDir, fileName));
								universalFilesWritten++;
							}
						}

						var commonSchemaXsd = xsdGenerator.GetCommonSchemaXsdOutput();
						xsdGenerator.SaveXsdFile(commonSchemaXsd, Path.Combine(tempDir, UniversalXmlInfo.CommonSchemaName));
						ExportUniversalInterchangeSchema(xsdGenerator, tempDir);
						ExportUniversalResponseSchema(xsdGenerator, tempDir);
						universalFilesWritten += 3;

						var targetZipFileName = Path.Combine(directory, $@"{UniversalXmlInfo.ZipFileName}.zip");

						success = RemoteZipCompression.Zip(tempDir, targetZipFileName);
					}

					if (success)
					{
						using (var tempDir = new TempDirectory())
						{
							var nativeInterface = ObjectFactory.Get<IImportService>("NativeXmlImportService");
							nativeFilesWritten = nativeInterface.GenerateAndSaveAllXSDs(tempDir);
							xsdGenerator.SaveXsdFile(xsdGenerator.GetCommonSchemaXsdOutput(), Path.Combine(tempDir, UniversalXmlInfo.CommonSchemaName));
							var targetZipFileName = Path.Combine(directory, $@"{NativeXmlInfo.ZipFileName}.zip");
							success = RemoteZipCompression.Zip(tempDir, targetZipFileName);
						}
					}

					if (success)
					{
						Globals.Message.ShowInformation(Res.GetString("0d36440e-8a01-41ce-9fbb-561afce48192"
							, "{0} Universal XML + {1} Native XML schema files exported to [{2}]."
							, universalFilesWritten
							, nativeFilesWritten + 3 // Extra 3 for the Native Root, Request Element and UniversalCommon Schema.
							, directory));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("37D6E8AB-48B5-46B2-B4CB-3E9384AF8B14", "Fail creating Universal or Native XML"));
					}
				}
			}
			catch (XsdCreationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (ArgumentException ex)
			{
				Globals.Message.ShowError(Res.GetString("37D6E8AB-48B5-46B2-B4CB-3E9384AF8B14", "Fail creating Universal or Native XML") + System.Environment.NewLine + ex.Message);
			}
		}

		void ExportUniversalInterchangeSchema(IUniversalXsdGenerator xsdGenerator, string directory)
		{
			var fullNameAndPath = directory + "\\UniversalInterchange.xsd";
			var schemaAssembly = Assembly.Load("Enterprise.Messaging.Module");
			using (var schemaStream = schemaAssembly.GetManifestResourceStream("Enterprise.Messaging.Module.Schemas.UniversalInterchange.xsd"))
			{
				xsdGenerator.SaveXsdFile(schemaStream.WriteToString(), fullNameAndPath);
			}
		}

		void ExportUniversalResponseSchema(IUniversalXsdGenerator xsdGenerator, string directory)
		{
			var fullNameAndPath = directory + "\\UniversalResponse.xsd";
			var schemaAssembly = Assembly.Load("Enterprise.Messaging.Module");
			using (var schemaStream = schemaAssembly.GetManifestResourceStream("Enterprise.Messaging.Module.Schemas.UniversalResponse.xsd"))
			{
				xsdGenerator.SaveXsdFile(schemaStream.WriteToString(), fullNameAndPath);
			}
		}

		public readonly string VersionHeader;

#if DEBUG
		protected virtual
#endif
 string GetDirectory()
		{
			using (var folderBrowserDialog = new ZFolderBrowserDialog())
			{
				var result = ZArchitecture.GUI.ZFormModaliser.ShowCommonDialogWithoutDispose(folderBrowserDialog);
				return result == DialogResult.OK ? folderBrowserDialog.UnmappedSelectedPath : null;
			}
		}

		void UniversalXmlMessageImport_Click(object sender, EventArgs e)
		{
			using (var stream = GetFileStream())
			{
				if (stream != null)
				{
					try
					{
						var message = CreateEDIMessage(stream);

						try
						{
							Factory.Save();
							Globals.Message.ShowInformation(Res.GetString("52e37ea3-1ffb-4bf6-9aa2-091f22b9d4b6", "{0} - {1} message was saved with message number [{2}].", message.EM_MessageSubType, new EDIMessageSubTypeList().GetDescriptionFromCode(message.EM_MessageSubType), message.EM_MessageNum));
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					catch (XmlException exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		void OnEAdaptorMessageImport_Click(object sender, EventArgs e)
		{
			var res = SendMessage();
			if (res != null)
			{
				Globals.Message.Show(res);
			}
		}

#if DEBUG
		protected virtual
#endif
		void OnSimulateMessageImportWithNoCommit_Click(object sender, EventArgs e)
		{
			var result = SendMessage(true);
			if (string.IsNullOrEmpty(result))
			{
				Globals.Message.ShowWarning(Res.GetString(
					"0c88c595-c66a-4f62-abb4-addbd6e60f81",
					"The response is empty."
				));
				return;
			}

			var stream = TryOpenSaveFileStream();
			if (stream == null)
			{
				Globals.Message.Show(Res.GetString(
					"0cefe08d-4ae5-4e45-9e0f-ae59c4f9cadd",
					"Save operation was canceled."
				));
				return;
			}

			using (var writer = new StreamWriter(stream))
			{
				writer.Write(result);
			}

			Globals.Message.Show(Res.GetString(
				"891e21db-c3c7-4e9c-8b47-db2adb6d8950",
				"Output file saved successfully."
			));
		}

#if DEBUG
		protected virtual
#endif
		Stream TryOpenSaveFileStream()
		{
			using (var fileDialog = new ZSaveFileDialog
			{
				DefaultExt = (NoResString)"txt",
				Filter = (NoResString)"Text documents|*.txt"
			})
			{
				if (fileDialog.ShowDialog() != DialogResult.OK)
				{
					return null;
				}

				return fileDialog.OpenFile();
			}
		}

		string SendMessage(bool isRollbackRequired = false)
		{
			using (var stream = GetFileStream())
			{
				if (stream == null)
				{
					return null;
				}
				var messageContent = new StreamReader(stream).ReadToEnd();
				var eAdaptorMessageSender = ObjectFactory.Get<IEAdaptorSupportMessageSender>();
				if (isRollbackRequired)
				{
					return eAdaptorMessageSender.SendInRollbackMode(messageContent);
				}
				else
				{
					return eAdaptorMessageSender.Send(messageContent);
				}
			}
		}

		XmlEDIMessage CreateEDIMessage(Stream stream)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var element = XElement.Parse(messageContent);
			var message = Factory.New<XmlEDIMessage>();

			var rootElementName = element.Name.LocalName;
			switch (rootElementName)
			{
				case "UniversalShipment":
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
					break;

				case "UniversalEvent":
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
					break;

				case "UniversalSchedule":
					if (IsUniversalScheduleSupported)
					{
						message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalSchedule;
					}
					else
					{
						throw new XmlException(GetUnexpectedRootElementTagExceptionMessage(rootElementName, shouldIncludeUniversalSchedule: false));
					}
					break;

				case "UniversalTransaction":
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
					break;

				case "UniversalTransactionBatch":
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
					break;

				case "UniversalActivity":
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivity;
					break;

				default:
					throw new XmlException(GetUnexpectedRootElementTagExceptionMessage(rootElementName, shouldIncludeUniversalSchedule: IsUniversalScheduleSupported));
			}

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageContent;

			return message;
		}

		static bool IsUniversalScheduleSupported => GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper;

		static string GetUnexpectedRootElementTagExceptionMessage(string rootElementName, bool shouldIncludeUniversalSchedule)
		{
			var allowedTypes = EdiMessageTags.UniversalMessage.RootTags.GetNonRequestTags()
				.Where(tag => !tag.Equals(EdiMessageTags.UniversalMessage.RootTags.Schedule) || shouldIncludeUniversalSchedule)
				.ToList();

			allowedTypes.Add(rootElementName);
			var allowedTypeStrings = allowedTypes.Cast<object>().ToArray();

			return shouldIncludeUniversalSchedule
				? Res.GetString("fcb6c479-efa6-487f-8f9f-dd85ed7ddc47", "Expected Root Element Tag to be '{0}', '{1}', '{2}', '{3}', '{4}', or '{5}', but found <{6}> instead.", allowedTypeStrings)
				: Res.GetString("90a3069a-4d66-4ed5-865d-6495e34ee39f", "Expected Root Element Tag to be '{0}', '{1}', '{2}', '{3}', or '{4}', but found <{5}> instead.", allowedTypeStrings);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a path or url.")]
#if DEBUG
		protected virtual
#endif
 Stream GetFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".xml";
				openFileDialog.Filter = "XML Data Files (*.xml)|*.xml";
				openFileDialog.Multiselect = false;

				var result = openFileDialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					return openFileDialog.OpenFile();
				}

				return null;
			}
		}

		public static string SelectAtLeastOneMessage
		{
			get { return Res.GetString("449cf448-66dc-4017-a802-c93093549f65", "Please select at least one message"); }
		}
	}
}
