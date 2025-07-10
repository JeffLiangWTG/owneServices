using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsAddressValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CustomsAddressValidator(null, "Mock", Factory.New<JobDeclaration>()));
	}

	public void TestValidateCompanyNameMaximumLength_WhenIsInTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false))
			{
				OrgHeaderPk = organization.PK,
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			const string fullNameMessageWarningExpected = "Mock Company Name is longer than 35 characters, it will be truncated in the message.";

			organization.OH_FullName = "".PadRight(36, 'A');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, fullNameMessageWarningExpected);

			organization.OH_FullName = "".PadRight(5, 'A');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, fullNameMessageWarningExpected);
		}
	}

	public void TestValidateCompanyNameMaximumLength_WhenIsInTransitionPeriodButShouldNotApplyTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false), shouldApplyTransitionPeriod: false)
			{
				OrgHeaderPk = organization.PK,
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			organization.OH_FullName = "".PadRight(36, 'A');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarnings("When shouldApplyTransitionPeriod = false, transition period limitations are not applied", propertyInfoWhereToAddWarnings);
		}
	}

	public void TestValidateCompanyNameMaximumLength_WhenNotInTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: false, isImport: false))
			{
				OrgHeaderPk = organization.PK,
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			const string fullNameMessageWarningExpected = "Mock Company Name is longer than 70 characters, it will be truncated in the message.";

			organization.OH_FullName = "".PadRight(71, 'A');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, fullNameMessageWarningExpected);

			organization.OH_FullName = "".PadRight(5, 'A');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, fullNameMessageWarningExpected);
		}
	}

	public void TestValidateAddressMaximumLength_WhenInTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false))
			{
				OrgHeaderPk = organization.PK
			};

			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;
			var mainAddress = organization.MainAddress;
			const string addressMessageWarningExpected = "Mock Address is longer than 35 characters, it will be truncated in the message.";

			mainAddress.OA_Address1 = "".PadRight(20, 'A');
			mainAddress.OA_Address2 = "".PadRight(20, 'B');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, addressMessageWarningExpected);

			mainAddress.OA_Address2 = "".PadRight(10, 'B');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, addressMessageWarningExpected);
		}
	}

	public void TestValidateAddressMaximumLength_WhenInTransitionPeriodButShouldNotApplyTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false), shouldApplyTransitionPeriod: false)
			{
				OrgHeaderPk = organization.PK
			};

			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;
			var mainAddress = organization.MainAddress;

			mainAddress.OA_Address1 = "".PadRight(20, 'A');
			mainAddress.OA_Address2 = "".PadRight(20, 'B');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarnings("When shouldApplyTransitionPeriod = false, transition period limitations are not applied", propertyInfoWhereToAddWarnings);
		}
	}

	public void TestValidateAddressMaximumLength_WhenNotInTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: false, isImport: false))
			{
				OrgHeaderPk = organization.PK
			};

			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;
			var mainAddress = organization.MainAddress;
			const string addressMessageWarningExpected = "Mock Address is longer than 70 characters, it will be truncated in the message.";

			mainAddress.OA_Address1 = "".PadRight(40, 'A');
			mainAddress.OA_Address2 = "".PadRight(40, 'B');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, addressMessageWarningExpected);

			mainAddress.OA_Address2 = "".PadRight(10, 'B');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, addressMessageWarningExpected);
		}
	}

	public void TestValidatePostcodeMaximumLength()
	{
		const string postCodeMessageWarningExpected = "Mock Postcode is longer than 9 characters, it will be truncated in the message.";

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false))
			{
				OrgHeaderPk = organization.PK
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			var mainAddress = organization.MainAddress;

			mainAddress.OA_PostCode = "".PadRight(10, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, postCodeMessageWarningExpected);

			mainAddress.OA_PostCode = "".PadRight(7, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, postCodeMessageWarningExpected);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: false, isImport: false)) { OrgHeaderPk = organization.PK };
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;
			var mainAddress = organization.MainAddress;
			mainAddress.OA_PostCode = "".PadRight(10, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, postCodeMessageWarningExpected);
		}
	}

	public void TestValidatePostcodeMaximumLengthWhenInTransitionPeriodButShouldNotApplyTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: true, isImport: false), shouldApplyTransitionPeriod: false)
			{
				OrgHeaderPk = organization.PK
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			var mainAddress = organization.MainAddress;

			mainAddress.OA_PostCode = "".PadRight(10, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarnings("When shouldApplyTransitionPeriod = false, transition period limitations are not applied", propertyInfoWhereToAddWarnings);
		}
	}

	public void TestValidateCityMaximumLength()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			var mockNonPersistentBusinessObject = new MockNonPersistentBusinessObject(Factory, new MockDeclaration(isUCC6AndIsExport: true, isTransitionPeriodAES30: false, isImport: false))
			{
				OrgHeaderPk = organization.PK
			};
			var propertyInfoWhereToAddWarnings = mockNonPersistentBusinessObject.OrgHeaderPkInfo;

			var mainAddress = organization.MainAddress;
			var cityMessageWarningExpected = "Mock City is longer than 35 characters, it will be truncated in the message.";

			mainAddress.OA_City = "".PadRight(40, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertHasWarningContaining(propertyInfoWhereToAddWarnings, cityMessageWarningExpected);

			mainAddress.OA_City = "".PadRight(10, '0');
			mockNonPersistentBusinessObject.Validation.ValidateOrgHeaderPk();
			AssertNoWarningContaining(propertyInfoWhereToAddWarnings, cityMessageWarningExpected);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		organization = Factory.NewWithValidTestData<OrgHeader>();
	}

	OrgHeader organization;

	#region MockNonPersistentBusinessObject

	class MockNonPersistentBusinessObject : NonPersistentBusinessObject<MockNonPersistentBusinessObjectValidation>
	{
		public MockNonPersistentBusinessObject(BusinessObjectFactory factory, IUCC6AndTransitionPeriodProvider declaration, bool shouldApplyTransitionPeriod = true) : base(factory)
		{
			Declaration = declaration;
			ShouldApplyTransitionPeriod = shouldApplyTransitionPeriod;
		}

		public ZGuid OrgHeaderPk
		{
			get => orgHeaderPk;
			set
			{
				SetNonPersistentPropertyValue(OrgHeaderPkInfo, ref orgHeaderPk, value);
				if (!orgHeaderPk.IsEmpty)
				{
					Validation.ValidateOrgHeaderPk();
				}
			}
		}

		ZGuid orgHeaderPk;

		public OrgAddress MainAddress => Factory.Load<OrgHeader>(OrgHeaderPk).MainAddress;

		public ZPropertyInfo OrgHeaderPkInfo => GetZPropertyInfo(nameof(OrgHeaderPk), "Mock");

		public override MockNonPersistentBusinessObjectValidation GetNewValidation() => new MockNonPersistentBusinessObjectValidation(this);

		public IUCC6AndTransitionPeriodProvider Declaration { get; }

		public bool ShouldApplyTransitionPeriod { get; }
	}

	class MockNonPersistentBusinessObjectValidation : ZValidation
	{
		public MockNonPersistentBusinessObjectValidation(MockNonPersistentBusinessObject parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly MockNonPersistentBusinessObject parent;

		public override Type AutoValidationType => typeof(MockNonPersistentBusinessObject);

		public override void ValidateAll()
		{
			ValidateOrgHeaderPk();
		}

		public void ValidateOrgHeaderPk()
		{
			ValidateCalculatedProperty(parent.OrgHeaderPkInfo);
		}

		protected void CheckOrgHeaderPk()
		{
			var orgHeaderPkInfo = parent.OrgHeaderPkInfo;
			if (parent.MainAddress != null)
			{
				new CustomsAddressValidator(parent.MainAddress, "Mock", parent.Declaration, parent.ShouldApplyTransitionPeriod)
					.ValidateMaximumLengthCustomsFields(orgHeaderPkInfo);
			}
		}
	}

	class MockDeclaration : IUCC6AndTransitionPeriodProvider
	{
		public MockDeclaration(bool isUCC6AndIsExport, bool isTransitionPeriodAES30, bool isImport)
		{
			this.isImport = isImport;
			this.isUCC6AndIsExport = isUCC6AndIsExport;
			this.isTransitionPeriodAES30 = isTransitionPeriodAES30;
		}

		readonly bool isUCC6AndIsExport;
		readonly bool isTransitionPeriodAES30;
		readonly bool isImport;

		bool IUCC6AndTransitionPeriodProvider.IsUCC6AndIsExport => isUCC6AndIsExport;

		bool IUCC6AndTransitionPeriodProvider.IsTransitionPeriodAES30 => isTransitionPeriodAES30;

		bool IUCC6AndTransitionPeriodProvider.IsImport => isImport;
	}

	#endregion
}
