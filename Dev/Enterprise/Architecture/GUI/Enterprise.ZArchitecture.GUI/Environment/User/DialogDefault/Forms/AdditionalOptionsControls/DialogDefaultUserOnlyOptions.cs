using System;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	internal partial class DialogDefaultUserOnlyOptions : DialogDefaultAdditionalOptions
	{
		[Obsolete("Use the constructor that takes a Context. This is for the designer only")]
		public DialogDefaultUserOnlyOptions()
		{
			InitializeComponent();
		}

		public DialogDefaultUserOnlyOptions(DialogDefaultContext context)
			: base(context)
		{
			InitializeComponent();
		}

		protected internal override ZCheckBox SaveForAllContextsCheckbox
		{
			get { return applyToAllContextsCheckbox; }
		}
	}
}
