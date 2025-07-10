using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(TransportProvider))]
	class TransportProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new TransportProvider(null));
		}

		public void TestUnitCode()
		{
			container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Tractor;
			AssertEquals(EMCSTransportUnitCodeList.Codes.Tractor, dataProvider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			container.CO_ContainerNumber = "PONU2864065";
			AssertEquals("PONU2864065", dataProvider.IdentityOfUnit);
		}

		public void TestCommercialSealIdentification()
		{
			container.CO_Seal = "4419151";
			AssertEquals("4419151", dataProvider.CommercialSealIdentification);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("Comment", "COMMENT ABOUT THE CONTAINER", dataProvider.ComplementaryInformation.Text);
		}

		public void TestComplementaryInformation_NotConsolidated()
		{
			SetupDeclarationAndAuthorization();
			container.Comment = "NOT CONSOLIDATED";
			CombineAssertions(() =>
			{
				AssertNotNull("Declaration Linked", container.Declaration);
				AssertEquals("Not Consolidated", "NOT CONSOLIDATED", dataProvider.ComplementaryInformation.Text);
			});
		}

		public void TestComplementaryInformation_ConsolidatedWithAuthorization()
		{
			SetupDeclarationAndAuthorization();
			container.Comment = ZString.Empty;
			container.Declaration.SetConsolidatedDocument();
			AssertEquals("Consolidated", "TESTAUTHORIZATION.", dataProvider.ComplementaryInformation.Text);
		}

		public void TestComplementaryInformation_ConsolidatedWithCommentAndAuthorization()
		{
			SetupDeclarationAndAuthorization();
			container.Declaration.SetConsolidatedDocument();
			container.Comment = "EXTRA CONTAINER COMMENT";
			AssertEquals("Consolidated", "TESTAUTHORIZATION. EXTRA CONTAINER COMMENT", dataProvider.ComplementaryInformation.Text);
		}

		public void TestComplementaryInformation_ConsolidatedWithCommentAndAuthorizationAndBRERuleValue()
		{
			SetupDeclarationAndAuthorization();
			var rule = authorization.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = DE.Business.CusAuthorisationRuleTypeList.Codes.BusinessReference;
			rule.CPR_ValueFrom = "Z 1210 / Z 1502 B - B12";

			container.Declaration.SetConsolidatedDocument();
			container.Comment = "EXTRA CONTAINER COMMENT";
			AssertEquals("Consolidated", "TESTAUTHORIZATION Z 1210 / Z 1502 B - B12. EXTRA CONTAINER COMMENT", dataProvider.ComplementaryInformation.Text);
		}

		public void TestSealInformation()
		{
			AssertEquals("SEAL INFORMATION", dataProvider.SealInformation.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<EMCSCusContainer>();
			container.Comment = "COMMENT ABOUT THE CONTAINER";
			container.SealDetails = "SEAL INFORMATION";
			dataProvider = new TransportProvider(container);
		}
		EMCSCusContainer container;
		IEMCSTransport dataProvider;

		protected override TransportProvider GetProvider() => (TransportProvider)dataProvider;

		protected override IEnumerable<Expression<Func<TransportProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
			yield return x => x.SealInformation;
		}

		void SetupDeclarationAndAuthorization()
		{
			var orgHeader = Factory.New<OrgHeader>();
			authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TESTAUTHORIZATION", Core.Constants.CountryCodes.Germany);
			var rule = authorization.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = DE.Business.CusAuthorisationRuleTypeList.Codes.Usage;
			rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			declaration.CusContainers.Add(container);
		}
		CusAuthorisationHeader authorization;
	}
}
