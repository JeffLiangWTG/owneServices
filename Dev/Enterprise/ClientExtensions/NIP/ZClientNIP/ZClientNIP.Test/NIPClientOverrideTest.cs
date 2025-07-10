using System;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.NIP.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class NIPClientOverrideTest : ClientOverrideTest
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
