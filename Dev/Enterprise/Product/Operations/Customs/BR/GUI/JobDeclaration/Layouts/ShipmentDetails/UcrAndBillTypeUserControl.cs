using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class UcrAndBillTypeUserControl : ZUserControl, IExtendedControl
	{
		public Control Host => JE_UCRTextBox;

		public IControlExtensionCollection Extensions => JE_UCRTextBox.Extensions;

		public UcrAndBillTypeUserControl()
		{
			InitializeComponent();
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return JE_UCRTextBox.CaptionResourceString; }
			set { JE_UCRTextBox.CaptionResourceString = value; }
		}
	}
}
