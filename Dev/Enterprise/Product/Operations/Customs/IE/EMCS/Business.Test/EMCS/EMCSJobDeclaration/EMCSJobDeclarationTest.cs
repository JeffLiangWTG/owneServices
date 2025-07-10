using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclaration))]
	public class EMCSJobDeclarationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationTest
	{
		public void TestMessageSendingConfiguration()
		{
			AssertType<EMCSJobDeclarationMessageSendingConfiguration>(declaration.MessageSendingConfiguration);
		}

		public void TestGetCusCodeDataType()
		{
			AssertEquals(typeof(OfficeCode), ((Integration.Customs.ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestGetCustomsOffices()
		{
			var customsOffices = declaration.CustomsOffices;
			CombineAssertions(() =>
			{
				AssertType<EU.EMCS.Business.OfficeCodeCollection<OfficeCode>>("OfficeCodeCollection type", customsOffices);
			});
		}

		public void TestSequenceNumber_Get()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", ZString.Empty, declaration.SequenceNumber);
				AssertNull("No Entry Number created", CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland));
				var eadNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
				eadNumber.CE_EntryLineReference = "234";
				AssertEquals("Ead Number Entry line Reference", "234", declaration.SequenceNumber);
			});
		}

		public void TestSequenceNumber_Set()
		{
			declaration.SequenceNumber = "498";
			var eadNumber = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			AssertEquals("498", eadNumber.CE_EntryLineReference);
		}

		public void TestIMessageAttacheeMembers()
		{
			var branch = declaration.Company.Branches.AddNew();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageStatus = LogicalStatusList.Codes.Accepted;
			declaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REG;
			declaration.EADNumber = "MRN1234";
			var cusAgent = Factory.New<GlbStaff>();
			cusAgent.GS_Code = "!2#";
			declaration.JE_GS_NKCusAgent = "!2#";
			IMessageAttachee messageAttachee = declaration;
			CombineAssertions(() =>
			{
				AssertEquals("Branch", branch, messageAttachee.Branch);
				AssertEquals("CustomsAgent", cusAgent, messageAttachee.CustomsAgent);
				AssertEquals("RelatedJob", declaration, messageAttachee.RelatedJob);
				AssertEquals("LogicalStatus", LogicalStatusList.Codes.Accepted, messageAttachee.LogicalStatus);
				AssertEquals("EntryStatus", EU.EMCS.Business.EntryStatusList.Codes.REG, messageAttachee.EntryStatus);
				AssertEquals("MovementReferenceNumber", "MRN1234", messageAttachee.MovementReferenceNumber);
			});
		}

		public void TestCertificateIdentifierDefaultsWhenOnly1Exists()
		{
			certificateIdentifier2.Delete();
			Factory.Save();

			var emcsTestDeclaration = Factory.New<EMCSJobDeclaration>();
			var filter = emcsTestDeclaration.Lookups.CertificateIdentifierList.CompleteFilter;
			Assert("Pre-conditon", certificateIdentifier1.MatchesFilter(filter));
			AssertEquals("JE_CustomsProfile - CertificateIdentifier should have defaulted as only 1 exists for Company", "DUBLIN WAREHOUSE 1", emcsTestDeclaration.JE_CustomsProfile);
		}

		public void TestCertificateIdentifierDoesNotDefaultForMultiples()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>("IE.EMCSJobDeclarationLookups.CertificateIdentifierList");
			AssertEquals("Pre-conditon", 2, declaration.Lookups.CertificateIdentifierList.Count);
			AssertEquals("JE_CustomsProfile - CertificateIdentifier should not default as there are multiple identifiers", "", declaration.JE_CustomsProfile);
		}

		public void TestCertificateIdentifierReadOnly()
		{
			declaration.JE_CustomsProfile = "DUBLIN WHAREHOUSE";
			declaration.JE_CustomsProfile = "KILKENNY WHAREHOUSE";
			Assert("Certificate Identifier editable", !declaration.JE_CustomsProfileInfo.ReadOnly);

			declaration.Messages.AddNew();
			declaration.JE_MessageStatus = "SNT";
			Assert("Certificate Identifier ReadOnly", declaration.JE_CustomsProfileInfo.ReadOnly);

			declaration.JE_CustomsProfile = "";
			Assert("Certificate Identifier should be editable when the field has not been entered yet, even when there are messages", !declaration.JE_CustomsProfileInfo.ReadOnly);
		}

		public void TestEMCSPackageType()
		{
			var package = declaration.EMCSPackages.AddNew();
			AssertType<EMCSPackage>(package);
		}

		public void TestDocumentsType()
		{
			var document = declaration.Documents.AddNew();
			AssertType<EMCSDocument>(document);
		}

		protected override Type ExpectedDocumentSupporterType => typeof(EMCSJobDeclarationDocumentSupporter);

		protected override void SetUp()
		{
			certificateIdentifier1 = Factory.New<EMCSGlbCompanyCredential>();
			certificateIdentifier1.GP_MailBoxID = "DUBLIN WAREHOUSE 1";
			certificateIdentifier1.GP_CertificateSerialNumber = "A0492387J";
			certificateIdentifier1.GP_GC = GlbCompany.CurrentCompany.PK;

			certificateIdentifier2 = Factory.New<EMCSGlbCompanyCredential>();
			certificateIdentifier2.GP_MailBoxID = "KILKENNY WAREHOUSE";
			certificateIdentifier2.GP_CertificateSerialNumber = "754927U38TZ";
			certificateIdentifier2.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;

		EMCSGlbCompanyCredential certificateIdentifier1;
		EMCSGlbCompanyCredential certificateIdentifier2;

		protected override BusinessObject GetNewBusinessObject() => declaration;
	}
}
