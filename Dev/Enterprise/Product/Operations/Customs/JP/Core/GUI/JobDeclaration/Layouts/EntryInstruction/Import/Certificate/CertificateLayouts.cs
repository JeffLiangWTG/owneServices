using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CertificateLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new CertificateLayoutBuilder();
				var importCertificateBag = builder.CommonBag;

				builder.AddColumn();
				builder.Add(importCertificateBag.OtherLawsAndRegulationsGroupBox, ControlWidthClass.LongNoCaption);
				builder.Add(importCertificateBag.CommonControlNumberTextBox, ControlWidthClass.Auto);
				builder.Add(importCertificateBag.FoodHygieneCertificateTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(importCertificateBag.PlantProtectionCertificateTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(importCertificateBag.AnimalQuarantineCertificateTypeDropEdit, ControlWidthClass.Auto);
				
				builder.AddColumn();
				builder.Add(importCertificateBag.ApprovalCertificateInfosGroupBox, ControlWidthClass.LongNoCaption);
				builder.Add(importCertificateBag.TradeControlOrderDropEdit, ControlWidthClass.Auto);
				builder.Add(importCertificateBag.CommercialValueTypeDropEdit, ControlWidthClass.Auto);

				return builder.Build();
			}
		}
	}
}
