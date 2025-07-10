using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCountry))]
	public class DocCountryTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCountry.New(BizCountry, Factory)
			};
		}

		public void TestCode()
		{
			ZString code = new ZString("TE");

			BizCountry.RN_Code = code;
			AssertEquals("Wrapped Value", code, DocCountry.Code);
		}

		public void TestName()
		{
			ZString name = new ZString("Test Description");

			BizCountry.RN_Desc = name;
			AssertEquals("Wrapped Value", name, DocCountry.Name);
		}

		public void TestIsActive()
		{
			ZBool isActive = ZBool.True;

			BizCountry.RN_IsActive = isActive;
			AssertEquals("Wrapped Value", isActive, DocCountry.IsActive);
		}

		public void TestIsSystem()
		{
			ZBool isSystem = ZBool.True;

			BizCountry.RN_IsSystem = isSystem;
			AssertEquals("Wrapped Value", isSystem, DocCountry.IsSystem);
		}

		public void TestToString()
		{
			ZString name = new ZString("Test Description");

			BizCountry.RN_Desc = name;
			AssertEquals("Wrapped Value", name, DocCountry.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			BizCountry = Factory.New<RefCountry>();
			DocCountry = DocCountry.New(BizCountry, Factory);
			AssertNotNull("PreCondition: Valid DocCountry", DocCountry);

			base.SetUp();
		}

		RefCountry BizCountry;
		DocCountry DocCountry;

		#endregion
	}
}
