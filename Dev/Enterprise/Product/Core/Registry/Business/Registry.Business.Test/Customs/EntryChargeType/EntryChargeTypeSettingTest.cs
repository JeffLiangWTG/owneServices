using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeSetting))]
	sealed class EntryChargeTypeSettingTest : RegistryBusinessObjectTemplateTestCase<EntryChargeTypeSetting>
	{
		public void TestChargeTypeStopsDuplicatesWhenUsedThroughTheCollection()
		{
			EntryChargeTypeSetting chargeType1 = ChargeTypeCollection.AddNew();
			chargeType1.ChargeType = EntryChargeTypeList_ForTest.Codes.Duty;

			EntryChargeTypeSetting chargeType2 = ChargeTypeCollection.AddNew();
			chargeType2.ChargeType = EntryChargeTypeList_ForTest.Codes.Duty;
			AssertHasWarningContaining(chargeType2.ChargeTypeInfo, EntryChargeTypeSetting.ErrorDuplicateChargeType);
			chargeType2.ChargeType = EntryChargeTypeList_ForTest.Codes.EntryFee;
			AssertNoNotifications(chargeType2.ChargeTypeInfo);

			EntryChargeTypeSetting chargeType3 = ChargeTypeCollection.AddNew();
			chargeType3.ChargeType = EntryChargeTypeList_ForTest.Codes.Duty;
			AssertHasWarningContaining(chargeType3.ChargeTypeInfo, EntryChargeTypeSetting.ErrorDuplicateChargeType);
			chargeType3.ChargeType = EntryChargeTypeList_ForTest.Codes.EntryFee;
			AssertHasWarningContaining(chargeType3.ChargeTypeInfo, EntryChargeTypeSetting.ErrorDuplicateChargeType);
			chargeType3.ChargeType = EntryChargeTypeList_ForTest.Codes.GST;
			AssertNoNotifications(chargeType3.ChargeTypeInfo);

			chargeType2.ChargeType = EntryChargeTypeList_ForTest.Codes.GST;
			AssertHasWarningContaining(chargeType2.ChargeTypeInfo, EntryChargeTypeSetting.ErrorDuplicateChargeType);
			chargeType2.ChargeType = EntryChargeTypeList_ForTest.Codes.EntryFee;
			AssertNoNotifications(chargeType2.ChargeTypeInfo);
		}

		public void TestChargeTypeValidation()
		{
			ChargeType.ChargeType = EntryChargeTypeList_ForTest.Codes.Duty;
			AssertNoNotifications(ChargeType.ChargeTypeInfo);
			ChargeType.ChargeType = "ZXZ";
			AssertHasWarnings(ChargeType.ChargeTypeInfo);
			ChargeType.ChargeType = EntryChargeTypeList_ForTest.Codes.GST;
			AssertNoNotifications(ChargeType.ChargeTypeInfo);
			ChargeType.ChargeType = "";
			AssertHasWarningContaining(ChargeType.ChargeTypeInfo, EntryChargeTypeSetting.ErrorMustHaveChargeType);
		}

		public void TestChargeTypeDescription()
		{
			ChargeType.ChargeType = EntryChargeTypeList_ForTest.Codes.EntryFeeGST;
			AssertEquals("ChargeType.ChargeTypeDescription", EntryChargeTypeList_ForTest.Descriptions.EntryFeeGST, ChargeType.ChargeTypeDescription);
			ChargeType.ChargeType = EntryChargeTypeList_ForTest.Codes.Duty;
			AssertEquals("ChargeType.ChargeTypeDescription", EntryChargeTypeList_ForTest.Descriptions.Duty, ChargeType.ChargeTypeDescription);
			ChargeType.ChargeType = ZString.Empty;
			AssertEquals("ChargeType.ChargeTypeDescription", ZString.Empty, ChargeType.ChargeTypeDescription);
		}

		public void TestValidationOnAC_ChargeCode()
		{
			ChargeType.AC_ChargeCode = AccChargeCode.PK;
			AssertNoNotifications(ChargeType.AC_ChargeCodeInfo);
			ChargeType.AC_ChargeCode = ZGuid.NewZGuid();
			AssertHasErrors(ChargeType.AC_ChargeCodeInfo);
			ChargeType.AC_ChargeCode = AccChargeCode.PK;
			AssertNoNotifications(ChargeType.AC_ChargeCodeInfo);
			ChargeType.AC_ChargeCode = ZGuid.Empty;
			AssertHasError(ChargeType.AC_ChargeCodeInfo, EntryChargeTypeSetting.ErrorMustHaveChargeCode);
		}

		public void TestDoNotValidateWhenFallbackIsUnknown()
		{
			var entryChargeTypeSetting = new EntryChargeTypeSetting(null, Factory, ChargeTypeCollection);
			entryChargeTypeSetting.AC_ChargeCode = ZGuid.NewZGuid();
			AssertNoNotifications("This is what is happening during loading of a registry item in a transient way, then the validation should have refreshed. We work around this way.", ChargeType.AC_ChargeCodeInfo);
		}

		public void TestChargeCode_List()
		{
			BizObj.CurrentFallbackLevel = null;
			AssertChargeCodes(BizObj.ChargeCode_List, Env.CurrentCompany.PK);

			IGlbCompany anotherCompany = Factory.LoadTop1<IGlbCompany>(
			new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK));
			BizObj.CurrentFallbackLevel = new FallbackLevel(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertChargeCodes(BizObj.ChargeCode_List, anotherCompany.PK);
		}

		void AssertChargeCodes(BusinessObjectCollection chargeCodes, ZGuid companyPK)
		{
			chargeCodes.Load();
			Assert("There should be at least one charge code.", chargeCodes.Count > 0);
			foreach (BusinessObject chargeCode in chargeCodes)
			{
				AssertEquals("AC_GC", companyPK, chargeCode[AccChargeCodeSchema.Constants.AC_GC]);
			}
		}

		public void TestChargeTypeListForAU()
		{
			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var chargeType = collection.AddNew();
			AssertType<AU.EntryChargeTypeList>(chargeType.ChargeType_List);
			Assert(!chargeType.ChargeType_List.ContainsCode(AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount));

			AssertNoErrorContaining(chargeType.ChargeTypeInfo, EntryChargeTypeSetting.ErrorCantHaveASPChargeType);
			chargeType.ChargeType = AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount;
			AssertHasErrorContaining(chargeType.ChargeTypeInfo, EntryChargeTypeSetting.ErrorCantHaveASPChargeType);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand);
		}

		#region ChargeType
		EntryChargeTypeSetting ChargeType
		{
			get
			{
				if (fChargeType == null)
				{
					fChargeType = new EntryChargeTypeSetting_ForTest(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory, ChargeTypeCollection);
				}
				return fChargeType;
			}
		}
		EntryChargeTypeSetting fChargeType;
		#endregion

		#region ChargeTypeCollection
		EntryChargeTypeSettingCollection_ForTest ChargeTypeCollection
		{
			get
			{
				if (fChargeTypeCollection == null)
				{
					fChargeTypeCollection = new EntryChargeTypeSettingCollection_ForTest(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
				}
				return fChargeTypeCollection;
			}
		}
		EntryChargeTypeSettingCollection_ForTest fChargeTypeCollection;
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
				}
				return fAccChargeCode;
			}
		}
		BusinessObject fAccChargeCode;
		#endregion

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override EntryChargeTypeSetting GetBusinessObjectToClone()
		{
			return new EntryChargeTypeSetting_ForTest(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory, ChargeTypeCollection);
		}

		protected override EntryChargeTypeSetting GetBusinessObjectToSerialise()
		{
			ChargeType.AC_ChargeCode = AccChargeCode.PK;
			ChargeType.ChargeType = EntryChargeTypeList_ForTest.Codes.EntryFee;
			return ChargeType;
		}

		#endregion
	}

	class EntryChargeTypeSetting_ForTest(FallbackLevel fallbackLevel, BusinessObjectFactory factory, EntryChargeTypeSettingCollection parentCollection) : EntryChargeTypeSetting(fallbackLevel, factory, parentCollection)
	{
		protected override EntryChargeTypeList GetChargeType_ListCore()
		{
			return new EntryChargeTypeList_ForTest();
		}
	}

	class EntryChargeTypeSettingCollection_ForTest(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : EntryChargeTypeSettingCollection(fallbackLevel, factory)
	{
		public new EntryChargeTypeSetting_ForTest AddNew()
		{
			return (EntryChargeTypeSetting_ForTest)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntryChargeTypeSetting_ForTest(CurrentFallbackLevel, CurrentFactory, this);
		}
	}
}
