using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	abstract class GeneralLedgerRegistryDataTypeTest : RegistryDataTypeTestCase<GeneralLedgerRegistryDataType>
	{
		[ExpectNoExceptions]
		public void TestValidateSameValue()
		{
			foreach (var headerLineCauseValidationError in HeaderLinesCauseValidationError)
			{
				ParentRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccGLHeaderPK);
				InsertHeaderLineToDB(headerLineCauseValidationError);
				DataType.Validate(ParentRegistryItem, AccGLHeaderPK, Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public void TestValidateDifferentValue()
		{
			ParentRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccGLHeaderPK);
			InsertDBRowsToNotCauseValidationError();
			AssertNoExceptionThrown(() => DataType.Validate(ParentRegistryItem, Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty));

			foreach (var headerLineCauseValidationError in HeaderLinesCauseValidationError)
			{
				InsertHeaderLineToDB(headerLineCauseValidationError);
				AssertExceptionThrown(typeof(RegistryValidationException),
									"This Registry item cannot be changed because an accounting transaction has already been posted to General Ledger. Please revert the value back to the original value (1234.12.34).",
									() => DataType.Validate(ParentRegistryItem, Guid.NewGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestValidateEmptyValue()
		{
			ParentRegistryItem.Options = RegistryOptions.IsValueMandatory;
			AssertExceptionThrown(typeof(RegistryValidationException), delegate
			{ DataType.Validate(ParentRegistryItem, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty); });
		}

		public void TestValidateNotAllowedForSeparateNumbering()
		{
			SetupForAlternateGLAccount();
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumberingRegistry(It.IsAny<IRegistryItem>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
				dissection.ADC_AAC_AlternateChart = Chart.PK;
				dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
				dissection.ADC_SeparateNumbering = true;
				Factory.Save();
				AssertExceptionThrown(typeof(RegistryValidationException),
									"GL Accounts with Dissections ticked for Separate Numbering cannot be selected",
									() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
				dissection.ADC_SeparateNumbering = false;
				Factory.Save();
				AssertNoExceptionThrown(() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestValidateNotAllowedForDissectionAttributes()
		{
			SetupForAlternateGLAccount();
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForDissectionAttributesRegistry(It.IsAny<IRegistryItem>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
				dissection.ADC_AAC_AlternateChart = Chart.PK;
				dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
				Factory.Save();
				AssertExceptionThrown(typeof(RegistryValidationException),
									"GL Accounts with Dissections cannot be selected",
									() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
				GLHeader.AlternateGLAccountDissections.RemoveAndDeleteAll();
				Factory.Save();
				AssertNoExceptionThrown(() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestValidateNotAllowedGLHeaderWhoseAlternateGLAccountIsMappedByMultipleGLAccountsSetToControlAccount()
		{
			SetupForAlternateGLAccount();
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "11", Core.Constants.AccountType.BalanceSheetAccount);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();

			AssertNoExceptionThrown(() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, Creator.CreateGLHeader().PK, attribute: "");
			Factory.Save();
			AssertExceptionThrown(typeof(RegistryValidationException),
								$"You cannot select this GL Account Number '{GLHeader.AG_AccountNum}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.",
								() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidationNotAllowedARAPControlAccountSetSPRAttribute()
		{
			SetupForAlternateGLAccount();
			var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = Chart.PK;
			dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			Factory.Save();

			if (CannotContainSPRDissection)
			{
				AssertExceptionThrown(typeof(RegistryValidationException),
									$"{ParentRegistryItem.Caption} cannot have SPR dissection.",
									() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			}
			else
			{
				AssertNoExceptionThrown(() => DataType.Validate(ParentRegistryItem, GLHeader.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		void SetupForAlternateGLAccount()
		{
			Factory = new BusinessObjectFactory();
			Creator = new TestObjectCreator(Factory);
			Chart = Creator.CreateAlternateChart("CH1");
			GLHeader = Creator.GLHeader1;
			Factory.Save();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			AccGLHeaderPK = Guid.NewGuid();
			InsertAccGLHeaderRow(AccGLHeaderPK);
		}

		BusinessObjectFactory Factory;

		TestObjectCreator Creator;

		AccAlternateChart Chart;

		AccGLHeader GLHeader;

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid newGuid = Guid.NewGuid();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Guid.Empty, Encoding.Unicode.GetBytes(Guid.Empty.ToString())),
				new ValidSampleAndBinaryValueInDB(newGuid, Encoding.Unicode.GetBytes(newGuid.ToString()))
			};
		}

		protected void InsertHeaderLineToDB(HeaderLine headerLine)
		{
			if (headerLine.Header != null)
			{
				InsertAccTransactionHeader(headerLine.Header);
			}
			if (headerLine.Line != null)
			{
				InsertAccTransactionLine(headerLine.Line);
				if (headerLine.Line.IsInsertAccCashBasisVAT)
				{
					InsertAccCashBasisVAT(headerLine.Line.PK, true);
				}
			}
		}

		protected void InsertDBRowsToNotCauseValidationError()
		{
			foreach (var headerLineNotCauseValidationError in HeaderLinesNotCauseValidationError)
			{
				InsertHeaderLineToDB(headerLineNotCauseValidationError);
			}
		}

		#region Parent Registry Item

		protected IRegistryItem ParentRegistryItem
		{
			get
			{
				if (fParentRegistryItem == null)
				{
					fParentRegistryItem = new GuidRegistryItem("GeneralLedgerRegistryDataTypeTest", (MultilingualString)null, null, null, RegistryStorageFlags.System);
				}

				return fParentRegistryItem;
			}
		}

		IRegistryItem fParentRegistryItem;

		#endregion

		#region Data Type

		protected override GeneralLedgerRegistryDataType GetNewDataType()
		{
			return (GeneralLedgerRegistryDataType)Activator.CreateInstance(TypeOfGeneralLedgerRegistryDataType);
		}

		protected abstract Type TypeOfGeneralLedgerRegistryDataType { get; }

		#endregion

		#region Inserting Test Data

		void InsertAccTransactionHeader(Header header)
		{
			InsertAccTransactionHeader(header.PK, header.Ledger, header.TransactionType, header.InvoiceNumber);
		}

		void InsertAccTransactionHeader(Guid pk, string ledger, string transactionType, string invoiceNumber)
		{
			var insertQuery = $@"INSERT INTO {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
										({AccTransactionHeaderSchema.Constants.PK},
										{AccTransactionHeaderSchema.Constants.AH_GC},
										{AccTransactionHeaderSchema.Constants.AH_GE},
										{AccTransactionHeaderSchema.Constants.AH_GB},
										{AccTransactionHeaderSchema.Constants.AH_InvoiceDate},
										{AccTransactionHeaderSchema.Constants.AH_Ledger},
										{AccTransactionHeaderSchema.Constants.AH_PostToGL},
										{AccTransactionHeaderSchema.Constants.AH_TransactionType},
										{AccTransactionHeaderSchema.Constants.AH_AG},
										{AccTransactionHeaderSchema.Constants.AH_TransactionNum},
										{AccTransactionHeaderSchema.Constants.AH_SystemCreateTimeUtc},
										{AccTransactionHeaderSchema.Constants.AH_SystemCreateUser},
										{AccTransactionHeaderSchema.Constants.AH_SystemLastEditTimeUtc},
										{AccTransactionHeaderSchema.Constants.AH_SystemLastEditUser})
									VALUES (@AH_PK, @AH_GC, @AH_GE, @AH_GB, @AH_InvoiceDate, @AH_Ledger, @AH_PostToGL, @AH_TransactionType, @AH_AG, @AH_TransactionNum, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand command = Db.Connection.Command(insertQuery))
			{
				command.AddParameterBasedOnDbColumn("@AH_PK", pk, AccTransactionHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@AH_GC", EnvProxy.Instance.CurrentCompany.PK, AccTransactionHeaderSchema.AH_GC);
				command.AddParameterBasedOnDbColumn("@AH_GE", EnvProxy.Instance.CurrentDepartment.PK, AccTransactionHeaderSchema.AH_GE);
				command.AddParameterBasedOnDbColumn("@AH_GB", EnvProxy.Instance.CurrentBranch.PK, AccTransactionHeaderSchema.AH_GB);
				command.AddParameterBasedOnDbColumn("@AH_InvoiceDate", DateTime.Now, AccTransactionHeaderSchema.AH_InvoiceDate);
				command.AddParameterBasedOnDbColumn("@AH_Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger);
				command.AddParameterBasedOnDbColumn("@AH_PostToGL", "Y", AccTransactionHeaderSchema.AH_PostToGL);
				command.AddParameterBasedOnDbColumn("@AH_TransactionType", transactionType, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameterBasedOnDbColumn("@AH_AG", AccGLHeaderPK, AccTransactionHeaderSchema.AH_AG);
				command.AddParameterBasedOnDbColumn("@AH_TransactionNum", invoiceNumber, AccTransactionHeaderSchema.AH_TransactionNum);

				command.ExecuteNonQuery();
			}
		}

		void InsertAccTransactionLine(Line line)
		{
			InsertAccTransactionLine(line.PK, line.AH, line.IsGST, line.LineType, line.GSTVATBasis, line.PostToGL);
		}

		void InsertAccTransactionLine(Guid pk, Guid headerPK, bool isGST, string lineType, string gstVATBasis, string postToGL)
		{
			var taxCodeSpecificColumn = (isGST) ? AccTransactionLinesSchema.Constants.AL_GSTVAT : AccTransactionLinesSchema.Constants.AL_WithholdingTax;

			var insertSQL = $@"INSERT INTO {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
										({AccTransactionLinesSchema.Constants.PK},
										{AccTransactionLinesSchema.Constants.AL_GB},
										{AccTransactionLinesSchema.Constants.AL_GE},
										{taxCodeSpecificColumn},
										{AccTransactionLinesSchema.Constants.AL_AH},
										{AccTransactionLinesSchema.Constants.AL_LineType},
										{AccTransactionLinesSchema.Constants.AL_PostToGL},
										{AccTransactionLinesSchema.Constants.AL_AG},
										{AccTransactionLinesSchema.Constants.AL_GC},
										{AccTransactionLinesSchema.Constants.AL_GSTVATBasis},
										{AccTransactionLinesSchema.Constants.AL_SystemCreateTimeUtc},
										{AccTransactionLinesSchema.Constants.AL_SystemCreateUser},
										{AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc},
										{AccTransactionLinesSchema.Constants.AL_SystemLastEditUser})
									VALUES (@AL_PK, @AL_GB, @AL_GE, @AL_GSTVAT, @AL_AH, @AL_LineType, @AL_PostToGL, @AL_AG, @AL_GC, @AL_GSTVATBasis, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(insertSQL))
			{
				command.AddParameterBasedOnDbColumn("@AL_PK", pk, AccTransactionLinesSchema.PK);
				command.AddParameterBasedOnDbColumn("@AL_GB", EnvProxy.Instance.CurrentBranch.PK, AccTransactionLinesSchema.AL_GB);
				command.AddParameterBasedOnDbColumn("@AL_GE", EnvProxy.Instance.CurrentDepartment.PK, AccTransactionLinesSchema.AL_GE);
				command.AddParameterBasedOnDbColumn("@AL_GSTVAT", 1, AccTransactionLinesSchema.AL_GSTVAT);
				if (headerPK == Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@AL_AH", DBNull.Value, AccTransactionLinesSchema.AL_AH);
				}
				else
				{
					command.AddParameterBasedOnDbColumn("@AL_AH", headerPK, AccTransactionLinesSchema.AL_AH);
				}
				command.AddParameterBasedOnDbColumn("@AL_PostToGL", postToGL, AccTransactionLinesSchema.AL_PostToGL);
				command.AddParameterBasedOnDbColumn("@AL_LineType", lineType, AccTransactionLinesSchema.AL_LineType);
				command.AddParameterBasedOnDbColumn("@AL_AG", AccGLHeaderPK, AccTransactionLinesSchema.AL_AG);
				command.AddParameterBasedOnDbColumn("@AL_GC", EnvProxy.Instance.CurrentCompany.PK, AccTransactionLinesSchema.AL_GC);
				command.AddParameterBasedOnDbColumn("@AL_GSTVATBasis", gstVATBasis, AccTransactionLinesSchema.AL_GSTVATBasis);

				command.ExecuteNonQuery();
			}
		}

		void InsertAccCashBasisVAT(Guid aL_PK, bool deleteQueue)
		{
			var yC_PK = Guid.NewGuid();

			var insertSQL = $@"INSERT INTO {AccCashBasisVATSchema.Constants.SqlSchemaName}.{AccCashBasisVATSchema.Constants.TableName}
									({AccCashBasisVATSchema.Constants.PK},
									{AccCashBasisVATSchema.Constants.YC_GC},
									{AccCashBasisVATSchema.Constants.YC_AL_TransactionLine},
									{AccCashBasisVATSchema.Constants.YC_PostDate},
									{AccCashBasisVATSchema.Constants.YC_SystemCreateTimeUtc},
									{AccCashBasisVATSchema.Constants.YC_SystemCreateUser},
									{AccCashBasisVATSchema.Constants.YC_SystemLastEditTimeUtc},
									{AccCashBasisVATSchema.Constants.YC_SystemLastEditUser})
								values (@YC_PK, @YC_GC, @YC_AL_TransactionLine, @YC_PostDate, @YC_PostDate, 'USR', GetUtcDate(), 'USR')";

			using (var command = Db.Connection.Command(insertSQL))
			{
				command.AddParameterBasedOnDbColumn("@YC_PK", yC_PK, AccCashBasisVATSchema.PK);
				command.AddParameterBasedOnDbColumn("@YC_GC", EnvProxy.Instance.CurrentCompany.PK, AccCashBasisVATSchema.YC_GC);
				command.AddParameterBasedOnDbColumn("@YC_AL_TransactionLine", aL_PK, AccCashBasisVATSchema.YC_AL_TransactionLine);
				command.AddParameterBasedOnDbColumn("@YC_PostDate", DateTime.Today, AccCashBasisVATSchema.YC_PostDate);

				command.ExecuteNonQuery();
			}

			if (deleteQueue)
			{
				var deleteSQL = $@"DELETE FROM {AccCashBasisVATQueueSchema.Constants.SqlSchemaName}.{AccCashBasisVATQueueSchema.Constants.TableName}
									WHERE {AccCashBasisVATQueueSchema.Constants.PK} = @YCC_YC";

				using (var command = Db.Connection.Command(deleteSQL))
				{
					command.AddParameterBasedOnDbColumn("@YCC_YC", yC_PK, AccCashBasisVATQueueSchema.PK);

					command.ExecuteNonQuery();
				}
			}
		}

		void InsertAccGLHeaderRow(Guid accGLHeaderPK)
		{
			var insertSql = $@"INSERT INTO {AccGLHeaderSchema.Constants.SqlSchemaName}.{AccGLHeaderSchema.Constants.TableName}
									({AccGLHeaderSchema.Constants.PK},
									{AccGLHeaderSchema.Constants.AG_AccountNum},
									{AccGLHeaderSchema.Constants.AG_AccountType},
									{AccGLHeaderSchema.Constants.AG_DebitCredit},
									{AccGLHeaderSchema.Constants.AG_SystemCreateTimeUtc},
									{AccGLHeaderSchema.Constants.AG_SystemCreateUser},
									{AccGLHeaderSchema.Constants.AG_SystemLastEditTimeUtc},
									{AccGLHeaderSchema.Constants.AG_SystemLastEditUser})
								values ('{accGLHeaderPK}', '1234.12.34', '{Core.Constants.AccountType.BalanceSheetAccount}', '{Core.Constants.DebitCredit.Debit}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(insertSql);
		}

		protected Guid AccGLHeaderPK;

		#endregion

		#endregion

		protected class Header
		{
			public Guid PK;
			public string Ledger;
			public string TransactionType;
			public string InvoiceNumber;
		}

		protected class Line
		{
			public Guid PK;
			public Guid AH;
			public bool IsGST;
			public string LineType;
			public string GSTVATBasis;
			public string PostToGL;
			public bool IsInsertAccCashBasisVAT;
			public bool IsDeleteAccCashBasisVATQueue;
		}

		protected class HeaderLine
		{
			public Header Header;
			public Line Line;
		}

		protected List<HeaderLine> HeaderLinesCauseValidationError;
		protected List<HeaderLine> HeaderLinesNotCauseValidationError;

		protected virtual bool CannotContainSPRDissection => false;
	}
}
