using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class DeltaGJobComInvoiceHeaderValuePostProcessingStrategyTest : TestCaseWithFactory
	{
		public void TestValuePostProcess()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoice = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var valueSetStrategy = new DeltaGJobComInvoiceHeaderValueSetStrategy(invoice);

			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoice.JZ_IncoTermPlace = "AAA";
			invoice.ZG_AgreedPlaceCode = "BBB";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("JZ_IncoTermPlace should be empty", string.Empty, invoice.JZ_IncoTermPlace);
			AssertEquals("ZG_AgreedPlaceCode should be 3", "3", invoice.ZG_AgreedPlaceCode);

			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("ZG_AgreedPlaceCode should be 1", "1", invoice.ZG_AgreedPlaceCode);
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml");
			var mapping = config.Mapping.First();

			jobDeclaration.JE_TransportMode = mapping.Filter.TransportModes.TransportMode[0];
			invoice.ZG_AgreedPlaceCode = mapping.Filter.AgreedPlace;
			jobDeclaration.JE_AirRouteType = mapping.Filter.AirRouteTypes.AirRouteType[0];
			invoice.JZ_IncoTerm = mapping.Filter.IncoTerm;

			AssertEquals("Number of charges should be 2", 2, jobDeclaration.TopGroupInvoice.Charges.Count);
			var expectedChargesKeys = new HashSet<MessageChargeKey>();
			foreach (var expectedCharge in mapping.Charges.Charge)
			{
				expectedChargesKeys.Add(new MessageChargeKey(expectedCharge.ChargeType, expectedCharge.IsDutiable, expectedCharge.IsGSTApplicable, expectedCharge.IsIncludedInInvoice, expectedCharge.IsStatisticalValueApplicable));
			}

			var actualChargesKeys = new HashSet<MessageChargeKey>();
			foreach (BaseGroupInvoiceCharge actualCharge in jobDeclaration.TopGroupInvoice.Charges)
			{
				actualChargesKeys.Add(new MessageChargeKey(actualCharge.J7_ChargeType, actualCharge.J7_IsDutiable, actualCharge.J7_IsGSTApplicable, actualCharge.J7_IsIncludedInITOT, actualCharge.J7_IsStatisticalValueApplicable));
			}

			AssertContainsExactElementsInAnyOrder("Declaration should have expected charges", expectedChargesKeys, actualChargesKeys);
		}
	}
}
