using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterPremisesLayout : IPanelLayoutProvider
	{
		public TempStorageRegisterPremisesLayout()
		{
			Layout = CreateTempStorageRegisterPremisesLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTempStorageRegisterPremisesLayout()
		{
			var builder = new TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PremisesCodeTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PremisesDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.LocationDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
