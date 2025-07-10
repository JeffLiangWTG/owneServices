using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitReportLookups : EU.ExitControl.Business.CusExitReportLookups
	{
		public CusExitReportLookups(CusExitReport parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<DEExitReportTypeList>();

		public CustomsOfficeCodeCollection OfficeOfExportList => GetCustomOfficeList(EuOfficeCodesTypes.Codes.OfficeOfExit);

		public override CustomsOfficeCodeCollection OfficeOfExitList
		{
			get
			{
				var additionalInfoFullTypes = Parent?.Consignment?.AdditionalInfos?.Cast<ExitControlAdditionalInfo>().Select(e => e.CSI_Code).ToList() ?? new List<ZString>();
				var fullTypeIsX1004 = additionalInfoFullTypes.Any(e => e == UniversalReferenceConstants.RefCusCodeList.Codes.X1004);

				if (fullTypeIsX1004)
				{
					return GetCustomOfficeList(EuOfficeCodesTypes.Codes.OfficeOfDeparture);
				}

				var x1002OrX1003 = new ZString[]
				{
					UniversalReferenceConstants.RefCusCodeList.Codes.X1002,
					UniversalReferenceConstants.RefCusCodeList.Codes.X1003,
				};
				var fullTypeInX1002OrX1003 = additionalInfoFullTypes.Any(e => e.In(x1002OrX1003));

				if (fullTypeInX1002OrX1003)
				{
					return GetCustomOfficeList(EuOfficeCodesTypes.Codes.OfficeOfExit);
				}

				return base.OfficeOfExitList;
			}
		}

		protected new CusExitReport Parent => (CusExitReport)base.Parent;

		CustomsOfficeCodeCollection GetCustomOfficeList(ZString role) => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Germany, role);

		public new ZZRefCusCodeListCombinedCollection TransportNationalities => Factory.GetEXNATCountryList();

		public OrgHeaderCollection LocationCollection => new OrgHeaderCollection(Factory);
	}
}
