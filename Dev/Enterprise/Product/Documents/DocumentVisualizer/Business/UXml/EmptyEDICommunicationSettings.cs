using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class EmptyEDICommunicationSettings : IEDICommunicationSettings
	{
		public ICodeDescription Recipient => recipient ?? (recipient = new CodeDescriptionPair(string.Empty, string.Empty));
		ICodeDescription recipient;

		public ICodeDescription Purpose => purpose ?? (purpose = new CodeDescriptionPair(string.Empty, string.Empty));
		ICodeDescription purpose;

		public IEnumerable<IEDICommunicationsMode> CommunicationsModes => Enumerable.Empty<IEDICommunicationsMode>();
	}
}
