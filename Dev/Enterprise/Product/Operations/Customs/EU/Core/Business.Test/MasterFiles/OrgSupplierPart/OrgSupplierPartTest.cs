using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestPivotsType()
		{
			var part = (OrgSupplierPart)GetNewBusinessObject();
			AssertType(typeof(CusClassPartPivotCollection<CusClassPartPivot>), part.PivotsForBinding);
		}

		public void TestDoesPartMatchPivotForInactiveCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var part = (OrgSupplierPart)GetNewBusinessObject();
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_RN_NKCountry = "!@";
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = "#@1";
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));
		}

		public void TestDoesNotLoadPivotsOnConstruction()
		{
			var part = (OrgSupplierPart)GetNewBusinessObject();

			Assert("Should not have loaded pivots on construction", !part.HasLoadedPivotsForBinding);
		}

		#region ExpectedClassificationCollectionType
		protected override Type ExpectedClassificationCollectionType
		{
			get { return typeof(ClassificationCollection<CusClassification>); }
		}
		#endregion

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			var newObj = OrgSupplierPart.New(Factory);
			newObj.OP_PartNum = newObj.PK.ToString().Replace("-", "");
			return newObj;
		}
		#endregion

		#region GetExpectedBusinessObjectType
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
		#endregion
	}
}
