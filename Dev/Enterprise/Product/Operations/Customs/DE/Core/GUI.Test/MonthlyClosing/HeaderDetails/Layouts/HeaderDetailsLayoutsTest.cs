using System.Collections.Generic;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsLayouts))]
	sealed class HeaderDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HeaderDetailsLayoutBuilder<CusReconDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonHeaderDetailsControlBag.Instance.EntryTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.PeriodFromDateEdit, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.PeriodToDateEdit, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.DeclarantTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.BuyingAgentAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonHeaderDetailsControlBag.Instance.EntryStatusTextBox, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Auto);
				yield return (HeaderDetailsControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.IsFinalizedCheckBox, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.IsDeclarantImporterCheckBox, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.AuthorizationNumberGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (HeaderDetailsControlBag.Instance.UnlinkedDeclarationsNumberLabel, ControlWidthClass.Long);
			}
		}
	}
}
