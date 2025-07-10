using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business
{
	public class CusEntryNumberWrapper
	{
		public CusEntryNumberWrapper(BusinessObject parent, ZString entryType)
			: this(parent, entryType, Core.Constants.CountryCodes.Germany)
		{
		}

		public CusEntryNumberWrapper(BusinessObject parent, ZString entryType, ZString countryCode)
		{
			this.parent = parent;
			this.entryType = entryType;
			this.countryCode = countryCode;
		}

		public ZBool ExistsCusEntryNumber
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber != null;
			}
		}

		public ZString EntryNumber
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			}
		}

		public void SetEntryNumber(ZString value, ZPropertyInfo propertyInfo)
		{
			var oldValue = EntryNumber;
			SetupCusEntryNumber(true);
			cusEntryNumber.CE_EntryNum = value.Left(propertyInfo.MaxLength);
			propertyInfo.RefreshBinding(oldValue);
		}

		public ZDateTime ExpiryDate
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;
			}
		}

		public void SetExpiryDate(ZDateTime value, ZPropertyInfo propertyInfo)
		{
			var oldValue = ExpiryDate;
			SetupCusEntryNumber(true);
			cusEntryNumber.CE_ExpiryDate = value;
			propertyInfo.RefreshBinding(oldValue);
		}

		public ZDateTime IssueDate
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			}
		}

		public ZString EntryStatus
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			}
		}

		public void SetEntryStatus(ZString value, ZPropertyInfo propertyInfo)
		{
			var oldValue = EntryStatus;
			SetupCusEntryNumber(true);
			cusEntryNumber.CE_EntryStatus = value;
			propertyInfo.RefreshBinding(oldValue);
		}

		void SetupCusEntryNumber(bool createIfNotExists)
		{
			if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
			{
				cusEntryNumber = createIfNotExists ? CusEntryNumber.LoadOrCreate(parent, entryType, countryCode) : CusEntryNumber.Load(parent, entryType, countryCode);
				if (cusEntryNumber != null)
				{
					parent.RegisterEditableChildObject(cusEntryNumber);
				}
			}
		}

		readonly BusinessObject parent;
		readonly ZString entryType;
		readonly ZString countryCode;
		CusEntryNumber cusEntryNumber;
	}
}
