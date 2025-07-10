using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitContainerTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitContainer>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitContainer>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitContainer>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitContainer);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitContainerCountryCode(row, factory));

		ZString GetCusExitContainerCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var cusExitHeaderId = row != null ? new ZGuid(row[ExitControlBase.Business.CusExitContainer.Schema.CXN_CXH_Header]) : ZGuid.Invalid;
			var cusExitHeader = factory.Load<CusExitHeader>(cusExitHeaderId);
			return cusExitHeader?.CountryCode ?? CurrentCountryCode;
		}
	}
}
