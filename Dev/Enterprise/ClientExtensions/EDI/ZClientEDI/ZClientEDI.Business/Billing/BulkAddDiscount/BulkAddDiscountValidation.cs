using System;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkAddDiscountValidation : ZValidation
	{
		public BulkAddDiscountValidation(BulkAddDiscount bulkAddDiscount, ClientLicenceBillingDiscount[] billingDiscounts)
			: base(bulkAddDiscount)
		{
			this.Parent = bulkAddDiscount;
			this.BillingDiscounts = billingDiscounts;
			this.ZValidationInternals = this;
		}
		readonly BulkAddDiscount Parent;
		readonly IValidationInternals ZValidationInternals;
		readonly ClientLicenceBillingDiscount[] BillingDiscounts;

		public override Type AutoValidationType
		{
			get { return typeof(BulkAddDiscountValidation); }
		}

		public override void ValidateAll()
		{
			ValidateSystemCode();
			ValidateSubCode();
			ValidateDiscountType();
			ValidateModuleCode();
			ValidateBreakAmount();
			ValidateUnits();
			ValidateBreakUnits();
			ValidateDiscount();
			ValidateStartDate();
			ValidateEndDate();
			ValidateDuration();
			ValidateDescription();
			ValidateComment();
		}

		#region Properties

		#region SystemCode

		public void ValidateSystemCode()
		{
			ZValidationInternals.Validate(Parent.SystemCodeInfo, () => { CheckSystemCode(); });
		}

		void CheckSystemCode()
		{
			ValidateProperty(Parent.SystemCodeInfo, (x) => { return x.L5_SystemCodeInfo; });
		}

		#endregion

		#region SubCode

		public void ValidateSubCode()
		{
			ZValidationInternals.Validate(Parent.SubCodeInfo, () => { CheckSubCode(); });
		}

		void CheckSubCode()
		{
			ValidateProperty(Parent.SubCodeInfo, (x) => { return x.L5_SubCodeInfo; });
		}

		#endregion

		#region DiscountType

		public void ValidateDiscountType()
		{
			ZValidationInternals.Validate(Parent.DiscountTypeInfo, () => { CheckDiscountType(); });
		}

		void CheckDiscountType()
		{
			ValidateProperty(Parent.DiscountTypeInfo, (x) => { return x.L5_TypeInfo; });
		}

		#endregion

		#region ModuleCode

		public void ValidateModuleCode()
		{
			ZValidationInternals.Validate(Parent.ModuleCodeInfo, () => { CheckModuleCode(); });
		}

		void CheckModuleCode()
		{
			ValidateProperty(Parent.ModuleCodeInfo, (x) => { return x.L5_ModuleCodeInfo; });
		}

		#endregion

		#region BreakAmount

		public void ValidateBreakAmount()
		{
			ZValidationInternals.Validate(Parent.BreakAmountInfo, () => { CheckBreakAmount(); });
		}

		void CheckBreakAmount()
		{
			ValidateProperty(Parent.BreakAmountInfo, (x) => { return x.L5_BreakAmountInfo; });
		}

		#endregion

		#region Units

		public void ValidateUnits()
		{
			ZValidationInternals.Validate(Parent.UnitsInfo, () => { CheckUnits(); });
		}

		void CheckUnits()
		{
			ValidateProperty(Parent.UnitsInfo, (x) => { return x.L5_UnitsInfo; });
		}

		#endregion

		#region BreakUnits

		public void ValidateBreakUnits()
		{
			ZValidationInternals.Validate(Parent.BreakUnitsInfo, () => { CheckBreakUnits(); });
		}

		void CheckBreakUnits()
		{
			ValidateProperty(Parent.BreakUnitsInfo, (x) => { return x.L5_BreakUnitsInfo; });
		}

		#endregion

		#region Discount

		public void ValidateDiscount()
		{
			ZValidationInternals.Validate(Parent.DiscountInfo, () => { CheckDiscount(); });
		}

		void CheckDiscount()
		{
			ValidateProperty(Parent.DiscountInfo, (x) => { return x.L5_DiscountInfo; });
		}

		#endregion

		#region StartDate

		public void ValidateStartDate()
		{
			ZValidationInternals.Validate(Parent.StartDateInfo, () => { CheckStartDate(); });
		}

		void CheckStartDate()
		{
			ValidateProperty(Parent.StartDateInfo, (x) => { return x.L5_StartDateInfo; });
		}

		#endregion

		#region EndDate

		public void ValidateEndDate()
		{
			ZValidationInternals.Validate(Parent.EndDateInfo, () => { CheckEndDate(); });
		}

		void CheckEndDate()
		{
			ValidateProperty(Parent.EndDateInfo, (x) => { return x.L5_EndDateInfo; });
		}

		#endregion

		#region Duration

		public void ValidateDuration()
		{
			ZValidationInternals.Validate(Parent.DurationInfo, () => { CheckDuration(); });
		}

		void CheckDuration()
		{
			ValidateProperty(Parent.DurationInfo, (x) => { return x.L5_DurationInfo; });
		}

		#endregion

		#region Description

		public void ValidateDescription()
		{
			ZValidationInternals.Validate(Parent.DescriptionInfo, () => { CheckDescription(); });
		}

		void CheckDescription()
		{
			ValidateProperty(Parent.DescriptionInfo, (x) => { return x.L5_DescriptionInfo; });
		}

		#endregion

		#region Comment

		public void ValidateComment()
		{
			ZValidationInternals.Validate(Parent.CommentInfo, () => { CheckComment(); });
		}

		void CheckComment()
		{
			ValidateProperty(Parent.CommentInfo, (x) => { return x.L5_CommentInfo; });
		}

		#endregion

		void ValidateProperty(ZPropertyInfo bulkAddDiscountPropertyInfo, Func<ClientLicenceBillingDiscount, ZPropertyInfo> billingDiscountPropertyInfoGetter)
		{
			foreach (var billingDiscount in BillingDiscounts)
			{
				foreach (var propertyNotification in billingDiscountPropertyInfoGetter(billingDiscount).Notifications)
				{
					bulkAddDiscountPropertyInfo.AddNotification(propertyNotification.Type, billingDiscount.Parent.Company.Header.OH_Code + " - " + propertyNotification.Message);
				}
			}
		}

		#endregion
	}
}

