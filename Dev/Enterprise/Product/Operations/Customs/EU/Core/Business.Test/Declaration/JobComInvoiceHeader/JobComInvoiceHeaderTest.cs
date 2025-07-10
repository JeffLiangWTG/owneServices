using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	#region JobComInvoiceHeaderTest

	public abstract class JobComInvoiceHeaderTest<TJobDeclaration, TInvoiceHeader, TInvoiceLine> : BaseJobComInvoiceHeaderTest<TJobDeclaration, TInvoiceHeader, TInvoiceLine>
		where TJobDeclaration : JobDeclaration
		where TInvoiceHeader : JobComInvoiceHeader
		where TInvoiceLine : JobComInvoiceLine
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var header = Factory.New<TInvoiceHeader>();

			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(header, "EUJobComInvoiceHeader");
		}

		public virtual void TestDefaultCountryOfSupply()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("Pre-Req: DefaultCountryOfSupplyFromSupplier and DefaultCountryOfSupplyFromInvoiceHeader are false for this test to correctly expect no defaulting behavior", false, declaration.Configuration.InvoiceLineConfiguration.DefaultCountryOfSupplyFromSupplier(declaration));

			var invoice = declaration.Invoices.AddNew();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("DefaultCountryOfSupply should return an empty string when its declaration has its supplier country defined, because DefaultCountryOfSupplyFromSupplier is deactivated in EU solution", ZString.Empty, invoice.DefaultCountryOfSupply);

			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertEquals("DefaultCountryOfSupply should return an empty string when the invoice has its supplier country defined, because DefaultCountryOfSupplyFromInvoiceHeader is deactivated in EU solution", ZString.Empty, invoice.DefaultCountryOfSupply);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();

			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertSame(chargeTypeList1, chargeTypeList2);

			var customsChargeTypeList = GetExpectedCustomsChargeTypeList();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		protected virtual CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Common.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.AddPair(ChargeTypeList.Codes.InternationalFreight, ChargeTypeList.Descriptions.InternationalFreight);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			var dec = GetNewDeclaration();
			dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = OverseasFreightCode;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			var adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = OverseasFreightCode;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		protected virtual string OverseasFreightCode => ChargeTypeList.Codes.InternationalFreight;

		public void TestHeaderDescriptions()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.HeaderDescriptions.RemoveAndDeleteAll();

			AssertEquals(GetExpectedDescriptionsType(), invoice.HeaderDescriptions.GetType());
			AssertEquals(0, invoice.HeaderDescriptions.Count);

			var newDescription = invoice.HeaderDescriptions.AddNew();
			newDescription.FillWithValidTestData();

			Factory.Save();

			var invoiceInOtherFactory = NewFactory().Load<TInvoiceHeader>(invoice.PK);
			var expectedCount = invoice.IsSupportHeaderDescription ? 1 : 0;
			AssertEquals("Should load data depend on IsSupportHeaderDescription.", expectedCount, invoiceInOtherFactory.HeaderDescriptions.Count);
		}

		protected virtual Type GetExpectedDescriptionsType()
		{
			return typeof(InvoiceHeaderDescriptionCollection);
		}

		public void TestIsSupportHeaderDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var expectedValue = GetIsSupportHeaderDescription();

			AssertEquals(expectedValue, invoice.IsSupportHeaderDescription);
		}

		protected virtual bool GetIsSupportHeaderDescription()
		{
			return false;
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			ICusCodeDataTypeSupporter supporter = invoice;
			AssertEquals(GetExpectedInvoiceHeaderDescriptionType(), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.DescriptionCode]);
		}

		protected virtual Type GetExpectedInvoiceHeaderDescriptionType() => typeof(InvoiceHeaderDescription);

		public void TestDefaultSupportingDocumentIfNecessary()
		{
			CombineAssertions(() =>
			{
				foreach (var defaultInvoiceDocument in DefaultInvoiceDocuments)
				{
					var messageType = defaultInvoiceDocument.messageType;
					var defaultInvoiceDocumentCode = defaultInvoiceDocument.document;
					var declaration = Factory.New<JobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
					declaration.JE_MessageType = messageType;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					var supplier = Factory.NewWithValidTestData<OrgHeader>();
					var supplierAddress = supplier.MainAddress;

					void assertHasDefaultDocumentIfAddingSupportingDocumentAutomaticallyIsEnabled(string message)
					{
						if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
						{
							AssertEquals($"{messageType}, {message} -> Supporting Documents count", 1, invoice.SupportingDocuments.Count);
							var defaultSupportingDocument = invoice.SupportingDocuments.Cast<SupportingDocument>()
								.SingleOrDefault(x => x.CSI_Code == defaultInvoiceDocumentCode);
							AssertNotNull($"{messageType}, {message} -> {defaultInvoiceDocumentCode} automatically created", defaultSupportingDocument);
						}
						else
						{
							AssertEquals($"{messageType}, Supporting Documents count", 0, invoice.SupportingDocuments.Count);
						}
					}

					invoice.JZ_OH_Supplier = supplier.PK;
					declaration.JE_OH_Supplier = supplier.PK;
					supplierAddress.OA_RN_NKCountryCode = "IT";
					invoice.JZ_InvoiceNumber = "1";
					invoice.JZ_InvoiceDate = ZDateTime.Today;
					assertHasDefaultDocumentIfAddingSupportingDocumentAutomaticallyIsEnabled("Setting InvoiceDate");

					invoice.SupportingDocuments.RemoveAndDeleteAll();
					invoice.JZ_OH_Supplier = ZGuid.Empty;
					AssertEquals($"{messageType}, Supplier at Declaration, clearing supplier for Invoice", 0, invoice.SupportingDocuments.Count);
					invoice.JZ_InvoiceNumber = "2";
					assertHasDefaultDocumentIfAddingSupportingDocumentAutomaticallyIsEnabled("Setting InvoiceNumber, JZ_OH_Supplier empty but JE_OH_Supplier not empty");

					invoice.SupportingDocuments.RemoveAndDeleteAll();
					declaration.JE_OH_Supplier = ZGuid.Empty;
					invoice.JZ_InvoiceNumber = "3";
					AssertEquals($"{messageType}, No supplier", 0, invoice.SupportingDocuments.Count);

					invoice.JZ_OH_Supplier = supplier.PK;
					assertHasDefaultDocumentIfAddingSupportingDocumentAutomaticallyIsEnabled("Setting supplier");

					invoice.SupportingDocuments.RemoveAndDeleteAll();
					invoice.JZ_InvoiceDate = ZDateTime.Empty;
					invoice.JZ_InvoiceNumber = "4";
					AssertEquals($"{messageType}, No InvoiceDate", 0, invoice.SupportingDocuments.Count);

					invoice.JZ_InvoiceDate = ZDateTime.Today;
					invoice.SupportingDocuments.RemoveAndDeleteAll();
					invoice.JZ_InvoiceNumber = ZString.Empty;
					AssertEquals($"{messageType}, No InvoiceNumber", 0, invoice.SupportingDocuments.Count);

					AssertDefaultDocumentWhenApplicationCodeChanges(declaration, invoice, messageType, defaultInvoiceDocumentCode);
				}
			});
		}

		protected virtual void AssertDefaultDocumentWhenApplicationCodeChanges(JobDeclaration declaration, JobComInvoiceHeader invoice, string messageType, string defaultInvoiceDocumentCode)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			invoice.JZ_InvoiceNumber = "6";
			AssertEquals($"{messageType}, Do not add {defaultInvoiceDocumentCode} document if application code is not BLT.", 0, invoice.SupportingDocuments.Count);
		}

		public virtual void TestUpdateJZ_InvoiceNumber_DefaultDocument()
		{
			CombineAssertions(() =>
			{
				foreach (var defaultInvoiceDocument in DefaultInvoiceDocuments)
				{
					var messageType = defaultInvoiceDocument.messageType;
					var defaultInvoiceDocumentCode = defaultInvoiceDocument.document;
					var declaration = GetNewDeclarationForTesting();
					declaration.JE_MessageType = messageType;
					var invoice = declaration.Invoices.AddNew();
					var doc1 = invoice.SupportingDocuments.AddNew();
					doc1.CSI_Code = defaultInvoiceDocumentCode;
					var doc2 = invoice.SupportingDocuments.AddNew();
					doc2.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.Certificate;

					invoice.JZ_InvoiceNumber = "1";

					AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled($"CSI_ReferenceNumber of {defaultInvoiceDocumentCode}");
					AssertEquals($"{messageType}, CSI_ReferenceNumber of not {defaultInvoiceDocumentCode}", ZString.Empty, doc2.CSI_ReferenceNumber);

					invoice.JZ_InvoiceNumber = ZString.Empty;
					AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled("InvoiceNumber empty");

					void AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled(string message)
					{
						if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
						{
							AssertEquals($"{messageType}, {message} -> AddingSupportingDocumentAutomatically enabled", "1", doc1.CSI_ReferenceNumber);
						}
						else
						{
							AssertEquals($"{messageType}, {message} -> AddingSupportingDocumentAutomatically disabled", ZString.Empty, doc1.CSI_ReferenceNumber);
						}
					}
				}
			});
		}

		public void TestUpdateJZ_InvoiceDate_DefaultDocument()
		{
			CombineAssertions(() =>
			{
				foreach (var defaultInvoiceDocument in DefaultInvoiceDocuments)
				{
					var messageType = defaultInvoiceDocument.messageType;
					var defaultInvoiceDocumentCode = defaultInvoiceDocument.document;
					var declaration = GetNewDeclarationForTesting();
					declaration.JE_MessageType = messageType;
					var invoice = declaration.Invoices.AddNew();
					var doc1 = invoice.SupportingDocuments.AddNew();
					doc1.CSI_Code = defaultInvoiceDocumentCode;
					var doc2 = invoice.SupportingDocuments.AddNew();
					doc2.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.Certificate;

					invoice.JZ_InvoiceDate = new ZDateTime(2020, 1, 1);

					AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled($"CSI_DateOfIssue of {defaultInvoiceDocumentCode}");
					AssertEquals($"{messageType}, CSI_DateOfIssue of not {defaultInvoiceDocumentCode}", ZDateTime.Empty, doc2.CSI_DateOfIssue);

					invoice.JZ_InvoiceDate = ZDateTime.Empty;
					AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled("InvoiceDate empty");

					void AssertDefaultDocumentUpdatedIfAddingSupportingDocumentAutomaticallyIsEnabled(string message)
					{
						if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
						{
							AssertEquals($"{messageType}, {message} -> AddingSupportingDocumentAutomatically enabled", new ZDateTime(2020, 1, 1), doc1.CSI_DateOfIssue);
						}
						else
						{
							AssertEquals($"{messageType}, {message} -> AddingSupportingDocumentAutomatically disabled", ZDateTime.Empty, doc1.CSI_DateOfIssue);
						}
					}
				}
			});
		}

		public void TestIPreviousDocumentsProviderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.SupportingDocuments.AddNew();

			var previousDocumentsProvider = (IPreviousDocumentsProvider)invoiceHeader;

			AssertNotNull("IPreviousDocumentsProvider.PreviousDocuments must be not null", previousDocumentsProvider.PreviousDocuments);
			AssertSame("IPreviousDocumentsProvider.PreviousDocuments must be the same of PreviousDocuments", invoiceHeader.PreviousDocuments, previousDocumentsProvider.PreviousDocuments);
		}

		public void TestJZ_AdditionalTermsInfo_Caption()
		{
			AssertCaption(Factory.New<JobComInvoiceHeader>().JZ_AdditionalTermsInfo, "Delivery", "Delivery Text");
		}

		public void TestCaptionOfPaymentProperties()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			CombineAssertions("Payment Properties Caption", () =>
			{
				AssertCaption(invoiceHeader.ZG_CommercialPaymentCodeInfo, "Payment Code", "Payment Code");
				AssertCaption(invoiceHeader.JZ_PaymentAmountInfo, "Amount", "Amount");
				AssertCaption(invoiceHeader.JZ_PaymentNoInfo, "Payment Reference", "Payment Reference");
			});
		}

		public virtual void TestZGFieldsCaption()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			CombineAssertions("Payment Properties Caption", () =>
			{
				AssertCaption(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, "MoP", "Transport Charges Method of Payment");
				AssertCaption(invoiceHeader.ZG_AgreedPlaceCodeInfo, "Incoterm Place", "Incoterm Place Code");
			});
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}

		public void TestJZ_PaymentNoMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("JZ_PaymentNoMaxLength", 20, invoiceHeader.JZ_PaymentNoInfo.MaxLength);
		}

		public void TestIncoTermsAgreedPlace()
		{
			CombineAssertions("Inco terms StmNote field", () =>
			{
				var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();

				invoiceHeader.IncoTermsAgreedPlace = "Agreed place of inco terms";
				AssertEquals("Agreed place of inco terms", invoiceHeader.IncoTermsAgreedPlace);

				Factory.Save();

				var incoTermsAgreedPlace = invoiceHeader.Notes;
				AssertEquals("Length", 1, incoTermsAgreedPlace.DatabaseCount);

				var note = incoTermsAgreedPlace.FindByDescription(PredefinedNoteTypes.Instance.IncoTermsAgreedPlace.Description)[0];
				AssertEquals("Agreed place of inco terms", note.ST_NoteText);

				var otherFactory = new BusinessObjectFactory();
				var invoiceHeaderCopy = otherFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);
				AssertEquals("Agreed place of inco terms", invoiceHeaderCopy.IncoTermsAgreedPlace);

				invoiceHeaderCopy.IncoTermsAgreedPlace = "Changed the text";
				otherFactory.Save();

				var thirdFactory = new BusinessObjectFactory();
				var invoiceHeaderCopy2 = thirdFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);
				AssertEquals("Changed the text", invoiceHeaderCopy2.IncoTermsAgreedPlace);
			});
		}

		public void TestCaptionJZ_OA_SupplierAddress()
		{
			var property = DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.JZ_OA_SupplierAddress));
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supplier Address", property.Caption);
				AssertEquals("MediumCaption", "Supplier Addr.", property.MediumCaption);
				AssertEquals("ShortCaption", "Supp. Addr.", property.ShortCaption);
				AssertEquals("FullDescription", "This address determines which CID code is used in the message for each invoice", property.FullDescription);
			});
		}

		public void TestChargesAddDeductTotal()
		{
			Assert("Will be implemented by WI's WI00831078 WI00837660 WI00838181 WI00838254 WI00838273 WI00838919 WI00838938 WI00838971 WI00839025 WI00839039 WI00839057", true);

			var invoiceGroupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals("Caption", "Add/Deduct Stat. Value", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.ChargesAddDeductTotal)).Caption);
		}

		public override string GetLocalCurrencyCode() => Constants.CurrencyCodes.EuropeanUnion;

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO) => base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is EuOfficeCode;

		protected virtual IReadOnlyList<(string messageType, string document)> DefaultInvoiceDocuments =>
			new[]
			{
				(EUJobMessageTypeList.Codes.Import, UniversalReferenceConstants.SupportingDocumentTypes.N380)
			};

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

		protected virtual JobDeclaration GetNewDeclarationForTesting() => Factory.New<JobDeclaration>();
	}
	#endregion
}
