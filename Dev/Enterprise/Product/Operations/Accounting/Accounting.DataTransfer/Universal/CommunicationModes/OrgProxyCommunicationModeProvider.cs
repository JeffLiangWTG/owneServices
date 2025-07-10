using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer
{
	class OrgProxyCommunicationModeProvider : IMessageProcessorCommunicationModesResult
	{
		public OrgProxyCommunicationModeProvider(string module, string format, Func<OrgHeader> orgProvider = null, Func<EDICommunicationsMode, bool> filter = null)
		{
			this.module = module;
			this.format = format;
			this.filter = filter ?? ((e) => true);
			this.orgProvider = orgProvider ?? (() => GlbCompany.CurrentCompany.OrgProxy);
		}

		readonly string module;
		readonly string format;
		readonly Func<EDICommunicationsMode, bool> filter;
		readonly Func<OrgHeader> orgProvider;

		IEDICommunicationsMode[] communicationModes;
		MultilingualString reasonForNoCommunicationModes;

		public IList<IMessageDestinationSource> Destinations
		{
			get
			{
				if (communicationModes == null)
				{
					InitCommunicationModes();
				}
				return communicationModes;
			}
		}

		public MultilingualString ConfigurationLogging
		{
			get
			{
				if (communicationModes == null)
				{
					InitCommunicationModes();
				}
				return reasonForNoCommunicationModes;
			}
		}

		void InitCommunicationModes()
		{
			var org = orgProvider();
			if (org == null)
			{
				communicationModes = Array.Empty<IEDICommunicationsMode>();
				reasonForNoCommunicationModes = ResString.GetMultilingualString("80ef1b08-87e5-449b-8bfa-b26576202e8f", "Organization could not be found.");
			}
			else
			{
				var mode = org.EDICommunicationsModes.FindByModuleAndFileFormat(module, format).FirstOrDefault(filter); // Why is it FirstOrDefault?
				if (mode != null)
				{
					communicationModes = new[] { mode };
				}
				else
				{
					communicationModes = Array.Empty<IEDICommunicationsMode>();
					reasonForNoCommunicationModes = ResString.GetMultilingualString("0e5e3e89-72cc-41d2-ac27-35ae255504a5", "Missing communication modes for Organization Proxy [{0}] with Module [{1}] in Format [{2}]", org.OH_FullName, module, format);
				}
			}
		}
	}
}
