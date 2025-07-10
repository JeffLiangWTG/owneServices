using System.Collections.Generic;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(OrganisationDetailsLayout))]
	sealed class OrganisationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganisationDetailsLayoutBuilder<CusIntrastatHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (OrganisationDetailsControlBag.Instance.SupplierOrganisationControl, ControlWidthClass.LongControl);
				yield return (OrganisationDetailsControlBag.Instance.ConsigneeOrganisationControl, ControlWidthClass.LongControl);
			}
		}
	}
}
