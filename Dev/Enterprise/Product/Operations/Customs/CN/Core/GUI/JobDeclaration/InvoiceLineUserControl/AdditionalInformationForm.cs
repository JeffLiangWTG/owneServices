using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class AdditionalInformationForm : ZChildForm
	{
		public AdditionalInformationForm(AdditionalInformationWrapper additionalInformation) : base(additionalInformation)
		{
			InitializeComponent();

			AdditionalInformationGrid.FontDeciding += AdditionalInformationGrid_FontDeciding;
		}

		void AdditionalInformationGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (!(e.ObjectAtRow as AdditionalElementWrapper)?.IsRequired ?? false)
			{
				e.Font = new System.Drawing.Font(e.OriginalFont, System.Drawing.FontStyle.Strikeout);
			}
		}

		public new AdditionalInformationWrapper BusinessEntity => base.BusinessEntity as AdditionalInformationWrapper;

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog(false);
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public override string FormHeading => Res.GetString("3677836e-7970-4faf-9868-85d7b5dd9dfb", "Additional Information");

		public static void ShowDialog(IAdditionalInformationWrapperParent parent, ZPropertyInfo nameOfGoodsInfo, ZPropertyInfo goodsSpecModelInfo, EnteringOrExiting isEnteringOrExiting)
		{
			var tariff = parent?.UniversalTariff;
			if (tariff != null)
			{
				var wrapper = new AdditionalInformationWrapper(parent, nameOfGoodsInfo.Value.ToString(), goodsSpecModelInfo.Value.ToString(), isEnteringOrExiting);
				if (ZFormModaliser.ShowDialogAndDispose(new AdditionalInformationForm(wrapper)) == DialogResult.OK)
				{
					nameOfGoodsInfo.Value = wrapper.NameOfGoods.Left(nameOfGoodsInfo.MaxLength);
					goodsSpecModelInfo.Value = wrapper.GoodsSpecModel.Left(goodsSpecModelInfo.MaxLength);

					parent.AdditionalInformationCodes?.CleanByTariff(tariff, isEnteringOrExiting);
				}
			}
		}
	}
}
