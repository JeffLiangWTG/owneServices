using System;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SIP.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class SipClientOverrideTest : ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
