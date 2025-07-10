using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(AlternativeEvidence))]
	class AlternativeEvidenceTest : Customs.Business.Testing.CusCodeDataTest<AlternativeEvidence>
	{
		public void TestHumanReadableName()
		{
			var alternativeEvidence = Factory.New<AlternativeEvidence>();
			AssertEquals("Alternative Evidence", alternativeEvidence.HumanReadableName);
		}

		public void TestAdditionalInfos()
		{
			var alternativeEvidence = Factory.New<AlternativeEvidence>();
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(alternativeEvidence.AdditionalInfos);
		}

		public void TestCY_Code_Attributes()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(AlternativeEvidence), nameof(AlternativeEvidence.CY_Code), false, attribute => attribute.MaxLength == 2);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(AlternativeEvidence), nameof(AlternativeEvidence.CY_Code), false, attribute => attribute.Caption == "Type" && string.IsNullOrEmpty(attribute.MultipleKey));
		}

		public void TestValidation()
		{
			var alternativeEvidence = Factory.New<AlternativeEvidence>();
			AssertType<AlternativeEvidenceValidation>(alternativeEvidence.Validation);
		}

		public void TestLookups()
		{
			var alternativeEvidence = Factory.New<AlternativeEvidence>();
			AssertType<AlternativeEvidenceLookups>(alternativeEvidence.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override IEnumerable<AlternativeEvidence> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (AlternativeEvidence)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Now;
			report.CER_OfficeOfExit = "DE001";
			return report.AlternativeEvidences.AddNew();
		}
	}
}
