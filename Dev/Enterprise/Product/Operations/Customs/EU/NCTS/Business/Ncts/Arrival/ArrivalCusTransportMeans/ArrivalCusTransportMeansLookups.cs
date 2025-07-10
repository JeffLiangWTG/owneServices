using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ArrivalCusTransportMeansLookups : Customs.Business.CusTransportMeansLookups
	{
		public ArrivalCusTransportMeansLookups(ArrivalCusTransportMeans parent)
		: base(parent)
		{
		}

		protected new ArrivalCusTransportMeans Parent => (ArrivalCusTransportMeans)base.Parent;

		public override CodeDescriptionPairList TransportStateList
		{
			get
			{
				var isNew = Parent.TPM_TransportStateInfo.OriginalValue.ToString() == NctsUnloadedStateList.Codes.NEW;

				return Factory.GetCachedValue($"{nameof(TransportStateList)}_{isNew}_{GetExclusionsHash()}", () =>
				{
					var exclusions = GetExclusions();

					var list = NctsUnloadedStateList.CreateConfigurableList(exclusions);

					if (!isNew)
					{
						list.RemoveCode(NctsUnloadedStateList.Codes.NEW);
					}

					return list;
				});
			}
		}

		IEnumerable<string> GetExclusions()
		{
			if (!ShouldIncludeDIFInUnloadedStatesList)
			{
				yield return NctsUnloadedStateList.Codes.DIF;
			}

			if (!ShouldIncludeDAMInUnloadedStatesList)
			{
				yield return NctsUnloadedStateList.Codes.DAM;
			}
		}

		string GetExclusionsHash()
		{
			var exclusions = GetExclusions();
			return string.Join("_", exclusions);
		}

		protected virtual ZBool ShouldIncludeDIFInUnloadedStatesList => false;

		protected virtual ZBool ShouldIncludeDAMInUnloadedStatesList => false;

		public CodeDescriptionPairList TypeOfIdentificationList => Factory.GetCachedValue<NctsTransportTypeOfIdList>();

		public ZZRefCusCodeListCombinedCollection TransportNationalityList => Factory.GetNCNATCountryList();
	}
}
