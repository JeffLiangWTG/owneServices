using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(COOandFTAControlBag))]
	sealed class COOandFTAControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(COOandFTAControlBag.Instance.GoodsOriginCodeFindBox);
				yield return nameof(COOandFTAControlBag.Instance.CODeterminationRuleDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.COLabelLocationDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.COLabelTypeDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.COLabelExemptionReasonDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.CoveredByCOOExporterSystemCheckBox);

				yield return nameof(COOandFTAControlBag.Instance.COIssuingCountryCodeFindBox);
				yield return nameof(COOandFTAControlBag.Instance.COIssueDateEdit);
				yield return nameof(COOandFTAControlBag.Instance.COReferenceNumberTextBox);
				yield return nameof(COOandFTAControlBag.Instance.COCodeDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.IssuingAgencyNameTextBox);
				yield return nameof(COOandFTAControlBag.Instance.IssuingAreaNameTextBox);
				yield return nameof(COOandFTAControlBag.Instance.IssuingPersonNameTextBox);
				yield return nameof(COOandFTAControlBag.Instance.COSplitYNDropEdit);

				yield return nameof(COOandFTAControlBag.Instance.ProductTypeDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.CountryInvIssuedDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.CountryCodeFindBox);
				yield return nameof(COOandFTAControlBag.Instance.ExporterNumberTextBox);
				yield return nameof(COOandFTAControlBag.Instance.SplitOrderCalcEdit);
				yield return nameof(COOandFTAControlBag.Instance.SupportingDocTypeDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.IssuerTypeDropEdit);
				yield return nameof(COOandFTAControlBag.Instance.TotalNetWeightCalcEdit);
				yield return nameof(COOandFTAControlBag.Instance.UQDropEdit);

				yield return nameof(COOandFTAControlBag.Instance.SequenceNoCalcEdit);
				yield return nameof(COOandFTAControlBag.Instance.UsedQuantityCalcEdit);
				yield return nameof(COOandFTAControlBag.Instance.UsedUQDropEdit);
			}
		}
		protected override ControlBag GetControlBagForTesting() => COOandFTAControlBag.Instance;
	}
}
