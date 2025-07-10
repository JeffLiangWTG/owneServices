using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(RequestedDocument))]
	class RequestedDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<RequestedDocument>
	{
		[ExpectNoExceptions]
		public void TestLookups()
		{
			var requestedDocument = Factory.New<RequestedDocument>();
			NUnit.Framework.Assert.That(requestedDocument.Lookups, NUnit.Framework.Is.TypeOf<RequestedDocumentLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var requestedDocument = Factory.New<RequestedDocument>();
			NUnit.Framework.Assert.That(requestedDocument.Validation, NUnit.Framework.Is.TypeOf<RequestedDocumentValidation>());
		}

		[ExpectNoExceptions]
		public void TestCSI_Code()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<MaxLengthAttribute>(nameof(RequestedDocument.CSI_Code), false, attribute => attribute.MaxLength == 4));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Code), false, attribute => attribute.Caption == "Type"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Code), false, attribute => attribute.MediumCaption == "Type"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Code), false, attribute => attribute.ShortCaption == "Type"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Code), false, attribute => attribute.FullDescription == "Requested document type."));
		}

		[ExpectNoExceptions]
		public void TestRequestInformation_Attributes()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<MaxLengthAttribute>(nameof(RequestedDocument.RequestInformation), false, attribute => attribute.MaxLength == 512));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.RequestInformation), false, attribute => attribute.Caption == "Requested Information"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.RequestInformation), false, attribute => attribute.MediumCaption == "Requested Info."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.RequestInformation), false, attribute => attribute.ShortCaption == "Req. Info."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.RequestInformation), false, attribute => attribute.FullDescription == "Requested information in relation to the requested document type."));
		}

		[ExpectNoExceptions]
		public void TestRequestInformation_GetAndSet()
		{
			var requestedDocument = Factory.New<RequestedDocument>();

			requestedDocument.RequestInformation = new string('A', RequestedDocument.Schema.CSI_DescriptionMaxLength) + new string('B', 128);
			NUnit.Framework.Assert.That(requestedDocument.CSI_Description, NUnit.Framework.Is.EqualTo(new string('A', RequestedDocument.Schema.CSI_DescriptionMaxLength)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(requestedDocument.CSI_AdditionalDescription, NUnit.Framework.Is.EqualTo(new string('B', 128)).Using(CustomComparers.TypeComparison));

			requestedDocument.CSI_Description = "ABC";
			requestedDocument.CSI_AdditionalDescription = "DEFG";
			NUnit.Framework.Assert.That(requestedDocument.RequestInformation, NUnit.Framework.Is.EqualTo("ABCDEFG").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCSI_DateOfIssue()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfIssue), false, attribute => attribute.Caption == "Date of Request"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfIssue), false, attribute => attribute.MediumCaption == "Date of Request"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfIssue), false, attribute => attribute.ShortCaption == "DOR"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfIssue), false, attribute => attribute.FullDescription == "Date of document request."));
		}

		[ExpectNoExceptions]
		public void TestCSI_DateOfExpiry()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfExpiry), false, attribute => attribute.Caption == "Provide By Date"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfExpiry), false, attribute => attribute.MediumCaption == "Prov. By Date"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfExpiry), false, attribute => attribute.ShortCaption == "PBD"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_DateOfExpiry), false, attribute => attribute.FullDescription == "Date by which the requested document must be provided."));
		}

		[ExpectNoExceptions]
		public void TestCSI_Status()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument).BaseType, CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(RequestedDocument.CSI_Status), true, attr => attr.ListDataSourceMember == nameof(RequestedDocument.Lookups) + "." + nameof(RequestedDocumentLookups.StatusList)));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Status), false, attribute => attribute.Caption == "Status"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Status), false, attribute => attribute.MediumCaption == "Status"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Status), false, attribute => attribute.ShortCaption == "Status"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_Status), false, attribute => attribute.FullDescription == "Requested document status."));
		}

		[ExpectNoExceptions]
		public void TestStatusDescription()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.StatusDescription), false, attribute => attribute.Caption == "Status Description"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.StatusDescription), false, attribute => attribute.MediumCaption == "Status Desc."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.StatusDescription), false, attribute => attribute.ShortCaption == "Desc."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.StatusDescription), false, attribute => attribute.FullDescription == "Requested document status description."));
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber()
		{
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_ReferenceNumber), false, attribute => attribute.Caption == "Reference Number"));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_ReferenceNumber), false, attribute => attribute.MediumCaption == "Reference No."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_ReferenceNumber), false, attribute => attribute.ShortCaption == "Ref. No."));
			NUnit.Framework.Assert.That(typeof(RequestedDocument), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>(nameof(RequestedDocument.CSI_ReferenceNumber), false, attribute => attribute.FullDescription == "Requested document reference number."));
		}
		protected override IEnumerable<RequestedDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().RequestedDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().RequestedDocuments.AddNew();
		}
	}
}
