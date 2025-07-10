using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesDetailsLayout))]
	sealed class TempStoragePremisesDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				var euBag = TempStoragePremisesDetailsControlBag.Instance;

				return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.CodeTextBox, ControlWidthClass.Long),
					(euBag.DescriptionTextBox, ControlWidthClass.Long),
					(euBag.TypeDropEdit, ControlWidthClass.Long),
					(euBag.AuthorizationNumberCodeFindBox, ControlWidthClass.Long),
					(euBag.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long),
					(euBag.PremisesAddressAddressControl, ControlWidthClass.Long),
					(euBag.CustomsLocationCodeFindBox, ControlWidthClass.Long),
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TempStoragePremisesDetailsLayoutBuilder<CusTempStorageRegPremises>();
	}
}
