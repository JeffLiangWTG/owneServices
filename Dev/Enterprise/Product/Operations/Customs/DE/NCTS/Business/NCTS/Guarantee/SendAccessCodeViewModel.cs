using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class SendAccessCodeViewModel : NonPersistentBusinessObject
	{
		public SendAccessCodeViewModel(CusGuaranteeHeader guaranteeHeader)
		{
			this.GuaranteeHeader = guaranteeHeader;
		}

		public readonly CusGuaranteeHeader GuaranteeHeader;

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("85d35004-5d6c-411b-813b-59ef890fefa3", Caption = "Guarantee Number", ShortCaption = "Number")]
		public ZString GuaranteeNumber => GuaranteeHeader.CPH_Number;

		public ZPropertyInfo GuaranteeNumberInfo => GetZPropertyInfo(nameof(GuaranteeNumber));

		[MaxLength(8)]
		[List(nameof(Lookups) + "." + nameof(SendAccessCodeViewModelLookups.Offices))]
		[ResourceStringData("5fb91ca4-68fe-4388-9993-2d51403d7334", Caption = "Office of Guarantee", ShortCaption = "Office")]
		public ZString OfficeOfGuarantee
		{
			get
			{
				return officeOfGuarantee;
			}
			set
			{
				SetNonPersistentPropertyValue(OfficeOfGuaranteeInfo, ref officeOfGuarantee, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOfficeOfGuarantee();
				}
			}
		}

		ZString officeOfGuarantee;

		public ZPropertyInfo OfficeOfGuaranteeInfo => GetZPropertyInfo(nameof(OfficeOfGuarantee));

		[MaxLength(4)]
		[ResourceStringData("55a33a66-d0f6-48e8-8f5a-08c31acf0810", Caption = "New Main Access Code", ShortCaption = "New Code")]
		public ZString NewMainAccessCode
		{
			get
			{
				return newMainAccessCode;
			}
			set
			{
				SetNonPersistentPropertyValue(NewMainAccessCodeInfo, ref newMainAccessCode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateNewMainAccessCode();
				}
			}
		}

		ZString newMainAccessCode;

		public ZPropertyInfo NewMainAccessCodeInfo => GetZPropertyInfo(nameof(NewMainAccessCode));

		#endregion

		#region Validation and Lookups

		public SendAccessCodeViewModelValidation Validation => new SendAccessCodeViewModelValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateAll();
		}

		public SendAccessCodeViewModelLookups Lookups => lookups ?? (lookups = new SendAccessCodeViewModelLookups(this, GuaranteeHeader.Factory));
		SendAccessCodeViewModelLookups lookups;

		#endregion

		public bool Send()
		{
			var sender = new GUACODSender(this);
			sender.Send();

			return true;
		}
	}
}
