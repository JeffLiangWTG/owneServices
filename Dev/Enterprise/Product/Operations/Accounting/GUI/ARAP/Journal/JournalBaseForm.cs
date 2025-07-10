using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using JournalBase = Enterprise.Accounting.Business.ARAP.Journal.Journal;

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	/// <summary>
	/// Base partial class for AP & AR Journal forms.
	/// </summary>
	public partial class JournalBaseForm : AccountingZForm
	{
		public JournalBaseForm(JournalBase journal) : base(journal)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = journal.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			DisplayModeChanged += JournalBaseForm_DisplayModeChanged;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public JournalBaseForm()
		{
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		public override string FormCaption
		{
			get { return ((JournalBase)BusinessEntity).JournalDefaultDescription; }
		}

		void JournalBaseForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool ShowAuditTab
		{
			get { return true; }
		}
	}
}

