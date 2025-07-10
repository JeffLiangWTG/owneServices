using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5BBAmendmentDetailsManager : AmendmentDetailsManager<Import5BAHeader, IImport5BAHeader>
	{
		public GOVCBR5BBAmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		public override ZString AmendmentType
		{
			get
			{
				if (!amendmentType.HasValue)
				{
					amendmentType = ZString.Empty;
					var amendTypes = AmendedItems.Select(x => x.AmendType).Distinct();
					var count = amendTypes.Count();
					if (count == 1)
					{
						switch (amendTypes.First())
						{
							case EntityAmendType.Add:
								amendmentType = _5BBAmendmentType.Codes.Add;
								break;
							case EntityAmendType.Delete:
								amendmentType = _5BBAmendmentType.Codes.Delete;
								break;
							case EntityAmendType.Update:
								amendmentType = _5BBAmendmentType.Codes.Update;
								break;
						}
					}
					else if (count > 1)
					{
						amendmentType = _5BBAmendmentType.Codes.Mix;
					}
				}
				return amendmentType.Value;
			}
		}

		protected override CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory) => factory.GetCachedValue<GOVCBR5BADataItemIDList>();

		ZString? amendmentType;

		protected override Import5BAHeader GetCurrentDataProvider() => new Import5BAHeaderCreator().Create(Entry);

		protected override IEnumerable<string> GetMandatoryItems() => Enumerable.Empty<string>();
	}
}
