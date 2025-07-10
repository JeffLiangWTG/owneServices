using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclaration))]
	sealed class EMCSJobDeclarationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationTest
	{
		public void TestSequenceNumber_Get()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", ZString.Empty, declaration.SequenceNumber);
				AssertNull("No Entry Number created", CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom));
				var eadNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				eadNumber.CE_EntryLineReference = "234";
				AssertEquals("Ead Number Entry line Reference", "234", declaration.SequenceNumber);
			});
		}

		public void TestSequenceNumber_Set()
		{
			declaration.SequenceNumber = "498";
			var eadNumber = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("498", eadNumber.CE_EntryLineReference);
		}

		public void TestCertificateIdentifierReadOnly()
		{
			declaration.JE_CustomsProfile = "LONDON WHAREHOUSE";
			declaration.JE_CustomsProfile = "SOUTHAMPTON WHAREHOUSE";
			Assert("Certificate editable", !declaration.JE_CustomsProfileInfo.ReadOnly);

			declaration.Messages.AddNew();
			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Assert("Certificate ReadOnly", declaration.JE_CustomsProfileInfo.ReadOnly);

			declaration.JE_CustomsProfile = "";
			Assert("Certificate should be editable when the field has not been entered yet, even when there are messages", !declaration.JE_CustomsProfileInfo.ReadOnly);
		}

		public void TestCustomsProfileDefaultFromCredentials()
		{
			var exciseId = "EM1";
			TestHelper.CreateEMCSCredential(Factory, GlbCompany.CurrentCompany.PK, exciseId, "123456789", PasswordTypesList.Codes.CDS);
			Factory.ClearCachedValue<CodeDescriptionPairList>($"EMCSCredentialCollection-{GlbCompany.CurrentCompany.PK.ToGuid()}");
			AssertEquals("Credentials should have 1 item", 1, declaration.Lookups.Credentials.Count);
			declaration = Factory.New<EMCSJobDeclarationForTest>();
			AssertEquals("JE_CustomsProfile should have defaulted when there is only 1 credential", "EDIDAT.123456789.EM1", declaration.JE_CustomsProfile);

			TestHelper.CreateEMCSCredential(Factory, GlbCompany.CurrentCompany.PK, "EM2", "123456789", PasswordTypesList.Codes.CDS);
			Factory.ClearCachedValue<CodeDescriptionPairList>($"EMCSCredentialCollection-{GlbCompany.CurrentCompany.PK.ToGuid()}");
			AssertEquals("Credentials should have 2 items", 2, declaration.Lookups.Credentials.Count);
			declaration = Factory.New<EMCSJobDeclarationForTest>();
			AssertEquals("JE_CustomsProfile should be empty when there is more than 1 credential", "", declaration.JE_CustomsProfile);
		}

		public void TestJobDeclarationMessageCollectionApplicationCodeList()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				ApplicationCodeList.Codes.GbCustomsEMCS,
				ApplicationCodeList.Codes.UniversalDataMessaging
			}
			, declaration.JobDeclarationMessageCollectionApplicationCodeListExposed);
		}
		
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclarationForTest>();
		}

		class EMCSJobDeclarationForTest : EMCSJobDeclaration
		{
			public EMCSJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListExposed
			{
				get { return JobDeclarationMessageCollectionApplicationCodeListCore; }
			}
		}

		EMCSJobDeclarationForTest declaration;
		protected override BusinessObject GetNewBusinessObject() => declaration;
	}
}
