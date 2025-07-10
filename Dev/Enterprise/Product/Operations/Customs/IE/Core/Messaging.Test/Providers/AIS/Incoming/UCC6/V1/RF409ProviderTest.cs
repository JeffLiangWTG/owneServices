using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using GoodsInformationType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.GoodsInformationType;
using PartiesType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.PartiesType;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	[TestedType(typeof(RF409Provider))]
	sealed class RF409ProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RF409Provider(null));
		}

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ApplicationReferenceId", "Abcd123aisdu3eh2u89764", provider.ApplicationReferenceId);
				AssertEquals("ApplicationDecisionCodeType_1_1", "20230802", provider.ApplicationDecisionCodeType);
				AssertEquals("RefundApplicationAccepted", true, provider.RefundApplicationAccepted);
				AssertEquals("Signature_1_2", "IE123456", provider.Signature);
				AssertEquals("DecisionTakingCustomsAuthority", "SOYJO", provider.DecisionTakingCustomsAuthority);
				AssertEquals("TotalNumberOfDocuments", "456", provider.TotalNumberOfDocuments);
				AssertEquals("Applicant", "APP_3_2_Content", provider.Applicant);
				AssertEquals("RepresentativeIdentification", "REPR_ID_3_4_Value", provider.RepresentativeIdentification);
				AssertEquals("Date", "20230812", provider.Date);
				AssertEquals("OfficeOfDept", "OFF_OF_DEPT", provider.OfficeOfDept);
				AssertEquals("OfficeOfResponsibility", "OFF_OF_RESP", provider.OfficeOfResponsibility);
				AssertEquals("MRN", "123456789IE", provider.MRN);
				AssertEquals("LegalBasisCode", "A04", provider.LegalBasisCode);
				AssertEquals("CustomsProcedureCode", "A08", provider.CustomsProcedureCode);
				AssertEquals("AmountOfDutiesToBeRemitted", "EUR 152.05", provider.AmountOfDutiesToBeRemitted);
				AssertEquals("DestinationOfGoods", Core.Constants.CountryCodes.Brazil, provider.DestinationOfGoods);
				AssertEquals("TimeLimit", "1502", provider.TimeLimit);
				AssertEquals("StatementOfTheDecision", "STA_OF_DEC", provider.StatementOfTheDecision);
				AssertEquals("DescriptionOfGrounds", "DESC_OF_GROUNDS", provider.DescriptionOfGrounds);
			});
		}

		public void TestGoodsInformations()
		{
			AssertNotNull("GoodsInformations", provider.GoodsInformations);
			AssertEquals("GoodsInformations Count", 2, provider.GoodsInformations.Count);

			var providerWithNoGoodsInformations = new RF409Provider(new Rf409());
			AssertNotNull("GoodsInformations when RF409 errors are null", providerWithNoGoodsInformations.GoodsInformations);
			AssertEquals("GoodsInformations Count when RF409 errors are null", 0, providerWithNoGoodsInformations.GoodsInformations.Count);
		}

		public void TestTypeOfDuties()
		{
			AssertNotNull("TypeOfDuties", provider.TypeOfDuties);
			AssertEquals("TypeOfDuties Count", 1, provider.TypeOfDuties.Count);

			var providerWithNoTypeOfDuties = new RF409Provider(new Rf409());
			AssertNotNull("TypeOfDuties when RF409 errors are null", providerWithNoTypeOfDuties.TypeOfDuties);
			AssertEquals("TypeOfDuties Count when RF409 errors are null", 0, providerWithNoTypeOfDuties.TypeOfDuties.Count);
		}

		public void TestAttachedDocuments()
		{
			AssertNotNull("AttachedDocuments", provider.AttachedDocuments);
			AssertEquals("AttachedDocuments Count", 2, provider.AttachedDocuments.Count);

			var providerWithNoAttachedDocuments = new RF409Provider(new Rf409());
			AssertNotNull("AttachedDocuments when RF409 errors are null", providerWithNoAttachedDocuments.AttachedDocuments);
			AssertEquals("AttachedDocuments Count when RF409 errors are null", 0, providerWithNoAttachedDocuments.AttachedDocuments.Count);
		}

		public void TestGeneralRemarks()
		{
			AssertNotNull("GeneralRemarks", provider.GeneralRemarks);
			AssertEquals("GeneralRemarks Count", 3, provider.GeneralRemarks.Count);

			var providerWithNoGeneralRemarks = new RF409Provider(new Rf409());
			AssertNotNull("GeneralRemarks when RF409 errors are null", providerWithNoGeneralRemarks.GeneralRemarks);
			AssertEquals("GeneralRemarks Count when RF409 errors are null", 0, providerWithNoGeneralRemarks.GeneralRemarks.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new RF409Provider(GenerateMessage());
		}

		RF409Provider provider;

		Rf409 GenerateMessage()
		{
			return new Rf409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					ApplicationDecisionCodeType = "20230802",
					RefundApplicationAccepted = "1",
					Signature = "IE123456",
					DecisionTakingCustomsAuthority = "SOYJO",
					TotalNumberOfDocuments = "456",
				},
				Parties = new PartiesType
				{
					Applicant = "APP_3_2_Content",
					RepresentativeIdentification = "REPR_ID_3_4_Value",
				},
				DatesPlaces = new DatePlacesType
				{
					Date = "20230812",
					OfficeOfDept = "OFF_OF_DEPT",
					OfficeOfResponsibility = "OFF_OF_RESP",
				},
				Mrn = "123456789IE",
				LegalBasisCodes = new LegalBasisCodeType
				{
					LegalBasis = "A04",
				},
				CustomsProcedure = new CustomsProcedureType
				{
					ProcedureCode = "A08",
				},
				DutiesToBeRemitted = new AmountOfDutiesRf409Type
				{
					AmountOfDutiesToBeRepaid = new AmountOfDutiesToBeRepaidRf409Type
					{
						Amount = 152.05m,
						Currency = Core.Constants.CurrencyCodes.Ireland,
					}
				},
				DestinationOfGoods = Core.Constants.CountryCodes.Brazil,
				TimeLimit = "1502",
				StatementOfTheDecision = "STA_OF_DEC",
				DescriptionOfGrounds = "DESC_OF_GROUNDS",
				GoodsInformation = new Collection<GoodsInformationType>
				{
					new GoodsInformationType
					{
						CustomsValue = new CustomsValueType
						{
							Amount = 150m,
							Currency = Core.Constants.CurrencyCodes.Ireland,
						}
					},
					new GoodsInformationType
					{
						CustomsValue = new CustomsValueType
						{
							Amount = 10.25m,
							Currency = Core.Constants.CurrencyCodes.Bahamas,
						}
					}
				},
				TypeOfDuty = new Collection<TypeOfDutyType>
				{
					new TypeOfDutyType
					{
						TypeOfDuty = new TypeOfDutyRf409Type
						{
							NationalCode = "ABCD",
							UnionCode = "DCBA",
						}
					}
				},
				AttachedDocuments = new Collection<AttachedDocumentType>
				{
					new AttachedDocumentType
					{
						DocumentDate = "20230512",
						DocumentIdentifier = "456789",
						DocumentType = "DOC_TYPE1"
					},
					new AttachedDocumentType
					{
						DocumentDate = "20230112",
						DocumentIdentifier = "0123465",
						DocumentType = "DOC_TYPE2"
					}
				},
				GeneralRemarks = new Collection<RemarksType>
				{
					new RemarksType
					{
						GeneralRemarks = "ETU",
					},
					new RemarksType
					{
						GeneralRemarks = "EHU",
					},
					new RemarksType
					{
						GeneralRemarks = "EPU",
					}
				}
			};
		}
	}
}
