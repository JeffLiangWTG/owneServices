using System;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	partial class DialogDefaultAdditionalOptions : ZUserControl
	{
		readonly protected DialogDefaultContext context;

		#region Designer

		[Obsolete("Use the constructor that takes a Context. This is for the designer only")]
		public DialogDefaultAdditionalOptions()
		{
			InitializeComponent();
		}

		#endregion

		public DialogDefaultAdditionalOptions(DialogDefaultContext context)
		{
			this.context = context;
			Load += DialogDefaultAdditionalOptions_Load;

			InitializeComponent();
		}

		void DialogDefaultAdditionalOptions_Load(object sender, EventArgs e)
		{
			if (context.NullContextAllowed)
			{
				SaveForAllContextsCheckbox.CaptionResourceString = context.NullContextDescription;
			}
			else
			{
				SaveForAllContextsCheckbox.Visible = false;
			}
		}

		protected internal virtual ZCheckBox SaveForAllContextsCheckbox { get { throw new InvalidOperationException("Override in subclasses"); } }
	}
}
