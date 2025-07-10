using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingReceivingFileTypeConfiguration))]
	public class EInvoicingReceivingFileTypeConfigurationTest : RegistryBusinessObjectTemplateTestCase<EInvoicingReceivingFileTypeConfiguration>
	{
		public void TestFileFormatList()
		{
			AssertEquals("FileFormatList count", 2, BizObj.FileFormatList.Count);
			AssertEquals("should contain OFD", true, BizObj.FileFormatList.ContainsCode("OFD"));
			AssertEquals("should contain XML", true, BizObj.FileFormatList.ContainsCode("XML"));
		}

		public void TestDebtorTypeList()
		{
			AssertEquals("DebtorTypeList count", 3, BizObj.DebtorTypeList.Count);
			AssertEquals("should contain ALL", true, BizObj.DebtorTypeList.ContainsCode("ALL"));
			AssertEquals("should contain GRP", true, BizObj.DebtorTypeList.ContainsCode(Core.Constants.DebtorTypes.Code.DebtorGroup));
			AssertEquals("should contain ORG", true, BizObj.DebtorTypeList.ContainsCode(Core.Constants.DebtorTypes.Code.DebtorOrganisation));
		}

		public void TestDebtorCode()
		{
			AssertNotEquals("Precondition", TestObjectCreator.ABIGAS.PK, BizObj.DebtorCode);
			BizObj.DebtorCode = TestObjectCreator.ABIGAS.PK;
			AssertEquals(TestObjectCreator.ABIGAS.PK, BizObj.DebtorCode);
		}

		public void TestDebtorCodeReadOnly()
		{
			BizObj.DebtorType = "ALL";
			Assert(BizObj.DebtorCodeInfo.ReadOnly);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup;
			Assert(!BizObj.DebtorCodeInfo.ReadOnly);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			Assert(!BizObj.DebtorCodeInfo.ReadOnly);
		}

		public void TestValidateFileFormat()
		{
			BizObj.FileFormat = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.FileFormat);
			BizObj.ValidateFileFormat();
			AssertHasError(BizObj.FileFormatInfo, "Please enter a value.");

			BizObj.FileFormat = "XXX";
			Assert("Precondition", !BizObj.FileFormatList.ContainsCode(BizObj.FileFormat));
			BizObj.ValidateFileFormat();
			AssertHasError(BizObj.FileFormatInfo, "Enter a valid selection.");

			BizObj.FileFormat = "OFD";
			Assert("Precondition", BizObj.FileFormatList.ContainsCode(BizObj.FileFormat));
			BizObj.ValidateFileFormat();
			AssertNoErrors(BizObj.FileFormatInfo);

			BizObj.FileFormat = "XML";
			Assert("Precondition", BizObj.FileFormatList.ContainsCode(BizObj.FileFormat));
			BizObj.ValidateFileFormat();
			AssertNoErrors(BizObj.FileFormatInfo);
		}

		public void TestValidateFileFormat_HasAllConfiguration()
		{
			var collection = new EInvoicingReceivingFileTypeConfigurationCollection();
			var config = collection.AddNew();
			config.FileFormat = "XML";
			config.DebtorType = "ALL";

			var config1 = collection.AddNew();
			config1.FileFormat = "XML";
			config1.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			config1.DebtorCode = TestObjectCreator.Debtor.PK;

			AssertEquals(2, collection.Count);

			collection.RunPreSaveValidation();
			AssertHasError(config1.FileFormatInfo, "A 'ALL' configuration already exists for file format 'XML'");
			AssertHasError(config1.DebtorTypeInfo, "A 'ALL' configuration already exists for file format 'XML'");
		}

		public void TestValidateDebtorType()
		{
			BizObj.DebtorType = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.DebtorType);
			BizObj.ValidateDebtorType();
			AssertHasError(BizObj.DebtorTypeInfo, "Please enter a value.");

			BizObj.DebtorType = "XXXX";
			Assert("Precondition", !BizObj.DebtorTypeList.ContainsCode(BizObj.DebtorType));
			BizObj.ValidateDebtorType();
			AssertHasError(BizObj.DebtorTypeInfo, "Enter a valid selection.");

			BizObj.DebtorType = "ALL";
			Assert("Precondition", BizObj.DebtorTypeList.ContainsCode(BizObj.DebtorType));
			BizObj.ValidateDebtorType();
			AssertNoErrors(BizObj.DebtorTypeInfo);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup;
			Assert("Precondition", BizObj.DebtorTypeList.ContainsCode(BizObj.DebtorType));
			BizObj.ValidateDebtorType();
			AssertNoErrors(BizObj.DebtorTypeInfo);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			Assert("Precondition", BizObj.DebtorTypeList.ContainsCode(BizObj.DebtorType));
			BizObj.ValidateDebtorType();
			AssertNoErrors(BizObj.DebtorTypeInfo);
		}

		public void TestValidateDebtorCode()
		{
			BizObj.DebtorCode = ZGuid.Empty;
			AssertEquals("Precondition", ZGuid.Empty, BizObj.DebtorCode);
			BizObj.ValidateDebtorCode();
			AssertHasError(BizObj.DebtorCodeInfo, "Please enter a value.");

			BizObj.DebtorType = "ALL";
			BizObj.DebtorCode = ZGuid.Empty;
			BizObj.ValidateDebtorCode();
			AssertNoErrors(BizObj.DebtorCodeInfo);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			BizObj.DebtorCode = ZGuid.Empty;
			BizObj.ValidateDebtorCode();
			AssertHasError(BizObj.DebtorCodeInfo, "Please enter a value.");

			BizObj.DebtorCode = TestObjectCreator.Debtor.PK;
			BizObj.ValidateDebtorCode();
			AssertNoErrors(BizObj.DebtorCodeInfo);

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup;
			BizObj.DebtorCode = ZGuid.Empty;
			BizObj.ValidateDebtorCode();
			AssertHasError(BizObj.DebtorCodeInfo, "Please enter a value.");
		}

		public void TestValidateDebtorCode_DebtorBelongsToDebtorGroup()
		{
			var debtorGroup = TestObjectCreator.CreateDebtorGroup();
			TestObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			Factory.Save();

			var collection = new EInvoicingReceivingFileTypeConfigurationCollection();
			var config = collection.AddNew();
			config.FileFormat = "OFD";
			config.DebtorType = Core.Constants.DebtorTypes.Code.DebtorGroup;
			config.DebtorCode = debtorGroup.PK;

			var config1 = collection.AddNew();
			config1.FileFormat = "OFD";
			config1.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			config1.DebtorCode = TestObjectCreator.Debtor.PK;

			collection.RunPreSaveValidation();
			AssertHasError(config1.DebtorCodeInfo, $"Debtor '{TestObjectCreator.Debtor.OH_Code}' is belong to the debtor group '{debtorGroup.OJ_Code}'.");
		}

		public void TestValidateUniqueConfiguration()
		{
			var collection = new EInvoicingReceivingFileTypeConfigurationCollection();
			var config = collection.AddNew();
			config.FileFormat = "XML";
			config.DebtorType = "ALL";

			var config1 = collection.AddNew();
			config1.FileFormat = "XML";
			config1.DebtorType = "ALL";

			AssertEquals(2, collection.Count);

			collection.RunPreSaveValidation();
			AssertHasRowError(config, "Configuration must be unique.");
			AssertHasRowError(config1, "Configuration must be unique.");
		}

		public void TestDebtorCodes()
		{
			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			AssertEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			Assert(BizObj.DebtorCodes is DebtorCollection);

			AssertDebtorCodesForDebtorOrganisation(Core.Constants.DebtorTypes.Code.DebtorGroup);
			AssertDebtorCodesForDebtorOrganisation("XXX");
			AssertDebtorCodesForDebtorOrganisation(ZString.Empty);
		}

		void AssertDebtorCodesForDebtorOrganisation(ZString debtorType)
		{
			BizObj.DebtorType = debtorType;
			AssertNotEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			Assert(BizObj.DebtorCodes is OrgDebtorGroupCollection);
		}

		public void TestDebtor()
		{
			var debtorGroup = TestObjectCreator.CreateDebtorGroup();
			Factory.Save();

			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			AssertEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			BizObj.DebtorCode = TestObjectCreator.AALSHI.PK;
			AssertEquals(TestObjectCreator.AALSHI.PK, BizObj.Debtor.PK);

			BizObj.DebtorCode = debtorGroup.PK;
			AssertNull(BizObj.Debtor);

			BizObj.DebtorCode = ZGuid.Invalid;
			AssertNull(BizObj.Debtor);

			BizObj.DebtorCode = ZGuid.Empty;
			AssertNull(BizObj.Debtor);

			AssertDebtorForDebtorOrganisation(Core.Constants.DebtorTypes.Code.DebtorGroup);
			AssertDebtorForDebtorOrganisation("XXX");
			AssertDebtorForDebtorOrganisation(ZString.Empty);
		}

		void AssertDebtorForDebtorOrganisation(ZString debtorType)
		{
			var debtorGroup = TestObjectCreator.CreateDebtorGroup();
			Factory.Save();

			BizObj.DebtorType = debtorType;
			AssertNotEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			BizObj.DebtorCode = debtorGroup.PK;
			AssertEquals(debtorGroup.PK, BizObj.Debtor.PK);

			BizObj.DebtorCode = TestObjectCreator.AALSHI.PK;
			AssertNull(BizObj.Debtor);

			BizObj.DebtorCode = ZGuid.Invalid;
			AssertNull(BizObj.Debtor);

			BizObj.DebtorCode = ZGuid.Empty;
			AssertNull(BizObj.Debtor);
		}

		public void TestDebtorDescription()
		{
			BizObj.DebtorType = Core.Constants.DebtorTypes.Code.DebtorOrganisation;
			AssertEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			BizObj.DebtorCode = TestObjectCreator.AALSHI.PK;
			AssertNotNull("Precondition", BizObj.Debtor);
			AssertEquals("Precondition", TestObjectCreator.AALSHI.PK, BizObj.Debtor.PK);
			AssertEquals(TestObjectCreator.AALSHI.OH_FullName, BizObj.DebtorDescription);

			BizObj.DebtorCode = ZGuid.Invalid;
			AssertNull("Precondition", BizObj.Debtor);
			AssertNullOrEmpty(BizObj.DebtorDescription);

			AssertDebtorDescriptionForDebtorOrganisation(Core.Constants.DebtorTypes.Code.DebtorGroup);
			AssertDebtorDescriptionForDebtorOrganisation("XXX");
			AssertDebtorDescriptionForDebtorOrganisation(ZString.Empty);
		}

		void AssertDebtorDescriptionForDebtorOrganisation(ZString debtorType)
		{
			var debtorGroup = TestObjectCreator.CreateDebtorGroup();
			Factory.Save();

			BizObj.DebtorType = debtorType;
			AssertNotEquals("Precondition", Core.Constants.DebtorTypes.Code.DebtorOrganisation, BizObj.DebtorType);
			BizObj.DebtorCode = debtorGroup.PK;
			AssertNotNull("Precondition", BizObj.Debtor);
			AssertEquals("Precondition", debtorGroup.PK, BizObj.Debtor.PK);
			AssertEquals(debtorGroup.OJ_Desc, BizObj.DebtorDescription);

			BizObj.DebtorCode = ZGuid.Invalid;
			AssertNull("Precondition", BizObj.Debtor);
			AssertNullOrEmpty(BizObj.DebtorDescription);
		}

		#region Implementation

		protected override EInvoicingReceivingFileTypeConfiguration GetBusinessObjectToClone()
		{
			return new EInvoicingReceivingFileTypeConfiguration();
		}

		protected override EInvoicingReceivingFileTypeConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
