using Enterprise.MailManager;
using Enterprise.MailManager.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Mail.Module
{
	public class EDIWorkTaskMailItemFilterBusinessObject : MailItemFilterBusinessObject
	{
		public EDIWorkTaskMailItemFilterBusinessObject()
		{
		}

		#region Default Values

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			ModuleTextFilter fltStatus = (ModuleTextFilter)filters["Status"];
			ModuleTextFilter fltDirection = (ModuleTextFilter)filters["Direction"];

			fltStatus.DefaultProperty = MailStatus.Unprocessed;
			fltDirection.DefaultProperty = MailDirection.Receive;

			return filters;
		}

		#endregion

		#region Lists

		protected override CodeDescriptionPairList GetStatusListCore()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("", "");
			list.AddPair(MailStatus.Processed, "Processed");
			list.AddPair(MailStatus.Unprocessed, "Unprocessed");
			return list;
		}

		#endregion
	}
}
