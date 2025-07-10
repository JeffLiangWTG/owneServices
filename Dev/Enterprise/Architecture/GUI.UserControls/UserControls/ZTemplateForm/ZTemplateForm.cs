#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	using System;
	using System.Globalization;
	using System.Threading;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Windows.UI;
	using CargoWise.Windows.UI.Testing;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;

	[SuppressFormDesignerAnalysis]
	[TestExcludeZWinFormsAllHaveFormBashers]
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class ZTemplateForm : ZEditForm
	{
		public ZTemplateForm()
		{
			Initialise();
		}

		/// <summary>
		/// Pass in a BusinessObject / BusinessObjectCollection
		/// </summary>
		/// <param name="businessEntity"></param>
		protected ZTemplateForm(IBusiness businessEntity) : base(businessEntity)
		{
			Initialise();
		}

		void Initialise()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (Thread.CurrentThread.IsBackground && SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.Value)
				{
					ErrorReporter.ReportOnce("FormIsNotAllowedToRunInBackground", string.Format(CultureInfo.InvariantCulture, @"The form should not run in background.
Form type: {0}", GetType()));
				}

				if (SupportsEDocs)
				{
					PlugIns.Add(ControllerIDs.eDocsPlugIn);
				}

				if (ShowAuditTab)
				{
					PlugIns.Add(ControllerIDs.Audit);
				}

				if (!ShowNotesTab)
				{
					MainTabControl.Controls.Remove(NotesTabPage);
					NotesTabPage.Dispose();
				}
			}
		}

		/// <summary>
		/// Top level tab control for the form.
		/// </summary>
		protected ZTemplateTabControl MainTabControl;

		/// <summary>
		/// First tab page for the form. Shown when the form opens.
		/// </summary>
		protected ZTabPage MainTabPage;

		/// <summary>
		/// Override this and return false if your form does not support eDocs
		/// </summary>
		protected virtual bool SupportsEDocs
		{
			get { return true; }
		}

		/// <summary>
		/// Override this and return false if you do not want the Notes tab to be shown
		/// </summary>
		protected virtual bool ShowNotesTab
		{
			get { return true; }
		}

		/// <summary>
		/// Override this and return true if you want the Audit tab to be shown
		/// </summary>
		protected virtual bool ShowAuditTab
		{
			get { return false; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if ((!CaptionRenderingEnabled.HasValue || !CaptionRenderingEnabled.Value) && string.IsNullOrEmpty(MainTabPage.Text))
			{
				MainTabPage.Text = Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("ZTemplateForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Details");
				if (DesignModeFinder.IsDesigning)
				{
					((IVariableLengthCaptionRenderer)MainTabPage).IsCaptionOverridden = false;
				}
			}
		}

		protected override sealed ZTabControl TopLevelTabControl
		{
			get { return MainTabControl; }
		}

		protected override void InitializeFormCore()
		{
			base.InitializeFormCore();
			InitializeComponent();
		}

		public override IBusiness BusinessEntityForHasChanges => GetTopLevelBusinessObjectIfTemplateRecord(base.BusinessEntityForHasChanges);
		protected override IBusiness BusinessEntityForValidation => GetTopLevelBusinessObjectIfTemplateRecord(base.BusinessEntityForValidation);

		public static IBusiness GetTopLevelBusinessObjectIfTemplateRecord(IBusiness baseEntity)
		{
			if (baseEntity is ITemplateRecordProvider templateRecordProvider &&
				templateRecordProvider.IsTemplateRecord &&
				templateRecordProvider.TemplateRecord is ITemplateRecord templateRecord &&
				templateRecordProvider.TemplateRecord != null)
			{
				SetTemplateToTopLevelOfForm(templateRecordProvider, templateRecord);
				return templateRecord as IBusiness;
			}

			return baseEntity;
		}

		static void SetTemplateToTopLevelOfForm(ITemplateRecordProvider templateRecordProvider, ITemplateRecord templateRecord)
		{
			(templateRecordProvider as BusinessObject).IsTopLevel = false;
			(templateRecord as BusinessObject).IsTopLevel = true;
		}
	}
}
