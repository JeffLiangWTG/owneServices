using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class ChargeProviderTest : TestCaseWithFactory
	{
		public void TestChargeMatrixAttribute()
		{
			//var ADV
			var advCharge = (FlagManagedCharge)ChargesProvider.Adjustment;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", !advCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", advCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !advCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", advCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", advCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", advCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", advCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !advCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, advCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", !advCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)advCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !advCharge.IsIncludedInInvoice);
			});
			//var BCM
			var bcmCharge = (FlagManagedCharge)ChargesProvider.BuyingCommissions;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", !bcmCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", bcmCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !bcmCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", bcmCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", bcmCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", bcmCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", bcmCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", bcmCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, bcmCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", bcmCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", (bool)bcmCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", bcmCharge.IsIncludedInInvoice);
			});
			//var CEA
			var ceaCharge = (FlagManagedCharge)ChargesProvider.ConstructionErectionAssembly;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", !ceaCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", ceaCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !ceaCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", ceaCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", ceaCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", ceaCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", ceaCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !ceaCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, ceaCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", ceaCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", (bool)ceaCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", ceaCharge.IsIncludedInInvoice);
			});
			//var COM
			var comCharge = (FlagManagedCharge)ChargesProvider.CommissionExceptBuyingCommissions;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", comCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", comCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", comCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", comCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", comCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", comCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", comCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !comCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, comCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", comCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)comCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !comCharge.IsIncludedInInvoice);
			});
			//var CPA
			var cpaCharge = (FlagManagedCharge)ChargesProvider.ContainersAndPacking;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", cpaCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", cpaCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", cpaCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", cpaCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", cpaCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", cpaCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", cpaCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", cpaCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, cpaCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", cpaCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)cpaCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !cpaCharge.IsIncludedInInvoice);
			});
			//var EDA
			var edaCharge = (FlagManagedCharge)ChargesProvider.EngineeringDevelopmentArtwork;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", edaCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", edaCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", edaCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", edaCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", edaCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", edaCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", edaCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", edaCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, edaCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", !edaCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)edaCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !edaCharge.IsIncludedInInvoice);
			});
			//var IDO
			var idoCharge = (FlagManagedCharge)ChargesProvider.ImportDutiesOrOther;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", !idoCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", idoCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !idoCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", idoCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", !idoCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", idoCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", idoCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !idoCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, idoCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", idoCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", (bool)idoCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", idoCharge.IsIncludedInInvoice);
			});
			//var INT
			var intCharge = (FlagManagedCharge)ChargesProvider.Interest;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", !intCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", intCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !intCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", intCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", intCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", intCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", intCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", intCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, intCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", intCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", (bool)intCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", intCharge.IsIncludedInInvoice);
			});
			//var MAC
			var macCharge = (FlagManagedCharge)ChargesProvider.MaterialsConsumed;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", macCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", macCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", macCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", macCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", macCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", macCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", macCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !macCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, macCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", !macCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)macCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !macCharge.IsIncludedInInvoice);
			});
			//var MCP
			var mcpCharge = (FlagManagedCharge)ChargesProvider.MaterialsComponentsParts;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", mcpCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", mcpCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", mcpCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", mcpCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", mcpCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", mcpCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", mcpCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !mcpCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, mcpCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", !mcpCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)mcpCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !mcpCharge.IsIncludedInInvoice);
			});
			//var PSR
			var psrCharge = (FlagManagedCharge)ChargesProvider.ProceedsOfAnySubsequentResale;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", psrCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", psrCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", psrCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", psrCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", psrCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", psrCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", psrCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !psrCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, psrCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", !psrCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)psrCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !psrCharge.IsIncludedInInvoice);
			});
			//var RLF
			var rlfCharge = (FlagManagedCharge)ChargesProvider.RoyaltiesLicenseFee;
			CombineAssertions(() =>
			{
				Assert("IsDutiable", rlfCharge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", rlfCharge.IsDutiableDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", rlfCharge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", rlfCharge.IsStatisticalValueApplicableDeemed);
				Assert("IsVATible", rlfCharge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", rlfCharge.IsVATibleDeemedForThisCharge);
				Assert("IsIncoTermNeutral", rlfCharge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", rlfCharge.IsPercentageApplicable);
				AssertEquals("ParentTypes", ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, rlfCharge.ParentTypes);
				Assert("IsIncludedInITOTDeemedForThisCharge", rlfCharge.IsIncludedInITOTDeemedForThisCharge);
				Assert("IsIncludedInITOTIfDeemed", !(bool)rlfCharge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInInvoice", !rlfCharge.IsIncludedInInvoice);
			});
			//var ANS
			var airInsuranceCosts = ChargesProvider.AirInsuranceCosts;
			Assert(airInsuranceCosts.IsDutiable);
			Assert(!airInsuranceCosts.IsDutiableDeemedForThisCharge);
			Assert(airInsuranceCosts.IsStatisticalValueApplicable);
			Assert(!airInsuranceCosts.IsStatisticalValueApplicableDeemed);
			Assert(airInsuranceCosts.IsVATible);
			Assert(!airInsuranceCosts.IsVATibleDeemedForThisCharge);
			Assert(!airInsuranceCosts.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice, airInsuranceCosts.ParentTypes);

			//var CEE
			var exclusiveFreightInsideEU = ChargesProvider.ExclusiveFreightInsideEU;
			Assert(!exclusiveFreightInsideEU.IsDutiable);
			Assert(!exclusiveFreightInsideEU.IsDutiableDeemedForThisCharge);
			Assert(exclusiveFreightInsideEU.IsStatisticalValueApplicable);
			Assert(!exclusiveFreightInsideEU.IsStatisticalValueApplicableDeemed);
			Assert(exclusiveFreightInsideEU.IsVATible);
			Assert(!exclusiveFreightInsideEU.IsVATibleDeemedForThisCharge);
			Assert(exclusiveFreightInsideEU.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, exclusiveFreightInsideEU.ParentTypes);

			//var CNE
			var exclusiveInsuranceInsideEU = ChargesProvider.ExclusiveInsuranceInsideEU;
			Assert(!exclusiveInsuranceInsideEU.IsDutiable);
			Assert(!exclusiveInsuranceInsideEU.IsDutiableDeemedForThisCharge);
			Assert(exclusiveInsuranceInsideEU.IsStatisticalValueApplicable);
			Assert(!exclusiveInsuranceInsideEU.IsStatisticalValueApplicableDeemed);
			Assert(exclusiveInsuranceInsideEU.IsVATible);
			Assert(!exclusiveInsuranceInsideEU.IsVATibleDeemedForThisCharge);
			Assert(exclusiveInsuranceInsideEU.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, exclusiveInsuranceInsideEU.ParentTypes);

			//var CEI
			var inclusiveFreightInsideEU = ChargesProvider.InclusiveFreightInsideEU;
			Assert(!inclusiveFreightInsideEU.IsDutiable);
			Assert(!inclusiveFreightInsideEU.IsDutiableDeemedForThisCharge);
			Assert(inclusiveFreightInsideEU.IsStatisticalValueApplicable);
			Assert(!inclusiveFreightInsideEU.IsStatisticalValueApplicableDeemed);
			Assert(inclusiveFreightInsideEU.IsVATible);
			Assert(!inclusiveFreightInsideEU.IsVATibleDeemedForThisCharge);
			Assert(!inclusiveFreightInsideEU.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, inclusiveFreightInsideEU.ParentTypes);

			//var CNI
			var inclusiveInsuranceInsideEU = ChargesProvider.InclusiveInsuranceInsideEU;
			Assert(!inclusiveInsuranceInsideEU.IsDutiable);
			Assert(!inclusiveInsuranceInsideEU.IsDutiableDeemedForThisCharge);
			Assert(inclusiveInsuranceInsideEU.IsStatisticalValueApplicable);
			Assert(!inclusiveInsuranceInsideEU.IsStatisticalValueApplicableDeemed);
			Assert(inclusiveInsuranceInsideEU.IsVATible);
			Assert(!inclusiveInsuranceInsideEU.IsVATibleDeemedForThisCharge);
			Assert(!inclusiveInsuranceInsideEU.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, inclusiveInsuranceInsideEU.ParentTypes);

			//var FRI
			var inclusiveFreightFromFrenchBorder = ChargesProvider.InclusiveFreightFromFrenchBorder;
			Assert(!inclusiveFreightFromFrenchBorder.IsDutiable);
			Assert(!inclusiveFreightFromFrenchBorder.IsDutiableDeemedForThisCharge);
			Assert(!inclusiveFreightFromFrenchBorder.IsStatisticalValueApplicable);
			Assert(!inclusiveFreightFromFrenchBorder.IsStatisticalValueApplicableDeemed);
			Assert(inclusiveFreightFromFrenchBorder.IsVATible);
			Assert(!inclusiveFreightFromFrenchBorder.IsVATibleDeemedForThisCharge);
			Assert(!inclusiveFreightFromFrenchBorder.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, inclusiveFreightFromFrenchBorder.ParentTypes);

			//var FNI
			var inclusiveInsuranceFromFrenchBorder = ChargesProvider.InclusiveInsuranceFromFrenchBorder;
			Assert(!inclusiveInsuranceFromFrenchBorder.IsDutiable);
			Assert(!inclusiveInsuranceFromFrenchBorder.IsDutiableDeemedForThisCharge);
			Assert(!inclusiveInsuranceFromFrenchBorder.IsStatisticalValueApplicable);
			Assert(!inclusiveInsuranceFromFrenchBorder.IsStatisticalValueApplicableDeemed);
			Assert(inclusiveInsuranceFromFrenchBorder.IsVATible);
			Assert(!inclusiveInsuranceFromFrenchBorder.IsVATibleDeemedForThisCharge);
			Assert(!inclusiveInsuranceFromFrenchBorder.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, inclusiveInsuranceFromFrenchBorder.ParentTypes);

			//var FRE
			var exclusiveFreightToFrenchDestination = ChargesProvider.ExclusiveFreightToFrenchDestination;
			Assert(!exclusiveFreightToFrenchDestination.IsDutiable);
			Assert(!exclusiveFreightToFrenchDestination.IsDutiableDeemedForThisCharge);
			Assert(!exclusiveFreightToFrenchDestination.IsStatisticalValueApplicable);
			Assert(!exclusiveFreightToFrenchDestination.IsStatisticalValueApplicableDeemed);
			Assert(exclusiveFreightToFrenchDestination.IsVATible);
			Assert(!exclusiveFreightToFrenchDestination.IsVATibleDeemedForThisCharge);
			Assert(!exclusiveFreightToFrenchDestination.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, exclusiveFreightToFrenchDestination.ParentTypes);

			//var FNE
			var exclusiveInsuranceToFrenchDestination = ChargesProvider.ExclusiveInsuranceToFrenchDestination;
			Assert(!exclusiveInsuranceToFrenchDestination.IsDutiable);
			Assert(!exclusiveInsuranceToFrenchDestination.IsDutiableDeemedForThisCharge);
			Assert(!exclusiveInsuranceToFrenchDestination.IsStatisticalValueApplicable);
			Assert(!exclusiveInsuranceToFrenchDestination.IsStatisticalValueApplicableDeemed);
			Assert(exclusiveInsuranceToFrenchDestination.IsVATible);
			Assert(!exclusiveInsuranceToFrenchDestination.IsVATibleDeemedForThisCharge);
			Assert(!exclusiveInsuranceToFrenchDestination.IsIncoTermNeutral);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, exclusiveInsuranceToFrenchDestination.ParentTypes);

			//var CUT
			var cut = ChargesProvider.Cut;
			Assert(!cut.IsDutiable);
			Assert(cut.IsDutiableDeemedForThisCharge);
			Assert(!cut.IsVATible);
			Assert(cut.IsVATibleDeemedForThisCharge);
			Assert(!cut.IsStatisticalValueApplicable);
			Assert(cut.IsStatisticalValueApplicableDeemed);
			Assert(cut.IsIncoTermNeutral);
			Assert(cut.IsPercentageApplicable);
			Assert(cut.IsIncludedInITOTDeemedForThisCharge);
			Assert((bool)cut.IsIncludedInITOTIfDeemed);
			Assert(!(cut as FlagManagedCharge).IsIncludedInInvoice);
			AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, cut.ParentTypes);
		}
	}
}
