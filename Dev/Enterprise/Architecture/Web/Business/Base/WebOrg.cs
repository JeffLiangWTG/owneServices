using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	internal sealed class WebOrg : LightweightBizo<OrgHeader>, IWebOrg
	{
		public WebOrg(WebFactory webUserFactory, OrgHeader org)
			: base(org)
		{
			webFactory = webUserFactory;
			OH_Code = org.OH_Code;
			OH_FullName = org.OH_FullName;
			OH_IsConsignee = org.OH_IsConsignee;
			OH_IsConsignor = org.OH_IsConsignor;
			OH_IsWarehouseClient = org.OH_IsWarehouseClient;
			OH_IsForwarder = org.OH_IsForwarder;
			OH_IsGlobalAccount = org.OH_IsGlobalAccount;
		}

		readonly WebFactory webFactory;

		#region Simple Properties

		public ZString OH_Code { get; }
		public ZString OH_FullName { get; }
		public ZBool OH_IsConsignee { get; }
		public ZBool OH_IsConsignor { get; }
		public ZBool OH_IsWarehouseClient { get; }
		public ZBool OH_IsForwarder { get; }
		public ZBool OH_IsGlobalAccount { get; }

		#endregion

		#region Complex Properties

		public ZString OH_RL_NKClosestPort
		{
			get
			{
				if (closestPort == null)
				{
					closestPort = GetHeader().OH_RL_NKClosestPort;
				}
				return closestPort;
			}
		}
		string closestPort;

		public ZString OH_FullNameTruncated => OH_FullName.SubstringSafe(0, OrgHeader.Schema.OH_FullNameTruncatedLength);
		public ZString CountryCode => OH_RL_NKClosestPort.Left(2);

		public ZString BranchOrOrgCountryCode
		{
			get
			{
				if (branchOrOrgCountryCode == null)
				{
					branchOrOrgCountryCode = GetHeader().Branch?.Country.Code ?? CountryCode;
				}
				return branchOrOrgCountryCode;
			}
		}
		string branchOrOrgCountryCode;

		#endregion

		public PartAttributeManager PartAttributeManager => GetHeader().PartAttributeManager;
		public BusinessObjectFactory Factory => webFactory.Factory;
		public OrgHeader GetHeader() => base.GetHeavy(this);

		#region MiscServ

		WebOrgMiscServ webOrgMiscServ;

		public OrgMiscServ GetMiscServ()
		{
			OrgMiscServ result;

			if (webOrgMiscServ == null)
			{
				result = GetHeader().MiscServ;
				webOrgMiscServ = new WebOrgMiscServ(result);
			}
			else
			{
				result = webOrgMiscServ.GetHeavy(this);
			}

			return result;
		}

		#endregion

#if DEBUG
		public override void DiscardAnyFactoryReferenceForTest()
		{
			base.DiscardAnyFactoryReferenceForTest();
			webFactory.DiscardAnyFactoryReferenceForTest();
		}
#endif
	}
}
