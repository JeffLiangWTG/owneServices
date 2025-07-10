using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPERateTransportProvider : RateTransportProvider
	{
		public const string CODPostcodeTransportOrgCode = "UPSCODTP";
		public const string BrownPostcodeTransportOrgCode = "UPSBROWN";

		public UPERateTransportProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static UPERateTransportProvider LoadCODPostcodeTransportProvider(BusinessObjectFactory factory)
		{
			return LoadTransportProviderFromOrgCode(factory, CODPostcodeTransportOrgCode);
		}

		public static UPERateTransportProvider LoadBrownPostcodeTransportProvider(BusinessObjectFactory factory)
		{
			return LoadTransportProviderFromOrgCode(factory, BrownPostcodeTransportOrgCode);
		}

		static UPERateTransportProvider LoadTransportProviderFromOrgCode(BusinessObjectFactory factory, string orgCode)
		{
			UPERateTransportProvider result = null;

			OrgHeader transportOrg = OrgHeader.LoadFromCode(factory, orgCode);
			if (transportOrg != null)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, transportOrg.PK);
				result = (UPERateTransportProvider)factory.LoadTop1(typeof(UPERateTransportProvider), filter);
			}

			return result;
		}

		#region Property Overrides

		public override ZGuid TP_OH_RelatedParty
		{
			get { return base.TP_OH_RelatedParty; }
			set
			{
				base.TP_OH_RelatedParty = value;
				Zones.MarkAsNeedingValidation();
			}
		}

		#endregion
	}
}
