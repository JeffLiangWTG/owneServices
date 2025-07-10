using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(SpecialProceduresLayout))]
	sealed class SpecialProceduresLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SpecialProceduresLayoutBuilder<JobDeclaration>();

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
				yield return (SpecialProceduresControlBag.Instance.ActivitiesAndProceduresUserControl, ControlWidthClass.LongNoCaption);
				yield return (SpecialProceduresControlBag.Instance.SpecialProceduresOthersUserControl, ControlWidthClass.LongNoCaption);
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
