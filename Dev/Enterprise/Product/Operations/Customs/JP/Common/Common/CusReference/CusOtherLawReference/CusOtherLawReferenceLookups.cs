using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public class CusOtherLawReferenceLookups : CusReferenceLookups
	{
		public CusOtherLawReferenceLookups(AutoCusReference parent) : base(parent)
		{
		}

		public new CusOtherLawReference Parent => (CusOtherLawReference)base.Parent;

		public CodeDescriptionPairList ReferenceList
		{
			get
			{
				var today = ZDateTime.Today;
				var iCusOtherLawReferenceParent = Parent.Parent as ICusOtherLawReferenceParent;
				var messageType = iCusOtherLawReferenceParent?.MessageType ?? ZString.Empty;
				var japanOtherLaws = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws;
				var tariffAttributes = iCusOtherLawReferenceParent?.GetTariffAttributesByKey(japanOtherLaws) ?? Enumerable.Empty<ZString>();
				return Factory.GetCachedValue($"JP.CusOtherLawReferenceLookups.ReferenceList-{today}-{messageType}-{ZString.Join("-", tariffAttributes.ToArray())}", () =>
				{
					var result = new CodeDescriptionPairList();
					var attributeName = messageType.ToString() switch
					{
						Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export => "IsExport",
						Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import => "IsImport",
						_ => string.Empty
					};
					var codes = string.IsNullOrEmpty(attributeName) ?
					ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, today) :
					ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, [japanOtherLaws], today, new[] { new RefCusCodeListAttributeFilter(attributeName, SQLComparisonOperator.Equal, "Y") });
					codes.Load();

					if (tariffAttributes.Any())
					{
						result.AddPairsIfNotExist(codes.Where(c => tariffAttributes.Contains(c.ZZD_Code)));
					}
					if (messageType == Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export)
					{
						result.AddPairsIfNotExist(codes.Where(c => c.ZZD_Code == "EI"));
					}
					result.AddPairsIfNotExist(codes);

					return result;
				});
			}
		}
	}
}
