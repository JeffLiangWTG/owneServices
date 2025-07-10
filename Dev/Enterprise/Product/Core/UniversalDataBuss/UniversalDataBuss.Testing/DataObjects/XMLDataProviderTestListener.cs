using System;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	public class XMLDataProviderTestListener : BaseTestListener
	{
		readonly IXMLDataProvider xmlDataProvider = ObjectFactory.Get<IXMLDataProvider>();

		public override void BeforeEachTest(DateTime startTime)
		{
			xmlDataProvider.SetUseDefaultValueForTest(true);
			xmlDataProvider.SetIgnoreTimestampForTest(true);
		}
	}
}
