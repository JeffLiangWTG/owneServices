using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ENettRegisteredBankAccount : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string BankAccountPK = "BankAccountPK";
			public const string IsDefault = "IsDefault";
		}

		#endregion

		public ENettRegisteredBankAccount()
		{
		}

		public ENettRegisteredBankAccount(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Bound Properties

		#region Bank Account

		ZGuid bankAccountPK;

		[List("BankAccountList")]
		public ZGuid BankAccountPK
		{
			get { return bankAccountPK; }
			set
			{
				if (bankAccountPK != value)
				{
					SetNonPersistentPropertyValue(BankAccountPKInfo, ref bankAccountPK, value);

					var accBankAccount = CurrentFactory.Load<AccBankAccount>(bankAccountPK);

					if (accBankAccount != null)
					{
						fBankAccountDescription = accBankAccount.AB_Desc;
						fBankAccountCurrencyCode = accBankAccount.AB_RX_NKAccountCurrency;
						fBankAccountCurrencyPK = RefCurrency.LoadFromCurrencyCode(CurrentFactory, accBankAccount.AB_RX_NKAccountCurrency).PK;
					}
					else
					{
						fBankAccountDescription = ZString.Empty;
						fBankAccountCurrencyCode = ZString.Empty;
						fBankAccountCurrencyPK = ZGuid.Empty;
					}

					if (ParentCollection != null && ParentCollection.GetDefaultReceiptBankAccount(fBankAccountCurrencyPK) == null)
					{
						IsDefault = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateBankAccountPK();
				}
			}
		}

		public ZPropertyInfo BankAccountPKInfo
		{
			get { return GetZPropertyInfo(Schema.BankAccountPK); }
		}

		public void ValidateBankAccountPK()
		{
			BankAccountPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BankAccountPKInfo);
			ListValidation.ErrorIfInvalidPK(BankAccountPKInfo, BankAccountList);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(BankAccountPKInfo, Res.GetString("5c09fbfb-582d-4aad-9fbf-ce60cfa6be91", "There must be only one line for each country/region."));
			}
		}

		#endregion

		#region Bank Account Description

		ZString fBankAccountDescription;
		public ZString BankAccountDescription
		{
			get { return fBankAccountDescription; }
		}

		#endregion

		#region BankAccountCurrencyCode

		ZString fBankAccountCurrencyCode;
		public ZString BankAccountCurrencyCode
		{
			get { return fBankAccountCurrencyCode; }
		}

		#endregion

		#region BankAccountCurrencyPK

		ZGuid fBankAccountCurrencyPK;
		public ZGuid BankAccountCurrencyPK
		{
			get { return fBankAccountCurrencyPK; }
		}

		#endregion

		#region IsDefault

		ZBool isDefault;
		[BusinessObjectTestExclude]
		public ZBool IsDefault
		{
			get { return isDefault; }
			set
			{
				if (isDefault != value)
				{
					ENettRegisteredBankAccountCollection parentCollection = ParentCollection;
					if (parentCollection != null)
					{
						if (value)
						{
							ENettRegisteredBankAccount defaultBankAccount = parentCollection.GetDefaultReceiptBankAccount(BankAccountCurrencyPK);

							if (defaultBankAccount != null)
							{
								defaultBankAccount.ResetDefault();
							}

							SetNonPersistentPropertyValue(IsDefaultInfo, ref isDefault, value);
						}
					}
					else
					{
						SetNonPersistentPropertyValue(IsDefaultInfo, ref isDefault, value);
					}
				}
			}
		}

		public ZPropertyInfo IsDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.IsDefault); }
		}

		internal void ResetDefault()
		{
			SetNonPersistentPropertyValue(IsDefaultInfo, ref isDefault, false);
		}

		#endregion

		#endregion

		#region Overridden

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateBankAccountPK();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ENettRegisteredBankAccount result = new ENettRegisteredBankAccount(fallbackLevel, factory);
			result.IsDefault = this.IsDefault;
			result.BankAccountPK = this.BankAccountPK;
			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.BankAccountPK, BankAccountPK.ToString());
			writer.WriteElementString(Schema.IsDefault, IsDefault.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BankAccountPK = new ZGuid(reader.ReadElementString(Schema.BankAccountPK));
			IsDefault = new ZBool(reader.ReadElementString(Schema.IsDefault));
		}

		#endregion

		#region Bank Account List

		public BusinessObjectCollection BankAccountList
		{
			get
			{
				var bankAccountList = new AccBankAccountCollection(CurrentFactory);
				bankAccountList.Load();
				return bankAccountList;
			}
		}

		#endregion

		#region Implementation

		ENettRegisteredBankAccountCollection ParentCollection
		{
			get { return (ENettRegisteredBankAccountCollection)GetParentCollection(this, typeof(ENettRegisteredBankAccountCollection)); }
		}

		#endregion
	}
}