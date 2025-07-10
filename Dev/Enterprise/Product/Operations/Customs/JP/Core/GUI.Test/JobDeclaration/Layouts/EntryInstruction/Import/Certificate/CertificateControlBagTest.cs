using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CertificateControlBag))]
	sealed class CertificateControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(CertificateControlBag.OtherLawsAndRegulationsGroupBox),
					nameof(CertificateControlBag.CommonControlNumberTextBox),
					nameof(CertificateControlBag.FoodHygieneCertificateTypeDropEdit),
					nameof(CertificateControlBag.PlantProtectionCertificateTypeDropEdit),
					nameof(CertificateControlBag.AnimalQuarantineCertificateTypeDropEdit),
					nameof(CertificateControlBag.ApprovalCertificateInfosGroupBox),
					nameof(CertificateControlBag.TradeControlOrderDropEdit),
					nameof(CertificateControlBag.CommercialValueTypeDropEdit),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => CertificateControlBag.Instance;
	}
}
