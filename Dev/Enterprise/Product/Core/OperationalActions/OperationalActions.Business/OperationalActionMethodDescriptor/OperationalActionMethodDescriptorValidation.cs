using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionMethodDescriptorValidation : AutoOperationalActionMethodDescriptorValidation
	{
		public OperationalActionMethodDescriptorValidation(AutoOperationalActionMethodDescriptor parent)
			: base(parent) { }

		protected override void CheckMethodGroup()
		{
			if (Parent.Action.ReadOnly)
			{
				return;
			}
			MandatoryValidation.CheckEntered(Parent.MethodGroupInfo);
			ListValidation.ErrorIfInvalidPK(Parent.MethodGroupInfo, Parent.Lookups.MethodGroups);
		}

		protected override void CheckMethodID()
		{
			CodeDescriptionPairList list = Parent.Lookups.MethodNames;

			if (list.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.MethodIDInfo);

				if (IsGroupAndNameDuplicated())
				{
					Parent.MethodIDInfo.AddError(Res.GetString("{7B4B79B7-C97C-4dea-B4F2-D095F8895C9E}", "This process already exists on this action."));
				}

				OperationalActionMethod method = Parent.Method;

				if (method == null)
				{
					Parent.MethodIDInfo.AddError(Res.GetString("{7A0D48C3-A1FB-4bd1-BC0C-97A5F11F3A66}", "Enter a valid selection."));
				}
				else
				{
					OperationalAction action = Parent.Action;

					try
					{
						IFilterExpression expression = FilterParser.Parse(action.SU_FilterList);

						foreach (FilterRequirement requirement in method.GetFilterRequirements())
						{
							IFilterConstraint constraint = EnvironmentFilterProvider.Instance.GetConstraint(requirement.ConstraintName);

							if (constraint != null && !expression.IsRequirementEnforced(requirement))
							{
								StringBuilder builder = new StringBuilder();

								builder.Append(Res.GetString("d4bd68a1-8dd4-4669-9449-2640c50be60f",
									"This method is only valid for the following {0} but this is not reflected in the filter:",
									requirement.Count == 1 ? constraint.SingularValueName : constraint.PluralValueName));

								foreach (string value in requirement)
								{
									builder.Append(' ');
									builder.Append(value);
								}

								Parent.MethodIDInfo.AddError(builder.ToString());
							}
						}
					}
					catch (ParseException)
					{
						// ignore.
					}
				}
			}
		}

		protected override void CheckOrder()
		{
			MandatoryValidation.CheckEntered(Parent.OrderInfo);
			if (Parent.ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.OrderInfo);
			}
		}

		#region Implementation

		bool IsGroupAndNameDuplicated()
		{
			OperationalActionMethodDescriptorCollection collection = FindCollection(Parent);

			if (collection != null)
			{
				foreach (OperationalActionMethodDescriptor descriptor in collection)
				{
					if (descriptor != Parent && descriptor.MethodGroup == Parent.MethodGroup && descriptor.MethodID == Parent.MethodID)
					{
						return true;
					}
				}
			}

			return false;
		}

		static OperationalActionMethodDescriptorCollection FindCollection(OperationalActionMethodDescriptor descriptor)
		{
			foreach (BusinessObjectCollection collection in descriptor.ParentCollections)
			{
				OperationalActionMethodDescriptorCollection result = collection as OperationalActionMethodDescriptorCollection;

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		new OperationalActionMethodDescriptor Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionMethodDescriptor)base.Parent; }
		}

		#endregion
	}
}
