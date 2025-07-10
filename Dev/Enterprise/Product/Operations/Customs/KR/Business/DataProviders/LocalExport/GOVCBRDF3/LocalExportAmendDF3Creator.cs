using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendDF3Creator
	{
		public LocalExportAmendEntryHeader Create(CusEntryHeader entry)
		{
			var localExportData = new LocalExportAmendEntryHeader();

			if (entry != null)
			{
				localExportData.CustomsReceiptNumber = entry.CH_BGMReference;
				localExportData.DeclarationCustomsOfficeAndDivision = entry.Declaration.JE_CustomsOffice + entry.Declaration.JE_CustomsDivision;
				if (entry.Declaration.JE_EntryDate != ZDateTime.Empty)
				{
					localExportData.LoadingDate = entry.Declaration.JE_EntryDate.ToDateTime();
				}

				localExportData.Stevedores = PopulateStevedores(entry);
			}

			return localExportData;
		}

		LocalExportStevedore[] PopulateStevedores(CusEntryHeader entry)
		{
			var persons = entry.Declaration.Persons;
			var stevedoreList = new List<LocalExportStevedore>();
			var i = 1;
			foreach (CusPerson person in persons)
			{
				var stevedore = entry.Declaration.StevedoreCompany;
				if (stevedore != null)
				{
					var item = new LocalExportStevedore()
					{
						SequenceNo = i,
						CompanyName = stevedore.CompanyName,
						FullName = person.Person?.PER_FullName ?? ZString.Empty,
						PhoneNumber = stevedore.E2_Phone,
						MobileNumber = person.Person?.PER_MobilePhone ?? ZString.Empty
					};
					i++;
					stevedoreList.Add(item);
				}
			}
			return stevedoreList.Count > 0 ? stevedoreList.ToArray() : null;
		}
	}
}
