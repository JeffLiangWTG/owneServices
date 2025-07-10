using System;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	internal partial class DialogDefaultFullOptions : DialogDefaultAdditionalOptions
	{
		[Obsolete("Use the constructor that takes a Context. This is for the designer only")]
		public DialogDefaultFullOptions()
		{
			InitializeComponent();
		}

		public DialogDefaultFullOptions(DialogDefaultContext context)
			: base(context)
		{
			InitializeComponent();
		}

		protected internal override ZCheckBox SaveForAllContextsCheckbox
		{
			get { return saveForAllContextsCheckbox; }
		}
	}
}
