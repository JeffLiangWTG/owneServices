using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectReaderTest
	{
		public void TestImportingJPAFRContainerData()
		{
			var containerDataObject = SetupContainer("CONT323423");

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var reader = new JPAFRContainerDataObjectReader(containerDataObject, logger, Factory, bill);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of header Business Object

			CombineAssertions(delegate
			{
				AssertJPAFRContainerContents(containerBO, "CONT323423");
				AssertEquals("JPC_JPB_Bill", bill.PK, containerBO.JPC_JPB_Bill);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching JPAFRContainer found, creating new JPAFRContainer.
Information - Populating JPAFRContainer...
Information - Successfully loaded matching Container Type.
".Trim(), logger.Logs);
			});

			#endregion
		}
	}

	partial class JPManifestDataObjectReaderTestHelper
	{
		#region Implementation

		protected AddInfo AddAddInfo(IAddInfoCollectionParent addInfosParent, ZString? key, ZString? value)
		{
			var addInfo = new AddInfo() { Key = key, Value = value };
			addInfosParent.SetAddInfoCollection(() => addInfosParent.AddInfoCollection.AddSafe(addInfo));
			return addInfo;
		}

		protected Container SetupContainer(ZString? containerNumber, ZString? sealNumber, ZString? secondSealNumber, ZBool? isEmptyContainer, ContainerType containerType, ZString? ownershipCode)
		{
			var result = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNumber,
				Seal = sealNumber,
				SecondSeal = secondSealNumber,
				IsEmptyContainer = isEmptyContainer,
				ContainerType = containerType
			};

			if (ownershipCode.HasValue)
			{
				AddAddInfo(result, AddInfoConstants.Container.ContainerOwnershipCode, ownershipCode);
			}

			return result;
		}

		protected Container SetupContainer(ZString? containerNumber)
		{
			return SetupContainer(containerNumber, "SEAL1", "SEAL2", ZBool.False, ContainerType1, "1");
		}

		protected Container SetupContainer2(ZString? containerNumber)
		{
			return SetupContainer(containerNumber, "SEAL4", "SEAL5", ZBool.True, ContainerType2, "2");
		}

		protected ContainerType ContainerType1
		{
			get
			{
				if (containerType1 == null)
				{
					containerType1 = new ContainerType()
					{
						Code = ContainerTypeBO1.RC_Code
					};
				}
				return containerType1;
			}
		}
		ContainerType containerType1;

		protected ContainerType ContainerType2
		{
			get
			{
				if (containerType2 == null)
				{
					containerType2 = new ContainerType()
					{
						Code = ContainerTypeBO2.RC_Code
					};
				}
				return containerType2;
			}
		}
		ContainerType containerType2;

		protected void AssertJPAFRContainerContents(JPAFRContainer billBO, ZString containerNumber)
		{
			AssertJPAFRContainerContents(billBO, containerNumber, "SEAL1", "SEAL2", ZBool.False, ContainerTypeBO1.PK, "1");
		}

		protected void AssertJPAFRContainerContents2(JPAFRContainer billBO, ZString containerNumber)
		{
			AssertJPAFRContainerContents(billBO, containerNumber, "SEAL4", "SEAL5", ZBool.True, ContainerTypeBO2.PK, "2");
		}

		protected void AssertJPAFRContainerContents(JPAFRContainer containerBO, ZString containerNumber, ZString seal1, ZString seal2, ZBool isEmpty, ZGuid containerTypePK, ZString onwershipCode)
		{
			AssertEquals("containerBO.JPC_HAWB", containerNumber, containerBO.JPC_ContainerNum);
			AssertEquals("containerBO.JPC_Seal1", seal1, containerBO.JPC_Seal1);
			AssertEquals("containerBO.JPC_Seal2", seal2, containerBO.JPC_Seal2);
			AssertEquals("containerBO.JPC_IsEmpty", isEmpty, containerBO.JPC_IsEmpty);
			AssertEquals("containerBO.JPC_RC_ContainerType", containerTypePK, containerBO.JPC_RC_ContainerType);
			AssertEquals("containerBO.JPC_OwnershipCode", onwershipCode, containerBO.JPC_OwnershipCode);
		}

		protected RefContainer ContainerTypeBO1
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

		protected RefContainer ContainerTypeBO2
		{
			get
			{
				if (containerTypeBO2 == null)
				{
					containerTypeBO2 = Factory.New<RefContainer>();
					containerTypeBO2.RC_Code = "K77G";
					containerTypeBO2.RC_Description = "CONTAINER 2";
					containerTypeBO2.RC_ContainerType = Core.Constants.ContainerTypes.OpenTop;
					containerTypeBO2.RC_Length = 20.000m;
					containerTypeBO2.RC_Height = 8.500m;
					containerTypeBO2.RC_Width = 8.000m;
				}
				return containerTypeBO2;
			}
		}
		RefContainer containerTypeBO2;

		#endregion
	}
}
