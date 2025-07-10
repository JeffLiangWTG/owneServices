using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public partial class IncidentTokenizerTest : TestCaseWithFactory
	{
		public void TestSpaces()
		{
			// Arrange
			var inputs = new[]
			{
				"  ",
				"  \t \r\n  ",
				"  \n  \r  \t   "
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" ", outputs[i]);
			}
		}

		public void TestFractions()
		{
			// Arrange
			var inputs = new[]
			{
				" 0.5 50% 1/2 ½ ",
				" 0.25 25% 1/4 ¼ ",
				" 0.75 75% 3/4 ¾ ",
			};
			var expectedOutputs = new[]
			{
				" half half half half ",
				" onequarter onequarter onequarter onequarter ",
				" threequarters threequarters threequarters threequarters ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(expectedOutputs[i], outputs[i]);
			}
		}

		public void TestDateTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" 2019/06/03 ",
				" 2019.06.03 ",
				" 2019-06-03 ",
				" 2019-06-03 11:59 ",
				" 2019/06/03t11:59 ",
				" 2019-06-03 11:59:32 ",
				" 2019-06-03t11:59:32 ",
				" 2019.06.03 11:59:32.1234 ",
				" 2019-06-03t11:59:32.1234 ",
				" 03-jun ",
				" 03-jun-19 ",
				" 3/jun/2019 ",
				" 3-june ",
				" 03 june ",
				" 3 june 19 ",
				" 03 june-2019 ",
				" 03-june 2019 11:59 ",
				" 03.june 19 11:59:32 ",
				" 3 jun 2019 11:59:32.1234 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" datetokenn ", outputs[i]);
			}
		}

		public void TestEmailTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" baby.spice@wisetech-global.com ",
				" ginger-spice@wisetech_global.com.au ",
				" sporty_spice@wisetech.global.c ",
				" scary+spice@wise.tech.global ",
				" sleepy.sp1ce@wise-t3ch.global ",
				" cr4nky.spice+123@wis3_tech.global ",
				" happy.spice@wis3_tech.glo-ba.1 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPreProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" emailtokenn ", outputs[i]);
			}
		}

		public void TestLocalhostTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" localhost ",
				" local host ",
				" local-host ",
				" 127.0.0.1 ",
				" 127.0.0.01 ",
				" 127.0.0.001 ",
				" 127.0.0.2 ",
				" 127.0.0.03 ",
				" 127.0.0.004 ",
				" 127.0.00.1 ",
				" 127.00.0.1 ",
				" 127.0.000.1 ",
				" 127.000.0.1 ",
				" 127.000.000.001 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" localhost ", outputs[i]);
			}
		}

		public void TestIpAddressTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" 1.1.1.1 ",
				" 01.1.1.1 ",
				" 1.01.2.1 ",
				" 1.1.01.3 ",
				" 1.1.1.01 ",
				" 001.001.001.001 ",
				" 10.31.009.174 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" ipaddresstokenn ", outputs[i]);
			}
		}

		public void TestVersionTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" v1 ",
				" v10.1 ",
				" v100.1.1 ",
				" v1000.1.1.1 ",
				" v 1 ",
				" v 1.10 ",
				" v 1.100.1 ",
				" v 1.1000.1.1 ",
				" ver 1 ",
				" ver 1.1 ",
				" ver 1.1.100 ",
				" ver 1.1.1.1000 ",
				" version 1 ",
				" version 1.1 ",
				" version 1.1.1 ",
				" version 1.1.1.1000 ",
				" 1.1.1 ",
				" 1.1.1.2015 ",
				" 100.10.1000.10 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" versiontokenn ", outputs[i]);
			}
		}

		public void TestSizeTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" 12mb ",
				" ~12 mib ",
				" 12.38 kb ",
				" 1,234.56 gb ",
				" ~1,234.56gib ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" sizetokenn ", outputs[i]);
			}
		}

		public void TestOrdinalTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" 1st ",
				" 2nd ",
				" 3rd ",
				" 4th ",
				" 50th ",
				" 601st ",
				" 722nd ",
				" 833rd ",
				" 904th ",
				" 1024th ",
				" 1,024th ",
				" first ",
				" second ",
				" third ",
				" fourth ",
				" fifth ",
				" sixth ",
				" seventh ",
				" eighth ",
				" ninth ",
				" tenth ",
				" hundredth ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" ordinaltokenn ", outputs[i]);
			}
		}

		public void TestPercentageTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" 0% ",
				" 2% ",
				" 2.1234% ",
				" 17% ",
				" 19.3862% ",
				" 100% ",
				" 100.9% ",
				" 350% ",
				" 350.90% ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" percentagetokenn ", outputs[i]);
			}
		}

		public void TestWorkItemTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" wi00000001 ",
				" wi12345678 ",
				" wi99999999 ",
				" wi 99999999 ",
				" work item 99999999 ",
				" workitem 99999999 ",
				" workitem99999999 ",
				" work item99999999 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" workitemtokenn ", outputs[i]);
			}
		}

		public void TestSupportIncidentTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" cs00000001 ",
				" cs12345678 ",
				" cs99999999 ",
				" cs 99999999 ",
				" issue 99999999 ",
				" incident 99999999 ",
				" request 99999999 ",
				" support issue 99999999 ",
				" support incident 99999999 ",
				" support request 99999999 ",
				" customer support issue 99999999 ",
				" customer support incident 99999999 ",
				" customer support request 99999999 ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" supportincidenttokenn ", outputs[i]);
			}
		}

		public void TestEncodingTokenn()
		{
			// Arrange
			var inputs = new[]
			{
				" utf ",
				" utf ",
				" utf1 ",
				" utf-1 ",
				" utf7 ",
				" utf-7 ",
				" utf8 ",
				" utf-8 ",
				" utf16 ",
				" utf-16 ",
				" utf32 ",
				" utf-32 ",
				" unicode ",
				" uni-code ",
				" ascii ",
				" ansi ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" encodingtokenn ", outputs[i]);
			}
		}

		public void TestCountryExpansion()
		{
			// Arrange
			var inputs = new[]
			{
				" europe european ",
				" uk united kingdom united-kingdom ",
				" usa u.s.a. u.s.a u.s. america united states ",
				" emirates uae arab emirates united arab emirates ",
			};
			var expectedOutputs = new[]
			{
				" europe europe ",
				" united kingdom united kingdom united kingdom ",
				" united states of america united states of america united states of america united states of america united states of america united states of america ",
				" united arab emirates united arab emirates united arab emirates united arab emirates ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(expectedOutputs[i], outputs[i]);
			}
		}

		public void TestComputingTerms()
		{
			// Arrange
			var inputs = new[]
			{
				" c# .net 64-bit or . net 32 bits ",
				" user-name sign ins & pass word teh log out ",
				" ms msft msoft operating systems os db data-base plug-in ",
				" google-drive one drive drop_box e-mail ",
				" f1 f3 f6 f10 f12 ",
				" 12px & 150em or 2.2pt ",
				" white-list black list ",
				" left-click right_clicking double-clicked singleclicked ",
			};
			var expectedOutputs = new[]
			{
				" csharp dotnet 64bit or dotnet 32bit ",
				" username login and password the logout ",
				" microsoft microsoft microsoft operatingsystem operatingsystem database database plugin ",
				" googledrive onedrive dropbox email ",
				" functionkey functionkey functionkey functionkey functionkey ",
				" screensizetokenn and screensizetokenn or screensizetokenn ",
				" whitelist blacklist ",
				" leftclick rightclick doubleclick singleclick ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(expectedOutputs[i], outputs[i]);
			}
		}

		public void TestWisetechTerms()
		{
			// Arrange
			var inputs = new[]
			{
				" wtg global ",
				" wise-tech global ",
				" wise tech global team ",
				" wise rates border wise ",
				" e-docs service task e-conversation ",
			};
			var expectedOutputs = new[]
			{
				" wtg ",
				" wtg ",
				" wtg ",
				" wiserates borderwise ",
				" edocs servicetask econversation ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(expectedOutputs[i], outputs[i]);
			}
		}

		public void TestPrefixesSuffixes()
		{
			// Arrange
			var inputs = new[]
			{
				" semi rugged anti-potato ",
				" trans optimal sub-uranium ",
				" wisetech-esque semi dmitr-ify ",
			};
			var expectedOutputs = new[]
			{
				" semirugged antipotato ",
				" transoptimal suburanium ",
				" wisetechesque semidmitrify ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(expectedOutputs[i], outputs[i]);
			}
		}

		public void TestCurrencyToken()
		{
			// Arrange
			var inputs = new[]
			{
				" $1 ",
				" $10 ",
				" $100 ",
				" $1.00 ",
				" $10.50 ",
				" $ 10.50 ",
				" aud 10.50 ",
				" aud10.50 ",
				" 1$ ",
				" 10$ ",
				" 100$ ",
				" 1.00$ ",
				" 10.50$ ",
				" 10.50 $ ",
				" 10.50 aud ",
				" 10.50aud ",
				" 10.50cad ",
				" usd10.50 ",
				" idr 10.50 ",
				" idr 10.50 ",
				" ₽1,234.50 ",
				" ₽ 1,234.50 ",
				" 1,234.50₽ ",
				" 1,234.50 ₽ ",
				" kč1,234.50 ",
				" kč 1,234.50 ",
				" 1,234.50kč ",
				" 1,234.50 kč ",
			};

			var tokenizer = new IncidentTokenizer(new Dictionary<string, string>(), null);

			// Act
			var outputs = inputs
				.Select(s => tokenizer.ApplyReplacementsPostProtect(s))
				.ToList();

			// Assert
			for (var i = 0; i < inputs.Length; i++)
			{
				AssertEquals(" currencytokenn ", outputs[i]);
			}
		}
	}
}
