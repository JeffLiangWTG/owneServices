using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CertificateControlBag : ControlBag
	{
		public static CertificateControlBag Instance => instance ??= new CertificateControlBag();

		[ThreadStatic]
		static CertificateControlBag instance;

		CertificateControlBag()
		{
			OtherLawsAndRegulationsGroupBox = RegisterControl(nameof(OtherLawsAndRegulationsGroupBox));
			CommonControlNumberTextBox = RegisterControl(nameof(CommonControlNumberTextBox));
			FoodHygieneCertificateTypeDropEdit = RegisterControl(nameof(FoodHygieneCertificateTypeDropEdit));
			PlantProtectionCertificateTypeDropEdit = RegisterControl(nameof(PlantProtectionCertificateTypeDropEdit));
			AnimalQuarantineCertificateTypeDropEdit = RegisterControl(nameof(AnimalQuarantineCertificateTypeDropEdit));
			ApprovalCertificateInfosGroupBox = RegisterControl(nameof(ApprovalCertificateInfosGroupBox));
			TradeControlOrderDropEdit = RegisterControl(nameof(TradeControlOrderDropEdit));
			CommercialValueTypeDropEdit = RegisterControl(nameof(CommercialValueTypeDropEdit));
		}

		protected override Control CreateTemplate() => new CertificateLayoutTemplate();

		public ControlReference OtherLawsAndRegulationsGroupBox { get; }
		public ControlReference CommonControlNumberTextBox { get; }
		public ControlReference FoodHygieneCertificateTypeDropEdit { get; }
		public ControlReference PlantProtectionCertificateTypeDropEdit { get; }
		public ControlReference AnimalQuarantineCertificateTypeDropEdit { get; }
		public ControlReference ApprovalCertificateInfosGroupBox { get; }
		public ControlReference TradeControlOrderDropEdit { get; }
		public ControlReference CommercialValueTypeDropEdit { get; }
	}
}
