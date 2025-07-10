using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal partial class NumberUserControl : RuntimeOptionUserControl
	{
		public NumberUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(CalcEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override Type ExpectedFilterType()
		{
			return typeof(NumberField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			CalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			CalcEdit.AllowNull = true;
			SetDataBinding(filter, "");
			CalcEdit.Decimals = ((NumberField)filter).DecimalPlaces;
			CalcEdit.ShowGroupSeparators = ((NumberField)filter).ShowGroupSeparators;
			CalcEdit.MaxLength = ((NumberField)filter).MaxValue.ToString(CultureInfo.InvariantCulture).Length;
		}
	}
}
