using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	[TestedType(typeof(UNDGStateConverter))]
	class UNDGStateConverterTest : EnumConverterTestCase<UNDGStateConverter, UNDGState>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			var substance = Factory.New<UNDGSubstance>();

			var property = substance.Lookups.GetType().GetProperty("StateList");

			CodeDescriptionPairList valuesReturned = (CodeDescriptionPairList)property?.GetValue(substance.Lookups);

			IList<ZString> values = new List<ZString>();

			if (valuesReturned != null)
			{
				foreach (ZArchitecture.Core.CodeDescriptionPair item in valuesReturned)
				{
					values.Add(item.Code);
				}
			}

			return values;
		}
	}
}
