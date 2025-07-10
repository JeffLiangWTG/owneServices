using System.Collections.Generic;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(CusTempStorageRegLineItemDetailsLayout))]
	public class CusTempStorageRegLineItemDetailsLayoutTest : LayoutsAbstractTest
	{
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
				var esBag = CusTempStorageRegLineItemDetailsControlBag.Instance;
				yield return (esBag.GoodsItemNumberCalcEdit, ControlWidthClass.Auto);
				yield return (esBag.TariffTextBox, ControlWidthClass.Auto);
				yield return (esBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				var esBag = CusTempStorageRegLineItemDetailsControlBag.Instance;
				yield return (esBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (esBag.CusC4NumberTextBox, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusTempStorageRegLineItemDetailsLayoutBuilder<CusTempStorageRegHeader>();
	}
}
