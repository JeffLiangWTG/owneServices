using CargoWise.EntityFramework;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsLabel))]
	public class DocWhsLabelTest : DocumentWrapperTestCase
	{
		#region Properties

		public void TestLabelNumber()
		{
			labelObj.Number = 10;
			AssertEquals(10, labelWrapper.LabelNumber);

			labelObj.Number = 20;
			AssertEquals(20, labelWrapper.LabelNumber);
		}

		public void TestLabelStringBarcode()
		{
			TextBarcode barcode = new TextBarcode("test");
			labelObj.String = "test";
			AssertEquals(barcode.TextAs128sFontString, labelWrapper.LabelStringBarcode);

			barcode = new TextBarcode("retested");
			labelObj.String = "retested";
			AssertEquals(barcode.TextAs128sFontString, labelWrapper.LabelStringBarcode);
		}

		public void TestLabelString()
		{
			labelObj.String = "test";
			AssertEquals("test", labelWrapper.LabelString);

			labelObj.String = "retested";
			AssertEquals("retested", labelWrapper.LabelString);
		}

		#endregion

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					GetNewDocWrapper(labelObj, Factory),
				};
		}

		protected WhsLabel labelObj;
		protected DocWhsLabel labelWrapper;

		protected override void SetUp()
		{
			labelObj = new WhsLabel();
			labelWrapper = GetNewDocWrapper(labelObj, Factory);
			base.SetUp();
		}

		protected virtual DocWhsLabel GetNewDocWrapper(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return DocWhsLabel.New(label, factoryToWrap);
		}

		#endregion
	}
}
