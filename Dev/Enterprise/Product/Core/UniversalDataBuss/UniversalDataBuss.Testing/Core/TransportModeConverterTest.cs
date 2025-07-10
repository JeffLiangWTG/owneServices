using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	[TestedType(typeof(TransportModeConverter))]
	class TransportModeConverterTest : EnumConverterTestCase<TransportModeConverter, TransportMode>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			Type t = Type.GetType("Enterprise.Freight.Business.FreightCodePairLists, Enterprise.Freight, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			MethodInfo method
				 = t.GetMethod("RoutingTransportModeList", BindingFlags.Static | BindingFlags.Public);

			CodeDescriptionPairList codeValuePair = (CodeDescriptionPairList)method.Invoke(null, null);

			IList<ZString> values = new List<ZString>();
			foreach (ZArchitecture.Core.CodeDescriptionPair item in codeValuePair)
			{
				values.Add(item.Code);
			}

			return values;
		}
	}
}
