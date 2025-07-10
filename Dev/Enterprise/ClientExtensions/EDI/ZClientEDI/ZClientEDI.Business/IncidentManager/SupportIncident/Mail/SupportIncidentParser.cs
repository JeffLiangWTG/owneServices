using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentParser : DocumentParser<SupportIncident>
	{
		public SupportIncidentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocSupportIncident); }
		}
	}
}