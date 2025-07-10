using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject
	{
		public OrgHeaderWrapper(OrgHeader header)
			: base(header.Factory)
		{
			Organisation = header;
		}
		public readonly OrgHeader Organisation;

		public static OrgHeaderWrapper New(OrgHeader header) => header?.Factory.GetCachedValue($"BR_OrgHeaderWrapper_{header.PK}", () => new OrgHeaderWrapper(header));

		[ChildEditable(true)]
		public AdditionalIdentificationCollection AdditionalIdentification
		{
			get
			{
				if (fAdditionalIdentification == null)
				{
					fAdditionalIdentification = new AdditionalIdentificationCollection(Organisation);
					fAdditionalIdentification.Load();
					RegisterEditableChildObject(fAdditionalIdentification);
				}
				return fAdditionalIdentification;
			}
		}

		AdditionalIdentificationCollection fAdditionalIdentification;

		public BROrgImpAddInfo AddInfo => fAddInfo ??= BROrgImpAddInfo.Get(Organisation);
		BROrgImpAddInfo fAddInfo;
	}
}
