using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPivotTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentPivot>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentPivot>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentPivot>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentPivot>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitConsignmentPivot);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitConsignmentPivotCountryCode(row, factory));

		ZString GetCusExitConsignmentPivotCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var consignmentItemPk = row != null ? new ZGuid(row[CusExitConsignmentPivot.Schema.CNP_CCI_ConsignmentItem]) : ZGuid.Invalid;
			var consignmentItem = factory.Load<CusExitConsignmentItem>(consignmentItemPk);
			return consignmentItem.Consignment?.Header?.CountryCode ?? CurrentCountryCode;
		}
	}
}
