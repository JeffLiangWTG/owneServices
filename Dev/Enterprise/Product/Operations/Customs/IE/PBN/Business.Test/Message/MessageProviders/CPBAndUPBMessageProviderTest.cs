using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(CPBAndUPBMessageProvider))]
	sealed class CPBAndUPBMessageProviderTest : DataProviderTestCase<CPBAndUPBMessageProvider>
	{
		public void TestICPBHeader()
		{
			Assert("Should implement ICPBHeader", Provider is ICPBAndUPBHeader);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("PBNMessageSendingObjectParent missing", () => new CPBAndUPBMessageProvider(null));
		}

		public void TestDirection()
		{
			pbn.AMA_Nature = "IMP";
			AssertEquals("Import", "IN_IRELAND", Provider.Direction);
			pbn.AMA_Nature = "EXP";
			AssertEquals("Export", "OUT_IRELAND", Provider.Direction);
		}

		public void TestEmptyVehicle()
		{
			pbn.IsEmptyVehicle = false;
			AssertEquals("Not Empty", false, Provider.EmptyVehicle);
			pbn.IsEmptyVehicle = true;
			Assert("Empty", Provider.EmptyVehicle);
		}

		public void TestDeclarations()
		{
			pbn.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS124782356";
			pbn.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS000000001";
			pbn.TransitDeclarationCollection.AddNew().CSI_ReferenceNumber = "IE09301248050000";

			var declarations = Provider.Declarations;
			AssertType<PBNDeclarationProvider[]>(declarations);
			AssertEquals("Should include declarations from CustomsReferenceCollection and TransitDeclarationCollection", 3, declarations.Count);
		}

		public void TestDeclarations_Blank()
		{
			pbn.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS124782356";
			pbn.CustomsReferenceCollection.AddNew();
			pbn.TransitDeclarationCollection.AddNew();

			var declarations = Provider.Declarations;
			AssertEquals("Only declarations with an MRN filled in should be included", 1, declarations.Count);
		}

		public void TestDeclarations_StatusNotTBA()
		{
			pbn.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS124782356";
			pbn.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS000000001";
			pbn.TransitDeclarationCollection.AddNew().CSI_ReferenceNumber = "IE09301248050000";

			var tbd = pbn.CustomsReferenceCollection.AddNew();
			tbd.CSI_ReferenceNumber = "25IEROS000000002";
			tbd.CSI_Status = PBNDeclarationReferenceStatusList.Codes.TBD;

			var declarations = Provider.Declarations;
			AssertType<PBNDeclarationProvider[]>(declarations);
			AssertEquals("Should include declarations from CustomsReferenceCollection and TransitDeclarationCollection", 3, declarations.Count);
		}

		public void TestContactDetails()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_EmailAddress = "bob@test.ie";
			person.PER_MobilePhone = "+353871234567";
			person.PER_HomePhone = "+353873456789";

			var contact = pbn.Persons.AddNew();
			contact.CPN_PER_Person = person.PK;

			AssertType<PBNContactDetailsProvider>(Provider.ContactDetails);
			CombineAssertions(() =>
			{
				AssertEquals("Email", "bob@test.ie", Provider.ContactDetails.Email);
				AssertEquals("MobileNum1", "+353871234567", Provider.ContactDetails.MobileNum1);
				AssertEquals("MobileNum2", "+353873456789", Provider.ContactDetails.MobileNum2);
			});
		}

		protected override CPBAndUPBMessageProvider GetProvider() => new CPBAndUPBMessageProvider(new PBNMessageSendingObject(pbn));

		protected override void SetUp()
		{
			base.SetUp();
			pbn = Factory.New<AsycudaManifestHeader>();
		}
		AsycudaManifestHeader pbn;
	}
}
