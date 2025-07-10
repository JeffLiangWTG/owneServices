using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterPremisesLayout))]
	public class TempStorageRegisterPremisesLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TempStorageRegisterPremisesControlBag.Instance.PremisesCodeTextBox, ControlWidthClass.Auto);
				yield return (TempStorageRegisterPremisesControlBag.Instance.PremisesDescriptionTextBox, ControlWidthClass.Auto);
				yield return (TempStorageRegisterPremisesControlBag.Instance.LocationDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises>();
	}
}
