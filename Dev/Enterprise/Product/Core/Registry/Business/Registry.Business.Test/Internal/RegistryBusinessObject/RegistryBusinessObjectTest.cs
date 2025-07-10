using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DummyRegistryBusinessObject))]
	public abstract class RegistryBusinessObjectTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestDefaultMaxCodeLength()
		{
			AssertEquals("MaxCodeLength", ExpectedDefaultMaxCodeLength, BizObj.CodeMaxLength);
		}

		protected virtual int ExpectedDefaultMaxCodeLength
		{
			get { return 3; }
		}

		public void TestCodeMaxLength()
		{
			AssertEquals("Default MaxCodeLength", ExpectedDefaultMaxCodeLength, BizObj.CodeMaxLength);
			AssertEquals("Property Info", ExpectedDefaultMaxCodeLength, BizObj.CodeInfo.MaxLength);
			BizObj.CodeMaxLength = 55;
			AssertEquals("Value should change", 55, BizObj.CodeMaxLength);
			AssertEquals("Property Info", 55, BizObj.CodeInfo.MaxLength);
		}

		public virtual void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 35, BizObj.DescriptionInfo.MaxLength);
		}

		public virtual void TestCloneValues()
		{
			AssertCloneValues((RegistryBusinessObject)((RegistryBusinessObject)GetBusinessObjectToClone()).Clone(null, null));
		}

		#region Implementation

		protected abstract void AssertCloneValues(RegistryBusinessObject clone);

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.CodeMaxLength = 5;
			BizObj.Code = "ABCDE";
			BizObj.Description = (NoResString)"Desc";

			return BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
