using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public abstract class RunnerField : NonPersistentBusinessObject, IObsoleteValidation, IOperationalActionFieldValuePair
	{
		protected RunnerField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			this.descriptor = descriptor;
		}

		public ZString Caption
		{
			get { return Descriptor == null ? ZString.Empty : Descriptor.FieldCaption; }
		}

		public OperationalActionFieldDescriptor Descriptor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return descriptor; }
		}

		public virtual bool ShouldApply
		{
			get { return false; }
		}

		#region IOperationalActionFieldValuePair Members

		IFilterExpression IOperationalActionFieldValuePair.Filter
		{
			get
			{
				try
				{
					return FilterParser.Parse(descriptor.Filter);
				}
				catch (ParseException)
				{
					return new AST.FilterEmpty();
				}
			}
		}

		OperationalActionFieldSupporter IOperationalActionFieldValuePair.Field
		{
			get { return FieldCore; }
		}
		protected abstract OperationalActionFieldSupporter FieldCore { get; }

		IZType IOperationalActionFieldValuePair.GetValue(Type expectedType)
		{
			throw new NotSupportedException();
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionFieldDescriptor descriptor;
	}

	[System.Diagnostics.DebuggerDisplay("Name = {FieldSupporter.Name}")]
	public abstract class RunnerField<TSupporter, TProperty> : RunnerField, IOperationalActionFieldValuePair
		where TSupporter : OperationalActionFieldSupporter
		where TProperty : struct, IZType
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Static holder types should be Static")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Schema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
			public const string Property = "Property";
		}

		#endregion

		protected RunnerField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, TSupporter fieldSupporter)
			: base(factory, descriptor)
		{
			if (fieldSupporter == null)
			{
				throw new ArgumentNullException(nameof(fieldSupporter));
			}

			this.fieldSupporter = fieldSupporter;
			this.property = descriptor.GetDefaultValue<TProperty>();
		}

		public virtual TProperty Property
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property; }
			set
			{
				SetPropertyCore(value);

				if (!IsValidationSuspended)
				{
					ValidateProperty();
				}
			}
		}

		public ZPropertyInfo PropertyInfo
		{
			get { return GetPropertyInfoCore(); }
		}

		public void ValidateProperty()
		{
			PropertyInfo.ClearAllNotifications();

			switch (Descriptor.EmptyBehaviour)
			{
				case EmptyBehaviourList.Codes.Mandatory:
					MandatoryValidation.CheckEntered(PropertyInfo);
					break;

				case EmptyBehaviourList.Codes.Apply:
					MandatoryValidation.WarnIfNotEntered(PropertyInfo);
					break;
			}

			ValidatePropertyCore();
		}

		protected virtual void SetPropertyCore(TProperty value)
		{
			SetNonPersistentPropertyValue(PropertyInfo, ref property, value);
		}

		protected virtual ZPropertyInfo GetPropertyInfoCore()
		{
			return GetZPropertyInfo(Schema.Property, Caption);
		}

		protected virtual void ValidatePropertyCore()
		{
		}

		public TSupporter FieldSupporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fieldSupporter; }
		}

		#region IOperationalActionFieldValuePair Members

		protected override OperationalActionFieldSupporter FieldCore
		{
			get { return FieldSupporter; }
		}

		IZType IOperationalActionFieldValuePair.GetValue(Type expectedType)
		{
			return GetValueCore(expectedType);
		}

		#endregion

		#region Implementation

		protected virtual IZType GetValueCore(Type expectedType)
		{
			return Property;
		}

		public override bool ShouldApply
		{
			get { return !Property.IsEmpty || Descriptor.EmptyBehaviour != EmptyBehaviourList.Codes.Skip; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateProperty();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		TProperty property;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly TSupporter fieldSupporter;

		#endregion
	}
}
