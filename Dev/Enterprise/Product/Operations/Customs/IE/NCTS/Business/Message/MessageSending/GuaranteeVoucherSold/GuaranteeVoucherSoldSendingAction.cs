using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeVoucherSoldSendingAction : Customs.Business.BaseMessageSendingObject, IMessageSendingAction
	{
		public GuaranteeVoucherSoldSendingAction(CusGuaranteeHeader header) : base(header.Factory)
		{
			Header = header;
		}
		public CusGuaranteeHeader Header;

		public MessageSender CreateSender() => (MessageSender)Activator.CreateInstance(typeof(GuaranteeMessageSender), this);

		[ResourceStringData("F5B95E40-648C-4F68-A444-0566CBB41072", Caption = "Transit Holder")]
		[List(nameof(Lookups) + "." + nameof(GuaranteeVoucherSoldSendingActionLookups.HolderOfTransitProcedure))]
		public ZString HolderOfTransitProcedure
		{
			get { return holderOfTransitProcedure; }
			set
			{
				SetNonPersistentPropertyValue(HolderOfTransitProcedureInfo, ref holderOfTransitProcedure, value);
			}
		}
		ZString holderOfTransitProcedure;

		public ZPropertyInfo HolderOfTransitProcedureInfo => GetZPropertyInfo(nameof(HolderOfTransitProcedure));

		[ResourceStringData("062080F7-D6E5-4117-8F20-CB71C578961D", Caption = "Voucher Amount")]
		public ZDecimal VoucherAmount
		{
			get { return voucherAmount; }
			set
			{
				SetNonPersistentPropertyValue(VoucherAmountInfo, ref voucherAmount, value);
			}
		}
		ZDecimal voucherAmount;

		public ZPropertyInfo VoucherAmountInfo => GetZPropertyInfo(nameof(VoucherAmount));

		[ResourceStringData("2F60E470-6DF5-46D4-884E-FEDBEDF9647E", Caption = "TIR Carnet")]
		public ZBool TIRCarnet
		{
			get { return tirCarnet; }
			set
			{
				if (SetNonPersistentPropertyValue(TIRCarnetInfo, ref tirCarnet, value))
				{
					Validation.ValidateVoucherAmount();
				}
			}
		}
		ZBool tirCarnet;

		public ZPropertyInfo TIRCarnetInfo => GetZPropertyInfo(nameof(TIRCarnet));

		[ResourceStringData("6379FADF-5C56-4E84-8C3B-49A545E9CE09", Caption = "Guarantee Office")]
		[List(nameof(Lookups) + "." + nameof(GuaranteeVoucherSoldSendingActionLookups.CustomsOfficeOfGuarantee))]
		public ZString CustomsOfficeOfGuarantee
		{
			get { return customsOfficeOfGuarantee; }
			set
			{
				SetNonPersistentPropertyValue(CustomsOfficeOfGuaranteeInfo, ref customsOfficeOfGuarantee, value);
			}
		}
		ZString customsOfficeOfGuarantee;

		public ZPropertyInfo CustomsOfficeOfGuaranteeInfo => GetZPropertyInfo(nameof(CustomsOfficeOfGuarantee));

		public GuaranteeVoucherSoldSendingActionValidation Validation => GetNewValidation();

		public GuaranteeVoucherSoldSendingActionLookups Lookups => GetNewLookups();

		public ZString MessageType { get; set; }

		public IMessageAttachee MessageAttachee => Header;

		public void AddMessage(OutboundEDIMessage message)
		{
			Header.Messages.Add(message);
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected GuaranteeVoucherSoldSendingActionValidation GetNewValidation() => new GuaranteeVoucherSoldSendingActionValidation(this);

		protected GuaranteeVoucherSoldSendingActionLookups GetNewLookups() => new GuaranteeVoucherSoldSendingActionLookups(this);
	}
}
