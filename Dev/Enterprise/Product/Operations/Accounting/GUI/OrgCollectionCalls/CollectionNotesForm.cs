using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls
{
	public partial class CollectionNotesForm : ZForm
	{
		public CollectionNotesForm(OrgHeader header)
			: base(header)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);

			DisableNewAction();
			PlugIns.Add(ControllerIDs.OrgCollectionCalls);
			PlugIns.Add(ControllerIDs.ARAccQueryClaim);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		OrgHeader Header
		{
			get { return (OrgHeader)BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				var orgInfo = Header.OH_Code + (Header.OH_Code != "" ? " / " : "") + Header.OH_FullName;
				return Res.GetString("Accounting|CollectionNotesFormCaptionSuffix", "{0} Collection Notes", orgInfo);
			}
		}

		protected override ZTabControl TopLevelTabControl
		{
			get
			{
				return DebtorSelectionTab;
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

