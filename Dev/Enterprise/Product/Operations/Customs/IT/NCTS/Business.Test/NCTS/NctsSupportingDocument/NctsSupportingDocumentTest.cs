using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsSupportingDocument))]
sealed class NctsSupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsSupportingDocument>
{
	public void TestCSI_QuantityDecimalPlaces()
	{
		var decimalPlacesAttribute = typeof(NctsSupportingDocument)
				.GetProperty("CSI_Quantity")
				.GetCustomAttributes(false)
				.OfType<DecimalPlacesAttribute>()
				.SingleOrDefault();
		AssertNotNull("[DecimalPlaces] attribute", decimalPlacesAttribute);
		AssertEquals("[DecimalPlaces] attribute value", 5, decimalPlacesAttribute.DecimalPlaces);
	}

	public void TestCSI_YearOfIssueMaxLength()
	{
		AssertEquals(nameof(supportingDocument.CSI_YearOfIssueInfo.MaxLength), 4, supportingDocument.CSI_YearOfIssueInfo.MaxLength);
	}

	public void TestCSI_YearOfIssue()
	{
		CombineAssertions("Case with no date set at all", () =>
		{
			AssertDateFields(ZDateTime.Empty, ZString.Empty);
		});

		CombineAssertions("Setting CSI_YearOfIssue with alphanumeric chars", () =>
		{
			supportingDocument.CSI_YearOfIssue = "ASD";
			AssertDateFields(ZDateTime.Empty, ZString.Empty);
		});

		CombineAssertions("Setting CSI_YearOfIssue with year out of minimum range", () =>
		{
			supportingDocument.CSI_YearOfIssue = "1";
			AssertDateFields(ZDateTime.Empty, ZString.Empty);
		});

		CombineAssertions("Setting CSI_YearOfIssue with year out of maximum range", () =>
		{
			supportingDocument.CSI_YearOfIssue = "11111";
			AssertDateFields(ZDateTime.Empty, ZString.Empty);
		});

		CombineAssertions("Setting CSI_YearOfIssue with valid year", () =>
		{
			supportingDocument.CSI_YearOfIssue = "2021";
			AssertDateFields(new ZDateTime(2021, 1, 1), "2021");
		});

		CombineAssertions("Setting CSI_DateOfIssue with valid date", () =>
		{
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2021, 11, 14, 2, 3, 4);
			AssertDateFields(new ZDateTime(2021, 1, 1), "2021");
		});

		CombineAssertions("Setting CSI_DateOfIssue with invalid date", () =>
		{
			supportingDocument.CSI_DateOfIssue = ZDateTime.Invalid;
			AssertDateFields(ZDateTime.Invalid, ZString.Empty);
		});

		CombineAssertions("Setting CSI_DateOfIssue with empty date", () =>
		{
			supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertDateFields(ZDateTime.Empty, ZString.Empty);
		});

		CombineAssertions("Setting CSI_DateOfIssue with dynamic date", () =>
		{
			var utcNow = ZDateTime.UtcNow;
			supportingDocument.CSI_DateOfIssue = utcNow;
			AssertDateFields(new ZDateTime(utcNow.Year, 1, 1), utcNow.Year.ToString());
		});

		void AssertDateFields(ZDateTime issueDate, ZString year)
		{
			AssertEquals(nameof(supportingDocument.CSI_DateOfIssue), issueDate, supportingDocument.CSI_DateOfIssue);
			AssertEquals(nameof(supportingDocument.CSI_YearOfIssue), year, supportingDocument.CSI_YearOfIssue);
		}
	}

	[ExpectNoExceptions]
	public void TestSetCSI_CodeTriggersOtherFieldsValidation()
	{
		var mockSupportingDocument = Factory.NewMoq<NctsSupportingDocument>();
		var mockSupportingDocumentValidation = new Mock<NctsSupportingDocumentValidation>(mockSupportingDocument.Object);
		mockSupportingDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockSupportingDocumentValidation.Object);
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_RN_NKCountryCode");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_ReferenceNumber");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_DateOfIssue");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_Quantity");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_UnitOfQuantity");

		mockSupportingDocument.Object.CSI_Code = "XXX";
		mockSupportingDocument.Verify();
		mockSupportingDocumentValidation.VerifyAll();
	}

	public void TestLookupsType()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsSupportingDocumentPhase4Lookups>(supportingDocument.Lookups);
	}

	public void TestLookupsType_Phase5()
	{
		AssertType<NctsSupportingDocumentPhase5Lookups>(supportingDocument.Lookups);
	}

	public void TestRefCusCode_Phase5_Departure()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var nctsCodeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
		var dataGroupingEU = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", dataGroupingEU);
		helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "Ncts Supporting Document Type");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Level", nctsCodeType, dataGroupingEU.ZZZ_DataGrouping);
		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, nctsCodeType, "YYY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListWithAttribute(dataGroupingEU.ZZZ_DataGrouping, nctsCodeType, "YYY", "YYY Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Level", "Item");
		Factory.Save();

		supportingDocument.CSI_Code = "YYY";
		var refCusCode = supportingDocument.RefCusCode;
		AssertNotNull("RefCusCode", refCusCode);
		CombineAssertions(() =>
		{
			AssertEquals("RefCusCode.ZZD_Code", "YYY", refCusCode.ZZD_Code);
			AssertEquals("RefCusCode.ZZD_CountryOrGrouping", "IT", refCusCode.ZZD_CountryOrGrouping);
		});
	}

	protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		yield return goodsItem.SupportingDocuments.AddNew();
	}

	public void TestAsISupportingDocument()
	{
		supportingDocument.CSI_Code = "A";
		supportingDocument.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		supportingDocument.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
		supportingDocument.CSI_ReferenceNumber = "1";
		supportingDocument.CSI_Quantity = 1m;
		supportingDocument.CSI_UnitOfQuantity = "X";
		supportingDocument.CSI_Status = SADConstants.CertificateFlag.DER;
		CombineAssertions(() =>
		{
			var asInterface = (ISupportingDocument)supportingDocument;
			AssertEquals(nameof(asInterface.CountryOfIssue), Core.Constants.CountryCodes.Italy, asInterface.CountryOfIssue);
			AssertEquals(nameof(asInterface.Quantity), 1m, asInterface.Quantity);
			AssertEquals(nameof(asInterface.ReferenceNumber), "1", asInterface.ReferenceNumber);
			AssertEquals(nameof(asInterface.Status), SADConstants.CertificateFlag.DER, asInterface.Status);
			AssertEquals(nameof(asInterface.Type), "A", asInterface.Type);
			AssertEquals(nameof(asInterface.UnitOfQuantity), "X", asInterface.UnitOfQuantity);
			AssertEquals(nameof(asInterface.YearOfIssue), "2021", asInterface.YearOfIssue);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => supportingDocument;

	#region ReadOnlyProviderTests

	public void TestReadOnlyProvider_Phase4()
	{
		var supportingDocument = Factory.New<NctsSupportingDocumentPhase4ForTest>();
		AssertType<NctsSupportingDocumentPhase4ReadOnlyProvider>(supportingDocument.ReadOnlyProvider);
	}

	public void TestReadOnlyProvider_Phase5Arrival()
	{
		var supportingDocument = Factory.New<NctsArrivalSupportingDocumentPhase5ForTest>();
		AssertType<NctsSupportingDocumentPhase5ArrivalReadOnlyProvider>(supportingDocument.ReadOnlyProvider);
	}

	public void TestReadOnlyProvider_Phase5Departure()
	{
		var supportingDocument = Factory.New<NctsDepartureSupportingDocumentPhase5ForTest>();
		AssertType<NctsSupportingDocumentNcts5DepartureReadOnlyConditionsProvider>(supportingDocument.ReadOnlyProvider);
	}

	#region Implementation

	sealed class NctsDepartureSupportingDocumentPhase5ForTest : NctsSupportingDocument
	{
		public NctsDepartureSupportingDocumentPhase5ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CSI_ParentID = nctsHeader.PK;
			CSI_ParentTableCode = nctsHeader.TablePrefix;
		}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();
	}

		sealed class NctsArrivalSupportingDocumentPhase5ForTest : NctsSupportingDocument
		{
			public NctsArrivalSupportingDocumentPhase5ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var nctsArrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
				CSI_ParentID = nctsArrivalMovementHeader.PK;
				CSI_ParentTableCode = nctsArrivalMovementHeader.TablePrefix;
			}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();

		internal Mock<ISupportingDocumentReadOnlyConditions> ReadOnlyProiderMock;

		protected override ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider() => ReadOnlyProiderMock?.Object ?? base.GetNewReadOnlyProvider();
	}

	sealed class NctsSupportingDocumentPhase4ForTest : NctsSupportingDocument
	{
		public NctsSupportingDocumentPhase4ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			CSI_ParentID = nctsHeader.PK;
			CSI_ParentTableCode = nctsHeader.TablePrefix;
		}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();
	}

	#endregion

	#endregion

	#region GetNewValidationTests

	public void TestGetNewValidation_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsSupportingDocumentValidation>(supportingDocument.Validation);
	}

	public void TestGetNewValidation_Phase5Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var supportingDocument = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
			AssertType<NctsSupportingDocumentPhase5ArrivalValidation>(supportingDocument.Validation);
		}

	public void TestGetNewValidation_Phase5Departure()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsSupportingDocumentPhase5DepartureValidation>(supportingDocument.Validation);
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		supportingDocument = goodsItem.SupportingDocuments.AddNew();
	}

	NctsHeader nctsHeader;
	NctsSupportingDocument supportingDocument;
}
