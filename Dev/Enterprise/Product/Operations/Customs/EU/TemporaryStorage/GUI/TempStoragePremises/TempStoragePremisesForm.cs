using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TempStoragePremisesForm : ZTemplateForm
	{
		public TempStoragePremisesForm()
		{
			InitializeComponent();
		}

		public TempStoragePremisesForm(CusTempStorageRegPremises tempStorageRegPremises)
			: base(tempStorageRegPremises)
		{
			InitializeComponent();
			SetMainDetailsLayout();
		}

		protected CusTempStorageRegPremises TempStorageRegPremises => (CusTempStorageRegPremises)BusinessEntity;

		public DynamicLayoutPanel MainDynamicLayoutPanel { get; private set; }

		ITempStoragePremisesLayoutProvider LayoutProvider => fLayoutProvider ??= TempStoragePremisesLayoutProviderHelper.GetLayoutProvider(TempStorageRegPremises);
		ITempStoragePremisesLayoutProvider fLayoutProvider;

		void SetMainDetailsLayout()
		{
			var layout = LayoutProvider?.GetTempStoragePremisesDetailsLayout();
			if (layout != null && MainDynamicLayoutPanel == null)
			{
				MainTabPage.Controls.RemoveAndDisposeAll();
				MainDynamicLayoutPanel = new DynamicLayoutPanel
				{
					Name = nameof(MainDynamicLayoutPanel),
					Dock = DockStyle.Fill,
					Padding = ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, isInStandardDpi: true)
			};
				MainTabPage.Controls.Add(MainDynamicLayoutPanel);
				MainDynamicLayoutPanel.UpdateLayout(layout);
			}
		}

		public override string FormCaption
		{
			get
			{
				var extraCaption = ZString.Empty;
				var tempStorageRegPremises = TempStorageRegPremises;
				if (!tempStorageRegPremises.SRP_Code.IsEmpty)
				{
					extraCaption =  string.Format("- {0}", tempStorageRegPremises.SRP_Code);
				}
				return Res.GetString("8D994F71-EA17-4112-80A3-A30EA8C22B81", "Temporary Storage Premises {0}", extraCaption);
			}
		}
	}
}
