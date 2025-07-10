using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class CFIARegistrationNumberLookups : Customs.Business.CusCodeDataLookups
	{
		public CFIARegistrationNumberLookups(CFIARegistrationNumber parent)
			: base(parent)
		{
		}

		protected new CusCodeData Parent => (CusCodeData)base.Parent;

		public override CodeDescriptionPairList CY_DataList
		{
			get
			{
				if (Parent.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence)
				{
					var invoiceLine = Parent.Parent as JobComInvoiceLine;
					return RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, invoiceLine?.Declaration?.EffectiveImporter);
				}
				return new CodeDescriptionPairList();
			}
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList CFIARegTypes
		{
			get { return new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, ZDateTime.UtcToday.Date); }
		}
	}
}
