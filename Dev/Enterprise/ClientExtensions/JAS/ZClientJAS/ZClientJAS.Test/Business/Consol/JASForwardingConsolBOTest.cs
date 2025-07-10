using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASForwardingConsol))]
	public class JASForwardingConsolBOTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.JASForwardingConsol);
			}
		}
	}
}
