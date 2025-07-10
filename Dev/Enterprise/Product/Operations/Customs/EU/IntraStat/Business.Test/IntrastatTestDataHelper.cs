using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	public class IntrastatTestDataHelper
	{
		IntrastatTestDataHelper(BusinessObjectFactory factory)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public static IntrastatTestDataHelper New(BusinessObjectFactory factory) => new IntrastatTestDataHelper(factory);

		public CusIntrastatGroup NewCusIntrastatGroupWithValidData<T>() where T : CusIntrastatGroup
		{
			var report = factory.New<T>();
			report.CIG_Flow = ReportFlowCodeDescriptionPairList.Codes.Import;
			var seed = Guid.NewGuid().ToString("N");
			report.CIG_GroupNumber = Guid.NewGuid().ToString("N");
			report.CIG_Period = seed.Substring(0, 7);
			report.CIG_GC_Company = GlbCompany.CurrentCompany.PK;
			report.CIG_OH_Reporter = GetOrCreateOrgHeader("R" + seed.Substring(0, 2)).PK;
			return report;
		}

		public CusIntrastatGroup NewCusIntrastatGroupWithValidData() => NewCusIntrastatGroupWithValidData<CusIntrastatGroup>();

		public CusIntrastatHeader NewCusIntrastatHeaderWithValidData<T>() where T : CusIntrastatHeader
		{
			var report = factory.New<T>();
			report.CIH_OH_Supplier = GetOrCreateOrgHeader("S01").PK;
			report.CIH_OH_Consignee = GetOrCreateOrgHeader("C01").PK;
			report.CIH_TradersReference = Guid.NewGuid().ToString("N");
			report.CIH_CountryOfReceipt = "DE";
			report.CIH_CountryOfSupply = "FR";
			report.CIH_TransactionDate = ZDate.BrettsBirthday;
			return report;
		}

		public CusIntrastatHeader NewCusIntrastatHeaderWithValidData() => NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();

		public CusIntrastatMergedLine NewCusIntrastatMergedLineWithValidData(CusIntrastatGroup parent = null, OrgHeader trader = null)
		{
			var report = parent ?? NewCusIntrastatGroupWithValidData();
			var line = report.CusIntrastatMergedLines.AddNew();
			line.CIM_OH_Trader = trader?.PK ?? GetOrCreateOrgHeader("R" + report.CIG_GroupNumber.Substring(0, 2)).PK;
			line.CIM_Tariff = "12345678";
			line.CIM_MemberState = Core.Constants.CountryCodes.Latvia;
			return line;
		}

		public CusIntrastatLine NewCusIntrastatLineWithValidData(CusIntrastatHeader parent = null)
		{
			var report = parent ?? NewCusIntrastatHeaderWithValidData();
			var line = report.CusIntrastatLines.AddNew();
			line.CIL_Tariff = "12345678";
			return line;
		}

		public OrgHeader GetOrCreateOrgHeader(string code)
		{
			var orgHeader = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));
			if (orgHeader == null)
			{
				orgHeader = OrgHeader.New(factory);
				orgHeader.OH_Code = code;
				var orgAddress = orgHeader.Addresses.AddNew();
				orgAddress.Address1 = code;
				orgAddress.AddressCode = code;
			}
			return orgHeader;
		}

		readonly BusinessObjectFactory factory;
	}
}
