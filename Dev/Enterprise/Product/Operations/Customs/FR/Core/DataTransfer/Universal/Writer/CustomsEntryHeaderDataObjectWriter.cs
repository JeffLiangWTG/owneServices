using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : EU.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, EU.DataTransfer.Universal.UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override void PopulateAddInfo(Customs.Business.CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			base.PopulateAddInfo(entryHeaderBO, entryHeaderData);

			entryHeaderData.AddInfoCollection.Add(
				new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = "GuaranteedAmount", Value = "0" });

			var events = entryHeaderBO.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.ConfirmationOfExit);
			var coeEvent = events?.FirstOrDefault();
			if (coeEvent != null)
			{
				entryHeaderData.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
				{
					Key = BondedWarehousingHelper.Constants.ExitDate,
					Value = coeEvent.EventTimeOffset.ToString("dd/MM/yyyy")
				});
			}

			if (!entryHeaderBO.EntryHeaderStatusDescription.IsEmpty)
			{
				entryHeaderData.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
				{
					Key = BondedWarehousingHelper.Constants.EntryStatusDescription,
					Value = entryHeaderBO.EntryHeaderStatusDescription
				});
			}
		}
	}
}
