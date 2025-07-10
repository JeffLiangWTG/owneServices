using System.Text;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	/// <summary>
	/// Adds an error when the specified child module is not ticked compared to a list of dependent modules
	/// This is best used to ensure that a logical dependency between modules can be defined.
	/// </summary>
	public class DependencyBetweenModulesRule
	{
		public DependencyBetweenModulesRule(string childModuleCode, string[] dependsOnTheseModuleCodes)
		{
			this.ChildModuleCode = childModuleCode;
			this.DependsOnTheseModuleCodes = dependsOnTheseModuleCodes;
		}

		#region Execute Rule

		public void ExecuteRule(LicenceModules actionedModule)
		{
			LicenceModules child = actionedModule.LicHeader.Modules.FindByCode(ChildModuleCode);

			if (ModuleIsAParentToADependentChild(actionedModule.LM_GroupModuleCode))
			{
				ExecuteRule(child);
			}
			else if (actionedModule.LM_GroupModuleCode == ChildModuleCode)
			{
				child.ClearRowNotifications();

				if (child.LM_Calc_IsEnabled && !AllDependentModulesTicked(actionedModule))
				{
					actionedModule.AddRowError(GeneratedErrorMessage);
				}
			}
		}

		bool AllDependentModulesTicked(LicenceModules actionedModule)
		{
			bool result = true;
			foreach (var dependentCode in DependsOnTheseModuleCodes)
			{
				LicenceModules module = actionedModule.LicHeader.Modules.FindByCode(dependentCode);
				if (!module.LM_Calc_IsEnabled)
				{
					result = false;
				}
			}

			return result;
		}

		bool ModuleIsAParentToADependentChild(string moduleCode)
		{
			bool result = false;
			foreach (var dependentCode in DependsOnTheseModuleCodes)
			{
				if (moduleCode == dependentCode)
				{
					result = true;
				}
			}

			return result;
		}

		string GeneratedErrorMessage
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (var dependentCode in DependsOnTheseModuleCodes)
				{
					builder.Append(dependentCode + System.Environment.NewLine);
				}

				return "The Module " + ChildModuleCode + " is dependent on the following modules: " + System.Environment.NewLine + builder.ToString() + System.Environment.NewLine
					+ "If you have all of the dependent modules ticked, then you must tick the module " + ChildModuleCode + ". " + System.Environment.NewLine + "Similarly, if you have " + ChildModuleCode
					+ " ticked then you must tick all of the dependent modules.";
			}
		}

		readonly string ChildModuleCode;
		readonly string[] DependsOnTheseModuleCodes;

		#endregion
	}
}
