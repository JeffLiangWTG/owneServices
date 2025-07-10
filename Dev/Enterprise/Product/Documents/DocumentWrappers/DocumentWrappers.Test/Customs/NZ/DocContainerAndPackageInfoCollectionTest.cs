using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocContainerAndPackageInfoCollection))]
	sealed class DocContainerAndPackageInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocContainerAndPackageInfoCollection>
	{
		protected override DocContainerAndPackageInfoCollection GetCollectionToTest()
		{
			return new DocContainerAndPackageInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocContainerAndPackageInfo(Factory, "", "", "", "", "0");
		}

		public void TestInstantiationAndEverything()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			DocContainerAndPackageInfoCollection packInfos = new DocContainerAndPackageInfoCollection(declaration);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_HouseBill = "MYHOUSE1";
			declaration.JE_TotalNoOfPacks = 33;
			packInfos.Load();
			AssertEquals(1, packInfos.Count);
			DocContainerAndPackageInfo packInfo = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo.BillNumber);
			AssertEquals("", packInfo.ContainerNumber);
			AssertEquals("", packInfo.ContainerStatus);
			AssertEquals("33 PCS", packInfo.PackagesAndType);

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONTAINER1";
			container1.CO_FCL_LCL_AIR = "LCL";
			packInfos.Load();
			AssertEquals(1, packInfos.Count);
			packInfo = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo.BillNumber);
			AssertEquals("", packInfo.ContainerNumber);
			AssertEquals("", packInfo.ContainerStatus);
			AssertEquals("33 PCS", packInfo.PackagesAndType);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			packInfos.Load();
			AssertEquals(1, packInfos.Count);
			packInfo = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo.BillNumber);
			AssertEquals("CONTAINER1", packInfo.ContainerNumber);
			AssertEquals("LCL", packInfo.ContainerStatus);
			AssertEquals("33 PCS", packInfo.PackagesAndType);

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONTAINER2";
			container2.CO_FCL_LCL_AIR = "FCL";
			packInfos.Load();
			AssertEquals(3, packInfos.Count);
			packInfo = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo.BillNumber);
			AssertEquals("", packInfo.ContainerNumber);
			AssertEquals("", packInfo.ContainerStatus);
			AssertEquals("33 PCS", packInfo.PackagesAndType);
			packInfo = packInfos[1];
			AssertEquals("", packInfo.BillNumber);
			AssertEquals("CONTAINER1", packInfo.ContainerNumber);
			AssertEquals("LCL", packInfo.ContainerStatus);
			AssertEquals("0", packInfo.PackagesAndType);
			packInfo = packInfos[2];
			AssertEquals("", packInfo.BillNumber);
			AssertEquals("CONTAINER2", packInfo.ContainerNumber);
			AssertEquals("FCL", packInfo.ContainerStatus);
			AssertEquals("0", packInfo.PackagesAndType);
		}
	}
}
