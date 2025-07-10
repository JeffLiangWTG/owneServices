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
	[TestedType(typeof(DrawbackImportLicense))]
	public class DrawbackImportLicenseTest : Customs.Business.Testing.CusSupportingInfoTest<DrawbackImportLicense>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			return invoiceLine.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();
		}

		public void TestOnSaving()
		{
			var supporting = (DrawbackImportLicense)GetNewBusinessObject();
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
			var supporting = (DrawbackImportLicense)GetNewBusinessObject();
			supporting.CSI_Code = "1";
			Assert("IsSavedByFactory is false because the CSI_Code is not empty.", supporting.IsSavedByFactory);
			supporting.CSI_Code = ZString.Empty;
			Assert("IsSavedByFactory is true because the CSI_Code is empty.", !supporting.IsSavedByFactory);
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<DrawbackImportLicense>();
			AssertEquals(CusSupportingInfoTypeList.Codes.Drawback, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		public void TestReadOnly()
		{
			var drawback = Factory.New<DrawbackImportLicense>();
			AssertFiledsAreReadOnly(DrawbackModalityList.Codes.GenericSuspension, false, false);
			AssertFiledsAreReadOnly(DrawbackModalityList.Codes.NonGenericSuspension, false, false);
			AssertFiledsAreReadOnly(DrawbackModalityList.Codes.NoDrawback, true, true);
			AssertFiledsAreReadOnly(DrawbackModalityList.Codes.ExemptionWeb, false, false);
			AssertFiledsAreReadOnly(DrawbackModalityList.Codes.ExemptionPaper, false, true);

			void AssertFiledsAreReadOnly(ZString code, bool referenceNumberIsReadOnly, bool itemNumberIsReadOnly)
			{
				drawback.CSI_Code = code;

				CombineAssertions($"CSI_Code: {code}", () =>
				{
					AssertEquals("CSI_ReferenceNumber", referenceNumberIsReadOnly, drawback.CSI_ReferenceNumberInfo.ReadOnly);
					AssertEquals("CSI_ItemNumber", itemNumberIsReadOnly, drawback.CSI_ItemNumberInfo.ReadOnly);
				});
			}
		}

		public void TestClearFieldsOnCodeChanged()
		{
			var drawback = Factory.New<DrawbackImportLicense>();
			AssertFiledsAreCleared(DrawbackModalityList.Codes.GenericSuspension, false, false);
			AssertFiledsAreCleared(DrawbackModalityList.Codes.NonGenericSuspension, false, false);
			AssertFiledsAreCleared(DrawbackModalityList.Codes.NoDrawback, true, true);
			AssertFiledsAreCleared(DrawbackModalityList.Codes.ExemptionWeb, false, false);
			AssertFiledsAreCleared(DrawbackModalityList.Codes.ExemptionPaper, false, true);

			void AssertFiledsAreCleared(ZString code, bool referenceNumberIsCleared, bool itemNumberIsCleared)
			{
				drawback.CSI_ReferenceNumber = "XXXX";
				drawback.CSI_ItemNumber = 1;
				drawback.CSI_Code = code;

				CombineAssertions($"CSI_Code: {code}", () =>
				{
					AssertEquals("CSI_ReferenceNumber", referenceNumberIsCleared, drawback.CSI_ReferenceNumber.IsEmpty);
					AssertEquals("CSI_ItemNumber", itemNumberIsCleared, drawback.CSI_ItemNumber.IsEmpty);
				});
			}
		}

		protected override IEnumerable<DrawbackImportLicense> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var drawback = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().DrawbackImportLicenseCollection.AddNew();
			drawback.CSI_Code = "1";
			drawback.CSI_ReferenceNumber = "XXXX";
			drawback.CSI_ItemNumber = 1;
			yield return drawback;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}
	}
}

