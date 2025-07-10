using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.Testing
{
	sealed class EntryChargeTypeTest : TestCaseWithFactory
	{
		public void TestChargeCodeForRating()
		{
			EntryChargeType element1 = new EntryChargeType(ChargeTypeList, "Code", "Description", true, "ZIN");
			AssertEquals("ChargeCodeForRating", "ZIN", element1.ChargeCodeForRating);

			EntryChargeType element2 = new EntryChargeType(ChargeTypeList, "ZAN", "Nevermind", false, null);
			AssertEquals("ChargeCodeForRating", "ZAN", element2.ChargeCodeForRating);

			EntryChargeType element3 = new EntryChargeType(ChargeTypeList, "ZON", "Never Woven", false, "");
			AssertEquals("ChargeCodeForRating", "ZON", element3.ChargeCodeForRating);
		}

		public void TestGetChargeTypeSpecificRegistrySettingsOnNonGSTItem()
		{
			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var chargeTypeSetting = chargeTypeSettings.AddNew();
			chargeTypeSetting.ChargeType = "DTY";
			chargeTypeSetting.AC_ChargeCode = AccChargeCode.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);

			EntryChargeType chargeTypeElement = ChargeTypeList["DTY"];
			AssertEquals("Precondition: chargeTypeElement.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing", false, chargeTypeElement.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);
			EntryChargeTypeSetting loadedChargeTypeSetting = chargeTypeElement.GetChargeTypeSpecificRegistrySettings();
			AssertNotNull("loadedChargeTypeSetting", loadedChargeTypeSetting);
			AssertEquals("loadedChargeTypeSetting.ChargeType", "DTY", loadedChargeTypeSetting.ChargeType);
			AssertEquals("loadedChargeTypeSetting.AC_ChargeCode", AccChargeCode.PK, loadedChargeTypeSetting.AC_ChargeCode);
		}

		public void TestGetChargeTypeSpecificRegistrySettingsOnGSTItem()
		{
			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var chargeTypeSetting = chargeTypeSettings.AddNew();
			chargeTypeSetting.ChargeType = "ENF";
			chargeTypeSetting.AC_ChargeCode = AccChargeCode.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);

			EntryChargeType chargeTypeElement = ChargeTypeList["EFG"];
			AssertEquals("Precondition: chargeTypeElement.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing", true, chargeTypeElement.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);
			EntryChargeTypeSetting loadedChargeTypeSetting = chargeTypeElement.GetChargeTypeSpecificRegistrySettings();
			AssertNotNull("loadedChargeTypeSetting", loadedChargeTypeSetting);
			AssertEquals("loadedChargeTypeSetting.ChargeType", "ENF", loadedChargeTypeSetting.ChargeType);
			AssertEquals("loadedChargeTypeSetting.AC_ChargeCode", AccChargeCode.PK, loadedChargeTypeSetting.AC_ChargeCode);
		}

		public void TestEntryChargeTypeElement()
		{
			EntryChargeType element1 = new EntryChargeType(ChargeTypeList, "Code", "Description", true, "ZIN");
			AssertEquals("Code", "Code", element1.Code);
			AssertEquals("Description", "Description", element1.Description);
			AssertEquals("IsPaidWhenMessageClears", true, element1.IsPaidWhenMessageClears);
			AssertEquals("ParentCodeForGSTOnARInvoice", "ZIN", element1.ParentCodeForGSTOnARInvoice);
			AssertEquals("IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing", true, element1.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);

			EntryChargeType element2 = new EntryChargeType(ChargeTypeList, "1", "2", false, null);
			AssertEquals("Code", "1", element2.Code);
			AssertEquals("Description", "2", element2.Description);
			AssertEquals("IsPaidWhenMessageClears", false, element2.IsPaidWhenMessageClears);
			AssertEquals("ParentCodeForGSTOnARInvoice", "", element2.ParentCodeForGSTOnARInvoice);
			AssertEquals("IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing", false, element2.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);

			EntryChargeType element3 = new EntryChargeType(ChargeTypeList, "44", "55", false, "");
			AssertEquals("Code", "44", element3.Code);
			AssertEquals("Description", "55", element3.Description);
			AssertEquals("IsPaidWhenMessageClears", false, element3.IsPaidWhenMessageClears);
			AssertEquals("ParentCodeForGSTOnARInvoice", "", element3.ParentCodeForGSTOnARInvoice);
			AssertEquals("IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing", false, element3.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);
		}

		#region ChargeTypeList
		EntryChargeTypeList ChargeTypeList => fChargeTypeList ??= new EntryChargeTypeList_ForTest();
		EntryChargeTypeList fChargeTypeList;
		#endregion

		#region AccChargeCode
		BusinessObject AccChargeCode
		{
			get
			{
				if (fAccChargeCode == null)
				{
					System.Reflection.Assembly masterFilesAssembly = System.Reflection.Assembly.Load("Enterprise.MasterFiles.Business");
					Type accChargeCodeType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.AccChargeCode");
					fAccChargeCode = Factory.New(accChargeCodeType);
					fAccChargeCode.FillWithValidTestData();
					Factory.Save();
				}
				return fAccChargeCode;
			}
		}
		BusinessObject fAccChargeCode;
		#endregion
	}

	public class EntryChargeTypeList_ForTest : EntryChargeTypeList
	{
		public static class Codes
		{
			public const string Duty = "DTY";
			public const string EntryFee = "ENF";
			public const string EntryFeeGST = "EFG";
			public const string GST = "GST";
		}

		public static class Descriptions
		{
			public const string Duty = "Duty";
			public const string EntryFee = "Entry Fee";
			public const string EntryFeeGST = "Entry Fee GST";
			public const string GST = "GST";
		}

		public EntryChargeTypeList_ForTest()
		{
			Add(Codes.Duty, Descriptions.Duty, true, ZString.Empty);
			Add(Codes.EntryFee, Descriptions.EntryFee, true, ZString.Empty);
			Add(Codes.EntryFeeGST, Descriptions.EntryFeeGST, true, Codes.EntryFee);
			Add(Codes.GST, Descriptions.GST, true, ZString.Empty);
		}

		public override string DutyCode => Codes.Duty;
		public override string TaxCode => Codes.GST;
	}
}
