using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FNLineCreator
	{
		public Import5FNLine Create(CusEntryLine entryLine)
		{
			var import5FNLineData = new Import5FNLine();
			var invoiceLine = entryLine.RandomLine;
			import5FNLineData.EntryLineNo = entryLine.CL_LineNumber;
			import5FNLineData.HSCode = invoiceLine.JI_Tariff;
			import5FNLineData.ModelName = invoiceLine.JI_Model;
			import5FNLineData.Remark = invoiceLine.AdditionalInformationContent;

			if (invoiceLine.ConsigneeAddress != null)
			{
				import5FNLineData.GoodsLocation = new Organisation(RoleType.Consignee)
				{
					PhoneNumber = invoiceLine.ConsigneeAddress.OA_Phone,
					Postcode = invoiceLine.ConsigneeAddress.Postcode,
					RoadNameCode = invoiceLine.ConsigneeAddress.GetRoadNameCode(),
					BuildingNumber = invoiceLine.ConsigneeAddress.GetBuildingNumber(),
					AddressLine1 = invoiceLine.ConsigneeAddress.Address1,
					AddressLine2 = invoiceLine.ConsigneeAddress.Address2,
					IsIndividual = invoiceLine.ConsigneeAddress.Header.GetIsIndividual()
				};
			}
			import5FNLineData.DutyReductionClassification = invoiceLine.DutyReductionClassificationCode;
			import5FNLineData.SerialNumber = invoiceLine.JI_SerialNumber;
			import5FNLineData.UseCodeDescription = invoiceLine.JI_SpecificUseCodeDescription;
			import5FNLineData.ProductTypeCode = invoiceLine.JI_SpecificUseProductType;

			ZString[] dutyReductionRateRegulationCode = invoiceLine.JI_DutyReductionRateRegulationCode.Split(Constants.ImportDutyReductionRateRegulationCodeSplitor);
			if (dutyReductionRateRegulationCode.Length == 3)
			{
				import5FNLineData.ReductionRateRegulationGroupNumber = dutyReductionRateRegulationCode[0];
				import5FNLineData.ReductionRateRegulationSeqNumber = dutyReductionRateRegulationCode[1];
				import5FNLineData.ReductionRateRegulationItemNumber = dutyReductionRateRegulationCode[2];
			}

			import5FNLineData.ScheduledReExportCustomsOffice = invoiceLine.JI_ScheduledReExportCustomsOffice;
			import5FNLineData.ReExportDestinationCountryCode = invoiceLine.JI_RN_NKReExportDestinationCountry;
			if (!invoiceLine.JI_ScheduledReExportDate.IsEmpty)
			{
				import5FNLineData.ScheduledReExportDate = invoiceLine.JI_ScheduledReExportDate.ToDateTime();
			}
			import5FNLineData.PostClearanceProcedureYN = entryLine.RandomLine.JI_PCProcedure;
			import5FNLineData.JurisdictionalCustomsOffice = invoiceLine.JI_JurisdictionalCusOffice;

			import5FNLineData.Header = Populateimport5FNHeader(entryLine.Header);
			return import5FNLineData;
		}

		Import5FNHeader Populateimport5FNHeader(CusEntryHeader entry)
		{
			var import5FNHeaderData = new Import5FNHeader();

			import5FNHeaderData.ImportDeclarationNumber = entry.EntryNumber;
			var declaration = entry.Declaration;
			import5FNHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			import5FNHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;

			var importer = OrgHeaderWrapper.New(declaration.ImporterAddress?.Header);
			if (importer != null)
			{
				import5FNHeaderData.TypeOfBusiness = importer.ZO_TypeOfBusiness;
			}

			if (declaration.PayerAddress != null)
			{
				import5FNHeaderData.Payer = new Organisation(RoleType.Payer)
				{
					CompanyName = declaration.PayerAddress.CompanyName,
					FaxNumber = declaration.PayerAddress.OA_Fax,
					MobileNumber = declaration.PayerAddress.OA_Mobile,
					Email = declaration.PayerAddress.OA_Email,
					IsIndividual = declaration.DutyPayer.GetIsIndividual()
				};

				var matchedNumber = declaration.PayerAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted() ?? declaration.PayerAddress.GetRegistrationIDNumber(IdentificationType.ForeignCompanyID);
				if (matchedNumber != null)
				{
					import5FNHeaderData.Payer.SetRegistrationIDNumbers(new IDNumberAndType[] { matchedNumber });
				}
			}

			if (declaration.BrokerAddress != null)
			{
				import5FNHeaderData.CustomsBroker = new Organisation(RoleType.Broker)
				{
					CompanyName = declaration.BrokerAddress.CompanyName,
					FaxNumber = declaration.BrokerAddress.OA_Fax,
					MobileNumber = declaration.BrokerAddress.OA_Mobile,
					Email = declaration.BrokerAddress.OA_Email,
					IsIndividual = declaration.BrokerAddress.Header.GetIsIndividual()
				};
			}

			return import5FNHeaderData;
		}
	}
}
