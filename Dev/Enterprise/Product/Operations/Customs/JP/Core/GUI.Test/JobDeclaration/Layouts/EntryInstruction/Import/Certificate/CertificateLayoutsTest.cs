using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CertificateLayouts))]
	sealed class CertificateLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CertificateControlBag.Instance.OtherLawsAndRegulationsGroupBox, ControlWidthClass.LongNoCaption);
				yield return (CertificateControlBag.Instance.CommonControlNumberTextBox, ControlWidthClass.Auto);
				yield return (CertificateControlBag.Instance.FoodHygieneCertificateTypeDropEdit, ControlWidthClass.Auto);
				yield return (CertificateControlBag.Instance.PlantProtectionCertificateTypeDropEdit, ControlWidthClass.Auto);
				yield return (CertificateControlBag.Instance.AnimalQuarantineCertificateTypeDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CertificateControlBag.Instance.ApprovalCertificateInfosGroupBox, ControlWidthClass.LongNoCaption);
				yield return (CertificateControlBag.Instance.TradeControlOrderDropEdit, ControlWidthClass.Auto);
				yield return (CertificateControlBag.Instance.CommercialValueTypeDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CertificateLayoutBuilder();
	}
}
