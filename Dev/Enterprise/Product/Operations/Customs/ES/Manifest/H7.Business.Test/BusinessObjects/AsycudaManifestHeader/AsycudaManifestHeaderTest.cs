using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : EU.H7.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestAMA_RN_NKCountry()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals("Default value", "ES", header.AMA_RN_NKCountry);
		}

		public void TestAMA_MasterInformation()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertResourceStringData(header.AMA_MasterInformationInfo, "DSDT MRN/Flight No.", "DSDT MRN/Flight No.", "DSDT MRN/Flight No.", "Flight number or Movement Reference Number of the associated Discharge Summary Declaration.");
		}

		public void TestAMA_AgentType()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			AssertResourceStringData(header.AMA_AgentTypeInfo, "Rep. Status", "Rep. Status", "Rep. Status", "The relevant code representing the status of the representative.");
		}

		public void TestTrainingEntry()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertResourceStringData(header.TrainingEntryInfo, "Training Entry", "Training Entry", "Training Entry", "When checked the declaration will be sent to Test.");
		}

		public void TestG3MRNToRevoke()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_ParentTable = "AsycudaManifestHeader";
			cusEntryNum.CE_ParentID = header.PK;
			cusEntryNum.CE_EntryType = "MRN";
			cusEntryNum.CE_EntryLineReference = "G3TOREVOKE";
			cusEntryNum.CE_RN_NKCountryCode = "ES";
			cusEntryNum.CE_EntryNum = "12345";

			AssertResourceStringData(header.G3MRNToRevokeInfo, "G3 MRN To Revoke", "G3 MRN Revoke", "G3 MRN Rev.", "MRN of the G3 Declaration linked to the H7 bills intending to revoke.");
			AssertEquals(cusEntryNum.CE_EntryNum, header.G3MRNToRevoke);
		}

		public void TestAMA_CustomsProfile()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertResourceStringData(header.AMA_CustomsProfileInfo, "Certificate", "Certif.", "Cert.", "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job.");
		}

		public void TestAMA_GS_NKCustomsAgent()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.AMA_GS_NKCustomsAgentInfo);
			AssertEquals("Caption", "Broker", captionResourceString.Caption);
		}

		public void TestTransportDocumentReferenceCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertResourceStringData(header.TransportDocumentReferenceInfo, "Transport Document Reference", "T. Doc. Reference", "T.Doc.Ref.", String.Empty);
		}

		public void TestTransportDocumentReferenceMaxLength()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var descriptor = header.TransportDocumentReferenceInfo.PropertyDescriptor;
			var maxLengthAttribute = descriptor.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;
			AssertEquals(70, maxLengthAttribute.MaxLength);
		}

		public void TestTransportDocumentReferenceDefaultValue()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals(string.Empty, header.TransportDocumentReference);
		}

		public void TestTransportDocumentTypeCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertResourceStringData(header.TransportDocumentTypeInfo, "Transport Document Type", "T. Doc. Type", "T.Doc.Type", String.Empty);
		}

		public void TestTransportDocumentTypeBindingList()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var descriptor = header.TransportDocumentTypeInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertEquals("Lookups.TransportDocumentTypes", listAttribute.ListDataSourceMember);
		}

		public void TestTransportDocumentTypeDefaultValue()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals(string.Empty, header.TransportDocumentType);
		}

		public void TestGetDefaultTrainingEntry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Training Entry Default", true, header.TrainingEntry);
		}

		public void TestEntryLineNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertResourceStringData(header.EntryLineNumberInfo, "Entry Line Number", "Ent. Line No.", "E. Line No.", "The Entry Line Number within the DSDT.");
			AssertEquals(5, header.EntryLineNumberInfo.MaxLength);

			header.EntryLineNumber = "12345";
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, header.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, GenAddOnColumnConstants.EntryLineNumberColumnName);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, AsycudaManifestHeaderSchema.Constants.Prefix);
			var entryLineNumber = Factory.LoadTop1<GenAddOnColumn>(query)?.XA_Data;
			AssertEquals("12345", entryLineNumber);
		}

		public void TestCusGoodsLocation()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var cusGoodsLocationProvider = header as ICusGoodsLocationProvider;

			AssertNotNull("AsycudaManifestHeader implements ICusGoodsLocationProvider", cusGoodsLocationProvider);

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", header.CusGoodsLocation);
			});
		}

		public void TestCusGoodsLocationDescription()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(header.GoodsLocationDescriptionInfo, multipleResourceKey: null, "Location of Goods (G3)", "Location of Goods (G3)", "Location of Goods (G3)", "Location where the goods may be examined. The location must be precise enough to allow Customs to carry out the physical control of the goods.");
		}

		public void TestCusGoodsLocationProviderKey()
		{
			var cusGoodsLocationProvider = GetNewBusinessObject() as ICusGoodsLocationProvider;

			AssertEquals("CusGoodsLocationProvider key", "ESH7D", cusGoodsLocationProvider.ProviderKey);
		}

		public void TestBroker()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var staffWithCertificateHelper = new StaffWithCertificateTestHelper(Factory);
			header.AMA_GS_NKCustomsAgent = staffWithCertificateHelper.Staff.GS_Code;

			AssertEquals(staffWithCertificateHelper.Staff.GS_Code, header.Broker.GS_Code);
		}

		#region IESResponseBOMessageStatus

		public void TestMessageStatus()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_MessageStatus = string.Empty;

			var responseBO = header as IESResponseBOMessageStatus;
			responseBO.MessageStatus = "ACC";
			AssertEquals("ACC", header.AMA_MessageStatus);
		}

		public void TestBranchPK()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var branch = Factory.New<GlbBranch>();
			header.AMA_GB = branch.PK;

			var responseBO = header as IESResponseBusinessObject;
			AssertEquals(branch.PK, responseBO.BranchPK);
		}

		public void TestMessageCollection()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.Messages.AddNew();
			header.Messages.AddNew();

			var responseBO = header as IESResponseBusinessObject;
			AssertEquals(2, responseBO.MessageCollection.Count);
		}

		public void TestEntryReference()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_JobReference = "H7D00000001";

			var messageBO = header as IESMessageBusinessObject;
			AssertEquals("H7D00000001", messageBO.EntryReference);
		}

		#endregion

		public void TestCloneManifestHeader()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Spain))
			{
				var header = SetupHeaderForClone();
				SetupBillForClone(header);
				Factory.Save();

				var clonedHeader = (AsycudaManifestHeader)header.Clone();

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("ES", clonedHeader.AMA_RN_NKCountry);
					AssertEquals(header.AMA_GB, clonedHeader.AMA_GB);
					AssertEquals("AIR", clonedHeader.AMA_TransportMode);
					AssertEquals(string.Empty, clonedHeader.AMA_Voyage);
					AssertEquals("GB2AB", clonedHeader.AMA_RL_NKPortOfLoading);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_E_DEP);
					AssertEquals("ADALV", clonedHeader.AMA_RL_NKPortOfFirstArrival);
					AssertEquals("IEORK", clonedHeader.AMA_RL_NKPortOfDischarge);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_E_ARV);
					AssertEquals(DateTime.MinValue, clonedHeader.AMA_A_ARV);
					AssertEquals(string.Empty, clonedHeader.MasterBill.ABL_BillNumber);
					AssertEquals(1, clonedHeader.Bills.Count);
					AssertEquals(header.AMA_OA_Declarant, clonedHeader.AMA_OA_Declarant);
					AssertEquals(header.AMA_OA_Representative, clonedHeader.AMA_OA_Representative);
					AssertEquals("DIR", clonedHeader.AMA_AgentType);
					AssertEquals("A", clonedHeader.AMA_PaymentMethod);
					AssertEquals("LV1", clonedHeader.AMA_ApplicationCode);
					AssertEquals("IEORK802", clonedHeader.AMA_CustomsOffice);
					AssertEquals("IEORK802", clonedHeader.PresentationOffice);
					AssertEquals("U", clonedHeader.CusGoodsLocation.CGL_Qualifier);
					AssertEquals("A", clonedHeader.CusGoodsLocation.CGL_Type);
					AssertEquals("123456", clonedHeader.CusGoodsLocation.Unlocode);
					AssertEquals("name", clonedHeader.CusGoodsLocation.Address.E2_Contact);
					AssertEquals("123456789", clonedHeader.CusGoodsLocation.Address.E2_Phone);
					AssertEquals("test@org.com", clonedHeader.CusGoodsLocation.Address.E2_Email);

					var clonedBill = clonedHeader.Bills.FirstOrDefault();
					AssertEquals(string.Empty, clonedBill.ABL_BillNumber);
					AssertEquals("bill des", clonedBill.ABL_GoodsDescription);
				});
			}
		}

		AsycudaManifestHeader SetupHeaderForClone()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brnach = Factory.NewWithValidTestData<GlbBranch>();
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_GB = brnach.PK;
			header.AMA_TransportMode = "AIR";
			header.AMA_Voyage = "QN006";
			header.AMA_RL_NKPortOfLoading = "GB2AB";
			header.AMA_E_DEP = DateTime.Now;
			header.AMA_RL_NKPortOfFirstArrival = "ADALV";
			header.AMA_RL_NKPortOfDischarge = "IEORK";
			header.AMA_E_ARV = DateTime.Now;
			header.AMA_A_ARV = DateTime.Now;
			header.MasterBill.ABL_BillNumber = "bill123";
			header.AMA_OA_Declarant = org.MainAddress.PK;
			header.AMA_OA_Representative = org.MainAddress.PK;
			header.AMA_AgentType = "DIR";
			header.AMA_PaymentMethod = "A";
			header.AMA_ApplicationCode = "LV1";
			header.AMA_CustomsOffice = "IEORK802";
			header.PresentationOffice = "IEORK802";

			var locationOfGoods = header.CusGoodsLocation;
			locationOfGoods.CGL_Qualifier = "U";
			locationOfGoods.CGL_Type = "A";
			locationOfGoods.Unlocode = "123456";

			var address = locationOfGoods.Address;
			address.E2_Contact = "name";
			address.E2_Phone = "123456789";
			address.E2_Email = "test@org.com";

			return header;
		}

		void SetupBillForClone(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "bill234";
			bill.ABL_GoodsDescription = "bill des";
		}

		public void TestSetCustomsProfileDefaultOnCustomsAgentWithOneCertificate()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TESTCERT1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff.PK;

			header.AMA_GS_NKCustomsAgent = "AH";

			AssertEquals("Default Single Certificate", "TESTCERT1", header.AMA_CustomsProfile);
		}

		public void TestSetCustomsProfileEmptyOnCustomsAgentWithMultipleCertificate()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TESTCERT1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff.PK;

			var cert2 = wrapper.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TESTCERT2";
			cert2.GP_MailBoxID = "Test";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff.PK;

			header.AMA_GS_NKCustomsAgent = "AH";

			AssertEquals("Default Empty Certificate", ZString.Empty, header.AMA_CustomsProfile);
		}

		public void TestOnSavingDefaultEmptyCustomAgentToCurrentUser()
		{
			var oldIsSystemAccountValue = GlbStaff.CurrentUser.GS_IsSystemAccount;
			try
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = false;
				var header = GetNewBusinessObject() as AsycudaManifestHeader;
				header.OnSaving();

				AssertEquals("Empty broker defaults to current user", GlbStaff.CurrentUser.GS_Code, header.AMA_GS_NKCustomsAgent);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = oldIsSystemAccountValue;
			}
		}

		public void TestLodgementCustomsOfficeInCanaryIsland()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsOffice = "ES0035111";

			Assert("Customs office is in the Canary Island", header.LodgementCustomsOfficeInCanaryIsland);
		}

		public void TestIsAgentTypeINDOrDCA()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			CombineAssertions(() =>
			{
				header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.IND;
				Assert("AgentType is IND", header.IsAgentTypeINDOrDCA);

				header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.DIR;
				Assert("AgentType is not IND or DCA", !header.IsAgentTypeINDOrDCA);

				header.AMA_AgentType = ESH7AgentTypes.Codes.DCA;
				Assert("AgentType is DCA", header.IsAgentTypeINDOrDCA);
			});
		}

		public void TestIsAgentTypeDIROrICA()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			CombineAssertions(() =>
			{
				header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.IND;
				Assert("When the AgentType is IND, it does not meet the requirements of the TriggerValidationAgentType.", !header.IsAgentTypeDIROrICA);

				header.AMA_AgentType = ESH7AgentTypes.Codes.SEL;
				Assert("When the AgentType is SEL, it does not meet the requirements of the TriggerValidationAgentType.", !header.IsAgentTypeDIROrICA);

				header.AMA_AgentType = ESH7AgentTypes.Codes.DCA;
				Assert("When the AgentType is DCA, it does not meet the requirements of the TriggerValidationAgentType.", !header.IsAgentTypeDIROrICA);

				header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.DIR;
				Assert("When the AgentType is DIR, it conforms to the TriggerValidationAgentType.", header.IsAgentTypeDIROrICA);

				header.AMA_AgentType = ESH7AgentTypes.Codes.ICA;
				Assert("When the AgentType is ICA, it conforms to the TriggerValidationAgentType.", header.IsAgentTypeDIROrICA);
			});
		}

		#region ILRNGenerator

		[TestDate(2025, 02, 10)]
		public void TestGenerateLocalReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
			(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);

			var lrnNumber = header.GenerateLocalReferenceNumber();

			CombineAssertions(() =>
			{
				AssertEquals("Length of Local Reference Number should be 22 characters", 22, lrnNumber.Length);
				AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the current year.", "25", lrnNumber);

				string partThatShouldStartWithEORI = lrnNumber.Substring(2);
				AssertStartsWith("Expected Local Reference Number, after the first 2 characters, to contain the EORI of the Declarant.", "1230789654", partThatShouldStartWithEORI);

				string partThatShouldBe10RandomDigits = lrnNumber.Substring(12);
				AssertEquals("Expected Local Reference Number, after the first 12 characters, sequence 0000000001", "0000000001", partThatShouldBe10RandomDigits);
				AssertEquals("LRN", "2512307896540000000001", lrnNumber);

				(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 10);
				AssertEquals("LRN when the next sequence number is 10", "2512307896540000000010", header.GenerateLocalReferenceNumber());
			});
		}

		#endregion

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();

		void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", caption, captionResourceString.Caption);
				AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
				AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

		AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory) => factory.NewWithValidTestData<AsycudaManifestHeader>();
	}
}
