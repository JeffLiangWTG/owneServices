using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(NonPersistentPackagePivot))]
	class NonPersistentPackagePivotTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsForInvoiceLine()
		{
			AssertEquals(false, packagePivot.IsForInvoiceLine);
		}

		public void TestIsForInvoiceLine_PivotCreated()
		{
			CombineAssertions(() =>
			{
				AssertNull("Pivot deleted", LoadGenPivot(emcsPackage));
				packagePivot.IsForInvoiceLine = true;
				AssertNotNull("InvoiceLinePivot created", LoadGenPivot(emcsPackage));
			});
		}

		public void TestIsForInvoiceLine_PivotDeleted()
		{
			CombineAssertions(() =>
			{
				packagePivot.IsForInvoiceLine = true;
				AssertNotNull("InvoiceLinePivot created", LoadGenPivot(emcsPackage));
				packagePivot.IsForInvoiceLine = false;
				AssertNull("Pivot deleted", LoadGenPivot(emcsPackage));
			});
		}

		public void TestUnitCount()
		{
			CombineAssertions(() =>
			{
				emcsPackage.B5_UnitCount = 10;
				AssertEquals("Unit Count", 10, packagePivot.UnitCount);
				emcsPackage.Delete();
				AssertEquals("Package deleted (null)", ZLong.Zero, packagePivot.UnitCount);
			});
		}

		public void TestUnitType()
		{
			CombineAssertions(() =>
			{
				emcsPackage.B5_UnitType = "AE";
				AssertEquals("Unit Type", "AE", packagePivot.UnitType);
				emcsPackage.Delete();
				AssertEquals("Package deleted (null)", ZString.Empty, packagePivot.UnitType);
			});
		}

		public void TestMarksAndNumbers()
		{
			CombineAssertions(() =>
			{
				emcsPackage.B5_MarksAndNumbers = "information";
				AssertEquals("Marks And Numbers", "information", packagePivot.MarksAndNumbers);
				emcsPackage.Delete();
				AssertEquals("Package deleted (null)", ZString.Empty, packagePivot.MarksAndNumbers);
			});
		}
		public void TestSealNumber()
		{
			CombineAssertions(() =>
			{
				emcsPackage.B5_SealNumber = "C12345";
				AssertEquals("Seal Number", "C12345", packagePivot.SealNumber);
				emcsPackage.Delete();
				AssertEquals("Package deleted (null)", ZString.Empty, packagePivot.SealNumber);
			});
		}
		public void TestSealComment()
		{
			CombineAssertions(() =>
			{
				emcsPackage.B5_SealComment = "package seal comment";
				AssertEquals("Seal Comment", "package seal comment", packagePivot.SealComment);
				emcsPackage.Delete();
				AssertEquals("Package deleted (null)", ZString.Empty, packagePivot.SealComment);
			});
		}

		public void TestPivotDeleted()
		{
			CombineAssertions(() =>
			{
				packagePivot.IsForInvoiceLine = true;
				var invoiceLineAndDeclarationPivot = LoadGenPivot(emcsPackage);
				AssertNotNull("InvoiceLinePivot created", invoiceLineAndDeclarationPivot);
				packagePivot.Delete();
				AssertEquals("GenPivot Is Deleted", true, invoiceLineAndDeclarationPivot.IsDeleted);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => packagePivot;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			emcsPackage = declaration.EMCSPackages.AddNew();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			packagePivot = invoiceLine.EMCSPackagePivots[0];
		}
		EMCSPackage emcsPackage;
		NonPersistentPackagePivot packagePivot;
		EMCSJobComInvoiceLine invoiceLine;

		GenPivot LoadGenPivot(EMCSPackage package)
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, "EMC");
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, invoiceLine.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, package.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceLineSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusInvPackSchema.Constants.Prefix);

			return invoiceLine.Factory.LoadTop1<GenPivot>(pivotQuery);
		}
	}
}
