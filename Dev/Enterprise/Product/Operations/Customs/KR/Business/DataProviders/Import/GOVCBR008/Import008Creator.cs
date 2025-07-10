using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import008Creator
	{
		public Import008Header Create(CusEntryHeader entry)
		{
			var import008Data = new Import008Header();

			var declaration = entry.Declaration;
			var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._008);
			if (entryNum != null)
			{
				import008Data.ImportDeclarationNumber = entryNum.CE_EntryNum;
			}
			import008Data.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			import008Data.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			import008Data.DecType = declaration.JE_MessageSubType;
			import008Data.LoadingPort = declaration.JE_RL_NKPortOfLoading;
			import008Data.HBL = declaration.JE_HouseBill;
			import008Data.ForeignCarrier = declaration.ShippingLine?.OH_FullName;
			import008Data.DomesticCarrier = declaration.DeliveryOrPickupCartageCoAddr?.CompanyName;

			if (!declaration.JE_RL_NKOrigin.IsEmpty)
			{
				var refUNLOCO = declaration.Origin;

				if (refUNLOCO != null)
				{
					import008Data.ForeignCountryCode = refUNLOCO.RL_RN_NKCountryCode;
					import008Data.ForeignCity = refUNLOCO.RL_PortName;
				}
			}

			if (!declaration.JE_ExportDate.IsEmpty)
			{
				import008Data.TransportationStartDate = declaration.JE_ExportDate.ToDateTime();
			}

			if (!declaration.JE_DateOfArrival.IsEmpty)
			{
				import008Data.TransportationArrivalDate = declaration.JE_DateOfArrival.ToDateTime();
			}

			var charge = declaration.Invoices.FirstOrDefault()?.Charges.Cast<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			if (charge != null)
			{
				import008Data.Freight = charge.J7_Amount;
			}

			PopulateImport008DecQuestion(declaration, import008Data);
			PopulateImport008Items(declaration, import008Data);
			PopulateImport008Person(declaration, import008Data);

			return import008Data;
		}

		void PopulateImport008DecQuestion(JobDeclaration declaration, Import008Header import008Data)
		{
			var orderedQuestions = declaration.PersonalItemDecQuestions.Cast<CusCodeData>().OrderBy(x => x.CY_Code);

			var decQuestionList = new List<Import008DecQuestion>();
			foreach (CusCodeData orderedQuestion in orderedQuestions)
			{
				var entryCodeData = new Import008DecQuestion()
				{
					QuestionID = orderedQuestion.CY_Code,
					Answer = orderedQuestion.CY_Data
				};
				decQuestionList.Add(entryCodeData);
			}
			import008Data.QuestionsAndAnswers = decQuestionList.ToArray();
		}

		void PopulateImport008Items(JobDeclaration declaration, Import008Header import008Data)
		{
			var movingItems = new List<Import008Line>();
			var orderedLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo);

			foreach (JobComInvoiceLine invoiceLine in orderedLines)
			{
				movingItems.Add(PopulateMovingGoods(invoiceLine));
			}
			import008Data.Lines = movingItems.Count > 0 ? movingItems.ToArray() : null;
			import008Data.Vehicle = PopulateVehicle(declaration);
		}

		static Import008Line PopulateMovingGoods(JobComInvoiceLine invoiceLine)
		{
			return new Import008Line()
			{
				ItemCategory = invoiceLine.JI_ProductTypeCode,
				ItemCode = invoiceLine.JI_InvoiceUQ,
				InvoiceDescription = invoiceLine.JI_Description,
				BrandName = invoiceLine.JI_BrandName,
				MonthOfUse = invoiceLine.JI_CustomsQuantity.ToZInt(),
				Quantity = invoiceLine.JI_InvoiceQuantity,
				Price = invoiceLine.JI_LinePrice,
				Model = invoiceLine.JI_Model
			};
		}

		static Import008BulkItem PopulateVehicle(JobDeclaration declaration)
		{
			var result = new Import008BulkItem()
			{
				Type = BulkItemCodeList.Codes.Vehicle,
				ModelName = declaration.ModelName,
				IdentificationNumber = declaration.VehicleIdentificationNumber,
				EngineDisplacement = declaration.EngineCapacity,
				ModelYear = declaration.ModelYear,
				ManufacturingCountry = declaration.CountryOfManufacture,
				SeatingCapacity = declaration.SeatingCapacity
			};

			if (!declaration.DateOfFirstRegistration.IsEmpty)
			{
				result.FirstRegistrationDate = declaration.DateOfFirstRegistration.ToDateTime();
			}

			if (!declaration.DateOfCurrentRegistration.IsEmpty)
			{
				result.CurrentRegistrationDate = declaration.DateOfCurrentRegistration.ToDateTime();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string owner = "본인";

		void PopulateImport008Person(JobDeclaration declaration, Import008Header import008Data)
		{
			if (declaration.ImporterAddress != null)
			{
				import008Data.Declarant = new Organisation(RoleType.Declarant)
				{
					AddressLine1 = declaration.ImporterAddress.Address1,
					AddressLine2 = declaration.ImporterAddress.Address2,
					RoadNameCode = declaration.ImporterAddress.GetRoadNameCode(),
					BuildingNumber = declaration.ImporterAddress.GetBuildingNumber(),
					Postcode = declaration.ImporterAddress.Postcode,
					PhoneNumber = declaration.ImporterAddress.OA_Mobile,
					Email = declaration.ImporterAddress.OA_Email
				};
			}

			var cusPersons = declaration.Persons;
			var familyList = new List<Import008Person>();
			if (cusPersons != null)
			{
				foreach (CusPerson cusPerson in cusPersons)
				{
					var glbPerson = cusPerson.Person;

					var person = new Import008Person()
					{
						Name = glbPerson.PER_FullName,
						PassportNumber = glbPerson.PER_Passport,
						JobCode = cusPerson.JobCode,
					};

					if (!glbPerson.PER_BirthDate.IsEmpty)
					{
						person.BirthDate = glbPerson.PER_BirthDate.ToDateTime();
					}

					if (!cusPerson.ResidencyStartDate.IsEmpty)
					{
						person.ScheduledStartDateInKR = cusPerson.ResidencyStartDate.ToDateTime();
					}

					if (!cusPerson.ResidencyEndDate.IsEmpty)
					{
						person.ScheduledEndDateInKR = cusPerson.ResidencyEndDate.ToDateTime();
					}

					if (cusPerson.RelationshipToDeclarant == owner)
					{
						person.Nationality = glbPerson.PER_RN_NKNationalityCodeISO;
						person.NationalityClassCode = cusPerson.NationalityClassCode;

						import008Data.Owner = person;
					}
					else
					{
						person.RelationshipToImporter = cusPerson.RelationshipToDeclarant;
						person.EntryToKR_YN = cusPerson.EntryStatus;

						familyList.Add(person);
					}
				}
				import008Data.FamilyMembers = familyList.Count > 0 ? familyList.ToArray() : null;
			}
		}
	}
}
