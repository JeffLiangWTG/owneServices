using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocActionMethodWrapperCollection))]
	internal sealed class DocActionMethodWrapperCollectionTest : DocumentWrapperCollectionTest<DocActionMethodWrapperCollection>
	{
		public void TestNew()
		{
			DocActionMethodWrapperCollection collection = DocActionMethodWrapperCollection.New(Supporter);
			List<string> expected = new List<string>();
			foreach (ActionMethodProviderID id in Supporter.Methods.GetAllIds())
			{
				foreach (OperationalActionMethod method in Supporter.Methods.GetMethods(id))
				{
					expected.Add(string.Format("{0}/{1}", id.Name, method.Name));
				}
			}

			string[] actual = Array.ConvertAll(collection.ToArray<DocActionMethodWrapper>(), (w) => string.Format("{0}/{1}", w.Group, w.Name));
			AssertContainsExactElementsInAnyOrder("Should contain a wrapper for each method", expected, actual);
		}

		#region Implementation
		protected override DocActionMethodWrapperCollection GetNewDocumentWrapperCollection()
		{
			return DocActionMethodWrapperCollection.New(Supporter);
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			DocActionMethodWrapper wrapper = DocActionMethodWrapper.New(ActionMethodProviderIDs.DummyWithMethods.Name, Method);
			collection.Add(wrapper);
			return wrapper;
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
