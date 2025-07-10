using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement
{
	public class NewsAnnouncementInterceptor : BaseInterceptor
	{
		public NewsAnnouncementInterceptor(NewsAnnouncementSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices) { }

		public override void Invoke(IEntitySet entitySet)
		{
			var note = entitySet.Root;
			if (!note.HasProperty(SectionPropName))
			{
				throw new NativeXMLUserVisibleException($"NewsAnnouncement cannot be imported as section has not been provided.");
			}

			var customerCodes = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly).GetAllCodes();
			var section = note[SectionPropName].ToString();
			if (!customerCodes.Contains(section))
			{
				throw new NativeXMLUserVisibleException($"NewsAnnouncement cannot be imported as {section} section is invalid.");
			}

			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string SectionPropName = "Section";
	}
}
