using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

sealed class InvoiceChargesTestHelper
{
	public static void AssertCaptions(JobComInvCharge charge) 
	{
		AssertEquals("Fixed %", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_Percentage), [JobDeclaration.CaptionKeyChargesExport]).Caption);
		AssertEquals("% of Line Price", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_Percentage), Array.Empty<string>()).Caption);

		AssertEquals("Include in Line?", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_IsIncludedInITOT), [JobDeclaration.CaptionKeyChargesExport]).Caption);
		AssertEquals("Included in Invoice Lines", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_IsIncludedInITOT), Array.Empty<string>()).Caption);

		AssertEquals("Stat. Value appl.", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_IsStatisticalValueApplicable), [JobDeclaration.CaptionKeyChargesExport]).Caption);
		AssertEquals("Stat. Value appl.", DataBoundResourceStrings.GetDataForProperty(charge.GetType(), nameof(JobComInvCharge.J7_IsStatisticalValueApplicable), Array.Empty<string>()).Caption);
	}
}
