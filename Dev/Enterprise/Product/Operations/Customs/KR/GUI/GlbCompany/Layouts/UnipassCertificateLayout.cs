using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class UnipassCertificateLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new UnipassCertificateLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.UserIDTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.MailBoxTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SenderIDTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CertificatePasswordTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.StatusTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.StatusReasonTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CertificateFileTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CertificateLoaderUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
