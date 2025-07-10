using System.Reflection;

namespace CargoWise.UniversalCopy.Test
{
	public class PropertyCopyTemplateNodeTest : CopyTemplateNodeTest<PropertyCopyTemplateNode>
	{
		public void TestPropertyNodeProperties()
		{
			var target = CreateConfigurationNode();

			AssertEquals("Int32", target.PropertyType);
			AssertEquals(default(CopyMethod), target.CopyMethod);
			AssertEquals(5, target.DefaultValue);
		}

		public void TestHasData()
		{
			var target = CreateConfigurationNode();
			Assert(!CheckHasData(target));

			target.Value = "xyz";
			Assert(!CheckHasData(target));

			target.CopyMethod = CopyMethod.Copy;
			Assert(CheckHasData(target));
		}

		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(PropertyCopyTemplateNode target)
		{
			base.PrepareDataForResetAndUpdateId(target);
			target.CopyMethod = CopyMethod.Copy;
			target.Value = "xyz";
			target.DefaultValue = "abc";
			target.PropertyType = "string";
		}

		protected override void AssertDataAfterResetAndUpdateId(PropertyCopyTemplateNode target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			AssertEquals(default(CopyMethod), target.CopyMethod);
			AssertNull(target.Value);
			AssertEquals("abc", target.DefaultValue);
			AssertEquals("string", target.PropertyType);
		}

		#endregion

		#region Implementation

		protected override PropertyCopyTemplateNode CreateConfigurationNode()
		{
			return new PropertyCopyTemplateNode(typeof(ITestModel).GetProperty(ExpectedName, BindingFlags.Public | BindingFlags.Instance));
		}

		protected override string ExpectedName
		{
			get { return "TM_Number"; }
		}

		#endregion
	}
}
