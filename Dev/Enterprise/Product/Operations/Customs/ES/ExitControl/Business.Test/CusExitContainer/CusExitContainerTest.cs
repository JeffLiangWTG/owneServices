using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitContainer))]
	sealed class CusExitContainerTest : EnterpriseBusinessObjectTestCase
	{
		public static CusExitContainer GetNewBusinessObject(BusinessObjectFactory factory) => (CusExitContainer)factory.New<CusExitHeader>().CusExitContainers.AddNew();

		public void TestValidation()
		{
			AssertType<CusExitContainerValidation>(cusExitContainer.Validation);
		}

		public void TestCusExitConsignmentPivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(cusExitContainer.CusExitConsignmentPivots);
		}

		public void TestAllSealNumbers()
		{
			AssertType<CusExitSealCollection>(cusExitContainer.AllSealNumbers);
		}

		public void TestClearStatusAndSetSealsReadOnlyIfStatusIsMissing()
		{
			CombineAssertions(() =>
			{
				var seal1 = cusExitContainer.AllSealNumbers.AddNew();
				seal1.BK_SealNumber = "1";
				seal1.BK_UnloadingState = "MIS";
				var seal2 = cusExitContainer.AllSealNumbers.AddNew();
				seal2.BK_SealNumber = "2";
				seal2.BK_UnloadingState = "DIF";

				AssertEquals("seal1 is not ReadOnly", false, seal1.ReadOnly);
				AssertEquals("seal2 is not ReadOnly", false, seal2.ReadOnly);

				cusExitContainer.CXN_Status = "MIS";
				AssertEquals("seal1 is ReadOnly", true, seal1.ReadOnly);
				AssertEquals("seal2 is ReadOnly", true, seal2.ReadOnly);
				AssertEquals("seal1 Status is empty", ZString.Empty, seal1.BK_UnloadingState);
				AssertEquals("seal2 Status is empty", ZString.Empty, seal2.BK_UnloadingState);

				cusExitContainer.CXN_Status = "DIF";
				AssertEquals("seal1 is not ReadOnly", false, seal1.ReadOnly);
				AssertEquals("seal2 is not ReadOnly", false, seal2.ReadOnly);

				cusExitContainer.CXN_Status = "MIS";
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var containerReloaded = newFactory.Load<CusExitContainer>(cusExitContainer.PK);
				var seal1Reloaded = containerReloaded.AllSealNumbers[0];
				var seal2Reloaded = containerReloaded.AllSealNumbers[1];
				AssertEquals("seal1Reloaded is ReadOnly", true, seal1Reloaded.ReadOnly);
				AssertEquals("seal2Reloaded is ReadOnly", true, seal2Reloaded.ReadOnly);
			});
		}

		public void TestCusSealType()
		{
			AssertType(((ICusSealTypeSupporter)cusExitContainer).CusSealType, cusExitContainer.AllSealNumbers.AddNew());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusExitContainer;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			cusExitContainer = (CusExitContainer)GetNewBusinessObject();
		}

		CusExitContainer cusExitContainer;
	}
}
