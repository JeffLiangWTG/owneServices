using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Registry;
using Enterprise.Edifact.Generic.V4;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using EDIFACTConstants = Enterprise.Customs.AE.Business.AEConstants.Messaging.EDIFACT;
using Placeholders = Enterprise.Customs.AE.Business.AEConstants.Messaging.Placeholders;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ManifestInterchangeSegmentProviderTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestGetHeaderSegment()
	{
		var bill = Factory.New<AsycudaBill>();
		var shipper = Factory.New<OrgHeader>();
		bill.ShipperOrgPK = shipper.PK;

		var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
		orgProxy.OH_Code = "ABC";
		shipper.OH_Code = "XYZ";

		CreateMPCICodeForOrg(orgProxy);
		CreateMPCICodeForOrg(shipper);

		using (AECustomsRegistry.Instance.NAICServiceProviderCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SENDER_ID"))
		{
			CombineAssertions(() =>
			{
				var result = Provider.TryGetHeaderSegment(bill, out var uNB);
				NUnit.Framework.Assert.That(result, Is.EqualTo(true), "Header Segment creation success");
				AssertHeaderSegment(uNB);

				result = Provider.TryGetHeaderSegment(shipper, out uNB);
				NUnit.Framework.Assert.That(result, Is.EqualTo(false), "Linked object not AsycudaBill");
			});
		}

		void CreateMPCICodeForOrg(OrgHeader org)
		{
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
			cusCode.OK_CodeType = OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber;
			cusCode.OK_CustomsRegNo = org.OH_Code + "_123";
		}
	}

	[ExpectNoExceptions]
	void AssertHeaderSegment(UNBSegment uNB)
	{
		NUnit.Framework.Assert.That(uNB.SyntaxIdentifier.SyntaxIdentifier, Is.EqualTo(EDIFACTConstants.Syntax), "SyntaxIdentifier");
		NUnit.Framework.Assert.That(uNB.SyntaxIdentifier.SyntaxVersionNumber, Is.EqualTo(EDIFACTConstants.SyntaxVersion), "SyntaxVersionNumber");
		NUnit.Framework.Assert.That(uNB.SyntaxIdentifier.CharacterEncoding, Is.EqualTo(EDIFACTConstants.CharacterEncoding), "CharacterEncoding");
		NUnit.Framework.Assert.That(uNB.SyntaxIdentifier.SyntaxReleaseNumber, Is.EqualTo(EDIFACTConstants.SyntaxRelesaseNumber), "SyntaxReleaseNumber");
		NUnit.Framework.Assert.That(uNB.InterchangeSender.SenderIdentification, Is.EqualTo("SENDER_ID"), "SenderId");
		NUnit.Framework.Assert.That(uNB.InterchangeSender.InterchangeSenderInternalIdentification, Is.EqualTo("ABC_123"), "SenderInternalId");
		NUnit.Framework.Assert.That(uNB.InterchangeSender.InterchangeSenderInternalSubIdentification, Is.EqualTo("XYZ_123"), "SenderInternalSubId");
		NUnit.Framework.Assert.That(uNB.DateTimeOfPreparation.Date, Is.EqualTo(Placeholders.DateOfCreation), "Date");
		NUnit.Framework.Assert.That(uNB.DateTimeOfPreparation.Time, Is.EqualTo(Placeholders.TimeOfCreation), "Time");
		NUnit.Framework.Assert.That(uNB.InterchangeControlReference, Is.EqualTo(Placeholders.InterchangeNumber), "ReferenceNumber");
		NUnit.Framework.Assert.That(uNB.ProcessingPriorityCode, Is.EqualTo(EDIFACTConstants.ProcessingPriority), "ProcessingPriority");
		NUnit.Framework.Assert.That(uNB.TestIndicator, Is.EqualTo(EDIFACTConstants.TestIndicatorValue), "TestIndicator");
	}

	[ExpectNoExceptions]
	public void TestSenderInternalIdHeaderSegmentSendByOtherCompany()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			company1.GC_Code = "DAE";
			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AAALFQA", Core.Constants.CountryCodes.UnitedArabEmirates);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "AUH";
			branch1.GB_BranchName = "AE - Branch 1";
			branch1.GB_OH_OrgProxy = proxy1.PK;
			Factory.Save();
			var bill = Factory.New<AsycudaBill>();

			using (AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AUH"))
			{
				CombineAssertions(() =>
				{
					var result = Provider.TryGetHeaderSegment(bill, out var uNB);
					NUnit.Framework.Assert.That(result, Is.EqualTo(true), "Header Segment creation success");
					NUnit.Framework.Assert.That(uNB.InterchangeSender.InterchangeSenderInternalIdentification, Is.EqualTo("AAALFQA"), "SenderInternalId");
				});
			}
		}
	}

	ManifestInterchangeSegmentProvider Provider => provider ??= new ManifestInterchangeSegmentProvider();
	ManifestInterchangeSegmentProvider provider;
}
