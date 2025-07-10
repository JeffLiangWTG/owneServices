using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public class ImportWizardTestHelper
	{
		public ImportWizardTestHelper(BusinessObjectFactory factory)
		{
			dummy = factory.New<DummyBusinessObjectWithForeignKey>();
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; set; }

		public ImportWizard Wizard
		{
			get
			{
				if (wizard == null)
				{
					var settingsStorageStub = new Mock<ISettingsStorage>();
					settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(System.Array.Empty<string>());

					wizard = new ImportWizard(CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
				}

				return wizard;
			}
		}

		ImportWizard wizard;

		public virtual IImportCollectionInfo GetCollectionInfo()
		{
			ImportPropertyInfoImpl.GetBindToListDelegate getListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToList);
			return new ImportCollectionInfoImpl(dummy.Collection)
					{
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Num" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Txt" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Short) { HeaderText = "Short" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_AnotherDecimal) { HeaderText = "Decimal" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Date) { HeaderText = "Date" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Description,getListDelegate) { HeaderText = "description" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_NVarCharMax) { HeaderText = "NTxt" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_AnotherNumber) { HeaderText = "AnotherNumber" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Guid, true) { HeaderText = "Guid" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_DateTimeOffset) { HeaderText = "DateTimeOffset" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Geography) { HeaderText = "Geography" },
						new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_DateOnly) { HeaderText = "DateOnly" },
					};
		}

		IList GetBindToList(BusinessObject bizObj)
		{
			DummyChildBusinessObjectCollection list = new DummyChildBusinessObjectCollection(Factory);
			DummyChildBusinessObject item1 = list.AddNew();
			item1.Z0_Description = "description1";
			item1.Z0_Code = "code1";
			DummyChildBusinessObject item2 = list.AddNew();
			item2.Z0_Description = "description2";
			item2.Z0_Code = "code2";
			return list;
		}

		public IImportCollectionInfo CollectionInfo
		{
			get
			{
				if (collectionInfo == null)
				{
					collectionInfo = GetCollectionInfo();
				}
				return collectionInfo;
			}
		}

		IImportCollectionInfo collectionInfo;
		readonly DummyBusinessObjectWithForeignKey dummy;
	}
}
