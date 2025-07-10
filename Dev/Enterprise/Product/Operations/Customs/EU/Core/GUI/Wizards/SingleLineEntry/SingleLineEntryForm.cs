using System;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.SingleLineEntry
{
	public partial class SingleLineEntryForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SingleLineEntryForm() { }

		public SingleLineEntryForm(ISingleLineEntryManager manager)
			: base(manager.SingleLineEntry)
		{
			this.manager = Argument.NotNull(manager, "manager");
			InitializeTariffFindBox();
			zLabel1.Text = Res.GetString("Customs.EU|SingleLineEntryForm|Description", "Answer the questions below to create a single line entry. Other details will default from the shipment or header level where appropriate. All existing commercial invoices will be discarded.");
		}
		readonly ISingleLineEntryManager manager;

		void zButton1_Click(object sender, EventArgs e)
		{
			manager.Execute();
		}

		protected Universal.GUI.TariffFindBox tariffFindBox;

		void InitializeTariffFindBox()
		{
			tariffFindBox = new Universal.GUI.TariffFindBox();
			if (!this.IsDesignMode())
			{
				tariffFindBox.GetCountryCode = () => manager.Declaration.CountryCode;
				tariffFindBox.GetDataGrouping = () => manager.Declaration.GetDefaultDataGroupingCode(Customs.Business.DefaultDataGroupingType.Tariff);
			}
			tariffFindBox.TariffType = TariffFormatter.GetTariffType(manager.Declaration.IsExport);
			tariffFindBox.CaptionResourceString = Res.GetData("SingleLineEntryForm|7f42c629-29d1-40ea-967e-deb80366d337", "Tariff");
			tariffFindBox.Name = "TariffFindBox";
			tariffFindBox.PreBoundMaxLength = 15;
			tariffFindBox.ShowDescriptionBox = false;
			tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 10, isInStandardDpi: true);
			tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, isInStandardDpi: true);
			tariffFindBox.TabIndex = 0;
			BindingSource.SetBindingMember(tariffFindBox, "TariffNumber");
			DetailsGroupBox.Controls.Add(tariffFindBox);
		}
	}
}
