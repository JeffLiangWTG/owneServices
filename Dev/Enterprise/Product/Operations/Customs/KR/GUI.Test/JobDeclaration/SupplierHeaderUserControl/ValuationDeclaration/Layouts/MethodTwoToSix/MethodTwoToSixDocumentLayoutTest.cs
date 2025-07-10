using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToSixDocumentLayout))]
	sealed class MethodTwoToSixDocumentLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

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
				yield return (methodTwoToSixControlBag.ExpectedCustomsValueCalcEdit, ControlWidthClass.Auto);
				yield return (methodTwoToSixControlBag.SupportingDocument1TextBox, ControlWidthClass.Long);
				yield return (methodTwoToSixControlBag.SupportingDocument2TextBox, ControlWidthClass.Long);
			}
		}

		MethodTwoToSixControlBag methodTwoToSixControlBag => MethodTwoToSixControlBag.InstanceForDeclaration;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MethodTwoToSixDocumentLayoutBuilder<JobDeclaration>(MethodTwoToSixControlBag.InstanceForDeclaration);
	}
}
