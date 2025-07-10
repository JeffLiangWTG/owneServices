using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.IT.Business.Res;

namespace Enterprise.Customs.IT.Registry;

[XmlSerializerAssembly("Enterprise.Customs.IT.Business.XmlSerializers")]
public class Account : RegistryBusinessObjectTemplate
{
	#region Schema and Constructors

	protected abstract class Schema
	{
		public const string AccountNumber = "AccountNumber";
		public const string AccountPassword = "AccountPassword";
		public const string AccountPasswordExpirationDate = "AccountPasswordExpirationDate";
		public const string AccountStatus = "AccountStatus";
		public const string AccountCertificate = "AccountCertificate";
		public const string AccountCertificatePassword = "AccountCertificatePassword";
		public const string AccountCertificateExpirationDate = "AccountCertificateExpirationDate";
		public const string AccountCertificateStatus = "AccountCertificateStatus";
		public const string AccountNode = "AccountNode";
		public const string AccountRangeStart = "AccountRangeStart";
		public const string AccountRangeEnd = "AccountRangeEnd";
		public const string EmcsNotificationEnabled = "EmcsNotificationEnabled";
		public const string ExciseNumbersCount = "ExciseNumbersCount";
		public const string AccountDetailsCount = "AccountDetailsCount";
		public const int AccountNumberMaxLength = 20;
	}

	public Account()
		: base()
	{
	}

	public Account(AccountCollection parentCollection)
		: base()
	{
		ParentAccountCollection = parentCollection;
	}

	public Account(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	: base(fallbackLevel, factory)
	{
	}

	public Account(FallbackLevel fallbackLevel, BusinessObjectFactory factory, AccountCollection parentCollection)
		: base(fallbackLevel, factory)
	{
		ParentAccountCollection = parentCollection;
	}

	[BusinessObjectTestExclude()]
	public AccountCollection ParentAccountCollection { get; set; }

	#endregion

	#region Properties

	#region AccountNumber

	[MaxLength(Schema.AccountNumberMaxLength)]
	public ZString AccountNumber
	{
		get { return fAccountNumber; }
		set
		{
			SetNonPersistentPropertyValue(AccountNumberInfo, ref fAccountNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountNumber();
				ResetDeclarantTaxNumberAndWorkstationSequentialNumber();
			}
		}
	}

	ZString fAccountNumber;

	public ZPropertyInfo AccountNumberInfo
	{
		get
		{
			var info = GetZPropertyInfo(Schema.AccountNumber);
			info.HumanReadableName = Res.GetString("28FD077E-B7E2-42B2-B079-519998063AF6", "Account Number");
			return info;
		}
	}

	#region AccountNumber Parts

	public ZString DeclarantTaxNumber => GetCachedSplittedValue(x => x.DeclarantTaxNumber);

	public ZInt WorkstationSequentialNumber => GetCachedSplittedValue(x => x.WorkstationSequentialNumber);

	T GetCachedSplittedValue<T>(Func<(ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber), T> func)
	{
		if (!cachedAccountNumberSplitted.HasValue)
		{
			cachedAccountNumberSplitted = AccountHelper.SplitCodeBySeparator(AccountNumber);
		}

		return func(cachedAccountNumberSplitted.Value);
	}
	(ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber)? cachedAccountNumberSplitted;

	void ResetDeclarantTaxNumberAndWorkstationSequentialNumber()
	{
		cachedAccountNumberSplitted = null;
	}

	#endregion

	#endregion

	#region AccountPassword

	[MaxLength(15)]
	[Password]
	public ZString AccountPassword
	{
		get { return fAccountPassword; }
		set
		{
			SetNonPersistentPropertyValue(AccountPasswordInfo, ref fAccountPassword, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountPassword();
			}
		}
	}

	ZString fAccountPassword;

	public ZPropertyInfo AccountPasswordInfo
	{
		get { return GetZPropertyInfo(Schema.AccountPassword); }
	}

	#endregion

	#region AccountPasswordExpirationDate

	public ZDate AccountPasswordExpirationDate
	{
		get { return fAccountPasswordExpirationDate; }
		set
		{
			SetNonPersistentPropertyValue(AccountPasswordExpirationDateInfo, ref fAccountPasswordExpirationDate, value);
		}
	}

	ZDate fAccountPasswordExpirationDate;

	public ZPropertyInfo AccountPasswordExpirationDateInfo => GetZPropertyInfo(Schema.AccountPasswordExpirationDate);
	#endregion

	#region AccountStatus

	[List(nameof(Lookups) + "." + nameof(AccountLookups.AccountStatusList))]
	public ZString AccountStatus
	{
		get { return fAccountStatus; }
		set
		{
			SetNonPersistentPropertyValue(AccountStatusInfo, ref fAccountStatus, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountStatus();
			}
		}
	}

	ZString fAccountStatus;

	public ZPropertyInfo AccountStatusInfo
	{
		get { return GetZPropertyInfo(Schema.AccountStatus); }
	}

	public ZString AccountStatusDisplay => Lookups.AccountStatusList.GetDescriptionFromCode(AccountStatus);

	#endregion

	#region Certificate

	#region AccountCertificate

	public ZBlob AccountCertificate
	{
		get { return fAccountCertificate; }
		set
		{
			SetNonPersistentPropertyValue(AccountCertificateInfo, ref fAccountCertificate, value);
			DefaultAccountCertificateExpirationDate();
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountCertificate();
			}
			CheckAccountCertificateStatus();
		}
	}

	ZBlob fAccountCertificate;

	public ZPropertyInfo AccountCertificateInfo
	{
		get
		{
			var info = GetZPropertyInfo(Schema.AccountCertificate);
			info.HumanReadableName = Res.GetString("39669DE7-1352-407C-800C-B8BB9AA5576C", "Account Certificate");
			return info;
		}
	}

	public ZString AccountCertificateState => GetCertificateState(AccountCertificate);
	#endregion

	#region AccountCertificatePassword

	[Password]
	public ZString AccountCertificatePassword
	{
		get { return fAccountCertificatePassword; }
		set
		{
			SetNonPersistentPropertyValue(AccountCertificatePasswordInfo, ref fAccountCertificatePassword, value);
			DefaultAccountCertificateExpirationDate();
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountCertificatePassword();
				Validation.ValidateAccountCertificate();
			}
			CheckAccountCertificateStatus();
		}
	}

	ZString fAccountCertificatePassword;

	public ZPropertyInfo AccountCertificatePasswordInfo
	{
		get { return GetZPropertyInfo(Schema.AccountCertificatePassword); }
	}

	#endregion

	#region AccountCertificateExpirationDate

	[ReadOnly(true)]
	public ZDate AccountCertificateExpirationDate
	{
		get { return fAccountCertificateExpirationDate; }
		set
		{
			SetNonPersistentPropertyValue(AccountCertificateExpirationDateInfo, ref fAccountCertificateExpirationDate, value);
			CheckAccountCertificateStatus();
		}
	}

	ZDate fAccountCertificateExpirationDate;

	public ZPropertyInfo AccountCertificateExpirationDateInfo => GetZPropertyInfo(Schema.AccountCertificateExpirationDate);

	#endregion

	#region AccountCertificateStatus

	public ZString AccountCertificateStatus
	{
		get { return fAccountCertificateStatus; }
		set
		{
			SetNonPersistentPropertyValue(AccountCertificateStatusInfo, ref fAccountCertificateStatus, value);
		}
	}

	ZString fAccountCertificateStatus;

	public ZPropertyInfo AccountCertificateStatusInfo
	{
		get { return GetZPropertyInfo(Schema.AccountCertificateStatus); }
	}

	public ZString AccountCertificateStatusDisplay => Lookups.AccountCertificateStatusList.GetDescriptionFromCode(AccountCertificateStatus);

	internal void CheckAccountCertificateStatus()
	{
		if (!AccountCertificateInfo.HasErrors()
			&& !AccountCertificatePasswordInfo.HasErrors()
			&& !IsCertificatePasswordExpired)
		{
			AccountCertificateStatus = AccountCertificateStatusList.Codes.Valid;
		}
		else if (IsCertificatePasswordExpired)
		{
			AccountCertificateStatus = AccountCertificateStatusList.Codes.Expired;
		}
		else
		{
			AccountCertificateStatus = AccountCertificateStatusList.Codes.Invalid;
		}

		AccountCertificateStatusInfo.RefreshBinding();
	}

	#endregion

	#region AccountNode

	[MaxLength(4)]
	public ZString AccountNode
	{
		get { return fAccountNode; }
		set
		{
			SetNonPersistentPropertyValue(AccountNodeInfo, ref fAccountNode, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateNode();
			}
		}
	}

	ZString fAccountNode;

	public ZPropertyInfo AccountNodeInfo => GetZPropertyInfo(Schema.AccountNode);

	#endregion

	#endregion

	#region AccountRangeStart

	[MaxLength(2)]
	public ZString AccountRangeStart
	{
		get { return PadLeftZeroIfNotEmpty(fAccountRangeStart); }
		set
		{
			fAccountRangeStart = PadLeftZeroIfNotEmpty(fAccountRangeStart);
			SetNonPersistentPropertyValue(AccountRangeStartInfo, ref fAccountRangeStart, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountRangeStart();
			}
		}
	}
	ZString fAccountRangeStart;

	public ZPropertyInfo AccountRangeStartInfo => GetZPropertyInfo(Schema.AccountRangeStart);

	#endregion

	#region AccountRangeEnd

	[MaxLength(2)]
	public ZString AccountRangeEnd
	{
		get { return PadLeftZeroIfNotEmpty(fAccountRangeEnd); }
		set
		{
			fAccountRangeEnd = PadLeftZeroIfNotEmpty(fAccountRangeEnd);
			SetNonPersistentPropertyValue(AccountRangeEndInfo, ref fAccountRangeEnd, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccountRangeEnd();
			}
		}
	}
	ZString fAccountRangeEnd;

	public ZPropertyInfo AccountRangeEndInfo => GetZPropertyInfo(Schema.AccountRangeEnd);

	#endregion

	#region EmcsNotificationEnabled

	public ZBool EmcsNotificationEnabled
	{
		get { return emcsNotificationEnabled; }
		set
		{
			SetNonPersistentPropertyValue(EmcsNotificationEnabledInfo, ref emcsNotificationEnabled, value);
			RemoveAllExciseNumbersAndSetReadOnlyIfNeeded(value);
		}
	}
	ZBool emcsNotificationEnabled;

	public ZPropertyInfo EmcsNotificationEnabledInfo => GetZPropertyInfo(Schema.EmcsNotificationEnabled);

	void RemoveAllExciseNumbersAndSetReadOnlyIfNeeded(ZBool emcsNotificationEnabled)
	{
		if (!emcsNotificationEnabled)
		{
			ExciseNumbers.RemoveAndDeleteAll();
		}
		ExciseNumbers.SetReadOnlyIncludingChildren(!emcsNotificationEnabled);
	}

	#endregion

	#region ExciseNumbers

	public ExciseNumberCollection ExciseNumbers
	{
		get
		{
			if (exciseNumbers == null)
			{
				exciseNumbers = new ExciseNumberCollection(this, CurrentFactory);
				exciseNumbers.CountChanged += ExciseNumbers_CountChanged;
				RegisterEditableChildObject(exciseNumbers);
			}
			return exciseNumbers;
		}
	}
	ExciseNumberCollection exciseNumbers;

	void ExciseNumbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		if (!IsValidationSuspended)
		{
			Validation.ValidateEmcsNotificationEnabled();
		}
	}

	#endregion

	#region AccountDetails

	public AccountDetailCollection AccountDetails
	{
		get
		{
			if (accountDetails is null)
			{
				accountDetails = new AccountDetailCollection(this, CurrentFactory);
				accountDetails.CountChanged += AccountDetails_CountChanged;
				RegisterEditableChildObject(accountDetails);
			}
			return accountDetails;
		}
	}
	AccountDetailCollection accountDetails;

	void AccountDetails_CountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		if (!IsValidationSuspended)
		{
			Validation.ValidateHasAtLeastOneAccountDetails();
		}
	}

	#endregion

	#endregion

	#region Validation

	AccountValidation Validation
	{
		get
		{
			if (fValidation == null)
			{
				fValidation = new AccountValidation(this, CurrentFactory);
			}
			return fValidation;
		}
	}

	AccountValidation fValidation;

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		var validation = Validation;

		validation.ValidateAccountNumber();
		validation.ValidateAccountPassword();
		validation.ValidateAccountStatus();
		validation.ValidateAccountCertificatePassword();
		validation.ValidateAccountCertificate();
		validation.ValidateNode();

		CheckAccountCertificateStatus();

		validation.ValidateEmcsNotificationEnabled();
		validation.ValidateHasAtLeastOneAccountDetails();
		ExciseNumbers.RunPreSaveValidation();
		AccountDetails.RunPreSaveValidation();
	}

	#endregion

	#region Lookups

	public AccountLookups Lookups => fLookups ?? (fLookups = new AccountLookups(CurrentFactory));
	AccountLookups fLookups;

	#endregion

	#region Cloning, XML Reading and Writing

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		var clonedAccount = new Account(fallbackLevel, factory, ParentAccountCollection);
		clonedAccount.ParentAccountCollection = ParentAccountCollection;
		CloneValidExciseNumbers(clonedAccount);
		CloneValidAccountDetails(clonedAccount);
		return clonedAccount;
	}

	void CloneValidExciseNumbers(Account clonedAccount)
	{
		foreach (ExciseNumber exciseNumber in ExciseNumbers)
		{
			if (!exciseNumber.IsDeleted)
			{
				var clonedExciseNumber = clonedAccount.ExciseNumbers.AddNew();
				clonedExciseNumber.Number = exciseNumber.Number;
			}
		}
	}

	void CloneValidAccountDetails(Account clonedAccount)
	{
		foreach (AccountDetail accountDetail in AccountDetails)
		{
			if (!accountDetail.IsDeleted)
			{
				var clonedAccountDetail = clonedAccount.AccountDetails.AddNew();
				clonedAccountDetail.InternalCode = accountDetail.InternalCode;
				clonedAccountDetail.DeclarantCode = accountDetail.DeclarantCode;
				clonedAccountDetail.AuthorizedUser = accountDetail.AuthorizedUser;
			}
		}
	}

	#region XML Reading and Writing

	protected sealed override void ReadElements(XmlReaderWrapper reader)
	{
		AccountNumber = reader.ReadElementString(Schema.AccountNumber);
		AccountPassword = reader.ReadElementString(Schema.AccountPassword);

		if (ZDate.TryParseJulianDate(reader.ReadElementString(Schema.AccountPasswordExpirationDate), out var accountPasswordExpirationDate))
		{
			AccountPasswordExpirationDate = accountPasswordExpirationDate;
		}

		AccountStatus = reader.ReadElementString(Schema.AccountStatus);

		ReadCertificate(reader);

		AccountNode = reader.ReadElementString(Schema.AccountNode);
		AccountRangeStart = reader.ReadElementString(Schema.AccountRangeStart);
		AccountRangeEnd = reader.ReadElementString(Schema.AccountRangeEnd);

		EmcsNotificationEnabled = reader.ReadElementStringAsZBool(Schema.EmcsNotificationEnabled);
		int exciseNumbersCount = reader.ReadElementStringAsZInt(Schema.ExciseNumbersCount);
		for (int i = 0; i < exciseNumbersCount; i++)
		{
			var exciseNumber = ExciseNumbers.AddNew();
			exciseNumber.Number = reader.ReadElementString(string.Format(ExciseNumber.Schema.Number, i));
		}

		var accountDetailsCount = reader.ReadElementStringAsZInt(Schema.AccountDetailsCount);
		for (int i = 0; i < accountDetailsCount; i++)
		{
			var accountDetail = AccountDetails.AddNew();
			accountDetail.InternalCode = reader.ReadElementString(AccountDetail.Schema.InternalCode + i);
			accountDetail.DeclarantCode = reader.ReadElementString(AccountDetail.Schema.DeclarantCode + i);
			accountDetail.AuthorizedUser = reader.ReadElementString(AccountDetail.Schema.AuthorizedUser + i);
		}
	}

	protected sealed override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.AccountNumber, AccountNumber.ToString());
		writer.WriteElementString(Schema.AccountPassword, AccountPassword.ToString());
		writer.WriteElementString(Schema.AccountPasswordExpirationDate, AccountPasswordExpirationDate.ToJulianDateString());
		writer.WriteElementString(Schema.AccountStatus, AccountStatus.ToString());

		writer.WriteElementString(Schema.AccountCertificate, Encoding.Default.GetString(AccountCertificate));
		writer.WriteElementString(Schema.AccountCertificatePassword, AccountCertificatePassword.ToString());
		writer.WriteElementString(Schema.AccountCertificateExpirationDate, AccountCertificateExpirationDate.ToJulianDateString());
		writer.WriteElementString(Schema.AccountCertificateStatus, AccountCertificateStatus.ToString());

		writer.WriteElementString(Schema.AccountNode, AccountNode.ToString());
		if (!AccountRangeStart.IsEmpty)
		{
			writer.WriteElementString(Schema.AccountRangeStart, AccountRangeStart.ToString());
		}
		if (!AccountRangeEnd.IsEmpty)
		{
			writer.WriteElementString(Schema.AccountRangeEnd, AccountRangeEnd.ToString());
		}

		writer.WriteElementString(Schema.EmcsNotificationEnabled, EmcsNotificationEnabled.ToString());
		writer.WriteElementString(Schema.ExciseNumbersCount, ExciseNumbers.Count.ToString());
		for (int i = 0; i < ExciseNumbers.Count; i++)
		{
			writer.WriteElementString(string.Format(ExciseNumber.Schema.Number, i), ExciseNumbers[i].Number);
		}

		writer.WriteElementString(Schema.AccountDetailsCount, AccountDetails.Count.ToString());
		for (int i = 0; i < AccountDetails.Count; i++)
		{
			var accountDetail = AccountDetails[i];

			writer.WriteElementString(AccountDetail.Schema.InternalCode + i, accountDetail.InternalCode);
			writer.WriteElementString(AccountDetail.Schema.DeclarantCode + i, accountDetail.DeclarantCode);
			writer.WriteElementString(AccountDetail.Schema.AuthorizedUser + i, accountDetail.AuthorizedUser);
		}
	}

	#endregion

	#endregion

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		EmcsNotificationEnabled = false;
		AccountStatus = AccountStatusList.Codes.Valid;
	}

	#region Implementation

	ZBool IsCertificatePasswordExpired => !AccountCertificateExpirationDate.IsEmpty && ZDate.Today > AccountCertificateExpirationDate;

	ZString PadLeftZeroIfNotEmpty(ZString value) => value.IsEmpty ? value : value.PadLeft(2, '0');

	string GetCertificateState(ZBlob certificate) => certificate.IsEmpty ? Res.GetString("33647C89-1F81-4463-B320-81A716872363", "Empty") : Res.GetString("26DC927C-BEEF-473C-85F2-6483429A684E", "Loaded");

	void ReadCertificate(XmlReaderWrapper reader)
	{
		AccountCertificate = new ZBlob(Encoding.Default.GetBytes(reader.ReadElementString(Schema.AccountCertificate)));
		AccountCertificatePassword = reader.ReadElementString(Schema.AccountCertificatePassword);

		if (ZDate.TryParseJulianDate(reader.ReadElementString(Schema.AccountCertificateExpirationDate), out var accountCertificateExpirationDate))
		{
			AccountCertificateExpirationDate = accountCertificateExpirationDate;
		}

		AccountCertificateStatus = reader.ReadElementString(Schema.AccountCertificateStatus);
	}

	void DefaultAccountCertificateExpirationDate() => DefaultDataFromCertificate(AccountCertificate, AccountCertificatePassword, AccountCertificateExpirationDateInfo);

	void DefaultDataFromCertificate(ZBlob certificateBinaryData, ZString certificatePassword, ZPropertyInfo targetPropertyInfo)
	{
		if (certificateBinaryData.IsEmpty || certificatePassword.IsEmpty)
		{
			targetPropertyInfo.Value = ZDate.Empty;
		}
		else
		{
			try
			{
				var certificateObject = new X509Certificate2(certificateBinaryData, certificatePassword);
				targetPropertyInfo.Value = (ZDate)certificateObject.NotAfter;
			}
			catch (CryptographicException ex) when (!ex.IsCriticalException())
			{
				targetPropertyInfo.Value = ZDate.Empty;
			}
		}
	}

	#endregion
}
