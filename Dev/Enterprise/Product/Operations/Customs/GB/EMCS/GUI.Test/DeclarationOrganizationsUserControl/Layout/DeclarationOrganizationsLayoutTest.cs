using System.Collections.Generic;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.GUI.Testing
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
				yield return (DeclarationOrganizationControlBag.Instance.CertificateIdentifierGroupBox, ControlWidthClass.LongNoCaption);
				yield return (EU.EMCS.GUI.DeclarationOrganizationsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongNoCaption);
				yield return (EU.EMCS.GUI.DeclarationOrganizationsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongNoCaption);
				yield return (EU.EMCS.GUI.DeclarationOrganizationsControlBag.Instance.OwnerDocAddressUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration>();
	}
}
