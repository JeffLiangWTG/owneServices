using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Utilities.Testing
{
	sealed class ProperCaseConverterTest : TestCaseWithFactory
	{
		public void TestGeneralUsage()
		{
			var converter = new ProperCaseConverter();

			CombineAssertions(delegate
			{
				AssertEquals("You're Here. I'm Here. We're Here", converter.Convert("you're here. i'm here. we're here"));
				AssertEquals("You're Here. I'm Here. We're Here", converter.Convert("YOU'RE HERE. I'M HERE. WE'RE HERE"));

				AssertEquals("Super Global Logistics", converter.Convert("super global logistics"));
				AssertEquals("Super Global Logistics", converter.Convert("SUPER GLOBAL LOGISTICS"));

				AssertEquals("Super-Fast Global Logistics", converter.Convert("SUPER-FAST GLOBAL LOGISTICS"));
				AssertEquals("Super-Fast Global Logistics", converter.Convert("SUPER-FAST GLOBAL LOGISTICS"));

				AssertEquals("Super Global Logistics (Thailand)", converter.Convert("super global logistics (thailand)"));
				AssertEquals("Super Global Logistics (Thailand)", converter.Convert("SUPER GLOBAL LOGISTICS (THAILAND)"));

				AssertEquals("L.O.L. Pty Ltd", converter.Convert("l.o.l. pty ltd"));
				AssertEquals("L.O.L. Pty Ltd", converter.Convert("L.O.L. PTY LTD"));

				AssertEquals("Ernst & Young", converter.Convert("ernst & young"));
				AssertEquals("Ernst & Young", converter.Convert("ERNST & YOUNG"));

				AssertEquals("3a/72 O'Riordan St Alexandria", converter.Convert("3a/72 o'riordan st alexandria"));
				AssertEquals("3a/72 O'Riordan St Alexandria", converter.Convert("3A/72 O'RIORDAN ST ALEXANDRIA"));

				AssertEquals("Unit 9, 1/F., Square Centre", converter.Convert("unit 9, 1/F., Square centre"));
				AssertEquals("Unit 9, 1/F., Square Centre", converter.Convert("UNIT 9, 1/F., SQUARE CENTRE"));

				AssertEquals("99-A 99th Road", converter.Convert("99-A 99th road"));
				AssertEquals("99-A 99th Road", converter.Convert("99-A 99TH ROAD"));

				AssertEquals("999 De l'Acadie", converter.Convert("999 de l'acadie"));
				AssertEquals("999 De l'Acadie", converter.Convert("999 DE L'ACADIE"));

				AssertEquals("99/99 Rue d'Athenes", converter.Convert("99/99 rue d'athenes"));
				AssertEquals("99/99 Rue d'Athenes", converter.Convert("99/99 RUE D'ATHENES"));

				AssertEquals("Ken O'Reed", converter.Convert("ken o'reed"));
				AssertEquals("Ken O'Reed", converter.Convert("KEN O'REED"));

				AssertEquals("A.L.", converter.Convert("a.l."));
				AssertEquals("A.L.", converter.Convert("A.L."));

				AssertEquals("Dong-Chau Nguyen", converter.Convert("dong-chau nguyen"));
				AssertEquals("Dong-Chau Nguyen", converter.Convert("DONG-CHAU NGUYEN"));

				AssertEquals("Tim (Tam) Tom", converter.Convert("tim (tam) tom"));
				AssertEquals("Tim (Tam) Tom", converter.Convert("TIM (TAM) TOM"));
			});
		}
	}
}
