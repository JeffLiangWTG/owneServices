using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroEvaluator.Testing
{
	[TestedType(typeof(FilterEvaluatorManager))]
	sealed class FilterEvaluatorManagerTest : EvaluatorManagerTest
	{
		public void TestEvaluateFilter()
		{
			Manager.Macro = "CTY=AU";
			Manager.Evaluate();
			AssertEquals("CTY=AU", "True", Manager.Output);

			Manager.Macro = "CTY=US";
			Manager.Evaluate();
			AssertEquals("CTY=US", "False", Manager.Output);

			Manager.Macro = "\"<UseDocBuilderFreightDocs>\" != \"Y\"";
			Manager.Evaluate();
			AssertEquals("\"<UseDocBuilderFreightDocs>\" != \"Y\"", "True", Manager.Output);

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Manager.Evaluate();
				AssertEquals("Docbuilder Freight Docs enabled", "False", Manager.Output);
			}
		}

		public void TestInvalidFilter()
		{
			Manager.Macro = "\"<CurrentCompany.Country.Code>\"==AU";
			AssertEquals("Result of \"AU\"==AU is not a True/False expression", Manager.Evaluate());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var dataContext = new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob));
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var manager = new FilterEvaluatorManager(Factory, dummy);

			return manager;
		}

		FilterEvaluatorManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = new FilterEvaluatorManager(Factory, ShipmentBO as IDocumentSupportable);
				}

				return manager;
			}
		}
		FilterEvaluatorManager manager;

		#endregion
	}
}
