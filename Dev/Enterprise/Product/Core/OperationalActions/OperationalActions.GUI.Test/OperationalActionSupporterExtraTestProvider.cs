using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	/// <summary>
	/// This class is instantiated by OperationalActionSupporterTest<> in OperationalActions.Support
	/// (using reflection) to implement tests requiring knowledge of classes defined
	sealed class OperationalActionSupporterExtraTestProvider<SupporterT> : OperationalActionSupporterTest<SupporterT>.IExtraTestProvider
			where SupporterT : OperationalActionSupporter
	{
		public OperationalActionSupporterExtraTestProvider(OperationalActionSupporterTest<SupporterT> test)
		{
			this.test = test;
		}

		public void TestModuleCorrectlySupportsOperationalActions()
		{
			string moduleType = Module.GetType().Name;
			Assertion.AssertNotNull(string.Format("{0} does not use the OperationalActions plugin", moduleType), Module.Plugins.GetPlugin(ControllerIDs.OperationalActions));

			IOperationalActionSupportable supportable = Module as IOperationalActionSupportable;
			Assertion.AssertNotNull(string.Format("{0} does not implement IOperationalActionSupportable", moduleType), supportable);

			OperationalActionSupporter supporter = supportable.OperationalActionSupporter;
			Assertion.AssertNotNull(string.Format("{0}.OperationalActionSupporter returned null", moduleType), supporter);
			Assertion.AssertEquals(string.Format("{0}.OperationalActionSupporter returned the wrong supporter", moduleType), typeof(SupporterT), supporter.GetType());

			OperationalActionContext context = new OperationalActionContext(supporter, "Module Filter");

			if (test.ShouldSupportDocuments)
			{
				Assertion.Assert(
					string.Format("{0} does not implemnt IDocumentSupportable (if this is correct then override ShouldSupportDocuments on the test class to return false)", Module.GridCollection.TypeOfElements),
					typeof(IDocumentSupportable).IsAssignableFrom(Module.GridCollection.TypeOfElements));
			}
			else
			{
				Assertion.Assert(
					string.Format("{0} implemnts IDocumentSupportable, you dont need ShouldSupportDocuments to return false anymore", Module.GridCollection.TypeOfElements),
					!typeof(IDocumentSupportable).IsAssignableFrom(Module.GridCollection.TypeOfElements));
			}

			Assertion.Assert("Supporter.RootType is Module.GridCollection.TypeOfElements", supporter.RootType.IsAssignableFrom(Module.GridCollection.TypeOfElements));
		}

		public void TestBusinessContextIsConsistent()
		{
			IDocumentSupportable docSupportable = test.NewTarget() as IDocumentSupportable;

			if (docSupportable != null)
			{
				Assertion.AssertEquals("The supporter should have the same business context as the target.", docSupportable.DocumentSupporter.BusinessContext, Supporter.BusinessContext);
			}
			else
			{
				Assertion.Assert(true);
			}
		}

		public void TestActionsOnlyReferenceFieldsThatExist()
		{
			IDictionary<string, IList<string>> brokenActions = new SortedDictionary<string, IList<string>>();
			IList<string> brokenFields = new List<string>();

			foreach (OperationalAction action in GetApplicableActions())
			{
				OperationalActionContext context = action.Context;

				foreach (OperationalActionFieldDescriptor descriptor in action.FieldDescriptors)
				{
					if (context.FieldSupporters[descriptor.FieldName] == null)
					{
						brokenFields.Add(descriptor.FieldName);
					}
				}

				if (brokenFields.Count > 0)
				{
					brokenActions.Add(ActionName(action), brokenFields);
					brokenFields = new List<string>();
				}
			}

			Assertion.AssertGroupedErrorList("The following actions refer to fields that dont exist.", brokenActions);
		}

		public void TestActionsOnlyReferenceMethodsThatExist()
		{
			IDictionary<string, IList<string>> brokenActions = new SortedDictionary<string, IList<string>>();
			IList<string> brokenMethods = new List<string>();

			foreach (OperationalAction action in GetApplicableActions())
			{
				foreach (OperationalActionMethodDescriptor methodDescriptor in action.MethodDescriptors)
				{
					if (methodDescriptor.Method == null)
					{
						brokenMethods.Add(string.Format("'{0}' - '{1}'", methodDescriptor.MethodGroup, methodDescriptor.MethodID));
					}
				}

				if (brokenMethods.Count > 0)
				{
					brokenActions.Add(ActionName(action), brokenMethods);
					brokenMethods = new List<string>();
				}
			}

			Assertion.AssertGroupedErrorList("The following actions refer to operational action methods that dont exist.", brokenActions);
		}

		public void TestAllMethodSettingsPassValidation()
		{
			IDictionary<string, IList<string>> brokenSettings = new SortedDictionary<string, IList<string>>();

			foreach (OperationalAction action in GetApplicableActions())
			{
				foreach (OperationalActionMethodDescriptor descriptor in action.MethodDescriptors)
				{
					OperationalActionMethodSettings settings = descriptor.Settings;
					if (settings == null)
					{
						continue;
					}

					settings.RunPreSaveValidation();
					if (!settings.HasErrors)
					{
						continue;
					}

					string name = string.Format("{0}: '{1}'-'{2}' [{3}]",
						ActionName(action), descriptor.MethodGroup, descriptor.MethodName, descriptor.Order);

					IList<string> errors = descriptor.Settings.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList();

					brokenSettings.Add(name, errors);
				}
			}

			Assertion.AssertGroupedErrorList("The following operational action method settings have errors", brokenSettings);
		}

		public void TestActionFiltersAreValid()
		{
			Dictionary<string, IList<string>> brokenActions = new Dictionary<string, IList<string>>();
			IList<string> errors = new List<string>();

			foreach (OperationalAction action in GetApplicableActions())
			{
				try
				{
					IFilterExpression expression = FilterParser.Parse(action.SU_FilterList);

					foreach (string constrant in expression.GetConstraints())
					{
						if (EnvironmentFilterProvider.Instance.GetConstraint(constrant) == null)
						{
							errors.Add(string.Format("'{0}' is not a valid constraint.", constrant));
						}
					}
				}
				catch (ParseException ex)
				{
					errors.Add(string.Format("Failed to parse '{0}'\nError:{1}", action.SU_FilterList, ex.Message));
				}

				if (errors.Count > 0)
				{
					brokenActions.Add(ActionName(action), errors);
					errors = new List<string>();
				}
			}

			Assertion.AssertGroupedErrorList("These actions have broken filters.", brokenActions);
		}

		public void TestFieldDefaultsAreValid()
		{
			Dictionary<string, IList<string>> brokenActions = new Dictionary<string, IList<string>>();
			IList<string> errors = new List<string>();

			foreach (OperationalAction action in GetApplicableActions())
			{
				foreach (OperationalActionFieldDescriptor descriptor in action.FieldDescriptors)
				{
					if (descriptor.DefaultingStrategy.IsEmpty)
					{
						continue;
					}

					if (descriptor.FieldSupporter == null)
					{
						continue;
					}

					IFieldDefaultingStrategy strategy;

					if (!descriptor.Lookups.DefaultStrategy_List.ContainsCode(descriptor.DefaultingStrategy) ||
						(strategy = descriptor.Lookups.DefaultStrategy_List[descriptor.DefaultingStrategy]) == null)
					{
						errors.Add(string.Format("'{0}' is not a valid defaulting strategy for '{1}'", descriptor.DefaultingStrategy, descriptor.FieldName));
					}
					else if (!strategy.GetDefaultValue(descriptor.DefaultValue).IsValid)
					{
						errors.Add(string.Format("'{0}' is not a valid default value for '{1}'", descriptor.DefaultValue, descriptor.FieldName));
					}
				}

				if (errors.Count > 0)
				{
					brokenActions.Add(ActionName(action), errors);
					errors = new List<string>();
				}
			}

			Assertion.AssertGroupedErrorList("These actions have broken field default values", brokenActions);
		}

		/// <summary>
		/// Ensure that no operational actions have made it into the database without passing validation
		/// </summary>
		public void TestExistingActionsPassValidation()
		{
			var brokenActions = new Dictionary<string, IList<string>>();
			foreach (OperationalAction action in GetApplicableActions())
			{
				action.Validation.ValidateAll();
				if (action.HasErrors)
				{
					var actionErrors = action.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList();
					brokenActions.Add(ActionName(action), actionErrors);
				}
			}
			Assertion.AssertGroupedErrorList("The following existing operational actions failed validation.", brokenActions);
		}

		public void TestDocumentBusinessContextIsConsistent()
		{
			IDocumentSupportable docSupportable = test.NewTarget() as IDocumentSupportable;

			if (docSupportable != null)
			{
				Assertion.AssertEquals("The supporter should have the same business context as the target.", docSupportable.DocumentSupporter.BusinessContext, Supporter.DocumentBusinessContext);
			}
			else
			{
				Assertion.Assert(true);
			}
		}

		#region Implementation

		public OperationalActionCollection GetApplicableActions()
		{
			OperationalActionContext context = new OperationalActionContext(Supporter, "Module Name");
			OperationalActionCollection collection = new OperationalActionCollection(Factory, context);
			collection.Load();

			ApplyFilterList(collection);

			return collection;
		}

		void ApplyFilterList(OperationalActionCollection collection)
		{
			foreach (var action in collection.Cast<OperationalAction>().ToList())
			{
				var expression = FilterParser.Parse(action.SU_FilterList);
				if (!expression.Evaluate(EnvironmentFilterProvider.Instance))
				{
					collection.Remove(action);
				}
			}
		}

		public string ActionName(OperationalAction action)
		{
			return MenuPathComparer.NormalisePath(action.SU_MenuPath) + "/" + action.SU_MenuName;
		}

		ZFilterGridModule Module
		{
			get { return test.Module; }
		}

		SupporterT Supporter
		{
			get { return test.Supporter; }
		}

		BusinessObjectFactory Factory
		{
			get { return test.Factory; }
		}

		readonly OperationalActionSupporterTest<SupporterT> test;

		#endregion
	}
}
