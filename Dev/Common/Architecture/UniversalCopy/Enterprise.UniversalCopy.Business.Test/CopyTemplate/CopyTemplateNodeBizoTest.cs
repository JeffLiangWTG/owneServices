using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.UniversalCopy.Business.Testing
{
	abstract class ConfigurationNodeBizoTest<T> : NonPersistentBusinessObjectTestCase
		where T : CopyTemplateNodeBizo
	{
		public virtual void TestHumanReadableName()
		{
			var nodeBizo = GetNewCopyTemplateNodeBizo();

			nodeBizo.CopyTemplateNode.Name = "xyz";

			AssertEquals("xyz", nodeBizo.HumanReadableName);
		}

		public void TestName()
		{
			T nodeBizo = GetNewCopyTemplateNodeBizo();

			nodeBizo.CopyTemplateNode.Name = "xyz";
			AssertEquals("xyz", nodeBizo.Name);
			Assert(!nodeBizo.NameInfo.HasNotifications());

			nodeBizo.CopyTemplateNode.Name = "";
			AssertEquals("", nodeBizo.Name);
			Assert(!nodeBizo.NameInfo.HasNotifications());
		}

		public void TestDescription()
		{
			T nodeBizo = GetNewCopyTemplateNodeBizo();

			nodeBizo.CopyTemplateNode.Name = "xyz";
			nodeBizo.CopyTemplateNode.Description = "abc";
			AssertEquals("abc", nodeBizo.Description);
			Assert(!nodeBizo.DescriptionInfo.HasNotifications());

			nodeBizo.CopyTemplateNode.Description = "";
			AssertEquals("Should fallback to name", "xyz", nodeBizo.Description);
			Assert(!nodeBizo.DescriptionInfo.HasNotifications());

			nodeBizo.CopyTemplateNode.Name = "";
			AssertEquals("", nodeBizo.Description);
			Assert(!nodeBizo.DescriptionInfo.HasNotifications());

			nodeBizo.Description = "abc2";
			AssertEquals("Should fallback to name", "abc2", nodeBizo.CopyTemplateNode.Description);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewCopyTemplateNodeBizo();
		}

		protected abstract T GetNewCopyTemplateNodeBizo();

		#endregion
	}
}
