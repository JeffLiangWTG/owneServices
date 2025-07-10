using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Customs
{
	public abstract class EntryChargeTypeList : CodeDescriptionPairList
	{
		public new EntryChargeType this[int i]
		{
			get { return (EntryChargeType)base[i]; }
			set { base[i] = value; }
		}

		public new EntryChargeType this[string chargeTypeCode]
		{
			get { return (EntryChargeType)base[chargeTypeCode]; }
		}

		public void Add(EntryChargeType element)
		{
			base.Add(element);
		}

		public void Add(string code, string description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
		{
			Add(new EntryChargeType(this, code, description, isPaidWhenMessageClears, parentCodeForGSTOnARInvoice));
		}

		public void AddIfNotExists(string code, string description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
		{
			if (!ContainsCode(code))
			{
				Add(new EntryChargeType(this, code, description, isPaidWhenMessageClears, parentCodeForGSTOnARInvoice));
			}
		}

		public void Add(string code, MultilingualString description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
		{
			Add(new EntryChargeType(this, code, description, isPaidWhenMessageClears, parentCodeForGSTOnARInvoice));
		}

		protected override int AddCore(ICodeDescription element)
		{
			if (!ExpectedElementType.IsInstanceOfType(element))
			{
				throw new NotSupportedException("An EntryChargeTypeList only accepts " + ExpectedElementType + " instances as elements.");
			}

			return base.AddCore(element);
		}

		protected virtual Type ExpectedElementType => typeof(EntryChargeType);

		public EntryChargeTypeSettingCollectionRegistryItem RegistryItem => RatingDataRegistry.Instance.EntryChargeTypesAndCodes;

		public virtual EntryChargeTypeSetting GetChargeTypeSpecificRegistrySetting(string chargeCode) => RegistryItem?.Value.FindByCode(chargeCode);

		public abstract string DutyCode { get; }
		public abstract string TaxCode { get; }

		public ZGuid[] GetAllChargeCodePKsOf(ZGuid companyPK)
		{
			var result = new List<ZGuid>();

			ZGuid genericOne = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);

			if (!genericOne.IsEmpty)
			{
				result.Add(genericOne);
			}

			ZGuid informationOnly = RatingDataRegistry.Instance.CustomDeferredChargeCode.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			if (!informationOnly.IsEmpty)
			{
				result.Add(informationOnly);
			}

			if (Count > 0)
			{
				foreach (EntryChargeTypeSetting setting in RegistryItem.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if (!setting.AC_ChargeCode.IsEmpty && !result.Contains(setting.AC_ChargeCode))
					{
						result.Add(setting.AC_ChargeCode);
					}
				}
			}
			result.AddRange(GetSpecialChargeCodePks(companyPK));

			return result.ToArray();
		}

		protected virtual IEnumerable<ZGuid> GetSpecialChargeCodePks(ZGuid companyPK) => new List<ZGuid>();

		public static EntryChargeTypeList GetCachedList(BusinessObjectFactory factory, ZString countryCode)
		{
			return factory.GetCachedValue("EntryChargeTypeList_" + countryCode, () => GetList(countryCode));
		}

		public static EntryChargeTypeList GetList(ZString countryCode)
		{
			if (countryCode.IsEmpty)
			{
				return new EmptyEntryChargeTypeList();
			}

			var objectHashtables = (Hashtable)ObjectFactory.Get("CustomsEntryChargeTypeList");
			var allList = new Dictionary<string, EntryChargeTypeList>(objectHashtables.Count);

			foreach (DictionaryEntry entry in objectHashtables)
			{
				var handle = (ObjectHandle)entry.Value;
				allList.Add((string)entry.Key, (EntryChargeTypeList)handle.GetObject());
			}

			allList.TryGetValue(countryCode, out var list);
			return list ?? new ZZEntryChargeTypeList(countryCode);
		}

		public static ZGuid[] GetAllChargeCodePKsOf(ZGuid companyPK, ZString countryCode)
		{
			countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			var list = GetList(countryCode);
			return list.GetAllChargeCodePKsOf(companyPK);
		}

		public void RemoveWhere(Func<EntryChargeType, bool> func)
		{
			var codesToRemove = new System.Collections.Generic.List<EntryChargeType>();
			foreach (var chargeType in this)
			{
				if (chargeType is EntryChargeType code && func(code))
				{
					codesToRemove.Add(code);
				}
			}

			foreach (var code in codesToRemove)
			{
				this.RemoveCode(code);
			}
		}

		public void FilterChargeTypesForRegistry() => FilterChargeTypesForRegistryCore();

		protected virtual void FilterChargeTypesForRegistryCore()
		{
		}
	}

	[ExcludeEntryChargeTypeListFromTest]
	public class EmptyEntryChargeTypeList : EntryChargeTypeList
	{
		public override string DutyCode => "";

		public override string TaxCode => "";
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExcludeEntryChargeTypeListFromTestAttribute : Attribute
	{
	}
}
