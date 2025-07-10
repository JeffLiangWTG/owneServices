using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(WarehouseAdjustmentAdditionalInfoLayout))]
	class WarehouseAdjustmentAdditionalInfoLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return Enumerable.Empty<(ControlReference, ControlWidthClass)>();
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInfoLayoutBuilder<JobDeclaration>();
	}
}
