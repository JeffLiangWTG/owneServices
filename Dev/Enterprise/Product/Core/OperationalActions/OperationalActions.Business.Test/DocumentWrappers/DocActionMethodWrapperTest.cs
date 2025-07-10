using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocActionMethodWrapper))]
	internal sealed class DocActionMethodWrapperTest : DocumentWrapperTest
	{
		public void TestGroup()
		{
			DocActionMethodWrapper wrapper = DocActionMethodWrapper.New("Group", Method);
			AssertEquals("Group", wrapper.Group);
		}

		public void TestName()
		{
			DocActionMethodWrapper wrapper = DocActionMethodWrapper.New("Group", Method);
			AssertEquals("Dummy Action Method With GUI", wrapper.Name);
		}

		public void TestDescription()
		{
			const string expected = "A dummy defined process strictly for the purpose of testing the operational " + "actions system (not for general use)." + "";
			DocActionMethodWrapper wrapper = DocActionMethodWrapper.New("Group", Method);
			AssertEquals(expected, Method.Description);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return DocActionMethodWrapper.New(ActionMethodProviderIDs.DummyWithMethods.Name, Method);
		}

		OperationalActionMethod Method
		{
			get
			{
				return method ?? (method = Supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI));
			}
		}

		OperationalActionSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					supporter = new MockOperationalActionSupportable().OperationalActionSupporter;
					supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
				}

				return supporter;
			}
		}

		OperationalActionMethod method;
		OperationalActionSupporter supporter;
		#endregion
	}
}
