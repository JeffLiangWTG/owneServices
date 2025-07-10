using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5BAHeaderCreator
	{
		public Import5BAHeader Create(CusEntryHeader entry)
		{
			var import5BAData = new Import5BAHeader();

			var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == SharedJobMessageTypeList.Codes.Import);
			if (entryNum != null)
			{
				import5BAData.ImportDeclarationNumber = entryNum.CE_EntryNum;
				if (!entryNum.CE_IssueDate.IsEmpty)
				{
					import5BAData.ImportDeclarationDate = entryNum.CE_IssueDate.ToDateTime();
				}
			}

			var instruction = entry.EntryInstruction;
			if (instruction != null)
			{
				import5BAData.TariffRateClassification = instruction.CEI_AgreedDutyRatePreferenceCode;
				import5BAData.TariffRate = instruction.CEI_AgreedDutyRate;
			}

			Populateimport5BABroker(entry, import5BAData);
			Populateimport5BAEntryLine(entry, import5BAData);

			return import5BAData;
		}

		void Populateimport5BABroker(CusEntryHeader entry, Import5BAHeader import5BAData)
		{
			if (entry.Declaration.BrokerAddress != null)
			{
				import5BAData.Declarant = new Organisation(Messaging.RoleType.Declarant)
				{
					CompanyName = entry.Declaration.BrokerAddress.CompanyName,
					RepresentativeName = entry.Declaration.BrokerAddress.Header.GetRepresentativeName(),
					AddressLine1 = entry.Declaration.BrokerAddress.Address1,
					AddressLine2 = entry.Declaration.BrokerAddress.Address2,
					Postcode = entry.Declaration.BrokerAddress.Postcode,
					RoadNameCode = entry.Declaration.BrokerAddress.GetRoadNameCode(),
					BuildingNumber = entry.Declaration.BrokerAddress.GetBuildingNumber(),
				};

				import5BAData.Declarant.SetRegistrationIDNumbers(entry.Declaration.BrokerAddress.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo }));
			}
		}

		void Populateimport5BAEntryLine(CusEntryHeader entry, Import5BAHeader import5BAData)
		{
			var entryLineList = new List<Import5BALine>();
			var entryLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			foreach (CusEntryLine entryLine in entryLines)
			{
				var invoiceLine = entryLine.RandomLine;
				var tariff = invoiceLine?.UniversalTariff;
				var entryLineData = new Import5BALine()
				{
					EntryLineNo = entryLine.CL_LineNumber,
					HSCode = entryLine.CL_AdValoremTariff,
					HSDescription = tariff?.ZZ1_Description ?? ZString.Empty,
					InvoiceDescription = invoiceLine?.JI_Description ?? ZString.Empty
				};
				entryLineList.Add(entryLineData);
			}
			import5BAData.EntryLines = entryLineList.ToArray();
		}
	}
}
