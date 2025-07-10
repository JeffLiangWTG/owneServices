using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(TariffDetach))]
	public class TariffDetachTest : CusCodeDataTest<TariffDetach>
	{
		public void TestValidation()
		{
			AssertEquals(typeof(TariffDetachValidation), TariffDetach.Validation.GetType());
		}

		public void TestInvoiceLine()
		{
			AssertSame(InvoiceLine, TariffDetach.TariffDetachParent);
		}

		public void TestCusClassPartPivot()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			var tariffDetach = pivot.TariffDetachs.AddNew();
			AssertSame(pivot, tariffDetach.TariffDetachParent);
		}

		public void TestReadOnly()
		{
			Assert("ReadOnly", !TariffDetach.ReadOnly);
			InvoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			InvoiceLine.JI_ParentID = ZGuid.NewZGuid();
			Assert("ReadOnly when cloned from attached Import License Entry", TariffDetach.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.TariffDetach, TariffDetach.CY_Type);
		}

		public void TestJG_ReferenceNumberMaxLength()
		{
			AssertEquals(3, TariffDetach.CY_CodeInfo.MaxLength);
		}

		public void TestDeleteIfReferenceNumberIsEmpty()
		{
			TariffDetach.CY_Code = "";
			var tariffDetach2 = InvoiceLine.TariffDetachs.AddNew();
			tariffDetach2.CY_Code = "999";
			Factory.Save();
			AssertEquals("Tariff Detach with empty reference is Deleted", true, TariffDetach.IsDeleted);
			AssertEquals("Tariff Detach with reference stays", false, tariffDetach2.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TariffDetach;
		}

		protected override IEnumerable<TariffDetach> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "123";
			yield return tariffDetach;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tariffDetach = InvoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";
			return tariffDetach;
		}

		TariffDetach TariffDetach
		{
			get
			{
				if (tariffDetach == null)
				{
					tariffDetach = InvoiceLine.TariffDetachs.AddNew();
				}

				return tariffDetach;
			}
		}

		TariffDetach tariffDetach;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}

