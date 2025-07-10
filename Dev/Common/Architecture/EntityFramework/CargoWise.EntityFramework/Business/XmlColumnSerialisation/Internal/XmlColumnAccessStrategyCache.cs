using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	class XmlColumnAccessStrategyCache : Dictionary<Tuple<Type, Type, XmlColumnSpecification>, IXmlColumnAccessStrategy[]>
	{
	}
}
