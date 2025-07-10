using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS305;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.AIS.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS305Provider))]
	sealed class TS305ProviderTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			CombineAssertions(() =>
			{
				AssertEquals("RejectionDate", ZDateTime.BrettsBirthday, provider.RejectionDate);
				AssertEquals("RejectionReason", "It was unreasonable", provider.RejectionReason);
				AssertEquals("MovementReferenceNumber", "01MR1234567890ABR9", provider.MovementReferenceNumber);
			});
		}

		public void TestFunctionalErrors()
		{
			AssertNotNull("FunctionalErrors", provider.FunctionalErrors);
			AssertEquals("FunctionalErrors Count", 2, provider.FunctionalErrors.Count);
			AISUCC6ProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(SequenceNumber: "1", ErrorPointer: "POINTER", ErrorCode: "12", ErrorReason: "THEREASON1", Remarks: "Test_Functional_Error", OriginalAttributeValue: "ATTR_VALUE"),
				(SequenceNumber: "2", ErrorPointer: "NPOINTER", ErrorCode: "13", ErrorReason: "THEREASON2", Remarks: "Test_Functional_Error_2", OriginalAttributeValue: "MYVALUEFORTEST")
			}, provider.FunctionalErrors);
		}

		public void TestNoFunctionalErrors()
		{
			var errorlessProvider = new TS305Provider(new Ts305());
			AssertNotNull("FunctionalErrors should not be null", errorlessProvider.FunctionalErrors);
			AssertEquals("FunctionalErrors Count", 0, errorlessProvider.FunctionalErrors.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS305Provider(new Ts305()
			{
				Declaration = new DeclarationType06()
				{
					AmendmentRejectionDate = ZDateTime.BrettsBirthday.ToDateTime(),
					AmendmentRejectionReason = "It was unreasonable",
					Mrn = "01MR1234567890ABR9"
				},
				SupervisingCustomsOffice = new SupervisingcustomofficeType()
				{
					ReferenceNumber = "AA123456"
				},
				CustomsOfficeLodgement = new MScoType01()
				{
					ReferenceNumber = "BB123456"
				},
				Declarant = new DeclarantType03()
				{
					Name = "Mr A",
					IdentificationNumber = "AA!",
					Address = new DeclarantAddressType02()
					{
						PoBox = "-",
						Number = "1",
						Street = "One Street",
						StreetAdditionalLine = "-",
						City = "London",
						Postcode = "W1 1AA",
						Country = "UK",
						SubDivision = "-",
					},
					ContactDetails = new ContactDetailsType()
					{
						IdType = "1",
						IdNumber = "12345678",
						Country = "GB"
					},
					Communication = new Collection<CommunicationType>(new[]
					{
						new CommunicationType()
						{
							Type = "T01",
							Identifier = "Identifier"
						}
					})
				},
				FunctionalError = new Collection<MFunctionalErrorType01>(new[]
				{
					new MFunctionalErrorType01
					{
						SequenceNumber = "1",
						Remarks = "Test_Functional_Error",
						ErrorCode = "12",
						ErrorPointer = "POINTER",
						ErrorReason = "THEREASON1",
						OriginalAttributeValue = "ATTR_VALUE",
					},
					new MFunctionalErrorType01
					{
						SequenceNumber = "2",
						Remarks = "Test_Functional_Error_2",
						ErrorCode = "13",
						ErrorPointer = "NPOINTER",
						ErrorReason = "THEREASON2",
						OriginalAttributeValue = "MYVALUEFORTEST",
					}
				})
			});
		}

		TS305Provider provider;
	}
}
