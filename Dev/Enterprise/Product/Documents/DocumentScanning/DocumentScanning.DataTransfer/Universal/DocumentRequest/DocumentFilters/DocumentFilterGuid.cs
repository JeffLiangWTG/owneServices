using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	class DocumentFilterGuid : DocumentFilterBase
	{
		public DocumentFilterGuid(string filterName, Func<IeDoc, ZGuid> guidGetter)
		{
			Argument.NotNull(guidGetter, nameof(guidGetter));

			FilterName = filterName;
			GuidGetter = guidGetter;
		}

		string FilterName { get; }

		Func<IeDoc, ZGuid> GuidGetter { get; }

		HashSet<ZGuid> Values { get; } = new HashSet<ZGuid>();

		public override void AddFilterValue(ZString value)
		{
			if (ZGuid.TryParse(value, out var guid))
			{
				Values.Add(guid);
			}
			else
			{
				throw new DataObjectReadFailureException($"Cannot parse guid filter {FilterName} value from text '{value}'.");
			}
		}

		public override bool IsMatch(IeDoc eDoc) => Values.Contains(GuidGetter(eDoc));
	}
}
