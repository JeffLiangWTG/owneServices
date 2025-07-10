using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CopyDocumentsSelectionLine))]
	sealed class CopyDocumentsSelectionLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInfo()
		{
			AssertSame(info, line.Info);
		}

		public void TestCSI_Type()
		{
			info.CSI_Type = "TYP";

			CombineAssertions(() =>
			{
				AssertEquals("TYP", line.CSI_Type);
				AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(line.CSI_TypeInfo).Caption);
			});
		}

		public void TestCSI_SubType()
		{
			info.CSI_SubType = "SUB";

			CombineAssertions(() =>
			{
				AssertEquals("SUB", line.CSI_SubType);
				AssertEquals("Kind", DataBoundResourceStrings.GetDataForProperty(line.CSI_SubTypeInfo).Caption);
			});
		}

		public void TestCSI_Code()
		{
			info.CSI_Code = "COD";

			CombineAssertions(() =>
			{
				AssertEquals("COD", line.CSI_Code);
				AssertEquals("Full Type", DataBoundResourceStrings.GetDataForProperty(line.CSI_CodeInfo).Caption);
			});
		}

		public void TestCSI_ReferenceNumber()
		{
			info.CSI_ReferenceNumber = "REF";

			CombineAssertions(() =>
			{
				AssertEquals("REF", line.CSI_ReferenceNumber);
				AssertEquals("Reference", DataBoundResourceStrings.GetDataForProperty(line.CSI_ReferenceNumberInfo).Caption);
			});
		}

		public void TestCSI_ReferenceNumber2()
		{
			info.CSI_ReferenceNumber2 = "REF2";

			CombineAssertions(() =>
			{
				AssertEquals("REF2", line.CSI_ReferenceNumber2);
				AssertEquals("Detail", DataBoundResourceStrings.GetDataForProperty(line.CSI_ReferenceNumber2Info).Caption);
			});
		}

		public void TestCSI_Description()
		{
			info.CSI_Description = "DESC";

			CombineAssertions(() =>
			{
				AssertEquals("DESC", line.CSI_Description);
				AssertEquals("Description", DataBoundResourceStrings.GetDataForProperty(line.CSI_DescriptionInfo).Caption);
			});
		}

		public void TestIsSelected()
		{
			AssertEquals("Select?", DataBoundResourceStrings.GetDataForProperty(line.IsSelectedInfo).Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			info = Factory.New<SupportingDocument>();
			line = new CopyDocumentsSelectionLine(info);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CopyDocumentsSelectionLine(info);
		}

		ImportExportAwareSupportingInfo info;
		CopyDocumentsSelectionLine line;
	}
}
