using System;
using System.Diagnostics;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class CustomBusinessObjectValidation : ZValidation
	{
		public CustomBusinessObjectValidation(CustomBusinessObject parent)
			: base(parent)
		{
			this.parent = parent;
			validationInternals = this;
		}

		public void Validate(string propertyIdentifier)
		{
			ICustomProperty property = Parent.GetCustomProperty(propertyIdentifier);
			if (property != null)
			{
				validationInternals.Validate(Parent.GetZPropertyInfo(propertyIdentifier),
					() =>
					{
						if (ShouldValidateProperty(property))
						{
							property.Validate(Parent);
						}
					});
			}
		}

		public override void ValidateAll()
		{
			foreach (string propertyName in ((IDynamicBusinessObject)parent).PropertyNames)
			{
				Validate(propertyName);
			}
		}

		#region Implementation

		protected virtual bool ShouldValidateProperty(ICustomProperty property)
		{
			return true;
		}

		public override Type AutoValidationType
		{
			get { return typeof(CustomBusinessObjectValidation); }
		}

		public CustomBusinessObject Parent
		{
			[DebuggerStepThrough]
			get { return parent; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly CustomBusinessObject parent;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IValidationInternals validationInternals;

		#endregion
	}
}
