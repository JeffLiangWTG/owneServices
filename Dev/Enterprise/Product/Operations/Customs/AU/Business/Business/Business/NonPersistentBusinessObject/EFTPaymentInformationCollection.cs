using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EFTPaymentInformationCollection : NonPersistentBusinessObjectCollection<EFTPaymentInformation>
	{
		public EFTPaymentInformationCollection(JobDeclaration declaration, bool initialiseFromLastClearance = true)
			: base(declaration.Factory)
		{
			Declaration = declaration;
			LoadCollection(initialiseFromLastClearance);
		}

		public readonly JobDeclaration Declaration;

		public EFTPaymentInformation GetElementByEntryHeader(CusEntryHeader entryHeader)
		{
			foreach (EFTPaymentInformation payInfo in this)
			{
				if (payInfo.EntryHeader == entryHeader)
				{
					return payInfo;
				}
			}
			return null;
		}

		public ZDecimal CustomsChargeAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (EFTPaymentInformation payInfo in this)
				{
					result += payInfo.CustomsChargeAmountPayableNow;
				}
				return result;
			}
		}

		public ZDecimal AQISAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (EFTPaymentInformation payInfo in this)
				{
					result += payInfo.AQISServicePaymentAmountPayableNow;
				}
				return result;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public bool HasAmountsToPay
		{
			get
			{
				foreach (EFTPaymentInformation eFTPayInfo in this)
				{
					if (eFTPayInfo.HasAmountsToPay)
					{
						return true;
					}
				}
				return false;
			}
		}

		void LoadCollection(bool initialiseFromLastClearance)
		{
			foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
			{
				if (!entryHeader.EntryNumber.IsEmpty)
				{
					Add(new EFTPaymentInformation(entryHeader, initialiseFromLastClearance));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("EFTPaymentInformation cannot be created by users in grid.");
		}
	}
}
