using System;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskValidation : AutoStmServiceTaskValidation
	{
		public StmServiceTaskValidation(AutoStmServiceTask parent) : base(parent)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateSecondaryProcessesMaxCount();
			ValidateSecondaryProcessesMaxCountDescription();
		}

		protected new StmServiceTask Parent => (StmServiceTask)base.Parent;

		public void ValidateSecondaryProcessesMaxCount()
		{
			ValidateCalculatedProperty(Parent.SecondaryProcessesMaxCountInfo);
		}

		protected void CheckSecondaryProcessesMaxCount()
		{
			Parent.ExtendedConfigProcessesMaxCountWarningMessage = string.Empty;
			Parent.ExtendedConfigProcessesMaxCountWarningLink = string.Empty;

			var result = GetTaskSpecificSecondaryProcessValidation()?.Validate();

			if (result == null)
			{
				return;
			}

			foreach (var error in result.Errors)
			{
				Parent.SecondaryProcessesMaxCountInfo.AddError(error);
			}

			foreach (var warning in result.PropertySpecificWarnings)
			{
				var propertyName = warning.Key;
				var value = warning.Value;
				var propertyInfo = Parent.GetType().GetProperty(propertyName);
				if (propertyInfo != null)
				{
					if (propertyInfo.PropertyType == typeof(ZString))
					{
						propertyInfo.SetValue(Parent, new ZString(value));
					}
					else
					{
						propertyInfo.PropertyType
							.GetMethod("AddWarning")
							?.Invoke(propertyInfo.GetValue(Parent), new object[] { value });
					}
				}
			}

			IServiceTaskSpecificValidation GetTaskSpecificSecondaryProcessValidation()
			{
				var typeName = Parent.StaticServiceAttributes.TaskSpecificValidationTypeName;
				var typeAssemblyName = Parent.StaticServiceAttributes.TaskSpecificValidationTypeAssemblyName;

				if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(typeAssemblyName))
				{
					return null;
				}

				return (IServiceTaskSpecificValidation)Activator.CreateInstance(
					Type.GetType($"{typeName}, {typeAssemblyName}", throwOnError: true),
					(int)Parent.SecondaryProcessesMaxCount);
			}
		}

		public void ValidateSecondaryProcessesMaxCountDescription()
		{
			ValidateCalculatedProperty(Parent.SecondaryProcessesMaxCountDescriptionInfo);
		}

		protected void CheckSecondaryProcessesMaxCountDescription()
		{
			if (Parent.StmServiceTaskSecondaryProcessesMaxCountsList.GetCodeFromDescription(Parent.SecondaryProcessesMaxCountDescription) == null)
			{
				Parent.SecondaryProcessesMaxCountDescriptionInfo.AddError(Res.GetString("4f5773bb-b379-4f10-967d-fde3a4d7cf20", "Please enter a valid maximum count for secondary processes."));
			}
		}
	}
}
