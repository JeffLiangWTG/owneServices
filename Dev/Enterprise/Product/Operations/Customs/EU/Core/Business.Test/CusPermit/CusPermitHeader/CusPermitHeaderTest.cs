using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusPermitHeader))]
	public class CusPermitHeaderTest : Customs.Business.Testing.BaseCusPermitHeaderTest
	{
		[ExpectNoExceptions]
		public void TestGetDefaultDataGroupingCode()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Latvia;
			NUnit.Framework.Assert.That(permit.GetDefaultDataGroupingCode(), NUnit.Framework.Is.EqualTo(Enterprise.Core.Constants.CountryCodes.Latvia).Using(CustomComparers.TypeComparison), "GetDefaultDataGroupingCode() should return LV because Latvia owns its own Customs jurisdiction.");

			permit.CPH_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Martinique;
			NUnit.Framework.Assert.That(permit.GetDefaultDataGroupingCode(), NUnit.Framework.Is.EqualTo(Enterprise.Core.Constants.CountryCodes.France).Using(CustomComparers.TypeComparison), "GetDefaultDataGroupingCode() should return FR because Martinique Customs jurisdiction is France.");
		}

		[ExpectNoExceptions]
		public void TestAllowNewLineTransactions()
		{
			var permit = Factory.NewWithValidTestData<CusPermitHeader>();
			NUnit.Framework.Assert.That(PermitHeader.IsTransactionsApplicable(), NUnit.Framework.Is.True);
			permit.TransactionCategory = ZString.Empty;
			NUnit.Framework.Assert.That(((IBindingList)permit.CusPermitLineTransactions).AllowNew, NUnit.Framework.Is.EqualTo(false));

			permit.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			NUnit.Framework.Assert.That(((IBindingList)permit.CusPermitLineTransactions).AllowNew, NUnit.Framework.Is.EqualTo(true), "Allow new transactions only when qty/val is not empty and categary is CUM.");

			permit.TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			NUnit.Framework.Assert.That(((IBindingList)permit.CusPermitLineTransactions).AllowNew, NUnit.Framework.Is.EqualTo(false));

			permit.CPH_QtyValIndicator = ZString.Empty;
			permit.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			NUnit.Framework.Assert.That(((IBindingList)permit.CusPermitLineTransactions).AllowNew, NUnit.Framework.Is.EqualTo(false), "Allow new transactions only when qty/val is not empty and categary is CUM.");
		}

		[ExpectNoExceptions]
		public void TestPermitCountrySpecificInstructionShouldBeOfEUType()
		{
			var permit = Factory.New<CusPermitHeader>();
			NUnit.Framework.Assert.That(permit.CountrySpecificInstruction, NUnit.Framework.Is.TypeOf<PermitCountrySpecificInstruction>());
		}

		[ExpectNoExceptions]
		public void TestCheckPropertyMaxLength()
		{
			var permit = Factory.New<CusPermitHeader>();
			NUnit.Framework.Assert.That(permit.GetPossiblyCustomPropertyMaxLength(nameof(CusPermitHeader.CPH_Type)), NUnit.Framework.Is.EqualTo(CusPermitHeader.Schema.CPH_TypeMaxLength));
			NUnit.Framework.Assert.That(permit.GetPossiblyCustomPropertyMaxLength(nameof(CusPermitHeader.CPH_SubType)), NUnit.Framework.Is.EqualTo(CusPermitHeader.Schema.CPH_SubTypeMaxLength));
			NUnit.Framework.Assert.That(permit.GetPossiblyCustomPropertyMaxLength(nameof(CusPermitHeader.CPH_FullType)), NUnit.Framework.Is.EqualTo(CusPermitHeader.Schema.CPH_FullTypeMaxLength));
		}

		[ExpectNoExceptions]
		public void TestCheckPropertyReadOnly()
		{
			var permit = Factory.New<CusPermitHeader>();
			NUnit.Framework.Assert.That(permit.GetPossiblyCustomPropertyReadOnly(nameof(CusPermitHeader.CPH_Type)), NUnit.Framework.Is.True, "User should not be able to edit Type directly.");
			NUnit.Framework.Assert.That(permit.GetPossiblyCustomPropertyReadOnly(nameof(CusPermitHeader.CPH_SubType)), NUnit.Framework.Is.True, "User should not be able to edit SubType directly.");
			NUnit.Framework.Assert.That(!permit.GetPossiblyCustomPropertyReadOnly(nameof(CusPermitHeader.CPH_FullType)), NUnit.Framework.Is.True, "User should be able to edit FullType directly.");
		}

		[ExpectNoExceptions]
		public void TestGetCPH_FullValue()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_Type = "3LLB";
			permit.CPH_SubType = "231";
			NUnit.Framework.Assert.That(permit.CPH_FullType, NUnit.Framework.Is.EqualTo("3LLB231").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetCPH_FullType()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_FullType = "3LLB231";
			NUnit.Framework.Assert.That(permit.CPH_Type, NUnit.Framework.Is.EqualTo("3LLB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permit.CPH_SubType, NUnit.Framework.Is.EqualTo("231").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetCPH_TypeShouldTruncateRedundantCharacter()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_Type = "12345";
			NUnit.Framework.Assert.That(permit.CPH_Type, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison), "The max length of CPH_Type is 5 in base/database, however, we accept 4 characters at most in EU.");
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			NUnit.Framework.Assert.That(permit.Validation, NUnit.Framework.Is.TypeOf<CusPermitHeaderValidation>());
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			NUnit.Framework.Assert.That(permit.Lookups, NUnit.Framework.Is.TypeOf<CusPermitHeaderLookups>());
		}

		[ExpectNoExceptions]
		public void TestCountrySpecificInstructionType()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			NUnit.Framework.Assert.That(permit.CountrySpecificInstruction, NUnit.Framework.Is.TypeOf<PermitCountrySpecificInstruction>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			supplier = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "REF1";

			outgoingMessage = Factory.NewWithValidTestData<EDIMessage>();
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_MessageNum = "OU1";

			outgoingMessage2 = Factory.NewWithValidTestData<EDIMessage>();
			outgoingMessage2.EM_LinkedObject = entryHeader;
			outgoingMessage2.EM_MessageNum = "OU2";

			incomingMessage = Factory.NewWithValidTestData<EDIMessage>();
			incomingMessage.EM_MessageNum = "APPID";
		}

		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		EDIMessage outgoingMessage2;
		EDIMessage incomingMessage;
		OrgHeader importer;
		OrgHeader supplier;
	}
}
