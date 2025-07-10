using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class CHLRNGeneratorHelperTest : TestCaseWithFactory
{
	[TestDate(2022, 12, 22)]
	public void TestGenerateLocalReferenceNumber_CurrentCompany()
	{
		var numberFountain = Env.NumberFountains.CHLocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());

		CombineAssertions(() =>
		{
			var lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain);
			AssertEquals("LRN empty when Current Branch BID empty and Current Company BID Empty", ZString.Empty, lrnNumber);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3000088059", "CH");
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain);
			AssertEquals("LRN 2230000880590000000001", "2230000880590000000001", lrnNumber);
			AssertEquals("LRN 2230000880590000000001 from company when branch BID empty", true, lrnNumber.Contains("3000088059"));

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3000088059777", "CH");
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain);
			AssertEquals("LRN 2230000880590000000002 if BID longer than 10", "2230000880590000000002", lrnNumber);
			AssertEquals("LRN 2230000880590000000002 from company when branch BID empty", true, lrnNumber.Contains("3000088059"));

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3088059", "CH");
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain);
			AssertEquals("LRN 2200030880590000000003 if BID shorter than 10", "2200030880590000000003", lrnNumber);
			AssertEquals("LRN 2200030880590000000003 from company when branch BID empty", true, lrnNumber.Contains("3088059"));

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain);
			AssertEquals("LRN 2210000880590000000004 if BID shorter than 10", "2210000880590000000004", lrnNumber);
			AssertEquals("LRN 1000088059 from branch if entered", true, lrnNumber.Contains("1000088059"));

			AssertEquals("Length of LRN Number", 22, lrnNumber.Length);
			AssertEquals("First two digits of LRN Number YY", "22", lrnNumber.SubstringSafe(0, 2));
			AssertEquals("Next 10 digits BID", "1000088059", lrnNumber.SubstringSafe(2, 10));
			AssertEquals("SEQUENCE", "0000000004", lrnNumber.SubstringSafe(12));
		});
	}

	[TestDate(2022, 12, 22)]
	public void TestGenerateLocalReferenceNumber_OrgHeader()
	{
		var orgHeader = OrgHeader.New(Factory);
		var numberFountain = Env.NumberFountains.CHLocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());

		CombineAssertions(() =>
		{
			var lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain, orgHeader);
			AssertEquals("LRN empty when orgHeader Company BID Empty", ZString.Empty, lrnNumber);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3000088059", Core.Constants.CountryCodes.Switzerland);
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain, orgHeader);
			AssertEquals("LRN 2230000880590000000001", "2230000880590000000001", lrnNumber);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3000088059777", Core.Constants.CountryCodes.Switzerland);
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain, orgHeader);
			AssertEquals("LRN 2230000880590000000002 if BID longer than 10", "2230000880590000000002", lrnNumber);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "3088059", Core.Constants.CountryCodes.Switzerland);
			lrnNumber = CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, numberFountain, orgHeader);
			AssertEquals("LRN 2200030880590000000003 if BID shorter than 10", "2200030880590000000003", lrnNumber);

			AssertEquals("Length of LRN Number", 22, lrnNumber.Length);
			AssertEquals("First two digits of LRN Number YY", "22", lrnNumber.SubstringSafe(0, 2));
			AssertEquals("Next 10 digits BID", "0003088059", lrnNumber.SubstringSafe(2, 10));
			AssertEquals("SEQUENCE", "0000000003", lrnNumber.SubstringSafe(12));
		});
	}
}
