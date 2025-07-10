using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CFXAccountDataType))]
	class CFXAccountDataTypeTest : RegistryDataTypeTestCase<CFXAccountDataType>
	{
		protected override CFXAccountDataType GetNewDataType()
		{
			return new CFXAccountDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new Guid("4D6A4E25-CDF5-4C87-9CBC-A8D8FBCA7793");
			var second = new Guid("E43085C0-F7FA-4394-A69F-9C6FD38AC6D2");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(first.ToString())),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(second.ToString()))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { Guid.Empty };
		}

		public void TestIsCFXTransactionsExist()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var journal1 = Factory.New<JCJournalHeader>();
			journal1.AH_GC = GlbCompany.CurrentCompany.PK;
			journal1.AH_GE = department1.PK;
			var journal1line = journal1.Lines.AddNew();
			journal1line.AL_GE = department2.PK;
			journal1line.AL_AG = Creator.GLHeader1.PK;

			var journal2 = Factory.New<JCJournalHeader>();
			journal2.AH_GB = Creator.NonCurrentCompany.Branches[0].PK;
			journal2.AH_GE = department2.PK;
			var journal2line = journal2.Lines.AddNew();
			journal2line.AL_GE = department1.PK;
			journal2line.AL_GB = Creator.NonCurrentCompany.Branches[0].PK;
			journal2line.AL_AG = Creator.GLHeader1.PK;

			Factory.Save();

			Assert("There are no CFX transaction lines posted for current company and department 1", !new CFXAccountDataType().IsCFXTransactionsExist(GlbCompany.CurrentCompany.PK.ToGuid(), department1.PK.ToGuid()));
			Assert("There are CFX transaction lines posted for current company and department 2, but not flagged as 'posted to GL'", !new CFXAccountDataType().IsCFXTransactionsExist(GlbCompany.CurrentCompany.PK.ToGuid(), department2.PK.ToGuid()));

			Assert("There are CFX transaction lines posted for non current company and department 1, but not flagged as 'posted to GL'", !new CFXAccountDataType().IsCFXTransactionsExist(Creator.NonCurrentCompany.PK.ToGuid(), department1.PK.ToGuid()));
			Assert("There are no CFX transaction lines posted for non current company and department 2", !new CFXAccountDataType().IsCFXTransactionsExist(Creator.NonCurrentCompany.PK.ToGuid(), department2.PK.ToGuid()));

			journal1.AH_PostToGL = ZBool.True.ToString();
			journal2.AH_PostToGL = ZBool.True.ToString();
			Factory.Save();

			Assert("There are no CFX transaction lines posted for current company and department 1", !new CFXAccountDataType().IsCFXTransactionsExist(GlbCompany.CurrentCompany.PK.ToGuid(), department1.PK.ToGuid()));
			Assert("There are CFX transaction lines posted for current company and department 2", new CFXAccountDataType().IsCFXTransactionsExist(GlbCompany.CurrentCompany.PK.ToGuid(), department2.PK.ToGuid()));

			Assert("There are CFX transaction lines posted for non current company and department 1", new CFXAccountDataType().IsCFXTransactionsExist(Creator.NonCurrentCompany.PK.ToGuid(), department1.PK.ToGuid()));
			Assert("There are no CFX transaction lines posted for non current company and department 2", !new CFXAccountDataType().IsCFXTransactionsExist(Creator.NonCurrentCompany.PK.ToGuid(), department2.PK.ToGuid()));
		}

		public void TestValidateNotAllowedForDissectionAttributes()
		{
			var cfxRegistry = new CFXAccountRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, Guid.NewGuid());
			var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = Chart.PK;
			dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			Factory.Save();
			AssertExceptionThrown(typeof(RegistryValidationException),
								"GL Accounts with Dissections cannot be selected",
								() => DataType.Validate(cfxRegistry, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			GLHeader.AlternateGLAccountDissections.RemoveAndDeleteAll();
			Factory.Save();
			AssertNoExceptionThrown(() => DataType.Validate(cfxRegistry, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidateNotAllowedGLHeaderWhichItsAlternateGLAccountIsMappedByMultipleGLAccountsSetToControlAccount()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "11", Core.Constants.AccountType.BalanceSheetAccount);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			var cfxRegistry = new CFXAccountRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, Guid.NewGuid());
			Factory.Save();
			AssertNoExceptionThrown(() => DataType.Validate(cfxRegistry, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, Creator.CreateGLHeader().PK, attribute: "");
			Factory.Save();
			AssertExceptionThrown(typeof(RegistryValidationException),
								$"You cannot select this GL Account Number '{GLHeader.AG_AccountNum}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.",
								() => DataType.Validate(cfxRegistry, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return true; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
			Creator = new TestObjectCreator(Factory);
			Chart = Creator.CreateAlternateChart("CH1");
			GLHeader = Creator.GLHeader1;
			Factory.Save();
		}

		BusinessObjectFactory Factory;

		TestObjectCreator Creator;

		AccAlternateChart Chart;

		AccGLHeader GLHeader;
	}
}
