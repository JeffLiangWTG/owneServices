using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IVISTO;

public class Ivisto : CustomsInterchange, ICustomsApplicationResponseProvider<CustomsApplicationResponse>
{
	public ZDateTime ElaborationDateTime { get; protected set; }
	public IList<CustomsApplicationResponse> ApplicationResponses { get; protected set; }

	protected override void Load(ZString content)
	{
		base.Load(content);
		ElaborationDateTime = LoadServiceRecord(Lines.ElementAt(1));
		ApplicationResponses = new List<CustomsApplicationResponse>();
		for (int i = 2; i < Lines.Count(); i++)
		{
			var applicationResponseRecord = new CustomsApplicationResponse();
			applicationResponseRecord.Load(Lines.ElementAt(i));
			ApplicationResponses.Add(applicationResponseRecord);
		}
	}

	ZDateTime LoadServiceRecord(ZString serviceRecordLine)
	{
		var date = serviceRecordLine.SubStringAndTrim(5, 10);
		var hour = serviceRecordLine.SubStringAndTrim(21, 8);
		return ((ZString)FormattableString.Invariant($"{date} {hour}")).ParseToDateTimeWithFormat((NoResString)"dd/MM/yyyy HH:mm:ss");
	}
}
