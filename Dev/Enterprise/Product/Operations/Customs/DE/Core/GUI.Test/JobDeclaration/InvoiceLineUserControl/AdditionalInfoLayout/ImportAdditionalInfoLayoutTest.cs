using System.Collections.Generic;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportAdditionalInfoLayout))]
	class ImportAdditionalInfoLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInfoLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalInfoControlBag.Instance.ContentInfoTypeSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (AdditionalInfoControlBag.Instance.ContentInfoTypeGrid, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (AdditionalInfoControlBag.Instance.AdditionalTariffsSeparatorUserControl, ControlWidthClass.LongControl);
				yield return (AdditionalInfoControlBag.Instance.AdditionalTariffsGrid, ControlWidthClass.LongControl);
			}
		}
	}
}
