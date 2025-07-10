using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class SealProviderTest : Customs.Business.Testing.DataProviderTestCase<SealProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>(() => new SealProvider(null));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(2, Provider.SequenceNumber);
	}

	public void TestIdentifier()
	{
		AssertEquals("identifier", Provider.Identifier);
	}

	protected override SealProvider GetProvider()
	{
		var seal = Factory.New<CusSeal>();
		seal.BK_SequenceNumber = 2;
		seal.BK_SealNumber = "identifier";
		return new SealProvider(seal);
	}

	public void TestSealProviderWithSequenceNumberAndIdentifier()
	{
		var sealProvider = new SealProvider("SEAL1", 1);
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new SealProvider(ZString.Empty, 1));
			AssertEquals("Seal Number field with sequence number 1" ,1, sealProvider.SequenceNumber);
			AssertEquals("Seal Number field with value as SEAL1", "SEAL1", sealProvider.Identifier);
		});
	}
}
