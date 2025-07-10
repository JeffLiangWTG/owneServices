using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCSI_Type_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_Type), false, r => r.Caption == "Type");
		}

		public void TestCSI_Type_ReadOnly()
		{
			AssertEquals(true, GetSupportingDocument(Factory).CSI_TypeInfo.ReadOnly);
		}

		public void TestCSI_Code_MaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_Code), false, r => r.MaxLength == 3);
		}

		public void TestCSI_Code_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_Code), false, r => r.Caption == "Code");
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_ReferenceNumber), false, r => r.Caption == "Reference");
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_AdditionalDescription), false, r => r.Caption == "Comments");
		}

		public void TestValidationAndLookups()
		{
			CombineAssertions(() =>
			{
				var supportingDoc = GetSupportingDocument(Factory);
				AssertType<SupportingDocumentValidation>("Type of Validation", supportingDoc.Validation);
				AssertType<SupportingDocumentLookups>("Type of Lookups", supportingDoc.Lookups);
			});
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetSupportingDocument(factory);
		}

		static SupportingDocument GetSupportingDocument(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var supportingDocumentsProvider = (ISupportingDocumentsProvider)declaration.Invoices.AddNew();
			return supportingDocumentsProvider.SupportingDocuments.AddNew();
		}
	}
}
