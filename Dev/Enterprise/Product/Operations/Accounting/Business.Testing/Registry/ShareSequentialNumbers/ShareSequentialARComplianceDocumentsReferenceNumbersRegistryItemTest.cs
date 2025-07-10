using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly))]
	class ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItemTest : StronglyTypedRegistryItemTestCase<IShareSequentialReferenceNumbers, ShareSequentialReferenceNumbers>
	{
		public void TestDeleteValueCore()
		{
			var companyPK = Env.CurrentCompanyPK;

			using (Db.Connection.BeginTransactionWithManager())
			{
				var invReferenceFountain = Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, companyPK);
				var crdReferenceFountain = Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, companyPK);

				invReferenceFountain.SetNext(Db.Connection, 1000);
				crdReferenceFountain.SetNext(Db.Connection, 1100);

				var item = (ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly)GetNewRegistryItem();
				AssertEquals("Pre-condition: value to delete is the same as default one", item.DefaultValue.Value, item.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).Value);

				item.DeleteValueCore_ForTestOnly(companyPK, Guid.Empty, Guid.Empty);

				AssertEquals("Number should NOT be synchonized.", 1000, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1100, crdReferenceFountain.PeekPreliminary(Db.Connection));

				var refNumbers = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);
				item.SetValue(companyPK, Guid.Empty, Guid.Empty, refNumbers);
				AssertNotEquals("Pre-condition: value to delete is different from default one", item.DefaultValue.Value, item.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).Value);

				item.DeleteValueCore_ForTestOnly(companyPK, Guid.Empty, Guid.Empty);

				AssertEquals("Number should be synchonized.", 1100, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1100, crdReferenceFountain.PeekPreliminary(Db.Connection));
			}
		}

		public void TestSetValueCore()
		{
			var companyPK = Env.CurrentCompanyPK;

			using (Db.Connection.BeginTransactionWithManager())
			{
				var invReferenceFountain = Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, companyPK);
				var crdReferenceFountain = Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, companyPK);

				invReferenceFountain.SetNext(Db.Connection, 1000);
				crdReferenceFountain.SetNext(Db.Connection, 1100);

				var refNumbers = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(false);
				var item = (ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly)GetNewRegistryItem();
				AssertEquals("Pre-condition: update with same value", item.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).Value, refNumbers.Value);

				item.SetValue(companyPK, Guid.Empty, Guid.Empty, refNumbers);

				AssertEquals("Number should NOT be synchonized.", 1000, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1100, crdReferenceFountain.PeekPreliminary(Db.Connection));

				var nextRef = crdReferenceFountain.GetNext(Db.Connection);
				AssertEquals(1100, nextRef);
				AssertEquals(1000, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1101, crdReferenceFountain.PeekPreliminary(Db.Connection));

				refNumbers = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);
				AssertEquals("Pre-condition: update with different value", false, item.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).Value);
				AssertEquals(true, refNumbers.Value);

				item.SetValue(companyPK, Guid.Empty, Guid.Empty, refNumbers);

				AssertEquals("Number should be synchonized when update registry from false to true.", 1101, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1101, crdReferenceFountain.PeekPreliminary(Db.Connection));

				nextRef = invReferenceFountain.GetNext(Db.Connection);
				AssertEquals(1101, nextRef);
				AssertEquals(1102, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1101, crdReferenceFountain.PeekPreliminary(Db.Connection));

				refNumbers = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(false);
				AssertEquals("Pre-condition: update with different value", true, item.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).Value);
				AssertEquals(false, refNumbers.Value);

				item.SetValue(companyPK, Guid.Empty, Guid.Empty, refNumbers);

				AssertEquals("Number should be synchonized when update registry from true to false.", 1102, invReferenceFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1102, crdReferenceFountain.PeekPreliminary(Db.Connection));
			}
		}

		public void TestDefaultValue()
		{
			var item = (ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly)GetNewRegistryItem();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				var resultObj = item.Inner.GetDefaultValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				AssertEquals("Default value should be with type ShareSequentialReferenceNumbers", typeof(ShareSequentialReferenceNumbers), resultObj.GetType());
				var result = (ShareSequentialReferenceNumbers)resultObj;
				AssertEquals("Current Company is a non-Taiwan company", false, result.Value);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertEquals("Current Company is a Taiwan company", true,
					((ShareSequentialReferenceNumbers)item.Inner.GetDefaultValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)).Value);
			}

			var factory = new BusinessObjectFactory();
			var testObjCreator = new TestObjectCreator(factory);
			var auCompany = testObjCreator.CreateNewCompany("DAU", Core.Constants.CountryCodes.Australia);
			var tpeCompany = testObjCreator.CreateNewCompany("TPE", Core.Constants.CountryCodes.Taiwan);
			AssertEquals("Precondition", true, auCompany.IsInDatabase);
			AssertEquals("Precondition", true, tpeCompany.IsInDatabase);

			AssertEquals("Default registry value of a non-Taiwan Company", false,
				((ShareSequentialReferenceNumbers)item.Inner.GetDefaultValue(auCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)).Value);
			AssertEquals("Default registry value of a Taiwan Company", true,
				((ShareSequentialReferenceNumbers)item.Inner.GetDefaultValue(tpeCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)).Value);
		}

		protected override StronglyTypedRegistryItem<IShareSequentialReferenceNumbers, ShareSequentialReferenceNumbers> GetNewRegistryItem()
		{
			return new ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		class ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly : ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem
		{
			public ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem_ForTestOnly(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option) : base(name, category, caption, hint, storage, option)
			{
			}

			public void DeleteValueCore_ForTestOnly(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				DeleteValueCore(companyPK, branchPK, departmentPK);
			}
		}
	}
}
