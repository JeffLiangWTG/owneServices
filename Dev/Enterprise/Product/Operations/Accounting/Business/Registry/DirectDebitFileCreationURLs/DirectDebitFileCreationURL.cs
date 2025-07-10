using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DirectDebitFileCreationURL : RegistryBusinessObjectTemplate
	{
		public DirectDebitFileCreationURL(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public DirectDebitFileCreationURL()
			: base()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string BankAccountPK = "BankAccountPK";
			public const string BankWebsite = "BankWebsite";
			public const string BankAccountDescription = "BankAccountDescription";
			public const string AllowAutoDDR = "AllowAutoDDR";
			public const string DDRFormat = "DDRFormat";
		}

		#endregion

		#region Implementation

		void PopulateInfoOfBankAccountProperties()
		{
			bankAccountDescription = ZString.Empty;
			allowAutoDDR = false;

			BusinessObject accBankAccount = CurrentFactory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.PK, bankAccountPK));
			if (accBankAccount != null)
			{
				bankAccountDescription = (ZString)accBankAccount[AccBankAccountSchema.AB_Desc];
				allowAutoDDR = (ZBool)accBankAccount[AccBankAccountSchema.AB_AllowAutoDDR];
				ddrFormat = (ZString)accBankAccount[AccBankAccountSchema.AB_AutoDDRFormat];
			}
		}

		#endregion

		#region Properties

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
					PopulateInfoOfBankAccountProperties();
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
			MandatoryValidation.CheckEntered(BankAccountPKInfo, Res.GetString("7dc26d30-d27e-4b40-9343-7ae5f411c3f9", "Bank Account"));
			ListValidation.ErrorIfInvalidPK(BankAccountPKInfo, BankAccountList);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(BankAccountPKInfo, ErrorDuplicateBankAccount);
			}
		}

		#endregion

		#region BankAccountDescription

		ZString bankAccountDescription;
		public ZString BankAccountDescription
		{
			get { return bankAccountDescription; }
		}

		public ZPropertyInfo BankAccountDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BankAccountDescription); }
		}

		#endregion

		#region BankWebsite

		ZString bankWebsite;

		[MaxLength(200)]
		public ZString BankWebsite
		{
			set
			{
				if (value != bankWebsite)
				{
					SetNonPersistentPropertyValue(BankWebsiteInfo, ref bankWebsite, value);

					if (!IsValidationSuspended) { ValidateBankWebsite(); }
				}
			}
			get { return bankWebsite; }
		}

		public ZPropertyInfo BankWebsiteInfo
		{
			get { return GetZPropertyInfo(Schema.BankWebsite); }
		}

		void ValidateBankWebsite()
		{
			BankWebsiteInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BankWebsiteInfo, Res.GetString("4153d762-19ad-4e88-8118-03f60715a6ab", "Bank URL"));
		}

		#endregion

		#region AllowAutoDDR

		ZBool allowAutoDDR;
		[BusinessObjectTestExclude]
		public ZBool AllowAutoDDR
		{
			get { return allowAutoDDR; }
		}

		public ZPropertyInfo AllowAutoDDRInfo
		{
			get { return GetZPropertyInfo(Schema.AllowAutoDDR); }
		}

		#endregion

		#region DDRFormat

		ZString ddrFormat;
		[BusinessObjectTestExclude]
		public ZString DDRFormat
		{
			get { return ddrFormat; }
		}

		public ZPropertyInfo DDRFormatInfo
		{
			get { return GetZPropertyInfo(Schema.DDRFormat); }
		}

		#endregion

		#region Bank Account List

		BusinessObjectCollection bankAccountList;
		public BusinessObjectCollection BankAccountList
		{
			get
			{
				if (bankAccountList == null)
				{
					ZQuery bankAcctQuery = new ZQuery(AccBankAccountSchema.AB_AutoDDRFormat, SQLComparisonOperator.NotEqual, ZString.Empty);
					bankAcctQuery.AddToFilter(AccBankAccountSchema.AB_AllowAutoDDR, true);
					bankAcctQuery.AddToFilter(AccBankAccountSchema.AB_IsActive, true);

					if (CurrentFallbackLevel != null)
					{
						bankAcctQuery.AddToFilter(AccBankAccountSchema.AB_GC, CurrentFallbackLevel.CompanyPK(false));
					}

					bankAccountList = new AccBankAccountCollection(CurrentFactory, bankAcctQuery);
				}
				return bankAccountList;
			}
		}

		#endregion

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			DirectDebitFileCreationURL clone = new DirectDebitFileCreationURL(fallbackLevel, factory);
			clone.BankAccountPK = BankAccountPK;
			clone.BankWebsite = BankWebsite;

			return clone;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BankAccountPK, BankAccountPK.ToString());
			writer.WriteElementString(Schema.BankWebsite, BankWebsite);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			bankAccountPK = new ZGuid(reader.ReadElementString(Schema.BankAccountPK));
			bankWebsite = reader.ReadElementString(Schema.BankWebsite);
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateBankAccountPK();
			ValidateBankWebsite();
		}
		#endregion

		public static string ErrorDuplicateBankAccount
		{
			get { return Res.GetString("fd3a5930-a8a2-41a3-a1d0-746f9f843abe", "Bank Account has already been selected. Cannot select the same Bank Account twice."); }
		}
	}
}
