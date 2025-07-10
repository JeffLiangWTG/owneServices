using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class DeltaIEJobComInvoiceHeaderValuePostProcessingStrategyTest : TestCaseWithFactory
	{
		public void TestValuePostProcess()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml");
			var mapping = config.Mapping.First();
			declaration.JE_TransportMode = mapping.Filter.TransportModes.TransportMode[0];
			invoice.ZG_AgreedPlaceCode = mapping.Filter.AgreedPlace;
			declaration.JE_AirRouteType = mapping.Filter.AirRouteTypes.AirRouteType[0];
			invoice.JZ_IncoTerm = mapping.Filter.IncoTerm;

			AssertEquals("Number of charges should be 2", 2, declaration.TopGroupInvoice.Charges.Count);
			var expectedChargesKeys = new HashSet<MessageChargeKey>();
			foreach (var expectedCharge in mapping.Charges.Charge)
			{
				expectedChargesKeys.Add(new MessageChargeKey(expectedCharge.ChargeType, expectedCharge.IsDutiable, expectedCharge.IsGSTApplicable, expectedCharge.IsIncludedInInvoice, expectedCharge.IsStatisticalValueApplicable));
			}

			var actualChargesKeys = new HashSet<MessageChargeKey>();
			foreach (BaseGroupInvoiceCharge actualCharge in declaration.TopGroupInvoice.Charges)
			{
				actualChargesKeys.Add(new MessageChargeKey(actualCharge.J7_ChargeType, actualCharge.J7_IsDutiable, actualCharge.J7_IsGSTApplicable, actualCharge.J7_IsIncludedInITOT, actualCharge.J7_IsStatisticalValueApplicable));
			}

			AssertContainsExactElementsInAnyOrder("Declaration should have expected charges", expectedChargesKeys, actualChargesKeys);
		}
	}
}
