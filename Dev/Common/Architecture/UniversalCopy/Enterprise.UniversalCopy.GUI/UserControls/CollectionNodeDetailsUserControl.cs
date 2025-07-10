using System;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.UniversalCopy.GUI
{
	partial class CollectionNodeDetailsUserControl : EntityNodeDetailsUserControl
	{
		protected CollectionNodeDetailsUserControl()
		{
			InitializeComponent();
		}

		public CollectionNodeDetailsUserControl(UniversalCopyManager copyManager, bool isSplitCollection = false)
			: base(copyManager)
		{
			InitializeComponent();
			zTextBox1.ReadOnly = !isSplitCollection;
			calcEditOrder.Visible = isSplitCollection;
		}

		CollectionCopyTemplateBizo CollectionTemplateBizo
		{
			get { return (CollectionCopyTemplateBizo)BindingSource.Current; }
		}

		protected override void HookDataSource()
		{
			base.HookDataSource();

			if (CollectionTemplateBizo != null)
			{
				CollectionTemplateBizo.CopyMethodInfo.ValueChanged += CopyMethodChanged;
				CopyMethodChanged(this, EventArgs.Empty);
			}
		}

		protected override void UnHookDataSource()
		{
			using (CollectionTemplateBizo?.SuspendSettingHasChanges())
			{
				base.UnHookDataSource();
			}

			if (CollectionTemplateBizo != null)
			{
				CollectionTemplateBizo.CopyMethodInfo.ValueChanged -= CopyMethodChanged;
			}
		}

		void CopyMethodChanged(object sender, EventArgs e)
		{
			if (CollectionTemplateBizo.CopyTemplateNode.CopyMethod != CollectionCopyMethod.None)
			{
				ShowPropertiesDetails();
			}
			else
			{
				HidePropertiesDetails();
			}

			if (CollectionTemplateBizo.CopyTemplateNode.CopyMethod == CollectionCopyMethod.Filter)
			{
				InitializeFilterControl();
			}
			else
			{
				ClearFilterControls();
			}
		}
	}
}
