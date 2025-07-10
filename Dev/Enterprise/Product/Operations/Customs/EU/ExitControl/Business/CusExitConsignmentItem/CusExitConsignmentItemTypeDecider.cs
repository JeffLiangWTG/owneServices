using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentItemTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentItem>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentItem>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentItem>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentItem>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentItem>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitConsignmentItem);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitConsignmentCountryCode(row, factory));

		ZString GetCusExitConsignmentCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var consignmentPK = row != null ? new ZGuid(row[AutoCusExitConsignmentItem.Schema.CCI_CXC_Consignment]) : ZGuid.Invalid;
			var consignment = factory.Load<CusExitConsignment>(consignmentPK);
			return consignment?.Header?.CountryCode ?? CurrentCountryCode;
		}
	}
}
