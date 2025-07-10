using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(DeclarationOrganizationsLayout))]
	sealed class DeclarationOrganizationsLayoutTest : LayoutsAbstractTest
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
				yield return (DeclarationOrganizationsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongNoCaption);
				yield return (DeclarationOrganizationsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongNoCaption);
				yield return (DeclarationOrganizationsControlBag.Instance.OwnerDocAddressUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationOrganizationsLayoutBuilder<Business.EMCSJobDeclaration>();
	}
}
