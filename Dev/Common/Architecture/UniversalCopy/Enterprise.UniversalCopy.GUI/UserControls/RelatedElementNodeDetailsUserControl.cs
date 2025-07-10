using System;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.UniversalCopy.GUI
{
	partial class RelatedElementNodeDetailsUserControl : EntityNodeDetailsUserControl
	{
		protected RelatedElementNodeDetailsUserControl()
		{
			InitializeComponent();
		}

		public RelatedElementNodeDetailsUserControl(UniversalCopyManager copyManager)
			: base(copyManager)
		{
			InitializeComponent();
		}

		RelatedEntityCopyTemplateBizo RelatedEntityTemplateBizo
		{
			get { return (RelatedEntityCopyTemplateBizo)BindingSource.Current; }
		}

		protected override void HookDataSource()
		{
			base.HookDataSource();

			if (RelatedEntityTemplateBizo != null)
			{
				RelatedEntityTemplateBizo.CopyMethodInfo.ValueChanged += CopyMethodChanged;
				CopyMethodChanged(this, EventArgs.Empty);
			}
		}

		protected override void UnHookDataSource()
		{
			base.UnHookDataSource();

			if (RelatedEntityTemplateBizo != null)
			{
				RelatedEntityTemplateBizo.CopyMethodInfo.ValueChanged -= CopyMethodChanged;
			}
		}

		void CopyMethodChanged(object sender, EventArgs e)
		{
			if (RelatedEntityTemplateBizo.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy)
			{
				ShowPropertiesDetails();
			}
			else
			{
				HidePropertiesDetails();
			}
		}
	}
}
