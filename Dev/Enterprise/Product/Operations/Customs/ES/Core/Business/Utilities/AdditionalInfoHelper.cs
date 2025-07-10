using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public static class AdditionalInfoHelper
	{
		public static AdditionalInfo GetAdditionalInfoByCode(AdditionalInfoCollection documentsList, ZString code) => documentsList.Cast<AdditionalInfo>().FirstOrDefault(x => x.CSI_Code == code);

		public static void ProcessExportEntryLineAdditionalInfos(this CusEntryLine entryLine)
		{
			var documentsToRemove = entryLine.GetPreviouslySentAdditionalInfos();

			foreach (var docu in documentsToRemove)
			{
				docu.Delete();
			}

			entryLine.ReadOnlyAdditionalInfos.LoadNew();
			var readOnlyAdditionalInfos = entryLine.ReadOnlyAdditionalInfos.Cast<ReadOnlyAdditionalInfo>();
			foreach (ReadOnlyAdditionalInfo readOnlyAddInf in readOnlyAdditionalInfos)
			{
				var document = AdditionalInfo.CopyFrom(readOnlyAddInf);
				document.CSI_Status = DocumentStatus.Accepted;
				document.CSI_ParentID = entryLine.PK;
				document.CSI_ParentTableCode = entryLine.TablePrefix;
			}
		}
	}
}
