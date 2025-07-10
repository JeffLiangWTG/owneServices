#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class PasswordAttributeTests : TestCase
	{
		public void TestGetMetaDataValue()
		{
			{
				KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantPassword1"];
				bool value = MetaData.GetPassword(new TestComponent(), property);
				AssertEquals("Constant Password value", true, value);
			}

			{
				KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantPassword2"];
				bool value = MetaData.GetPassword(new TestComponent(), property);
				AssertEquals("Constant Password value", false, value);
			}

			{
				KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())[" PropertyWithoutPassword"];
				bool value = MetaData.GetPassword(new TestComponent(), property);
				AssertEquals("Default Password value", false, value);
			}
		}

		public void TestGetMetaDataValueMember()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithCalculatedPasswordMember"];
			bool value = MetaData.GetPassword(new TestComponent(), property);
			AssertEquals("Default Password value", true, value);
		}

		internal class TestComponent : KComponent
		{
			[Password]
			public string PropertyWithConstantPassword1
			{ get { return ""; } }

			[Password(false)]
			public string PropertyWithConstantPassword2
			{ get { return ""; } }

			public int PropertyWithoutPassword
			{ get { return 5; } }

			[Password(nameof(IsPassword))]
			public string PropertyWithCalculatedPasswordMember
			{ get { return ""; } }

			public bool IsPassword => true;
		}
	}
}
#endif
