using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public sealed class MassUpdateWizardTestHelper
	{
		public MassUpdateWizardTestHelper(BusinessObjectFactory factory)
		{
			dummy = factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();
		}

		public MassUpdateWizard Wizard
		{
			get
			{
				if (wizard == null)
				{
					wizard = new MassUpdateWizard(CollectionInfo);
				}

				return wizard;
			}
		}

		MassUpdateWizard wizard;

		public IImportCollectionInfo CollectionInfo
		{
			get
			{
				if (collectionInfo == null)
				{
					collectionInfo = new ImportCollectionInfoImpl(dummy.Collection)
					{
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Number" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Text" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Short) { HeaderText = "Short" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_AnotherDecimal) { HeaderText = "Decimal" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Date) { HeaderText = "Date" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Time) { HeaderText = "Time" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Guid, GetZ0_GuidBindToList, GetZ0_GuidModuleID) { HeaderText = "Guid" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_FK_Code, GetZ0_FK_CodeBindToList) { HeaderText = "FK_Code" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Byte) { HeaderText = "Byte" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_NVarCharMax, GetZ0_NTextFieldType, GetZ0_NTextBindToList) { HeaderText = "NText" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_DateTimeOffset) { HeaderText = "Date (With Offset)" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Geography) { HeaderText = "Geography" },
					};
				}

				return collectionInfo;
			}
		}

		public FieldType Z0_NTextFieldTypeForTesting = FieldType.Text;
		FieldType GetZ0_NTextFieldType(BusinessObject bizObj)
		{
			if (Z0_NTextFieldTypeForTesting != FieldType.Text)
			{
				return Z0_NTextFieldTypeForTesting;
			}

			var dummy = bizObj as DummyChildBusinessObject;
			if (dummy != null)
			{
				return dummy.Z0_BitFiltered ? FieldType.Guid : FieldType.Text;
			}
			return Z0_NTextFieldTypeForTesting;
		}

		public IList Z0_NTextBindToListForTesting;
		public IList GetZ0_NTextBindToList(BusinessObject bizObj)
		{
			return Z0_NTextBindToListForTesting;
		}

		public IList Z0_GuidBindToListForTesting;
		public IList GetZ0_GuidBindToList(BusinessObject bizObj)
		{
			return Z0_GuidBindToListForTesting;
		}

		public IList Z0_FK_CodeBindToListForTesting;
		public IList GetZ0_FK_CodeBindToList(BusinessObject bizObj)
		{
			return Z0_FK_CodeBindToListForTesting;
		}

		public ModuleIdentifier Z0_GuidModuleIDForTesting = ModuleIDs.NotAssigned;
		public ModuleIdentifier GetZ0_GuidModuleID(BusinessObject bizObj)
		{
			return Z0_GuidModuleIDForTesting;
		}

		IImportCollectionInfo collectionInfo;
		readonly DummyBusinessObject dummy;
	}
}
