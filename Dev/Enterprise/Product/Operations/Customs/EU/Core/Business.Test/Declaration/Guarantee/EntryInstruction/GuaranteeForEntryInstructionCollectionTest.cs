using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class GuaranteeForEntryInstructionCollectionAbstractTest<T> : BusinessObjectCollectionTestCase
		where T : GuaranteeForEntryInstructionCollection
	{
		public void TestMaxCount()
		{
			var guaranteeCollection = GetNewGuaranteeCollection();
			AssertEquals(nameof(guaranteeCollection.MaxCount), MaxCountForValidation, guaranteeCollection.MaxCount);
		}

		public void TestSetDefaultsForNewChild()
		{
			var guaranteeCollection = GetNewGuaranteeCollection();
			var newGuarantee = guaranteeCollection.AddNew();

			var entryInstruction = guaranteeCollection.Master;

			CombineAssertions(() =>
			{
				AssertEquals(nameof(newGuarantee.PW_RX_NKCurrency), "EUR", newGuarantee.PW_RX_NKCurrency);
				AssertEquals(nameof(newGuarantee.PW_SuretyCode), LiabilityApplicablePercentageCodeList.Codes.FUL, newGuarantee.PW_SuretyCode);
				AssertEquals(nameof(newGuarantee.PW_ParentTableCode), entryInstruction.TablePrefix, newGuarantee.PW_ParentTableCode);
				AssertEquals(nameof(newGuarantee.PW_ParentID), entryInstruction.PK, newGuarantee.PW_ParentID);
			});
		}

		protected sealed override BusinessObjectCollection GetCollectionToTest() => GetNewGuaranteeCollection();

		protected sealed override Type GetExpectedCollectionType() => typeof(T);

		protected abstract T GetNewGuaranteeCollection();

		protected virtual int MaxCountForValidation
		{
			get => maxCountForValidation;
		}
		const int maxCountForValidation = 99;
	}

	[TestedType(typeof(GuaranteeForEntryInstructionCollection))]
	sealed class GuaranteeForEntryInstructionCollectionBaseOnlyTest : GuaranteeForEntryInstructionCollectionAbstractTest<GuaranteeForEntryInstructionCollection>
	{
		protected override GuaranteeForEntryInstructionCollection GetNewGuaranteeCollection() => Factory.New<CusEntryInstruction>().Guarantees;

		public void TestFkColumnName()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var guaranteeCollection = new CusEntryInstructionGuaranteeCollectionForTest(entryInstruction);

			AssertEquals("FkColumnName", CusBondDetail.Schema.PW_ParentID, guaranteeCollection.FkColumnNameExposed);
		}

		#region CusEntryInstructionGuaranteeCollectionForTest

		class CusEntryInstructionGuaranteeCollectionForTest : GuaranteeForEntryInstructionCollection
		{
			public CusEntryInstructionGuaranteeCollectionForTest(CusEntryInstruction master) : base(master)
			{
			}

			public ZString FkColumnNameExposed => FkColumnName;
		}

		#endregion
	}
}
