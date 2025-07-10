using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI
{
	public class XmlDataImporterForm : DataImporterForm, IXmlDataImporterForm
	{
		protected XmlDataImporterForm()
		{
			InitializeComponent();
		}

		public XmlDataImporterForm(string formCaption, BillingInterfaceName interfaceName)
			: base(formCaption, interfaceName)
		{
			InitializeComponent();
		}

		public XmlDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName)
		{
			InitializeComponent();
		}

		public XmlDataImporterForm(BillingInterfaceName interfaceName)
			: this(new DataImporterBusinessObject(new BusinessObjectFactory()), null, interfaceName)
		{
		}

		public new static XmlDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new XmlDataImporterForm(interfaceName);
		}

		public override string FormCaption
		{
			get { return Res.GetString("XmlDataImporterForm|42d09331-6c81-4b0b-ba9f-d8482a602b9c", "XML Data Importer"); }
		}

		protected override string ImportFileFilter
		{
			get { return Res.GetString("f223ea7c-e20f-4d28-ab82-c2362ee6364d", "XML Files (*.XML)|*.XML|DAT Files (*.DAT)|*.DAT"); }
		}

		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 496, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 26, true);
			// 
			// XmlDataImporterForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Name = "XmlDataImporterForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void IXmlDataImporterForm.ImportFromFile(ZString fileName) => ImportFromFile(fileName);

		ZDialogResult IXmlDataImporterForm.ShowDialog() => (ZDialogResult)ZFormModaliser.ShowDialogWithoutDispose(this);
	}
}
