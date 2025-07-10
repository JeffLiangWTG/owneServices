using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(DeleteNegativeAmountAllowedOnAccountReceivableTransactionsForVN))]
	class DeleteNegativeAmountAllowedOnAccountReceivableTransactionsForVNTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteNegativeAmountAllowedOnAccountReceivableTransactionsForVN();
		}

		protected override void PrepareTestData()
		{
			CreateCompanies();
			InsertRowsInRegistry();

			AssertNull(Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull(Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull(Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull(Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));

			AssertEquals("ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
		}

		void CreateCompanies()
		{
			var testDataCreator = new TransformationTestDataCreator();

			vNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN1", "VN");
			vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN2", "VN");
			vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN3", "VN");
			vNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN4", "VN");
			vNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN5", "VN");
			vNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("VN6", "VN");
			nonVNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN1", "CN");
			nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN2", "CN");
			nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN3", "CN");
			nonVNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN4", "CN");
			nonVNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN5", "CN");
			nonVNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions = testDataCreator.CreateCompany("CN6", "CN");
		}

		void InsertRowsInRegistry()
		{
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, vNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, nonVNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));
			RegistryHelper.InsertStmDataRow(EnableEInvoicingFunctionalityRegistry, nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "BOL", Encoding.Unicode.GetBytes("True"));

			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("ALL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NAL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("ALL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NAL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("ALL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NAL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("ALL"));
			RegistryHelper.InsertStmDataRow(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NAL"));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(7, RegistryHelper.GetStmDataRowCount(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry));

			AssertNull("Maintain default for original is default", Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull("Maintain default for original is default", Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull("Maintain default for original is default", Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull("Maintain default for original is default", Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, nonVNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertNull("NegativeAmountAllowedOnAccountReceivableTransactions will become default for VN eInvoicing company and origin is override to ALL", Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));

			AssertEquals("Maintain value as NAL for VN eInvoicing company  and origin is override to NAL", "NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(vNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "ALL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions));
			AssertEquals("Maintain original value for not VN eInvoicing company", "NAL", GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(nonVNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions));
		}

		string GetNegativeAmountAllowedOnAccountReceivableTransactionsFromStmData(Guid ownerPK) => Encoding.Unicode.GetString(Helper.GetStmDataValue(NegativeAmountAllowedOnAccountReceivableTransactionsRegistry, ownerPK));

		Guid vNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid vNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid vNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid vNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid vNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid vNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithoutEInvoicingDefaultNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithoutEInvoicingALLNegativeAmountAllowedOnAccountReceivableTransactions;
		Guid nonVNCompanyWithoutEInvoicingNALNegativeAmountAllowedOnAccountReceivableTransactions;

		RegistryTransformationHelper RegistryHelper => registryHelper ?? (registryHelper = new RegistryTransformationHelper());
		RegistryTransformationHelper registryHelper;

		const string EnableEInvoicingFunctionalityRegistry = "EnableEInvoicingFunctionality";
		const string NegativeAmountAllowedOnAccountReceivableTransactionsRegistry = "NegativeAmountAllowedOnAccountReceivableTransactions";
	}
}
