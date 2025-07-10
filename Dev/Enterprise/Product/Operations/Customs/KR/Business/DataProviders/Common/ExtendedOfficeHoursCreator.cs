using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public abstract class ExtendedOfficeHoursCreator<THeader, TEntry>
		where THeader : ExtendedOfficeHoursHeader<TEntry>, new()
		where TEntry : ExtendedOfficeHoursEntry, new()
	{
		public THeader Create(ExtendedHoursRequestHeader messageSendingObject)
		{
			var headerData = new THeader();
			headerData.ApplicationNumber = EDIMessage.EntryNumberPlaceHolder;
			if (messageSendingObject.StartDate.IsValid)
			{
				headerData.StartDateTime = messageSendingObject.StartDate.ToDateTime();
			}
			if (messageSendingObject.EndDate.IsValid)
			{
				headerData.EndDateTime = messageSendingObject.EndDate.ToDateTime();
			}
			headerData.DeclarationCustomsOffice = messageSendingObject.CustomsOffice;
			headerData.DeclarationCustomsDivision = messageSendingObject.Department;
			headerData.ApplicationReason = messageSendingObject.Reason;
			headerData.Declarant = PopulateBroker();
			headerData.Entries = PopulateEntries(messageSendingObject.ExtendedHoursRequestLines);
			return headerData;
		}

		TEntry[] PopulateEntries(ExtendedHoursRequestLineCollection requestLines)
		{
			var entryLineDataList = new List<TEntry>();
			foreach (ExtendedHoursRequestLine line in requestLines)
			{
				entryLineDataList.Add(PopulateEntry(line));
			}
			return entryLineDataList.ToArray();
		}

		TEntry PopulateEntry(ExtendedHoursRequestLine line)
		{
			var entryLineData = new TEntry();
			entryLineData.ReferenceNumber = line.ReferenceNumber;
			entryLineData.TotalCustomsValueInUSD = line.CustomsValue;
			entryLineData.TotalPackQty = line.PackageCount;
			entryLineData.TotalGrossWeightInKG = Core.Constants.Weight.Convert(line.TotalWeight, line.UQ, Core.Constants.Weight.Kilograms);
			PopulateMoreEntryFields(entryLineData, line);
			return entryLineData;
		}

		protected virtual void PopulateMoreEntryFields(TEntry entryData, ExtendedHoursRequestLine line)
		{ }

		Organisation PopulateBroker()
		{
			Organisation result = null;
			var broker = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
			if (broker != null)
			{
				result = new Organisation(Messaging.RoleType.Declarant)
				{
					CompanyName = broker.OH_FullName,
					RepresentativeName = broker.GetRepresentativeName(),
					PhoneNumber = broker.GetPhoneNumber()
				};
			}
			return result;
		}
	}
}
