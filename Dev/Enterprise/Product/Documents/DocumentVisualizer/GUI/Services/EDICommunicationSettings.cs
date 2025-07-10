using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class EDICommunicationSettings : IEDICommunicationSettings
	{
		public EDICommunicationSettings(ICodeDescription recipient, ICodeDescription purpose, IEnumerable<IEDICommunicationsMode> communicationModes)
		{
			Recipient = recipient ?? new CodeDescriptionPair(string.Empty, string.Empty);
			Purpose = purpose ?? new CodeDescriptionPair(string.Empty, string.Empty);
			CommunicationsModes = communicationModes ?? Enumerable.Empty<IEDICommunicationsMode>();
		}

		public ICodeDescription Recipient { get; }
		public ICodeDescription Purpose { get; }
		public IEnumerable<IEDICommunicationsMode> CommunicationsModes { get; }
	}
}