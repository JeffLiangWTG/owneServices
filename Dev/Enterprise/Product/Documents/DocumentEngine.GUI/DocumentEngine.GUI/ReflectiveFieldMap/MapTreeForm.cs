using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.MacroEvaluator;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using FlexCel.Core;
using static Enterprise.MasterFiles.Business.WorkflowMacroDataContextManager;
using IDataObject = Enterprise.UniversalDataBuss.Integration.IDataObject;
using SimpleLogger = Enterprise.UniversalDataBuss.Management.SimpleLogger;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public partial class MapTreeForm : ZChildForm
	{
		public MapTreeForm(DataReflectorValueProviderWrapper wrapper, DocumentSupporter documentSupporter = null, string dataContextIdentifier = "", object[] parentBusinessObjects = null, bool shouldEscapeAllSpecialCharacters = false, IMacroEvaluationContext context = null, Func<string, string> errorMessageExtender = null)
			: base(wrapper)
		{
			Argument.NotNull(wrapper.DocDataProviderReflectors, "wrapper.DocDataProviderReflectors");
			Argument.GreaterThanZero(wrapper.DocDataProviderReflectors.Count, "wrapper.DocDataProviderReflectors.Length");
			Argument.NotNull(wrapper.ValueProviderMap, "wrapper.ValueProviderMap");

			this.Wrapper = wrapper;
			this.documentSupporter = documentSupporter;
			this.dataContextIdentifier = dataContextIdentifier;
			this.parentBusinessObjects = parentBusinessObjects;
			var createXmlTabPage = wrapper.XmlReflector != null;
			this.context = context ?? GetDefaultMacroContext(MapTreePresentationManager.SelectDefaultLibraryCollection(createXmlTabPage));
			this.ErrorMessageExtender = errorMessageExtender;
			ShouldEscapeAllSpecialCharacters = shouldEscapeAllSpecialCharacters;

			InitializeComponent();
			this.dataPropertiesTab = CreateTabPage(out this.mapTreeUserControl, Res.GetData("MapTreeForm|1a079188-097b-4686-9c92-2a4aa1275137", "Data Fields", "Data Fields", "Heading for Data Fields Tab in Data Field Map Form."), "dataPropertiesTab", "mapTreeUserControl", 0, 1);

			this.dataPropertiesTab.Enter += (sender, e) =>
			{
				Wrapper.SetSelectedProperty(this.mapTreeUserControl.SelectedMember);
			};

			this.editMacroSplitter.AllowOverlap(this.mainPanel);

			mapTreeUserControl.NodeSelected += new MapTreeUserControl.NodeSelectEvent(NodeSelected);
			mapTreeUserControl.NodeDoubleClicked += new MapTreeUserControl.NodeSelectEvent(NodeDoubleClicked);
			this.KeyUp += new KeyEventHandler(MapTreeForm_KeyUp);
			macroGrid.DoubleClick += new EventHandler(macroGrid_DoubleClick);
			tabControl.Click += new EventHandler(tabControl_focus);

			if (wrapper.OperationsMode == DataReflectorValueProviderWrapper.Mode.Browse)
			{
				actionButton.Text = Res.GetString("0e94579e-a6a1-47eb-99a8-2efe684b724a", "Copy to Clipboard");
			}
			
			evaluateMacroButton.Visible = documentSupporter != null && (MacroEvaluatorManager.DataContext_List.ContainsCode(dataContextIdentifier) || MacroEvaluatorManager.SelectedDataContextValue.DataContext == Core.Constants.DataContext.BusinessObject);

			if (createXmlTabPage)
			{
				xmlTab = CreateTabPage(out xmlTreeUserControl, Res.GetData("aae9b397-36c2-4ca6-9878-ceb1ec58ea14", "XML Fields"), "xmlTab", "xmlTreeUserControl", 2, 2);
				xmlTreeUserControl.NodeSelected += new MapTreeUserControl.NodeSelectEvent(XmlNodeSelected);
				xmlTreeUserControl.NodeDoubleClicked += new MapTreeUserControl.NodeSelectEvent(NodeDoubleClicked);
				xmlTab.Enter += (sender, e) =>
				{
					Wrapper.SetSelectedProperty(xmlTreeUserControl.SelectedMember);
				};
				selectXmlButton.Visible = true;
				uxmlStatusLabel.Visible = true;
			}

			if (wrapper.VariableReflectors?.Count > 0)
			{
				this.variablesTab = CreateTabPage(out this.variableTreeUserControl, Res.GetData("MapTreeForm|2e8ed00d-508f-4557-a466-24162db40b27", "Variables", "Variables", "Heading for Variables Tab."), "variablesTab", "variableTreeUserControl", 3, 3);

				this.variablesTab.Enter += (sender, e) =>
				{
					Wrapper.SetSelectedProperty(this.variableTreeUserControl.SelectedMember);
				};

				this.editMacroSplitter.AllowOverlap(this.mainPanel);

				variableTreeUserControl.NodeSelected += new MapTreeUserControl.NodeSelectEvent(NodeSelected);
				variableTreeUserControl.NodeDoubleClicked += new MapTreeUserControl.NodeSelectEvent(NodeDoubleClicked);
			}

			this.dataPropertiesTab.Focus();
			this.tabControl.SelectedTab = this.dataPropertiesTab;
		}

		public void FindExactly(string bindingMember)
		{
			using (var findForm = new MapTreeFindForm(mapTreeUserControl.mapTreeView, mapTreeUserControl))
			{
				findForm.FindExactly(bindingMember);
			}
		}

		public event MacroSelectedEventHandler MacroSelected;

		internal readonly DataReflectorValueProviderWrapper Wrapper;
		readonly DocumentSupporter documentSupporter;
		readonly string dataContextIdentifier;
		internal readonly object[] parentBusinessObjects;
		internal object xmlObject;
		readonly Func<string, string> ErrorMessageExtender;

		public int MacroMaxLength { get; set; }
		bool ShouldEscapeAllSpecialCharacters { get; }
		public bool UseMcrEvaluator { get; set; }
		readonly IMacroEvaluationContext context;
#if DEBUG
		public MacroValueProviderMapCollection MacroGridList
		{
			get
			{
				return (MacroValueProviderMapCollection)macroGrid.List;
			}
		}
#endif

		public bool ShowEditor
		{
			get { return showEditor; }
			set
			{
				showEditor = value;
				editMacroPanel.Visible = value;
				editMacroSplitter.Visible = value;
			}
		}
		bool showEditor;

		internal void NodeDoubleClicked(TreeNode nodeSelected)
		{
			if (ShowEditor)
			{
				buttonAddMacro_Click(null, EventArgs.Empty);
			}
			else
			{
				actionButton_Click(null, EventArgs.Empty);
			}
		}

		void macroGrid_DoubleClick(object sender, EventArgs e)
		{
			if (ShowEditor)
			{
				buttonAddMacro_Click(null, EventArgs.Empty);
			}
			else
			{
				actionButton_Click(null, EventArgs.Empty);
			}
		}

		internal string SelectedMacro
		{
			get
			{
				return ShowEditor && !string.IsNullOrEmpty(textBoxMacro.Text)
					? textBoxMacro.Text
					: SelectedSingleMacro;
			}
		}

		void tabControl_focus(object sender, EventArgs e)
		{
			if (tabControl.SelectedTab == macrosTab)
			{
				macroGrid.Focus();
			}
			else
			{
				(tabControl.SelectedTab ?? dataPropertiesTab).Focus();
			}
		}

		internal string SelectedSingleMacro
		{
			get
			{
				string macro = null;
				var selectedTab = tabControl.SelectedTab ?? dataPropertiesTab;
				if (selectedTab == macrosTab)
				{
					if (macroGrid.SelectedElements.Length > 0)
					{
						macro = ((MacroValueProviderMap)macroGrid.SelectedElements[0]).Usage;
					}
				}
				else if (selectedTab == dataPropertiesTab)
				{
					var selectedMember = mapTreeUserControl.SelectedMember;
					if (selectedMember != null)
					{
						macro = selectedMember.GetMacro();
						if (macro.StartsWith(MacroConstants.DefaultMacroOpeningBracket) && Wrapper.DocDataProviderReflectors.Count > 1)
						{
							var currentTopParentType = Wrapper.DocDataProviderReflectors[Wrapper.SelectedDocDataProviderReflector].DocDataProviderType;
							var additionalRootTypeAttribute = (AdditionalRootTypeAttribute)currentTopParentType.GetCustomAttribute(typeof(AdditionalRootTypeAttribute), true);
							if (additionalRootTypeAttribute != null && Wrapper.DocDataProviderReflectors.Any(x => typeof(IAllowAdditionalRootType).IsAssignableFrom(x.DocDataProviderType)))
							{
								macro = macro.Replace(MacroConstants.DefaultMacroOpeningBracket, MacroConstants.DefaultMacroOpeningBracket + MacroDataSource.Constants.Prefix + "." + additionalRootTypeAttribute.DataSourceTypeName + ".");
							}
						}
					}
				}
				else if (selectedTab == xmlTab)
				{
					var selectedMember = xmlTreeUserControl.SelectedMember;
					if (selectedMember != null)
					{
						macro = selectedMember.GetMacro();
					}
				}
				else if (selectedTab == variablesTab)
				{
					if (variableTreeUserControl.SelectedTreeNode?.LastNode != null)
					{
						return string.Empty;
					}
					var selectedMember = variableTreeUserControl.SelectedMember;
					if (selectedMember != null)
					{
						macro = selectedMember.GetMacro();
						macro = $"@{variableTreeUserControl.SelectedTreeNode.FullPath.Split('\\')[0]}.{macro}";
					}
				}
				return macro;
			}
		}

		internal void NodeSelected(MapTreeNode nodeSelected)
		{
			if (nodeSelected != null && Wrapper.DocDataProviderReflectors.Count > 1)
			{
				TreeNode rootNode = nodeSelected;
				while (rootNode.Parent != null)
				{
					rootNode = rootNode.Parent;
				}
				var index = GetSelectedDocDataProviderReflectorIndex(rootNode);
				if (index >= 0)
				{
					Wrapper.SelectedDocDataProviderReflector = index;
				}
			}
			Wrapper.SetSelectedProperty(nodeSelected?.MemberDescription);
		}

		int GetSelectedDocDataProviderReflectorIndex(TreeNode node)
		{
			var index = -1;
			foreach (var reflector in Wrapper.DocDataProviderReflectors)
			{
				index++;
				if (node.Tag is Type nodeType && nodeType == reflector.DocDataProviderType)
				{
					break;
				}
			}
			return index;
		}

		internal void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void actionButton_Click(object sender, EventArgs e)
		{
			if (SelectedMacro == null)
			{
				Globals.Message.Show(Res.GetString("631c5ef9-6950-4629-9c4a-9d75aaef28ad", "Please select a macro"));
			}
			else
			{
				switch (Wrapper.OperationsMode)
				{
					case DataReflectorValueProviderWrapper.Mode.Browse:
						SafeClipboard.SetText(SelectedMacro);
						break;

					case DataReflectorValueProviderWrapper.Mode.Select:
						if (MacroMaxLength > 0 && SelectedMacro.Length > MacroMaxLength)
						{
							Globals.Message.Show(Res.GetString("{40f55ff9-872d-43de-bd9c-030bdaf85208}", "Macro length cannot be greater than {0} symbols", MacroMaxLength));
							return;
						}

						Wrapper.SelectedMacro = SelectedMacro;
						if (MacroSelected != null)
						{
							MacroSelected(SelectedMacro);
						}
						Close();
						break;
				}
			}
		}

		void buttonAddMacro_Click(object sender, EventArgs e)
		{
			textBoxMacro.SelectedText = RemoveTextWithinBraces(SelectedSingleMacro);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			var onlyIfPassNullParentBusinessObject = OnlyIfPassNullParentBusinessObject();
			if (onlyIfPassNullParentBusinessObject)
			{
				buttonEvaluateJob.Visible = true;
				buttonEvaluateJob.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(buttonAddMacro.Location.X) + 100, 83);
				buttonEvaluateMacro.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(buttonAddMacro.Location.X) - 100, 83);
			}
			else
			{
				buttonEvaluateJob.Visible = false;
				buttonEvaluateJob.Location = ControlDpiScalingHelper.NewScaledPoint(372, 83);
				buttonEvaluateMacro.Location = ControlDpiScalingHelper.NewScaledPoint(323, 83);
			}
		}

		bool OnlyIfPassNullParentBusinessObject()
		{
			return documentSupporter is null && OnlyIfPassNullBizObjToEvaluateMarco(GetScope());
		}

		bool OnlyIfPassNullBizObjToEvaluateMarco(MacroScope scope)
		{
			return UseMcrEvaluator && scope?.Data is BusinessObject { IsNull: true };
		}

		void buttonEvaluateMacro_Click(object sender, EventArgs e)
		{
			buttonEvaluateMacro_Click();
		}

		void buttonEvaluateJob_Click(object sender, EventArgs e)
		{
			buttonEvaluateJob_Click();
		}

#if DEBUG
		public
#endif
		void buttonEvaluateMacro_Click()
		{
			textBoxEvaluationResult.ForeColor = SystemColors.ControlText;
			var macroValuePath = textBoxMacro.Text;
			if (macroValuePath.IsNullOrEmpty())
			{
				return;
			}

			if (documentSupporter != null)
			{
				MacroEvaluatorManager.Macro = macroValuePath;
				MacroEvaluatorManager.Evaluate();
				textBoxEvaluationResult.Text = MacroEvaluatorManager.Output;
			}
			else
			{
				if (UseMcrEvaluator)
				{
					var scope = GetScope();
					EvaluateByMcr(scope, macroValuePath);
				}
				else if (parentBusinessObjects is { Length: > 0 })
				{
					textBoxEvaluationResult.Text = ObjectFactory.Get<ITextMacroProcessor>().Replace(macroValuePath, parentBusinessObjects, shouldEscapeAllSpecialCharacters: ShouldEscapeAllSpecialCharacters);
				}
			}
		}

		void buttonEvaluateJob_Click()
		{
			if (!UseMcrEvaluator)
			{
				return;
			}

			textBoxEvaluationResult.ForeColor = SystemColors.ControlText;
			var macroValuePath = textBoxMacro.Text;
			if (macroValuePath.IsNullOrEmpty())
			{
				return;
			}

			var scope = GetEvaluateJobScope();
			EvaluateByMcr(scope, macroValuePath);
		}

		MacroScope GetScope()
		{
			var result = (parentBusinessObjects is { Length: > 0 } && parentBusinessObjects[0] is MacroScope)
				? (MacroScope)parentBusinessObjects[0]
				: new MacroScope(parentBusinessObjects is { Length: > 0 }
					? parentBusinessObjects[0]
					: null);

			return result;
		}

		MacroScope GetEvaluateJobScope()
		{
			var scope = GetScope();

			if (OnlyIfPassNullBizObjToEvaluateMarco(scope)
				&& scope.Data is IWorkflowProvider workflowProvider
				&& WorkflowDescriptors.Instance.TryGetValue(workflowProvider.WorkflowType, out var descriptor)
				&& descriptor.ControllerID is { } controllerId
				&& ZControllerFactory.Create(controllerId) is { ModuleID: { } moduleId }
				&& moduleId != ModuleIDs.NotAssigned)
			{
				using var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleId);
				var jobs = BusinessObjectModulePicker.PickFromModuleScreen(module.GridCollection, moduleId, allowMultiSelect: false);
				if (jobs.Length == 1)
				{
					return new MacroScope(jobs[0]);
				}
			}

			return null;
		}

		void EvaluateByMcr(MacroScope scope, string macroValuePath)
		{
			if (!UseMcrEvaluator || scope is null || macroValuePath.IsNullOrEmpty())
			{
				return;
			}

			using (scope)
			{
				var expression = macroValuePath.With(context).CreateExpression();

				if (xmlObject != null)
				{
					scope.SetVariable("UXML", xmlObject);
				}

				textBoxEvaluationResult.Text = HandleMcrResult(expression.Evaluate(scope));
				if (!expression.HasErrors())
				{
					return;
				}

				var type = scope.Data?.GetType();
				var isProxy = type != null && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessObjectEventDataModel<,>));
				textBoxEvaluationResult.ForeColor = Colors.Red;
				textBoxEvaluationResult.Text = string.Join(System.Environment.NewLine, expression.Errors.Select(em => em.Message));
				if (isProxy)
				{
					var regex = new Regex(Regex.Escape(type.Name));
					textBoxEvaluationResult.Text = regex.Replace(textBoxEvaluationResult.Text, type.BaseType.Name, 1);
				}

				if (ErrorMessageExtender != null)
				{
					textBoxEvaluationResult.Text = ErrorMessageExtender(textBoxEvaluationResult.Text);
				}
			}
		}

		public static string RemoveTextWithinBraces(string input)
		{
			string pattern = @"(?<=\()[^)]*(?=\))";
			return string.IsNullOrEmpty(input) ? string.Empty : Regex.Replace(input, pattern, "").Trim();
		}

		void evaluateMacroButton_Click(object sender, EventArgs e)
		{
			MacroEvaluatorManager.Macro = textBoxMacro.Text;
			ZFormModaliser.Show(new MacroEvaluatorForm(MacroEvaluatorManager), this);
		}

		string HandleMcrResult(object resultObj)
		{
			if (resultObj == null)
			{
				return string.Empty;
			}
			if (resultObj is ZDateTime dateTime)
			{
				var includeTicks = dateTime.Ticks % TimeSpan.TicksPerSecond > 0;
				return dateTime.ToString(Culture.Invariant.DateTimeFormat.FullDateTimePattern + (includeTicks ? ".fff" : ""), CultureInfo.InvariantCulture);
			}
			if (resultObj is ZDateTimeOffset dateTimeOffset)
			{
				var includeTicks = dateTimeOffset.Ticks % TimeSpan.TicksPerSecond > 0;
				return dateTimeOffset.ToString(Culture.Invariant.DateTimeFormat.FullDateTimePattern + (includeTicks ? ".fffffff" : "") + " zzz", CultureInfo.InvariantCulture);
			}
			return resultObj.ToString();
		}

		MacroEvaluatorManager MacroEvaluatorManager
		{
			get
			{
				if (macroEvaluatorManager == null)
				{
					macroEvaluatorManager = new MacroEvaluatorManager(documentSupporter.Factory, documentSupporter.BusinessObject as IDocumentSupportable) { DataContextCode = dataContextIdentifier };
				}

				return macroEvaluatorManager;
			}
		}
		MacroEvaluatorManager macroEvaluatorManager;

		void MapTreeForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C && SelectedMacro != null)
			{
				if (textBoxMacro.SelectionLength > 0)
				{
					SafeClipboard.SetText(textBoxMacro.SelectedText);
				}
				else
				{
					SafeClipboard.SetText(SelectedMacro);
				}
				e.Handled = true;
			}
		}

		void MapTreeForm_Load(object sender, EventArgs e)
		{
			mapTreeUserControl.SetReflectors(Wrapper.DocDataProviderReflectors);
			if (Wrapper.XmlReflector != null)
			{
				xmlTreeUserControl.SetReflectors(new[] { Wrapper.XmlReflector });
			}
			if (Wrapper.VariableReflectors?.Count > 0)
			{
				variableTreeUserControl.SetReflectors(Wrapper.VariableReflectors, Wrapper.VariableNames);
			}
			ResizeElements();
		}

		public override string FormHeading
		{
			get
			{
				var docDataProvideType = Wrapper.DocDataProviderReflectors[0].DocDataProviderType;
				if (docDataProvideType.GetInterfaces()
						.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessObjectEventDataModel<,>)))
				{
					docDataProvideType = docDataProvideType.BaseType;
				}

				var typeName = docDataProvideType.IsInterface ? docDataProvideType.Name.Substring(1) : docDataProvideType.Name;
				var prefix = UseMcrEvaluator ? Res.GetString("738a2ec7-ce2f-4b44-8ec5-30e36bc4f645", "MCR Data Field Map") : Res.GetString("2f2de7b6-33b7-4b19-a7b2-0be7d1f6ab36", "Data Field Map");
				return prefix + " - " + typeName;
			}
		}

		void MapTreeForm_Resize(object sender, EventArgs ne)
		{
			ResizeElements();
		}

		void ResizeElements()
		{
			var widthAdjusted = macroGrid.Size.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			macroGrid.SetColumnWidth((NoResString)"Usage", widthAdjusted < ControlDpiScalingHelper.ScaleToCurrentDpiX(700) ? ControlDpiScalingHelper.ScaleToCurrentDpiX(700) : widthAdjusted);   // Contains a reference to a column name only, not visible to end user.
		}

		void MacroGrid_MouseDown(object sender, MouseEventArgs e)
		{
			Point pt = macroGrid.PointToClient(Cursor.Position);
			DataGrid.HitTestInfo info = macroGrid.HitTest(pt);
			string macroText = string.Empty;

			try
			{
				macroText = macroGrid[info.Row, info.Column].ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }

			if (!string.IsNullOrEmpty(macroText))
			{
				DoDragDrop(macroText, DragDropEffects.All);
			}
		}

		#region XML tab
		internal ZTabPage xmlTab;
		internal MapTreeUserControl xmlTreeUserControl;

		internal void XmlNodeSelected(MapTreeNode nodeSelected)
		{
			Wrapper.SetSelectedProperty(nodeSelected?.MemberDescription);
		}

		#endregion

		ZTabPage CreateTabPage(out MapTreeUserControl ctrl, ResourceStringData stringData, string tabName, string ctrlName, int tabIdx, int ctrlIdx)
		{
			var tab = new ZTabPage();
			ctrl = new MapTreeUserControl();
			var panel = new MapTreeRightPanel(this.Wrapper);
			tab.SuspendLayout();
			ctrl.SuspendLayout();
			tabControl.Controls.Add(tab);

			tab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 13, true);
			tab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			tab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 334, true);

			tab.CaptionResourceString = stringData;
			tab.Controls.Add(ctrl);
			tab.Location = macrosTab.Location;
			tab.Name = tabName;
			tab.Padding = macrosTab.Padding;
			tab.Size = macrosTab.Size;
			tab.TabIndex = tabIdx;
			tab.UseVisualStyleBackColor = true;
			tab.Controls.Add(panel);

			ctrl.AllowDrop = true;
			ctrl.Dock = System.Windows.Forms.DockStyle.Fill;
			ctrl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			ctrl.Name = ctrlName;
			ctrl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 328, true);
			ctrl.TabIndex = ctrlIdx;

			tab.ResumeLayout(false);
			tab.PerformLayout();
			ctrl.ResumeLayout(true);
			ctrl.PerformLayout();

			return tab;
		}

		void selectXmlButton_Click(object sender, EventArgs e)
		{
			var hasErrors = false;
			var fileName = string.Empty;
			try
			{
				using (var fileDialog = new ZOpenFileDialog())
				{
					fileDialog.Multiselect = false;
					fileDialog.Filter = (NoResString)"XML files (*.xml)|*.xml|All files (*.*)|*.*";

					if (fileDialog.ShowDialog() == DialogResult.OK)
					{
						fileName = fileDialog.UnmappedFileName;
						using (var stream = fileDialog.OpenFile())
						{
							using (var memoryStream = new MemoryStream(stream.ToByteArray()))
							{
								var xml = (IDataObject)Activator.CreateInstance(Wrapper.XmlReflector.DocDataProviderType);
								IXmlSessionTracker logger = new XmlSessionTracker(new SimpleLogger());
								ObjectFactory.Get<IXmlReader>().ReadXML(xml, (SubStreamableStream)memoryStream, logger);

								if (!logger.HasErrors)
								{
									xmlObject = xml;
								}
								else
								{
									hasErrors = true;
								}
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				hasErrors = true;
			}

			if (hasErrors)
			{
				Globals.Message.ShowError(Res.GetString("3f5a08a9-87e2-4db6-98ab-49a2bb8866ef", "Unable to load Universal XML from File. Ensure that the file exists and contains valid data"));
			}
			else
			{
				uxmlStatusLabel.ForeColor = Colors.Green;
				uxmlStatusLabel.Text = Path.GetFileName(fileName);
			}
		}

		public static IMacroEvaluationContext GetDefaultMacroContext(IMacroLibrary[] libraryCollection)
		{
			return libraryCollection.CreateContext();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.Yes;
		}
	}
}
