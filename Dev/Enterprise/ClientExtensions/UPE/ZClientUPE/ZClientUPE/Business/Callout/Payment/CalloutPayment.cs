using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	#region Enum

	public enum UPECargoPaymentMethod
	{
		// the numbers are important because they're persisted in the db
		None = 0,
		Account = 1,
		BPay = 2,
		CreditCard = 3,
		Cheque = 4,
		EFT = 5,
		Nett7Day = 6,
		Other = 7,
		PurchaseOrder = 8,
	}

	#endregion

	public class CalloutPayment : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CalloutPayment(Callout callout)
		{
			this.Callout = callout;
		}

		#region Events

		public event PaymentMethodChangedEventHandler PaymentMethodChanged;

		protected virtual void OnPaymentMethodChanged(PaymentMethodChangedEventArgs e)
		{
			if (PaymentMethodChanged != null)
			{
				PaymentMethodChanged(this, e);
			}
		}

		#endregion

		#region IsCreditCard

		[BusinessObjectTestExclude]
		public ZBool IsCreditCard
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.CreditCard); }
			set
			{
				if (IsCreditCard != value)
				{
					PaymentMethod = UPECargoPaymentMethod.CreditCard;
				}
				IsCreditCardInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCreditCardInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsCreditCard));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsCheque

		[BusinessObjectTestExclude]
		public ZBool IsCheque
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.Cheque); }
			set
			{
				if (IsCheque != value)
				{
					PaymentMethod = UPECargoPaymentMethod.Cheque;
				}
				IsChequeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsChequeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsCheque));
				if (Callout != null)
				{
					((IZPropertyInfoObsolete)result).ReadOnly = !GlbStaff.CurrentUser.GS_IsController && (ShouldBeReadOnlyIfPaymentMethodSpecified || !Callout.IsConsigneeOrDeliveryAddressPostcodeCOD);
				}
				return result;
			}
		}

		#endregion

		#region IsPurchaseOrder

		[BusinessObjectTestExclude]
		public ZBool IsPurchaseOrder
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.PurchaseOrder); }
			set
			{
				if (IsPurchaseOrder != value)
				{
					PaymentMethod = UPECargoPaymentMethod.PurchaseOrder;
				}
				IsPurchaseOrderInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPurchaseOrderInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsPurchaseOrder));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsAccount

		[BusinessObjectTestExclude]
		public ZBool IsAccount
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.Account); }
			set
			{
				if (IsAccount != value)
				{
					PaymentMethod = UPECargoPaymentMethod.Account;
				}
				IsAccountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsAccountInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsAccount));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsOther

		[BusinessObjectTestExclude]
		public ZBool IsOther
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.Other); }
			set
			{
				if (IsOther != value)
				{
					PaymentMethod = UPECargoPaymentMethod.Other;
				}
				IsOtherInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsOtherInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsOther));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsNett7Day

		[BusinessObjectTestExclude]
		public ZBool IsNett7Day
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.Nett7Day); }
			set
			{
				if (IsNett7Day != value)
				{
					PaymentMethod = UPECargoPaymentMethod.Nett7Day;
				}
				IsNett7DayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsNett7DayInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsNett7Day));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsBPay

		[BusinessObjectTestExclude]
		public ZBool IsBPay
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.BPay); }
			set
			{
				if (IsBPay != value)
				{
					PaymentMethod = UPECargoPaymentMethod.BPay;
				}
				IsBPayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsBPayInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsBPay));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsEFT

		[BusinessObjectTestExclude]
		public ZBool IsEFT
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.EFT); }
			set
			{
				if (IsEFT != value)
				{
					PaymentMethod = UPECargoPaymentMethod.EFT;
				}
				IsEFTInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsEFTInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsEFT));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region IsNone

		[BusinessObjectTestExclude]
		public ZBool IsNone
		{
			get { return (PaymentMethod == UPECargoPaymentMethod.None); }
			set
			{
				if (IsNone != value)
				{
					PaymentMethod = UPECargoPaymentMethod.None;
				}
				IsNoneInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsNoneInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsNone));
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadOnlyIfPaymentMethodSpecified;
				return result;
			}
		}

		#endregion

		#region PaymentMethod

		public UPECargoPaymentMethod PaymentMethod
		{
			get { return Callout.PaymentMethod; }
			set
			{
				if (Callout.PaymentMethod != value)
				{
					PaymentMethodChangedEventArgs e = new PaymentMethodChangedEventArgs(value);
					OnPaymentMethodChanged(e);
					if (!e.Cancel)
					{
						Callout.PaymentDate = (value == UPECargoPaymentMethod.None) ? ZDateTime.Empty : ZDateTime.Now;
						Callout.PaymentMethod = value;
					}

					if (!IsNone)
					{
						Callout.MoveToQueue(
							CommercialQueueCodeDescriptionPairList.Codes.Completed,
							ZString.Empty,
							ZString.Empty,
							string.Format("PAYMENT MADE BY {0} & AUTO-COMPLETED", Callout.PaymentMethod));
					}
					Callout.RefreshBinding();
				}
			}
		}

		#endregion

		bool ShouldBeReadOnlyIfPaymentMethodSpecified
		{
			get { return Callout != null && (Callout.PaymentMethodOriginalValue != UPECargoPaymentMethod.None); }
		}

		readonly Callout Callout;
	}
}
