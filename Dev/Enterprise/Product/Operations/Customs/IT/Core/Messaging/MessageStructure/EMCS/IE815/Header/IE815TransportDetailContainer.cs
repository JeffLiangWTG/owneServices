using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815TransportDetailContainer
{
	public IE815TransportDetailContainer(ITransportDetailContainer transportDetailContainer)
	{
		this.transportDetailContainer = Argument.NotNull(transportDetailContainer, "transportDetailContainer");
	}
	readonly ITransportDetailContainer transportDetailContainer;

	[MessageLayout(Order = 18)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt TotalTransportDetailsIterations => TransportDetails.Count();

	[MessageLayout(Order = 19)]
	public IEnumerable<IE815TransportDetails> TransportDetails
	{
		get
		{
			int i = 0;
			foreach (var transportDetail in transportDetailContainer.TransportDetails)
			{
				yield return new IE815TransportDetails(transportDetail, ++i);
			}
		}
	}
}
