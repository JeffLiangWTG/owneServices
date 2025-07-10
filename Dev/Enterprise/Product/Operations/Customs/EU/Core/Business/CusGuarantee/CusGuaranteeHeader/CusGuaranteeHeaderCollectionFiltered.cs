using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business
{
	[ModuleID(ModuleId.Guarantees)]
	public class CusGuaranteeHeaderCollectionFiltered : CusGuaranteeHeaderCollection
	{
		public CusGuaranteeHeaderCollectionFiltered(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusGuaranteeHeaderCollectionFiltered(BusinessObjectFactory factory, IReadOnlyList<ZString> countryCodesToLoad, IReadOnlyList<ZString> types)
			: base(factory, countryCodesToLoad, types)
		{
		}

		public CusGuaranteeHeaderCollectionFiltered(BusinessObjectFactory factory, IReadOnlyList<ZString> countryCodesToLoad, IReadOnlyList<ZString> types, IReadOnlyList<ZString> references)
			: base(factory, countryCodesToLoad, types, references)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public new class FilterConstants : CusGuaranteeHeaderCollection.FilterConstants
		{
			public const string GuaranteeRule = "Guarantee Rule";
		}
	}
}
