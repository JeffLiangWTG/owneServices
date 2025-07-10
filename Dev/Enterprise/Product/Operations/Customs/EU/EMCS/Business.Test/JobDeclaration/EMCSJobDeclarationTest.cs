using System;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclaration))]
	public class EMCSJobDeclarationTest : BaseJobDeclarationAbstractTest
	{
		public void TestIsConsignor()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("Not Consignor", false, declaration.IsConsignor);
				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("Is Consignor", true, declaration.IsConsignor);
			});
		}

		public void TestIsConsignee()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("Not Consignee", false, declaration.IsConsignee);
				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("Is Consignee", true, declaration.IsConsignee);
			});
		}

		[TestDate(2007, 3, 1)]
		public void TestJE_UCR()
		{
			Factory.Save();
			AssertEquals("7-" + declaration.JE_DeclarationReference, declaration.JE_UCR);
		}

		[UseSnapshotProtection]
		public void TestJE_UCR_NoDuplicateReferenceException()
		{
			var declaration1 = Factory.New<EMCSJobDeclarationForTest>();
			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			declaration1.OnSaving();
			var ucr = declaration1.JE_UCR;
			dbConnection.RollbackTransaction();

			var newFactory = new BusinessObjectFactory();
			var declaration2 = newFactory.New<EMCSJobDeclarationForTest>();
			declaration2.JE_GB = newFactory.NewWithValidTestData<GlbBranch>().PK;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Saving declaration2", () => newFactory.Save());
				AssertEquals("declaration1 and declaration2 have the same JE_UCR", ucr, declaration2.JE_UCR);

				dbConnection.BeginTransaction();
				AssertEquals("Before saved, declaration1 has duplicate JE_UCR", ucr, declaration1.JE_UCR);
				AssertNoExceptionThrown("Saving declaration1", () => Factory.Save());
				AssertNotEquals("After saved, declaration1 has unique JE_UCR", ucr, declaration1.JE_UCR);
			});
		}

		public void TestZG_DeferredSubmission()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", EMCSDeferredSubmissionList.Codes.No, declaration.ZG_DeferredSubmission);
				AssertEquals("Caption", "Deferred", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSJobDeclaration), nameof(EMCSJobDeclaration.ZG_DeferredSubmission)).Caption);
			});
		}

		public void TestJE_DeclarantType_Caption()
		{
			AssertEquals("Declaration Type", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSJobDeclaration), nameof(EMCSJobDeclaration.JE_DeclarantType)).Caption);
		}

		public void TestEADNumber_Caption()
		{
			AssertEquals("EAD Number", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSJobDeclaration), nameof(EMCSJobDeclaration.EADNumber)).Caption);
		}

		public void TestDestinationType_Caption()
		{
			AssertEquals("Destination Type", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSJobDeclaration), nameof(EMCSJobDeclaration.JE_MessageSubType
				)).Caption);
		}

		public void TestJE_DeclarantType_DefaultToConsignor()
		{
			AssertEquals(EMCSEntryTypeList.Codes.Consignor, declaration.JE_DeclarantType);
		}

		public void TestJE_DeclarantType_ReadOnly()
		{
			AssertEquals(true, declaration.JE_DeclarantTypeInfo.ReadOnly);
		}

		public void TestIsMessageStatusSentOrAcknowledged()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var certificate = declaration.Documents.AddNew();
			certificate.FillWithValidTestData();

			var sadNumber = declaration.ImportSADNumbers.AddNew();
			sadNumber.FillWithValidTestData();

			var container = declaration.CusContainers.AddNew();
			container.FillWithValidTestData();

			var officeOfDelivery = declaration.CustomsOffices.AddNew();
			officeOfDelivery.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;

			var officeOfDispatch = declaration.CustomsOffices.AddNew();
			officeOfDispatch.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;

			var propertyInfos = new[]
			{
				declaration.JE_OH_SupplierInfo,
				declaration.ZG_DeferredSubmissionInfo,
				declaration.ZG_OriginTypeInfo,
				declaration.ZG_SubmissionTypeInfo,
				declaration.JE_DateAtOriginInfo,
				declaration.ZG_CCTMSAInfo,
				declaration.ZG_CertOfExemptionInfo,
				declaration.ZG_DispatchReferenceInfo,

				officeOfDispatch.CY_CodeInfo,
				officeOfDispatch.CY_DataInfo,

				certificate.CSI_DescriptionInfo,
				certificate.CSI_ReferenceNumberInfo,

				sadNumber.CSI_DescriptionInfo
			};

			var objects = new BusinessObject[]
			{
				declaration.OwnerDocumentaryAddress,
				declaration.DispatchWarehouseDocumentaryAddress
			};

			void AssertReadOnly(string message, bool expectedReadOnly)
			{
				CombineAssertions(() =>
				{
					AssertEquals(message, expectedReadOnly, declaration.IsMessageStatusSentOrAcknowledged);

					AssertEquals("JE_OH_Importer should always be editable.", false, declaration.JE_OH_ImporterInfo.ReadOnly);
					AssertEquals("JE_MessageSubType should always be editable.", false, declaration.JE_MessageSubTypeInfo.ReadOnly);
					AssertEquals("JE_TransportMode should always be editable.", false, declaration.JE_TransportModeInfo.ReadOnly);
					AssertEquals("JourneyTimeNumericPart should always be editable.", false, declaration.JourneyTimeNumericPartInfo.ReadOnly);
					AssertEquals("JourneyTimeFormatPart should always be editable.", false, declaration.JourneyTimeFormatPartInfo.ReadOnly);
					AssertEquals("ZG_GuarantorType should always be editable.", false, declaration.ZG_GuarantorTypeInfo.ReadOnly);
					AssertEquals("ZG_TransportArrangement should always be editable.", false, declaration.ZG_TransportArrangementInfo.ReadOnly);
					AssertEquals("InvoiceNumber should always be editable.", false, declaration.InvoiceNumberInfo.ReadOnly);
					AssertEquals("InvoiceDate should always be editable.", false, declaration.InvoiceDateInfo.ReadOnly);
					AssertEquals("SpecialInstructions should always be editable.", false, declaration.SpecialInstructionsInfo.ReadOnly);

					AssertEquals("DestinationWarehouseDocumentaryAddress should always be editable.", false, declaration.DestinationWarehouseDocumentaryAddress.ReadOnly);
					AssertEquals("CarrierAgentDocumentaryAddress should always be editable.", false, declaration.CarrierAgentDocumentaryAddress.ReadOnly);
					AssertEquals("TransporterDocumentaryAddress should always be editable.", false, declaration.TransporterDocumentaryAddress.ReadOnly);

					AssertEquals("Container should always be editable.", false, container.ReadOnly);
					AssertEquals("Office Type should always be editable when the type is DEL.", false, officeOfDelivery.CY_CodeInfo.ReadOnly);
					AssertEquals("Office Code should always be editable when the type is DEL.", false, officeOfDelivery.CY_DataInfo.ReadOnly);

					foreach (var info in propertyInfos)
					{
						AssertEquals(string.Concat(info.Name, "-", message), expectedReadOnly, info.ReadOnly);
					}

					foreach (var obj in objects)
					{
						AssertEquals(string.Concat(obj.HumanReadableName, "-", message), expectedReadOnly, obj.ReadOnly);
					}

					AssertEquals("FilteredInvoiceLines should only be readonly when the mesage status is SNT or ACK.", expectedReadOnly, declaration.FilteredInvoiceLines.ReadOnly);
					AssertEquals("Invoice Line should only be readonly when the mesage status is SNT or ACK.", expectedReadOnly, invoiceLine.ReadOnly);
					AssertEquals("Invoice Line should not allow to delete when the mesage status is SNT or ACK.", expectedReadOnly, !((ICanDelete)invoiceLine).CanDelete);
				});
			}

			AssertReadOnly("Should default to false.", false);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			AssertReadOnly("Should be true when the message status of declaration is SNT.", true);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			AssertReadOnly("Should be true when the message status of declaration is ACK.", true);
		}

		public void TestOwnerDocumentaryAddress_Enabled()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Is false default", false, declaration.OwnerDocumentaryAddress_Enabled);

				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
				AssertEquals("Is true when ZG_GuarantorType is 3", true, declaration.OwnerDocumentaryAddress_Enabled);
			});
		}

		public void TestZG_GuarantorType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions(() =>
			{
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
				declaration.OwnerDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				AssertEquals("Set ZG_GuarantorType to 3", orgHeader.PK, declaration.OwnerDocumentaryAddress.OrganisationPK);

				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorAndOfTheOwnerOfTheExciseProducts;
				AssertEquals("Set ZG_GuarantorType to other value", ZGuid.Empty, declaration.OwnerDocumentaryAddress.OrganisationPK);
			});
		}

		public void TestSupportedAddressTypes()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.SupplierDocumentaryAddress,
				DocAddressType.GoodsOwner,
				DocAddressType.CarrierAgent,
				DocAddressType.Transporter,
				DocAddressType.DispatchWarehouse,
				DocAddressType.DestinationWarehouse
			}, ((IDocAddresses)declaration).SupportedAddressTypes);
		}

		public void TestConsignee()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = orgHeader.PK;

			AssertEquals(orgHeader.PK, declaration.Consignee.PK);
		}

		public void TestAllGroupHeaders()
		{
			AssertType<EMCSGroupHeaderCollection>(declaration.AllGroupHeaders);
		}

		public void TestDocuments()
		{
			CombineAssertions(() =>
			{
				var documents = declaration.Documents;
				AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(documents));
				AssertSame("Cached", documents, declaration.Documents);
			});
		}

		public void TestTypeDecider()
		{
			Factory.Save();
			var query = new ZQuery(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
			Assert("Update BaseJobDeclaration to include a decider for this class", Factory.LoadTop1<BaseJobDeclaration>(query).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestAddInfoDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZG_GuarantorType", EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements, declaration.ZG_GuarantorType);
				AssertEquals("ZG_OriginType", EMCSOriginTypeList.Codes.TaxWarehouse, declaration.ZG_OriginType);
				AssertEquals("ZG_SubmissionType", EMCSSubmissionTypeList.Codes.StandardSubmission, declaration.ZG_SubmissionType);
			});
		}

		public void TestZG_TransportArrangement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", EMCSTransportArrangementList.Codes.Consignor, declaration.ZG_TransportArrangement);
				AssertEquals("Caption", "Transport Arr.", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSJobDeclaration), nameof(EMCSJobDeclaration.ZG_TransportArrangement)).Caption);
			});
		}

		public void TestEADNumberCreated()
		{
			declaration.EADNumber = "MRN123";
			Factory.Save();
			var cusEntryNumber = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			AssertEquals("Entry Number", "MRN123", cusEntryNumber.CE_EntryNum);
		}

		public void TestJE_EntryAuthorisationDate()
		{
			var testDate = DateTime.Now.Date;

			declaration.JE_MessageType = "1";

			AssertEquals(ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);

			var declaration2 = Factory.New<EMCSJobDeclarationForTest>();
			declaration2.JE_MessageType = "1";
			declaration2.JE_EntryAuthorisationDate = testDate;

			AssertEquals(testDate, declaration2.JE_EntryAuthorisationDate);

			var cusEntryNum = declaration2.LoadCusEntryNumber(false);
			AssertEquals(testDate, cusEntryNum.CE_IssueDate);
			AssertEquals(CusEntryNumberTypes.Standard.MovementReferenceNumber, cusEntryNum.CE_EntryType);

			declaration2.EADNumber = "123"; // required to be able to save the CusEntryNumber

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var declarationCopy = otherFactory.Load<EMCSJobDeclarationForTest>(declaration2.PK);
			AssertEquals(testDate, declarationCopy.JE_EntryAuthorisationDate);

			var cusEntryNum2 = declarationCopy.LoadCusEntryNumber(false);
			AssertEquals(testDate, cusEntryNum2.CE_IssueDate);
			AssertEquals(CusEntryNumberTypes.Standard.MovementReferenceNumber, cusEntryNum2.CE_EntryType);
		}

		public void TestJourneyTime()
		{
			var declaration = Factory.New<EMCSJobDeclarationForTest>();

			AssertEquals(1, declaration.JourneyTimeNumericPart);
			AssertEquals(JourneyTimeUnitList.Codes.Hours, declaration.JourneyTimeFormatPart);
			AssertEquals("", declaration.JourneyTimeValue);

			declaration.JourneyTimeNumericPart = 4;
			AssertEquals(" 4H", declaration.JourneyTimeValue);

			declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
			AssertEquals(" 4D", declaration.JourneyTimeValue);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationCopy = otherFactory.Load<EMCSJobDeclaration>(declaration.PK);
			AssertEquals(4, declarationCopy.JourneyTimeNumericPart);
			AssertEquals(JourneyTimeUnitList.Codes.Days, declarationCopy.JourneyTimeFormatPart);
		}

		public void TestInvoiceNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SUPPLIER";

			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.InvoiceNumber = "AA123456";

			AssertEquals("AA123456", declaration.InvoiceNumber);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals("Expecting Supplier to be the Consignor when Goods Owner is not set.", orgHeader.PK, declaration.Invoices[0].JZ_OH_Supplier);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationCopy = otherFactory.Load<EMCSJobDeclaration>(declaration.PK);
			AssertEquals("AA123456", declarationCopy.InvoiceNumber);
			AssertEquals(1, declarationCopy.Invoices.Count);
			AssertEquals(orgHeader.PK, declarationCopy.Invoices[0].JZ_OH_Supplier);

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_OH_Supplier = orgHeader.PK;
			declaration2.InvoiceNumber = "BB98765";

			var goodsOwner = Factory.New<OrgHeader>();
			goodsOwner.OH_Code = "OWNER";
			var goodsOwnerAddress = goodsOwner.Addresses.AddNew();
			goodsOwnerAddress.OA_Address1 = "ABC";

			var goodsOwnerDocAddress = declaration2.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
			goodsOwnerDocAddress.E2_OA_Address = goodsOwnerAddress.PK;

			AssertEquals("BB98765", declaration2.InvoiceNumber);
			AssertEquals(1, declaration2.Invoices.Count);
			AssertEquals("Expecting Supplier to be the Goods Owner when set.", goodsOwner.PK, declaration2.Invoices[0].JZ_OH_Supplier);

			Factory.Save();

			var declaration2Copy = otherFactory.Load<EMCSJobDeclaration>(declaration2.PK);
			AssertEquals("BB98765", declaration2Copy.InvoiceNumber);
			AssertEquals(1, declaration2Copy.Invoices.Count);
			AssertEquals(goodsOwner.PK, declaration2Copy.Invoices[0].JZ_OH_Supplier);
		}

		public void TestInvoiceDate()
		{
			var testDate = new ZDateTime(DateTime.Now.Date, DateTimeKind.Local);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SUPPLIER";

			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.InvoiceDate = testDate;

			AssertEquals(testDate, declaration.InvoiceDate);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals("Expecting Supplier to be the Consignor when Goods Owner is not set.", orgHeader.PK, declaration.Invoices[0].JZ_OH_Supplier);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationCopy = otherFactory.Load<EMCSJobDeclaration>(declaration.PK);
			AssertEquals(testDate, declarationCopy.InvoiceDate);
			AssertEquals(1, declarationCopy.Invoices.Count);
			AssertEquals(orgHeader.PK, declarationCopy.Invoices[0].JZ_OH_Supplier);

			var testDate2 = testDate.AddDays(1);

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_OH_Supplier = orgHeader.PK;
			declaration2.InvoiceDate = testDate2;

			var goodsOwner = Factory.New<OrgHeader>();
			goodsOwner.OH_Code = "OWNER";
			var goodsOwnerAddress = goodsOwner.Addresses.AddNew();
			goodsOwnerAddress.OA_Address1 = "ABC";

			var goodsOwnerDocAddress = declaration2.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
			goodsOwnerDocAddress.E2_OA_Address = goodsOwnerAddress.PK;

			AssertEquals(testDate2, declaration2.InvoiceDate);
			AssertEquals(1, declaration2.Invoices.Count);
			AssertEquals("Expecting Supplier to be the Goods Owner when set.", goodsOwner.PK, declaration2.Invoices[0].JZ_OH_Supplier);

			Factory.Save();

			var declaration2Copy = otherFactory.Load<EMCSJobDeclaration>(declaration2.PK);
			AssertEquals(testDate2, declaration2Copy.InvoiceDate);
			AssertEquals(1, declaration2Copy.Invoices.Count);
			AssertEquals(goodsOwner.PK, declaration2Copy.Invoices[0].JZ_OH_Supplier);
		}

		public void TestSpecialInstructions()
		{
			declaration.SpecialInstructions = "Some extra instruction";
			AssertEquals("Some extra instruction", declaration.SpecialInstructions);
		}

		public void TestSpecialInstructions_Caption()
		{
			AssertEquals("Complementary Information (Transport Mode)", DataBoundResourceStrings.GetDataForProperty(declaration.SpecialInstructionsInfo).Caption);
		}

		public void TestJE_OH_SupplierChanged_DefaultSupplierDocAddresses()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "Org";
			org.OH_FullName = "OrgHeader";
			org.MainAddress.OA_Address1 = "Main address 1";
			org.MainAddress.OA_Address2 = "Main address 2";
			org.MainAddress.OA_City = "Main City";
			declaration.JE_OH_Supplier = org.PK;

			AssertEquals("Default DocAddresses should be set on JE_OH_SupplierChanged", org.MainAddress.PK, declaration.SupplierDocumentaryAddress.Address.PK);
		}

		public void TestJE_OwnerRef()
		{
			AssertEquals(22, declaration.JE_OwnerRefInfo.MaxLength);
		}

		public void TestGetFetchStrategies()
		{
			var expectedTypes = new[]
			{
				typeof(CusSupportingInfoTypeSupporterFetchStrategy),
				typeof(CusCodeDataTypeSupporterFetchStrategy)
			};
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetFetchStrategies().Select(c => c.GetType());

			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestIControllerIDProvider()
		{
			var provider = (IControllerIDProvider)declaration;
			CombineAssertions(() =>
			{
				AssertEquals("ControllerID", ControllerIDs.Customs.EU.EMCS, provider.ControllerID);
				AssertEquals("BusinessObjectPK", declaration.PK, provider.BusinessObjectPK);
			});
		}

		public void TestZG_CertOfExemption_MaxLength()
		{
			AssertEquals(255, declaration.ZG_CertOfExemptionInfo.MaxLength);
		}

		public void TestJobDocAddressRequirement()
		{
			var docAddresses = declaration as IDocAddresses;

			CombineAssertions(() =>
			{
				AssertNotNull("ImporterDocumentaryAddress", docAddresses.GetDocAddressRequirement(DocAddressType.ImporterDocumentaryAddress));
				AssertNotNull("SupplierDocumentaryAddress", docAddresses.GetDocAddressRequirement(DocAddressType.SupplierDocumentaryAddress));
				AssertNotNull("GoodsOwner", docAddresses.GetDocAddressRequirement(DocAddressType.GoodsOwner));
				AssertNotNull("CarrierAgent", docAddresses.GetDocAddressRequirement(DocAddressType.CarrierAgent));
				AssertNotNull("Transporter", docAddresses.GetDocAddressRequirement(DocAddressType.Transporter));
				AssertNotNull("DispatchWarehouse", docAddresses.GetDocAddressRequirement(DocAddressType.DispatchWarehouse));
				AssertNotNull("DestinationWarehouse", docAddresses.GetDocAddressRequirement(DocAddressType.DestinationWarehouse));
			});
		}

		public void TestJE_EntryStatus_Caption()
		{
			AssertEquals("Registration Status", DataBoundResourceStrings.GetDataForProperty(declaration.JE_EntryStatusInfo).Caption);
		}

		public void TestZG_ExplanationOnReasonForShortageValidation()
		{
			AssertEquals("Default", false, declaration.ZG_ExplanationOnReasonForShortageValidation);
		}

		public void TestPackages()
		{
			CombineAssertions(() =>
			{
				var packages = declaration.EMCSPackages;
				AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(packages));
				AssertSame("Cached", packages, declaration.EMCSPackages);
			});
		}

		public void TestAccessingPropertiesOfDeletedCusEntryNumber()
		{
			var cusEntryNumber = declaration.LoadCusEntryNumber(true);
			cusEntryNumber.Delete();
			CombineAssertions(() =>
			{
				AssertEquals("Accessing EADNumber after cusEntryNumber is deleted", ZString.Empty, declaration.EADNumber);
				AssertEquals("Accessing JE_EntryAuthorisationDate after cusEntryNumber is deleted", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			});
		}

		public void TestLoadCusEntryNumber_JustLoad()
		{
			CombineAssertions(() =>
			{
				AssertNull("No CusEntryNumber", declaration.LoadCusEntryNumber(false));

				var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
				AssertSame("Has CusEntryNumber", cusEntryNumber, declaration.LoadCusEntryNumber(false));

				cusEntryNumber.Delete();
				AssertNull("Return null when current CusEntryNumber is deleted", declaration.LoadCusEntryNumber(false));
			});
		}

		public void TestLoadCusEntryNumber_LoadOrCreate()
		{
			var cusEntryNumber = declaration.LoadCusEntryNumber(true);
			CombineAssertions(() =>
			{
				AssertNotNull("CusEntryNumber created", cusEntryNumber);
				AssertSame("Same CusEntryNumber returned", cusEntryNumber, declaration.LoadCusEntryNumber(true));
			});
		}

		public void TestLoadCusEntryNumber_LoadOrCreate_CurrentIsDeleted()
		{
			CombineAssertions(() =>
			{
				var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
				AssertSame("Same CusEntryNumber returned", cusEntryNumber, declaration.LoadCusEntryNumber(true));

				cusEntryNumber.Delete();
				var cusEntryNumber2 = declaration.LoadCusEntryNumber(true);
				AssertNotEquals("New CusEntryNumber is created", cusEntryNumber.PK, cusEntryNumber2.PK);
				AssertEquals("New CusEntryNumber is not deleted", false, cusEntryNumber2.IsDeleted);
			});
		}

		public void TestJobNumberDefaultPrefix()
		{
			Factory.Save();
			AssertEquals("Job number's prefix should be 'E' by default", true, System.Text.RegularExpressions.Regex.IsMatch(declaration.JE_DeclarationReference, @"^E[0-9]{8}$"));
		}

		public void TestDocumentSupporter()
		{
			Assertion.AssertType(ExpectedDocumentSupporterType, declaration.DocumentSupporter);
		}

		protected virtual Type ExpectedDocumentSupporterType => typeof(EMCSJobDeclarationDocumentSupporter);

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.BaseJobDeclaration);
		
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;

		class EMCSJobDeclarationForTest : EMCSJobDeclaration
		{
			public EMCSJobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString JourneyTimeValue => ZG_JourneyTime;

			public new EuOfficeCodeCollection GetCustomsOffices() => base.GetCustomsOffices();

			public new CusEntryNumber LoadCusEntryNumber(bool create) => base.LoadCusEntryNumber(create);

			public override void PopulateJE_DeclarationReferenceIfNeeded()
			{
				JE_DeclarationReference = Env.NumberFountains.CustomsJobNoByExternalAgent.GetNextFormatted(Factory).ToUpper();
			}
		}
	}
}
