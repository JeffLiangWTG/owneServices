using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class ExpressionPlaceholder : NonPersistentBusinessObject
	{
		public abstract class Schema
		{
			public const string Sequence = "Sequence";
			public const string Description = "Description";
		}

		public ExpressionPlaceholder(ZInt sequence)
		{
			this.sequence = sequence;
		}

		readonly ZInt sequence;

		[ReadOnly(true)]
		public ZInt Sequence
		{
			get { return sequence; }
		}

		[MaxLength(60)]
		public ZString Description
		{
			get { return description; }
			set
			{
				if (description != value)
				{
					SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
					DescriptionInfo.RefreshBinding();
					Validation.ValidateDescription();
				}
			}
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		[MaxLength(35)]
		public ZString Replacement
		{
			get;
			set;
		}
		//ZString replacement; VALIDATION LATER

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ExpressionPlaceholderValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ExpressionPlaceholderValidation GetNewValidation()
		{
			return new ExpressionPlaceholderValidation(this);
		}

		#endregion
	}
}
