using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSRegistrationNumberLookups : Customs.Business.CusCodeDataLookups
	{
		public AIRSRegistrationNumberLookups(AIRSRegistrationNumber parent)
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
					var pgaHeader = Parent.Parent as IPGAProgramRequirementProvider;
					return RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, pgaHeader?.GetParentInvoiceLine()?.Declaration?.EffectiveImporter);
				}
				return new CodeDescriptionPairList();
			}
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList AIRSRegTypes
		{
			get { return new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, ZDateTime.UtcToday.Date); }
		}
	}
}
