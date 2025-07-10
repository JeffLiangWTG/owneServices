using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(SpecialProceduresControlBag))]
	sealed class SpecialProceduresControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SpecialProceduresControlBag.PrimaryOwnerOfGoodsUserControl);
				yield return nameof(SpecialProceduresControlBag.OwnerOfGoodsUserControl);
				yield return nameof(SpecialProceduresControlBag.PeriodForDischargeUserControl);
				yield return nameof(SpecialProceduresControlBag.BillOfDischargeUserControl);
				yield return nameof(SpecialProceduresControlBag.FirstPlaceOfUseOrProcessingUserControl);
				yield return nameof(SpecialProceduresControlBag.PlaceOfUseOrProcessingGoodsLocationUserControl);
				yield return nameof(SpecialProceduresControlBag.IdentificationOfGoodsUserControl);
				yield return nameof(SpecialProceduresControlBag.ConditionsAndTermsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SpecialProceduresControlBag.Instance;
	}
}
