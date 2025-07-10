using System;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ZTemplateTabPage : ZTabPage
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int ImageIndex
		{
			get { return base.ImageIndex; }
			set { base.ImageIndex = value; }
		}

		protected sealed override bool IsAutoSized
		{
			get { return true; }
		}

		protected override void OnAdded()
		{
			base.OnAdded();
			if (!(Parent is ZTemplateTabControl))
			{
				throw new InvalidOperationException("You can only add a " + GetType().Name + " to a " + nameof(ZTemplateTabControl));
			}
		}
	}
}
