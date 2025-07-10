using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public class IssueManagerModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.IssueManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.IssueManager; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		#region Assign

		void AutoCreateWorkItem_Click(object sender, EventArgs e)
		{
			AutoCreateWorkItems(IssueManagerEmbeddedControl.FilteredGrid.SelectedElements);
		}

		void CloseMenuItem_Click(object sender, EventArgs e)
		{
			Close(IssueManagerEmbeddedControl.FilteredGrid.SelectedElements);
		}

		#region Auto Create WorkItems

		internal void AutoCreateWorkItems(IEnumerable<BusinessObject> selectedElements)
		{
			int changedCount = 0;
			int totalCount = 0;
			foreach (EdiHelpErrorLog errorLog in selectedElements)
			{
				EdiHelpErrorLog clonedLog = Factory.Load<EdiHelpErrorLog>(errorLog.PK);
				if (clonedLog != null)
				{
					clonedLog.CreateWorkItem();
					changedCount++;
				}
				totalCount++;
			}
			Factory.Save();
			string message = changedCount.ToString() + " WorkItem been created within " + totalCount + " issues.";
			Globals.Message.ShowInformation(message);
		}

		#endregion

		#endregion

		#region Merge

		internal void MergeMenuItem_Click(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.IssueManagerMergeTool.IsAllowed)
			{
				LogMerger logMerger = new LogMerger();
				ZGuid newMasterPK = logMerger.Merge(SelectedHelpErrorLogs);

				if (newMasterPK.IsValid)
				{
					SelectInGrid(newMasterPK);
					ShowInformation("Selected issues succesfully merged.");
				}
				else
				{
					ShowError(logMerger.ErrorMessage);
				}
			}
			else
			{
				EDISecurityCheckpoints.IssueManagerMergeTool.ShowError();
			}
		}

		EdiHelpErrorLog[] SelectedHelpErrorLogs
		{
			get
			{
				List<EdiHelpErrorLog> result = new List<EdiHelpErrorLog>();
				foreach (BusinessObject o in IssueManagerEmbeddedControl.FilteredGrid.SelectedElements)
				{
					EdiHelpErrorLog log = o as EdiHelpErrorLog;
					if (log != null)
					{
						result.Add(log);
					}
				}
				return result.ToArray();
			}
		}

		void SelectInGrid(ZGuid masterPK)
		{
			if (Grid.List != null)
			{
				Grid.Select(Grid.List.IndexOf(((BusinessObjectCollection)Grid.List).FindByPK(masterPK)));
			}
		}

		#endregion

		#region Close

		internal void Close(BusinessObject[] selectedElements)
		{
			if (selectedElements.Length > 0)
			{
				LogCloser closer = new LogCloser(selectedElements);
				closer.Close();

				if (!closer.HasDBChanged)
				{
					string issuePlural = selectedElements.Length > 1 ? "issues" : "issue";
					string hasPlural = selectedElements.Length > 1 ? "have" : "has";

					if (closer.Errors.Length == 0)
					{
						ShowInformation(selectedElements.Length + " " + issuePlural + " " + hasPlural + " been closed.");
					}
					else
					{
						ShowError("Errors were encountered trying to close the " + issuePlural + "." + System.Environment.NewLine + closer.Errors);
					}
				}
				else
				{
					ShowError("One or more of the selected issues has been changed. Please refresh the grid and try again.");
				}
			}
			else
			{
				ShowError("Please select one or more issues to close.");
			}
		}

		#endregion

		#region Implementation

		protected override Control GetNewEmbeddedControl()
		{
			IssueManagerFilterControl filterControl = (IssueManagerFilterControl)base.GetNewEmbeddedControl();
			filterControl.FilteredGrid.AfterBind += new EventHandler(FilteredGrid_AfterBind);

			return filterControl;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.IssueManager);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IssueManagerFilterControl(GridCollection, (IssueManagerFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HelpErrorLogCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IssueManagerFilterBusinessObject();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>();

			result.Add(new ZMenuItem("View", HandleViewClick));
			result.Add(new ZMenuItem("Merge", MergeMenuItem_Click));
			result.Add(new ZMenuItem("Close", CloseMenuItem_Click));

			return result.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem autoCreateWorkMenuItem = new ZMenuItem("Auto-create Workitem", AutoCreateWorkItem_Click);
			result.Add(autoCreateWorkMenuItem);
#if DEBUG
			MenuItem debugMenuItem = new ZMenuItem("Debug");
			result.Add(debugMenuItem);
#endif
			return result.ToArray();
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			IssueManagerEmbeddedControl.SplitterValue = Settings.SplitterValue;
			IssueManagerEmbeddedControl.FilteredGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
			IssueManagerEmbeddedControl.FilteredGrid.ListManager.PositionChanged += ListManager_PositionChanged;
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (IsEmbeddedControlConstructed)
				{
					if (IssueManagerEmbeddedControl.FilteredGrid.ListManager != null)
					{
						IssueManagerEmbeddedControl.FilteredGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
						IssueManagerEmbeddedControl.FilteredGrid.ListManager.PositionChanged -= ListManager_PositionChanged;
					}

					IssueManagerEmbeddedControl.FilteredGrid.AfterBind -= FilteredGrid_AfterBind;
					Settings.Save(this, IssueManagerEmbeddedControl);
				}
			}

			base.Dispose(isDisposing);
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			SetPreview();
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			SetPreview();
		}

		EdiHelpErrorLog CurrentLog;

		void SetPreview()
		{
			string cleanExceptionText = "";

			if (Grid.ListManager != null)
			{
				EdiHelpErrorLog currentLog = Grid.ListManager.GetCurrent() as EdiHelpErrorLog;
				if (currentLog != CurrentLog)
				{
					CurrentLog = currentLog;

					if (CurrentLog != null && CurrentLog.FirstOccurrence != null)
					{
						try
						{
							if (CurrentLog.FirstOccurrence.HO_XMLData.EndsWith("#TRUNCATED#"))
							{
								cleanExceptionText = "Xml was truncated due to excessive length.";
							}
							else
							{
								XmlDocument xmlDoc = new XmlDocument();
								xmlDoc.LoadXml(CurrentLog.FirstOccurrence.HO_XMLData);
								cleanExceptionText = ExceptionText(xmlDoc);
							}
						}
						catch (XmlException ex)
						{
							cleanExceptionText = ex.Message;
						}
					}

					IssueManagerEmbeddedControl.PreviewTextBox.Text = cleanExceptionText;
					IssueManagerEmbeddedControl.PreviewTextBox.SelectionLength = 0;
				}
			}
			else
			{
				CurrentLog = null;
				IssueManagerEmbeddedControl.PreviewTextBox.Text = "";
			}
		}

		IssueManagerFilterControl IssueManagerEmbeddedControl
		{
			get { return (IssueManagerFilterControl)EmbeddedControl; }
		}

		#region Exception Text

		internal protected string ExceptionText(XmlDocument xmlDoc)
		{
			StringBuilder sb = new StringBuilder();
			AppendException(0, sb, xmlDoc.DocumentElement);

			return sb.ToString();
		}

		void AppendException(int level, StringBuilder sb, XmlElement xmlElement)
		{
			string baseXPath = "ExceptionDetails/";
			for (int i = 0; i < level; i++)
			{
				baseXPath += "InnerException/";
			}

			XmlNodeList nodes = xmlElement.SelectNodes(baseXPath.Substring(0, baseXPath.Length - 1));
			if (nodes.Count > 0 && nodes[0].InnerText != "NULL")
			{
				AppendException(level + 1, sb, xmlElement);

				string messageXPath = baseXPath + "Message";
				string exceptionTypeXPath = baseXPath + "ExceptionType";
				string callXPath = baseXPath + "StackTrace/Call";

				XmlNodeList message = xmlElement.SelectNodes(messageXPath);
				XmlNodeList exceptionType = xmlElement.SelectNodes(exceptionTypeXPath);
				if (message.Count > 0 && exceptionType.Count > 0)
				{
					if (level > 0)
					{
						sb.Append("");
						sb.Append(exceptionType[0].InnerText);
						sb.Append(" (Inner ");
						sb.Append(level);
						sb.Append("): ");
					}
					else
					{
						sb.Append(exceptionType[0].InnerText);
						sb.Append(": ");
					}

					sb.Append(RemoveFormatting(message[0].InnerText));
					sb.Append(System.Environment.NewLine);
				}

				XmlNodeList calls = xmlElement.SelectNodes(callXPath);
				for (int i1 = 0; i1 < calls.Count; i1++)
				{
					sb.Append(RemoveFormatting(calls[i1].InnerText));
				}

				if (level > 0)
				{
					sb.Append(System.Environment.NewLine);
				}
			}
		}

		const int MaxPreviewMessageLength = 1024 * 1024;

		string RemoveFormatting(string text)
		{
			text = text.Trim().Replace(new string('\\', 10), "");
			text = text.Length > MaxPreviewMessageLength ? text.Substring(0, MaxPreviewMessageLength) : text;
			if (text.StartsWith("at") || text.StartsWith("-----"))
			{
				text = "\t" + text;
			}

			return text + System.Environment.NewLine;
		}

		#endregion

		#region Module Custom Settings

		ModuleSettings Settings
		{
			get { return settings ?? (settings = new ModuleSettings()); }
		}
		ModuleSettings settings;

		class ModuleSettings
		{
			public ModuleSettings()
			{
				RegistryItem = new StringRegistryItem("IssueManagerFormData", null, null, null, RegistryStorageFlags.Company);
				Load();
			}

			public bool FilterControlVisibility;
			public int SplitterValue;

			readonly IRegistryItem RegistryItem;

			void Load()
			{
				string[] settings = ModuleData.Split(',');
				if (settings.Length == 3)
				{
					FilterControlVisibility = bool.Parse(settings[0]);
					//PreviewVisibility = bool.Parse(settings[1]); removed....
					SplitterValue = int.Parse(settings[2]);
				}
			}

			public void Save(IssueManagerModule module, IssueManagerFilterControl control)
			{
				ModuleData = string.Join(",", new string[] { bool.TrueString, bool.TrueString, control.SplitterValue.ToString() });
			}

			string ModuleData
			{
				get { return (string)RegistryItem.GetValueWithoutFallback(GlbStaff.CurrentUser.PK.ToGuid(), Guid.Empty, Guid.Empty); }
				set { RegistryItem.SetValue(GlbStaff.CurrentUser.PK.ToGuid(), Guid.Empty, Guid.Empty, value); }
			}
		}

		#endregion

		void ShowInformation(string message)
		{
			Globals.Message.ShowInformation(message);
		}

		void ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

#endregion
	}
}
