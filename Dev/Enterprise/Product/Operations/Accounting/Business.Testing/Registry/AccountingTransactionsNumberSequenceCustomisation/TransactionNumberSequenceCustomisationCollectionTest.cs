using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionNumberSequenceCustomisationCollection))]
	public class TransactionNumberSequenceCustomisationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TransactionNumberSequenceCustomisationCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TransactionNumberSequenceCustomisationCollection GetCollectionToTest()
		{
			return new TransactionNumberSequenceCustomisationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransactionNumberSequenceCustomisation();
		}

		public void TestOrderFieldShouldBeUnique()
		{
			var object1 = TestBizObj.AddNew();
			var object2 = TestBizObj.AddNew();
			var object3 = TestBizObj.AddNew();
			object1.Order = 1;
			object2.Order = 1;
			object3.Order = 2;
			object1.Include = true;
			object2.Include = true;
			object3.Include = true;
			object2.RunPreSaveValidation();
			AssertHasErrors(object1.OrderInfo);
			AssertHasErrors(object2.OrderInfo);
			AssertNoErrors(object3.OrderInfo);
			object2.Include = false;
			object2.RunPreSaveValidation();
			object1.RunPreSaveValidation();
			AssertNoErrors(object1.OrderInfo);
			AssertNoErrors(object2.OrderInfo);
			AssertNoErrors(object3.OrderInfo);
		}

		[TestDate(2018, 12, 01)]
		public void TestTotalLengthInNumeric()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateNewCompany("ABC");
			var branch = objectCreator.CreateNewBranch(company, "AB1");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AddNew(TestBizObj, 1, true, ElementNames.TransactionHeaderBranchCode, "");
				AddNew(TestBizObj, 2, true, ElementNames.TransactionHeaderDepartmentCode, "");
				AddNew(TestBizObj, 3, true, ElementNames.CustomElement1, "A01");
				AddNew(TestBizObj, 4, true, ElementNames.YearAsLetter, "");
				AddNew(TestBizObj, 5, true, ElementNames.AccountingPeriodAs2Digits, "");
				AddNew(TestBizObj, 6, true, ElementNames.AccountingYearAsLetter, "");
				AddNew(TestBizObj, 6, true, ElementNames.TaxAndNonTax, "ABC/XYZ");
				AddNew(TestBizObj, 6, true, ElementNames.SelfBillingAndStandard, "X/Y1");
				AddNew(TestBizObj, 6, true, ElementNames.CorrectedAndOriginal, "1/0");
				AssertEquals("5+6+4+2+2+2+3+2+1", 27, TestBizObj.TotalLengthInNumeric);
			}
		}

		TransactionNumberSequenceCustomisation AddNew(TransactionNumberSequenceCustomisationCollection collection, ZByte order, ZBool inculde, ZString elementName, ZString code)
		{
			var item = collection.AddNew();
			item.Order = order;
			item.Include = inculde;
			item.ElementName = elementName;
			item.Code = code;
			return item;
		}

		TransactionNumberSequenceCustomisationCollection TestBizObj
		{
			get { return Collection; }
		}
	}
}
