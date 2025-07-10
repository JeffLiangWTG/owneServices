using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	[TestedType(typeof(DocActionManagerWrapper))]
	internal sealed class DocActionManagerWrapperTest : DocumentWrapperTest
	{
		public void TestModuleName()
		{
			DocActionManagerWrapper wrapper = DocActionManagerWrapper.New(Manager);
			AssertEquals("Module Name", wrapper.ModuleName);
		}

		public void TestMethods()
		{
			Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			DocActionManagerWrapper wrapper = DocActionManagerWrapper.New(Manager);
			List<string> expected = new List<string>();
			foreach (ActionMethodProviderID id in Supporter.Methods.GetAllIds())
			{
				foreach (OperationalActionMethod method in Supporter.Methods.GetMethods(id))
				{
					expected.Add(string.Format("{0}/{1}", id.Name, method.Name));
				}
			}

			string[] actual = Array.ConvertAll(wrapper.Methods.ToArray<DocActionMethodWrapper>(), (w) => string.Format("{0}/{1}", w.Group, w.Name));
			AssertContainsExactElementsInAnyOrder("Should contain a wrapper for each method", expected, actual);
		}

		public void TestConstraints()
		{
			DocActionManagerWrapper wrapper = DocActionManagerWrapper.New(Manager);
			AssertContainsExactElementsInAnyOrder("Should contain a wrapper for each constraint", (c) => c.Name, EnvironmentFilterProvider.Instance, Array.ConvertAll(wrapper.Constraints.ToArray<DocConstraintWrapper>(), (c) => (IFilterConstraint)c.WrappedObject));
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return DocActionManagerWrapper.New(Manager);
		}

		OperationalActionManager Manager
		{
			get
			{
				return manager ?? (manager = new OperationalActionManager(Factory, new OperationalActionContext(Supporter, "Module Name")));
			}
		}

		OperationalActionManager manager;
		OperationalActionSupporter Supporter
		{
			get
			{
				return supporter ?? (supporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter supporter;
		#endregion
	}
}
