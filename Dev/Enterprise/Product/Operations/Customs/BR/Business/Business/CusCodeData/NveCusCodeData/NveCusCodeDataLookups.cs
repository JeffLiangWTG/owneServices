using System.Linq;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NveCusCodeDataLookups : Customs.Business.CusCodeDataLookups
	{
		public NveCusCodeDataLookups(NveCusCodeData parent)
			: base(parent)
		{
		}

		protected new NveCusCodeData Parent
		{
			get { return (NveCusCodeData)base.Parent; }
		}

		public CodeDescriptionPairList PositionList => Factory.GetCachedValue<PositionList>();

		public CodeDescriptionPairList SpecificationList
		{
			get
			{
				CodeDescriptionPairList result = null;

				var characteristic = Parent.TariffCharacteristic;
				if (characteristic != null && characteristic.Values != null)
				{
					return Factory.GetCachedValue($"NveCusCodeDataLookups_PossibleValues_{characteristic.PK}", () =>
					{
						result = new CodeDescriptionPairList();
						foreach (var item in characteristic.Values.Cast<RefCusTariffBRCharacteristicValue>())
						{
							result.AddPairIfNotExist(item.ZB2_Value, item.ZB2_Description);
						}
						result.Sort();
						return result;
					});
				}

				return result ?? new CodeDescriptionPairList();
			}
		}
	}
}
