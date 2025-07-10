using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(SpecialProceduresLayout))]
	sealed class SpecialProceduresLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SpecialProceduresLayoutBuilder();

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SpecialProceduresControlBag.Instance.PrimaryOwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.OwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.FirstPlaceOfUseOrProcessingUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.PlaceOfUseOrProcessingGoodsLocationUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (SpecialProceduresControlBag.Instance.PeriodForDischargeUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.BillOfDischargeUserControl, ControlWidthClass.LongNoCaption);
				yield return (EU.GUI.SpecialProceduresControlBag.Instance.ActivitiesAndProceduresUserControl, ControlWidthClass.LongNoCaption);
				yield return (EU.GUI.SpecialProceduresControlBag.Instance.SpecialProceduresOthersUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (SpecialProceduresControlBag.Instance.IdentificationOfGoodsUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.ConditionsAndTermsUserControl, ControlWidthClass.LongNoCaption);
			}
		}
	}
}
