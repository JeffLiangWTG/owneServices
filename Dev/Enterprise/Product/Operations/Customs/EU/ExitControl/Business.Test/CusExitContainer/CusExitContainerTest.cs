using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitContainer))]
	sealed class CusExitContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusExitConsignmentPivots()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(container.CusExitConsignmentPivots);
		}

		public void TestHeader()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitHeader>(container.Header);
		}

		public void TestCXN_IsEquipment_Caption()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(container.CXN_IsEquipmentInfo, (string)null, "Is Equipment");
		}

		public void TestCXN_ContainerNumber_Caption()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(container.CXN_ContainerNumberInfo, (string)null, "Number");
		}

		public void TestCXN_Sequence_Tags()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			AssertEquals("Sequence Number", DataBoundResourceStrings.GetDataForProperty(container.CXN_SequenceInfo).Caption);
			AssertEquals("Seq Number", DataBoundResourceStrings.GetDataForProperty(container.CXN_SequenceInfo).MediumCaption);
			AssertEquals("Seq Num.", DataBoundResourceStrings.GetDataForProperty(container.CXN_SequenceInfo).ShortCaption);
		}

		public void TestAdditionalSealNumbersNumberDictionary()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			var seal1 = CusExitSeal.Create(container, 1);
			seal1.BK_SealNumber = "ONE";
			var allSealNumbers = container.AllSealNumbers;
			var seal11 = allSealNumbers.AddNew();
			seal11.BK_SealNumber = "TWO";
			var seal12 = allSealNumbers.AddNew();
			seal12.BK_SealNumber = "TWO";
			Factory.Save();

			var dictionary = new Dictionary<ZString, ZShort>();
			dictionary.Add("ONE", 1);
			dictionary.Add("TWO", 2);
			AssertEquals("Dictionary two lines", true, container.AdditionalSealNumbersNumberDictionary.ContainsSameElementsInAnyOrder(dictionary));
		}

		public void TestAdditionalSealNumbersSequenceNumberDictionary()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			var seal1 = CusExitSeal.Create(container, 1);
			seal1.BK_SealNumber = "ONE";
			var allSealNumbers = container.AllSealNumbers;
			var seal11 = allSealNumbers.AddNew();
			seal11.BK_SealNumber = "TWO";
			var seal12 = allSealNumbers.AddNew();
			seal12.BK_SealNumber = "THREE";
			Factory.Save();

			var dictionary = new Dictionary<ZShort, ZShort>();
			dictionary.Add(1, 1);
			dictionary.Add(2, 1);
			dictionary.Add(3, 1);
			AssertEquals("Dictionary three lines", true, container.AdditionalSealNumbersSequenceNumberDictionary.ContainsSameElementsInAnyOrder(dictionary));
		}

		public void TestCXN_ContainerNumber_MaxLength()
		{
			(var exitContainer, var exitHeader) = GetNewBusinessObject(Factory);
			AssertEquals(17, exitContainer.CXN_ContainerNumberInfo.MaxLength);
		}

		public void TestAllSealNumbers()
		{
			(var container, _) = GetNewBusinessObject(Factory);
			var seal1 = CusExitSeal.Create(container, 1);
			_ = CusExitSeal.Create(container, ZShort.Zero);
			var seal3 = CusExitSeal.Create(container, 2);
			var allSealNumbers = container.AllSealNumbers;
			allSealNumbers.Load();
			AssertType<CusExitSealCollection>(allSealNumbers);
			AssertEquals("allSealNumbers.Count", 3, allSealNumbers.Count);
			AssertCollectionContains("seal1", seal1, allSealNumbers);
			AssertCollectionContains("seal3", seal3, allSealNumbers);
		}

		public void TestCXN_Status_ResourceStringData()
		{
			(var exitContainer, _) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(exitContainer.CXN_StatusInfo, null);
			AssertNotNull(resourceStringData);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", resourceStringData.Caption);
				AssertEquals("Short Caption", "Status", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Status", resourceStringData.MediumCaption);
				AssertEquals("Full Description", "Container/Equipment Status", resourceStringData.FullDescription);
			});
		}

		public void TestCXN_SealCount_ResourceStringData()
		{
			(var exitContainer, _) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(exitContainer.CXN_SealCountInfo, null);
			AssertNotNull(resourceStringData);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Seals Quantity", resourceStringData.Caption);
				AssertEquals("Short Caption", "Seals Qty", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Seals Quantity", resourceStringData.MediumCaption);
				AssertEquals("Full Description", "Container/Equipment Seals Quantity", resourceStringData.FullDescription);
			});
		}
		
		public void TestCusSealType()
		{
			var (exitContainer, _) = GetNewBusinessObject(Factory);
			AssertType(((ICusSealTypeSupporter)exitContainer).CusSealType, exitContainer.AllSealNumbers.AddNew());
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitContainer;
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitContainer>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		public void TestLookups() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitContainer;
			AssertType<CusExitContainerLookups>("not ucc6", item.Lookups);

			item = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
			AssertType<CusExitContainerUcc6Lookups>("ucc6", item.Lookups);
		});

		public void TestValidation() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitContainer;
			AssertType<CusExitContainerValidation>("not ucc6", item.Validation);

			item = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
			AssertType<CusExitContainerValidation>("ucc6 - Validation Decider should be different, not validation class", item.Validation);
		});

		public void TestValidationDecider() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitContainer;
			AssertNull("not ucc6", item.ValidationDecider);

			item = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
			AssertType<CusExitContainerUcc6ValidationDecider>("ucc6", item.ValidationDecider);
		});

		public void TestClearStatusAndSetSealsReadOnlyIfStatusIsMissing() => CombineAssertions(() =>
		{
			var header = Factory.GetUcc6ExitHeader();
			var cusExitContainer = header.CusExitContainers.AddNew();
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
			var containerReloaded = newFactory.LoadUcc6ExitHeader(header).CusExitContainers[0];
			var seal1Reloaded = containerReloaded.AllSealNumbers[0];
			var seal2Reloaded = containerReloaded.AllSealNumbers[1];
			AssertEquals("seal1Reloaded is ReadOnly", true, seal1Reloaded.ReadOnly);
			AssertEquals("seal2Reloaded is ReadOnly", true, seal2Reloaded.ReadOnly);
		});

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).container;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public static (CusExitContainer container, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderAbstractTest<CusExitHeader>.GetNewBusinessObject(factory);
			var container = header.CusExitContainers.AddNew();
			return (container, header);
		}
	}
}
