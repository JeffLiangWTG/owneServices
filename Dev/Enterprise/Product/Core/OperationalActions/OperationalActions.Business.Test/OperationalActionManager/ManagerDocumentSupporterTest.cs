using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class ManagerDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestContexts()
		{
			string[] supportedDataContextsIncludingRetardedEntries = DocumentSupporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals("BusinessContext", BusinessContext.OperationalActions, DocumentSupporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("DataContext", new string[] { nameof(DataContext.OperationalActions) }, supportedDataContexts);
		}

		public void TestGetDocumentWrappers()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.OperationalActions, null);
			AssertHasWrappersFor(DataContext.OperationalActions, DocumentDirection.ANY, "DocActionManagerWrapper", Manager);
		}

		#region Implementation
		void AssertHasWrappersFor(DataContext context, DocumentDirection direction, string expectedWrapperName, params object[] expected)
		{
			List<object> wrapped = new List<object>();
			DocumentWrapper[] wrapers = DocumentSupporter.GetDocumentWrappers(context, StmMenuItem.GetForTesting(direction)) ?? Array.Empty<DocumentWrapper>();
			foreach (DocumentWrapper wrapper in wrapers)
			{
				object wrappedObject = wrapper.WrappedObject;
				AssertEquals(string.Format("Wrapper type name for '{0}'", wrappedObject), expectedWrapperName, wrapper.GetType().Name);
				wrapped.Add(wrappedObject);
			}

			AssertContainsExactElementsInAnyOrder(expected, wrapped);
		}

		OperationalActionManager Manager
		{
			get
			{
				if (manager == null)
				{
					OperationalActionSupporter actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter;
					manager = new OperationalActionManager(Factory, new OperationalActionContext(actionSupporter, "ModuleName"));
				}

				return manager;
			}
		}

		ManagerDocumentSupporter DocumentSupporter
		{
			get
			{
				if (documentSupporter == null)
				{
					documentSupporter = (ManagerDocumentSupporter)Manager.DocumentSupporter;
				}

				return documentSupporter;
			}
		}

		OperationalActionManager manager;
		ManagerDocumentSupporter documentSupporter;
		#endregion
	}
}
