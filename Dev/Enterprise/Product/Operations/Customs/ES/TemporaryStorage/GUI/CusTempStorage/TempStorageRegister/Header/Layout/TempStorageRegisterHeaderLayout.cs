using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterHeaderLayout : IPanelLayoutProvider
	{
		public TempStorageRegisterHeaderLayout()
		{
			Layout = CreateTempStorageRegisterHeaderLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTempStorageRegisterHeaderLayout()
		{
			var builder = new TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.InternalReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DDTNumberUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ArrivalDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.PreviousReferenceTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PreviousReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PresentationDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
