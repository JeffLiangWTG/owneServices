using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RegistryItemReservedNameTestExcludeAttribute : Attribute
	{
	}

	public abstract class RegistryItemSet : IRegistryItemSet
	{
		public RegistryItemSet()
		{
			Tracer = ObjectFactory.Get<ITracer>();
		}

		#region ContryFilterPKs

		public static class CountryFilterPKs
		{
			// Please keep these in alphabetical order

			public static IEnumerable<Guid> Argentina
			{
				get { return argentina ?? (argentina = new[] { Enterprise.Core.Constants.CountryGuids.Argentina }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> argentina;

			public static IEnumerable<Guid> Australia
			{
				get { return australia ?? (australia = new[] { Enterprise.Core.Constants.CountryGuids.Australia }); }
			}
			static IEnumerable<Guid> australia;

			public static IEnumerable<Guid> Belgium
			{
				get { return belgium ?? (belgium = new[] { Enterprise.Core.Constants.CountryGuids.Belgium }); }
			}
			static IEnumerable<Guid> belgium;

			public static IEnumerable<Guid> Brazil
			{
				get { return brazil ?? (brazil = new[] { Enterprise.Core.Constants.CountryGuids.Brazil }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> brazil;

			public static IEnumerable<Guid> Canada
			{
				get { return canada ?? (canada = new[] { Enterprise.Core.Constants.CountryGuids.Canada }); }
			}
			static IEnumerable<Guid> canada;

			public static IEnumerable<Guid> Chile
			{
				get { return chile ?? (chile = new[] { Enterprise.Core.Constants.CountryGuids.Chile }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> chile;

			public static IEnumerable<Guid> China
			{
				get { return china ?? (china = new[] { Enterprise.Core.Constants.CountryGuids.China }); }
			}
			static IEnumerable<Guid> china;

			public static IEnumerable<Guid> EuropeanUnion
			{
				get { return europeanUnion ?? (europeanUnion = Enterprise.Core.CountryGuids.CountriesUnderEUCustomsJurisdictionExceptUK); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> europeanUnion;

			public static IEnumerable<Guid> FranceAndOverseasDepartments
			{
				get
				{
					return franceandDepartments ?? (franceandDepartments = new[]
					{
						Enterprise.Core.Constants.CountryGuids.France,
						Enterprise.Core.Constants.CountryGuids.FrenchGuiana,
						Enterprise.Core.Constants.CountryGuids.Mayotte,
						Enterprise.Core.Constants.CountryGuids.Guadeloupe,
						Enterprise.Core.Constants.CountryGuids.Martinique,
						Enterprise.Core.Constants.CountryGuids.Reunion,
						Enterprise.Core.Constants.CountryGuids.SaintBarthelemy,
						Enterprise.Core.Constants.CountryGuids.SaintMartin
					});
				}
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> franceandDepartments;

			public static IEnumerable<Guid> France
			{
				get { return france ?? (france = new[] { Enterprise.Core.Constants.CountryGuids.France }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> france;

			public static IEnumerable<Guid> Germany
			{
				get { return germany ?? (germany = new[] { Enterprise.Core.Constants.CountryGuids.Germany }); }
			}
			static IEnumerable<Guid> germany;

			public static IEnumerable<Guid> HongKong
			{
				get { return hongKong ?? (hongKong = new[] { Enterprise.Core.Constants.CountryGuids.HongKong }); }
			}
			static IEnumerable<Guid> hongKong;

			public static IEnumerable<Guid> IceLand
			{
				get { return iceLand ?? (iceLand = new[] { Enterprise.Core.Constants.CountryGuids.Iceland }); }
			}
			static IEnumerable<Guid> iceLand;

			public static IEnumerable<Guid> India
			{
				get { return india ?? (india = new[] { Enterprise.Core.Constants.CountryGuids.India }); }
			}
			static IEnumerable<Guid> india;

			public static IEnumerable<Guid> Ireland
			{
				get { return ireland ?? (ireland = new[] { Enterprise.Core.Constants.CountryGuids.Ireland }); }
			}
			static IEnumerable<Guid> ireland;

			public static IEnumerable<Guid> Israel
			{
				get { return israel ?? (israel = new[] { Enterprise.Core.Constants.CountryGuids.Israel }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> israel;

			public static IEnumerable<Guid> Italy
			{
				get { return italy ?? (italy = new[] { Enterprise.Core.Constants.CountryGuids.Italy }); }
			}
			static IEnumerable<Guid> italy;

			public static IEnumerable<Guid> Japan
			{
				get { return japan ?? (japan = new[] { Enterprise.Core.Constants.CountryGuids.Japan }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> japan;

			public static IEnumerable<Guid> Jordan
			{
				get { return jordan ?? (jordan = new[] { Enterprise.Core.Constants.CountryGuids.Jordan }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> jordan;

			public static IEnumerable<Guid> KoreaRepublicOf
			{
				get { return koreaRepublicoOf ?? (koreaRepublicoOf = new[] { Enterprise.Core.Constants.CountryGuids.KoreaRepublicof }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> koreaRepublicoOf;

			public static IEnumerable<Guid> Malaysia
			{
				get { return malaysia ?? (malaysia = new[] { Enterprise.Core.Constants.CountryGuids.Malaysia }); }
			}
			static IEnumerable<Guid> malaysia;

			public static IEnumerable<Guid> Mauritius
			{
				get { yield return Enterprise.Core.Constants.CountryGuids.Mauritius; }
			}

			public static IEnumerable<Guid> Mexico
			{
				get { return mexico ?? (mexico = new[] { Enterprise.Core.Constants.CountryGuids.Mexico }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> mexico;

			public static IEnumerable<Guid> Netherlands
			{
				get { return netherlands ?? (netherlands = new[] { Enterprise.Core.Constants.CountryGuids.Netherlands }); }
			}
			static IEnumerable<Guid> netherlands;

			public static IEnumerable<Guid> NewZealand
			{
				get { return newZealand ?? (newZealand = new[] { Enterprise.Core.Constants.CountryGuids.NewZealand }); }
			}
			static IEnumerable<Guid> newZealand;

			public static IEnumerable<Guid> Norway
			{
				get { return norway ?? (norway = new[] { Enterprise.Core.Constants.CountryGuids.Norway }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> norway;

			public static IEnumerable<Guid> Poland
			{
				get { return poland ?? (poland = new[] { Enterprise.Core.Constants.CountryGuids.Poland }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> poland;

			public static IEnumerable<Guid> Romania
			{
				get { return romania ?? (romania = new[] { Enterprise.Core.Constants.CountryGuids.Romania }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> romania;

			public static IEnumerable<Guid> Spain
			{
				get { return spain ?? (spain = new[] { Enterprise.Core.Constants.CountryGuids.Spain }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> spain;

			public static IEnumerable<Guid> Singapore
			{
				get { return singapore ?? (singapore = new[] { Enterprise.Core.Constants.CountryGuids.Singapore }); }
			}
			static IEnumerable<Guid> singapore;

			public static IEnumerable<Guid> SouthAfrica
			{
				get { return southAfrica ?? (southAfrica = new[] { Enterprise.Core.Constants.CountryGuids.SouthAfrica }); }
			}
			static IEnumerable<Guid> southAfrica;

			public static IEnumerable<Guid> Sweden
			{
				get { return sweden ?? (sweden = new[] { Enterprise.Core.Constants.CountryGuids.Sweden }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is protected by the critical section.")]
			static IEnumerable<Guid> sweden;

			public static IEnumerable<Guid> Switzerland
			{
				get { return switzerland ?? (switzerland = new[] { Enterprise.Core.Constants.CountryGuids.Switzerland }); }
			}
			static IEnumerable<Guid> switzerland;

			public static IEnumerable<Guid> Taiwan
			{
				get { return taiwan ?? (taiwan = new[] { Enterprise.Core.Constants.CountryGuids.Taiwan }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> taiwan;

			public static IEnumerable<Guid> Turkey
			{
				get { return turkey ?? (turkey = new[] { Enterprise.Core.Constants.CountryGuids.Turkey }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> turkey;

			public static IEnumerable<Guid> Uruguay
			{
				get { return uruguay ?? (uruguay = new[] { Enterprise.Core.Constants.CountryGuids.Uruguay }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> uruguay;

			public static IEnumerable<Guid> UnitedArabEmirates
			{
				get { return unitedArabEmirates ?? (unitedArabEmirates = new[] { Enterprise.Core.Constants.CountryGuids.UnitedArabEmirates }); }
			}
			static IEnumerable<Guid> unitedArabEmirates;

			public static IEnumerable<Guid> UnitedKingdom
			{
				get { return unitedKingdom ?? (unitedKingdom = new[] { Enterprise.Core.Constants.CountryGuids.UnitedKingdom }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> unitedKingdom;

			public static IEnumerable<Guid> Vietnam
			{
				get { return vietnam ?? (vietnam = new[] { Enterprise.Core.Constants.CountryGuids.Vietnam }); }
			}
			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			static IEnumerable<Guid> vietnam;

			// Please keep these in alphabetical order
		}

		#endregion

		public static MultilingualString CombineCategories(params MultilingualString[] categories)
		{
			return new ModifiedMultilingualString(
				delegate (string[] str)
				{
					StringBuilder result = new StringBuilder();

					foreach (string category in str)
					{
						string trimmedCategory = (category == null) ? "" : category.Trim().Trim(Delimiter[0]);
						if (trimmedCategory.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
						{
							ErrorReporter.ReportOnce(FormattableString.Invariant($@"Category text should not end with with the escape character '\': '{trimmedCategory}'"));
						}
						if ((result.Length > 0) && !string.IsNullOrEmpty(trimmedCategory))
						{
							result.Append(Delimiter);
						}
						result.Append(trimmedCategory);
					}

					return result.ToString();
				},
				categories
			);
		}

		/// <summary>
		/// Finds an IRegistryItem with the specified name.
		/// </summary>
		/// <param name="name">The name to find. This is case-insensitive.</param>
		/// <returns>The result of the search. If no IRegistryItem exists with the specified name, null is returned.</returns>
		public IRegistryItem FindByName(string name)
		{
			foreach (IRegistryItem item in GetAllItems())
			{
				if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}
			return null;
		}

		public IRegistryItem[] GetAllItems()
		{
			List<IRegistryItem> result = new List<IRegistryItem>();

			Type type = GetType();
			List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
			propertyInfos.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
			propertyInfos.AddRange(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (typeof(IRegistryItem).IsAssignableFrom(propertyInfo.PropertyType) &&
					!IsExplicitInterfaceImpl(propertyInfo))
				{
					result.Add((IRegistryItem)propertyInfo.GetValue(this, null));
				}
			}

			IEnumerable<IRegistryItem> itemsNotAccessedUsingProperties = GetItemsNotAccessedUsingProperties();
			if (itemsNotAccessedUsingProperties != null)
			{
				result.AddRange(itemsNotAccessedUsingProperties);
			}

			return result.ToArray();
		}

		static bool IsExplicitInterfaceImpl(PropertyInfo property)
		{
			return property.Name.Contains(".");
		}

		protected IRegistryItem GetItem(string key, CreateItemDelegate<IRegistryItem> createItemDelegate)
		{
			return GetItem<IRegistryItem>(key, createItemDelegate);
		}

		protected T GetItem<T>(string key, CreateItemDelegate<T> createItemDelegate) where T : IRegistryItem
		{
			var item = (T)RegistryItemDictionary.Instance.GetOrAdd(key, () => GetNonCachedItem(createItemDelegate));
			if (Tracer.IsEnabled(CoreTraceSourceCodes.Registry))
			{
				TraceRegistryAccess(item);
			}
			return item;
		}

		void TraceRegistryAccess(IRegistryItem item)
		{
			try
			{
				var filter = Tracer.GetTraceFilter(CoreTraceSourceCodes.Registry);
				var itemCategory = item is IMultilingualRegistryItem multilingualItem ? multilingualItem.CategoryMultilingual.GetUnresolvedString() : item.Category;
				if (itemCategory.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Tracer.TraceInformation(CoreTraceSourceCodes.Registry, () => BuildRegistryAccessTraceMessage(item));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("RegistryItemSet.TraceRegistryAccess", "Unhandled exception was caught tracing registry item access.", ex);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The message is for developer")]
		string BuildRegistryAccessTraceMessage(IRegistryItem item)
		{
			var data = GetDataSafely(item);
			if (CensorRegistryValue(item))
			{
				var emptiness = data.Length == 0 ? "is empty" : "is not empty";
				data = $"******** ({emptiness})";
			}
			return $@"
Registry Path: {item.Category}/{item.Caption}
Registry Key: {item.Name}
Registry Data Type: {item.DataType.GetType().Name}
Value: {data}";

			static string GetDataSafely(IRegistryItem item)
			{
				try
				{
					var dataType = item.DataType;
					return Encoding.Unicode.GetString(dataType.Serialise(item.Value));
				}
				catch (Exception ex)
				{
					return $"******** (Error Retrieving Value: {ex.GetType()})";
				}
			}

			static bool CensorRegistryValue(IRegistryItem item)
			{
				return !(AcceptableSubDataTypes.Contains(item.DataType.DataType) || AcceptableRegistryDataTypes.Contains(item.DataType.GetType()));
			}
		}

		public IEnumerable<IRegistryItem> GetDynamicItems()
		{
			return GetItemsNotAccessedUsingProperties() ?? Array.Empty<IRegistryItem>();
		}

		protected virtual IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
		{
			return null;
		}

		protected T GetNonCachedItem<T>(CreateItemDelegate<T> createItemDelegate) where T : IRegistryItem
		{
			T result = createItemDelegate.Invoke();
			SetDefaultsForNewItem(result);
			return result;
		}

		protected CodeDescriptionPairList GetUndefinedCodeDescriptionPairList(MultilingualString itemPath)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("UDF", ResString.GetMultilingualString("fca744ab-06eb-42c8-97bd-8656e0f40c31", "Undefined - You can modify this in the System Registry, under {0}", itemPath));
			return result;
		}

		public void RemoveItemFromCacheIfOlderThan(string key, TimeSpan age)
		{
			RegistryItemDictionary.Instance.PurgeIfOlderThan(key, age);
		}

		protected virtual void SetDefaultsForNewItem(IRegistryItem item)
		{
		}

		public const string Delimiter = "/";
		protected delegate T CreateItemDelegate<T>() where T : IRegistryItem;

		public abstract bool IsForProductivityWise { get; }

		public bool IsEdiProd => ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI;

		readonly ITracer Tracer;

		#region TraceableRegistryTypes

		//Security: Types may be added to the following lists ONLY if they can NEVER have sensitive information stored in them
		static ImmutableHashSet<Type> AcceptableSubDataTypes => new Type[] { typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(decimal), typeof(double), typeof(float), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(DateTime) }.ToImmutableHashSet();
		static ImmutableHashSet<Type> AcceptableRegistryDataTypes => new Type[] { typeof(CodePairRegistryDataType), typeof(DurationRegistryDataType), typeof(TimeRegistryDataType), typeof(WeightAndVolumeDataType) }.ToImmutableHashSet();

		#endregion
	}
}
