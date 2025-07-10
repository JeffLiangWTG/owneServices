using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAcceptabilityBandViewModel))]
	class MENTAcceptabilityBandViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MENTAcceptabilityBandViewModel(Factory.NewWithValidTestData<BMComponentAcceptabilityBand>());
		}

		public void TestNoQueryExists_CreatesButDoesntSave()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "Band[]CopterMouseMan";

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			AssertNotNull(viewModel.Query);

			Factory.Save();

			AssertEquals(false, viewModel.Query.IsInDatabase);
			AssertEquals(false, viewModel.Query.QuerySchedule.IsInDatabase);
		}

		public void TestNoQueryExists_CreatesDummyQueryWhenMENTing()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "Band[]CopterMouseMan";

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			var query = viewModel.Query;
			AssertNotNull(query);
			AssertEquals(true, query.MAQ_IsActive);
			AssertEquals("BandCopter", query.MAQ_Code);
			AssertEquals("Generated MENT code from Acceptability Band: " + band.BAB_Name, query.MAQ_QueryDescription);
			AssertEquals(band.PK, query.MAQ_BAB_RelatedAcceptabilityBand);
			AssertEquals(false, band.HasChanges);
			AssertEquals(false, query.HasChanges);

			query.MAQ_QueryDescription = "Making changes is fun";
			viewModel.MentEnabled = true;

			AssertEquals(true, band.HasChanges);
			AssertEquals(true, query.HasChanges);
			AssertEquals(true, query.MAQ_IsActive);
			AssertEquals("BandCopter", query.MAQ_Code);
			AssertEquals(band.PK, query.MAQ_BAB_RelatedAcceptabilityBand);

			Factory.Save();

			AssertEquals(true, viewModel.Query.IsInDatabase);
		}

		public void TestActiveQueryExists()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "BandCopterMouseMan";

			var query = MENTTestHelper.CreateQuery(Factory, "NEWOOWW");
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			query.MAQ_IsActive = true;

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			AssertEquals(query, viewModel.Query);
			AssertEquals(false, band.HasChanges);
		}

		public void TestInActiveQueryExists()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "BandCopterMouseMan";

			var query = MENTTestHelper.CreateQuery(Factory, "NEWOOWW");
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			query.MAQ_IsActive = false;

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			AssertEquals(query, viewModel.Query);
			AssertEquals(false, band.HasChanges);
		}

		public void TestInActiveQueryExistsReactivating()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "BandCopterMouseMan";

			var query = MENTTestHelper.CreateQuery(Factory, "NEWOOWW");
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			query.MAQ_IsActive = false;

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			AssertEquals(query, viewModel.Query);
			AssertEquals(false, band.HasChanges);
			AssertEquals(false, viewModel.Query.MAQ_IsActive);

			viewModel.Query.MAQ_IsActive = true;
			AssertEquals(true, band.HasChanges);
			AssertEquals(true, viewModel.Query.MAQ_IsActive);

			Factory.Save();

			var anotherViewModel = new MENTAcceptabilityBandViewModel(band);
			AssertEquals(query, anotherViewModel.Query);
			AssertEquals(false, band.HasChanges);
			AssertEquals(true, anotherViewModel.Query.MAQ_IsActive);
		}

		public void TestMENTAttributeDescriptionHasFromABAppended()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "QCODEFRMAB";

			Factory.Save();

			var viewModel = new MENTAcceptabilityBandViewModel(band);

			var query = viewModel.Query;

			AssertContains("FROM_AB:", query.MAQ_AttributeDescription);
		}

		public void TestMENTFromClonedABIsUnique()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "Paolo Test 1";

			var viewModel = new MENTAcceptabilityBandViewModel(band);
			var query = viewModel.Query;

			viewModel.MentEnabled = true;

			AssertEquals("PaoloTest1", query.MAQ_Code);
			AssertEquals(band.PK, query.MAQ_BAB_RelatedAcceptabilityBand);
			AssertEquals(true, query.HasChanges);

			Factory.Save();

			AssertEquals(true, query.IsInDatabase);

			var copyAB = (BMComponentAcceptabilityBand)band.Clone();

			AssertEquals(true, copyAB.BAB_Name.EndsWith("Copy"));

			var viewCloneModel = new MENTAcceptabilityBandViewModel(copyAB);
			var queryClone = viewCloneModel.Query;

			AssertNotNull(queryClone);
			AssertEquals(false, queryClone.IsInDatabase);

			viewCloneModel.MentEnabled = true;

			AssertEquals(true, query.MAQ_CodeInfo.ReadOnly);
			AssertEquals(query.MAQ_Code, queryClone.MAQ_Code);
			AssertEquals(copyAB.PK, queryClone.MAQ_BAB_RelatedAcceptabilityBand);
			AssertNotEquals(query.MAQ_BAB_RelatedAcceptabilityBand, queryClone.MAQ_BAB_RelatedAcceptabilityBand);

			Factory.Save();

			AssertEquals(true, queryClone.IsInDatabase);
		}

		public void TestABNameChangeTriggersNewMENTCode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "TEST1NG RANDOM BAND NAME";

			var viewModel = new MENTAcceptabilityBandViewModel(band);
			var query = viewModel.Query;

			AssertNotNull(viewModel.Query);
			AssertEquals("TEST1NGRAN", query.MAQ_Code);

			band.BAB_Name = "T3STING RANDOM BAND NAME";

			AssertEquals("T3STINGRAN", query.MAQ_Code);

			viewModel.MentEnabled = true;

			Factory.Save();

			AssertEquals(true, band.IsInDatabase);
			AssertEquals(true, query.IsInDatabase);
			AssertEquals("T3STINGRAN", query.MAQ_Code);
		}
	}
}
