using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ExchangeHedge))]
	public class ExchangeHedgeTest : Customs.Business.Testing.CusSupportingInfoTest<ExchangeHedge>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration.Invoices.AddNew().ExchangeHedge;
		}

		public void TestOnSaving()
		{
			var supporting = (ExchangeHedge)GetNewBusinessObject();
			supporting.CSI_Code = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var parentPK = supporting.Parent.PK;
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, parentPK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, supporting.CSI_Type);
			var supportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			Assert("The supportingInfoInDB was not deleted", supportingInfoInDB.Any());
			supporting.CSI_Code = ZString.Empty;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			supportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			Assert("The supportingInfoInDB was deleted", !supportingInfoInDB.Any());
		}

		public void TestIsSavedByFactory()
		{
			var supporting = (ExchangeHedge)GetNewBusinessObject();
			supporting.CSI_Code = "1";
			Assert("IsSavedByFactory is false because the CSI_Code is not empty.", supporting.IsSavedByFactory);
			supporting.CSI_Code = ZString.Empty;
			Assert("IsSavedByFactory is true because the CSI_Code is empty.", !supporting.IsSavedByFactory);
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ExchangeHedge>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ExchangeHedge, supporting.CSI_Type);
			AssertEquals(JobComInvoiceHeaderSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		public void TestReadOnly()
		{
			var exchangeHedge = (ExchangeHedge)GetNewBusinessObject();

			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._1, false, false, true, true, true, true);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._2, false, true, true, true, true, true);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._3, true, true, true, false, false, false);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._4, true, true, false, true, false, true);
		}

		public void TestClearFieldsOnCodeChanged()
		{
			var exchangeHedge = (ExchangeHedge)GetNewBusinessObject();
			AssertFiledsAreCleared(ExchangeHedgeList.Codes._1, false, false, true, true, true, true);
			AssertFiledsAreCleared(ExchangeHedgeList.Codes._2, false, true, true, true, true, true);
			AssertFiledsAreCleared(ExchangeHedgeList.Codes._3, true, true, true, false, false, false);
			AssertFiledsAreCleared(ExchangeHedgeList.Codes._4, true, true, false, true, false, true);

			void AssertFiledsAreCleared(ZString code, bool subTypeIsCleared, bool quantityIsCleared, bool additionalDescriptionCleared, bool issuerTypeIsCleared, bool referenceNumberCleared, bool valueCleared)
			{
				exchangeHedge.CSI_SubType = "3";
				exchangeHedge.CSI_Quantity = 339;
				exchangeHedge.CSI_AdditionalDescription = "TE";
				exchangeHedge.CSI_IssuerType = "2";
				exchangeHedge.CSI_ReferenceNumber = "AAAA";
				exchangeHedge.CSI_Value = 200m;
				exchangeHedge.CSI_Code = code;

				CombineAssertions($"CSI_Code: {code}", () =>
				{
					AssertEquals("CSI_SubType", subTypeIsCleared, exchangeHedge.CSI_SubType.IsEmpty);
					AssertEquals("CSI_Quantity", quantityIsCleared, exchangeHedge.CSI_Quantity.IsEmpty);
					AssertEquals("CSI_AdditionalDescription", additionalDescriptionCleared, exchangeHedge.CSI_AdditionalDescription.IsEmpty);
					AssertEquals("CSI_IssuerType", issuerTypeIsCleared, exchangeHedge.CSI_IssuerType.IsEmpty);
					AssertEquals("CSI_ReferenceNumber", referenceNumberCleared, exchangeHedge.CSI_ReferenceNumber.IsEmpty);
					AssertEquals("CSI_Value", valueCleared, exchangeHedge.CSI_Value.IsEmpty);
				});
			}
		}

		public void TestReadOnlyFieldsWhenInvoicesAreLinked()
		{
			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var iswInvoice = iswDeclaration.Invoices.AddNew();
			var iswInvoiceLine = iswInvoice.InvoiceLines.AddNew();

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvoice = licDeclaration.Invoices.AddNew();
			var licInvoiceLine = licInvoice.InvoiceLines.AddNew();

			iswInvoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			iswInvoiceLine.JI_ParentID = licInvoiceLine.PK;

			licInvoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			licInvoiceLine.JI_ParentID = iswInvoiceLine.PK;

			var exchangeHedge = licInvoice.ExchangeHedge;

			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._1, false, false, true, true, true, true);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._2, false, true, true, true, true, true);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._3, true, true, true, true, true, true);
			AssertFiledsAreReadOnly(exchangeHedge, ExchangeHedgeList.Codes._4, true, true, true, true, true, true);
		}

		void AssertFiledsAreReadOnly(ExchangeHedge hedge, ZString code, bool subTypeIsReadOnly, bool quantityIsReadOnly, bool additionalDescriptionReadOnly, bool issuerTypeIsReadOnly, bool referenceNumberReadOnly, bool valueReadOnly)
		{
			hedge.CSI_Code = code;

			CombineAssertions($"CSI_Code: {code}", () =>
			{
				AssertEquals("CSI_SubType", subTypeIsReadOnly, hedge.CSI_SubTypeInfo.ReadOnly);
				AssertEquals("CSI_Quantity", quantityIsReadOnly, hedge.CSI_QuantityInfo.ReadOnly);
				AssertEquals("CSI_AdditionalDescription", additionalDescriptionReadOnly, hedge.CSI_AdditionalDescriptionInfo.ReadOnly);
				AssertEquals("CSI_IssuerType", issuerTypeIsReadOnly, hedge.CSI_IssuerTypeInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber", referenceNumberReadOnly, hedge.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("CSI_Value", valueReadOnly, hedge.CSI_ValueInfo.ReadOnly);
			});
		}

		protected override IEnumerable<ExchangeHedge> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var exchangeHedge = declaration.Invoices.AddNew().ExchangeHedge;
			exchangeHedge.CSI_Code = "1";
			exchangeHedge.CSI_SubType = "3";
			exchangeHedge.CSI_Quantity = 339;
			exchangeHedge.CSI_AdditionalDescription = "TE";
			exchangeHedge.CSI_IssuerType = "2";
			yield return exchangeHedge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}
	}
}
