using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ComponentRelationshipForm : ZTemplateForm
	{
		ComponentRelationshipControl ComponentRelationshipControl;

		public ComponentRelationshipForm(ComponentRelationship component)
			: base(component)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => true;

		protected override bool ShowAuditTab => true;

		#region Fetch Hints

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var componentRelationship = dataSource as ComponentRelationship;

			componentRelationship?.AddFetchHints();
		}

		#endregion
	}
}
