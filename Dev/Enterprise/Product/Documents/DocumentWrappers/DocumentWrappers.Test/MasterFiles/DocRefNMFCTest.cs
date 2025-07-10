using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Orders
{
	[TestedType(typeof(DocRefNMFC))]
	public class DocRefNMFCTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { RefNMFCWrapper };
		}

		public void TestClass()
		{
			AssertEquals("456", RefNMFCWrapper.Class);
			fNMFC.FN_Class = ZString.Empty;
			AssertEquals(ZString.Empty, RefNMFCWrapper.Class);
		}

		public void TestCode()
		{
			AssertEquals("123|456", RefNMFCWrapper.Code);
			fNMFC.FN_Code = ZString.Empty;
			AssertEquals(ZString.Empty, RefNMFCWrapper.Code);
		}

		public void TestDescription()
		{
			AssertEquals("abc", RefNMFCWrapper.Description);
			fNMFC.FN_Description = ZString.Empty;
			AssertEquals(ZString.Empty, RefNMFCWrapper.Description);
		}

		public void TestItemNo()
		{
			AssertEquals("123", RefNMFCWrapper.ItemNo);
			fNMFC.FN_ItemNo = ZString.Empty;
			AssertEquals(ZString.Empty, RefNMFCWrapper.ItemNo);
		}

		public void TestIsActive()
		{
			AssertEquals(ZBool.True, RefNMFCWrapper.IsActive);
			fNMFC.FN_IsActive = ZBool.False;
			AssertEquals(ZBool.False, RefNMFCWrapper.IsActive);
		}

		RefNMFC fNMFC;
		DocRefNMFC RefNMFCWrapper;
		protected override void SetUp()
		{
			fNMFC = Factory.New<RefNMFC>();
			fNMFC.FN_ItemNo = "123";
			fNMFC.FN_Class = "456";
			fNMFC.FN_Description = "abc";
			RefNMFCWrapper = DocRefNMFC.New(fNMFC, Factory);
			AssertNotNull(RefNMFCWrapper);
			base.SetUp();
		}
	}
}
