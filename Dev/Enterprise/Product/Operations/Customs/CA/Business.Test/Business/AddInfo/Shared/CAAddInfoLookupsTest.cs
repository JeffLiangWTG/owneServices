using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	public abstract class CAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2020, 12, 24)]
		public void TestModelYearList()
		{
			var parent = GetNewAddInfo();
			var list = parent.Lookups.ModelYearList;
			Assert(list.ContainsCode("2022"));
		}

		public void TestProperties()
		{
			var parent = GetNewAddInfo();
			AssertEquals(parent.Lookups.Parent, parent);
			AssertEquals(typeof(RefCurrencyCollection), parent.Lookups.DeclaredCurrencies.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), parent.Lookups.CBSAOffices.GetType());
			AssertEquals(typeof(ReasonForExportList), parent.Lookups.ReasonForExportCodes.GetType());
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.CanadianProvinces.GetType());
			AssertEquals(typeof(USStatesList), parent.Lookups.CFIAStatesOfOrigin.GetType());
			AssertEquals(typeof(ImportReasonCodes), parent.Lookups.ImportReasonCodes.GetType());
			AssertEquals(typeof(ValueForDutyCodes), parent.Lookups.ValueForDutyCodes.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), parent.Lookups.TreatmentCodes.GetType());
			AssertEquals(typeof(CACFIAEndUseCodesCollection), parent.Lookups.CFIAEndUseCodes.GetType());
			AssertEquals(typeof(CACFIAMiscCodesCollection), parent.Lookups.CFIAMiscIDCodes.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), parent.Lookups.USPortOfExitList.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), parent.Lookups.TradeZones.GetType());
			AssertEquals(typeof(GSTStatusCodes), parent.Lookups.GSTStatusCodes.GetType());
			AssertEquals(typeof(SIMACodes), parent.Lookups.SIMACodes.GetType());
			AssertEquals(typeof(ExciseTaxExemptionCodes), parent.Lookups.ETExemptionCodes.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), parent.Lookups.CasualImportCommodity.GetType());
			AssertEquals(typeof(AVSStatusList), parent.Lookups.OGDStatusCodes.GetType());
			AssertEquals(typeof(CAInitiatedByList), parent.Lookups.CAInitiatedByList.GetType());
			AssertEquals(typeof(AmendmentToList), parent.Lookups.AmendmentToList.GetType());
		}

		protected abstract AddInfo GetNewAddInfo();
	}
}
