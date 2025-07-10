using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(DV1DetailsLayout))]
	sealed class DV1DetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestDetailsFieldsAreLinedCorrectly()
		{
			using (var control = new DV1DetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("relationDetailsTextBox must align with priceInfluenceDropEdit", control.PriceInfluenceDropEdit.Location.Y, control.RelationDetailsTextBox.Location.Y);
					AssertEquals("restrictionsConsiderationTextBox must align with considerationDropEdit", control.ConsiderationDropEdit.Location.Y, control.RestrictionsConsiderationTextBox.Location.Y);
					AssertEquals("royalitiesLicenceDetailsTextBox must align with royalitiesLicenceDropEdit", control.RoyalitiesLicenceDropEdit.Location.Y, control.RoyalitiesLicenceDetailsTextBox.Location.Y);
					AssertEquals("resaleDetailsTextBox must align with resaleDropEdit", control.ResaleDropEdit.Location.Y, control.ResaleDetailsTextBox.Location.Y);
				});
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
				yield return (DV1DetailsControlBag.Instance.RelationshipDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.PriceInfluenceDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.RestrictionsDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.ConsiderationDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.ResaleDropEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (DV1DetailsControlBag.Instance.RelationDetailsTextBox, ControlWidthClass.LongControl);
				yield return (DV1DetailsControlBag.Instance.RestrictionsConsiderationTextBox, ControlWidthClass.LongControl);
				yield return (DV1DetailsControlBag.Instance.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.LongControl);
				yield return (DV1DetailsControlBag.Instance.ResaleDetailsTextBox, ControlWidthClass.LongControl);
				yield return (DV1DetailsControlBag.Instance.CustomsDecisionNumberTextBox, ControlWidthClass.LongControl);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DV1DetailsLayoutBuilder<JobDeclaration>();
	}
}
