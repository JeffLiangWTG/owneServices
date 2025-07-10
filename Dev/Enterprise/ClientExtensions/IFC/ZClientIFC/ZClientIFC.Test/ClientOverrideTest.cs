using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Client.IFC.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestClientOverride()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals(Clients.IFC, @override.Client);
			AssertEquals("International Freight Consolidators", @override.ClientDisplayName);
			AssertEquals("Should haev IFC registry items", IFCDataRegistry.Instance, @override.AdditionalRegistryItemSet);
		}

		public void TestTypeDeciderDictionary()
		{
			ITypeDeciderDictionary typeDeciders = ClientOverride.Instance.ClientTypeDeciders;
			AssertEquals("Should be 1 Type Decider", 1, new List<KeyValuePair<Type, ITypeDecider>>(typeDeciders).Count);
			foreach (KeyValuePair<Type, ITypeDecider> keyValuePair in typeDeciders)
			{
				TypeDeciderImpl typeDecider = keyValuePair.Value as TypeDeciderImpl;
				AssertNotNull(typeDecider);
				AssertEquals("Should be a IFCStorageDocsValueObjectDataAdapter Type", typeof(IFCStorageDocsValueObjectDataAdapter), typeDecider.ClientType);
			}
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
