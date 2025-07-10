using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AeoCertificateManagerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when supportingDocumentProcider is null", () => new AeoCertificateManager(new Mock<IAeoCertificateSupporter>().Object, null));
	}

	public void TestAddAeoCertificatesIfNeeded_AgainstSupplier()
	{
		aeoCertificateSupporterMock.Setup(m => m.ShouldAddY022Certificate).Returns(true);

		AssertThatAeoCertificateIsAddedIfNeeded(
			organisationPropertyName: CertificateOrgType.Supplier
			, aeoDocumentCode: UniversalReferenceConstants.SupportingDocumentTypes.Y022
			, aeoDocumentReference: "AEO022");

		AssertThatNoDocumentIsAddedWhenConditionAreNotSatisfied(ConditionPropertyName.ShouldAddY022Certificate, false);
	}

	public void TestAddAeoCertificatesIfNeeded_AgainstImporter()
	{
		aeoCertificateSupporterMock.Setup(m => m.ShouldAddY023Certificate).Returns(true);

		AssertThatAeoCertificateIsAddedIfNeeded(
			organisationPropertyName: CertificateOrgType.Importer
			, aeoDocumentCode: UniversalReferenceConstants.SupportingDocumentTypes.Y023
			, aeoDocumentReference: "AEO023");

		AssertThatNoDocumentIsAddedWhenConditionAreNotSatisfied(ConditionPropertyName.ShouldAddY023Certificate, false);
	}

	public void TestAddAeoCertificatesIfNeeded_AgainstDeclarant()
	{
		aeoCertificateSupporterMock.Setup(m => m.RepresentationType).Returns(RepresentationTypeList.Codes._2Direct);

		AssertThatAeoCertificateIsAddedIfNeeded(
			organisationPropertyName: CertificateOrgType.Declarant
			, aeoDocumentCode: UniversalReferenceConstants.SupportingDocumentTypes.Y024
			, aeoDocumentReference: "AEO024");

		AssertThatNoDocumentIsAddedWhenConditionAreNotSatisfied(ConditionPropertyName.RepresentationType, RepresentationTypeList.Codes._1Self);
	}

	#region Implementation

	protected override void SetUp()
	{
		base.SetUp();

		aeoCertificateSupporterMock = new Mock<IAeoCertificateSupporter>() { CallBase = true };
		aeoCertificateSupporterMock.Setup(m => m.ShouldAddY022Certificate).Returns(true);
		aeoCertificateSupporterMock.Setup(m => m.ShouldAddY023Certificate).Returns(true);
		aeoCertificateSupporterMock.Setup(m => m.RepresentationType).Returns(RepresentationTypeList.Codes._2Direct);

		var supportingDocumentProviderMock = new Mock<ISupportingDocumentsProvider>();
		var dummyObject = Factory.New<DummyBusinessObject>();
		supportingDocumentProviderMock.Setup(m => m.SupportingDocuments).Returns(new SupportingDocumentCollection(dummyObject));
		supportingDocumentsProvider = supportingDocumentProviderMock.Object;

		aeoCertificatesManager = new AeoCertificateManager(aeoCertificateSupporterMock.Object, supportingDocumentsProvider);
	}

	Mock<IAeoCertificateSupporter> aeoCertificateSupporterMock;
	ISupportingDocumentsProvider supportingDocumentsProvider;
	AeoCertificateManager aeoCertificatesManager;

	void AssertThatAeoCertificateIsAddedIfNeeded(CertificateOrgType organisationPropertyName, ZString aeoDocumentCode, ZString aeoDocumentReference)
	{
		aeoCertificatesManager.AddAeoCertificatesIfNeeded();
		AssertEquals($"No supporting documents expected when {organisationPropertyName} is empty", 0, supportingDocumentsProvider.SupportingDocuments.Count);

		var organisation = Factory.New<OrgHeader>();
		var cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, aeoDocumentReference);

		AssertThatOneDocumentIsAddedWhenConditionAreOk();
		AssertThatNoDocumentsAreAddedTwice();

		void AssertThatOneDocumentIsAddedWhenConditionAreOk()
		{
			switch (organisationPropertyName)
			{
				case CertificateOrgType.Supplier:
					aeoCertificateSupporterMock.Setup(m => m.Supplier).Returns(organisation);
					break;
				case CertificateOrgType.Importer:
					aeoCertificateSupporterMock.Setup(m => m.Importer).Returns(organisation);
					break;
				case CertificateOrgType.Declarant:
					aeoCertificateSupporterMock.Setup(m => m.Declarant).Returns(organisation);
					break;
			}

			aeoCertificatesManager.AddAeoCertificatesIfNeeded();

			AssertHasOnlySupportingDocument(aeoDocumentCode, aeoDocumentReference);
		}

		void AssertThatNoDocumentsAreAddedTwice()
		{
			aeoCertificatesManager.AddAeoCertificatesIfNeeded();
			AssertHasOnlySupportingDocument(aeoDocumentCode, aeoDocumentReference);

			var newAeoDocumentReference = aeoDocumentReference + "X";
			cusCode.OK_CustomsRegNo = newAeoDocumentReference;
			aeoCertificatesManager.AddAeoCertificatesIfNeeded();
			AssertHasManySupportingDocuments(aeoDocumentCode, aeoDocumentReference, newAeoDocumentReference);
		}
	}

	void AssertThatNoDocumentIsAddedWhenConditionAreNotSatisfied(ConditionPropertyName necessaryConditionPropertyName, object notSatisfiedNecessaryConditionPropertyValue)
	{
		aeoCertificateSupporterMock.Reset();
		switch (necessaryConditionPropertyName)
		{
			case ConditionPropertyName.ShouldAddY022Certificate:
				aeoCertificateSupporterMock.Setup(m => m.ShouldAddY022Certificate).Returns((bool)notSatisfiedNecessaryConditionPropertyValue);
				break;
			case ConditionPropertyName.ShouldAddY023Certificate:
				aeoCertificateSupporterMock.Setup(m => m.ShouldAddY023Certificate).Returns((bool)notSatisfiedNecessaryConditionPropertyValue);
				break;
			case ConditionPropertyName.RepresentationType:
				aeoCertificateSupporterMock.Setup(m => m.RepresentationType).Returns((string)notSatisfiedNecessaryConditionPropertyValue);
				break;
		}
		supportingDocumentsProvider.SupportingDocuments.RemoveAndDeleteAll();

		aeoCertificatesManager.AddAeoCertificatesIfNeeded();
		AssertEquals("No supporting documents expected when condition are not satisfied", 0, supportingDocumentsProvider.SupportingDocuments.Count);
	}
	void AssertHasOnlySupportingDocument(ZString codeExpected, ZString referenceNumberExpected)
	{
		var supportingDocuments = supportingDocumentsProvider.SupportingDocuments.Cast<SupportingDocument>();
		AssertEquals($"{codeExpected} Supporting Document is expected, Count", 1, supportingDocuments.Count(x => x.CSI_Code == codeExpected));
		var expectedSupportingDocument = supportingDocuments.SingleOrDefault(x => x.CSI_Code == codeExpected);
		CombineAssertions($"Check {codeExpected} Supporting Document", () =>
		{
			AssertEquals("Code", codeExpected, expectedSupportingDocument.CSI_Code);
			AssertEquals("ReferenceNumber", referenceNumberExpected, expectedSupportingDocument.CSI_ReferenceNumber);
		});
	}

	void AssertHasManySupportingDocuments(ZString codeExpected, params ZString[] referenceNumbersExpected)
	{
		var supportingDocuments = supportingDocumentsProvider.SupportingDocuments.Cast<SupportingDocument>();
		AssertEquals($"{codeExpected} Supporting Documents are expected, Count", referenceNumbersExpected.Length, supportingDocuments.Count(x => x.CSI_Code == codeExpected));
		AssertContainsExactElementsInAnyOrder("Reference Codes", referenceNumbersExpected, supportingDocuments.Select(x => x.CSI_ReferenceNumber).ToArray());
	}

	enum CertificateOrgType
	{
		None,
		Supplier,
		Importer,
		Declarant
	}

	enum ConditionPropertyName
	{
		None,
		ShouldAddY022Certificate,
		ShouldAddY023Certificate,
		RepresentationType
	}

	#endregion
}
