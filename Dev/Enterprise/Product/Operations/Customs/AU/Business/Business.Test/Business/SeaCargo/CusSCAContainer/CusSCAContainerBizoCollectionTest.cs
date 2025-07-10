using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAContainerCollection))]
	class CusSCAContainerBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		#region RemoveAndDelete

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsTrue()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainerCollection containerCollection = new CusSCAContainerCollection(oceanBill, Factory);

			CusSCAContainer container = containerCollection.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;

			AssertEquals("Container should be deletable", true, container.CanDelete);
			AssertEquals("One item must exist", 1, containerCollection.Count);

			containerCollection.RemoveAndDelete(container);
			AssertEquals("No item must remain", 0, containerCollection.Count);
		}

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsFalse()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainerCollection containerCollection = new CusSCAContainerCollection(oceanBill, Factory);

			CusSCAContainer container = containerCollection.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			AssertEquals("Container should not be deletable", false, container.CanDelete);
			AssertEquals("One item must exist", 1, containerCollection.Count);

			containerCollection.RemoveAndDelete(container);
			AssertEquals("One item must remain", 1, containerCollection.Count);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			return new CusSCAContainerCollection(oceanBill, Factory);
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
		#endregion
	}
}
