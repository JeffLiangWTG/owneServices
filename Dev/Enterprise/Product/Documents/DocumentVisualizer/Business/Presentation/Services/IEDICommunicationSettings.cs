using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IEDICommunicationSettings
	{
		ICodeDescription Recipient { get; }
		ICodeDescription Purpose { get; }

		IEnumerable<IEDICommunicationsMode> CommunicationsModes { get; }
	}
}