using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public abstract class DocDataObject : NonPersistentBusinessObject, IDocDataObject, IValueChangedActionSupporter, IObsoleteValidation
	{
		protected DocDataObject(object identifier = default)
			: base()
		{
			Identifier = identifier ?? PK;
		}

		protected DocDataObject(BusinessObjectFactory factory, object identifier = default)
			: base(factory)
		{
			Identifier = identifier ?? PK;
		}

		public object Identifier { get; }

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return valueChangedActions != null
				? new DocDataObjectValueSetStrategy(valueChangedActions)
				: null;
		}

		#region IValueChangedActionSupporter members

		void IValueChangedActionSupporter.AddOnValueChangedAction(string propertyName, Action action)
		{
			if (string.IsNullOrWhiteSpace(propertyName)
				|| action == null)
			{
				return;
			}

			if (valueChangedActions == null)
			{
				valueChangedActions = new Dictionary<string, List<Action>>();
			}

			if (valueChangedActions.TryGetValue(propertyName, out var actions))
			{
				actions.Add(action);
			}
			else
			{
				valueChangedActions[propertyName] = new List<Action>
				{
					action
				};
			}
		}

		IDictionary<string, List<Action>> valueChangedActions;

		#endregion

		#region IAdHocValidationSupporter members

		public bool AddValidationRule(string propertyName, Core.NotificationType notificationType, Func<bool> validationRule, string errorMessage)
		{
			if (string.IsNullOrWhiteSpace(propertyName)
				|| notificationType == Core.NotificationType.None
				|| validationRule == null
				|| string.IsNullOrWhiteSpace(errorMessage))
			{
				return false;
			}

			if (validations == null)
			{
				validations = new Dictionary<string, List<Validator>>();
			}

			if (!validations.TryGetValue(propertyName, out List<Validator> list))
			{
				list = new List<Validator>();
				validations.Add(propertyName, list);
			}

			list.Add(new Validator
			{
				NotificationType = notificationType,
				Rule = validationRule,
				Message = errorMessage
			});

			return true;
		}

		Dictionary<string, List<Validator>> validations;

		public void Validate(params string[] propertyNames)
		{
			if (propertyNames == null)
			{
				return;
			}

			foreach (var propertyName in propertyNames)
			{
				var propertyInfo = ZPropertyInfoHash.GetPropertySafe(propertyName);

				if (propertyInfo != null)
				{
					Validate(propertyInfo);
				}
			}
		}

		public void ValidateAll()
		{
			foreach (var propertyInfo in ZPropertyInfoHash.OfType<ZPropertyInfo>())
			{
				Validate(propertyInfo);
			}
		}

		public void ValidateAllIncludingChildren()
		{
			ValidateAll();

			foreach (var child in ((IBusiness)this).Children.OfType<IAdHocValidationSupporter>())
			{
				child.ValidateAllIncludingChildren();
			}
		}

		#endregion

		#region Validation

		protected bool Validate(ZPropertyInfo info)
		{
			if (IsValidationSuspended)
			{
				return false;
			}

			info.ClearAllNotifications();

			if (info is ZWrappedPropertyInfo wrappedInfo)
			{
				var docDataObj = wrappedInfo.InnerInfo?.BizObj as DocDataObject;
				docDataObj?.Validate(wrappedInfo.InnerInfo);
			}

			if (validations == null
				|| !validations.TryGetValue(info.Name, out List<Validator> validators))
			{
				return true;
			}

			foreach (var validator in validators)
			{
				if (!validator.Rule())
				{
					continue;
				}

				switch (validator.NotificationType)
				{
					case Core.NotificationType.Warning:
						info.AddWarning(validator.Message);
						break;

					case Core.NotificationType.MessageError:
						info.AddMessageError(validator.Message);
						break;

					case Core.NotificationType.Error:
						info.AddError(validator.Message);
						break;

					case Core.NotificationType.DeliveryError:
						info.Add(new NotificationType(nameof(Core.NotificationType.DeliveryError), 500, false, nameof(Core.NotificationType.DeliveryError)), validator.Message);
						break;
				}
			}

			return true;
		}

		#endregion

		#region Children

		protected T SetChild<T>(T previousChild, T newChild) where T : class
		{
			if (previousChild is IBusiness previousChildBiz)
			{
				UnRegisterEditableChildObject(previousChildBiz);
			}

			if (newChild is IBusiness newChildBiz)
			{
				RegisterEditableChildObject(newChildBiz);
			}

			return newChild;
		}

		protected IReadOnlyCollection<T> SetChildCollection<T>(IReadOnlyCollection<T> previousChild, IReadOnlyCollection<T> newChild) where T : class
		{
			if (previousChild != null)
			{
				foreach (var element in previousChild.OfType<IBusiness>())
				{
					UnRegisterEditableChildObject(element);
				}
			}

			if (newChild != null)
			{
				foreach (var element in newChild.OfType<IBusiness>())
				{
					RegisterEditableChildObject(element);
				}
			}

			return newChild;
		}

		#endregion

		#region Nested Types

		struct Validator
		{
			public Core.NotificationType NotificationType { get; set; }
			public Func<bool> Rule { get; set; }
			public string Message { get; set; }
		}

		#endregion
	}
}
