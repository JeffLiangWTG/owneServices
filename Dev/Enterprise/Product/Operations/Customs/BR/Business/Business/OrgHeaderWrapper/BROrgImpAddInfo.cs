using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BROrgImpAddInfo : AutoBROrgImpAddInfo, IOrgImpAddInfo
	{
		public BROrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public BROrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		[ResourceStringData("BROrgImpAddInfo.ZO_BankCode", Caption = "Bank Code")]
		public override ZString ZO_BankCode
		{
			get { return base.ZO_BankCode; }
			set { base.ZO_BankCode = value; }
		}

		[ResourceStringData("BROrgImpAddInfo.ZO_BSBNumber", Caption = "BSB No.")]
		public override ZString ZO_BSBNumber
		{
			get { return base.ZO_BSBNumber; }
			set { base.ZO_BSBNumber = value; }
		}

		[ResourceStringData("BROrgImpAddInfo.ZO_AccountNumber", Caption = "Account No.")]
		public override ZString ZO_AccountNumber
		{
			get { return base.ZO_AccountNumber; }
			set { base.ZO_AccountNumber = value; }
		}

		[List(nameof(Lookups) + "." + nameof(BROrgImpAddInfoLookups.StaffList))]
		[ResourceStringData("BROrgImpAddInfo.ZO_BrokerCode", Caption = "Credential to send message")]
		public override ZString ZO_BrokerCode
		{
			get { return base.ZO_BrokerCode; }
			set { base.ZO_BrokerCode = value; }
		}

		public GlbStaff Broker => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ZO_BrokerCode);

		public GlbExternalPassword_CCT BrokerCertificate => BRGlbStaffWrapper.Get(Broker)?.GetCCTPassword();

		public static BROrgImpAddInfo Get(OrgHeader organisation) => (BROrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.Brazil).ImpAddInfo;
	}
}
