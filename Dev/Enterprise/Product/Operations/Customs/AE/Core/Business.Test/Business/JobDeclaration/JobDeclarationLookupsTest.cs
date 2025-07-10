using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class JobDeclarationLookupsTest : TestCaseWithFactory
{
	public void TestMessageTypeList()
	{
		CombineAssertions(() =>
		{
			var messageTypeList = lookups.MessageTypeList;
			AssertEquals("Default MessageTypeList", "DRW, EXP, EXW, IMP, MSC, REF", messageTypeList.CodesAsString);
			NUnit.Framework.Assert.That(messageTypeList, Is.SameAs(lookups.MessageTypeList), "Cached");

			declaration.JE_ApplicationCode = "AED";
			messageTypeList = lookups.MessageTypeList;
			AssertEquals("AEDeclarationApplicationCode MessageTypeList", "CTF, EXP, IMP, TMP, TRF, TRS", messageTypeList.CodesAsString);
			NUnit.Framework.Assert.That(messageTypeList, Is.SameAs(lookups.MessageTypeList), "Cached");

			declaration.JE_ApplicationCode = "ITF";
			messageTypeList = lookups.MessageTypeList;
			AssertEquals("Default MessageTypeList", "DRW, EXP, EXW, IMP, MSC, REF", messageTypeList.CodesAsString);
			NUnit.Framework.Assert.That(messageTypeList, Is.SameAs(lookups.MessageTypeList), "Cached");
		});
	}

	public void TestMessageSubTypeList()
	{
		var messageSubTypeList = lookups.MessageSubTypeList;
		AssertEquals("MessageSubTypeList Should Be Override to Empty List", 0, messageSubTypeList.Count);
	}

	public void TestPaymentPartyList()
	{
		CombineAssertions(() =>
		{
			var paymentPartyList = lookups.PaymentPartyList;
			AssertEquals("List", "CA, DC, IA", paymentPartyList.CodesAsString);
			NUnit.Framework.Assert.That(paymentPartyList, Is.SameAs(Factory.GetCachedValue<PaymentByList>()), "Cached");
		});
	}

	public void TestEntryStatusList()
	{
		CombineAssertions(() =>
		{
			var entryStatusList = lookups.EntryStatusList;
			AssertEquals("List", "A, C, F, H, I, N, R, S", entryStatusList.CodesAsString);
			NUnit.Framework.Assert.That(entryStatusList, Is.SameAs(Factory.GetCachedValue<AEEntryStatusList>()), "Cached");
		});
	}

	public void TestOperationalStatusList()
	{
		CombineAssertions(() =>
		{
			var operationalStatusList = lookups.OperationalStatusList;
			AssertEquals("List", "NRM, HLD", operationalStatusList.CodesAsString);
			NUnit.Framework.Assert.That(operationalStatusList, Is.SameAs(Factory.GetCachedValue<OperationalStatusList>()), "Cached");
		});
	}

	[ExpectNoExceptions]
	public void TestIncoTermList()
	{
		NUnit.Framework.Assert.That(lookups.IncoTermList, Is.SameAs(Factory.GetCachedValue("Declaration|IncoTermList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms))));
	}

	public void TestApplicationCodeList() => CombineAssertions(() =>
	{
		var applicationCodeList = lookups.ApplicationCodeList;
		AssertEquals("Application Code List", "AED, ITF", applicationCodeList.CodesAsString);

		NUnit.Framework.Assert.That(applicationCodeList, Is.SameAs(lookups.ApplicationCodeList), "Cached");
	});

	public void TestNormalTransportTypeList()
	{
		CombineAssertions(() =>
		{
			var transportTypeList = lookups.TransportTypeList;
			AssertEquals("Normal TransportTypeList", "AIR, FIX, IWT, OWN, MAI, RAI, ROA, SEA", transportTypeList.CodesAsString);
			NUnit.Framework.Assert.That(transportTypeList, Is.SameAs(lookups.TransportTypeList), "Cached");
		});
	}

	public void TestAEDeclarationApplicationCodeTransportTypeList()
	{
		CombineAssertions(() =>
		{
			declaration.JE_ApplicationCode = "AED";
			var transportTypeList = lookups.TransportTypeList;
			AssertEquals("AEDeclarationApplicationCode TransportTypeList", "AIR, CRA, CRL, CRR, CST, FIX, IWT, MAI, OWN, PAS, RAI, ROA, SEA", transportTypeList.CodesAsString);
			NUnit.Framework.Assert.That(transportTypeList, Is.SameAs(lookups.TransportTypeList), "Cached");
		});
	}

	public void TestTransferApplicationCodeTransportTypeList()
	{
		CombineAssertions(() =>
		{
			var transportTypeList = lookups.TransportTypeList;
			AssertEquals("Normal TransportTypeList", "AIR, FIX, IWT, OWN, MAI, RAI, ROA, SEA", transportTypeList.CodesAsString);

			declaration.JE_ApplicationCode = "AED";
			transportTypeList = lookups.TransportTypeList;
			AssertEquals("AEDeclarationApplicationCode TransportTypeList", "AIR, CRA, CRL, CRR, CST, FIX, IWT, MAI, OWN, PAS, RAI, ROA, SEA", transportTypeList.CodesAsString);

			declaration.JE_ApplicationCode = "ITF";
			transportTypeList = lookups.TransportTypeList;
			AssertEquals("Normal TransportTypeList", "AIR, FIX, IWT, OWN, MAI, RAI, ROA, SEA", transportTypeList.CodesAsString);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		lookups = new JobDeclarationLookups(declaration);
	}

	JobDeclarationLookups lookups;
	JobDeclaration declaration;
}
