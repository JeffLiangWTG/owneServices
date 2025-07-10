using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterDetailsLayout))]
	class TempStorageRegisterDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var bag = TempStorageRegisterDetailsControlBag.Instance;
				yield return (bag.LineNumberCalcEdit, ControlWidthClass.Auto);
				yield return (bag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (bag.LocationOfGoodsTextBox, ControlWidthClass.Auto);
				yield return (bag.GrossWeightUQTextBox, ControlWidthClass.Auto);
				yield return (bag.GrossWeightRemainingCalcEdit, ControlWidthClass.Auto);
				yield return (bag.GoodsOwnerIdentifierTextBox, ControlWidthClass.Auto);
				yield return (bag.OwnerReferenceNumberTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				var bag = TempStorageRegisterDetailsControlBag.Instance;
				yield return (bag.LimitDateEdit, ControlWidthClass.Auto);
				yield return (bag.UnionStatusDropEdit, ControlWidthClass.Auto);
				yield return (bag.PackageTypeDropEdit, ControlWidthClass.Auto);
				yield return (bag.PackagesRemainingCalcEdit, ControlWidthClass.Auto);
				yield return (bag.PackageMarksTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				var bag = TempStorageRegisterDetailsControlBag.Instance;
				yield return (bag.BondAmountRemainingCalculatedCalcEdit, ControlWidthClass.Auto);
				yield return (bag.CustomsStatusDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TempStorageRegisterDetailsLayoutBuilder<CusTempStorageRegHeader>();
	}
}
