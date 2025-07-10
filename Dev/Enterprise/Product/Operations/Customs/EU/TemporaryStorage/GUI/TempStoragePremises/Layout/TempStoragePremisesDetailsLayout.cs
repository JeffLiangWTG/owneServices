using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TempStoragePremisesDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout TempStorageRegPremises { get; }

		PanelLayout IPanelLayoutProvider.Layout => TempStorageRegPremises;

		public TempStoragePremisesDetailsLayout()
		{
			TempStorageRegPremises = CreateTempStorageRegPremisesLayout();
		}

		PanelLayout CreateTempStorageRegPremisesLayout()
		{
			var builder = new TempStoragePremisesDetailsLayoutBuilder<CusTempStorageRegPremises>();

			var euBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(euBag.CodeTextBox, ControlWidthClass.Long);
			builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Long);
			builder.Add(euBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
			builder.Add(euBag.PremisesAddressAddressControl, ControlWidthClass.Long);
			builder.Add(euBag.CustomsLocationCodeFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
