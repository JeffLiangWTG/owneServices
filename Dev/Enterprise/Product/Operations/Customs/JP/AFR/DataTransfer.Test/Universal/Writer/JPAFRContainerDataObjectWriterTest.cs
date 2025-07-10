using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectWriterTest
	{
		public void TestJPAFRContainerMappings_NVOCC()
		{
			CreateJapanPackageTypes();
			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			var bill = SetupJPAFRBills(header.Bills.AddNew(), "HB3243");
			SetupJPAFRBillsVOCCFields(bill, true, "COC1", "GTN1");
			var container = SetupJPAFRContainer(bill.Containers.AddNew(), "CONT234");
			SetupJPAFRContainerVOCCFields(container, "53", "1", "t", "A");
			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertEquals("headerData.SubShipmentCollection.Count", 1, headerData.SubShipmentCollection.Count);
			var billData = headerData.SubShipmentCollection[0];
			AssertAFRBillContents(billData, "HB3243", false);
			AssertNotNull("billData.ContainerCollection", billData.ContainerCollection);
			AssertEquals("billData.ContainerCollection.Count", 1, billData.ContainerCollection.Count);
			var containerData = billData.ContainerCollection[0];
			AssertAFRContainerContents(containerData, "CONT234", false);
		}

		public void TestJPAFRContainerMappings_VOCC()
		{
			CreateJapanPackageTypes();
			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			header.JPH_IsShippingLineEntry = true;
			var bill = SetupJPAFRBills(header.Bills.AddNew(), "HB3243");
			SetupJPAFRBillsVOCCFields(bill, true, "COC1", "GTN1");
			var container = SetupJPAFRContainer(bill.Containers.AddNew(), "CONT234");
			SetupJPAFRContainerVOCCFields(container, "53", "1", "t", "A");
			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertEquals("headerData.SubShipmentCollection.Count", 1, headerData.SubShipmentCollection.Count);
			var billData = headerData.SubShipmentCollection[0];
			AssertAFRBillContents(billData, "HB3243", true);
			AssertNotNull("billData.ContainerCollection", billData.ContainerCollection);
			AssertEquals("billData.ContainerCollection.Count", 1, billData.ContainerCollection.Count);
			var containerData = billData.ContainerCollection[0];
			AssertAFRContainerContents(containerData, "CONT234", true);
		}

		void AssertAFRContainerContents(Container containerData, ZString? containerNumber, bool isShippingLineEntry)
		{
			AssertAFRContainerContents(containerData, containerNumber, "S12", "S54", ZBool.False, CodeDescriptionPairForTesting.New(ContainerTypeBO1.RC_Code, ContainerTypeBO1.RC_Description), ContainerTypeBO1.RC_Height, ContainerTypeBO1.RC_Width, ContainerTypeBO1.RC_Length, "B", CodeDescriptionPairForTesting.New(Core.Constants.ContainerTypes.Refrigerated, Core.Constants.ContainerTypeDescriptions.Refrigerated));
			if (isShippingLineEntry)
			{
				AssertAFRContainerVOCCContents(containerData, "53", "1", "t", "A");
			}
			else
			{
				AssertAFRContainerVOCCContents(containerData, null, null, null, null);
			}
		}

		void AssertAFRContainerContents(Container containerData, ZString? containerNumber, ZString? seal1, ZString? seal2, ZBool? isEmpty, ICodeDescription containerType, ZDecimal? totalHeight, ZDecimal? totalWidth, ZDecimal? totalLength, ZString? onwershipCode, ICodeDescription containerTypeCategory)
		{
			AssertNotNull("Precondition: containerData", containerData);

			CombineAssertions(delegate
			{
				AssertEquals("containerData.ContainerNumber", containerNumber, containerData.ContainerNumber);
				AssertEquals("containerData.Seal", seal1, containerData.Seal);
				AssertEquals("containerData.SecondSeal", seal2, containerData.SecondSeal);
				AssertEquals("containerData.IsEmptyContainer", isEmpty, containerData.IsEmptyContainer);
				AssertEquals("containerData.IsEmptyContainer", isEmpty, containerData.IsEmptyContainer);
				AssertNotNull("containerData.ContainerType", containerData.ContainerType);
				AssertEquals("containerData.ContainerType.Code", containerType.Code, containerData.ContainerType.Code);
				AssertEquals("containerData.ContainerType.Description", containerType.Description, containerData.ContainerType.Description);
				AssertNotNull("containerData.ContainerType.Category", containerData.ContainerType.Category);
				AssertEquals("containerData.ContainerType.Category.Code", containerTypeCategory.Code, containerData.ContainerType.Category.Code);
				AssertEquals("containerData.ContainerType.Category.Description", containerTypeCategory.Description, containerData.ContainerType.Category.Description);
				AssertEquals("containerData.TotalHeight", totalHeight, containerData.TotalHeight);
				AssertEquals("containerData.TotalWidth", totalWidth, containerData.TotalWidth);
				AssertEquals("containerData.TotalLength", totalLength, containerData.TotalLength);
				AssertNotNull("containerData.AddInfoCollection", containerData.AddInfoCollection);
				AssertEquals("containerData.AddInfoCollection.GetZStringValue(Constants.Container.ContainerOwnershipCode)", onwershipCode, containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerOwnershipCode));
			});
		}

		void AssertAFRContainerVOCCContents(Container containerData, ZString? typeOfService, ZString? vanningType, ZString? cCCApplicationId, ZString? searchExclusionId)
		{
			AssertNotNull("Precondition: containerData", containerData);
			CombineAssertions(() =>
			{
				AssertNotNull("containerData.AddInfoCollection", containerData.AddInfoCollection);
				AssertEquals("containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerTypeOfService)", typeOfService, containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerTypeOfService));
				AssertEquals("containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerVanningType)", vanningType, containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerVanningType));
				AssertEquals("containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerCCCApplicationId)", cCCApplicationId, containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerCCCApplicationId));
				AssertEquals("containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerSearchExclusionId)", searchExclusionId, containerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerSearchExclusionId));
			});
		}

		void CreateJapanPackageTypes()
		{
			var factory = new BusinessObjectFactory();
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(factory);
			universalReferenceTestHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "Package types");
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BA", "Barrel", startDate, endDate);
			factory.Save();
		}

		JPAFRContainer SetupJPAFRContainer(JPAFRContainer container, ZString containerNumber)
		{
			return SetupJPAFRContainer(container, containerNumber, "S12", "S54", ZBool.False, ContainerTypeBO1.PK, "B");
		}

		JPAFRContainer SetupJPAFRContainer(JPAFRContainer container, ZString containerNumber, ZString seal1, ZString seal2, ZBool isEmpty, ZGuid containerTypePK, ZString onwershipCode)
		{
			container.JPC_ContainerNum = containerNumber;
			container.JPC_Seal1 = seal1;
			container.JPC_Seal2 = seal2;
			container.JPC_IsEmpty = isEmpty;
			container.JPC_RC_ContainerType = containerTypePK;
			container.JPC_OwnershipCode = onwershipCode;
			return container;
		}

		JPAFRContainer SetupJPAFRContainerVOCCFields(JPAFRContainer container, ZString typeOfService, ZString vanningType, ZString cCCApplicationId, ZString searchExclusionId)
		{
			container.JPC_TypeOfService = typeOfService;
			container.JPC_VanningType = vanningType;
			container.JPC_CCCApplicationId = cCCApplicationId;
			container.JPC_SearchExclusionId = searchExclusionId;
			return container;
		}

		RefContainer ContainerTypeBO1
		{
			get
			{
				if (containerTypeBO1 == null)
				{
					containerTypeBO1 = Factory.New<RefContainer>();
					containerTypeBO1.RC_Code = "Z12S";
					containerTypeBO1.RC_Description = "CONTAINER 1";
					containerTypeBO1.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
					containerTypeBO1.RC_Length = 40.000m;
					containerTypeBO1.RC_Height = 8.500m;
					containerTypeBO1.RC_Width = 8.000m;
				}
				return containerTypeBO1;
			}
		}
		RefContainer containerTypeBO1;
	}
}
