using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed partial class AmountAndUnitControl : ZUserControl, IExtendedControl
{
	public AmountAndUnitControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
		SetDefaultValueForUnitOfMeasure();
	}

	[Browsable(true)]
	public string UnitOfMeasureText
	{
		get => unitOfMeasureText;
		set
		{
			var oldValue = unitOfMeasureText;
			if (oldValue != value)
			{
				unitOfMeasureText = value;
				UnitOfMeasureTextBox.Text = unitOfMeasureText;
			}
		}
	}

	[Browsable(true)]
	public override ResourceStringData CaptionResourceString
	{
		get => base.CaptionResourceString;
		set
		{
			base.CaptionResourceString = value;
			AmountCalcEdit.CaptionResourceString = value;
		}
	}

	public Control Host => this;

	[Browsable(false)]
	public IControlExtensionCollection Extensions { get; }

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, "");
		BindingSource.SetBindingMember(AmountCalcEdit, dataMember);
	}

	void SetDefaultValueForUnitOfMeasure()
	{
		UnitOfMeasureText = "KG";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Extensions.Dispose();
		}

		base.Dispose(disposing);
	}

	string unitOfMeasureText;
}
