using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CARMSOAStatementForm : ZTemplateForm, IPostingButtonsProvider
	{
		public CARMSOAStatementForm(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
			this.statementHeader = statementHeader;
			statementHeader.ReadOnly = true;

			WorkflowTabPage.Initialize(statementHeader);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		readonly CusStatementHeader statementHeader;

		public override string FormCaption => statementHeader.HumanReadableName;

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion
	}
}
