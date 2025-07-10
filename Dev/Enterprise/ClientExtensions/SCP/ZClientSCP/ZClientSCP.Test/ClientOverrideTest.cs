using System;
using NUnit.Framework;

namespace Enterprise.Client
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
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
