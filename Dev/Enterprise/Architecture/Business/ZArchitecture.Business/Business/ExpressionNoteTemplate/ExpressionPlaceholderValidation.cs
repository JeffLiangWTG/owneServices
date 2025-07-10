using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ExpressionPlaceholderValidation : ZValidation
	{
		public ExpressionPlaceholderValidation(ExpressionPlaceholder parent)
			: base(parent)
		{
			this.parent = parent;
			this.parentListInternals = parent;
		}

		readonly ExpressionPlaceholder parent;
		readonly ISingleElementListInternal parentListInternals;

		protected ExpressionPlaceholder Parent
		{
			get { return parent; }
		}

		public override Type AutoValidationType
		{
			get { return typeof(ExpressionPlaceholderValidation); }
		}

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
				ValidateDescription();
			}
		}

		public void ValidateDescription()
		{
			ValidateCalculatedProperty(Parent.DescriptionInfo);
		}

		protected virtual void CheckDescription()
		{
			MandatoryValidation.CheckEntered(Parent.DescriptionInfo);
		}
	}
}
