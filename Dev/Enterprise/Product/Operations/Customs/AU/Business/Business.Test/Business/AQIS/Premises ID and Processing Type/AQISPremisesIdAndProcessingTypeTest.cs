using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPremisesIdAndProcessingType))]
	sealed class AQISPremisesIdAndProcessingTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPremisesId()
		{
			AQISPremisesIdAndProcessingType premisesId = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertEquals("PremisesId is empty", true, premisesId.PremisesId.IsEmpty);

			premisesId.PremisesId = "Prem";
			AssertEquals("PremisesId is empty", false, premisesId.PremisesId.IsEmpty);
		}

		public void TestPremisesIdMaxLength()
		{
			AQISPremisesIdAndProcessingType premisesId = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertEquals("PremisesId", 10, premisesId.PremisesIdInfo.MaxLength);
		}

		public void TestProcessingType()
		{
			AQISPremisesIdAndProcessingType processingType = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertEquals("ProcessingType is empty", true, processingType.ProcessingType.IsEmpty);

			processingType.ProcessingType = "Processing";
			AssertEquals("ProcessingType is empty", false, processingType.ProcessingType.IsEmpty);
		}

		public void TestProcessingTypeMaxLength()
		{
			AQISPremisesIdAndProcessingType processingType = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertEquals("ProcessingType", 10, processingType.ProcessingTypeInfo.MaxLength);
		}

		public void TestLookups()
		{
			AQISPremisesIdAndProcessingType processingType = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertNotNull("Lookups", processingType.Lookups);
		}

		public void TestValidation()
		{
			AQISPremisesIdAndProcessingType processingType = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			AssertNotNull("Validation", processingType.Validation);
		}

		public void TestIAQISUniqueCodeForSort()
		{
			AQISPremisesIdAndProcessingType processingType = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			processingType.PremisesId = "ABCD";
			processingType.ProcessingType = "EFG";

			AssertEquals("IAQISUniqueCodeForSort Length", 2, ((IAQISUniqueCodeForSort)processingType).CodesToSortBy.Length);
			AssertEquals("IAQISUniqueCodeForSort the first item", "EFG", ((IAQISUniqueCodeForSort)processingType).CodesToSortBy[0]);
			AssertEquals("IAQISUniqueCodeForSort the Second item", "ABCD", ((IAQISUniqueCodeForSort)processingType).CodesToSortBy[1]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
		}

		protected override BusinessObject GetNewBusinessObject() => new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
	}
}
