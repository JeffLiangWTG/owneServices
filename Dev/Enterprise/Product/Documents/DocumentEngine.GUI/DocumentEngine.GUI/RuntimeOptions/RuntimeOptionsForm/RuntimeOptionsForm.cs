using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class RuntimeOptionsForm : ZChildForm, IDeliverCapableForm, ICanAttachWithoutSecurity
	{
		public RuntimeOptionsForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Used when constructing the form to display errors for one Report.
		/// </summary>
		/// <param name="report"></param>
		public RuntimeOptionsForm(Report report) : this(report, AllowedDeliveryOptions.All, null, null, true)
		{
		}

		/// <summary>
		/// Used when running a Report from a Reports Module Grid. Will show Filter Options and any errors found during the pre run Analysis of the template.
		/// </summary>
		public RuntimeOptionsForm(PrintTask printTask, Report report, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			: this(report, deliveryOptions, instructions, modifyDocumentCheckPoint)
		{
			this.printTask = printTask;
			DeliveryRequested += printTask.Form_DeliveryRequested;

			ActionButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("1f6a5c77-a4dc-47bc-9324-9ece36962bbc", "&Deliver");
			CloseButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("7c5280ea-9b40-49f4-b114-4afcc9f04e36", "&Close");
		}

		internal readonly PrintTask printTask;

		internal RuntimeOptionsForm(Report report, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint, bool isErrorForm = false)
			: base(report)
		{
			this.isShowingReportOptions = (instructions != null);
			fInstructions = instructions;
			InitializeComponent();
			isErrorDisplayForm = isErrorForm;
			OpenedFormCache.GetInstance().AdditionalFormsCount += 1;
			fReport = report;

			fReport.PrepareForRender();

			AddControls();

			if (fReport.Parent.StmMenuCommand != null)
			{
				Text = fReport.Parent.StmMenuCommand.SU_MenuNameMultilingual;
			}

			fDeliveryOptions = deliveryOptions;
			fModifyDocumentCheckPoint = modifyDocumentCheckPoint;
			fReport.ColumnHeadingManager.CurrentConfigurationRefreshed += headingManager_ColumnHeadingManagerLoaded;
			SetLanguage(fReport.Language);
		}

		/// <summary>
		/// Used when you are entering parameters for a Report being configured as a Scheduled Report.
		/// </summary>
		public RuntimeOptionsForm(Report report, ReportScheduleTask scheduleTask)
			: base(report)
		{
			InitializeComponent();
			OpenedFormCache.GetInstance().AdditionalFormsCount += 1;
			this.ScheduleTask = scheduleTask;
			fReport = report;
			ReportSerializationInfo info = null;
			info = scheduleTask.CreateReportFromTask();
			if (info != null)
			{
				fReport.DeserializedReport = info.Report;
				shouldSuspendResetReportHeadingTextFirstTime = true;
			}

			fReport.PrepareForRender();
			if (info != null)
			{
				fReport.Parent.Language = info.Language;
				SetLanguage(info.Language);
			}
			fReport.ColumnHeadingManager.SaveLastSavedSetting(fReport);
			fReport.ColumnHeadingManager.CurrentConfigurationRefreshed += headingManager_ColumnHeadingManagerLoaded;
			AddControls();
		}

		protected override bool ShouldRememberPositionAndSize => false;

		bool shouldSuspendResetReportHeadingTextFirstTime;
		readonly bool isErrorDisplayForm;
		internal readonly Report fReport;
		readonly AllowedDeliveryOptions fDeliveryOptions;
		readonly DeliveryInstructions fInstructions;
		readonly ISecurityCheckpoint fModifyDocumentCheckPoint;
		internal ZButton PreviewButton;
		readonly bool isShowingReportOptions;
		IDisposable additionalDisposableAction;

		public new Report BusinessEntity
		{
			get { return fReport; }
		}

		#region GUI Setup

		const int MaxClientHeight = 720;
		const int MinSizeWidth = 595;
		const int MinSizeHeight = 200;

		void RuntimeOptionsForm_Load(object sender, EventArgs e)
		{
			var preferredSize = CalculateDesiredSizes();
			if (preferredSize.Height > ControlDpiScalingHelper.ScaleToCurrentDpiY(MaxClientHeight) || FilterTabControl.DesiredHeight > FilterTabControl.MaxPageHeight)
			{
				MakeGroupBoxesHaveTwoColumns();
				preferredSize = CalculateDesiredSizes();
			}
			PositionGroupBoxesVertically();

			var screenSize = CachedScreenInfo.Instance.FromControl(this);
			var borderSize = Size - ClientSize;
			var preferredMaxSize = preferredSize + borderSize;

			if (!isErrorDisplayForm)
			{
				MinimumSize = ControlDpiScalingHelper.NewScaledSize(
					MinSizeWidth,
					MinSizeHeight,
					false
				);
				MaximumSize = ControlDpiScalingHelper.NewScaledSize(
					Math.Max(MinSizeWidth, preferredMaxSize.Width),
					Math.Max(MinSizeHeight, preferredMaxSize.Height),
					false
				);
			}

			var realSizeWidth = Math.Min(screenSize.Width, preferredSize.Width + borderSize.Width);
			if (realSizeWidth < MinSizeWidth)
			{
				realSizeWidth = MinSizeWidth;
			}
			var realSizeHeight = Math.Min(screenSize.Height, preferredSize.Height + borderSize.Height);
			if (realSizeHeight < MinSizeHeight)
			{
				realSizeHeight = MinSizeHeight;
			}

			Size = ControlDpiScalingHelper.NewScaledSize(realSizeWidth, realSizeHeight, false);

			ResizeErrorsGrid();
		}

		void ResizeErrorsGrid()
		{
			if (ErrorsGridView.Visible)
			{
				this.ErrorsGridView.Size = ControlDpiScalingHelper.NewScaledSize(this.ErrorsPanel.Width, this.ErrorsPanel.Height);
			}
		}

		void RuntimeOptionsForm_Shown(object sender, EventArgs e)
		{
			ConfirmReportDatabaseOption();
		}

		void RuntimeOptionsForm_ClientSizeChanged(object sender, EventArgs e)
		{
			if (LanguageZDropEdit.Visible)
			{
				ControlDpiScalingHelper.SetLeft(ref DropEditReportOrientation, LanguageZDropEdit.Right + LabelReportOrientation.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(50), false);
				ControlDpiScalingHelper.SetLeft(ref LabelReportOrientation, DropEditReportOrientation.Left - LabelReportOrientation.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			}
			else
			{
				ControlDpiScalingHelper.SetLeft(ref DropEditReportOrientation, LabelReportOrientation.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);
				ControlDpiScalingHelper.SetLeft(ref LabelReportOrientation, ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			}
		}

		#region Show Override Report Database Confirmation

		void ConfirmReportDatabaseOption()
		{
			if (reportDbManager.IsReportingDbEnabled)
			{
				var report = BusinessEntity;

				if (report != null)
				{
					if (ScheduleTask != null)
					{
						report.OverrideReportDbOption = false;
					}
					else if (report.MustRunOnline)
					{
						report.OverrideReportDbOption = true;
					}
					else if (report.Style == Report.Styles.Report)
					{
						report.OverrideReportDbOption = ShouldOverrideReportDbOption();
					}
				}
			}
		}

		bool ShouldOverrideReportDbOption()
		{
			bool result = false;

			if (sessionOverrideReportDbOption.HasValue)
			{
				result = sessionOverrideReportDbOption.Value;
			}
			else
			{
				if (Env.Security.OverrideReportDatabaseOption.IsAllowed)
				{
					waitWhileICalculateDelay.Reset();
					var thread = new Thread(new ThreadStart(delegate
					{
						using (Db.DisposableActionForDbConnection())
						{
							var tmp = this.NeedToOverrideReportDbOption;//this.ReportServerDelay;
							waitWhileICalculateDelay.Set();
						}
					}));
					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();
					needToCalculateOverrideReportDbOption = true;
				}
				else
				{
					sessionOverrideReportDbOption = result = false;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		internal void CalculateOverrideReportDbOption()
		{
			if (needToCalculateOverrideReportDbOption)
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					progressForm.ShowModalTo(this);

					while (!waitWhileICalculateDelay.WaitOne(100))
					{
						Application.DoEvents();
					}
				}

				var report = BusinessEntity;
				if (report != null)
				{
					report.OverrideReportDbOption = NeedToOverrideReportDbOption;
				}

				sessionOverrideReportDbOption = NeedToOverrideReportDbOption;
				needToCalculateOverrideReportDbOption = false;
			}
		}

		readonly EventWaitHandle waitWhileICalculateDelay = new EventWaitHandle(true, EventResetMode.ManualReset);
		bool needToCalculateOverrideReportDbOption;

		bool NeedToOverrideReportDbOption
		{
			get
			{
				if (!needToOverrideReportDbOption.HasValue)
				{
					needToOverrideReportDbOption = reportDbManager.NeedUsePrimaryServer;
				}
				return needToOverrideReportDbOption.Value;
			}
		}
		protected bool? needToOverrideReportDbOption;

		internal ISecondaryServerConnectionProvider reportDbManager = SecondaryServerConnectionProviderProvider.GetProvider(Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox);
		protected static bool? sessionOverrideReportDbOption;

		#endregion

		void AddControls()
		{
			if (isErrorDisplayForm)
			{
				AddErrors();
			}
			else
			{
				AddAllControls();
			}
		}

		void AddAllControls()
		{
			AddFilterControls();
			AddSortOrderControls();
			AddGroupByControls();
			AddOptionalTemplateControls();
			HideOrientationDropEditIfNotRequired();
			AddColumnArrangementControls();
			SetupTabControl();
			SetupShortcutMenuItems();
			AddErrors();
			additionalDisposableAction = BindDataSourceForLanguageSelectControlWithDisposable();
			HideLanguageControlIfNotSystemDefinedReport();
		}

		void SetupTabControl()
		{
			FilterTabControl.MakeTabControlFitGroups();
			FilterTabControl.MakeVisibleIfGroupsContainControls();
			FilterTabControl.Visible = FilterTabControl.NeedsToShow;
			NeedsToShow = FilterTabControl.NeedsToShow;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Env.Security.PreviewReportButton.IsAllowed)
			{
				PreviewButton.Visible = true;
			}
			if (!Env.Security.AllowReportTimeOutOverride.IsAllowed || isErrorDisplayForm)
			{
				TimeOutEdit.Visible = false;
			}
			if (!isShowingReportOptions || !(fReport?.ErrorManager.HasWarningsOnly ?? true))
			{
				PreviewButton.Visible = false;
			}

			var reportsSupportedEDW = SystemDataRegistry.Instance.ListOfSupportedReportsUsingEdwAsDataSource.Value;
			if (fReport == null ||
				fReport.MenuItem == null ||
				!reportsSupportedEDW.Cast<SupportEdwDataSourceReport>().Any(x => x.ReportName == fReport.MenuItem.SU_MenuName && x.BusinessContext == fReport.MenuItem.SU_BusinessContext) ||
				!string.IsNullOrEmpty(BiServiceTaskHelpers.IsEdwEnabled()))
			{
				EdwDataSourceCheckBox.Visible = false;
			} 
		}

		#endregion

		#region Schedule Task

		public readonly ReportScheduleTask ScheduleTask;

		#endregion

		#region Filter Controls

		public bool NeedsToShow
		{
			get { return fNeedsToShow; }
			set
			{
				if (!fNeedsToShow)
				{
					fNeedsToShow = value;
				}
			}
		}

		bool fNeedsToShow;

		void AddFilterControls()
		{
			AddFilterGroupControls();
			foreach (IFilter filter in fReport.FilterCollection)
			{
				FilterField nextFilterField = filter as FilterField;

				if (nextFilterField != null)
				{
					if (!string.IsNullOrEmpty(nextFilterField.Language))
					{
						using (Res.TemporarilySwitchLanguage(nextFilterField.Language))
						{
							NewRuntimeOptionUserControl(nextFilterField);
						}
					}
					else
					{
						NewRuntimeOptionUserControl(nextFilterField);
					}
				}
			}

			FilterTabControl.MakeTabControlFitGroups();
			FilterTabControl.MakeVisibleIfGroupsContainControls();
			FilterTabControl.Visible = FilterTabControl.NeedsToShow;
			NeedsToShow = FilterTabControl.NeedsToShow;
		}

		void NewRuntimeOptionUserControl(FilterField filterField)
		{
			var runtimeOptionUserControl = RuntimeOptionUserControlFactory.New(filterField);
			if (runtimeOptionUserControl != null)
			{
				runtimeOptionUserControl.SetFilter(filterField);
				FilterTabControl.AddControlToFilterGroup(filterField.GroupName, runtimeOptionUserControl);
			}
		}

		#endregion

		#region Filter Group Controls

		void AddFilterGroupControls()
		{
			foreach (CodeDescriptionPair groupNameAndDescription in fReport.FilterCollection.FilterGroups)
			{
				FilterTabControl.AddPageAndGroupBox(groupNameAndDescription.Code, groupNameAndDescription.Description);
			}
		}

		#endregion

		#region Optional Template Controls

		void AddOptionalTemplateControls()
		{
			if (fReport.OptionalTemplateSheetCollection.Count == 0)
			{
				OptionalTemplatesGroupBox.Visible = false;
				return;
			}

			var maxWidth = 0;
			foreach (OptionalTemplateSheet optionalSheet in fReport.OptionalTemplateSheetCollection)
			{
				maxWidth = Math.Max(maxWidth, (int)Math.Ceiling((double)TextRenderer.MeasureText(optionalSheet.DisplayNameLocalized, OptionalTemplatesGroupBox.Font).Width));
			}
			maxWidth += ControlDpiScalingHelper.ScaleToCurrentDpiX(24);
			OptionalTemplatesGroupBox.ColumnWidth = Math.Max(OptionalTemplatesGroupBox.ColumnWidth, maxWidth);

			foreach (var optionalSheet in fReport.OptionalTemplateSheetCollection.ToList().Cast<OptionalTemplateSheet>().OrderBy(sheet => sheet.Order))
			{
				var checkBox = new ZCheckBox();
				checkBox.Text = optionalSheet.DisplayNameLocalized.Replace(" & ", " && ");
				checkBox.Name = optionalSheet.Name;
				ControlDpiScalingHelper.SetWidth(ref checkBox, maxWidth, false);
				OptionalTemplatesGroupBox.AddControl(checkBox);
				BindingSource.SetBindingMember(checkBox, "");
				DataBoundControl.Get(checkBox).SetDataBinding(optionalSheet, "Selected");
			}

			NeedsToShow = fReport.OptionalTemplateSheetCollection.Count > 0;
			OptionalTemplatesGroupBox.SetHeight();
			OptionalTemplatesGroupBox.Visible = fReport.OptionalTemplateSheetCollection.Count > 0;
		}

		#endregion

		#region Report Orientation Control
		void HideOrientationDropEditIfNotRequired()
		{
			if (!(fReport.Style == Report.Styles.Report && fReport.GetPageStyle() == PageStyles.Continuous))
			{
				LabelReportOrientation.Hide();
				DropEditReportOrientation.Hide();
				orientationIsHidden = true;
			}
		}

		bool orientationIsHidden;
		#endregion

		#region Group By Controls

		void AddGroupByControls()
		{
			if (fReport.GroupByCollection.Count == 0)
			{
				GroupByGroupBox.Visible = false;
				return;
			}

			int maxWidthScaled = 0;
			foreach (GroupBy groupBy in fReport.GroupByCollection)
			{
				maxWidthScaled = Math.Max(maxWidthScaled, (int)Math.Ceiling((double)TextRenderer.MeasureText(groupBy.DisplayNameLocalized, GroupByGroupBox.Font).Width));
			}
			if (fReport.GetPageStyle() == PageStyles.Continuous)
			{
				maxWidthScaled = Math.Max(maxWidthScaled, (int)Math.Ceiling((double)TextRenderer.MeasureText(Res.GetString("b136c9c2-d4a2-4dc5-be78-e954d80d7de9", "Page Break on New group"), OptionalTemplatesGroupBox.Font).Width));
			}
			maxWidthScaled += ControlDpiScalingHelper.ScaleToCurrentDpiX(24);
			GroupByGroupBox.ColumnWidth = Math.Max(GroupByGroupBox.ColumnWidth, maxWidthScaled);

			foreach (GroupBy groupBy in fReport.GroupByCollection)
			{
				ZRadioButton optionRadioButton = new ZRadioButton();
				if (groupBy.DisplayNameLocalizedData != null)
				{
					optionRadioButton.CaptionResourceString = groupBy.DisplayNameLocalizedData;
				}
				else
				{
					optionRadioButton.Text = groupBy.DisplayName;
				}
				ControlDpiScalingHelper.SetWidth(ref optionRadioButton, maxWidthScaled, false);
				optionRadioButton.DataBindings.Add(new KBinding("Checked", groupBy, "Selected"));
				GroupByGroupBox.AddControl(optionRadioButton);
			}
			NeedsToShow = fReport.GroupByCollection.Count > 1;
			if (fReport.GetPageStyle() == PageStyles.Continuous)
			{
				ZCheckBox pageBreakCheckBox = new ZCheckBox();
				pageBreakCheckBox.Name = "PageBreakCheckBox";
				pageBreakCheckBox.CaptionResourceString = Res.GetData("b136c9c2-d4a2-4dc5-be78-e954d80d7de9", "Page Break on New group");
				ControlDpiScalingHelper.SetWidth(ref pageBreakCheckBox, maxWidthScaled, false);

				DataBoundControl.Get(pageBreakCheckBox).SetDataBinding(fReport, "GroupByCollection+BreakPageOverride");

				GroupByGroupBox.AddControl(pageBreakCheckBox);
			}
			GroupByGroupBox.SetHeight();
		}

		#endregion

		#region Column Arrangement Controls

		void AddColumnArrangementControls()
		{
			if (fReport.Style == Report.Styles.Report && (!fReport.ColumnHeadingManager.IsEmpty || fReport.FilterCollection.Count > 0 || !orientationIsHidden))
			{
				ColumnArrangementGroupBox = FilterTabControl.AddPageAndGroupBox(ResString.GetMultilingualString("1c4215f4-7f7b-4b5a-a6a4-c1178a6488e8", "Configuration Management"), Res.GetString("55630904-aacf-408d-ad70-acdc49c4c489", "Please set the report configuration settings"));
				ColumnArrangementGroupBox.AddControl(new ColumnArrangementUserControl(fReport));
			}
		}

		#endregion

		#region Sort Order Controls

		void AddSortOrderControls()
		{
			if (fReport.SortOrderCollection.Count == 0)
			{
				SortOrderGroupBox.Visible = false;
				return;
			}

			var maxWidth = 0;
			foreach (DocumentEngine.RuntimeOptions.SortOrder sort in fReport.SortOrderCollection)
			{
				maxWidth = Math.Max(maxWidth, (int)Math.Ceiling((double)TextRenderer.MeasureText(sort.DisplayNameLocalized, SortOrderGroupBox.Font).Width));
			}
			maxWidth += ControlDpiScalingHelper.ScaleToCurrentDpiX(24);
			SortOrderGroupBox.ColumnWidth = Math.Max(SortOrderGroupBox.ColumnWidth, maxWidth);

			foreach (DocumentEngine.RuntimeOptions.SortOrder sort in fReport.SortOrderCollection)
			{
				var optionRadioButton = new ZRadioButton();
				if (sort.DisplayNameLocalizedData != null)
				{
					optionRadioButton.CaptionResourceString = sort.DisplayNameLocalizedData;
				}
				else
				{
					optionRadioButton.Text = sort.DisplayName;
				}

				ControlDpiScalingHelper.SetWidth(ref optionRadioButton, maxWidth, false);
				optionRadioButton.DataBindings.Add(new KBinding("Checked", sort, "Selected"));
				SortOrderGroupBox.AddControl(optionRadioButton);
			}
			NeedsToShow = fReport.SortOrderCollection.Count > 1;
			SortOrderGroupBox.SetHeight();
		}

		#endregion

		#region Language Control

		IDisposable BindDataSourceForLanguageSelectControlWithDisposable()
		{
			IDisposable disposableAction = null;

			if (!isErrorDisplayForm)
			{
				LanguageControlBindingSource.SetBindingMember(LanguageZDropEdit, "Language");
				((DeliveryInstructions)LanguageControlBindingSource.DataSource).LanguageInfo.ValueChanged += LanguageInfo_ValueChanged;
				disposableAction = new DisposableAction(() => ((DeliveryInstructions)LanguageControlBindingSource.DataSource).LanguageInfo.ValueChanged -= LanguageInfo_ValueChanged);
			}
			MissingResourceStringChecker.ExcludeFromTest(LanguageZDropEdit);

			return disposableAction;
		}

		void LanguageInfo_ValueChanged(object sender, EventArgs e)
		{
			if (shouldSuspendResetReportHeadingTextFirstTime)
			{
				shouldSuspendResetReportHeadingTextFirstTime = false;
				using (fReport.Parent.SuspendResetingReportHeadingText())
				{
					fReport.Parent.Language = ((DeliveryInstructions)LanguageControlBindingSource.DataSource).Language;
				}
				return;
			}
			fReport.Parent.Language = ((DeliveryInstructions)LanguageControlBindingSource.DataSource).Language;
		}

		KBindingSource languageControlBindingSource;

		KBindingSource LanguageControlBindingSource
		{
			get
			{
				if (languageControlBindingSource == null)
				{
					languageControlBindingSource = new KBindingSource();
					languageControlBindingSource.DataSource = fInstructions ?? new DeliveryInstructions();
				}
				return languageControlBindingSource;
			}
		}

		void headingManager_ColumnHeadingManagerLoaded(object sender, EventArgs e)
		{
			SetLanguage(fReport.Language);
		}

		void SetLanguage(string language)
		{
			if (!isErrorDisplayForm)
			{
				((DeliveryInstructions)LanguageControlBindingSource.DataSource).Language = language;
			}
		}

		void HideLanguageControlIfNotSystemDefinedReport()
		{
			if (fReport.ContainsAnyCustomisation)
			{
				LabelReportLanguage.Visible = false;
				LanguageZDropEdit.Visible = false;
			}
		}

		#endregion

		#region Errors

		/// <summary>
		/// DataGrid needs to have a concrete type for the collection it binds to or the ColumnStyles seem to be ignored.
		/// </summary>
		public class ReportProcessingErrorList : NonPersistentBusinessObjectCollection<ReportProcessingError>
		{
			public ReportProcessingErrorList(IHaveReportProcessingErrorsForGUI errorManager)
			{
				foreach (IReportProcessingError error in errorManager.GetErrors())
				{
					Add(new ReportProcessingError(error));
				}
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new ReportProcessingError();
			}
		}

		public class ReportProcessingError : NonPersistentBusinessObject
		{
			public ReportProcessingError()
			{
			}

			public ReportProcessingError(IReportProcessingError parent)
			{
				this.Message = parent.Message;
				this.TemplatePath = parent.TemplatePath;
				this.SheetName = parent.SheetName;
				this.CellReference = parent.CellName;
				this.Severity = parent.Severity.ToStringFormatted();
				this.OuterContent = parent.OuterContent;
				this.Count = parent.Occurrences.ToString();
			}

			public string Severity { get; private set; }
			public string CellReference { get; private set; }
			public string Message { get; private set; }
			public string SheetName { get; private set; }
			public string TemplatePath { get; private set; }
			public string OuterContent { get; private set; }
			public string Count { get; private set; }
		}

		public class ReportProcessingErrorToBind : NonPersistentBusinessObject
		{
			public ReportProcessingErrorToBind(ReportProcessingErrorList errors)
			{
				_errors = errors;
			}

			readonly ReportProcessingErrorList _errors;

			public ReportProcessingErrorList Errors
			{
				get
				{
					return _errors;
				}
			}
		}

		void AddErrors()
		{
			ReportProcessingErrorList templateErrors = new ReportProcessingErrorList(fReport.ErrorManager);

			if (templateErrors.Count == 0)
			{
				ErrorsGroupBox.Visible = false;
				return;
			}

			var severity = new ZTextBoxColumnStyleInfo();
			severity.ColumnName = "Severity";
			severity.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("87276cb3-1ffd-41e1-ac4b-22b4b151f748", "Severity");
			severity.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.ErrorsGridView.ColumnStyles.Add(severity);

			var message = new ZTextBoxColumnStyleInfo();
			message.ColumnName = "Message";
			message.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("11a5bd1a-514c-4190-b75a-4cdc06b7d607", "Description of error");
			message.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(430);
			this.ErrorsGridView.ColumnStyles.Add(message);

			var occurrences = new ZTextBoxColumnStyleInfo();
			occurrences.ColumnName = "Count";
			occurrences.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("91c2833e-a744-41b2-a1ee-96143b69b1df", "Count");
			occurrences.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.ErrorsGridView.ColumnStyles.Add(occurrences);

			var outerContent = new ZTextBoxColumnStyleInfo();
			outerContent.ColumnName = "OuterContent";
			outerContent.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3d59e22e-54e6-4d04-92b0-92478ffb4b4c", "Cell Content");
			outerContent.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ErrorsGridView.ColumnStyles.Add(outerContent);

			var cell = new ZTextBoxColumnStyleInfo();
			cell.ColumnName = "CellReference";
			cell.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3e219ae0-3deb-4b72-bd0d-b4e7b2a7592c", "1st Cell");
			cell.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ErrorsGridView.ColumnStyles.Add(cell);

			var sheet = new ZTextBoxColumnStyleInfo();
			sheet.ColumnName = "SheetName";
			sheet.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("1fbc86a4-abb4-410c-bf81-3c65c0dd8e97", "Sheet");
			sheet.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ErrorsGridView.ColumnStyles.Add(sheet);

			var sourceFile = new ZTextBoxColumnStyleInfo();
			sourceFile.ColumnName = "TemplatePath";
			sourceFile.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("a02afb37-183d-403c-9ee7-5bcda4af4a56", "Source File");
			sourceFile.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.ErrorsGridView.ColumnStyles.Add(sourceFile);

			var errors = new ReportProcessingErrorToBind(templateErrors);
			this.ErrorsGridView.SetDataBinding(errors, "Errors");

			CopyButton.Visible = true;

			if (!fReport.ErrorManager.HasWarningsOnly)
			{
				errorLabel.Text = Res.GetString("3b58d1e1-f14b-4c0e-ae61-7afda689780b", @"ERRORS WERE FOUND during the generation of this template that either substantially affect the layout of this document, or make it impossible to produce any meaningful result. 

THIS DOCUMENT CANNOT BE PRODUCED, click 'Cancel' to exit.");
				ActionButton.Enabled = false;
				ActionButton.Visible = false;
				PreviewButton.Enabled = false;
				PreviewButton.Visible = false;
				ControlDpiScalingHelper.SetLeft(ref CopyButton, ActionButton.Right - CopyButton.Width, false);
			}
			else
			{
				errorLabel.Text = Res.GetString("7fe4295e-2792-4638-bb26-0fcd93ea224a", @"ERRORS WERE FOUND during the generation of this template, but the document is still able to be produced. Some fields may be missing data, but the general layout of the document will probably be OK. 

If you want to produce the document 'As is', click 'Continue' or click 'Cancel' to exit.");
				ActionButton.Text = Res.GetString("690d67d1-c227-4a49-92df-7cf9d15bb5aa", "Continue");
				ActionButton.Enabled = true;
				ControlDpiScalingHelper.SetLeft(ref CopyButton, PreviewButton.Right - CopyButton.Width, false);
			}

			var template = fReport.StTemplate;
			if (template != null && template.IsDocBuilderStyle)
			{
				var customDocBuilderTemplates = template.Factory.Load<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.User));
				if (customDocBuilderTemplates.Length > 0)
				{
					errorLabel.Text += System.Environment.NewLine;
					errorLabel.Text += System.Environment.NewLine;
					errorLabel.Text += Res.GetString("7dca93d3-50bc-40b8-8720-d14d43271a3f", "WARNING: Customized Document Elements used, please check any custom strips used in this document.");
				}
			}

			CreateShortcutLinkLabel.Visible = false;
			NeedsToShow = true;

			if (!fReport.ErrorManager.HasWarningsOnly || !isShowingReportOptions)
			{
				FilterTabControl.Visible = false;
				SortOrderGroupBox.Visible = false;
				GroupByGroupBox.Visible = false;
				UserDefinedFieldsGroupBox.Visible = false;
				OptionalTemplatesGroupBox.Visible = false;
				LabelReportOrientation.Visible = false;
				DropEditReportOrientation.Visible = false;
				LabelReportLanguage.Visible = false;
				LanguageZDropEdit.Visible = false;
				EdwDataSourceCheckBox.Visible = false;
			}
		}

		public string ErrorLabel
		{
			get { return errorLabel.Text; }
		}

		#endregion

		#region Layout Calculation

		Size CalculateDesiredSizes()
		{
			int errorHeight = ErrorsGroupBox.Visible ? ErrorsGroupBox.Height : 0;
			int filterHeight = FilterTabControl.Visible ? FilterTabControl.Height : 0;
			int sortOrderHeight = SortOrderGroupBox.Visible ? SortOrderGroupBox.Height : 0;
			int groupByGroupBoxHeight = GroupByGroupBox.Visible ? GroupByGroupBox.Height : 0;
			int userDefinedFieldsHeight = UserDefinedFieldsGroupBox.Visible ? UserDefinedFieldsGroupBox.Height : 0;
			int optionalTemplateHeight = OptionalTemplatesGroupBox.Visible ? OptionalTemplatesGroupBox.Height : 0;

			int rightmostEdge = GroupByGroupBox.DesiredWidth;
			rightmostEdge = Math.Max(SortOrderGroupBox.DesiredWidth, rightmostEdge);
			rightmostEdge = Math.Max(UserDefinedFieldsGroupBox.DesiredWidth, rightmostEdge);
			rightmostEdge = Math.Max(FilterTabControl.DesiredWidth + TabControlPaddingFudge + (FilterTabControl.AreGroupsScrolling ? SystemInformation.VerticalScrollBarWidth : 0), rightmostEdge);
			rightmostEdge = Math.Max(OptionalTemplatesGroupBox.DesiredWidth, rightmostEdge);
			if (ErrorsGridView.Visible)
			{
				rightmostEdge = Math.Max(ErrorsGridView.Right, rightmostEdge);
			}
			int desiredWidth = DockPadding.Left + rightmostEdge + DockPadding.Right;
			int desiredHeight = DockPadding.Top + errorHeight + filterHeight + sortOrderHeight + groupByGroupBoxHeight + userDefinedFieldsHeight + optionalTemplateHeight + FooterPanel.Height + DockPadding.Bottom;

			return ControlDpiScalingHelper.NewScaledSize(desiredWidth, desiredHeight + MainStatusBar.Height, false);
		}

		[DpiState(DpiState.ScaleX)]
		internal readonly int TabControlPaddingFudge = ControlDpiScalingHelper.MarkAsScaled(24); //Tab pages are automatically placed at left = 4 with a width of parent.width-8. 

		void MakeGroupBoxesHaveTwoColumns()
		{
			FilterTabControl.RearrangeGroupBoxesInNColumns(2);
			GroupByGroupBox.RearrangeInNColumns(2);
			SortOrderGroupBox.RearrangeInNColumns(2);
			UserDefinedFieldsGroupBox.RearrangeInNColumns(2);
			OptionalTemplatesGroupBox.RearrangeInNColumns(2);
		}

		void PositionGroupBoxesVertically()
		{
			int nextGroupTop = DockPadding.Top;
			ControlDpiScalingHelper.SetTop(ref ErrorsGroupBox, nextGroupTop, false);
			nextGroupTop += ErrorsGroupBox.Visible ? ErrorsGroupBox.Height : 0;
			ControlDpiScalingHelper.SetTop(ref FilterTabControl, nextGroupTop, false);
			nextGroupTop += FilterTabControl.Visible ? FilterTabControl.Height : 0;
			ControlDpiScalingHelper.SetTop(ref SortOrderGroupBox, nextGroupTop, false);
			nextGroupTop += SortOrderGroupBox.Visible ? SortOrderGroupBox.Height : 0;
			ControlDpiScalingHelper.SetTop(ref GroupByGroupBox, nextGroupTop, false);
			nextGroupTop += GroupByGroupBox.Visible ? GroupByGroupBox.Height : 0;
			ControlDpiScalingHelper.SetTop(ref UserDefinedFieldsGroupBox, nextGroupTop, false);
			nextGroupTop += UserDefinedFieldsGroupBox.Visible ? UserDefinedFieldsGroupBox.Height : 0;
			ControlDpiScalingHelper.SetTop(ref OptionalTemplatesGroupBox, nextGroupTop, false);
			nextGroupTop += OptionalTemplatesGroupBox.Visible ? OptionalTemplatesGroupBox.Height : 0;
			ControlDpiScalingHelper.SetTop(ref FooterPanel, nextGroupTop, false);
			nextGroupTop += FooterPanel.Height;
			ControlDpiScalingHelper.SetTop(ref MainStatusBar, nextGroupTop, false);
		}

		#endregion

		#region Creating Shortcuts

		void SetupShortcutMenuItems()
		{
			if (fReport.Parent == null || fReport.Parent.StmMenuCommand == null)
			{
				CreateShortcutLinkLabel.Dispose();
			}

			CreateShortcutLinkLabel.LinkClicked += delegate
			{
				CreateShortcutContextMenu.Show(CreateShortcutLinkLabel, CreateShortcutLinkLabel.Width, CreateShortcutLinkLabel.Height);
			};
			CreateHyperlinkMenuItem.Click += delegate
			{
				GetShortcutCreator().CopyHyperlinkToClipboard(fReport.Parent.StmMenuCommand.SU_MenuNameMultilingual, ReportUrlHandler.Instance.Create(fReport));
			};
			CreateDesktopShortcutMenuItem.Click += delegate
			{
				GetShortcutCreator().CreateDesktopShortcut(fReport.Parent.StmMenuCommand.SU_MenuNameMultilingual, ReportUrlHandler.Instance.Create(fReport));
			};
		}

		protected virtual ShortcutCreator GetShortcutCreator()
		{
			return new ShortcutCreator();
		}

		#endregion

		#region Event Handlers

		void CopyButton_Click(object sender, EventArgs e)
		{
			if (fReport != null && fReport.ErrorManager != null)
			{
				if (!SafeClipboard.SetText(fReport.ErrorManager.ToString("Severity:\t[{0}] \r\n" +
"Description:\t[{1}] \r\n" +
"Occurrences:\t[{6}] \r\n" +
"Cell Content:\t[{5}] \r\n" +
"First Cell:\t[{2}]\tWorkSheet:\t[{3}] \r\n" +
"Source File:\t[{4}] \r\n\r\n", false).Trim()))
				{
					Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
				}
			}
		}

		internal void PreviewButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
				return;
			}

			CalculateOverrideReportDbOption();

			var previewReportStatisticsMap = new Dictionary<Report, Tuple<StmReportRun, ReportStatistics>>();

			try
			{
				var deliveryInstructions = fInstructions.Clone() as DeliveryInstructions;
				if (deliveryInstructions.DeliverablesToBePrinted != null)
				{
					foreach (var deliveryInstruction in deliveryInstructions.DeliverablesToBePrinted)
					{
						var report = deliveryInstruction as Report;
						var stmReportRun = CreateStmReportRunForPreview(report);
						stmReportRun.RRI_ReportDescription = StmReportRun.PreviewTaskDescription;
						var reportStatistics = new ReportStatistics();
						printTask.PrepareReportStatisticsForPreview(report, stmReportRun, reportStatistics);
						previewReportStatisticsMap.Add(report, new Tuple<StmReportRun, ReportStatistics>(stmReportRun, reportStatistics));
					}
				}

				Preview(this, deliveryInstructions);
			}
			catch (DocumentEngineException ex)
			{
				Globals.Message.ShowError(
					Res.GetString("39C2856F-0173-48F9-8F06-451C0ACD1DE4",
						"There are errors in the report that cannot be previewed.{0}{1}", System.Environment.NewLine,
						ex.Message), Res.GetString("ECDF2A9F-A879-4E5E-802D-D83AC9E4A253", "Unable to preview report"));
			}
			finally
			{
				foreach (var entry in previewReportStatisticsMap)
				{
					var report = entry.Key;
					var tuple = entry.Value;
					tuple.Item1.RRI_EndTimeUtc = ZDateTime.UtcNow;
					printTask.RecordReportStatisticsForPreview(report, tuple.Item1, tuple.Item2);
				}
			}
		}

		StmReportRun CreateStmReportRunForPreview(Report report)
		{
			var stmReportRun = report.Factory.New<StmReportRun>();
			stmReportRun.RRI_ReportDescription = StmReportRun.PreviewTaskDescription;
			stmReportRun.RRI_StartTimeInQueueUtc = ZDateTime.UtcNow;
			stmReportRun.RRI_EndTimeInQueueUtc = ZDateTime.UtcNow;
			stmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;
			stmReportRun.RRI_GS_NKPrintUser = GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
			stmReportRun.RRI_Status = Core.Constants.StmReportRunState.Running;
			stmReportRun.RRI_RunningServer = System.Environment.MachineName;
			stmReportRun.RRI_ReportName = report.Name;
			stmReportRun.RRI_IsSystemDefined = report.MenuItem.SU_IsSystemDefined;

			return stmReportRun;
		}

		protected virtual void Preview(IDeliverCapableForm parentForm, DeliveryInstructions deliveryInstructions)
		{
			printTask.Preview(parentForm, deliveryInstructions);
		}

		internal void SubmitButton_Click(object sender, EventArgs e)
		{
			string columnsWithPerformanceWarnings = string.Empty;
			foreach (Worksheet worksheet in BusinessEntity.ColumnHeadingManager.CurrentConfiguration.Worksheets)
			{
				foreach (ColumnHeading columnHeading in worksheet.ColumnHeadings)
				{
					if (columnHeading.ShowPerformanceWarning && !columnHeading.Hidden)
					{
						columnsWithPerformanceWarnings += Res.GetString("d8f76531-3b08-4ba0-ac41-f9355c20dfff", "Worksheet {0}, column {1}", worksheet.Name, columnHeading.DisplayLabel) + "\r\n";
					}
				}
			}

			if (!string.IsNullOrEmpty(columnsWithPerformanceWarnings))
			{
				if (Globals.Message.Show(Res.GetString("cea29bd7-22d9-47ed-905a-72d37170d654", @"*** WARNING *** this report contains one or more columns (listed below) that are very high cost in terms of database performance. It is recommended that you consider:
 a)	If these columns are needed - if not please remove them
 b)	If you should run the report at a time that the system is not under production load 
 c)	Consider scheduling the report to run on the report scheduler out of production hours
 d)	Consider making a special version of this report for the key customers or managers that need this column and reducing the number of times this column is needed

List of Fields Considered high cost by the report engine: -

{0}
Do you want to continue?", columnsWithPerformanceWarnings), Res.GetString("48bf5ce0-b90d-4650-a985-b2458e5fff1f", "Performance Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}
			}

			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
				return;
			}

			CalculateOverrideReportDbOption();

			if (ScheduleTask != null)
			{
				try
				{
					ScheduleTask.S5_ScheduleState = ScheduleTask.Serialize(fReport);
				}
				catch (InvalidOperationException ex)
				{
					Globals.Message.Show(ex.Message, Res.GetString("fbf735cf-a99b-4af3-8f5e-a0bbca94ed26", "Validation Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
			}

			RequestDelivery();

			DialogResult = DialogResult.OK;
		}

		void ButtonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Delivery Event

		public event PrintTask.DeliveryRequestedEventHandler DeliveryRequested;

		public void RequestDelivery()
		{
			OnDeliveryRequested(fDeliveryOptions, fInstructions == null ? null : (DeliveryInstructions)fInstructions.Clone(), fModifyDocumentCheckPoint);
		}

		public void OnDeliveryRequested(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			if (DeliveryRequested != null)
			{
				DeliveryRequested(this, deliveryOptions, instructions, modifyDocumentCheckPoint);
			}
		}

		#endregion

		#region Dispose

		bool fIsFormClosed;

		bool loweredAdditionalFormsCount;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (additionalDisposableAction != null)
			{
				additionalDisposableAction.Dispose();
				additionalDisposableAction = null;
			}

			if (!loweredAdditionalFormsCount)
			{
				OpenedFormCache.GetInstance().AdditionalFormsCount -= 1;
				loweredAdditionalFormsCount = true;
			}

			IsFormClosed = true;
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			if (printTask != null)
			{
				DeliveryRequested -= printTask.Form_DeliveryRequested;
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region IDeliverCapableForm members

		public void Deliver(DeliveryInstructions instructions)
		{
			OnDeliveryRequested(fDeliveryOptions, instructions, fModifyDocumentCheckPoint);
		}

		public bool IsFormClosed
		{
			get { return fIsFormClosed; }
			set { fIsFormClosed = value; }
		}

		#endregion

		#region Test
#if DEBUG

		public AutoLayoutGroupBox ColumnArrangementGroupBox_Exposed => ColumnArrangementGroupBox;

		public Report ReportExposedForTesting
		{
			get { return fReport; }
		}

#endif
		#endregion
	}
}
