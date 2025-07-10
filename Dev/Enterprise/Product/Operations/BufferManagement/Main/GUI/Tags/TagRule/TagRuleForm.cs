using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagRuleForm : ZTemplateForm
	{
		public TagRuleForm(TagRule businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		public override string FormCaption
		{
			get
			{
				var rule = (TagRule)BusinessEntity;
				if (rule != null)
				{
					return rule.HumanReadableName;
				}

				return base.FormCaption;
			}
		}
	}
}
