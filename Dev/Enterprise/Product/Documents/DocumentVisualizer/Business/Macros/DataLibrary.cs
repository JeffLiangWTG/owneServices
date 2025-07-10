using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using ICountry = Enterprise.ZArchitecture.Environment.ICountry;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DataLibrary : MacroLibrary
	{
		public DataLibrary()
			: this(null)
		{
		}

		public DataLibrary(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region SuppressResourceStringsCheckRegion

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<DateTime>>(
					"Now",
					"Returns current date/time.",
					() => ZDateTime.Now.ToDateTime());

				yield return new Handler<Func<DateTime>>(
					"Today",
					"Returns current date.",
					() => ZDate.Today.ToDateTime());

				yield return new Handler<Func<DateTime>>(
					"EmptyDate",
					"Returns an empty date.",
					() => new DateTime());

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, IDynamicData>>(
					"CustomField",
					"Returns custom field value. If custom field is not found creates one with the specified value.",
					(scope, parent, name, value) => GetOrCreateCustomField(scope, parent, name, value, null));

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, MacroClosure, IDynamicData>>(
					"CustomField",
					"Returns custom field value. If custom field is not found creates one with the specified value.",
					(scope, parent, name, value, onValueChanged) => GetOrCreateCustomField(scope, parent, name, value, onValueChanged));

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, IDynamicData>>(
					"LocalCustomField",
					"Returns custom field value. If custom field is not found creates one with the specified value. This field will not be included in outgoing message.",
					(scope, parent, name, value) => GetOrCreateLocalCustomField(scope, parent, name, value, null));

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, MacroClosure, IDynamicData>>(
					"LocalCustomField",
					"Returns custom field value. If custom field is not found creates one with the specified value. This field will not be included in outgoing message.",
					(scope, parent, name, value, onValueChange) => GetOrCreateLocalCustomField(scope, parent, name, value, onValueChange));

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, IDynamicData>>(
					"TempField",
					"Returns custom field value. If custom field is not found creates one with the specified value. This field will not be included in outgoing message and won't be persist the override.",
					(scope, parent, name, value) => GetOrCreateTempField(scope, parent, name, value, null));

				yield return new Handler<Func<IMacroScope, IDynamicData, string, object, MacroClosure, IDynamicData>>(
					"TempField",
					"Returns custom field value. If custom field is not found creates one with the specified value. This field will not be included in outgoing message and won't be persist the override.",
					(scope, parent, name, value, onValueChange) => GetOrCreateTempField(scope, parent, name, value, onValueChange));

				yield return new Handler<Func<decimal, string, string, decimal>>(
					"Convert",
					convertMacroDescription,
					(value, sourceUnit, targetUnit) => Convert(value, sourceUnit, targetUnit));

				yield return new Handler<Func<Lookups>>(
					"Lookups",
					lookupsMacroDescription,
					() => Lookups);

				// ZArch custom fields

				yield return new Handler<Func<object, string, object>>(
					"GetCustomField",
					getCustomFieldMacroDescription,
					(obj, name) => GetCustomField(obj, name));

				// validation rules

				yield return new Handler<Func<MacroClosure>>(
					"IsValidUnloco",
					"Returns true if data is a valid unloco.",
					() => new MacroClosure(scope => IsValidUnloco(scope)));

				yield return new Handler<Func<MacroClosure>>(
					"IsValidCountryCode",
					"Returns true if data is a valid country code.",
					() => new MacroClosure(scope => IsValidCountryCode(scope)));

				yield return new Handler<Func<MacroClosure>>(
					"IsValidEmailFormat",
					"Returns true if data is a valid email address Format.",
					() => new MacroClosure(scope => IsValidEmailFormat(scope)));

				yield return new Handler<Func<MacroClosure>>(
					"IsValidCurrency",
					"Returns true if data is a valid curency.",
					() => new MacroClosure(scope => IsValidCurrency(scope)));

				yield return new Handler<Func<int, MacroClosure>>(
					"IsNotOlderThan",
					"Returns true if date is within given number of days from today.",
					(days) => new MacroClosure(CreateIsNotOlderThanRule(days)));

				// country

				yield return new Handler<Func<object, ICountry>>(
					"GetCountry",
					"Returns country data for given identifier.",
					pk => GetCountry(pk));

				yield return new Handler<Func<string, string>>(
					"GetCountryName",
					"Returns country name based on a 2 letter code.",
					(code) => GetCountryName(code));

				yield return new Handler<Func<string, string>>(
					"GetPortName",
					"Returns port name based on unloco code.",
					(code) => GetPortName(code));

				// logs

				yield return new Handler<Func<IDynamicData, IEnumerable<Log>>>(
					"GetLogs",
					"Returns logs.",
					(parent) => GetLogs(parent));

				yield return new Handler<Func<Document, MacroClosure, ILog>>(
					"GetLatestEvent",
					"Returns the latest event based on a selector.",
					(doc, selector) => GetLatestEvent(doc, selector));

				yield return new Handler<Func<object, string>>(
					"NumberToWords",
					"Convert number to words.",
					(number) => ConvertNumberToWords(number, Res.CurrentLanguage));

				yield return new Handler<Func<object, string, string>>(
					"NumberToWords",
					"Convert number to words.",
					(number, lang) => ConvertNumberToWords(number, lang));

				// other

				yield return new Handler<Func<MacroMap>>(
					"GetCurrentUserDetails",
					"Returns current user details.",
					() => GetCurrentUserDetails());
			}
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		#endregion

		#region Convert

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		const string convertMacroDescription = "Converts a value from a source unit of measure to a target unit of measure for length, weight and volume or a value from one currency to another as of today's rate.";

		decimal Convert(decimal obj, string sourceUnit, string targetUnit)
		{
			if (Constants.Weight.ContainsCode(sourceUnit) && Constants.Weight.ContainsCode(targetUnit))
			{
				return Constants.Weight.Convert(obj, sourceUnit, targetUnit);
			}
			if (Constants.Volume.ContainsCode(sourceUnit) && Constants.Volume.ContainsCode(targetUnit))
			{
				return Constants.Volume.Convert(obj, sourceUnit, targetUnit);
			}
			if (Constants.Area.ContainsCode(sourceUnit) && Constants.Area.ContainsCode(targetUnit))
			{
				return Constants.Area.Convert(obj, sourceUnit, targetUnit);
			}
			if (Constants.Length.ContainsCode(sourceUnit) && Constants.Length.ContainsCode(targetUnit))
			{
				return Constants.Length.Convert(obj, sourceUnit, targetUnit);
			}

			var sourceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, sourceUnit);
			var targetCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, targetUnit);

			if (sourceCurrency != null && targetCurrency != null)
			{
				var currencyConverter = CurrencyConverter.New(factory);
				currencyConverter.DateForRate = ZDateTime.Today;

				var money = currencyConverter.ConvertExact(new Money(obj, sourceCurrency), targetCurrency);

				return money != null
					? System.Convert.ToDecimal(money.Amount)
					: decimal.Zero;
			}

			return decimal.Zero;
		}

		#endregion

		#region CustomField

		IDynamicData GetOrCreateTempField(IMacroScope scope, IDynamicData parent, string name, object value, MacroClosure onValueChanged)
		{
			var customField = GetOrCreateLocalCustomField(scope, parent, name, value, onValueChanged);

			customField?.SetMetaData(MetaDataType.IsNonPersistent, true);

			return customField;
		}

		IDynamicData GetOrCreateLocalCustomField(IMacroScope scope, IDynamicData parent, string name, object value, MacroClosure onValueChanged)
		{
			var customField = GetOrCreateCustomField(scope, parent, name, value, onValueChanged);

			customField?.SetMetaData(MetaDataType.IsLocal, true);

			return customField;
		}

		IDynamicData GetOrCreateCustomField(IMacroScope scope, IDynamicData parent, string name, object value, MacroClosure onValueChanged)
		{
			if (parent == null
				|| string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			var property = parent.FindDynamicProperty(name);

			if (property != null)
			{
				if (!property.IsAdded())
				{
					var message = string.Format(CultureInfo.InvariantCulture,
					(NoResString)"There is already a property '{0}' of type {1} on {2}. Choose a different name.",
					name,
					property.Type.GetFormattedName(),
					parent.Type.GetFormattedName());

					throw new MacroRuntimeException(message);
				}

				return property;
			}

			ThrowExceptionIfInvalidName(name);

			if (value is MacroClosure lambda)
			{
				object ValueProvider() => ProcessCustomFieldValue(lambda.Invoke(scope));

				property = parent.Properties.GetOrCreate(name, ValueProvider);
			}
			else
			{
				var zTypedValue = ProcessCustomFieldValue(value);

				property = parent.Properties.GetOrCreate(name, zTypedValue, zTypedValue?.GetType() ?? typeof(object));
			}

			if (property != null
				&& onValueChanged != null)
			{
				void OnValueChangedAction()
				{
					using (var evaluationScope = new MacroScope(scope))
					{
						onValueChanged.Invoke(evaluationScope);
					}
				}

				property.SubscribeOnValueChanged(name, OnValueChangedAction);
			}

			return property;
		}

		void ThrowExceptionIfInvalidName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new MacroRuntimeException("Custom field name cannot be empty.");
			}

			const char underscore = '_';

			if (!Char.IsLetter(name[0]) && name[0] != underscore)
			{
				throw new MacroRuntimeException("Custom field name should begin with a letter or underscore.");
			}

			if (name.Skip(1).Any(ch => !Char.IsLetterOrDigit(ch) && ch != underscore))
			{
				throw new MacroRuntimeException("Custom field name contains invalid characters. Only letters, digits and underscores are allowed.");
			}
		}

		object ProcessCustomFieldValue(object value)
		{
			if (value == null)
			{
				return null;
			}

			var dynamicValue = value as IDynamicData;

			if (dynamicValue != null)
			{
				return dynamicValue.Value;
			}

			if (ZDataType.IsConvertibleToZType(value))
			{
				return ZDataType.ObjectToZType(value);
			}

			return value;
		}

		#endregion

		#region CustomField (ZArch)

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		const string getCustomFieldMacroDescription = "Retuns value of a custom field.";

		object GetCustomField(object obj, string name)
		{
			switch (obj)
			{
				case IDynamicData data:
					return GetCustomField(data.Value, name);

				case ICustomFieldProviderProxy providerProxy:
					return CustomiseFieldsMacroHelper.GetCustomField(providerProxy.CustomBusinessObject, name, factory);

				case ICustomFieldProvider provider:
					return CustomiseFieldsMacroHelper.GetCustomField(provider, name, factory);
			}

			return null;
		}

		#endregion

		#region Lookups

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		const string lookupsMacroDescription = "Contains collection of lookups.";

		Lookups Lookups
		{
			get { return lookups ?? (lookups = new Lookups(Factory)); }
		}

		Lookups lookups;

		#endregion

		#region Validation Rules

		bool IsValidUnloco(IMacroScope scope)
		{
			if (scope == null || scope.Data == null)
			{
				return false;
			}

			var unloco = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			if (unloco.Length != StandardUnlocoLength)
			{
				return false;
			}

			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unloco) != null;
		}

		const int StandardUnlocoLength = 5;

		bool IsValidCountryCode(IMacroScope scope)
		{
			if (scope == null || scope.Data == null)
			{
				return false;
			}

			var countryCode = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			if (countryCode.Length != StandardCountryCodeLength)
			{
				return false;
			}

			return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode) != null;
		}

		const int StandardCountryCodeLength = 2;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		bool IsValidEmailFormat(IMacroScope scope)
		{
			if (scope == null || scope.Data == null)
			{
				return false;
			}

			const string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
			var emailText = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			return emailText.Length > 6
				&& new Regex(pattern, RegexOptions.IgnoreCase).Match(emailText).Success;
		}

		bool IsValidCurrency(IMacroScope scope)
		{
			if (scope == null || scope.Data == null)
			{
				return false;
			}

			var currency = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency) != null;
		}

		Func<IMacroScope, object> CreateIsNotOlderThanRule(int days)
		{
			return scope =>
			{
				try
				{
					return System.Convert.ToDateTime(scope.Data, CultureInfo.InvariantCulture) > ZDateTime.Today.AddDays(-days);
				}
				catch (OperationOnInvalidZDateTimeException)
				{
					return false;
				}
				catch (InvalidCastException)
				{
					return false;
				}
			};
		}

		#endregion

		#region GetCountry

		ICountry GetCountry(object pk)
		{
			Guid guid;

			if (Guid.TryParse(System.Convert.ToString(pk, CultureInfo.InvariantCulture), out guid))
			{
				return Factory.Load<RefCountry>(guid);
			}

			return null;
		}

		#endregion

		#region GetCountryName

		string GetCountryName(string code)
		{
			if (code == null || code.Length != StandardCountryCodeLength)
			{
				return string.Empty;
			}

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, code);

			return country != null
				? country.RN_DescMultilingual.ToString()
				: string.Empty;
		}

		#endregion

		#region GetPortName

		string GetPortName(string code)
		{
			if (string.IsNullOrWhiteSpace(code) || (code.Length != StandardUnlocoLength))
			{
				return string.Empty;
			}

			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			return unloco?.RL_PortName.ToString() ?? string.Empty;
		}

		#endregion

		#region GetLogs

		IEnumerable<Log> GetLogs(IDynamicData logParent)
		{
			if (logParent == null)
			{
				return Enumerable.Empty<Log>();
			}

			var id = logParent.GetMetaData<object>(MetaDataType.Identifier);

			if (!Guid.TryParse(System.Convert.ToString(id, CultureInfo.InvariantCulture), out var parentId))
			{
				return Enumerable.Empty<Log>();
			}

			var query = new ZQuery(StmALogSchema.SL_Parent, parentId);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var logs = Factory.Load<StmALog>(query);

			return logs.Select(log => new Log(log));
		}

		#endregion

		#region GetLogs

		ILog GetLatestEvent(Document doc, MacroClosure filter)
		{
			if (doc == null || filter == null)
			{
				return null;
			}

			var logs = doc.Logs;

			if (logs == null)
			{
				return null;
			}

			foreach (var log in logs)
			{
				using (var elementScope = new MacroScope(log))
				{
					if (System.Convert.ToBoolean(filter.Invoke(elementScope), CultureInfo.InvariantCulture))
					{
						return log;
					}
				}
			}

			return null;
		}

		#endregion

		#region ConvertNumberToWords

		string ConvertNumberToWords(object numberObj, string language)
		{
			decimal number;

			if (Decimal.TryParse(System.Convert.ToString(numberObj, CultureInfo.InvariantCulture), out number))
			{
				var result = DecimalNumberToStringConvertor.ConvertDecimalToString(number, language);
				if (string.IsNullOrWhiteSpace(result))
				{
					result = DecimalNumberToStringConvertor.ConvertDecimalToString(number, SharedConstants.Languages.English);
				}

				return result;
			}

			throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to decimal.", numberObj));
		}

		#endregion

		#region GetCurrentUserDetails

		MacroMap GetCurrentUserDetails()
		{
			if (GlbBranch.CurrentBranch?.OrgProxy?.MainAddress != null
				&& !GlbBranch.CurrentBranch.OrgProxy.MainAddress.Address1.IsEmpty)
			{
				var address = GetOrganizationAddress(GlbBranch.CurrentBranch.OrgProxy.MainAddress);

				if (address != null
					&& address.Country == null)
				{
					address.Country = new Enterprise.UniversalDataBuss.DataObjects.Universal.Country
					{
						Code = GlbBranch.CurrentBranch.BaseCountry?.Code,
						Name = GlbBranch.CurrentBranch.BaseCountry?.Description
					};
				}

				return address?.ToMap();
			}

			if (GlbCompany.CurrentCompany?.OrgProxy?.MainAddress != null)
			{
				var address = GetOrganizationAddress(GlbCompany.CurrentCompany.OrgProxy.MainAddress);

				if (address != null
					&& address.Country == null)
				{
					address.Country = new Enterprise.UniversalDataBuss.DataObjects.Universal.Country
					{
						Code = GlbCompany.CurrentCompany.Country?.Code,
						Name = GlbCompany.CurrentCompany.Country?.Description
					};
				}

				return address?.ToMap();
			}

			return null;
		}

		OrganizationAddress GetOrganizationAddress(OrgAddress address)
		{
			if (address == null
				|| address.Header == null)
			{
				return null;
			}

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance)
			{
				AddressType = AddressTypes.CurrentUser,
				OrganizationCode = address.Header.OH_Code,
				CompanyName = address.Header.OH_FullName,
				AddressShortCode = address.OA_Code,
				Address1 = address.Address1,
				Address2 = address.Address2,
				City = address.City,
				State = OrganizationAddressState.New(address.StateCode, (code) => address.State),
				Postcode = address.Postcode,
				Contact = GlbStaff.CurrentUser?.GS_FullName,
				Email = GlbStaff.CurrentUser?.GS_EmailAddress,
				Phone = GlbStaff.CurrentUser?.GS_WorkPhone,
				Fax = GlbStaff.CurrentUser?.GS_FaxNum
			};

			if (address.Country != null)
			{
				orgAddress.Country = new Enterprise.UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = address.Country.Code,
					Name = address.Country.Description
				};
			}

			return orgAddress;
		}

		#endregion
	}
}
