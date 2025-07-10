using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Module
{
	public class InspectionStatusFilter : ModuleCodeFilter
	{
		public InspectionStatusFilter(ZString description, GetCodeQuery queryDelegate, BusinessObjectFactory factory)
			: base(description, queryDelegate, DummyGetList, DummyGetList)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		protected override FilterCategory DefaultCategory => FilterCategories.TextSearch;

		static GetList DummyGetList => () => null;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		public override ZString Property1
		{
			get
			{
				if (string.IsNullOrWhiteSpace(FirstChar) && string.IsNullOrWhiteSpace(SecondChar) && string.IsNullOrWhiteSpace(ThirdChar) && string.IsNullOrWhiteSpace(FourthChar))
				{
					return string.Empty;
				}

				return GetWildcardIfNeeded(FirstChar) + GetWildcardIfNeeded(SecondChar) + GetWildcardIfNeeded(ThirdChar) + GetWildcardIfNeeded(FourthChar);
			}
		}

		ZString GetWildcardIfNeeded(ZString inputString) => inputString.IsEmpty ? new ZString("_") : inputString;

		[BusinessObjectTestExclude]
		[List(nameof(FirstCharList))]
		public ZString FirstChar
		{
			get { return firstChar; }
			set
			{
				if (!FirstCharList.ContainsCode(value))
				{
					value = ZString.Empty;
				}

				InvalidateCachedQuery();
				SetNonPersistentPropertyValue(FirstCharInfo, ref firstChar, value);
				FirstCharInfo.RefreshBinding();
			}
		}
		ZString firstChar;

		public ZPropertyInfo FirstCharInfo
		{
			get { return GetZPropertyInfo(nameof(FirstChar)); }
		}

		public ICodeDescriptionPairList FirstCharList => factory.GetCachedValue<InspectionStatus_1_CargoType>();

		[BusinessObjectTestExclude]
		[List(nameof(SecondCharList))]
		public ZString SecondChar
		{
			get { return secondChar; }
			set
			{
				if (!SecondCharList.ContainsCode(value))
				{
					value = ZString.Empty;
				}

				InvalidateCachedQuery();
				SetNonPersistentPropertyValue(SecondCharInfo, ref secondChar, value);
				SecondCharInfo.RefreshBinding();
			}
		}
		ZString secondChar;

		public ZPropertyInfo SecondCharInfo
		{
			get { return GetZPropertyInfo(nameof(SecondChar)); }
		}

		public ICodeDescriptionPairList SecondCharList => factory.GetCachedValue<InspectionStatus_2_InspectionType>();

		[BusinessObjectTestExclude]
		[List(nameof(ThirdCharList))]
		public ZString ThirdChar
		{
			get { return thirdChar; }
			set
			{
				if (!ThirdCharList.ContainsCode(value))
				{
					value = ZString.Empty;
				}

				InvalidateCachedQuery();
				SetNonPersistentPropertyValue(ThirdCharInfo, ref thirdChar, value);
				ThirdCharInfo.RefreshBinding();
			}
		}
		ZString thirdChar;

		public ZPropertyInfo ThirdCharInfo
		{
			get { return GetZPropertyInfo(nameof(ThirdChar)); }
		}

		public ICodeDescriptionPairList ThirdCharList => factory.GetCachedValue<InspectionStatus_3_InspectionSubType>();

		[BusinessObjectTestExclude]
		[List(nameof(FourthCharList))]
		public ZString FourthChar
		{
			get { return fourthChar; }
			set
			{
				if (!FourthCharList.ContainsCode(value))
				{
					value = ZString.Empty;
				}

				InvalidateCachedQuery();
				SetNonPersistentPropertyValue(FourthCharInfo, ref fourthChar, value);
				FourthCharInfo.RefreshBinding();
			}
		}
		ZString fourthChar;

		public ZPropertyInfo FourthCharInfo
		{
			get { return GetZPropertyInfo(nameof(FourthChar)); }
		}
		public ICodeDescriptionPairList FourthCharList => factory.GetCachedValue<InspectionStatus_4_DocumentRequestType>();
	}
}
