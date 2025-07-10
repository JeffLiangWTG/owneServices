using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsPackageLayoutWithGrid))]
	class NctsPackageLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override Type ExpectedGridUserControlType => typeof(NctsPackagesGridUserControl);

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (NctsPackageControlBag.Instance.SequenceNumberCalcEdit, ControlWidthClass.Medium);
				yield return (NctsPackageControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Auto);
				yield return (NctsPackageControlBag.Instance.UnitTypeDropEdit, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.UnitCountCalcEdit, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.PackageIDTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.BrandTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.ModelTextBox, ControlWidthClass.Long);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (NctsPackageControlBag.Instance.PlaceHolder1Label, ControlWidthClass.Auto);
				yield return (NctsPackageControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Auto);
				yield return (NctsPackageControlBag.Instance.DifUnitTypeDropEdit, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.DifUnitCountCalcEdit, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.DifMarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.DifPackageIDTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.DifBrandTextBox, ControlWidthClass.Long);
				yield return (NctsPackageControlBag.Instance.DifModelTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new NctsPackageLayoutBuilder<NctsPackage>();
	}
}
